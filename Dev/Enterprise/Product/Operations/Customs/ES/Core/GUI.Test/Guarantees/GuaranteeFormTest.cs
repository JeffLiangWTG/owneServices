using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusGuaranteeLineTransactionCollection = Enterprise.Customs.ES.Business.CusGuaranteeLineTransactionCollection;

namespace Enterprise.Customs.ES.GUI.Test
{
	[TestedType(typeof(GuaranteeForm))]
	class GuaranteeFormTest : ZFormBasherTest
	{
		public void TestShowTransactionsWithPendingDebtMenuItem()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();

			using (var form = new GuaranteeForm(guaranteeHeader, new EU.GUI.GuaranteeTransactionFilterStripBusinessObject()))
			{
				form.Show();
				Application.DoEvents();

				CombineAssertions(() =>
				{
					var actionsMenuItem = form.Menu.MenuItems.FindByText("&Actions");
					AssertNotNull("actionsMenuItem", actionsMenuItem);

					var showTransactionsMenuItem = actionsMenuItem.MenuItems.FindByText("Show Transactions With Pending Debt");
					AssertNotNull("showTransactionsMenuItem", showTransactionsMenuItem);
				});
			}
		}

		public void TestShowTransactionsWithPendingDebtMenuItemFuntionality()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();

			var transaction1 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", "ADJ", "ref1", 1);
			var transaction2 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", "ADJ", "ref2", -2);
			var transaction3 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", "ADJ", "ref2", 2);
			var transaction4 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", "ADJ", "ref3", 3);
			var transaction5 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", "ADJ", "ref3", -5);
			var transaction6 = AddCusGuaranteeLineTransaction(guaranteeHeader, "DEL", "ADJ", "ref4", 3);
			var transaction7 = AddCusGuaranteeLineTransaction(guaranteeHeader, "DEL", "ADJ", "ref4", -5);
			var transaction8 = AddCusGuaranteeLineTransaction(guaranteeHeader, "PND", "TRA", "ref5", 3);
			var transaction9 = AddCusGuaranteeLineTransaction(guaranteeHeader, "PND", "TRA", "ref5", -5);
			var transaction10 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", "OBA", "ref6", 3);
			var transaction11 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", "OBA", "ref6", -5);
			var transaction12 = AddCusGuaranteeLineTransaction(guaranteeHeader, "CON", "CUS", "ref7", -10);

			Factory.Save();

			using (var form = new GuaranteeFormForTest(guaranteeHeader, new EU.GUI.GuaranteeTransactionFilterStripBusinessObject()))
			{
				form.Show();
				Application.DoEvents();

				var actionsMenuItem = form.Menu.MenuItems.FindByText("&Actions");
				var showTransactionsMenuItem = actionsMenuItem.MenuItems.FindByText("Show Transactions With Pending Debt");

				ZFormModaliser.ShowDialogsInTest = true;

				CombineAssertions(() =>
				{
					ZFormModaliser.SetDelegateToCallOnFormShown(AssertType<ShowPendingDebtTransactionsForm>);
					showTransactionsMenuItem.PerformClick();

					var resultList = form.TransactionCollection;
					AssertEquals("ResultList contains 3 elements", 5, resultList.Count);
					AssertEquals("Transaction 1 is not in the list (no debt)", false, resultList.Contains(transaction1));

					AssertEquals("Transaction 2 is not in the list (no debt)", false, resultList.Contains(transaction2));
					AssertEquals("Transaction 3 is not in the list (no debt)", false, resultList.Contains(transaction3));

					AssertEquals("Transaction 4 is in the list when (with debt and correct status and type)", true, resultList.Contains(transaction4));
					AssertEquals("Transaction 5 is in the list when (with debt and correct status and type)", true, resultList.Contains(transaction5));

					AssertEquals("Transaction 6 is not in the list when (with debt and correct type but wrong status)", false, resultList.Contains(transaction6));
					AssertEquals("Transaction 7 is not in the list when (with debt and correct type but wrong status)", false, resultList.Contains(transaction7));

					AssertEquals("Transaction 8 is in the list when (with debt and correct status and type)", true, resultList.Contains(transaction8));
					AssertEquals("Transaction 9 is in the list when (with debt and correct status and type)", true, resultList.Contains(transaction9));

					AssertEquals("Transaction 10 is not in the list when (with debt and correct status but wrong type)", false, resultList.Contains(transaction10));
					AssertEquals("Transaction 11 is not in the list when (with debt and correct status but wrong type)", false, resultList.Contains(transaction11));

					AssertEquals("Transaction 12 is in the list when (with debt and correct status and type)", true, resultList.Contains(transaction12));
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new GuaranteeForm(GetGuaranteeHeader(), new EU.GUI.GuaranteeTransactionFilterStripBusinessObject());
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		CusGuaranteeHeader GetGuaranteeHeader()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
			helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ", "@#$_Basher_Test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Type = "ZZZ";
			guaranteeHeader.CPH_StartDate = ZDate.Today;
			Factory.Save();
			return guaranteeHeader;
		}

		CusGuaranteeLineTransaction AddCusGuaranteeLineTransaction(CusGuaranteeHeader guarantee, string transactionStatus, string transactionType, string reference , decimal tranValue)
		{
			var transaction = guarantee.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_TransactionType = transactionType;
			transaction.CPL_TransactionStatus = transactionStatus;
			transaction.CPL_Reference = reference;
			transaction.CPL_TranValue = tranValue;
			return transaction;
		}

		class GuaranteeFormForTest : GuaranteeForm
		{
			public GuaranteeFormForTest(CusGuaranteeHeader guaranteeHeader, EU.GUI.GuaranteeTransactionFilterStripBusinessObject filterStripBusinessObject) : base(guaranteeHeader, filterStripBusinessObject)
			{
			}

			public CusGuaranteeLineTransactionCollection TransactionCollection;

			protected override void ShowTransactionsWithPendingDebtForm(CusGuaranteeLineTransactionCollection transactionCollection) => TransactionCollection = transactionCollection;
		}
	}
}
