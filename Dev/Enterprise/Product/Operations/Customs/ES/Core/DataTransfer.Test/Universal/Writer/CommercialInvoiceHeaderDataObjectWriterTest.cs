using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.ES.DataTransfer.Universal.Testing
{
	class CommercialInvoiceHeaderDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateInvoiceLineJI_StateOrRegionOfOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "501999";

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "S222101575080";

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;
			invoiceLine.JI_StateOrRegionOfOrigin = "28";

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			var invoiceData = writer.GetDataObject(invoice);

			AssertEquals("invoiceData.CommercialInvoiceLineCollection.Count", 1, invoiceData.CommercialInvoiceLineCollection.Count);

			var invoiceLineData = invoiceData.CommercialInvoiceLineCollection.FirstOrDefault(x => x.PartNo.GetValueOrDefault() == "S222101575080");
			AssertEquals("invoiceLineData.CountryOfOrigin.Code", "ES", invoiceLineData.CountryOfOrigin.Code);
			AssertEquals("invoiceLineData.AddInfoCollection.ProvinceOfOrigin", "28", invoiceLineData.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.InvoiceLine.ProvinceOfOrigin).Value);
		}

		public void TestVehiclesOnInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;
			var vehicle1 = invoiceLine.Vehicles.AddNew();
			var vehicle2 = invoiceLine.Vehicles.AddNew();

			vehicle1.CVH_VehicleIdentificationNumber = "VIN1";
			vehicle1.CVH_BrandName = "BRAND1";
			vehicle1.CVH_ModelName = "MODEL1";

			vehicle2.CVH_VehicleIdentificationNumber = "VIN2";
			vehicle2.CVH_BrandName = "BRAND2";
			vehicle2.CVH_ModelName = "MODEL2";

			var writer = new CommercialInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, invoice)), new UniversalDataObjectWriterHelper(declaration.Factory, declaration.CountryCode));
			var result = writer.GetDataObject(invoice);
			var invoiceLineData = result.CommercialInvoiceLineCollection[0];
			CombineAssertions(() =>
			{
				AssertEquals("result.CommercialInvoiceLineCollection.Count", 1, result.CommercialInvoiceLineCollection.Count);
				AssertEquals(2, invoiceLineData.VehicleCollection.Count);
				var vehicle1 = invoiceLineData.VehicleCollection[0];
				AssertEquals("Vehicle 1, VIN", "VIN1", vehicle1.VIN);
				AssertEquals("Vehicle 1, Brand", "BRAND1", vehicle1.Brand);
				AssertEquals("Vehicle 1, Model", "MODEL1", vehicle1.Model);
				AssertEquals("Vehicle 2, VIN", "VIN2", invoiceLineData.VehicleCollection[1].VIN);
			});
		}
	}
}
