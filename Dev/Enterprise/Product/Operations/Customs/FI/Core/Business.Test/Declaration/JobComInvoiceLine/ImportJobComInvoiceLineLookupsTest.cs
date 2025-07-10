using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Business.Testing;

[TestedType(typeof(ImportJobComInvoiceLineLookups))]
class ImportJobComInvoiceLineLookupsTest : JobComInvoiceLineLookupsAbstractTest<ImportJobComInvoiceLineLookups>
{
	protected override string MessageType => MessageTypeList.Codes.Import;

	protected override ImportJobComInvoiceLineLookups GetLookups() => new ImportJobComInvoiceLineLookups(invoiceLine);
}
