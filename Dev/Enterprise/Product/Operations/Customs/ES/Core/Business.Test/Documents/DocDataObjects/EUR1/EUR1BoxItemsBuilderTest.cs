using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects.Testing;

public class EUR1BoxItemsBuilderTest : EU.Business.Documents.DocDataObjects.Testing.EUR1BoxItemsBuilderTest
{
	protected override EU.Business.Documents.DocDataObjects.EUR1BoxItemsBuilder GetNewEUR1BoxItemsBuilder(IEnumerable<EU.Business.Declaration.JobComInvoiceLine> invoiceLines) => new EUR1BoxItemsBuilder(invoiceLines);

	protected override ZString ExpectedBox8 => @"1 PDTA: 123
100 VG, M&N1, 50 CT, M&N2.
invoice1line1 stuff
2 PDTA: 123
150 CT, M&N2.
invoice1line2 stuff
3 PDTA: 123
invoice2line1 stuff

PESO BRUTO TOTAL 4,4 Kg 

TOTAL 300 BULTOS
-------------------------------------------------------------------------------------------------------";

	public void TestItemsInfoBox8_Vehicles()
	{
		var language = Enterprise.Core.SharedConstants.Languages.Spanish;
		using (Res.UseMockData())
		using (var spanishMock = Res.GetLanguageInstance(language).UseMockData())
		{
			spanishMock.Put("516A9519-B7D4-400A-8AEB-8155ABB69D08", new ResourceStringData("516A9519-B7D4-400A-8AEB-8155ABB69D08", "TOTAL"));
			spanishMock.Put("10A58664-29C0-48A6-B9C5-87EC7EF24F50", new ResourceStringData("10A58664-29C0-48A6-B9C5-87EC7EF24F50", "BULTOS"));
			spanishMock.Put("BD27AE69-96C3-4165-AD95-BB07668BACF1", new ResourceStringData("BD27AE69-96C3-4165-AD95-BB07668BACF1", "PESO BRUTO TOTAL"));
			spanishMock.Put("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", new ResourceStringData("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", "BASTIDOR"));
			spanishMock.Put("645AF2E8-F73B-4E35-B253-7C5CE9E89004", new ResourceStringData("645AF2E8-F73B-4E35-B253-7C5CE9E89004", "BASTIDORES"));

			AssertWithEmptyInvoiceLineCollection(nameof(EUR1BoxItemsBuilder.ItemsInfoBox8), x => x.ItemsInfoBox8);

			var invoiceLines = GetInvoiceLines(addVehicles: true);
			var boxItemsBuilder = GetNewEUR1BoxItemsBuilder(invoiceLines);

			AssertEquals($"[PRE-CONDITION] {nameof(EUR1BoxItemsBuilder.ItemsInfoBox8)}", "", boxItemsBuilder.ItemsInfoBox8);
			boxItemsBuilder.Build();

			var expectedBox8_Vehicles = @"1 PDTA: 7894
2 BASTIDORES, 123 Brand-a Model-a, 123x Brand-x Model-x.
Books
2 PDTA: 4561
2 BASTIDORES, 456 Brand-b Model-b, 789 Brand-c Model-c.
More Books
3 PDTA: 3216
TV

PESO BRUTO TOTAL 111,55 Kg 

TOTAL 4 BULTOS
-------------------------------------------------------------------------------------------------------";

			AssertEquals(nameof(EUR1BoxItemsBuilder.ItemsInfoBox8), expectedBox8_Vehicles, boxItemsBuilder.ItemsInfoBox8);
		}
	}

	public void TestItemsInfoBox8Translation()
	{
		var language = Enterprise.Core.SharedConstants.Languages.Spanish;
		using (Res.UseMockData())
		using (var spanishMock = Res.GetLanguageInstance(language).UseMockData())
		{
			spanishMock.Put("516A9519-B7D4-400A-8AEB-8155ABB69D08", new ResourceStringData("516A9519-B7D4-400A-8AEB-8155ABB69D08", "I am Spanish translation: TOTAL"));
			spanishMock.Put("10A58664-29C0-48A6-B9C5-87EC7EF24F50", new ResourceStringData("10A58664-29C0-48A6-B9C5-87EC7EF24F50", "I am Spanish translation: PACKAGES"));
			spanishMock.Put("BD27AE69-96C3-4165-AD95-BB07668BACF1", new ResourceStringData("BD27AE69-96C3-4165-AD95-BB07668BACF1", "I am Spanish translation: TOTAL GROSS WEIGHT"));
			spanishMock.Put("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", new ResourceStringData("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", "I am Spanish translation: FRAME"));
			spanishMock.Put("645AF2E8-F73B-4E35-B253-7C5CE9E89004", new ResourceStringData("645AF2E8-F73B-4E35-B253-7C5CE9E89004", "I am Spanish translation: FRAMES"));

			var expectedGoodsDescriptionNoPackagesNoVehicles = "1 PDTA: 7894" + System.Environment.NewLine + "Books" + System.Environment.NewLine + "2 PDTA: 4561" + System.Environment.NewLine + "More Books" + System.Environment.NewLine + "3 PDTA: 3216" + System.Environment.NewLine + "TV" + System.Environment.NewLine + System.Environment.NewLine + "I am Spanish translation: TOTAL GROSS WEIGHT 111,55 Kg " + System.Environment.NewLine + System.Environment.NewLine + "I am Spanish translation: TOTAL 0 I am Spanish translation: PACKAGES" + System.Environment.NewLine + "-------------------------------------------------------------------------------------------------------";
			var expectedGoodsDescriptionPackages = "1 PDTA: 7894" + System.Environment.NewLine + "150 VG, m&n1." + System.Environment.NewLine + "Books" + System.Environment.NewLine + "2 PDTA: 4561" + System.Environment.NewLine + "50 CT, m&n2, 5 BX, m&n3." + System.Environment.NewLine + "More Books" + System.Environment.NewLine + "3 PDTA: 3216" + System.Environment.NewLine + "TV" + System.Environment.NewLine + System.Environment.NewLine + "I am Spanish translation: TOTAL GROSS WEIGHT 111,55 Kg " + System.Environment.NewLine + System.Environment.NewLine + "I am Spanish translation: TOTAL 205 I am Spanish translation: PACKAGES" + System.Environment.NewLine + "-------------------------------------------------------------------------------------------------------";
			var expectedGoodsDescriptionVehicles = "1 PDTA: 7894" + System.Environment.NewLine + "2 I am Spanish translation: FRAMES, 123 Brand-a Model-a, 123x Brand-x Model-x." + System.Environment.NewLine + "Books" + System.Environment.NewLine + "2 PDTA: 4561" + System.Environment.NewLine + "2 I am Spanish translation: FRAMES, 456 Brand-b Model-b, 789 Brand-c Model-c." + System.Environment.NewLine + "More Books" + System.Environment.NewLine + "3 PDTA: 3216" + System.Environment.NewLine + "TV" + System.Environment.NewLine + System.Environment.NewLine + "I am Spanish translation: TOTAL GROSS WEIGHT 111,55 Kg " + System.Environment.NewLine + System.Environment.NewLine + "I am Spanish translation: TOTAL 4 I am Spanish translation: PACKAGES" + System.Environment.NewLine + "-------------------------------------------------------------------------------------------------------";

			CombineAssertions(() =>
			{
				var boxItemsBuilder = GetNewEUR1BoxItemsBuilder(GetInvoiceLines());
				boxItemsBuilder.Build();
				AssertEquals("line without packages or vehicles", expectedGoodsDescriptionNoPackagesNoVehicles, boxItemsBuilder.ItemsInfoBox8);

				boxItemsBuilder = GetNewEUR1BoxItemsBuilder(GetInvoiceLines(addPackages: true));
				boxItemsBuilder.Build();
				AssertEquals("line with packages", expectedGoodsDescriptionPackages, boxItemsBuilder.ItemsInfoBox8);

				boxItemsBuilder = GetNewEUR1BoxItemsBuilder(GetInvoiceLines(addVehicles: true));
				boxItemsBuilder.Build();
				AssertEquals("line with vehicles", expectedGoodsDescriptionVehicles, boxItemsBuilder.ItemsInfoBox8);
			});
		}
	}

	protected override ZString ExpectedGrossMassVolumeBox9 => @"1,1 Kg
1,2 Kg
2,1 Kg";

	protected override ZString ExpectedInvoicesBox10 => @"FACT123
18-09-1971";

	protected override void AddDataToInvoiceLine(EU.Business.Declaration.JobComInvoiceLine invoiceLine)
	{
		var supDoc = invoiceLine.SupportingDocuments.AddNew();
		supDoc.CSI_Code = "N380";
		supDoc.CSI_ReferenceNumber = "FACT123";
		supDoc.CSI_DateOfIssue = ZDateTime.BrettsBirthday;

		var newInvLine = invoiceLine.CusEntryLine.InvoiceLines.AddNew();
		newInvLine.JI_LineNo = 3;
		newInvLine.JI_JZ = invoiceLine.JI_JZ;

		var declaration = newInvLine.Declaration;

		var packageVgInvoiceLine1 = newInvLine.PackagesPivot.AddNew();
		packageVgInvoiceLine1.CHC_CW = declaration.Packages[0].PK;
		packageVgInvoiceLine1.CHC_NumberOfPacks = 1;
	}

	protected override void SetUp()
	{
		base.SetUp();
		var language = Enterprise.Core.SharedConstants.Languages.Spanish;
		Res.UseMockData();
		var spanishMock = Res.GetLanguageInstance(language).UseMockData();
		spanishMock.Put("516A9519-B7D4-400A-8AEB-8155ABB69D08", new ResourceStringData("516A9519-B7D4-400A-8AEB-8155ABB69D08", "TOTAL"));
		spanishMock.Put("10A58664-29C0-48A6-B9C5-87EC7EF24F50", new ResourceStringData("10A58664-29C0-48A6-B9C5-87EC7EF24F50", "BULTOS"));
		spanishMock.Put("BD27AE69-96C3-4165-AD95-BB07668BACF1", new ResourceStringData("BD27AE69-96C3-4165-AD95-BB07668BACF1", "PESO BRUTO TOTAL"));
		spanishMock.Put("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", new ResourceStringData("1BEB56AA-A439-4FE1-92C0-2A60DCEC6B99", "BASTIDOR"));
		spanishMock.Put("645AF2E8-F73B-4E35-B253-7C5CE9E89004", new ResourceStringData("645AF2E8-F73B-4E35-B253-7C5CE9E89004", "BASTIDORES"));
	}

	IEnumerable<JobComInvoiceLine> GetInvoiceLines(bool addPackages = false, bool addVehicles = false)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_Tariff = "78945612";
		invoiceLine1.JI_InvoiceQuantity = 100m;
		invoiceLine1.JI_InvoiceUQ = "BAG";
		invoiceLine1.JI_Description = "Books";
		invoiceLine1.JI_Weight = 10.634m;
		invoiceLine1.JI_WeightUQ = "KG";
		invoiceLine1.JI_Volume = 54m;
		invoiceLine1.JI_VolumeUQ = "M3";

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "45612378";
		invoiceLine2.JI_InvoiceQuantity = 100m;
		invoiceLine2.JI_InvoiceUQ = "BAG";
		invoiceLine2.JI_Description = "More Books";
		invoiceLine2.JI_Weight = 50.456m;
		invoiceLine2.JI_WeightUQ = "KG";
		invoiceLine2.JI_Volume = 63m;
		invoiceLine2.JI_VolumeUQ = "M3";

		var invoiceLine3 = invoice.InvoiceLines.AddNew();
		invoiceLine3.JI_Tariff = "45612378";
		invoiceLine3.JI_InvoiceQuantity = 100m;
		invoiceLine3.JI_InvoiceUQ = "BAG";
		invoiceLine3.JI_Description = "More Books";
		invoiceLine3.JI_Weight = 50.456m;
		invoiceLine3.JI_WeightUQ = "KG";
		invoiceLine3.JI_Volume = 63m;
		invoiceLine3.JI_VolumeUQ = "M3";

		var invoiceLine4 = invoice.InvoiceLines.AddNew();
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

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		var entryLine3 = entryHeader.MergedLines.AddNew();
		entryLine3.CL_LineNumber = 3;

		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine2.PK;
		invoiceLine3.JI_CL = entryLine2.PK;
		invoiceLine4.JI_CL = entryLine3.PK;

		var invoiceLine5 = invoice.InvoiceLines.AddNew();
		invoiceLine5.JI_Tariff = "12345678";
		invoiceLine5.JI_WeightUQ = "KG";
		invoiceLine5.JI_VolumeUQ = "DD";
		invoiceLine5.JI_InvoiceUQ = "DDJ";
		invoiceLine5.JI_Description = "LN";

		yield return invoiceLine1;
		yield return invoiceLine2;
		yield return invoiceLine3;
		yield return invoiceLine4;
		yield return invoiceLine5;
	}
}
