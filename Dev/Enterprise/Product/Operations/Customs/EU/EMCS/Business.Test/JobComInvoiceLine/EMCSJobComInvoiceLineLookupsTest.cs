using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class EMCSJobComInvoiceLineLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
	{
		public void TestPartsList()
		{
			var consignor = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var goodsOwner = Factory.New<OrgHeader>();

			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.OwnerDocumentaryAddress.OrganisationPK = goodsOwner.PK;

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine1.JI_PartNo = "AAA";
			invoiceLine2.JI_PartNo = string.Empty;

			var parts = invoiceLine1.Lookups.PartsList;

			AssertEquals("OP_PartNum is set", true, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Product Code:Property"));

			AssertEquals("Correct Importer", goodsOwner.PK, (ZGuid)parts.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
			AssertEquals("Correct Supplier", consignor.PK, (ZGuid)parts.FilterBusinessObjectDefaults["Importer/Supplier:Property2"].Value);

			parts = invoiceLine2.Lookups.PartsList;

			AssertEquals("OP_PartNum is not set", false, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Product Code:Property"));

			AssertEquals("Correct Importer", goodsOwner.PK, (ZGuid)parts.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
			AssertEquals("Correct Supplier", consignor.PK, (ZGuid)parts.FilterBusinessObjectDefaults["Importer/Supplier:Property2"].Value);

			declaration.OwnerDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			parts = invoiceLine1.Lookups.PartsList;

			AssertEquals("Correct Importer", consignee.PK, (ZGuid)parts.FilterBusinessObjectDefaults["Importer/Supplier:Property1"].Value);
			AssertEquals("Correct Supplier", consignor.PK, (ZGuid)parts.FilterBusinessObjectDefaults["Importer/Supplier:Property2"].Value);

			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Importer = ZGuid.Empty;
			parts = invoiceLine1.Lookups.PartsList;

			AssertEquals("Importer is not set", false, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property1"));
			AssertEquals("Supplier is not set", false, parts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier:Property2"));
		}

		public void TestCustomsUQList()
		{
			AssertType<EMCSCustomsQuantityTypeList>(lookups.CustomsUQList);
			AssertSame(lookups.CustomsUQList, Factory.New<EMCSJobComInvoiceLine>().Lookups.CustomsUQList);
			AssertEquals("1, 2, 3, 4", lookups.CustomsUQList.CodesAsString);
		}

		public void TestTariffs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, "01", ZDateTime.Now.AddMonths(-1), ZDateTime.Now.AddYears(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSCNCodes, "02", ZDateTime.Now.AddMonths(-1), ZDateTime.Now.AddYears(1));

			Factory.Save();

			var list = lookups.CNCodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Code list", new[] { "01", "02" }, list.Select(x => x.ZZD_Code));
				AssertEquals("Cached", list, lookups.CNCodeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
			invoiceLine = declaration.InvoiceHeader.InvoiceLines.AddNew();
			lookups = new EMCSJobComInvoiceLineLookups(invoiceLine);
		}
		EMCSJobDeclaration declaration;
		EMCSJobComInvoiceLine invoiceLine;
		EMCSJobComInvoiceLineLookups lookups;
	}
}
