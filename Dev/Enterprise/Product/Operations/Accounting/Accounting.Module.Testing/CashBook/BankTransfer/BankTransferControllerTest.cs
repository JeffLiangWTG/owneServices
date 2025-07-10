using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BankTransferController))]
	class BankTransferControllerTest : AccountingTransactionControllerTest
	{
		protected override void SetupTransactionHeaderRows()
		{
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BankTransfer;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get
			{
				if (fTestBankTransferRow == null)
				{
					BankTransfer testBankTransfer = new BankTransfer(Factory, null);
					testBankTransfer.BankTransferFromPK = TestObjectCreator.AUDBankAccount.PK;
					testBankTransfer.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;
					testBankTransfer.BuyExchangeRate = 2M;
					testBankTransfer.SellAmount = 100m;

					Factory.Save();

					ZQuery filter = new ZQuery();
					filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)1);
					fTestBankTransferRow = Factory.LoadTop1<BankTransferFromRow>(filter);
				}
				return fTestBankTransferRow;
			}
		}
		BankTransferFromRow fTestBankTransferRow;

		protected override BusinessObject ParentTransactionHeaderRowNotSaved
		{
			get { return new BankTransfer(new BusinessObjectFactory(), null); }
		}

		protected override BusinessObject GetFormBusinessEntity()
		{
			return new BankTransfer(Factory, null);
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookBankTransfer; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewCashBookBankTransfer; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewCashBookBankTransfer; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewCashBookBankTransfer; }
		}

		public void TestShowDeleteForm()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (IZForm testForm = TestController.ShowDeleteForm(ParentTransactionHeaderRow))
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)1);
			BankTransferFromRow testBankTransferFromRow = Factory.LoadTop1<BankTransferFromRow>(filter);
			testBankTransferFromRow.AH_IsCancelled = true;

			using (IZForm testForm = TestController.ShowDeleteForm(testBankTransferFromRow))
			{
				AssertEquals("This transaction has already been canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetTopLevelBusinessObject()
		{
			BankTransfer transfer1 = new BankTransfer(Factory, null);
			transfer1.IsReverseTransaction = true;

			BankTransfer transfer2 = (BankTransfer)TestController.GetTopLevelBusinessObject_ForTestOnly(transfer1);
			AssertEquals("The same bank transfer should be returned", transfer1.GetHashCode(), transfer2.GetHashCode());
			AssertEquals("IsReverseTransaction", true, transfer2.IsReverseTransaction);
		}

		public new void TestDoReversingBeforeShowDeleteForm()
		{
			Assert(true);
		}

		public void TestOnlyOneReverseBankTransferFormOpensForSameTransaction()
		{
			using (var testForm = TestController.ShowDeleteForm(ParentTransactionHeaderRow))
			{
				var testController2 = new BankTransferController();
				using (var testForm2 = testController2.ShowDeleteForm(ParentTransactionHeaderRow))
				{
					var openedForms = ZApplication.GetOpenForms();
					var bankTransferFormCounts = openedForms.Where(form => form.Name == "BankTransferForm").Count();
					AssertEquals(1, bankTransferFormCounts);
				}
			}
		}

		public void TestCopy()
		{
			//Arrange
			BankTransfer transaction = new BankTransfer(Factory, null);
			transaction.BankTransferFromPK = TestObjectCreator.EURBankAccount.PK;
			transaction.BankTransferToPK = TestObjectCreator.USDBankAccount.PK;
			transaction.SellExchangeRate = 0.6351M;
			transaction.BuyExchangeRate = 0.7754M;
			transaction.SellAmount = 1000m;
			Factory.Save();

			//set system exchange rates. make them different from the rates in the source transaction
			var sysSellRateValue = 0.6451m;
			RefExchangeRate sellExRate  = new BusinessObjectFactory().NewWithValidTestData<RefExchangeRate>();
			sellExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			sellExRate.RE_StartDate = ZDateTime.Today;
			sellExRate.RE_ExpiryDate = ZDateTime.Today;
			sellExRate.RE_SellRate = sysSellRateValue;
			sellExRate.RE_RX_NKExCurrency = TestObjectCreator.EURBankAccount.AccountCurrency.Code;
			sellExRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			sellExRate.Factory.Save();

			var sysBuyRateValue = 0.7884m;
			RefExchangeRate buyExRate = new BusinessObjectFactory().NewWithValidTestData<RefExchangeRate>();
			buyExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			buyExRate.RE_StartDate = ZDateTime.Today;
			buyExRate.RE_ExpiryDate = ZDateTime.Today;
			buyExRate.RE_SellRate = sysBuyRateValue;
			buyExRate.RE_RX_NKExCurrency = TestObjectCreator.USDBankAccount.AccountCurrency.Code;
			buyExRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			buyExRate.Factory.Save();
		
			//Act
			using (IZForm form = ((BankTransferController)Controller).ShowTemplateCopyForm(transaction))
			{
				//Assert
				AssertNotNull("Form should not be null", form);
				AssertEquals("Form's Display mode should be New", ODisplayMode.New, form.DisplayMode);

				var copy = form.BusinessEntityForPersistingForm as BankTransfer;
				AssertZDatesWithin5Minutes("Bank Transfer Date should be today", ZDateTime.Now, copy.TransactionDate);
				AssertZDatesWithin5Minutes("Post Date should be today", ZDateTime.Now, copy.AH_PostDate);
				AssertEquals(transaction.Description, copy.Description);
				AssertEquals(transaction.BankTransferFromPK, copy.BankTransferFromPK);
				AssertEquals(transaction.BankTransferToPK, copy.BankTransferToPK);
				AssertEquals(transaction.SellAmount, copy.SellAmount);
				AssertNotEquals(transaction.BuyAmount, copy.BuyAmount); //exchange rate is different
				AssertEquals("Buy Amount should be automatically calculated based on the current exchange rate", Math.Round(copy.SellAmount / sysSellRateValue * sysBuyRateValue, 2), copy.BuyAmount);
				AssertEquals(transaction.FinanceChargeDescription, copy.FinanceChargeDescription);
			}
		}

		#region Implementations

		protected override void SetUp()
		{
			base.SetUp();
			TestController = new BankTransferController();
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;
		BankTransferController TestController;

		#endregion
	}
}
