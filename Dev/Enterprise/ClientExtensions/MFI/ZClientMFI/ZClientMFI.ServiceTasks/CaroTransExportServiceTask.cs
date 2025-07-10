using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.MFI;
using Enterprise.Client.MFI.CaroTrans;
using Enterprise.Client.MFI.ServiceTasks;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MFIConstants.ServiceTasks.CaroTransExport,
	"Caro Trans Shipment Export",
	"CSP",
	typeof(CoroTransExportServiceTask),
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true
	)]
namespace Enterprise.Client.MFI.ServiceTasks
{
	internal class CoroTransExportServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			if (CaroTransValidateEnvironment.ValidateExport(Notify))
			{
				try
				{
					RunListeners(token);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					string innerException = ex.InnerException != null ? ex.InnerException.Message : string.Empty;
					Notify.Notify(new ErrorNotification(ErrorType.Error, string.Format(CultureInfo.InvariantCulture, "Error occurred when exporting consol/shipment information: {0} {1}{1} Inner Exception:{2} {1}{1} Call Stack:{1}{3}", ex.Message, System.Environment.NewLine, innerException, ex.StackTrace)));
				}
			}
		}

		#region Implementation

		void RunListeners(CancellationToken token)
		{
			HighWaterMark = MFIDataRegistry.Instance.CaroTransHighWaterMark;
			var startProcessingTime = new[] { HighWaterMark.AddDays(1), ZDateTime.UtcNow.AddHours(-1) }.Min();
			var reader = new FilteredBusinessObjectReader<StmALog>(FactoryProvider, CreateFilter(startProcessingTime)) { BatchSize = 1000 };

			BusinessObject lastBizRead = null;
			IEnumerable<StmALog> candidates = null;
			while ((candidates = reader.LoadNextBatchInANewFactory(lastBizRead).Cast<StmALog>()).Any())
			{
				token.ThrowIfCancellationRequested();
				var tempExportDirectory = new DirectoryInfo(Path.Combine(Env.TempPath));

				foreach (var log in candidates)
				{
					var listeners = GetListeners(tempExportDirectory.FullName);
					foreach (var listener in listeners)
					{
						listener.Match(log, Notify);
					}

					lastBizRead = log;
				}

				if (tempExportDirectory.Exists)
				{
					foreach (var current in tempExportDirectory.GetFiles(Constants.ShipmentDataExportFile + "*"))
					{
						if (current.Length > 0)
						{
							ProcessExportedFile(current);
						}
					}
				}
			}

			MFIDataRegistry.Instance.CaroTransHighWaterMark = startProcessingTime;
		}

		ZQuery CreateFilter(ZDateTime endDateTime)
		{
			var result = new ZQuery(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThan, HighWaterMark);
			result.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, endDateTime);

			var jobDecEventQuery = new ZQuery(StmALogSchema.SL_Table, JobDeclarationSchema.Constants.TableName);
			jobDecEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatusCode);
			jobDecEventQuery.AddToFilter(StmALogSchema.SL_Reference, new string[] { CMRImportEntryAdvice.Clear.Code, CMRImportEntryAdvice.Finalised.Code, CMRImportEntryAdvice.ATDReceived.Code, CustomsEntryStatus.ClearCPDec.Code, CustomsEntryStatus.ClearLodge.Code, CustomsEntryStatus.ClearPay.Code });

			var conolOrShipmentEventQuery = new ZQuery(StmALogSchema.SL_Table, new string[] { JobShipmentSchema.Constants.TableName, JobConsolSchema.Constants.TableName });
			conolOrShipmentEventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, new string[] { Events.DeliveryCartageCompleteFinalisedCode, Events.CargoAvailableCode, Events.ArrivalCode });

			var subQuery = new ZQuery(jobDecEventQuery);
			subQuery.AddToFilter(conolOrShipmentEventQuery, JoinCondition.Or);

			result.AddToFilter(subQuery);
			result.AddToFilter(StmALogSchema.SL_IsEstimate, false);

			result.IsNoLock = false;
			return result;
		}

		void ProcessExportedFile(FileInfo exportedFile)
		{
			var lines = GetDistinctExportedLines(exportedFile);

			string fileName = string.Format("ST{0}{1}", ZDateTime.Now.ToString("yyyyMMddhhmmss"), exportedFile.Extension);
			string finalFile = Path.Combine(exportedFile.DirectoryName, fileName);

			DeleteFile(finalFile);

			bool successfullyWriteToFile = false;

			for (int i = 1; i <= MaximumAttempts && !successfullyWriteToFile; i++)
			{
				try
				{
					using (var writer = new StreamWriter(finalFile))
					{
						foreach (ZString line in lines)
						{
							writer.WriteLine(line);
						}
						writer.WriteLine("99," + lines.Length.ToString());
						writer.Flush();
						successfullyWriteToFile = true;
					}
				}
				catch (IOException ex)
				{
					if (i == MaximumAttempts)
					{
						Notify.Notify(new ErrorNotification(ErrorType.Error, string.Format("Errors occurred when generating export file : {0} - {1}", finalFile, ex.Message)));
					}
					else
					{
						Thread.Sleep(new TimeSpan(0, 0, 2));
					}
				}
			}

			DeleteFile(exportedFile.FullName);

			var email = new EmailDef();
			email.AddRecipientForSystemCommunication(MFIDataRegistry.Instance.CaroTransTrackEmailAddress);
			email.Subject = "Shipments Data";
			var attachment = new AttachmentDef(fileName, finalFile);
			email.Attachments.Add(attachment);

			Env.OutgoingMailManager.CreateAndSave(email);

			DeleteFile(finalFile);
		}

		internal ZString[] GetDistinctExportedLines(FileInfo exportedFile)
		{
			var result = new ArrayList();

			var temp = GetExportedDataInReverseOrder(exportedFile.FullName);
			foreach (ZString line in temp)
			{
				if (IsOkToAddShipmentLine(result, line))
				{
					result.Insert(0, line);
				}
			}

			return (ZString[])result.ToArray(typeof(ZString));
		}

		ArrayList GetExportedDataInReverseOrder(ZString exportedFileName)
		{
			var result = new ArrayList();
			using (var reader = new StreamReader(exportedFileName))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					result.Insert(0, (ZString)line);
				}
			}
			return result;
		}

		bool IsOkToAddShipmentLine(ArrayList list, ZString line)
		{
			int lengthToCompare = line.IndexOf(',', Constants.ShipmentRecord.ArrivalDate);
			if (lengthToCompare == -1)
			{
				return false;
			}
			else
			{
				foreach (var item in list.Cast<ZString>())
				{
					if (item.Left(lengthToCompare) == line.Left(lengthToCompare))
					{
						return false;
					}
				}
			}
			return true;
		}

		void DeleteFile(ZString filePath)
		{
			try { if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
			}
			catch (IOException) { }
		}

		const int MaximumAttempts = 3;

		#region Listeners

		internal CaroTransShipmentListener[] GetListeners(ZString exportPath)
		{
			return new CaroTransShipmentListener[]
					{
						new CaroTransConsolArrivalListener(exportPath),
						new CaroTransConsolAvailableListener(exportPath),
						new CaroTransShipmentCustomsListener(exportPath),
						new CaroTransShipmentDeliveryListener(exportPath)
					};
		}

		#endregion

		#region Notification Buffer

		internal NotificationBuffer Notify
		{
			get { return notify ?? (notify = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber())); }
		}
		NotificationBuffer notify;

		#endregion

		#region Factory Provider

		BusinessObjectFactoryProvider FactoryProvider
		{
			get { return factoryProvider ?? (factoryProvider = new BusinessObjectFactoryProvider()); }
		}
		BusinessObjectFactoryProvider factoryProvider;

		#endregion

		ZDateTime HighWaterMark;

		#endregion
	}
}
