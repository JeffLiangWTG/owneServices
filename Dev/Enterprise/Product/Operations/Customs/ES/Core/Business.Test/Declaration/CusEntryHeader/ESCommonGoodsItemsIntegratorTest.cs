using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Business.Testing.CommonGoodsItemsIntegration;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Declaration.Testing;

sealed class ESCommonGoodsItemsIntegratorTest : BaseCommonGoodsItemsIntegratorTest<JobDeclaration, CusEntryHeader, EuCommonGoodsItemsIntegrator, Customs.Business.NctsHeaderToAttachCollection>
{
	protected override void AssertExpectedPropertiesOnGoodsItem(ICommonGoodsItem goodsItem, Customs.Business.BaseJobComInvoiceLine line1)
	{
		CombineAssertions(() =>
		{
			var packages = goodsItem.Packages.ToArray();
			AssertEquals("Number of packages", 2, packages.Length);

			AssertEquals("PackageType", "FR", packages[0].PackageType);
			AssertEquals("PackageCount", 1, packages[0].PackageCount);
			AssertEquals("MarksAndNumbers", ZString.Empty, packages[0].MarksAndNumbers);
			AssertEquals("VehicleIdentificationNumber", "vin1", packages[0].VehicleIdentificationNumber);
			AssertEquals("BrandName", "peugeot", packages[0].BrandName);
			AssertEquals("ModelName", "308", packages[0].ModelName);

			AssertEquals("PackageType", "FR", packages[1].PackageType);
			AssertEquals("PackageCount", 1, packages[1].PackageCount);
			AssertEquals("MarksAndNumbers", ZString.Empty, packages[1].MarksAndNumbers);
			AssertEquals("VehicleIdentificationNumber", "vin2", packages[1].VehicleIdentificationNumber);
			AssertEquals("BrandName", "opel", packages[1].BrandName);
			AssertEquals("ModelName", "corsa", packages[1].ModelName);
		});
	}

	protected override (Customs.Business.CusEntryHeader, Customs.Business.BaseJobComInvoiceLine line1, Customs.Business.BaseJobComInvoiceLine line2) CreateEntryWithTwoInvoiceLines(BusinessObjectFactory factory)
	{
		var declaration = Factory.New<Customs.Business.BaseJobDeclaration>();
		var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		var invoice = declaration.Invoices.AddNew();

		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine1.JI_CEI = entryinstruction.PK;
		invoiceLine1.FillWithValidTestData();

		entryHeader.CH_CEI_Instruction = entryinstruction.PK;
		entryHeader.MovementReferenceNumberSetter("MRN123");

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;
		invoiceLine2.JI_CEI = entryinstruction.PK;
		invoiceLine2.FillWithValidTestData();

		var vehicle1 = invoiceLine1.Vehicles.AddNew();
		vehicle1.CVH_VehicleIdentificationNumber = "vin1";
		vehicle1.CVH_BrandName = "peugeot";
		vehicle1.CVH_ModelName = "308";

		var vehicle2 = invoiceLine1.Vehicles.AddNew();
		vehicle2.CVH_VehicleIdentificationNumber = "vin2";
		vehicle2.CVH_BrandName = "opel";
		vehicle2.CVH_ModelName = "corsa";

		return (entryHeader, invoiceLine1, invoiceLine2);
	}

	protected override void AssertExpectedPropertiesOnInvoiceLine(Customs.Business.BaseJobComInvoiceLine invoiceLine)
	{
	}

	protected override ICommonGoodsItem CreateCommonGoodsItem()
	{
		return new CommonGoodsItem() { };
	}
}
