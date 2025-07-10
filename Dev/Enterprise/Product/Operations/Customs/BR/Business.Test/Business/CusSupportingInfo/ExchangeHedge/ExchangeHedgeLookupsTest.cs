using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ExchangeHedgeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestExchangeHedgeTypeList()
		{
			var exchangeHedgeTypeList = oInvoice.ExchangeHedge.Lookups.ExchangeHedgeList;
			CombineAssertions(() =>
			{
				AssertEquals(4, exchangeHedgeTypeList.Count);
				AssertEquals("1, 2, 3, 4", exchangeHedgeTypeList.CodesAsString);
			});
		}

		public void TestFinancialInstitutionList()
		{
			ReferenceTestDataHelper.CreateRefCusCodeListMISCC_BNKTestData(Factory);

			var financialInstitutionList = oInvoice.ExchangeHedge.Lookups.FinancialInstitutionList;
			CombineAssertions(() =>
			{
				AssertEquals(4, financialInstitutionList.Count);
				AssertEquals("1, 3, 4, 99",
					financialInstitutionList.CodesAsString);
			});
		}

		public void TestReasonTypeList()
		{
			ReferenceTestDataHelper.CreateRefCusCodeListMISCC_BNKTestData(Factory);

			var reasonTypeList = oInvoice.ExchangeHedge.Lookups.ReasonTypeList;
			CombineAssertions(() =>
			{
				AssertEquals(3, reasonTypeList.Count);
				AssertEquals("30, 31, 99",
					reasonTypeList.CodesAsString);
			});
		}

		public void TestPaymentMethodList()
		{
			ReferenceTestDataHelper.CreateRefCusCodeListMISCC_BNKTestData(Factory);

			var paymentMethodList = oInvoice.ExchangeHedge.Lookups.PaymentMethodList;
			CombineAssertions(() =>
			{
				AssertEquals(3, paymentMethodList.Count);
				AssertEquals("60, 70, 99",
					paymentMethodList.CodesAsString);
			});
		}

		JobDeclaration oJobDeclaration;
		JobComInvoiceHeader oInvoice;

		protected override void SetUp()
		{
			base.SetUp();
			oJobDeclaration = Factory.New<JobDeclaration>();
			oInvoice = oJobDeclaration.Invoices.AddNew();
		}
	}
}
