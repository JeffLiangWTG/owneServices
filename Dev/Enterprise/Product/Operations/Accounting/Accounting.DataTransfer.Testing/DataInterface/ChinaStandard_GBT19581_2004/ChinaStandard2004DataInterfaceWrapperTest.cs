using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.DataInterface.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004.Testing
{
	[TestedType(typeof(ChinaStandard2004DataInterfaceWrapper))]
	public class ChinaStandard2004DataInterfaceWrapperTest : ChinaStandardWrapperTest
	{
		public void TestSelectBranchShouldShowWarning()
		{
			var branch = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK)).FirstOrDefault();
			AssertNotNull(branch);
			BizObj.AccountingVouchers = true;
			BizObj.Branch = branch.PK;
			AssertHasWarning(BizObj.BranchInfo, "You cannot select Branch Filter if you are printing Job Costing Voucher.");
			BizObj.Branch = ZGuid.Empty;
			AssertNoWarnings(BizObj.BranchInfo);
		}

		public void TestValidateDeliveryTo()
		{
			BizObj.ExportTXT = true;
			BizObj.DeliveryTo = "abc@email.com";
			BizObj.RunPreSaveValidation();
			AssertNoErrors("DeliveryTo should not have error", BizObj.DeliveryToInfo);
			AssertNoErrors("ExportDirectory should not have error", BizObj.ExportDirectoryInfo);

			BizObj.DeliveryTo = "c:\b"; // specific special path
			BizObj.RunPreSaveValidation();
			AssertHasErrors("Enter a valid email address", BizObj.DeliveryToInfo);
		}

		public void TestPropertyExportTXTOrXML()
		{
			BizObj.ExportXML = true;
			BizObj.ExportTXT = true;
			AssertEquals("The 'ExportTXTOrXML' should have be TXT.", BizObjThatDoesntSaveForCN2004.FilesType.TXT, BizObj.ExportTXTOrXML);
			AssertEquals("The 'ExportXML' should be false.", false, BizObj.ExportXML);
			BizObj.ExportXML = true;
			AssertEquals("The 'ExportTXTOrXML' should have be XML.", BizObjThatDoesntSaveForCN2004.FilesType.XML, BizObj.ExportTXTOrXML);
			AssertEquals("The 'ExportTXT' should be false.", false, BizObj.ExportTXT);
		}

		public void TestPropertiesZboolAndValidatExportFiles()
		{
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles count should be 0", 0, BizObj.ExportFiles.Count);

			BizObj.AccountBook = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have AccountBook", "AccountBook", BizObj.ExportFiles[0]);

			BizObj.AccountBook = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.SupplementaryAccounts = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have SupplementaryAccounts", "SupplementaryAccounts", BizObj.ExportFiles[0]);

			BizObj.SupplementaryAccounts = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.AccountingVouchers = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have AccountingVouchers", "AccountingVouchers", BizObj.ExportFiles[0]);

			BizObj.AccountingVouchers = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.TrialBalance = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have TrialBalance", "TrialBalance", BizObj.ExportFiles[0]);

			BizObj.TrialBalance = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.BalanceSheet = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have BalanceSheet", "BalanceSheet", BizObj.ExportFiles[0]);

			BizObj.BalanceSheet = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.ProfitAndLoss = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have ProfitAndLoss", "ProfitAndLoss", BizObj.ExportFiles[0]);

			BizObj.ProfitAndLoss = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.ChartOfAccounts = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have ChartOfAccounts", "ChartOfAccounts", BizObj.ExportFiles[0]);

			BizObj.ChartOfAccounts = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.VATDetailed = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have VATDetailed", "VATDetailed", BizObj.ExportFiles[0]);

			BizObj.VATDetailed = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.AssetProvision = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have AssetProvision", "AssetProvision", BizObj.ExportFiles[0]);

			BizObj.AssetProvision = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.PNLAppropriation = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have PNLAppropriation", "PNLAppropriation", BizObj.ExportFiles[0]);

			BizObj.PNLAppropriation = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.EquityMovement = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have EquityMovement", "EquityMovement", BizObj.ExportFiles[0]);

			BizObj.EquityMovement = false;
			AssertPropertyInfoHasErrors();
			AssertEquals("ExportFiles should be remove to empty.", 0, BizObj.ExportFiles.Count);
			BizObj.CashFlowStatement = true;
			AssertPropertyInfoNoErrors();
			AssertEquals("ExportFiles should be have CashFlowStatement", "CashFlowStatement", BizObj.ExportFiles[0]);
		}

		public void TestValidatePeriod()
		{
			BizObj.AccountingVouchers = true;
			AssertHasErrors("The 'Period' should have data.", BizObj.PeriodInfo);

			BizObj.AccountingVouchers = false;
			AssertNoErrors(BizObj.PeriodInfo);

			BizObj.TrialBalance = true;
			AssertHasErrors("The 'Period' should have data.", BizObj.PeriodInfo);

			BizObj.TrialBalance = false;
			AssertNoErrors(BizObj.PeriodInfo);

			BizObj.BalanceSheet = true;
			AssertHasErrors("The 'Period' should have data.", BizObj.PeriodInfo);

			BizObj.BalanceSheet = false;
			AssertNoErrors(BizObj.PeriodInfo);

			BizObj.ProfitAndLoss = true;
			AssertHasErrors("The 'Period' should have data.", BizObj.PeriodInfo);

			BizObj.ProfitAndLoss = false;
			AssertNoErrors(BizObj.PeriodInfo);

			BizObj.VATDetailed = true;
			AssertHasErrors("The 'Period' should have data.", BizObj.PeriodInfo);

			BizObj.VATDetailed = false;
			AssertNoErrors(BizObj.PeriodInfo);

			BizObj.AssetProvision = true;
			AssertHasErrors("The 'Period' should have data.", BizObj.PeriodInfo);

			BizObj.AssetProvision = false;
			AssertNoErrors(BizObj.PeriodInfo);

			BizObj.PNLAppropriation = true;
			AssertHasErrors("The 'Period' should have data.", BizObj.PeriodInfo);

			BizObj.PNLAppropriation = false;
			AssertNoErrors(BizObj.PeriodInfo);

			BizObj.EquityMovement = true;
			AssertHasErrors("The 'Period' should have data.", BizObj.PeriodInfo);

			BizObj.EquityMovement = false;
			AssertNoErrors(BizObj.PeriodInfo);

			BizObj.CashFlowStatement = true;
			AssertHasErrors("The 'Period' should have data.", BizObj.PeriodInfo);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			new AccountingPeriodTestHelper().SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1),
																new ZDateTime(2006, 3, 31, 23, 59, 00));
			BizObj.Period = 200603;
			AssertHasErrors(@"This Period '200603' is not closed. Please close both sub ledger and general ledger for this period before running the export function.",
				BizObj.PeriodInfo);

			BizObj.Period = 2010101;
			AssertHasErrors(@"There is no period set up for 201010.
Please go to General Ledger >> Period Management to setup periods.", BizObj.PeriodInfo);
		}

		void AssertPropertyInfoNoErrors()
		{
			AssertNoErrors(BizObj.AccountBookInfo);
			AssertNoErrors(BizObj.SupplementaryAccountsInfo);
			AssertNoErrors(BizObj.AccountingVouchersInfo);
			AssertNoErrors(BizObj.TrialBalanceInfo);
			AssertNoErrors(BizObj.BalanceSheetInfo);
			AssertNoErrors(BizObj.ProfitAndLossInfo);
			AssertNoErrors(BizObj.ChartOfAccountsInfo);
			AssertNoErrors(BizObj.VATDetailedInfo);
			AssertNoErrors(BizObj.AssetProvisionInfo);
			AssertNoErrors(BizObj.PNLAppropriationInfo);
			AssertNoErrors(BizObj.EquityMovementInfo);
			AssertNoErrors(BizObj.CashFlowStatementInfo);
		}

		void AssertPropertyInfoHasErrors()
		{
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.AccountBookInfo);
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.SupplementaryAccountsInfo);
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.AccountingVouchersInfo);
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.TrialBalanceInfo);
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.BalanceSheetInfo);
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.ProfitAndLossInfo);
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.VATDetailedInfo);
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.AssetProvisionInfo);
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.PNLAppropriationInfo);
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.EquityMovementInfo);
			AssertHasErrors("At least one of the 'Export Files' should have checked.", BizObj.CashFlowStatementInfo);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChinaStandard2004DataInterfaceWrapper(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BizObj = new ChinaStandard2004DataInterfaceWrapper(Factory);
			BizObj.RunPreSaveValidation();
		}

		new ChinaStandard2004DataInterfaceWrapper BizObj;
		#endregion
	}
}
