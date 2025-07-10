using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module.Testing
{
	abstract class ContraControllerTest : AccountingTransactionControllerTest
	{
		public void TestContraLoadingFilter()
		{
			ARRow = Factory.NewWithValidTestData<ARContraRow>();
			ARRow.AH_TransactionNum = "00000002";
			ARRow.AH_GB = GlbBranch.CurrentBranch.PK;
			ARRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();

			APRow = Factory.NewWithValidTestData<APContraRow>();
			APRow.AH_TransactionNum = "00000002";
			APRow.AH_GB = GlbBranch.CurrentBranch.PK;
			APRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			Factory.Save();

			AssertNull("Rows are from different branches so there is no TopLevel Contra", ((ContraController)Controller).GetTopLevelBusinessObject_ForTestOnly(ARRow));
		}

		public void TestContraControllerLedgerAfterLoading()
		{
			AssertEquals(LedgerTypes.AccountsReceivable, (((ContraController)Controller).GetTopLevelBusinessObject_ForTestOnly(ARRow) as Contra).ControllerLedger);
			AssertEquals(LedgerTypes.AccountsPayable, (((ContraController)Controller).GetTopLevelBusinessObject_ForTestOnly(APRow) as Contra).ControllerLedger);
		}

		public void TestContraLoadingFilterForTopLevelExists()
		{
			ARRow = Factory.NewWithValidTestData<ARContraRow>();
			ARRow.AH_TransactionNum = "00000002";
			ARRow.AH_GB = GlbBranch.CurrentBranch.PK;
			ARRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();

			APContraRow aPRowCurrentCompany = Factory.NewWithValidTestData<APContraRow>();
			aPRowCurrentCompany.AH_TransactionNum = "00000002";
			aPRowCurrentCompany.AH_GB = GlbBranch.CurrentBranch.PK;
			aPRowCurrentCompany.AH_TransactionBelongsToGroup = ARRow.AH_TransactionBelongsToGroup;
			Factory.Save();

			AssertNotNull("There is a TopLevel Contra since AH_TransactionBelongsToGroup is the same", ((ContraController)Controller).GetTopLevelBusinessObject_ForTestOnly(aPRowCurrentCompany));
		}

		public void TestGetTopLevelBusinessObject()
		{
			var contra1 = Contra.New(Factory);
			contra1.IsReverseTransaction = true;

			var controller1 = new ARContraController();
			var contraAR = (Contra)controller1.GetTopLevelBusinessObject_ForTestOnly(contra1);
			AssertEquals(ZArchitecture.Core.LedgerTypes.AccountsReceivable, contraAR.ControllerLedger);

			var contra2 = Contra.New(Factory);
			var controller2 = new APContraController();
			var contraAP = (Contra)controller2.GetTopLevelBusinessObject_ForTestOnly(contra2);
			AssertEquals(ZArchitecture.Core.LedgerTypes.AccountsPayable, contraAP.ControllerLedger);

			var contra3 = (Contra)((ContraController)Controller).GetTopLevelBusinessObject_ForTestOnly(contra1);
			AssertEquals("The same contra should be returned", contra1.GetHashCode(), contra3.GetHashCode());
			AssertEquals("IsReverseTransaction", true, contra3.IsReverseTransaction);
		}

		protected override void SetupTransactionHeaderRows()
		{
			ARRow = Factory.New(typeof(ARContraRow)) as ARContraRow;
			ARRow.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			ARRow.AH_TransactionNum = "00000001";
			ARRow.AH_TransactionCount = 1;
			ARRow.AH_PostDate = (ZDateTime)Env.Time.CurrentLocalDate;
			ARRow.AH_InvoiceDate = (ZDateTime)Env.Time.CurrentLocalDate;
			ARRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();

			APRow = Factory.New(typeof(APContraRow)) as APContraRow;
			APRow.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			APRow.AH_TransactionNum = "00000001";
			APRow.AH_TransactionCount = 2;
			APRow.AH_PostDate = (ZDateTime)Env.Time.CurrentLocalDate;
			APRow.AH_InvoiceDate = (ZDateTime)Env.Time.CurrentLocalDate;
			APRow.AH_TransactionBelongsToGroup = ARRow.AH_TransactionBelongsToGroup;
			Factory.Save();
		}

		protected ARContraRow ARRow;
		protected APContraRow APRow;

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		protected override BusinessObject GetFormBusinessEntity()
		{
			return Contra.New(Factory);
		}
	}
}
