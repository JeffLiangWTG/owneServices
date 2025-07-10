using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(ImportJobComInvoiceHeaderLookups))]
class ImportJobComInvoiceHeaderLookupsTest : JobComInvoiceHeaderLookupsAbstractTest<ImportJobComInvoiceHeaderLookups>
{
	protected override string MessageType => MessageTypeList.Codes.Import;

	protected override ImportJobComInvoiceHeaderLookups GetLookups() => new ImportJobComInvoiceHeaderLookups(invoice);
}
