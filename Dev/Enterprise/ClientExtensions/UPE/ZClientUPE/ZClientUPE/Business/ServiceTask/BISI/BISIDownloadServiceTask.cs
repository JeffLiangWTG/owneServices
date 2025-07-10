using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.Ftp;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Renci.SshNet.Common;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(BISIDownloadServiceTask.Code, "BISI COD Download", "CSP",
	typeof(BISIDownloadServiceTask),
	MinimumPeriod = "1minute",
	MaximumPeriod = "240minutes",
	DefaultScheduleRunEvery = "30minutes"
	)]
namespace Enterprise.Client.UPE.ServiceTask
{
	public class BISIDownloadServiceTask : UPEServiceTask
	{
		public const string Code = "ZU1";

		BusinessObjectFactoryProvider provider;

		public BISIDownloadServiceTask()
		{
		}

		public BISIDownloadServiceTask(ILogger logger)
			: base(logger)
		{
		}

		public override void RunTask(CancellationToken token)
		{
			var branch = UPETools.Instance.UPECustomisationBranches(false).FirstOrDefault(b => b.Country.Code == CountryCodes.Australia);
			if (branch != null)
			{
				using (branch.SetAsTemporaryContext())
				{
					try
					{
						WaitFor<Object>.Run(TimeSpan.FromSeconds(UPEDataRegistry.Instance.SftpServerTimeout), () =>
						{
							provider = new BusinessObjectFactoryProvider();
							provider.Current.RefreshEnabled = false;

							bool bISIFilesDownloaded = false;

							CleanupOldCharges(provider);
							ScanAndMatchPreDownloadedCharges(provider, token);

							ServiceLogger.Log(LogType.Information, "Start Downloading");
							try
							{
								bISIFilesDownloaded = DownloadAndProcessBISIZipFiles(provider, token);
							}
							catch (Exception ex)
							{
								if (ex is SshException || ex is InvalidOperationException || ex is SocketException)
								{
									ServiceLogger.Log(LogType.Error, ex.Message);
								}
								else
								{
									throw;
								}
							}
							finally
							{
								SendUploadWarningEmailIfRequired(bISIFilesDownloaded);
								ServiceLogger.Log(LogType.Information, "Finish Downloading");
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

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		[SuppressMessage("Enterprise", "EDI003", Justification = "Callout.BillToAccountNumber will truncate to 40 chars, and UAN CusCode type should be less than 35 anyway")]
		void ScanAndMatchPreDownloadedCharges(BusinessObjectFactoryProvider factoryProvider, CancellationToken token)
		{
			Notify("Searching for pre-downloaded charges...");

			string query = String.Format(CultureInfo.InvariantCulture, "SELECT {1} FROM {0} WHERE {2} >= @Date",
				ClientPWSHeaderSchema.PK.TableName,
				ClientPWSHeaderSchema.PK.Name,
				ClientPWSHeaderSchema.U1_ImportedDate.Name);

			List<ZGuid> pkList = new List<ZGuid>();

			using (DbCommand command = Db.Connection.Command(query))
			{
				command.AddParameter("@Date", SqlDbType.DateTime, ZDateTime.Now.AddDays(-7).Date.ToDateTime());
				using (IDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						token.ThrowIfCancellationRequested();
						pkList.Add(new ZGuid(reader[0]));
					}
				}
			}

			if (pkList.Count > 0)
			{
				Notify(pkList.Count + " shipments were found with pre-downloaded charges.");

				foreach (ZGuid pk in pkList)
				{
					token.ThrowIfCancellationRequested();
					ClientPWSHeader pwsHeader = factoryProvider.Current.Load<ClientPWSHeader>(pk);

					Callout callout = FindCusHawbWithTrackingID(factoryProvider.Current, pwsHeader.U1_WayBillNumber) ?? FindCusHawbWithTrackingID(factoryProvider.Current, pwsHeader.U1_WayBillShortNumber);

					if (callout != null)
					{
						IBisiUpload bisiUploadShipment = callout;
						if (bisiUploadShipment.TransferredDateTime.IsEmpty)
						{
							Notify(string.Format(CultureInfo.InvariantCulture, "Shipment {0} has not been uploaded to BISI yet...", callout.CS_HAWB));
						}
						else
						{
							IBisiDownload bisiDownloadShipment = callout;
							if (bisiDownloadShipment.TransferredDateTime.IsEmpty)
							{
								Notify("Processing " + callout.CS_HAWB);

								callout.EnsureJobHeaderExists();
								foreach (ClientPWSCharge pwsCharge in pwsHeader.Charges)
								{
									if (callout.JobHeader.Charges.FindByChargeDescription(pwsCharge.U2_ChargeDescription) == null)
									{
										CalloutCharge charge = callout.JobHeader.Charges.AddNew();
										charge.JR_Desc = pwsCharge.U2_ChargeDescription;
										charge.TaxableAmount = pwsCharge.U2_TaxableAmount;
										charge.NonTaxableAmount = pwsCharge.U2_NonTaxableAmount;
										charge.Discount = pwsCharge.U2_Discount;
										charge.NettAmount = pwsCharge.U2_NetAmount;
										Notify("Charge added: " + charge.JR_Desc);
									}
								}

								callout.CalculateTotalAmountDue();
								callout.CS_ChargableWeight = pwsHeader.U1_BillableWeight;

								if (pwsHeader.U1_BillToAccount.IsEmpty && callout.ImporterOrConsigneeMatchedOrgPK.IsValid)
								{
									callout.BillToAccountNumber = factoryProvider.Current.Load<UPEOrgHeader>(callout.ImporterOrConsigneeMatchedOrgPK).AccountNumber;
								}
								else
								{
									callout.BillToAccountNumber = pwsHeader.U1_BillToAccount;
								}

								callout.InvoiceNumber = pwsHeader.U1_InvoiceNumber;
								bisiDownloadShipment.TransferredDateTime = pwsHeader.U1_ImportedDate;
								bisiDownloadShipment.OnAfterBisiDownload();
							}

							pwsHeader.Delete();
							factoryProvider.SaveCurrentAndCreateNew();
						}
					}
					else
					{
						Notify(string.Format(CultureInfo.InvariantCulture, "Shipment {0} has not been imported from LEVEL 1 file yet...", pwsHeader.U1_WayBillNumber));
					}
				}
			}
			else
			{
				Notify("No shipments were found with pre-downloaded charges.");
			}
		}

		Callout FindCusHawbWithTrackingID(BusinessObjectFactory factory, ZString longOrShortTrackingID)
		{
			ZDBOnlyQuery trackingNumberFilter = new ZDBOnlyQuery(typeof(Callout));
			trackingNumberFilter.AddToFilter(CusHAWBSchema.CS_HAWB, longOrShortTrackingID);

			ZDBOnlySubQuery recordAddedFilter = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			recordAddedFilter.AddToFilter(StmALogSchema.SL_Table, SQLComparisonOperator.Equal, CusHAWBSchema.Constants.TableName);
			recordAddedFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.AddedARecordToTheSystem.Code);
			recordAddedFilter.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.UtcNow.AddDays(-7));

			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(UPECusHAWB));

			var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
			ZDBOnlySubQuery mawbSubQuery = new ZDBOnlySubQuery(typeof(UPECusMAWB), CusMAWBSchema.PK);
			mawbSubQuery.FilterByForeignKey(CusMAWBSchema.CM_GB, branches);

			filter.AddSubQuery(CusHAWBSchema.CS_CM, mawbSubQuery, JoinCondition.And);

			filter.AddToFilter(trackingNumberFilter);
			filter.AddSubQuery(recordAddedFilter, JoinCondition.And);

			return factory.LoadTop1<Callout>(filter);
		}

		void CleanupOldCharges(BusinessObjectFactoryProvider factoryProvider)
		{
			Notify("Pre-downloaded charges HOUSE CLEANING...");

			string query = String.Format(CultureInfo.InvariantCulture, "DELETE {0} FROM {0} INNER JOIN {2} ON {0}.{1} = {2}.{3}  WHERE {4} < @Date  DELETE FROM {2}  WHERE {4} < @Date",
				ClientPWSChargeSchema.U2_U1.TableName,
				ClientPWSChargeSchema.U2_U1.Name,
				ClientPWSHeaderSchema.PK.TableName,
				ClientPWSHeaderSchema.PK.Name,
				ClientPWSHeaderSchema.U1_ImportedDate.Name);

			using (DbCommand command = Db.Connection.Command(query))
			{
				command.AddParameter("@Date", SqlDbType.DateTime, ZDateTime.Now.AddDays(-7).AddDays(1).Date.ToDateTime());
				command.ExecuteNonQuery();
			}

			factoryProvider.Current.Save();
			Notify("House Cleaning Done!");
		}

		bool DownloadAndProcessBISIZipFiles(BusinessObjectFactoryProvider factoryProvider, CancellationToken token)
		{
			var successfullyImportedFiles = new List<string>();
			var zipFiles = Downloader.ListZipFiles();
			foreach (string zipFile in zipFiles)
			{
				token.ThrowIfCancellationRequested();
				if (Downloader.DownloadFile(zipFile))
				{
					if (ExtractZipFile(Downloader.DownloadedFileName, zipFile))
					{
						bool isImportSuccess = ImportCODFiles();
						if (isImportSuccess)
						{
							successfullyImportedFiles.Add(zipFile);
						}
					}
					TryDeleteFile(Downloader.DownloadedFileName);
				}
			}

			factoryProvider.Current.Save();
			Downloader.DeleteRemoteFile(successfullyImportedFiles);
			return zipFiles.Count > 0;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void SendUploadWarningEmailIfRequired(bool bISIFilesDownloaded)
		{
			try
			{
				Notifications.Notify(new InfoNotification($"BISI files downloaded = '{bISIFilesDownloaded}'"));

				if (bISIFilesDownloaded)
				{
					GetBISIUploadWarningReporter().SendWarningEmailIfRequired(Notifications);
				}
				else
				{
					Notifications.Notify(new InfoNotification("No warning email have been sent because there was no BISI files to be downloaded"));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notifications.Notify(new ErrorNotification(ErrorType.Error, "An error has occurred with warning report: " + ex.Message + System.Environment.NewLine + System.Environment.NewLine + ex));
			}
		}

		protected virtual BISIUploadWarningReporter GetBISIUploadWarningReporter()
		{
			return new BISIUploadWarningReporter();
		}

		#region Implementation

		protected virtual IBISIDownloader GetBISIDownloader()
		{
			return new BISIDownloader(Notifications);
		}

		protected virtual bool ImportCODFile(PWSFileImporter importer, string cODFile, INotifications notifications)
		{
			return importer.Import(cODFile, notifications);
		}

		bool ExtractZipFile(string localZipFileName, string serverZipFileName)
		{
			bool result = ExtractZipFileCore(localZipFileName);
			if (!result)
			{
				ErrorNotification errorNotification = new ErrorNotification(ErrorType.Error, "Cannot unzip file '" + serverZipFileName + "' downloaded from the ftp server (local file is '" + localZipFileName + "'). File may be corrupt.");
				Notifications.Notify(errorNotification);
			}
			return result;
		}

		protected virtual bool ExtractZipFileCore(string localZipFileName)
		{
			if (Directory.Exists(UnzipTargetPath))
			{
				Directory.CreateDirectory(UnzipTargetPath);
			}
			ClearDirectoryContent(UnzipTargetPath);
			return ZipCompression.Unzip(localZipFileName, UnzipTargetPath);
		}

		bool ImportCODFiles()
		{
			bool result = true;

			PWSFileImporter importer = new PWSFileImporter();
			foreach (string cODFile in Directory.GetFiles(UnzipTargetPath))
			{
				result = ImportCODFile(importer, cODFile, Notifications);
				if (!result)
				{
					TryDeleteFile(cODFile);
					break;
				}
				else
				{
					TryMoveOrDeleteFile(cODFile, UPEDataRegistry.Instance.BISIDownloadArchiveDirectory);
				}
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void ClearDirectoryContent(string directoryPath)
		{
			try
			{
				foreach (string fileToBeDeleted in Directory.GetFiles(directoryPath))
				{
					File.Delete(fileToBeDeleted);
				}

				foreach (string directoryToBeDeleted in Directory.GetDirectories(directoryPath))
				{
					ClearDirectoryContent(directoryToBeDeleted);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Do not do anything if files cannot be deleted as there is no consequences to the operation
				// This is simply a housekeeping routine
			}
		}

		string UnzipTargetPath
		{
			get { return Path.Combine(Env.TempPath, "BISIUnzippedFiles"); }
		}

		IBISIDownloader Downloader
		{
			get
			{
				if (fDownloader == null)
				{
					fDownloader = GetBISIDownloader();
				}
				return fDownloader;
			}
		}

		IBISIDownloader fDownloader;

		#endregion
	}
}
