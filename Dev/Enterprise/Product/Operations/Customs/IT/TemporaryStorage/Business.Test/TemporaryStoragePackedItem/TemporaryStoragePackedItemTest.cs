using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStoragePackedItem))]
sealed class TemporaryStoragePackedItemTest : EnterpriseBusinessObjectTestCase
{
	public void TestAPI_GrossWeight()
	{
		var packedItem = Factory.New<TemporaryStoragePackedItem>();

		CombineAssertions("API_GrossWeight", () =>
		{
			AssertResourceStringData(packedItem.API_GrossWeightInfo, "Gross Weight");
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(TemporaryStoragePackedItem), nameof(TemporaryStoragePackedItem.API_GrossWeight), false, x => x.DecimalPlacesMember == "API_GrossWeightDecimalPlaces");
		});
	}

	public void TestAPI_NetWeight()
	{
		var packedItem = Factory.New<TemporaryStoragePackedItem>();

		CombineAssertions("API_NetWeight", () =>
		{
			AssertResourceStringData(packedItem.API_NetWeightInfo, "Net Weight");
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(TemporaryStoragePackedItem), nameof(TemporaryStoragePackedItem.API_NetWeight), false, x => x.DecimalPlaces == 3);
		});
	}

	public void TestAPI_CustomsQty2()
	{
		var packedItem = Factory.New<TemporaryStoragePackedItem>();

		CombineAssertions("API_CustomsQty2", () =>
		{
			AssertResourceStringData(packedItem.API_CustomsQty2Info, "Supplementary Quantity", "Supplementary Qty", "Sup. Qty", "Supplementary Additional Quantity for Liability Amount Calculation");
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(TemporaryStoragePackedItem), nameof(TemporaryStoragePackedItem.API_CustomsQty2), false, x => x.DecimalPlaces == 6);
		});
	}

	public void TestAPI_CustomsUQ2()
	{
		AssertResourceStringData(packedItem.API_CustomsUQ2Info, "Supplementary Unit Quantity", "Supplementary Unit Qty", "Sup. Unit Qty", "Supplementary Additional Unit for Liability Amount Calculation");
	}

	public void TestSupportingDocuments()
	{
		AssertType<EU.Business.CusTempStorage.TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument>>(packedItem.SupportingDocuments);
	}

	public void TestAdditionalInfos()
	{
		AssertType<EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>>(packedItem.AdditionalInfos);
	}

	public void TestLinkPackages()
	{
		AssertType<TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage>>(packedItem.TemporaryStorageLinkPackages);
	}

	public void TestValidationType()
	{
		AssertType<TemporaryStoragePackedItemValidation>(packedItem.Validation);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var supportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)packedItem).GetCusSupportingInfoTypes();

		CombineAssertions(() =>
		{
			AssertEquals("SupportingInfoTypes Count", 3, supportingInfoTypes.Count);

			AssertEquals("Expected type for AdditionalInfo", typeof(TemporaryStorageAdditionalInfo), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			AssertEquals("Expected type for SupportingDocument", typeof(TemporaryStorageSupportingDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
			AssertEquals("Expected type for PreviousDocument", typeof(TemporaryStoragePreviousDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		});
	}

	public void TestTypeOfPreviousDocuments()
	{
		AssertType<EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>>(packedItem.PreviousDocuments);
	}

	public void TestBillType()
	{
		AssertType<TemporaryStorageBill>(packedItem.Bill);
	}

	public void TestGrossWeightInKG()
	{
		CombineAssertions(() =>
		{
			packedItem.API_GrossWeight = 10m;
			packedItem.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(10m, packedItem.GrossWeightInKG);

			packedItem.API_GrossWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(0.01m, packedItem.GrossWeightInKG);

			packedItem.API_GrossWeight = 0.1237777m;
			AssertEquals(0.000124m, packedItem.GrossWeightInKG);

			packedItem.API_GrossWeightUQ = "BB";
			AssertEquals(0m, packedItem.GrossWeightInKG);
		});
	}

	public void TestReadOnlyPropertiesWhenCustomsStatusAMG()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		packedItem = bill.PackedItems.AddNew();
		header.CustomsStatus = "AMG";
		CombineAssertions("When Customs status = AMG", () =>
		{
			AssertEquals("Net Weight should be read-only", true, packedItem.API_NetWeightInfo.ReadOnly);
			AssertEquals("Customs Qty2 should be read-only", true, packedItem.API_CustomsQty2Info.ReadOnly);
			AssertEquals("Chemical Substance Code should be read-only", true, packedItem.API_ChemicalSubstanceCodeInfo.ReadOnly);
			AssertEquals("Formatted Tariff should be read-only", true, packedItem.API_FormattedTariffInfo.ReadOnly);

			var additionalInfos = packedItem.AdditionalInfos.AddNew();
			AssertEquals("Additional Infos should be read-only", true, additionalInfos.ReadOnly);

			var supportingDocuments = packedItem.SupportingDocuments.AddNew();
			AssertEquals("Supporting Documents should be read-only", true, supportingDocuments.ReadOnly);

			var supplyChainActors = packedItem.SupplyChainActors.AddNew();
			AssertEquals("Supply Chain Actors should be read-only", true, supplyChainActors.ReadOnly);
		});
	}

	public void TestNetWeightInKG()
	{
		CombineAssertions(() =>
		{
			packedItem.API_NetWeight = 10m;
			packedItem.API_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(10m, packedItem.NetWeightInKG);

			packedItem.API_NetWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(0.01m, packedItem.NetWeightInKG);

			packedItem.API_NetWeight = 0.1237777m;
			AssertEquals(0.000124m, packedItem.NetWeightInKG);

			packedItem.API_NetWeightUQ = "BB";
			AssertEquals(0m, packedItem.NetWeightInKG);
		});
	}

	public void TestRegistrationNo()
	{
		var regEntryNumber = TemporaryStorageTestHelper.CreateCusEntryNumber(packedItem, CusEntryNumberTypes.EU.CustomsRegistry);
		regEntryNumber.CE_EntryNum = "TestREG";
		AssertEquals("When RegistrationNo is filled", "TestREG", packedItem.RegistrationNo);

		regEntryNumber.CE_EntryNum = ZString.Empty;
		AssertEquals("When RegistrationNo is empty", ZString.Empty, packedItem.RegistrationNo);
	}

	public void TestReleaseDate()
	{
		var regEntryNumber = TemporaryStorageTestHelper.CreateCusEntryNumber(packedItem, CusEntryNumberTypes.EU.CustomsRegistry);
		regEntryNumber.CE_IssueDate = new ZDateTime(2024, 07, 24);
		AssertEquals("When Release Date is filled", new ZDateTime(2024, 07, 24), packedItem.ReleaseDate);

		regEntryNumber.CE_IssueDate = ZDateTime.Empty;
		AssertEquals("When Release Date is empty", ZDateTime.Empty, packedItem.ReleaseDate);
	}

	protected override BusinessObject GetNewBusinessObject() => packedItem;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => packedItem;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => packedItem;

	void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption = null, string shortCaption = null, string fullDescription = null)
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
		AssertEquals("Caption", caption, captionResourceString.Caption);

		if (mediumCaption is not null)
		{
			AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
		}

		if (shortCaption is not null)
		{
			AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
		}

		if (fullDescription is not null)
		{
			AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		packedItem = bill.PackedItems.AddNew();
	}

	TemporaryStoragePackedItem packedItem;
}
