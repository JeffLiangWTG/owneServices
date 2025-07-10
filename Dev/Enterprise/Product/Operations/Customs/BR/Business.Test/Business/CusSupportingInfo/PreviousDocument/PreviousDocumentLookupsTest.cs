using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class PreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var previousDocument = invoiceLine.PreviousDocuments.AddNew();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			AssertType<ExportPreviousDocumentList>(previousDocument.Lookups.CodeList);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertType<ImportPreviousDocumentList>(previousDocument.Lookups.CodeList);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			AssertType<ImportSiscomexPreviousDocumentList>(previousDocument.Lookups.CodeList);
		}
	}
}
