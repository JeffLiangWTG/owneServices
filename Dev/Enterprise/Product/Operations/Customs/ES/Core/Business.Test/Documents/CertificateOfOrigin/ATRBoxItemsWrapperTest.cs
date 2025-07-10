using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin.Testing;

class ATRBoxItemsBuilderTest : BusinessObjectValidationTestCase
{
	public void TestGoodsDescription_NoPackagesNoVehicles()
	{
		var invoiceLines = GetInvoiceLines();
		var expectedGoodsDescriptionEntryLine1 = ZString.Empty;
		expectedGoodsDescriptionEntryLine1 = "PDTA: 7894" + System.Environment.NewLine + "BOOKS" + System.Environment.NewLine + "PDTA: 4561" + System.Environment.NewLine + "MORE BOOKS" + System.Environment.NewLine + "PDTA: 4561" + System.Environment.NewLine + "MORE BOOKS" + System.Environment.NewLine + "PDTA: 3216" + System.Environment.NewLine + "TV";

		CombineAssertions(() =>
		{
			var boxItemBuilder = new ATRBoxItemsWrapper(invoiceLines);
			boxItemBuilder.Build();
			var marksNumber = boxItemBuilder.MarksNumberBox10;
			AssertEquals("Goods description no packages no vehicles", expectedGoodsDescriptionEntryLine1, marksNumber);
		});
	}

	public void TestGoodsDescription_Packages()
	{
		var invoiceLines = GetInvoiceLines(addPackages: true);
		var expectedGoodsDescriptionEntryLine1 = ZString.Empty;
		expectedGoodsDescriptionEntryLine1 = "PDTA: 7894" + System.Environment.NewLine + "150 VG, M&N1." + System.Environment.NewLine + "BOOKS" + System.Environment.NewLine + "PDTA: 4561" + System.Environment.NewLine + "50 CT, M&N2." + System.Environment.NewLine + "5 BX, M&N3." + System.Environment.NewLine + "MORE BOOKS" + System.Environment.NewLine + "PDTA: 4561" + System.Environment.NewLine + "50 CT, M&N2." + System.Environment.NewLine + "5 BX, M&N3." + System.Environment.NewLine + "MORE BOOKS" + System.Environment.NewLine + "PDTA: 3216" + System.Environment.NewLine + "TV";

		CombineAssertions(() =>
		{
			var boxItemBuilder = new ATRBoxItemsWrapper(invoiceLines);
			boxItemBuilder.Build();
			var marksNumber = boxItemBuilder.MarksNumberBox10;
			AssertEquals("Goods description packages", expectedGoodsDescriptionEntryLine1, marksNumber);
		});
	}

	public void TestGoodsDescription_Vehicles()
	{
		var language = Core.SharedConstants.Languages.Spanish;
		using (Res.UseMockData())
		using (Res.GetLanguageInstance(language).UseMockData())
		{
			var spanish = Res.GetLanguageInstance(language);
			var spanishMock = spanish.UseMockData();
			spanishMock.Put("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", new ResourceStringData("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", "I am Spanish translation: FRAME"));
			spanishMock.Put("645AF2E8-F73B-4E35-B253-7C5CE9E89004", new ResourceStringData("645AF2E8-F73B-4E35-B253-7C5CE9E89004", "I am Spanish translation: FRAMES"));

			var invoiceLines = GetInvoiceLines(addVehicles: true);
			var expectedGoodsDescriptionEntryLine1 = ZString.Empty;
			expectedGoodsDescriptionEntryLine1 = "PDTA: 7894" + System.Environment.NewLine + "2 I AM SPANISH TRANSLATION: FRAMES, 123 BRAND-A MODEL-A, 123X BRAND-X MODEL-X." + System.Environment.NewLine + "BOOKS" + System.Environment.NewLine + "PDTA: 4561" + System.Environment.NewLine + "2 I AM SPANISH TRANSLATION: FRAMES, 456 BRAND-B MODEL-B, 789 BRAND-C MODEL-C." + System.Environment.NewLine + "MORE BOOKS" + System.Environment.NewLine + "PDTA: 4561" + System.Environment.NewLine + "2 I AM SPANISH TRANSLATION: FRAMES, 456 BRAND-B MODEL-B, 789 BRAND-C MODEL-C." + System.Environment.NewLine + "MORE BOOKS" + System.Environment.NewLine + "PDTA: 3216" + System.Environment.NewLine + "TV";

			CombineAssertions(() =>
			{
				var boxItemBuilder = new ATRBoxItemsWrapper(invoiceLines);
				boxItemBuilder.Build();
				var marksNumber = boxItemBuilder.MarksNumberBox10;
				AssertEquals("Goods description vehicles", expectedGoodsDescriptionEntryLine1, marksNumber);
				AssertContains("Goods description vehicles", expectedGoodsDescriptionEntryLine1, marksNumber);
			});
		}
	}

	public void TestWeight()
		{
		var invoiceLines = GetInvoiceLines();

		CombineAssertions(() =>
		{
			var boxItemBuilder = new ATRBoxItemsWrapper(invoiceLines);
			boxItemBuilder.Build();
			var grossWeight = boxItemBuilder.GrossWeightBox11;
			AssertEquals("Box 11", "10,63 KG" + System.Environment.NewLine + "50,46 KG" + System.Environment.NewLine + "50,46 KG" + System.Environment.NewLine + "0 KG", grossWeight);
		});
	}

	IEnumerable<JobComInvoiceLine> GetInvoiceLines(bool addPackages = false, bool addVehicles = false)
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine1 = declaration.InvoiceLines.AddNew();
		invoiceLine1.JI_Tariff = "78945612";
		invoiceLine1.JI_InvoiceQuantity = 100m;
		invoiceLine1.JI_InvoiceUQ = "BAG";
		invoiceLine1.JI_Description = "Books";
		invoiceLine1.JI_Weight = 10.634m;
		invoiceLine1.JI_WeightUQ = "KG";
		invoiceLine1.JI_Volume = 54m;
		invoiceLine1.JI_VolumeUQ = "M3";

		var invoiceLine2 = declaration.InvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "45612378";
		invoiceLine2.JI_InvoiceQuantity = 100m;
		invoiceLine2.JI_InvoiceUQ = "BAG";
		invoiceLine2.JI_Description = "More Books";
		invoiceLine2.JI_Weight = 50.456m;
		invoiceLine2.JI_WeightUQ = "KG";
		invoiceLine2.JI_Volume = 63m;
		invoiceLine2.JI_VolumeUQ = "M3";

		var invoiceLine3 = declaration.InvoiceLines.AddNew();
		invoiceLine3.JI_Tariff = "45612378";
		invoiceLine3.JI_InvoiceQuantity = 100m;
		invoiceLine3.JI_InvoiceUQ = "BAG";
		invoiceLine3.JI_Description = "More Books";
		invoiceLine3.JI_Weight = 50.456m;
		invoiceLine3.JI_WeightUQ = "KG";
		invoiceLine3.JI_Volume = 63m;
		invoiceLine3.JI_VolumeUQ = "M3";

		var invoiceLine4 = declaration.InvoiceLines.AddNew();
		invoiceLine4.JI_Tariff = "32165498";
		invoiceLine4.JI_WeightUQ = "LT";
		invoiceLine4.JI_VolumeUQ = "M3";
		invoiceLine4.JI_InvoiceUQ = "BBK";
		invoiceLine4.JI_Description = "TV";

		if (addPackages)
		{
			var billPackingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();

			var package1 = declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = billPackingGroup.PK;
			package1.CW_PackQty = 100;
			package1.CW_PackType = "VG";
			package1.CW_MarksAndNos = "m&n1";
			var packageCtInvoiceLine1 = invoiceLine1.PackagesPivot.AddNew();
			packageCtInvoiceLine1.CHC_CW = package1.PK;
			packageCtInvoiceLine1.CHC_NumberOfPacks = 150;

			var package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = billPackingGroup.PK;
			package2.CW_PackQty = 200;
			package2.CW_PackType = "CT";
			package2.CW_MarksAndNos = "m&n2";
			var packageCtInvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
			packageCtInvoiceLine2.CHC_CW = package2.PK;
			packageCtInvoiceLine2.CHC_NumberOfPacks = 50;

			var package3 = declaration.Packages.AddNew();
			package3.CW_CR_HouseContainer = billPackingGroup.PK;
			package3.CW_PackQty = 10;
			package3.CW_PackType = "BX";
			package3.CW_MarksAndNos = "m&n3";
			var packageCtInvoiceLine3 = invoiceLine3.PackagesPivot.AddNew();
			packageCtInvoiceLine3.CHC_CW = package3.PK;
			packageCtInvoiceLine3.CHC_NumberOfPacks = 5;
		}

		if (addVehicles)
		{
			var vehicle1 = invoiceLine1.Vehicles.AddNew();
			vehicle1.CVH_VehicleIdentificationNumber = "123";
			vehicle1.CVH_BrandName = "Brand-a";
			vehicle1.CVH_ModelName = "Model-a";

			var vehicle1x = invoiceLine1.Vehicles.AddNew();
			vehicle1x.CVH_VehicleIdentificationNumber = "123x";
			vehicle1x.CVH_BrandName = "Brand-x";
			vehicle1x.CVH_ModelName = "Model-x";

			var vehicle2 = invoiceLine2.Vehicles.AddNew();
			vehicle2.CVH_VehicleIdentificationNumber = "456";
			vehicle2.CVH_BrandName = "Brand-b";
			vehicle2.CVH_ModelName = "Model-b";

			var vehicle3 = invoiceLine3.Vehicles.AddNew();
			vehicle3.CVH_VehicleIdentificationNumber = "789";
			vehicle3.CVH_BrandName = "Brand-c";
			vehicle3.CVH_ModelName = "Model-c";
		}

		var invoiceLines = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = invoiceLines.MergedLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		var entryLine2 = invoiceLines.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		var entryLine3 = invoiceLines.MergedLines.AddNew();
		entryLine3.CL_LineNumber = 3;

		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine2.PK;
		invoiceLine3.JI_CL = entryLine2.PK;
		invoiceLine4.JI_CL = entryLine3.PK;

		yield return invoiceLine1;
		yield return invoiceLine2;
		yield return invoiceLine3;
		yield return invoiceLine4;
	}
}
