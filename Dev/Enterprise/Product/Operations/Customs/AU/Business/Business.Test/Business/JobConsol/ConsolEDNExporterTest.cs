using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class ConsolEDNExporterTest : TestCaseWithFactory
	{
		public void TestExporterOnlyAppliesToAUExportDeclaration()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);

			var auExportCompany = Factory.NewWithValidTestData<GlbCompany>();
			auExportCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var auExportBranch = Factory.NewWithValidTestData<GlbBranch>();
			auExportBranch.GB_GC = auExportCompany.PK;

			var auExportJob = Factory.New<BaseJobDeclaration>();
			auExportJob.JE_MessageType = JobMessageTypeList.Codes.Export;
			auExportJob.JE_GB = auExportBranch.PK;
			var auExportInvoice = auExportJob.Invoices.AddNew();
			auExportInvoice.InvoiceLines.AddNew();

			var auImportCompany = Factory.NewWithValidTestData<GlbCompany>();
			auImportCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var auImportBranch = Factory.NewWithValidTestData<GlbBranch>();
			auImportBranch.GB_GC = auImportCompany.PK;

			var auImportJob = Factory.New<BaseJobDeclaration>();
			auImportJob.JE_MessageType = JobMessageTypeList.Codes.Import;
			auImportJob.JE_GB = auImportBranch.PK;
			var auImportInvoice = auImportJob.Invoices.AddNew();
			auImportInvoice.InvoiceLines.AddNew();

			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);

			var usBranch = Factory.NewWithValidTestData<GlbBranch>();
			usBranch.GB_GC = usCompany.PK;

			var usExportJob = Factory.New<BaseJobDeclaration>();
			usExportJob.JE_MessageType = JobMessageTypeList.Codes.Export;
			usExportJob.JE_GB = usBranch.PK;
			var usExportInvoice = auImportJob.Invoices.AddNew();
			usExportInvoice.InvoiceLines.AddNew();

			Factory.Save();

			var exporter = new ConsolEDNExporter(consol);

			usExportJob.JE_JS = shipment.PK;
			auImportJob.JE_JS = shipment.PK;
			var generatedData = exporter.Generate();
			AssertEquals("No Au Export Job to export", ZString.Empty, generatedData.Content);

			auExportJob.JE_JS = shipment.PK;

			generatedData = exporter.Generate();
			AssertNotEquals("Au Export Job to export", ZString.Empty, generatedData.Content);
		}

		public void TestPopulateSupplierCIDCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);

			var supplier = Factory.New<OrgHeader>();
			var cusCode = supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "123456");
			var auExportJob = Factory.New<JobDeclaration>();
			auExportJob.JE_MessageType = JobMessageTypeList.Codes.Export;
			auExportJob.JE_JS = shipment.PK;
			auExportJob.JE_OH_Supplier = supplier.PK;
			var auExportInvoice = auExportJob.Invoices.AddNew();
			auExportInvoice.InvoiceLines.AddNew();

			var exporter = new ConsolEDNExporter(consol);
			AssertContains(",123456,", exporter.Generate().Content);

			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			AssertNotContains(",123456,", exporter.Generate().Content);
		}
	}
}
