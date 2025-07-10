using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(ImportJobComInvoiceHeaderValidation))]
class ImportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationAbstractTest<ImportJobComInvoiceHeaderValidation>
{
	protected override string MessageType => MessageTypeList.Codes.Import;

	protected override ImportJobComInvoiceHeaderValidation GetValidation() => new ImportJobComInvoiceHeaderValidation(invoice);
}
