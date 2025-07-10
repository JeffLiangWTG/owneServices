using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(ReleaseItemLineDetailsWrapper))]
sealed class ReleaseItemLineDetailsWrapperTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject() => new ReleaseItemLineDetailsWrapper(Factory.New<CusEntryLine>(), 1);

	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new ReleaseItemLineDetailsWrapper(null, 1));

	public void TestLineItem() => AssertEquals(1, wrapper.LineItem);

	public void TestGrossWeight() => AssertEquals(50.91m, wrapper.GrossWeight);

	public void TestNettWeight() => AssertEquals(48.32m, wrapper.NettWeight);

	public void TestTariffCode() => AssertEquals("1234567890", wrapper.TariffCode);

	public void TestProcedure() => AssertEquals("7110", wrapper.Procedure);

	public void TestUNDG() => AssertEquals("405201,103702", wrapper.UNDG);

	public void TestGoodsDescription() => AssertEquals("Smartwatch", wrapper.GoodsDescription);

	public void TestQtyUoM() => AssertEquals("48.32 KGM", wrapper.QtyUoM);

	public void TestStatValue() => AssertEquals(52316.32m, wrapper.StatValue);

	public void TestPackageMarks()
	{
		var package1 = declaration.Packages.AddNew();
		package1.CW_CR_HouseContainer = declaration.PrimaryHouseBill.PackingGroups[0].PK;
		package1.CW_PackQty = 5;
		package1.CW_MarksAndNos = "123456";

		var package2 = declaration.Packages.AddNew();
		package2.CW_CR_HouseContainer = declaration.PrimaryHouseBill.PackingGroups[0].PK;
		package2.CW_PackQty = 15;
		package2.CW_MarksAndNos = "654321";

		var packagePivot1 = invoiceLine1.PackagesPivot.AddNew();
		packagePivot1.CHC_CW = package1.PK;
		packagePivot1.CHC_NumberOfPacks = 5;

		var packagePivot2 = invoiceLine1.PackagesPivot.AddNew();
		packagePivot2.CHC_CW = package2.PK;
		packagePivot2.CHC_NumberOfPacks = 4;

		var packagePivot3 = invoiceLine2.PackagesPivot.AddNew();
		packagePivot3.CHC_CW = package2.PK;
		packagePivot3.CHC_NumberOfPacks = 11;

		AssertEquals("123456,654321", wrapper.PackageMarks);
	}

	public void TestAdditionalDocuments()
	{
		CombineAssertions(() =>
		{
			AssertType<ReleaseAdditionalDocumentWrapper>("AdditionalDocuments should be of type 'ReleaseAdditionalDocumentWrapper'", wrapper.AdditionalDocuments.First());
			AssertEquals("3 ReleaseAdditionalDocumentWrappers should be added to the list", 3, wrapper.AdditionalDocuments.Count);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		CusEntryHeader cusEntryHeader;
		var undgSubstance1 = Factory.NewWithValidTestData<UNDGSubstance>();
		undgSubstance1.DG_Code = "405201";
		var undgSubstance2 = Factory.NewWithValidTestData<UNDGSubstance>();
		undgSubstance2.DG_Code = "103702";
		Factory.Save();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_HouseBill = "HouseBill";

		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_InvoiceNumber = "INV001";
		invoice1.JZ_InvoiceDate = new ZDateTime(2021, 11, 18);
		invoice1.JZ_InvoiceAmount = 1200.00;
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		invoice1.JZ_InvoiceCurrExRate = 1;

		invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_Tariff = "1234567890";
		invoiceLine1.JI_Description = "Smartwatch";
		invoiceLine1.JI_InvoiceQuantity = 200;
		invoiceLine1.JI_InvoiceUQ = "UNT";
		invoiceLine1.JI_CountryOfOrigin = "NL";
		invoiceLine1.JI_CustomsQuantity = 200;
		invoiceLine1.JI_CustomsUnitQty = "KGM";
		invoiceLine1.ZG_TransNature = "6";
		invoiceLine1.ZG_CountryOfDestination = "NL";
		invoiceLine1.JI_LinePrice = 52316.32;
		invoiceLine1.ZG_StatisticalValue = 121.36;
		invoiceLine1.JI_Weight = 50.91;
		invoiceLine1.JI_WeightUQ = "KG";
		invoiceLine1.JI_NetWeight = 48.32;
		invoiceLine1.JI_NetWeightUQ = "KG";
		invoiceLine1.JI_Procedure = "7110";

		AddAdditionalInfo(invoiceLine1, "INF", "N123", "ReferenceNr1", 5, 164.20m);
		AddAdditionalInfo(invoiceLine1, "TRA", "T152", "ReferenceNr2", 4, 53.57m);
		AddAdditionalInfo(invoiceLine1, "REF", "R915", "ReferenceNr2", 3, 265.79m);

		var undg1 = invoiceLine1.UNDGs.AddNew();
		undg1.DI_DG = undgSubstance1.PK;
		var undg2 = invoiceLine1.UNDGs.AddNew();
		undg2.DI_DG = undgSubstance2.PK;

		invoiceLine2 = invoice1.InvoiceLines.AddNew();
		invoiceLine2.JI_Tariff = "9876543210";
		invoiceLine2.JI_Description = "Mobile phone";
		invoiceLine2.JI_InvoiceQuantity = 400;
		invoiceLine2.JI_InvoiceUQ = "UNT";
		invoiceLine2.JI_CountryOfOrigin = "NL";
		invoiceLine2.JI_CustomsQuantity = 400;
		invoiceLine2.JI_CustomsUnitQty = "KGM";
		invoiceLine2.ZG_StatisticalValue = 285.61;
		invoiceLine2.JI_Weight = 160.63;
		invoiceLine2.JI_WeightUQ = "KG";
		invoiceLine2.JI_NetWeight = 150.32;
		invoiceLine2.JI_NetWeightUQ = "KG";

		var undg3 = invoiceLine2.UNDGs.AddNew();
		undg3.DI_DG = undgSubstance1.PK;
		var undg4 = invoiceLine2.UNDGs.AddNew();
		undg4.DI_DG = undgSubstance2.PK;

		Factory.Save();
		var shutterUpperer = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);
		Factory.Save();

		cusEntryHeader = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault();
		cusEntryHeader.MovementReferenceNumberSetter("MRN1234567890", new ZDateTime(2021, 11, 18, 15, 00, 00));
		cusEntryHeader.CH_EntryReleaseDate = new ZDateTime(2021, 11, 18, 15, 00, 00);

		wrapper = new ReleaseItemLineDetailsWrapper(cusEntryHeader.RandomEntryLine as CusEntryLine, 1);
	}
	ReleaseItemLineDetailsWrapper wrapper;
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine1;
	JobComInvoiceLine invoiceLine2;

	void AddAdditionalInfo(JobComInvoiceLine invoiceLine, string subType, string code, string referenceNumber, decimal quantity, decimal value)
	{
		var addInfo = invoiceLine.AdditionalInfos.AddNew();
		addInfo.CSI_SubType = subType;
		addInfo.CSI_Code = code;
		addInfo.CSI_ReferenceNumber = referenceNumber;
		addInfo.CSI_Quantity = quantity;
		addInfo.CSI_Value = value;
	}
}
