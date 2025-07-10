using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class LineDeliveryTermsProviderTest : Customs.Business.Testing.DataProviderTestCase<LineDeliveryTermsProvider>
	{
		public void TestNew()
		{
			AssertNull(LineDeliveryTermsProvider.NewOrNull(null));
		}

		public void TestIncotermCode()
		{
			AssertEquals("Correct incoterm", "FOB", dataProvider.IncotermCode);
		}

		public void TestLocation()
		{
			AssertEquals("Correct incoterm place", "PLACE", dataProvider.Location);
		}

		public void TestUNLocode()
		{
			AssertNullOrEmpty(dataProvider.UNLocode);
		}

		public void TestCountry()
		{
			AssertNullOrEmpty(dataProvider.Country);
		}

		public void TestText()
		{
			AssertNullOrEmpty(dataProvider.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_IncoTermPlace = "PLACE";
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			dataProvider = LineDeliveryTermsProvider.NewOrNull(invoiceLine);
		}
		IDeliveryTerms dataProvider;

		protected override LineDeliveryTermsProvider GetProvider() => (LineDeliveryTermsProvider)dataProvider;
	}
}
