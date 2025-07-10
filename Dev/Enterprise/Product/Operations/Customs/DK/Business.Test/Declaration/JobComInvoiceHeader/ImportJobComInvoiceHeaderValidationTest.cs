using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DK.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobComInvoiceHeaderValidation))]
	sealed class ImportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationAbstractTest<ImportJobComInvoiceHeaderValidation>
	{
		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override ImportJobComInvoiceHeaderValidation GetValidation() => new ImportJobComInvoiceHeaderValidation(invoice);
	}
}
