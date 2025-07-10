using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;

	internal class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			previousDocument = invoiceLine.PreviousDocuments.AddNew();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var collection = (CodeDescriptionPairList)previousDocument.Lookups.CodeList;
			AssertEquals(42, collection.Count);
			AssertEquals("AAD", collection[0].Code);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			previousDocument = invoiceLine.PreviousDocuments.AddNew();
		}

		PreviousDocument previousDocument;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;

		#endregion
	}
}
