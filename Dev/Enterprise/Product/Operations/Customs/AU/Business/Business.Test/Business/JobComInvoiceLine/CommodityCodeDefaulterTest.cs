using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CommodityCodeDefaulterTest : TestCaseWithFactory
	{
		public void TestDefaultInvoiceLineCommodityCode()
		{
			testDefaulter.DefaultInvoiceLineCommodityCode(line1, null);

			line1.JI_RH_NKCommodity_Code = commodityCode1.RH_Code;
			testDefaulter.DefaultInvoiceLineCommodityCode(line1, commodityCode2);
			AssertEquals("If there is a commodity code already, system does not override it", commodityCode1.RH_Code, line1.JI_RH_NKCommodity_Code);

			line1.JI_RH_NKCommodity_Code = ZString.Empty;
			testDefaulter.DefaultInvoiceLineCommodityCode(line1, commodityCode2);
			AssertEquals("CommodityCode2 is set", commodityCode2.RH_Code, line1.JI_RH_NKCommodity_Code);
		}

		public void TestIsJobExportAndSupplierHasExportCommodity()
		{
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("Job is import", false, testDefaulter.IsJobExportAndSupplierHasExportCommodity(supplier));

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Passed Supplier is null", false, testDefaulter.IsJobExportAndSupplierHasExportCommodity(null));

			supplier.MiscServ.OM_RH_NKCMMainExportCmdty = "";
			supplier.MiscServ.OM_RH_NKCMMainImportCmdty = commodityCode2.RH_Code;
			AssertEquals("This supplier does not have Export Cmdty Code", false, testDefaulter.IsJobExportAndSupplierHasExportCommodity(supplier));

			supplier.MiscServ.OM_RH_NKCMMainExportCmdty = commodityCode1.RH_Code;
			AssertEquals("This supplier has Export Cmmdty Code", true, testDefaulter.IsJobExportAndSupplierHasExportCommodity(supplier));
		}

		public void TestIsJobImportAndImporterHasImportCommodity()
		{
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("Job is Export", false, testDefaulter.IsJobImportAndImporterHasImportCommodity(importer));

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("Passed Supplier is null", false, testDefaulter.IsJobImportAndImporterHasImportCommodity(null));

			importer.MiscServ.OM_RH_NKCMMainExportCmdty = commodityCode3.RH_Code;
			importer.MiscServ.OM_RH_NKCMMainImportCmdty = "";
			AssertEquals("This importer does not have Import Cmdty Code", false, testDefaulter.IsJobImportAndImporterHasImportCommodity(importer));

			importer.MiscServ.OM_RH_NKCMMainImportCmdty = commodityCode4.RH_Code;
			AssertEquals("This Importer has Import Cmmdty Code", true, testDefaulter.IsJobImportAndImporterHasImportCommodity(importer));
		}

		public void TestAttemptDefaultFromSupplier()
		{
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoice.JZ_OH_Supplier = supplier.PK;

			//Setting Supplier or importer defaults Commodity Codes for Invoice Lines
			line1.JI_RH_NKCommodity_Code = ZString.Empty;
			line2.JI_RH_NKCommodity_Code = ZString.Empty;

			testDefaulter.AttemptDefaultFromSupplier(supplier, invoice.JobComInvoiceLines);
			AssertEquals("Job is Import and Commodity Code is not defaulted from supplier", ZString.Empty, line1.JI_RH_NKCommodity_Code);
			AssertEquals("Job is Import and Commodity Code is not defaulted from supplier", ZString.Empty, line2.JI_RH_NKCommodity_Code);

			testDefaulter.AttemptDefaultFromSupplier(supplier, line1);
			testDefaulter.AttemptDefaultFromSupplier(supplier, line2);
			AssertEquals("Job is Import and Commodity Code is not defaulted from supplier", ZString.Empty, line1.JI_RH_NKCommodity_Code);
			AssertEquals("Job is Import and Commodity Code is not defaulted from supplier", ZString.Empty, line2.JI_RH_NKCommodity_Code);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			testDefaulter.AttemptDefaultFromSupplier(supplier, line1);
			AssertEquals("Job is Export and Commodity Code defaults from Supplier", commodityCode1.RH_Code, line1.JI_RH_NKCommodity_Code);
			AssertEquals("Job is Export and Commodity Code defaults from Supplier", ZString.Empty, line2.JI_RH_NKCommodity_Code);

			testDefaulter.AttemptDefaultFromSupplier(supplier, invoice.JobComInvoiceLines);
			AssertEquals("Job is Export and Commodity Code defaults from Supplier", commodityCode1.RH_Code, line2.JI_RH_NKCommodity_Code);
		}

		public void TestAttemptDefaultFromImporter()
		{
			testDec.JE_OH_Importer = importer.PK;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoice.JZ_OH_Supplier = supplier.PK;

			//Setting Supplier or importer defaults Commodity Codes for Invoice Lines
			line1.JI_RH_NKCommodity_Code = ZString.Empty;
			line2.JI_RH_NKCommodity_Code = ZString.Empty;

			testDefaulter.AttemptDefaultFromImporter(importer, testDec.InvoiceLines);
			AssertEquals("Job is Export and Commodity Code is not defaulted from Importer", ZString.Empty, line1.JI_RH_NKCommodity_Code);
			AssertEquals("Job is Export and Commodity Code is not defaulted from Importer", ZString.Empty, line2.JI_RH_NKCommodity_Code);

			testDefaulter.AttemptDefaultFromImporter(importer, line1);
			testDefaulter.AttemptDefaultFromImporter(importer, line2);
			AssertEquals("Job is Export and Commodity Code is not defaulted from Importer", ZString.Empty, line1.JI_RH_NKCommodity_Code);
			AssertEquals("Job is Export and Commodity Code is not defaulted from Importer", ZString.Empty, line2.JI_RH_NKCommodity_Code);

			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDefaulter.AttemptDefaultFromImporter(importer, line1);
			AssertEquals("Job is Import and Commodity Code defaults from importer", commodityCode4.RH_Code, line1.JI_RH_NKCommodity_Code);
			AssertEquals("Job is Import and Commodity Code defaults from importer", ZString.Empty, line2.JI_RH_NKCommodity_Code);

			testDefaulter.AttemptDefaultFromImporter(importer, testDec.InvoiceLines);
			AssertEquals("Job is Import and Commodity Code defaults from importer", commodityCode4.RH_Code, line2.JI_RH_NKCommodity_Code);
		}

		#region Implementation

		JobDeclaration testDec;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine line1;
		JobComInvoiceLine line2;

		RefCommodityCode commodityCode1;
		RefCommodityCode commodityCode2;
		RefCommodityCode commodityCode3;
		RefCommodityCode commodityCode4;

		OrgHeader supplier;
		OrgHeader importer;

		CommodityCodeDefaulter testDefaulter;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			invoice = testDec.Invoices.AddNew();

			commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "XXXX";

			commodityCode2 = Factory.New<RefCommodityCode>();
			commodityCode2.RH_Code = "YYYY";

			commodityCode3 = Factory.New<RefCommodityCode>();
			commodityCode3.RH_Code = "XXYY";

			commodityCode4 = Factory.New<RefCommodityCode>();
			commodityCode4.RH_Code = "YYXX";

			supplier = OrgHeader.New(Factory);
			supplier.MiscServ.OM_RH_NKCMMainExportCmdty = commodityCode1.RH_Code;
			supplier.MiscServ.OM_RH_NKCMMainImportCmdty = commodityCode2.RH_Code;

			importer = OrgHeader.New(Factory);
			importer.MiscServ.OM_RH_NKCMMainExportCmdty = commodityCode3.RH_Code;
			importer.MiscServ.OM_RH_NKCMMainImportCmdty = commodityCode4.RH_Code;

			line1 = invoice.JobComInvoiceLines.AddNew();
			line2 = invoice.JobComInvoiceLines.AddNew();

			testDefaulter = new CommodityCodeDefaulter(testDec);
		}

		#endregion
	}
}
