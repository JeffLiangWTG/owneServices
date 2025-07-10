using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(ImportJobComInvoiceLineLookups))]
class ImportJobComInvoiceLineLookupsTest : JobComInvoiceLineLookupsAbstractTest<ImportJobComInvoiceLineLookups>
{
	protected override string MessageType => EU.Business.MessageTypeList.Codes.Import;

	protected override ImportJobComInvoiceLineLookups GetLookups() => new ImportJobComInvoiceLineLookups(invoiceLine);
}
