using System;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE224GuaranteeReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<IE224GuaranteeReferenceProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("Sending Action missing", () => new IE224GuaranteeReferenceProvider(null));
			});
		}

		public void TestGrn()
		{
			cusGuaranteeHeader.CPH_Number = "GRN234325";
			AssertEquals("GRN", "GRN234325", Provider.Grn);
		}

		public void TestCurrency()
		{
			AssertEquals("Currency", Core.Constants.CurrencyCodes.EuropeanUnion, Provider.Currency);
		}

		public void TestIssueDate()
		{
			CombineAssertions(() =>
			{
				cusGuaranteeHeader.CPH_StartDate = ZDate.Today;
				AssertEquals("Issue Date", ZDate.Today, Provider.IssueDate);

				cusGuaranteeHeader.CPH_StartDate = ZDate.Empty;
				AssertEquals("IssueDate should be MinValue from Empty", DateTime.MinValue, Provider.IssueDate);

				cusGuaranteeHeader.CPH_StartDate = new ZDate(DateTime.MinValue);
				AssertEquals("IssueDate should be MinValue from MinValue", DateTime.MinValue, Provider.IssueDate);
			});
		}

		public void TestAccessCode()
		{
			cusGuaranteeHeader.MainAccessCode = "9876";
			AssertEquals("Access Code", "9876", Provider.AccessCode);
		}

		public void TestTIRCarnet()
		{
			sendingAction.TIRCarnet = false;
			AssertEquals("TIRCarnet", false, Provider.TIRCarnet);
			sendingAction.TIRCarnet = true;
			AssertEquals("TIRCarnet", true, Provider.TIRCarnet);
		}

		public void TestExpiryDate()
		{
			CombineAssertions(() =>
			{
				cusGuaranteeHeader.CPH_EndDate = ZDate.Today.AddMonths(1);
				AssertEquals("Expiry Date", ZDate.Today.AddMonths(1), Provider.ExpiryDate);

				cusGuaranteeHeader.CPH_EndDate = ZDate.Empty;
				AssertEquals("ExpiryDate should be null from Empty", DateTime.MinValue, Provider.ExpiryDate);

				cusGuaranteeHeader.CPH_EndDate = new ZDate(DateTime.MinValue);
				AssertEquals("ExpiryDate should be null from MinValue", DateTime.MinValue, Provider.ExpiryDate);
			});
		}

		public void TestCopyGiven()
		{
			AssertEquals("Copy Given", true, Provider.CopyGiven);
		}

		public void TestVoucherAmount()
		{
			sendingAction.VoucherAmount = 12500m;
			AssertEquals("Voucher amount", 12500m, Provider.VoucherAmount);
		}

		protected override IE224GuaranteeReferenceProvider GetProvider() => new IE224GuaranteeReferenceProvider(sendingAction);

		protected override void SetUp()
		{
			base.SetUp();
			cusGuaranteeHeader = Factory.New<CusGuaranteeHeader>();
			sendingAction = new GuaranteeVoucherSoldSendingAction(cusGuaranteeHeader);
		}

		CusGuaranteeHeader cusGuaranteeHeader;
		GuaranteeVoucherSoldSendingAction sendingAction;
	}
}
