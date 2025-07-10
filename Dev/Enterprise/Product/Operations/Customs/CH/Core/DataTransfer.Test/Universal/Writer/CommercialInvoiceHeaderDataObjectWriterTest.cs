using Enterprise.Customs.CH.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CH.DataTransfer.Testing;

sealed class CommercialInvoiceHeaderDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
{
	public void TestVehiclesOnInvoiceLine()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;
		var vehicle1 = invoiceLine.Vehicles.AddNew();
		var vehicle2 = invoiceLine.Vehicles.AddNew();

		vehicle1.CVH_VehicleIdentificationNumber = "VIN1";
		vehicle1.CVH_RegistrationNumber = "REGNUM1";
		vehicle1.CVH_ModelName = "ML1";

		vehicle2.CVH_VehicleIdentificationNumber = "VIN2";
		vehicle2.CVH_RegistrationNumber = "REGNUM2";
		vehicle2.CVH_ModelName = "ML2";

		var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
		var result = writer.GetDataObject(invoice);
		var invoiceLineData = result.CommercialInvoiceLineCollection[0];
		CombineAssertions(() =>
		{
			AssertEquals("result.CommercialInvoiceLineCollection.Count", 1, result.CommercialInvoiceLineCollection.Count);
			AssertEquals(2, invoiceLineData.VehicleCollection.Count);
			var vehicle1 = invoiceLineData.VehicleCollection[0];
			AssertEquals("Vehicle 1, VIN", "VIN1", vehicle1.VIN);
			AssertEquals("Vehicle 1, RegistrationNumber", "REGNUM1", vehicle1.RegistrationNumber);
			AssertEquals("Vehicle 1, Model", "ML1", vehicle1.Model);
			AssertEquals("Vehicle 2, VIN", "VIN2", invoiceLineData.VehicleCollection[1].VIN);
		});
	}
}
