using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(ImportJobComInvoiceLineValidation))]
class ImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest<ImportJobComInvoiceLineValidation>
{
	protected override string MessageType => MessageTypeList.Codes.Import;

	protected override ImportJobComInvoiceLineValidation GetValidation() => new ImportJobComInvoiceLineValidation(invoiceLine);

	public void TestCheckZG_CountryOfDestination()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_RL_NKOrigin = ZString.Empty;
		var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine.ZG_CountryOfDestination = ZString.Empty;
		AssertHasMessageError(invLine.ZG_CountryOfDestinationInfo, "Destination is required on either Invoice header OR Invoice Item level");
		declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Netherlands;
		invLine.ZG_CountryOfDestination = ZString.Empty;
		AssertNoMessageErrors(invLine.ZG_CountryOfDestinationInfo);
		declaration.JE_GoodsDestination = ZString.Empty;
		invLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.Netherlands;
		AssertNoMessageError(invLine.ZG_CountryOfDestinationInfo, "Destination is required on either Invoice header OR Invoice Item level");
	}
}
