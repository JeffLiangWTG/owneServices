using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(ExportJobComInvoiceLineValidation))]
sealed class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationAbstractTest<ExportJobComInvoiceLineValidation>
{
	protected override string MessageType => MessageTypeList.Codes.Export;

	protected override ExportJobComInvoiceLineValidation GetValidation() => new ExportJobComInvoiceLineValidation(invoiceLine);

	public void TestCheckJI_RN_NKCountryOfExport()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_RL_NKOrigin = ZString.Empty;
		var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invLine.JI_RN_NKCountryOfExport = ZString.Empty;
		AssertHasMessageError(invLine.JI_RN_NKCountryOfExportInfo, "Export country is required on either Invoice header OR Invoice Item level");
		declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.Netherlands;
		invLine.JI_RN_NKCountryOfExport = ZString.Empty;
		AssertNoMessageErrors(invLine.JI_RN_NKCountryOfExportInfo);
		declaration.JE_RL_NKOrigin = ZString.Empty;
		invLine.JI_RN_NKCountryOfExport = Core.Constants.CountryCodes.Netherlands;
		AssertNoMessageErrors(invLine.JI_RN_NKCountryOfExportInfo);
	}

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
