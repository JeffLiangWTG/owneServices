using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.CalendarArithmetic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.Ftp;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(BISIUploadServiceTask.Code, "BISI Upload", "CSP",
	typeof(BISIUploadServiceTask),
	MinimumPeriod = "1minute",
	MaximumPeriod = "240minutes",
	DefaultScheduleRunEvery = "30minutes"
	)]
namespace Enterprise.Client.UPE.ServiceTask
{
	public partial class BISIUploadServiceTask : UPEServiceTask
	{
		public const string Code = "ZU3";

		readonly int processID;

		public BISIUploadServiceTask()
		{
			processID = System.Diagnostics.Process.GetCurrentProcess().Id;
		}

		public BISIUploadServiceTask(ILogger logger)
			: base(logger)
		{
			processID = System.Diagnostics.Process.GetCurrentProcess().Id;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public override void RunTask(CancellationToken token)
		{
			try
			{
				Notifications.Notify(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "Start BISI Uploading - PID: {0}", processID)));

				var branches = UPETools.Instance.UPECustomisationBranches(false);
				Notifications.Notify(new InfoNotification(FormattableString.Invariant($"Companies found with an active branch and Customisations enabled are: {string.Join("; ", branches.Select(branch => branch.CompanyName))}")));

				foreach (var branch in branches)
				{
					using (branch.SetAsTemporaryContext())
					{
						try
						{
							WaitFor<Object>.Run(TimeSpan.FromSeconds(UPEDataRegistry.Instance.SftpServerTimeout), () =>
							{
								Notifications.Notify(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "Attempting to verify server : '{0}' using username '{1}' for company {2} and branch {3} in country {4}", Uploader.ServerAddress, Uploader.Username, Env.CurrentCompany.Code, branch.GB_Code, Env.CurrentCompany.Country.Code)));
								if (Uploader.CanUpload())
								{
									Notifications.Notify(new InfoNotification(" Uploader: OK."));
									ExecuteCore();
								}
								else
								{
									Notifications.Notify(new InfoNotification(" Uploader: Not OK."));
								}
								return null;
							});
						}
						catch (TimeoutException)
						{
							UPETools.Instance.SendTimeoutEmail(Code);
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notifications.Notify(new ErrorNotification(ErrorType.Error, string.Format(CultureInfo.InvariantCulture, "An error has occured while connecting to the FTP server: {0}{1}{2}", ex.Message, System.Environment.NewLine, ex.StackTrace)));
			}
			finally
			{
				Notifications.Notify(new InfoNotification("Finish BISI Uploading"));
			}
		}

		void ExecuteCore()
		{
			Notifications.Notify(new InfoNotification(" ExecuteCore()"));
			string tempFile = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());

			ExportInformation exportInformation = CreateExportInformation();
			BISIExportResult exportResult = ExportToTempFile(Exporter, tempFile, exportInformation);
			Notifications.Notify(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "Searching for shipments (Every day) between {0} and {1}", exportInformation.EveryDayStartDate.ToString(), exportInformation.EveryDayEndDate.ToString())));

			if (exportResult != BISIExportResult.ExportFails)
			{
				if (exportResult == BISIExportResult.ExportSuccess)
				{
					Notifications.Notify(new InfoNotification(" Export success."));
					if (UploadToFtpServerAndArchive(tempFile))
					{
						foreach (var shipmentData in Exporter.LastUploadedCompletedShipments)
						{
							shipmentData.LogBISIEventAfterUploaded();
						}

						SendUploadedShipmentsNotificationEmail(Exporter.LastUploadedCompletedShipments, UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber);
						UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber++;
						SaveModifiedRecords(Exporter, exportInformation);
					}
				}
				else if (exportResult == BISIExportResult.ExportSuccessButNoData)
				{
					Notifications.Notify(new InfoNotification(" Export success but no data."));
					SaveModifiedRecords(Exporter, exportInformation);
				}
			}
			else
			{
				Notifications.Notify(new InfoNotification(" Export failed."));
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void SendUploadedShipmentsNotificationEmail(IReadOnlyList<IShipmentData> uploadedShipments, int bISIUploadCurrentBatchNumber)
		{
			try
			{
				NewShipmentsUploadedReporter(uploadedShipments, bISIUploadCurrentBatchNumber).SendEmailIfRequired(Notifications);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ErrorSendingShipmentsUploadedReport", ex.Message, ex);
			}
		}

		protected virtual BISIShipmentsUploadedReporter NewShipmentsUploadedReporter(IReadOnlyList<IShipmentData> uploadedShipments, int bISIUploadCurrentBatchNumber)
		{
			return new BISIShipmentsUploadedReporter(uploadedShipments, bISIUploadCurrentBatchNumber);
		}

		#region Decide Which Exports To Run

		ExportInformation CreateExportInformation()
		{
			var runUntilTime = ZDateTime.Now;
			if (UPEDataRegistry.Instance.BISIUploadCompletedHWM.IsValid && UPEDataRegistry.Instance.BISIUploadCompletedHWM.AddHours(2) < runUntilTime)
			{
				runUntilTime = UPEDataRegistry.Instance.BISIUploadCompletedHWM.AddHours(2);
			}

			ZDateTime dateForHWM = runUntilTime.AddMilliseconds(-runUntilTime.Millisecond).AddSeconds(-60);

			ExportInformation result = new ExportInformation(UPEDataRegistry.Instance.BISIUploadCurrentBatchNumber, UPEDataRegistry.Instance.BISIUploadCompletedHWM, dateForHWM);

			TrySetEverydayExportInformation(dateForHWM, result);
			if (!CurrentBranchWorkingDays.IsDateTimeAHoliday(dateForHWM.ToDateTime()) && !CurrentBranchWorkingDays.IsDateAWeekend(dateForHWM.ToDateTime()))
			{
				TrySet1030ExportInformation(dateForHWM, result);
				TrySet1630ExportInformation(dateForHWM, result);
				TrySet2200ExportInformation(dateForHWM, result);
			}
			return result;
		}

		void TrySetEverydayExportInformation(ZDateTime dateForHWM, ExportInformation exportInformation)
		{
			exportInformation.RunEveryDayExport = true;
			exportInformation.EveryDayStartDate = UPEDataRegistry.Instance.BISIUploadEverydayHWM;
			exportInformation.EveryDayEndDate = GetHighWaterMarkChunk(UPEDataRegistry.Instance.BISIUploadEverydayHWM, dateForHWM);

			if (dateForHWM.Date > UPEDataRegistry.Instance.BISIUploadEverydayDateOfArrivalHWM)
			{
				exportInformation.RunEveryDayDateOfArrivalExport = true;
				exportInformation.EveryDayDateOfArrivalStartDate = UPEDataRegistry.Instance.BISIUploadEverydayDateOfArrivalHWM.AddDays(1);
				exportInformation.EveryDayDateOfArrivalEndDate = dateForHWM;
			}
		}

		void TrySet1030ExportInformation(ZDateTime dateForHWM, ExportInformation exportInformation)
		{
			if (dateForHWM.TimeOfDay >= UPEDataRegistry.Instance.BISIUploadWorkingDayMetroExportTime.TimeOfDay)
			{
				if (UPEDataRegistry.Instance.BISIUploadWorkingDayMetroHWM < new ZDateTime(dateForHWM.Year, dateForHWM.Month, dateForHWM.Day,
					UPEDataRegistry.Instance.BISIUploadWorkingDayMetroExportTime.Hour, UPEDataRegistry.Instance.BISIUploadWorkingDayMetroExportTime.Minute, 0))
				{
					exportInformation.RunWorkingDayMetroExport = true;
					exportInformation.WorkingDayMetroStartDate = UPEDataRegistry.Instance.BISIUploadWorkingDayMetroHWM;
					exportInformation.WorkingDayMetroEndDate = dateForHWM;
				}

				if (dateForHWM.Date > UPEDataRegistry.Instance.BISIUploadWorkingDayDateOfArrivalMetroHWM)
				{
					exportInformation.RunWorkingDayDateOfArrivalMetroExport = true;

					exportInformation.WorkingDayDateOfArrivalMetroStartDate = UPEDataRegistry.Instance.BISIUploadWorkingDayDateOfArrivalMetroHWM.AddDays(1);
					exportInformation.WorkingDayDateOfArrivalMetroEndDate = dateForHWM;
				}
			}
		}

		void TrySet1630ExportInformation(ZDateTime dateForHWM, ExportInformation exportInformation)
		{
			if (dateForHWM.TimeOfDay >= UPEDataRegistry.Instance.BISIUploadWorkingDayOtherExportTime.TimeOfDay)
			{
				if (UPEDataRegistry.Instance.BISIUploadWorkingDayOtherHWM < new ZDateTime(dateForHWM.Year, dateForHWM.Month, dateForHWM.Day,
					UPEDataRegistry.Instance.BISIUploadWorkingDayOtherExportTime.Hour, UPEDataRegistry.Instance.BISIUploadWorkingDayOtherExportTime.Minute, 0))
				{
					exportInformation.RunWorkingDayOtherExport = true;
					exportInformation.WorkingDayOtherStartDate = UPEDataRegistry.Instance.BISIUploadWorkingDayOtherHWM;
					exportInformation.WorkingDayOtherEndDate = dateForHWM;
				}

				if (dateForHWM.Date > UPEDataRegistry.Instance.BISIUploadWorkingDayDateOfArrivalOtherHWM)
				{
					exportInformation.RunWorkingDayDateOfArrivalOtherExport = true;

					exportInformation.WorkingDayDateOfArrivalOtherStartDate = UPEDataRegistry.Instance.BISIUploadWorkingDayDateOfArrivalOtherHWM.AddDays(1);
					exportInformation.WorkingDayDateOfArrivalOtherEndDate = dateForHWM;
				}
			}
		}

		void TrySet2200ExportInformation(ZDateTime dateForHWM, ExportInformation exportInformation)
		{
			if (dateForHWM.TimeOfDay >= UPEDataRegistry.Instance.BISIUploadWorkingDayMetroAndOtherExportTime.TimeOfDay)
			{
				if (UPEDataRegistry.Instance.BISIUploadWorkingDayMetroHWM < new ZDateTime(dateForHWM.Year, dateForHWM.Month, dateForHWM.Day,
					UPEDataRegistry.Instance.BISIUploadWorkingDayMetroAndOtherExportTime.Hour, UPEDataRegistry.Instance.BISIUploadWorkingDayMetroAndOtherExportTime.Minute, 0))
				{
					exportInformation.RunWorkingDayMetroExport = true;
					exportInformation.WorkingDayMetroStartDate = UPEDataRegistry.Instance.BISIUploadWorkingDayMetroHWM;
					exportInformation.WorkingDayMetroEndDate = dateForHWM;
				}

				if (UPEDataRegistry.Instance.BISIUploadWorkingDayOtherHWM < new ZDateTime(dateForHWM.Year, dateForHWM.Month, dateForHWM.Day,
					UPEDataRegistry.Instance.BISIUploadWorkingDayMetroAndOtherExportTime.Hour, UPEDataRegistry.Instance.BISIUploadWorkingDayMetroAndOtherExportTime.Minute, 0))
				{
					exportInformation.RunWorkingDayOtherExport = true;
					exportInformation.WorkingDayOtherStartDate = UPEDataRegistry.Instance.BISIUploadWorkingDayOtherHWM;
					exportInformation.WorkingDayOtherEndDate = dateForHWM;
				}
			}
		}

		#endregion

		BISIExportResult ExportToTempFile(IBISIFileExporter exporter, string tempFile, ExportInformation exportInformation)
		{
			BISIExportResult result;
			Notifications.Notify(new InfoNotification("Start Exporting"));
			result = exporter.ExportToFile(tempFile, exportInformation);
			Notifications.Notify(new InfoNotification("Finish Exporting"));
			return result;
		}

		bool UploadToFtpServerAndArchive(string localFile)
		{
			bool result = false;
			Notifications.Notify(new InfoNotification("Start Uploading via FTP"));
			result = BISIUploader.UploadToFtpServerAndArchive(localFile);
			Notifications.Notify(new InfoNotification("Finish Uploading via FTP"));
			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void SaveModifiedRecords(IBISIFileExporter export, ExportInformation exportInformation)
		{
			Notifications.Notify(new InfoNotification("Start Saving Modified Records"));

			try
			{
				UPESaveConcurrencyExceptionResolver.HandleException(() => { export.SaveDateUploadedAndBISIUploadData(); }, Notifications);

				UPEDataRegistry.Instance.BISIUploadCompletedHWM = exportInformation.CompletedEndDate;

				if (exportInformation.RunEveryDayExport)
				{
					UPEDataRegistry.Instance.BISIUploadEverydayHWM = exportInformation.EveryDayEndDate;
				}

				if (exportInformation.RunEveryDayDateOfArrivalExport)
				{
					UPEDataRegistry.Instance.BISIUploadEverydayDateOfArrivalHWM = exportInformation.EveryDayDateOfArrivalEndDate;
				}

				if (exportInformation.RunWorkingDayMetroExport)
				{
					UPEDataRegistry.Instance.BISIUploadWorkingDayMetroHWM = exportInformation.WorkingDayMetroEndDate;
				}

				if (exportInformation.RunWorkingDayDateOfArrivalMetroExport)
				{
					UPEDataRegistry.Instance.BISIUploadWorkingDayDateOfArrivalMetroHWM = exportInformation.WorkingDayDateOfArrivalMetroEndDate;
				}

				if (exportInformation.RunWorkingDayOtherExport)
				{
					UPEDataRegistry.Instance.BISIUploadWorkingDayOtherHWM = exportInformation.WorkingDayOtherEndDate;
				}

				if (exportInformation.RunWorkingDayDateOfArrivalOtherExport)
				{
					UPEDataRegistry.Instance.BISIUploadWorkingDayDateOfArrivalOtherHWM = exportInformation.WorkingDayDateOfArrivalOtherEndDate;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notifications.Notify(new ErrorNotification(ErrorType.Error, "Fail to Save Modified Records " + ex.Message));
			}
			Notifications.Notify(new InfoNotification("Finish Saving Modified Records"));
		}

		ZDateTime GetHighWaterMarkChunk(ZDateTime previousHighWaterMark, ZDateTime dateForHWM)
		{
			ZDateTime result = dateForHWM;

			TimeSpan timeDifferences = result - previousHighWaterMark;
			if (timeDifferences.TotalHours > UPEDataRegistry.Instance.BISIUploadTimeBuffer)
			{
				result = previousHighWaterMark.AddHours(UPEDataRegistry.Instance.BISIUploadTimeBuffer);
			}

			return result;
		}

		IWorkTimeArithmetic CurrentBranchWorkingDays
		{
			get { return currentBranchWorkingDays ?? (currentBranchWorkingDays = WorkingDays.GetInstance(new BusinessObjectFactory { NameForDebugging = "WorkingDays" }, ZGuid.Empty, GlbBranch.CurrentBranch.PK)); }
		}
		IWorkTimeArithmetic currentBranchWorkingDays;

		IBISIFileExporter Exporter
		{
			get { return exporter ?? (exporter = BISIFileExporter); }
		}
		IBISIFileExporter exporter;

		FtpUploader Uploader
		{
			get { return uploader ?? (uploader = BISIUploader); }
		}
		FtpUploader uploader;

		protected virtual IBISIFileExporter BISIFileExporter
		{
			get { return new BISIFileExporter(Notifications); }
		}

		protected virtual FtpUploader BISIUploader
		{
			get { return new BISIUploader(Notifications); }
		}
	}
}
