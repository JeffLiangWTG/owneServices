using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Business.Testing;

[TestedType(typeof(ImportJobComInvoiceLineValidation))]
sealed class ImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest<ImportJobComInvoiceLineValidation>
{
	protected override string MessageType => MessageTypeList.Codes.Import;

	protected override ImportJobComInvoiceLineValidation GetValidation() => new ImportJobComInvoiceLineValidation(invoiceLine);
}
