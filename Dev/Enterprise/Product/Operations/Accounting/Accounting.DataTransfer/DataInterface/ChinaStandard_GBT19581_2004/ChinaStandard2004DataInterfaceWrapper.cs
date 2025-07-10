using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004
{
	public class ChinaStandard2004DataInterfaceWrapper : ChinaStandardWrapper
	{
		public ChinaStandard2004DataInterfaceWrapper()
			: this(new BusinessObjectFactory()) { }

		public ChinaStandard2004DataInterfaceWrapper(BusinessObjectFactory factory)
			: base(factory)
		{ }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (ExportXML)
			{
				ProcessFiles = ExportFiles;
				ExportFile();
			}
			else
			{
				ZString exportFileName = Path.Combine(Env.TempPath, "GSSM.txt");
				if (!File.Exists(exportFileName))
				{
					File.WriteAllText(exportFileName, GSSMFile.Gssm, Encoding.GetEncoding("utf-8"));
				}

				try
				{
					EmailDef mail = new EmailDef();
					mail.Attachments.Add(new AttachmentDef(exportFileName));
					mail.Subject = Res.GetString("B14DFC80-5DB2-4B4B-A8EE-C1B3E1BDA26A", "GBT19581_2004 File - GSSM.txt");
					mail.Body = Res.GetString("CE414C5E-FF79-41C8-8EF8-6EF7E008EF2E", "Enclosed please find the mapped GBT19581_2004 file. You can open it with Notepad or other text file editors.");
					mail.AddRecipientForUserCommunication(DeliveryTo);
					mail.FromAddress = "eHubNoRely@cargowise.com";
					mail.FromDisplayName = "eHubNoRely@cargowise.com";
					Env.OutgoingMailManager.Create(Factory, mail);
				}
				finally
				{
					File.Delete(exportFileName);
				}

				ExportDirectory = "";
				foreach (var exportFileType in ExportFiles.ToList())
				{
					var exportFile = exportFileType;
					ProcessFiles.Clear();
					if (exportFile == "SupplementaryAccounts")
					{
						ProcessFiles.Add("Department");
						ExportFile();

						ProcessFiles.Clear();
						ProcessFiles.Add("Staff");
						ExportFile();

						ProcessFiles.Clear();
						exportFile = "Client";
					}

					ProcessFiles.Add(exportFile);
					ExportFile();
				}
			}
			HasChanges = false;
		}

		void ExportFile()
		{
			if (Exporter != null && !Exporter.ExportData(new ChinaStandard2004DataAdapter()))
			{
				throw new ZCannotSaveException("Export failed.", Res.GetString("aedb5d81-b897-44ea-b6bc-787cc6f76b8c", "Export files failed!"));
			}
		}

		#region Public Properties

		public List<ZString> ProcessFiles = new List<ZString>();
		public List<ZString> ExportFiles = new List<ZString>();

		public ZBool AssetProvision
		{
			get { return fAssetProvision; }
			set
			{
				SetNonPersistentPropertyValue(AssetProvisionInfo, ref fAssetProvision, value);
				if (fAssetProvision)
				{
					if (ExportFiles.All(file => file != "AssetProvision"))
					{
						ExportFiles.Add("AssetProvision");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "AssetProvision"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo AssetProvisionInfo
		{
			get { return GetZPropertyInfo(nameof(AssetProvision)); }
		}

		public ZBool ChartOfAccounts
		{
			get { return fChartOfAccounts; }
			set
			{
				SetNonPersistentPropertyValue(ChartOfAccountsInfo, ref fChartOfAccounts, value);
				if (fChartOfAccounts)
				{
					if (ExportFiles.All(file => file != "ChartOfAccounts"))
					{
						ExportFiles.Add("ChartOfAccounts");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "ChartOfAccounts"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo ChartOfAccountsInfo
		{
			get { return GetZPropertyInfo(nameof(ChartOfAccounts)); }
		}

		public ZBool AccountBook
		{
			get { return fAccountBook; }
			set
			{
				SetNonPersistentPropertyValue(AccountBookInfo, ref fAccountBook, value);
				if (fAccountBook)
				{
					if (ExportFiles.All(file => file != "AccountBook"))
					{
						ExportFiles.Add("AccountBook");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "AccountBook"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo AccountBookInfo
		{
			get { return GetZPropertyInfo(nameof(AccountBook)); }
		}

		public ZBool SupplementaryAccounts
		{
			get { return fSupplementaryAccounts; }
			set
			{
				SetNonPersistentPropertyValue(SupplementaryAccountsInfo, ref fSupplementaryAccounts, value);
				if (fSupplementaryAccounts)
				{
					if (ExportFiles.All(file => file != "SupplementaryAccounts"))
					{
						ExportFiles.Add("SupplementaryAccounts");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "SupplementaryAccounts"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo SupplementaryAccountsInfo
		{
			get { return GetZPropertyInfo(nameof(SupplementaryAccounts)); }
		}

		public ZBool AccountingVouchers
		{
			get { return fAccountingVouchers; }
			set
			{
				SetNonPersistentPropertyValue(AccountingVouchersInfo, ref fAccountingVouchers, value);
				if (fAccountingVouchers)
				{
					if (ExportFiles.All(file => file != "AccountingVouchers"))
					{
						ExportFiles.Add("AccountingVouchers");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "AccountingVouchers"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo AccountingVouchersInfo
		{
			get { return GetZPropertyInfo(nameof(AccountingVouchers)); }
		}

		public ZBool TrialBalance
		{
			get { return fTrialBalance; }
			set
			{
				SetNonPersistentPropertyValue(TrialBalanceInfo, ref fTrialBalance, value);
				if (fTrialBalance)
				{
					if (ExportFiles.All(file => file != "TrialBalance"))
					{
						ExportFiles.Add("TrialBalance");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "TrialBalance"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo TrialBalanceInfo
		{
			get { return GetZPropertyInfo(nameof(TrialBalance)); }
		}

		public ZBool BalanceSheet
		{
			get { return fBalanceSheet; }
			set
			{
				SetNonPersistentPropertyValue(BalanceSheetInfo, ref fBalanceSheet, value);
				if (fBalanceSheet)
				{
					if (ExportFiles.All(file => file != "BalanceSheet"))
					{
						ExportFiles.Add("BalanceSheet");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "BalanceSheet"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo BalanceSheetInfo
		{
			get { return GetZPropertyInfo(nameof(BalanceSheet)); }
		}

		public ZBool ProfitAndLoss
		{
			get { return fProfitAndLoss; }
			set
			{
				SetNonPersistentPropertyValue(ProfitAndLossInfo, ref fProfitAndLoss, value);
				if (fProfitAndLoss)
				{
					if (ExportFiles.All(file => file != "ProfitAndLoss"))
					{
						ExportFiles.Add("ProfitAndLoss");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "ProfitAndLoss"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo ProfitAndLossInfo
		{
			get { return GetZPropertyInfo(nameof(ProfitAndLoss)); }
		}

		public ZBool VATDetailed
		{
			get { return fVATDetailed; }
			set
			{
				SetNonPersistentPropertyValue(VATDetailedInfo, ref fVATDetailed, value);
				if (fVATDetailed)
				{
					if (ExportFiles.All(file => file != "VATDetailed"))
					{
						ExportFiles.Add("VATDetailed");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "VATDetailed"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo VATDetailedInfo
		{
			get { return GetZPropertyInfo(nameof(VATDetailed)); }
		}

		public ZBool PNLAppropriation
		{
			get { return fPNLAppropriation; }
			set
			{
				SetNonPersistentPropertyValue(PNLAppropriationInfo, ref fPNLAppropriation, value);
				if (fPNLAppropriation)
				{
					if (ExportFiles.All(file => file != "PNLAppropriation"))
					{
						ExportFiles.Add("PNLAppropriation");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "PNLAppropriation"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo PNLAppropriationInfo
		{
			get { return GetZPropertyInfo(nameof(PNLAppropriation)); }
		}

		public ZBool EquityMovement
		{
			get { return fEquityMovement; }
			set
			{
				SetNonPersistentPropertyValue(EquityMovementInfo, ref fEquityMovement, value);
				if (fEquityMovement)
				{
					if (ExportFiles.All(file => file != "EquityMovement"))
					{
						ExportFiles.Add("EquityMovement");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "EquityMovement"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo EquityMovementInfo
		{
			get { return GetZPropertyInfo(nameof(EquityMovement)); }
		}

		public ZBool CashFlowStatement
		{
			get { return fCashFlowStatement; }
			set
			{
				SetNonPersistentPropertyValue(CashFlowStatementInfo, ref fCashFlowStatement, value);
				if (fCashFlowStatement)
				{
					if (ExportFiles.All(file => file != "CashFlowStatement"))
					{
						ExportFiles.Add("CashFlowStatement");
					}
				}
				else
				{
					foreach (var file in ExportFiles.Where(file => file == "CashFlowStatement"))
					{
						ExportFiles.Remove(file);
						break;
					}
				}
				if (!IsValidationSuspended)
				{
					ValidatExportFiles();
				}
			}
		}

		public ZPropertyInfo CashFlowStatementInfo
		{
			get { return GetZPropertyInfo(nameof(CashFlowStatement)); }
		}

		ZBool fEquityMovement;
		ZBool fAssetProvision;
		ZBool fAccountBook;
		ZBool fSupplementaryAccounts;
		ZBool fAccountingVouchers;
		ZBool fTrialBalance;
		ZBool fBalanceSheet;
		ZBool fProfitAndLoss;
		ZBool fChartOfAccounts;
		ZBool fVATDetailed;
		ZBool fPNLAppropriation;
		ZBool fCashFlowStatement;
		ZString fDeliveryTo;

		#endregion

		#region Export TXT Or XML files

		public BizObjThatDoesntSaveForCN2004.FilesType ExportTXTOrXML { get; set; }

		public ZPropertyInfo ExportTXTInfo
		{
			get { return GetZPropertyInfo(nameof(ExportTXT)); }
		}

		public ZBool ExportTXT
		{
			get { return ExportTXTOrXML == BizObjThatDoesntSaveForCN2004.FilesType.TXT; }
			set
			{
				ExportTXTOrXML = value ? BizObjThatDoesntSaveForCN2004.FilesType.TXT : BizObjThatDoesntSaveForCN2004.FilesType.XML;
				ExportTXTInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExportXMLInfo
		{
			get { return GetZPropertyInfo(nameof(ExportXML)); }
		}

		public ZBool ExportXML
		{
			get { return ExportTXTOrXML == BizObjThatDoesntSaveForCN2004.FilesType.XML; }
			set
			{
				ExportTXTOrXML = value ? BizObjThatDoesntSaveForCN2004.FilesType.XML : BizObjThatDoesntSaveForCN2004.FilesType.TXT;
				ExportXMLInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DeliveryToInfo
		{
			get { return GetZPropertyInfo(nameof(DeliveryTo)); }
		}

		public ZString DeliveryTo
		{
			get
			{
				return fDeliveryTo;
			}
			set
			{
				if (fDeliveryTo != value)
				{
					SetNonPersistentPropertyValue(DeliveryToInfo, ref fDeliveryTo, value);
					if (!IsValidationSuspended)
					{
						ValidateDeliveryTo();
					}
				}
			}
		}

		#endregion

		#region Validation

		protected override void ValidateExportDirectory()
		{
			if (ExportXML)
			{
				base.ValidateExportDirectory();
			}
			else
			{
				ExportDirectoryInfo.ClearAllNotifications();
			}
		}

		protected void ValidateDeliveryTo()
		{
			DeliveryToInfo.ClearAllNotifications();
			if (ExportTXT && !EmailAddressValidation.IsEmailAddressValidAndNotEmpty(DeliveryTo))
			{
				DeliveryToInfo.AddError(Res.GetString("ffce971e-51fe-60ee-7dce-81f49b5ea4e2", "Enter a valid email address"));
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatExportFiles();
			ValidateDeliveryTo();
		}

		protected void ValidatExportFiles()
		{
			AccountBookInfo.ClearAllNotifications();
			SupplementaryAccountsInfo.ClearAllNotifications();
			ChartOfAccountsInfo.ClearAllNotifications();
			AccountingVouchersInfo.ClearAllNotifications();
			TrialBalanceInfo.ClearAllNotifications();
			BalanceSheetInfo.ClearAllNotifications();
			ProfitAndLossInfo.ClearAllNotifications();
			VATDetailedInfo.ClearAllNotifications();
			AssetProvisionInfo.ClearAllNotifications();
			PNLAppropriationInfo.ClearAllNotifications();
			EquityMovementInfo.ClearAllNotifications();
			CashFlowStatementInfo.ClearAllNotifications();
			if (ExportFiles.Count == 0)
			{
				ZString errorMessage = (Res.GetString("96D3EDCD-AFA8-4F31-913B-B2DD3F8BCB6D", "Please select at least one \"Data Types\" for export."));
				AccountBookInfo.AddError(errorMessage);
				SupplementaryAccountsInfo.AddError(errorMessage);
				AccountingVouchersInfo.AddError(errorMessage);
				TrialBalanceInfo.AddError(errorMessage);
				BalanceSheetInfo.AddError(errorMessage);
				ProfitAndLossInfo.AddError(errorMessage);
				ChartOfAccountsInfo.AddError(errorMessage);
				VATDetailedInfo.AddError(errorMessage);
				AssetProvisionInfo.AddError(errorMessage);
				PNLAppropriationInfo.AddError(errorMessage);
				EquityMovementInfo.AddError(errorMessage);
				CashFlowStatementInfo.AddError(errorMessage);
			}

			ValidatePeriod();
		}

		protected override void ValidatePeriod()
		{
			PeriodInfo.ClearAllNotifications();

			if (AccountingVouchers || TrialBalance || BalanceSheet || ProfitAndLoss || VATDetailed || AssetProvision || PNLAppropriation || EquityMovement || CashFlowStatement)
			{
				if (Period.IsEmpty || Period == 0)
				{
					ZString errorMessage = Res.GetString("189CDE9D-C80E-4F03-A57C-F559759F2B6A",
														 "Please enter a valid accounting period.");
					PeriodInfo.AddError(errorMessage);
				}
				else if (!PeriodCalculator.IsPeriodValid(Period))
				{
					PeriodInfo.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(Period));
				}
				else if (!PeriodCalculator.IsPeriodGLClosed(Period) || !PeriodCalculator.IsPeriodSubLedgerClosed(Period))
				{
					string errorMessage1 = Res.GetString("02DDA54D-CE04-2678-F262-EE5F3CE336DF",
														"This Period '{0}' is not closed. Please close both sub ledger and general ledger for this period before running the export function.",
														Period);
					PeriodInfo.AddError(errorMessage1);
				}
			}
		}

		protected override void ValidateBranch()
		{
			base.ValidateBranch();
			if (AccountingVouchers && !Branch.IsEmpty)
			{
				BranchInfo.AddWarning(ResString.GetMultilingualString("E3B82D9E-B757-4461-BD70-4D85DA93704F", "You cannot select Branch Filter if you are printing Job Costing Voucher."));
			}
		}

		#endregion
	}
}
