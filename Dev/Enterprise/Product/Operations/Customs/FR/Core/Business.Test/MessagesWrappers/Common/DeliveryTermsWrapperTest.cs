using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class DeliveryTermsWrapperTest : TestCaseWithFactory
	{
		public void TestIncotermPlace()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "A";

			var invoice = declaration.Invoices.AddNew();
			invoice.ZG_AgreedPlaceCode = "3";

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = cei.PK;
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			var wrapper = new DeliveryTermsWrapper(declaration.CustomsEntryHeaders[0].MergedLines[0]);
			AssertEquals("3", wrapper.IncotermPlace);
		}
	}
}
