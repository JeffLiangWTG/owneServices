using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class AddInfoCusLineTariffDetailLookupsTest : EU.Business.Testing.AddInfoCusLineTariffDetailLookupsTest
	{
		public void TestPaymentMethodList()
		{
			var cuslineTariffDetail2 = invoiceLine.CusLineTariffDetails.AddNew();
			var list = lookups.PaymentMethodList;

			AssertSame("PaymentMethodList", Factory.GetCachedValue<PaymentMethodList>(), list);
			AssertSame("List is cached", list, cuslineTariffDetail2.AddInfoLookups.PaymentMethodList);
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			var cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			lookups = cusLineTariffDetail.AddInfoLookups;
		}
		JobComInvoiceLine invoiceLine;
		AddInfoCusLineTariffDetailLookups lookups;
	}
}
