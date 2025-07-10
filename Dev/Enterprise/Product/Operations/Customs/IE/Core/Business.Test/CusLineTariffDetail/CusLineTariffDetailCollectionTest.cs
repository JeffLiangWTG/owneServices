using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetailCollection))]
	sealed class CusLineTariffDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			return (CusLineTariffDetailCollection)invoiceLine.CusLineTariffDetails;
		}

		public void TestSetDefaultsForNewChild()
		{
			var cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			AssertEquals("Default Payment Method", "A", cusLineTariffDetail.ZG_MethodOfPayment);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaymentMethod = "A";
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}
		JobComInvoiceLine invoiceLine;
	}
}
