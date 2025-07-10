using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CustomsQuantityConverterTest : TestCaseWithFactory
	{
		public void TestCalculateCustomsQuantity()
		{
			var cusRefPack = Factory.New<CusRefPacks>();
			cusRefPack.RP_CustomsCountry = "CA";
			cusRefPack.RP_CustomsPack = "KGM";
			cusRefPack.RP_CommercialPack = "BBK";
			cusRefPack.RP_ConversionFactor = 2;
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var product = MasterFiles.Business.OrgSupplierPart.New(Factory);
			product.OP_PartNum = "XXX123";
			var partUnit = product.PartUnits.AddNew();
			partUnit.OF_ParentPackType = "KGM";
			partUnit.OF_PackType = "MTK";
			partUnit.OF_QuantityInParent = 2;
			OrgPartRelation relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = invoiceHeader.JZ_OH_Supplier;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			invoiceLine.JI_CustomsUnitQty = "PCE";
			invoiceLine.JI_InvoiceQuantity = 100m;
			invoiceLine.JI_InvoiceUQ = "PCE";
			AssertEquals("JI_CustomsQuantity", 100m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_InvoiceQuantity = 200m;
			invoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("JI_CustomsQuantity", 200m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceUQ = "LB";
			AssertEquals("JI_CustomsQuantity", 90.7184m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_PartNo = "XXX123";
			invoiceLine.JI_InvoiceUQ = "MTK";
			AssertEquals("JI_CustomsQuantity", 100m, invoiceLine.JI_CustomsQuantity);

			invoiceLine.JI_InvoiceUQ = "BBK";
			AssertEquals("JI_CustomsQuantity", 400m, invoiceLine.JI_CustomsQuantity);
		}

		public void TestCalculateCustomsQuantityAndMapToUQ()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "20201055", ZDateTime.Today.AddYears(-1), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff1, "CU1", "LBR");

			var caTariffLBR = Factory.New<CACClassHeader>();
			caTariffLBR.ZA_ClassificationNumber = "20201055";
			caTariffLBR.ZA_EffectiveDate = ZDateTime.Today.AddYears(-1);
			caTariffLBR.ZA_ExpiryDate = ZDateTime.Today.AddYears(1);
			caTariffLBR.ZA_AreaCode = "AAA";
			caTariffLBR.ZA_StatisticalUOMCode = "LBR";

			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine.JI_Tariff = "20201055";
			invoiceLine.JI_InvoiceQuantity = 1m;
			AssertEquals("JI_InvoiceUQ should be LBR from Tariff's UoM", "LBR", invoiceLine.JI_CustomsUnitQty);
		}

		public void TestInvoiceQuantityIsRoundedAsIntForIID()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = IIDUnitOfCountCodeList.Codes.Each;
			invoiceLine.JI_CustomsUnitQty = CustomsUnitOfMeasureList.Codes.Each;

			invoiceLine.JI_InvoiceQuantity = 200.59m;
			AssertEquals("JI_InvoiceQuantity", 200.59m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("JI_CustomsQuantity", 200.59m, invoiceLine.JI_CustomsQuantity);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_InvoiceQuantity = 200.78m;
			AssertEquals("JI_InvoiceQuantity", 200.78m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("JI_CustomsQuantity", 200.78m, invoiceLine.JI_CustomsQuantity);
		}
	}
}
