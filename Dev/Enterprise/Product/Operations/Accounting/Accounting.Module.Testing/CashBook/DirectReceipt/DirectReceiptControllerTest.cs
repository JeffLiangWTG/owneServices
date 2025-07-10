using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(DirectReceiptController))]
	class DirectReceiptControllerTest : AccountingTransactionControllerTest
	{
		public void TestModuleID()
		{
			var controller = new DirectReceiptController();
			AssertEquals("ModuleID of DirectReceiptController is CashbookTransaction", ModuleIDs.CashbookTransaction, controller.ModuleID);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DirectReceipt;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return DirectReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseCashBookDirectReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.ViewCashBookDirectReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewCashBookDirectReceipt; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ViewCashBookDirectReceipt; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			DirectReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			Factory.Save();
		}

		public void TestCopy()
		{
			//Arrange
			var testDirectReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			testDirectReceipt.AH_InvoiceDate = ZDateTime.Now.AddDays(-2);
			testDirectReceipt.AH_PostDate = ZDateTime.Now.AddDays(-1);
			testDirectReceipt.AH_AB = TestObjectCreator.USDBankAccount.PK;
			testDirectReceipt.AH_ReceiptType = ReceiptTypes.DirectCredit;
			testDirectReceipt.AH_ExchangeRate = 0.8530m;
			testDirectReceipt.AH_Desc = "Cash book direct receipt test";

			var testLine = testDirectReceipt.Lines.AddNew();
			testLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			testLine.AL_GB = TestObjectCreator.DefaultBranchPK;
			testLine.AL_GE = TestObjectCreator.FIADepartment.PK;
			testLine.AL_Desc = "Test Line";
			testLine.AL_OSExTaxAmount = 1000m;
			testLine.AL_AT = TestObjectCreator.CC1.GSTRate.PK;
			testLine.AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;
			testLine.AL_OSTaxAmount = 110m;
			Factory.Save();

			//set system exchange rate
			var sysExRateValue = 0.8130m;
			RefExchangeRate sellExRate = new BusinessObjectFactory().NewWithValidTestData<RefExchangeRate>();
			sellExRate.RE_GC = GlbCompany.CurrentCompany.PK;
			sellExRate.RE_StartDate = ZDateTime.Today;
			sellExRate.RE_ExpiryDate = ZDateTime.Today;
			sellExRate.RE_SellRate = sysExRateValue;
			sellExRate.RE_RX_NKExCurrency = TestObjectCreator.USDBankAccount.AccountCurrency.Code;
			sellExRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellExRate.Factory.Save();

			//Act
			using (IZForm form = ((DirectReceiptController)Controller).ShowTemplateCopyForm(testDirectReceipt))
			{
				//Assert
				AssertNotNull("Form should not be null", form);
				AssertEquals("Form's Display mode should be New", ODisplayMode.New, form.DisplayMode);

				var copy = form.BusinessEntityForPersistingForm as DirectReceipt;
				AssertZDatesWithin5Minutes("Receipt Date should be today", ZDateTime.Now, copy.AH_InvoiceDate);
				AssertZDatesWithin5Minutes("Post Date should be today", ZDateTime.Now, copy.AH_PostDate);
				AssertEquals(testDirectReceipt.AH_AB, copy.AH_AB);
				AssertEquals(testDirectReceipt.AH_ReceiptType, copy.AH_ReceiptType);
				AssertEquals(TestObjectCreator.USDBankAccount.AccountCurrency.Code, copy.ExchangeRate.Currency);
				AssertEquals(sysExRateValue, copy.ExchangeRate.Rate);
				AssertEquals(testDirectReceipt.AH_Desc, copy.AH_Desc);
				AssertEquals(testDirectReceipt.AH_ChequeDrawer, copy.AH_ChequeDrawer);

				//Assert lines are copied
				AssertEquals(copy.Lines.Count, testDirectReceipt.Lines.Count);
				for (int i = 0; i < testDirectReceipt.Lines.Count; i++)
				{
					var line = testDirectReceipt.Lines[i];
					var lineCopy = copy.Lines[i];

					AssertEquals(line.AL_AG, lineCopy.AL_AG);
					AssertEquals(line.AL_GB, lineCopy.AL_GB);
					AssertEquals(line.AL_GE, lineCopy.AL_GE);
					AssertEquals(line.AL_Desc, lineCopy.AL_Desc);
					AssertEquals(line.AL_OSExTaxAmount, lineCopy.AL_OSExTaxAmount);
					AssertEquals(line.AL_AT, lineCopy.AL_AT);
					AssertEquals(line.AL_A9_VATClass, lineCopy.AL_A9_VATClass);
					AssertEquals(line.AL_OSTaxAmount, lineCopy.AL_OSTaxAmount);
					AssertEquals("Local amount should be automatically calculated based on current exchange rate", Math.Round(line.AL_OSExTaxAmount / sysExRateValue, 2), lineCopy.AL_LocalExTaxAmount);
					AssertZDatesWithin5Minutes("Line TaxDate should be today", ZDate.Today, lineCopy.AL_TaxDate);
				}
			}
		}

		#region Implementation

		DirectReceipt DirectReceipt;

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

		#endregion

		TestObjectCreator fTestObjectCreator;
	}
}
