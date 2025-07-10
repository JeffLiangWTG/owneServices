using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Client.TGE.PMS;
using Enterprise.Client.TGE.ServiceTask;
using Enterprise.ClientSharedComponents;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	PMSImportServiceTask.Code,
	"Import of Export Freight Consols & Shipments from PMS System",
	"CSP",
	typeof(PMSImportServiceTask),
	MinimumPeriod = "30seconds",
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)]
namespace Enterprise.Client.TGE.ServiceTask
{
	class PMSImportServiceTask : ServiceProviderImpl
	{
		#region Service Task override

		public override void RunTask(CancellationToken token)
		{
			Buffer = new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());
			Buffer.Notify(new InfoNotification("================== Task started =================="));
			if (IsEnvironmentDataValid())
			{
				Process(token);
			}
			Buffer.Notify(new InfoNotification("================== Task ended =================="));
		}

		#endregion

		#region Implementation

		#region Process

		void Process(CancellationToken token)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(TGEDataRegistry.Instance.PMSFileImportDirectory);
			FileInfo[] files = directoryInfo.GetFiles("expcon*.tsv");

			Buffer.Notify(new InfoNotification(string.Format(CultureInfo.CurrentCulture, "Processing {0} file(s) from {1}", files.Length, directoryInfo.FullName)));

			foreach (FileInfo file in files)
			{
				token.ThrowIfCancellationRequested();
				ProcessFile(file);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void ProcessFile(FileInfo file)
		{
			bool importWithNoError = true;

			try
			{
				importWithNoError = DataImport(file);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, "Failure processing Files: " + e.Message));
				importWithNoError = false;
			}
			finally
			{
				if (!importWithNoError)
				{
					SendErrorEmail(file);
				}

				MoveFileToArchiveDirectory(file);
				Application.DoEvents();
			}
		}

		bool DataImport(FileInfo file)
		{
			bool result = true;

			DataImporter importer = GetImporter();
			importer.ImportData(file.FullName, Buffer, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ClientSpecifiedImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, file.FullName));

			if (!Buffer.HasErrors && !Buffer.HasWarnings)
			{
				Buffer.Notify(new InfoNotification("Imported file " + file.Name));
			}
			else
			{
				result = false;

				if (Buffer.HasErrors)
				{
					Buffer.Notify(new ErrorNotification(ErrorType.Error, "Error importing all lines in file : " + file.Name + ". Please see email error report for more Details."));
				}
				else
				{
					Buffer.Notify(new WarningNotification("Warning with File: " + file.Name + ". Please see mail error report for more Details."));
				}
			}

			return result;
		}

		DataImporter GetImporter()
		{
			return TGEImporterDelegate();
		}

		public delegate DataImporter GetImporterDelegate();
		public GetImporterDelegate TGEImporterDelegate
		{
			get { return getTGEImporterDelegate ?? (getTGEImporterDelegate = new GetImporterDelegate(() => { return new TGEDataImporter(); })); }
			internal set
			{
				if (Globals.IsTest)
				{
					getTGEImporterDelegate = value;
				}
			}
		}
		GetImporterDelegate getTGEImporterDelegate;

		void MoveFileToArchiveDirectory(FileInfo file)
		{
			try
			{
				ZString archiveFileName = file.Name.Substring(0, file.Name.LastIndexOf(".tsv")) + ZDateTime.Now.ToString("yyyyMMddHHmmss") + ".tsv";
				file.MoveTo(Path.Combine(TGEDataRegistry.Instance.PMSFileArchiveDirectory, archiveFileName));
			}
			catch (IOException e)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.IOError, e.Message));
			}
		}

		#endregion

		#region Send Email

		void SendErrorEmail(FileInfo file)
		{
			AttachmentDefCollection attachments = new AttachmentDefCollection();
			attachments.Add(new AttachmentDef(file.FullName));

			Buffer.SendEmail(TGEDataRegistry.Instance.PMSNotificationGroup, null, "Error Processing Flat Text File(s)", "", "", Buffer.Inner, Env.OutgoingMailManager, Factory, attachments);
		}

		#endregion

		#region Is Environment Data Valid

		bool IsEnvironmentDataValid()
		{
			ZStringBuilder message = new ZStringBuilder();

			if (TGEDataRegistry.Instance.PMSFileImportDirectory.IsEmpty || !Directory.Exists(TGEDataRegistry.Instance.PMSFileImportDirectory))
			{
				message.Append("The PMS Import File Directory has not been set up or not exists.");
			}

			if (TGEDataRegistry.Instance.PMSFileArchiveDirectory.IsEmpty || !Directory.Exists(TGEDataRegistry.Instance.PMSFileArchiveDirectory))
			{
				message.Append("The PMS Archive Directory has not been set up or not exists.");
			}

			if (!SystemDataRegistry.Instance.AllowDepartureDepotAddressImport.Value)
			{
				message.Append("The Registry Allow Departure Depot Address Import has to be set to true for data import.");
			}

			if (!NotificationGroupValidator.IsValid(TGEDataRegistry.Instance.PMSNotificationGroup, Factory))
			{
				message.Append("The PMS Notification Group has not been set up or invalid.");
			}

			if (TGEDataRegistry.Instance.CodeMapPMSOrganisation.IsEmpty)
			{
				message.Append("The PMS Organisation For Code Mapping has not been set up.");
			}

			if (SystemDataRegistry.Instance.UpdateConsolShipmentsDuringAutomaticImportOther.Value)
			{
				message.Append("The Registry Update Consol Shipments During Automatic Import has to be set to false for data import.");
			}

			if (SystemDataRegistry.Instance.UpdateConsolDuringAutomaticImportOther.Value)
			{
				message.Append("The Registry Update Consol During Automatic Import has to be set to false for data import.");
			}

			if (!message.IsEmpty)
			{
				Buffer.Notify(new ErrorNotification(ErrorType.Error, message.ToStringWithNewLineBetweenAppends()));
			}

			return message.IsEmpty;
		}

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		#endregion

		#region Buffer

		NotificationBuffer Buffer;

		internal NotificationBuffer GetBuffer()
		{
			return Buffer;
		}
		#endregion

		#endregion

		public const string Code = "ZT1";
	}
}
