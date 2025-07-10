using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class AccTransactionsExportTask : XMLTask
	{
		public AccTransactionsExportTask(INotifications notifications, AccountingTransactionExporter transactionsDataExporter, IRegistryItem dataTransferSwitchRegistryItem, GlbCompany company)
			: base(notifications)
		{
			exporter = transactionsDataExporter ?? throw new ArgumentNullException(nameof(transactionsDataExporter));

			accTransactionsExportItem = dataTransferSwitchRegistryItem as DataTransferSwitchRegistryItem ?? throw new ArgumentNullException(nameof(dataTransferSwitchRegistryItem));

			NotificationBuffer = new NotificationBuffer(notifications);
			Company = company;
			Factory = exporter.Factory;
		}

		readonly GlbCompany Company;
		readonly BusinessObjectFactory Factory;
		readonly NotificationBuffer NotificationBuffer;

		#region Override

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Putting back the initial context")]
		protected override void RunTask()
		{
			var initialUserContext = Env.CurrentUserContext;

			try
			{
				if (LoginToBranchOfCompany())
				{
					ZDateTime now = ZDateTime.Now;
					if (now >= AccountingTransactionsExportNextRun)
					{
						SetupExportFilter(exporter);
						NotificationBuffer.Clear();
						NotificationBuffer.Notify(new InfoNotification(Res.GetString("ce221bdf-e7a5-4b3d-92be-22570dd5796c", "Running Accounting Transaction Export For Company {0} (Branch {1}) ...", Company.GC_Code, BranchesOfExportCompany[0].GB_Code)));
						var errorOccured = false;
						try
						{
							int count = NumberOfWipAndAccrualTransactionsForExport;
							if (count > MaxNumberOfWipAndAccrualTransactionsToExport)
							{
								NotificationBuffer.Notify(new InfoNotification(Res.GetString("e57abac2-7916-429c-bb9a-307dfb3e975f", "Error: Batch was not created.  Unable to export more than {0} WIP and Accrual transactions at a time.  However, there are {1} WIP and Accrual transactions for export.\r\nRun the manual export, located at Accounts -> General Ledger -> Export Transactions, with date filters to create a number of files that have less than {0} WIP and Accrual transactions per file.\r\nAfter this backlog is cleared, the automatic batch export will be able to resume.", MaxNumberOfWipAndAccrualTransactionsToExport, count)));
								errorOccured = true;
							}
							else
							{
								using (TempFile tempFile = TempFile.New())
								using (FileStream stream = new FileStream(tempFile.Filename, FileMode.Create))
								{
									exporter.Export(stream);
									NotificationBuffer.Notify(new InfoNotification(exporter.GetMessageToDisplayWhenExportIsFinished()));
									if (!exporter.ErrorHasOccured && exporter.FilterProvider.CurrentBatchNo != 0)
									{
										errorOccured = !CopyFileWithMultiAttempts(tempFile.Filename, 3);
									}
								}
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							errorOccured = true;
							AddErrorToNotificationBuffer(ex);
							if (!(ex is IOException || ex is UnauthorizedAccessException))
							{
								throw;
							}
						}
						finally
						{
							NotificationBuffer.Notify(new InfoNotification(Res.GetString("d266432d-1dd7-4c26-b7ef-06162760d3f6", "Accounting Transaction Export...finished")));
							SendNotificationEmail(errorOccured);
							UpdateAccountingTransactionsExportLastRun(now);
							exporter.ClearExportedLists();
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				AddErrorToNotificationBuffer(ex);
				throw;
			}
			finally
			{
				Env.SetUserContext(initialUserContext); // Putting back the initial context
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		bool CopyFileWithMultiAttempts(string sourceFileName, int count)
		{
			var result = false;

			for (var i = 0; i < count; i++)
			{
				GenerateFileName(i);
				try
				{
					File.Copy(sourceFileName, currentFileName);
					result = CheckThatFileHasBeenCopiedSuccessfully(sourceFileName, currentFileName, NotificationBuffer);
					if (result)
					{
						break;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					AddErrorToNotificationBuffer(ex, currentFileName);
				}
			}

			return result;
		}

		void AddErrorToNotificationBuffer(Exception ex, string fileName = null)
		{
			if (!ex.IsCriticalException())
			{
				ZString errorMesg;
				if (fileName == null)
				{
					errorMesg = Res.GetString("124CA365-8548-4B7F-B710-15270782AC5D", "Error Exporting Transaction For Company {0} (Branch {1}) - {2}", Company.GC_Code, BranchesOfExportCompany[0].GB_Code, ex.Message);
				}
				else
				{
					var info = new FileInfo(fileName);
					errorMesg = Res.GetString("9438EF0A-C8AE-4F5A-99CC-45BD87595F9E",
						"Error Exporting Transaction For Company {0} (Branch {1}) - {2} Creation Time: {3} Last Write Time {4}",
						Company.GC_Code, BranchesOfExportCompany[0].GB_Code, ex.Message, info.CreationTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
						info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
				}

				NotificationBuffer.Notify(new ErrorNotification(ErrorType.Error, errorMesg));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Caller will put back the initial context")]
		bool LoginToBranchOfCompany()
		{
			bool result = Company.Branches.FindByPK(GlbBranch.CurrentBranch.PK) != null;

			GlbDepartment department = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, DefaultDepartmentCode);

			if (!result && BranchesOfExportCompany.Count > 0 && department != null)
			{
				Env.SetUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, BranchesOfExportCompany[0].PK.ToGuid(), department.PK.ToGuid())); // Caller will put back the initial context
				result = true;
			}

			return result;
		}

		GlbBranchCollection BranchesOfExportCompany
		{
			get
			{
				if (branchesOfExportCompany == null)
				{
					branchesOfExportCompany = AccountingUtils.GetBranchesOfCompanySortedByTimeZone(Company.PK, exporter.Factory);
				}

				return branchesOfExportCompany;
			}
		}
		GlbBranchCollection branchesOfExportCompany;

		protected override bool IsEnvironmentDataValid()
		{
			bool result = ExportSettings.EnableInterface;
			if (result)
			{
				result = AccountingTransactionsExportIsOkToExport;
				if (!AccountingTransactionsExportDirectory.IsEmpty &&
					!Directory.Exists(Path.GetFullPath(AccountingTransactionsExportDirectory)))
				{
					NotificationBuffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("38d14af9-d415-43ee-9835-7b62a335782b", "Export directory {0} doesn't exist.", AccountingTransactionsExportDirectory)));
					result = false;
				}
				if (ExportSettings.GroupPK.IsEmpty || !ExportSettings.GroupPK.IsValid)
				{
					NotificationBuffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("31a0d50c-15e4-46ed-a8e5-00c01ee3c7d4", "Cannot notify group, System/Data Export Settings/Accounting Transactions/Notify Group is empty or invalid.", AccountingTransactionsExportDirectory)));
					result = false;
				}
			}
			return result;
		}

		protected bool ErrorHasOccured
		{
			get
			{
				return exporter.ErrorHasOccured;
			}
		}

		protected bool NoBatchCreated
		{
			get
			{
				return NumberOfWipAndAccrualTransactionsForExport > MaxNumberOfWipAndAccrualTransactionsToExport;
			}
		}

		public override ZString UniqueIdentifier
		{
			get
			{
				return ZString.Format("AccTransactionsExportTask. Company: {0}.", Company.GC_Code);
			}
		}

		protected override string NotificationEmailSubject
		{
			get
			{
				string result = Res.GetString("bc50499c-4925-47b2-8e0f-5303972ed03c", "Accounting Transactions XML Export {0} (Branch {1}) ", Company.GC_Code, BranchesOfExportCompany[0].GB_Code);
				if (NoBatchCreated)
				{
					result += Res.GetString("5061d63f-d549-43b1-a795-d2831293fdb8", "- Export Failed (No Batch Created)");
				}
				else if (exporter.FilterProvider.CurrentBatchNo == 0)
				{
					result += Res.GetString("77bcee08-bd2d-4e55-9389-3f88b1e06092", "- No Transactions To Export");
				}
				else if (!ErrorEmailGeneration)
				{
					result += Res.GetString("0beed601-1425-4f1b-9c89-6a8b5261f899", "- Success");
				}
				else
				{
					result += Res.GetString("496bc1e0-8e92-4dc0-82a7-5f656bfeef5e", "- Export Failed");
				}

				if (exporter.FilterProvider.CurrentBatchNo != 0)
				{
					result += " " + Res.GetString("bcdab543-9a1e-4057-9384-8502d69762a2", "(Batch {0})", exporter.FilterProvider.CurrentBatchNo);
				}

				return result;
			}
		}

		protected override EmailDef CreateEmailDef(ZString message, FileInfo dataFile)
		{
			EmailDef email = new EmailDef();
			email.Subject = NotificationEmailSubject;
			email.Body = message;
			if (dataFile != null && dataFile.Exists)
			{
				email.Attachments.Add(new AttachmentDef(string.Format("{0}-{1}.{2}", NotificationEmailSubject, Path.GetFileNameWithoutExtension(currentFileName), TFileFormats.Xls), dataFile.FullName));
			}
			return email;
		}

		#endregion

		#region Properties

#if DEBUG
		protected virtual
#endif
 short MaxNumberOfWipAndAccrualTransactionsToExport
		{
			get { return short.MaxValue; }
		}

		int NumberOfWipAndAccrualTransactionsForExport
		{
			get
			{
				int count = 0;

				if (exporter.FilterProvider.IncludeAccrualsPosting)
				{
					count += exporter.NumberOfAccrualPostingsInBatch;
				}

				if (exporter.FilterProvider.IncludeAccrualsReversing)
				{
					count += exporter.NumberOfAccrualReversalsInBatch;
				}

				if (exporter.FilterProvider.IncludeWIPsPosting)
				{
					count += exporter.NumberOfWipPostingsInBatch;
				}

				if (exporter.FilterProvider.IncludeWIPsReversing)
				{
					count += exporter.NumberOfWipReversalsInBatch;
				}

				return count;
			}
		}

		#endregion

		#region Implementation

		bool CheckThatFileHasBeenCopiedSuccessfully(string sourceFile, string currentFile, NotificationBuffer notificationBuffer)
		{
			var result = false;

			var sourceFileInfo = new FileInfo(sourceFile);
			var currentFileInfo = new FileInfo(currentFile);

			if (currentFileInfo.Exists)
			{
				if (sourceFileInfo.Length == currentFileInfo.Length)
				{
					notificationBuffer.Notify(new InfoNotification(Res.GetString("f754bd0b-06c2-4676-be72-68c418b92ac6",
@"The XML file for Batch {0} was created successfully:
- File Path: {1}
- File Size: {2}", exporter.FilterProvider.CurrentBatchNo, currentFileInfo.FullName,
currentFileInfo.Length < 1024 ? currentFileInfo.Length + " B" : Utilities.Round(currentFileInfo.Length / 1024, 0) + " KB") + "\r\n\r\n"));

					result = true;
				}
				else
				{
					notificationBuffer.Notify(new InfoNotification(Res.GetString("edbdb521-e524-417d-836b-f4a6cb96a1a0",
@"Batch {0} was created successfully, however an error occurred when copying the XML file.
- File Path: {1}
- File Size: {2}
- Temporary File Size: {3}", exporter.FilterProvider.CurrentBatchNo, currentFileInfo.FullName,
currentFileInfo.Length < 1024 ? currentFileInfo.Length + " B" : Utilities.Round(currentFileInfo.Length / 1024, 0) + " KB",
sourceFileInfo.Length < 1024 ? sourceFileInfo.Length + " B" : Utilities.Round(sourceFileInfo.Length / 1024, 0) + " KB") + "\r\n\r\n"));
				}
			}
			else
			{
				notificationBuffer.Notify(new InfoNotification(Res.GetString("94c8e22c-1927-4715-a1bd-9365ddc6ca87",
@"Batch {0} was created successfully, however an error occurred when copying the XML file.
You can re-export batch {0} from Accounts > General Ledger > Export Transactions > Export Existing Batch.", exporter.FilterProvider.CurrentBatchNo)));
			}

			return result;
		}

#if DEBUG
		protected virtual
#endif
 void SetupExportFilter(AccountingTransactionExporter exporter)
		{
			exporter.FilterProvider.CopyRegistryValuesToFilter();
			exporter.FilterProvider.CurrentBatchNo = 0;
		}

		string GenerateFileName(int i = 0)
		{
			return currentFileName = Path.Combine(Path.GetFullPath(AccountingTransactionsExportDirectory), string.Format(CultureInfo.CurrentCulture, "{0}-{1}.xml", ZDateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.CurrentCulture), exporter.FilterProvider.CurrentBatchNo + (i == 0 ? "" : "_" + i)));
		}

#if DEBUG
		protected
#endif
 string currentFileName = string.Empty;

		void SendNotificationEmail(bool hasErrorOccured)
		{
			if (hasErrorOccured || exporter.ErrorHasOccured || exporter.FilterProvider.CurrentBatchNo == 0)
			{
				ErrorEmailGeneration = hasErrorOccured || exporter.ErrorHasOccured;
				try
				{
					SendEmailToNotificationGroup(AccountingTransactionsExportNotifyGroupPK.ToGuid(), accTransactionsExportItem, null, NotificationBuffer.AsString);
				}
				finally
				{
					ErrorEmailGeneration = false;
				}
			}
			else
			{
				using (TempFile xlsTempFile = TempFile.NewWithExtension(nameof(TFileFormats.Xls)))
				{
					BuildXlsReport(xlsTempFile);
					SendEmailToNotificationGroup(AccountingTransactionsExportNotifyGroupPK.ToGuid(), accTransactionsExportItem, new FileInfo(xlsTempFile.Filename), NotificationBuffer.AsString);
				}
			}
		}

		bool ErrorEmailGeneration;

		#endregion

		#region Excel Report

		void BuildXlsReport(TempFile xlsFile)
		{
			using (ExcelInterface excel = new ExcelInterface())
			{
				excel.NewExcelFile(1);
				Dictionary<string, object> reportDetails = new Dictionary<string, object>();

				reportDetails.Add(Res.GetString("375996f8-93a3-48fb-ab9d-8cae28ac825e", "Batch No."), exporter.FilterProvider.CurrentBatchNo);
				reportDetails.Add(Res.GetString("e3e0b0ec-2ebd-45a6-b0a8-963ff9632a3f", "Total Number of WIPs"), exporter.NumberOfWipPostingProcessed_Standard + exporter.NumberOfWipReversingProcessed_Standard);
				reportDetails.Add(Res.GetString("d6dcc216-f7d6-403c-aae0-d74c79468dd5", "Total Number of Accruals"), exporter.NumberOfAccrualPostingProcessed_Standard + exporter.NumberOfAccrualReversingProcessed_Standard);
				reportDetails.Add(Res.GetString("ebae0c0a-653e-4a7b-8edc-641e66d29b96", @"Total A/R amount (in local currency)"), exporter.TotalARAmount);
				reportDetails.Add(Res.GetString("e2618b85-a2bc-44a2-9931-1997f87ec085", @"Total A/P amount (in local currency)"), exporter.TotalAPAmount);

				int start = 1;
				foreach (KeyValuePair<string, object> pair in reportDetails)
				{
					excel.Xls.SetCellValue(start, 1, pair.Key);
					excel.Xls.SetCellValue(start++, 2, pair.Value is ZDecimal ? ((ZDecimal)pair.Value).Round(2) : pair.Value);
				}

				PrintInvoiceList(excel.Xls, exporter.ExportedARInvoices, Res.GetString("434b182f-81aa-4ed3-af59-770d0deec911", "AR Invoices"), start += 5);
				PrintInvoiceList(excel.Xls, exporter.ExportedAPInvoices, Res.GetString("81aa907f-adfe-4ba7-9401-c7b00c0a355c", "AP Invoices"), start += 5 + exporter.ExportedARInvoices.Count);
				PrintInvoiceList(excel.Xls, exporter.ExportedARCreditNote, Res.GetString("fa08a63c-005a-492b-a94e-1c73adafd809", "AR Credit Notes"), start += 5 + exporter.ExportedAPInvoices.Count);
				PrintInvoiceList(excel.Xls, exporter.ExportedAPCreditNote, Res.GetString("e55789ab-01e2-4eb0-b582-faa77a0ba8c9", "AP Credit Notes"), start += 5 + exporter.ExportedARCreditNote.Count);
				excel.SaveToFile(xlsFile.Filename);
			}
		}

		void PrintInvoiceList(ExcelFile xlsFile, IList<InvoicingBase> invoices, string headerName, int startRow)
		{
			xlsFile.SetCellValue(startRow++, 1, headerName);
			PrintHeader(xlsFile, startRow++);
			startRow++;
			foreach (InvoicingBase invoice in invoices)
			{
				xlsFile.SetCellValue(startRow, 1, invoice.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.LegacySystemCode, GlbCompany.CurrentCompany.Country));
				xlsFile.SetCellValue(startRow, 2, invoice.Header.OH_Code);
				xlsFile.SetCellValue(startRow, 3, invoice.AH_TransactionNum);
				xlsFile.SetCellValue(startRow++, 4, invoice.AH_InvoiceAmount + invoice.AH_GSTAmount);
			}
		}

		void PrintHeader(ExcelFile xlsFile, int row)
		{
			xlsFile.SetCellValue(row, 1, Res.GetString("fa8b3efb-5929-4a52-993b-edd3fcaf74cc", "Legacy Organization code"));
			xlsFile.SetCellValue(row, 2, Res.GetString("7f743c36-b071-404f-b1e5-84b7c9357463", "{0} Organization code", Core.Constants.ProductName));
			xlsFile.SetCellValue(row, 3, Res.GetString("142a5f7d-6b32-4508-af56-e079e7e477cc", "Reference no."));
			xlsFile.SetCellValue(row, 4, Res.GetString("054d147c-b931-4126-b862-16c6c8f43bd5", "Total amount (in local currency)"));
		}

		#endregion

		readonly AccountingTransactionExporter exporter;

		public AccountingTransactionExporter Exporter
		{
			get { return exporter; }
		}

		#region Accounting Transactions Export Settings

		readonly DataTransferSwitchRegistryItem accTransactionsExportItem;

		DataTransferSwitchRegistryBusinessObject ExportSettings
		{
			get { return accTransactionsExportItem.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty); }
		}

		public ZDateTime AccountingTransactionsExportNextRun
		{
			get { return ExportSettings.NextRunDateTime; }
		}

		public ZString AccountingTransactionsExportDirectory
		{
			get { return ExportSettings.Directory; }
		}

		public ZBool AccountingTransactionsExportIsOkToExport
		{
			get { return ExportSettings.IsGoodToGo; }
		}

		public ZGuid AccountingTransactionsExportNotifyGroupPK
		{
			get { return ExportSettings.GroupPK; }
		}

		public void UpdateAccountingTransactionsExportLastRun(ZDateTime lastRun)
		{
			accTransactionsExportItem.UpdateLastRun(lastRun);
		}

		#endregion

		readonly ZString DefaultDepartmentCode = "BRN";
	}
}
