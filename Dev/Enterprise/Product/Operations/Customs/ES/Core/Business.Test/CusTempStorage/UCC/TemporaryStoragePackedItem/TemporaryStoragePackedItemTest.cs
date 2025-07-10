using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStoragePackedItem))]
sealed class TemporaryStoragePackedItemTest : EnterpriseBusinessObjectTestCase
{
	public void TestPreviousDocuments()
	{
		AssertType<EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>>(packedItem.PreviousDocuments);
	}

	public void TestAdditionalInfos()
	{
		AssertType<EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>>(packedItem.AdditionalInfos);
	}

	public void TestValidation()
	{
		AssertType<TemporaryStoragePackedItemValidation>(packedItem.Validation);
	}

	public void TestCalculateAndRefreshAllDutyAmountFromTariffRates()
	{
		bill.Header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
		AssertEquals("CalculateAndRefreshAllDutyAmountFromTariffRates is true when no TSM and UnionGoods", true, packedItem.CalculateAndRefreshAllDutyAmountFromTariffRates);

		bill.Header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
		bill.Header.UnionGoods = true;
		AssertEquals("CalculateAndRefreshAllDutyAmountFromTariffRates is false when TSM and UnionGoods", false, packedItem.CalculateAndRefreshAllDutyAmountFromTariffRates);

		bill.Header.UnionGoods = false;
		AssertEquals("CalculateAndRefreshAllDutyAmountFromTariffRates is true when TSM but no UnionGoods", true, packedItem.CalculateAndRefreshAllDutyAmountFromTariffRates);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var supportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)packedItem).GetCusSupportingInfoTypes();

		CombineAssertions(() =>
		{
			AssertEquals("SupportingInfoTypes Count", 3, supportingInfoTypes.Count);
			AssertEquals("Contains AdditionalInfo?", expected: true, supportingInfoTypes.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo));
			AssertEquals("Contains SupportingDocument?", expected: true, supportingInfoTypes.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument));
			AssertEquals("Contains PreviousDocument?", expected: true, supportingInfoTypes.ContainsKey(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument));

			AssertEquals("Expected type for AdditionalInfo", typeof(TemporaryStorageAdditionalInfo), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			AssertEquals("Expected type for SupportingDocument", typeof(EU.Business.CusTempStorage.TemporaryStorageSupportingDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
			AssertEquals("Expected type for PreviousDocument", typeof(TemporaryStoragePreviousDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		});
	}

	public void TestIsMissingCaptions()
	{
		CombineAssertions(() =>
		{
			AssertResourceStringData(packedItem.IsMissingInfo, "Missing", "Missing", "Missing", "Is Missing");
		});
	}

	public void TestIsMissing()
	{
		CombineAssertions(() =>
		{
			packedItem.IsMissing = false;
			AssertEquals("API_PackStatus should be empty", ZString.Empty, packedItem.API_PackStatus);
			packedItem.IsMissing = true;
			AssertEquals("API_PackStatus should be MIS", "MIS", packedItem.API_PackStatus);

			packedItem.API_PackStatus = ZString.Empty;
			AssertEquals("IsMissing should be false", false, packedItem.IsMissing);

			packedItem.API_PackStatus = "MIS";
			AssertEquals("IsMissing should be true", true, packedItem.IsMissing);
		});
	}

	public void TestPresentationDateCaptions()
	{
		CombineAssertions(() =>
		{
			AssertResourceStringData(packedItem.PresentationDateInfo, "Presentation Date", "Presentation Date", "Presentation Date", "Date of Presentation");
		});
	}

	public void TestPresentationDatePersistance()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "PresentationDateOrigin");

		packedItem.PresentationDate = new ZDateTime(2022, 02, 16);

		CombineAssertions(() =>
		{
			AssertEquals("PresentationDate", new ZDateTime(2022, 02, 16), packedItem.PresentationDate);
			AssertNotNull("PresentationDate is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
		});
	}

	public void TestUCRCaptions()
	{
		CombineAssertions(() =>
		{
			AssertResourceStringData(packedItem.UCRInfo, "UCR", "UCR", "UCR", "UCR Number");
		});
	}

	public void TestUCR()
	{
		CombineAssertions(() =>
		{
			AssertEquals("UCR should be empty", ZString.Empty, packedItem.UCR);

			AssertEquals("Prereq: packedItem's CountryCode is ES", "ES", packedItem.CountryCode);
			var newEntryNumber = CusEntryNumber.LoadOrCreate(packedItem, "UCR", "ES");
			newEntryNumber.CE_EntryNum = "1234";
			newEntryNumber.CE_EntryIsSystemGenerated = true;

			AssertEquals("UCR should be filled when UCR EntryNum added/updated", "1234", packedItem.UCR);

			newEntryNumber.Delete();
			newEntryNumber = CusEntryNumber.LoadOrCreate(packedItem, "UCR", "ES");
			newEntryNumber.CE_EntryNum = "1234";
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			newEntryNumber.CE_RN_NKCountryCode = "AA";
			AssertEquals("UCR should not be filled when CE_RN_NKCountryCode is not the same as packedItem's CountryCode", ZString.Empty, packedItem.UCR);

			newEntryNumber.Delete();
			packedItem.UCR = "AAA";
			AssertEquals("UCR should be filled when it is set", "AAA", packedItem.UCR);
			var entryNum = CusEntryNumber.Load(packedItem, "UCR", "ES");
			AssertNotNull("UCR EntryNum should not be null when UCR is set manually", entryNum);
			AssertEquals("UCR EntryNum.CE_EntryNum should have the correct value", "AAA", entryNum.CE_EntryNum);
		});
	}

	public void TestGrossWeightInKG()
	{
		packedItem.API_GrossWeight = new ZDecimal(2000);
		packedItem.API_GrossWeightUQ = Core.Constants.Weight.Pounds;
		AssertEquals(907.18474m, packedItem.GrossWeightInKG);
	}

	public void TestTotalPackageQuantity()
	{
		packedItem.API_LineNo = 2;
		var newPackedItem = bill.PackedItems.AddNew();
		newPackedItem.API_LineNo = 1;
		var package1 = bill.Packs.AddNew();
		package1.APA_PackUQ = "BX";
		package1.APA_PackQty = 5;
		var package2 = bill.Packs.AddNew();
		package2.APA_PackUQ = EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked;
		package2.APA_PackQty = 4;
		var package3 = bill.Packs.AddNew();
		package3.APA_PackUQ = "FR";
		package3.APA_PackQty = 3;
		var package4 = bill.Packs.AddNew();
		package4.APA_PackUQ = "VG";
		package4.APA_PackQty = 3;
		var package5 = bill.Packs.AddNew();
		package5.APA_PackUQ = "NE";
		package5.APA_PackQty = 50;
		var package6 = bill.Packs.AddNew();
		package6.APA_PackUQ = "AA";
		package6.APA_PackQty = 35;

		var linkPackage1 = packedItem.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package1);
		linkPackage1.IsLinked = true;
		var linkPackage2 = packedItem.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package2);
		linkPackage2.IsLinked = true;
		var linkPackage3 = packedItem.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package3);
		linkPackage3.IsLinked = true;
		var linkPackage4 = packedItem.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package4);
		linkPackage4.IsLinked = true;
		var linkPackage5 = packedItem.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package5);
		linkPackage5.IsLinked = false;
		var linkPackage6 = packedItem.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package6);
		linkPackage6.IsLinked = true;
		var linkPackage7 = newPackedItem.TemporaryStorageLinkPackages.FirstOrDefault(p => p.Package == package6);
		linkPackage7.IsLinked = true;

		AssertEquals(15, packedItem.TotalPackageQuantity);
	}

	public void TestAPI_LineNo()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "ES";
		var bill = header.Bills.AddNew();
		var packedItems = bill.PackedItems;

		CombineAssertions("When MessageType is not TSM", () =>
		{
			var item1 = packedItems.AddNew();
			var item2 = packedItems.AddNew();
			item2.API_LineNo = 20;
			var item3 = packedItems.AddNew();

			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)2, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)3, item3.API_LineNo);

			packedItems.RemoveAndDelete(item2);
			AssertEquals("item1.API_LineNo after item2 is removed (reset)", (ZInt)1, item1.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is removed (reset)", (ZInt)2, item3.API_LineNo);
		});

		packedItems.RemoveAndDeleteAll();
		header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;

		CombineAssertions("When MessageType is TSM", () =>
		{
			var item1 = packedItems.AddNew();
			var item2 = packedItems.AddNew();
			item2.API_LineNo = 20;
			var item3 = packedItems.AddNew();

			AssertEquals("item1.API_LineNo", (ZInt)1, item1.API_LineNo);
			AssertEquals("item2.API_LineNo", (ZInt)20, item2.API_LineNo);
			AssertEquals("item3.API_LineNo", (ZInt)21, item3.API_LineNo);

			packedItems.RemoveAndDelete(item2);
			AssertEquals("item1.API_LineNo after item2 is removed (not reset)", (ZInt)1, item1.API_LineNo);
			AssertEquals("item3.API_LineNo after item2 is removed (not reset)", (ZInt)21, item3.API_LineNo);
		});
	}

	public void TestClone()
	{
		TemporaryStorageTestHelper.SetUpTariff(Factory, Core.Constants.CountryCodes.Spain);

		packedItem.UCR = "A001";
		packedItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;

		packedItem.API_GoodsValue = 1000m;
		packedItem.AdditionalSupplementaryCodes.AddNew("A002");
		packedItem.AdditionalSupplementaryCodes.AddNew("A003");
		AssertDutyAndTaxes(packedItem, 1, "DTY", "%", 1000m, 12m, 120m);

		var clonedPackedItem = (TemporaryStoragePackedItem)packedItem.Clone();
		AssertEquals("UCR is cloned", "A001", clonedPackedItem.UCR);
		AssertEquals("Supplementary Codes are cloned", "A002,A003", clonedPackedItem.API_Supplements);
		AssertDutyAndTaxes(clonedPackedItem, 1, "DTY", "%", 1000m, 12m, 120m);

		packedItem.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		clonedPackedItem = (TemporaryStoragePackedItem)packedItem.Clone();
		AssertEquals("UCR is cloned", "A001", clonedPackedItem.UCR);
		AssertEquals("Supplementary Codes is empty", string.Empty, clonedPackedItem.API_Supplements);
		AssertDutyAndTaxes(clonedPackedItem, 1, "DTY", "%", 1000m, 12m, 120m);
	}

	void AssertDutyAndTaxes(TemporaryStoragePackedItem packedItem, int expectedCount, string expectedChargeType, string expectedMethodOfCalculation, decimal expectedBaseValue, decimal expectedRate, decimal expectedChargeAmount)
	{
		CombineAssertions("Duty and Taxes", () =>
		{
			AssertEquals("Count", expectedCount, packedItem.DutiesAndTaxes.Count);
			var dutyAndTax = packedItem.DutiesAndTaxes[0];
			AssertEquals("Charge Type", expectedChargeType, dutyAndTax.AET_ChargeType);
			AssertEquals("Metod of Calculation", expectedMethodOfCalculation, dutyAndTax.AET_MethodOfCalculation);
			AssertEquals("Base Value", expectedBaseValue, dutyAndTax.AET_BaseValue);
			AssertEquals("Rate", expectedRate, dutyAndTax.AET_Rate);
			AssertEquals("Charge Amount", expectedChargeAmount, dutyAndTax.AET_ChargeAmount);
		});
	}

	void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption, string shortCaption, string fullDescription)
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
		AssertEquals("Caption", caption, captionResourceString.Caption);
		AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
		AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
		AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
	}

	protected override BusinessObject GetNewBusinessObject() => packedItem;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => packedItem;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => packedItem;

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "ES";
		bill = header.Bills.AddNew();
		packedItem = bill.PackedItems.AddNew();
	}

	TemporaryStoragePackedItem packedItem;
	TemporaryStorageBill bill;
}
