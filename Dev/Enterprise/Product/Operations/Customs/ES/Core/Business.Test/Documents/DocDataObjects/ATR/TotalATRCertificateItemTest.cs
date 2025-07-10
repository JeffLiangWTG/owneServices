using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects.Testing;

[TestedType(typeof(TotalATRCertificateItem))]
class TotalATRCertificateItemTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when parameter is null", () => new TotalATRCertificateItem(null));
	}

	public void TestLineNumber()
	{
		var entryHeader = SetupEntryHeaderWithInvoiceLines();

		var atrCertificateItem1 = new TotalATRCertificateItem(entryHeader);
		DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(atrCertificateItem1, ZString.Empty, nameof(TotalATRCertificateItem.LineNumber));
	}

	public void TestGoodsDescription_NoPackagesNoVehicles()
	{
		using (Res.UseMockData())
		using (Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Spanish).UseMockData())
		{
			SetMockTranslations();
			var entryHeader = SetupEntryHeaderWithInvoiceLines();

			var atrCertificateItem1 = new TotalATRCertificateItem(entryHeader);
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(atrCertificateItem1, "I am Spanish translation: TOTAL GROSS WEIGHT 111,55 KG\nI am Spanish translation: TOTAL 0 I am Spanish translation: PACKAGES", nameof(TotalATRCertificateItem.GoodsDescription));
		}
	}

	public void TestGoodsDescription_Packages()
	{
		using (Res.UseMockData())
		using (Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Spanish).UseMockData())
		{
			SetMockTranslations();
			var entryHeader = SetupEntryHeaderWithInvoiceLines(addPackages: true);

			var atrCertificateItem1 = new TotalATRCertificateItem(entryHeader);
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(atrCertificateItem1, "I am Spanish translation: TOTAL GROSS WEIGHT 111,55 KG\nI am Spanish translation: TOTAL 205 I am Spanish translation: PACKAGES", nameof(TotalATRCertificateItem.GoodsDescription));
		}
	}

	public void TestGoodsDescription_Vehicles()
	{
		using (Res.UseMockData())
		using (Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Spanish).UseMockData())
		{
			SetMockTranslations();
			var entryHeader = SetupEntryHeaderWithInvoiceLines(addVehicles: true);

			var atrCertificateItem1 = new TotalATRCertificateItem(entryHeader);
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(atrCertificateItem1, "I am Spanish translation: TOTAL GROSS WEIGHT 111,55 KG\nI am Spanish translation: TOTAL 3 I am Spanish translation: PACKAGES", nameof(TotalATRCertificateItem.GoodsDescription));
		}
	}

	public void TestWeight()
	{
		var entryHeader = SetupEntryHeaderWithInvoiceLines();

		var atrCertificateItem1 = new TotalATRCertificateItem(entryHeader);
		DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(atrCertificateItem1, "111,55 KG", nameof(TotalATRCertificateItem.Weight));
	}

	public void TestVolume()
	{
		var entryHeader = SetupEntryHeaderWithInvoiceLines();

		var atrCertificateItem1 = new TotalATRCertificateItem(entryHeader);
		DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(atrCertificateItem1, ZString.Empty, nameof(TotalATRCertificateItem.Volume));
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		return new TotalATRCertificateItem(entryHeader);
	}

	void SetMockTranslations()
	{
		var spanish = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Spanish);
		var spanishMock = spanish.UseMockData();
		spanishMock.Put("516A9519-B7D4-400A-8AEB-8155ABB69D08", new ResourceStringData("516A9519-B7D4-400A-8AEB-8155ABB69D08", "I am Spanish translation: TOTAL"));
		spanishMock.Put("10A58664-29C0-48A6-B9C5-87EC7EF24F50", new ResourceStringData("10A58664-29C0-48A6-B9C5-87EC7EF24F50", "I am Spanish translation: PACKAGES"));
		spanishMock.Put("BD27AE69-96C3-4165-AD95-BB07668BACF1", new ResourceStringData("BD27AE69-96C3-4165-AD95-BB07668BACF1", "I am Spanish translation: TOTAL GROSS WEIGHT"));
	}

	CusEntryHeader SetupEntryHeaderWithInvoiceLines(bool addPackages = false, bool addVehicles = false)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
		var invoiceHeader = declaration.Invoices.AddNew();

		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine1.JI_Tariff = "78945612";
		invoiceLine1.JI_InvoiceQuantity = 100m;
		invoiceLine1.JI_InvoiceUQ = "BAG";
		invoiceLine1.JI_Description = "Books";
		invoiceLine1.JI_Weight = 10.634m;
		invoiceLine1.JI_WeightUQ = "KG";
		invoiceLine1.JI_Volume = 54m;
		invoiceLine1.JI_VolumeUQ = "M3";

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "45612378";
		invoiceLine2.JI_InvoiceQuantity = 100m;
		invoiceLine2.JI_InvoiceUQ = "BAG";
		invoiceLine2.JI_Description = "More Books";
		invoiceLine2.JI_Weight = 50.456m;
		invoiceLine2.JI_WeightUQ = "KG";
		invoiceLine2.JI_Volume = 63m;
		invoiceLine2.JI_VolumeUQ = "M3";

		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine3.JI_Tariff = "45612378";
		invoiceLine3.JI_InvoiceQuantity = 100m;
		invoiceLine3.JI_InvoiceUQ = "BAG";
		invoiceLine3.JI_Description = "More Books";
		invoiceLine3.JI_Weight = 50.456m;
		invoiceLine3.JI_WeightUQ = "KG";
		invoiceLine3.JI_Volume = 63m;
		invoiceLine3.JI_VolumeUQ = "M3";

		var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
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
			package1.CW_MarksAndNos = "M&N1";
			var packageCtInvoiceLine1 = invoiceLine1.PackagesPivot.AddNew();
			packageCtInvoiceLine1.CHC_CW = package1.PK;
			packageCtInvoiceLine1.CHC_NumberOfPacks = 150;

			var package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = billPackingGroup.PK;
			package2.CW_PackQty = 200;
			package2.CW_PackType = "CT";
			package2.CW_MarksAndNos = "M&N2";
			var packageCtInvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
			packageCtInvoiceLine2.CHC_CW = package2.PK;
			packageCtInvoiceLine2.CHC_NumberOfPacks = 50;

			var package3 = declaration.Packages.AddNew();
			package3.CW_CR_HouseContainer = billPackingGroup.PK;
			package3.CW_PackQty = 10;
			package3.CW_PackType = "BX";
			package3.CW_MarksAndNos = "M&N3";
			var packageCtInvoiceLine3 = invoiceLine3.PackagesPivot.AddNew();
			packageCtInvoiceLine3.CHC_CW = package3.PK;
			packageCtInvoiceLine3.CHC_NumberOfPacks = 5;
		}

		if (addVehicles)
		{
			var vehicle1 = invoiceLine1.Vehicles.AddNew();
			vehicle1.CVH_VehicleIdentificationNumber = "123";
			vehicle1.CVH_BrandName = "BRAND-A";
			vehicle1.CVH_ModelName = "MODEL-A";

			var vehicle2 = invoiceLine2.Vehicles.AddNew();
			vehicle2.CVH_VehicleIdentificationNumber = "456";
			vehicle2.CVH_BrandName = "BRAND-B";
			vehicle2.CVH_ModelName = "MODEL-B";

			var vehicle3 = invoiceLine3.Vehicles.AddNew();
			vehicle3.CVH_VehicleIdentificationNumber = "789";
			vehicle3.CVH_BrandName = "BRAND-C";
			vehicle3.CVH_ModelName = "MODEL-C";
		}

		declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

		var invoiceLine5 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine5.JI_Tariff = "12345678";
		invoiceLine5.JI_WeightUQ = "KG";
		invoiceLine5.JI_VolumeUQ = "DD";
		invoiceLine5.JI_InvoiceUQ = "DDJ";
		invoiceLine5.JI_Description = "LN";

		return declaration.CustomsEntryHeaders[0];
	}
}
