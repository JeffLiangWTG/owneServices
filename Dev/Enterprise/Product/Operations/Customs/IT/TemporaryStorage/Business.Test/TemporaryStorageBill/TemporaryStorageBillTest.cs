using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageBill))]
sealed class TemporaryStorageBillTest : TemporaryStorageBillAbstractTest<TemporaryStorageBill, TemporaryStorageHeader>
{
	public void TestCaptions()
	{
		CombineAssertions(() =>
		{
			AssertCaptions(nameof(TemporaryStorageBill.GrossWeightInKG), "Gross Weight in KG", "Gross Weight", "Gross Wgt.", "Bill Total Gross Weight in KG");
			AssertCaptions(nameof(TemporaryStorageBill.NetWeightInKG), "Net Weight in KG", "Net Weight", "Net Wgt.", "Bill Total Net Weight in KG");
			AssertCaptions(nameof(TemporaryStorageBill.SuppQuantity), "Supp. Quantity", "Supp. Quantity", "Supp. Qty.", "Bill Total Supplementary Quantity");
			AssertCaptions(nameof(TemporaryStorageBill.Lrn), "LRN", "LRN", "LRN");
			AssertCaptions(nameof(TemporaryStorageBill.Mrn), "MRN", "MRN", "MRN");
		});

		void AssertCaptions(string propertyName, string caption, string mediumCaption, string shortCaption, string fullDescription = null)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageBill), propertyName);
			AssertEquals($"{propertyName} Caption", caption, resourceStringData.Caption);
			AssertEquals($"{propertyName} Medium Caption", mediumCaption, resourceStringData.MediumCaption);
			AssertEquals($"{propertyName} Short Caption", shortCaption, resourceStringData.ShortCaption);

			if (fullDescription != null)
			{
				AssertEquals($"{propertyName} Full Description", fullDescription, resourceStringData.FullDescription);
			}
		}
	}

	public void TestGrossWeightInKG()
	{
		var packedItem1 = bill.PackedItems.AddNew();
		var packedItem2 = bill.PackedItems.AddNew();
		var grossWeight = typeof(TemporaryStorageBill).GetProperty(nameof(bill.GrossWeightInKG));

		packedItem1.API_GrossWeight = 10;
		packedItem1.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

		packedItem2.API_GrossWeight = 8;
		packedItem2.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

		bill.PackedItems.Add(packedItem1);
		bill.PackedItems.Add(packedItem2);
		AssertEquals("When all API_GrossWeight are in Kg", bill.GrossWeightInKG, 18m);

		packedItem1.API_GrossWeight = 10;
		packedItem1.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;

		packedItem2.API_GrossWeight = 8467;
		packedItem2.API_GrossWeightUQ = Core.Constants.Weight.Grams;

		bill.PackedItems.Add(packedItem1);
		bill.PackedItems.Add(packedItem2);
		AssertEquals("When some API_GrossWeight are not in Kg", bill.GrossWeightInKG, 18.467m);

		AssertEquals("Decimal places", 6, grossWeight.GetCustomAttribute<DecimalPlacesAttribute>().DecimalPlaces);
	}

	public void TestNetWeightInKG()
	{
		var packedItem1 = bill.PackedItems.AddNew();
		var packedItem2 = bill.PackedItems.AddNew();
		var netWeight = typeof(TemporaryStorageBill).GetProperty(nameof(bill.NetWeightInKG));

		packedItem1.API_NetWeight = 10;
		packedItem1.API_NetWeightUQ = Core.Constants.Weight.Kilograms;

		packedItem2.API_NetWeight = 8;
		packedItem2.API_NetWeightUQ = Core.Constants.Weight.Kilograms;

		bill.PackedItems.Add(packedItem1);
		bill.PackedItems.Add(packedItem2);
		AssertEquals("When all API_NetWeight are in Kg", bill.NetWeightInKG, 18m);

		packedItem1.API_NetWeight = 10;
		packedItem1.API_NetWeightUQ = Core.Constants.Weight.Kilograms;

		packedItem2.API_NetWeight = 8467;
		packedItem2.API_NetWeightUQ = Core.Constants.Weight.Grams;

		bill.PackedItems.Add(packedItem1);
		bill.PackedItems.Add(packedItem2);
		AssertEquals("When some API_NetWeight are not in Kg", bill.NetWeightInKG, 18.467m);

		AssertEquals("Decimal places", 6, netWeight.GetCustomAttribute<DecimalPlacesAttribute>().DecimalPlaces);
	}

	public void TestSupplementaryQuantity()
	{
		var packedItem1 = Factory.New<TemporaryStoragePackedItem>();
		var packedItem2 = Factory.New<TemporaryStoragePackedItem>();
		var supplementaryQuantity = typeof(TemporaryStorageBill).GetProperty(nameof(bill.SuppQuantity));

		packedItem1.API_CustomsUQ2 = Core.Constants.Weight.Kilograms;
		packedItem1.API_CustomsQty2 = 5;

		packedItem2.API_CustomsUQ2 = Core.Constants.Weight.Kilograms;
		packedItem2.API_CustomsQty2 = 12;
		bill.PackedItems.Add(packedItem1);
		bill.PackedItems.Add(packedItem2);
		AssertEquals("SupplementaryQuantity when all API_CustomsQty2 have same UQ", bill.SuppQuantity, 17m);

		packedItem1.API_CustomsUQ2 = Core.Constants.Weight.Kilograms;
		packedItem1.API_CustomsQty2 = 5;

		packedItem2.API_CustomsUQ2 = Core.Constants.Weight.Grams;
		packedItem2.API_CustomsQty2 = 7;

		bill.PackedItems.Add(packedItem1);
		bill.PackedItems.Add(packedItem2);
		AssertEquals("SupplementaryQuantity when some API_CustomsQty2 do not have same UQ", bill.SuppQuantity, 0m);

		AssertEquals("Decimal places", 6, supplementaryQuantity.GetCustomAttribute<DecimalPlacesAttribute>().DecimalPlaces);
	}

	public void TestLrn()
	{
		var lrnEntryNumber = TemporaryStorageTestHelper.CreateCusEntryNumber(bill, CusEntryNumberTypes.Standard.LocalReferenceNumber);
		lrnEntryNumber.CE_EntryNum = "TestLRN123";
		AssertEquals("When LRN is filled", lrnEntryNumber.CE_EntryNum, bill.Lrn);

		lrnEntryNumber.CE_EntryNum = ZString.Empty;
		AssertEquals("When LRN is empty", lrnEntryNumber.CE_EntryNum, bill.Lrn);
	}

	public void TestMrn()
	{
		var mrnEntryNumber = TemporaryStorageTestHelper.CreateCusEntryNumber(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		mrnEntryNumber.CE_EntryNum = "TestMRN";
		AssertEquals("When MRN is filled", mrnEntryNumber.CE_EntryNum, bill.Mrn);

		mrnEntryNumber.CE_EntryNum = ZString.Empty;
		AssertEquals("When MRN is empty", mrnEntryNumber.CE_EntryNum, bill.Mrn);
	}

	public void TestReadOnlyPropertiesWhenCustomsStatusAMG()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		header.CustomsStatus = "AMG";

		CombineAssertions("When Customs status = AMG", () =>
		{
			AssertEquals("UCR Number should be read-only", true, bill.ABL_UCRNumberInfo.ReadOnly);
			AssertEquals("Goods Description should be read-only", true, bill.ABL_GoodsDescriptionInfo.ReadOnly);

			var supplyChainActors = bill.SupplyChainActors.AddNew();
			AssertEquals("SupplyChainActors collection should be read-only", true, bill.SupplyChainActors.ReadOnly);
			AssertEquals("individual SupplyChainActor should be read-only", true, supplyChainActors.ReadOnly);
		});
	}

	public void TestValidationType()
	{
		AssertType<TemporaryStorageBillValidation>(bill.Validation);
	}

	public void TestGetCusSupportingInfoTypes()
	{
		var supportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes();

		CombineAssertions(() =>
		{
			AssertEquals("SupportingInfoTypes Count", 3, supportingInfoTypes.Count);

			AssertEquals("Expected type for AdditionalInfo", typeof(EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			AssertEquals("Expected type for SupportingDocument", typeof(EU.Business.CusTempStorage.TemporaryStorageSupportingDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
			AssertEquals("Expected type for PreviousDocument", typeof(TemporaryStoragePreviousDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
		});
	}

	public void TestTypeOfPreviousDocuments()
	{
		AssertType<EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>>(bill.PreviousDocuments);
	}

	public void TestPackTypeCore()
	{
		AssertType<TemporaryStoragePack>(bill.Packs.AddNew());
	}

	public void TestCanDelete()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var mrnEntryNumber = TemporaryStorageTestHelper.CreateCusEntryNumber(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber);

		mrnEntryNumber.CE_EntryNum = string.Empty;
		Assert("MRN is empty; bill can be deleted.", bill.CanDelete);

		mrnEntryNumber.CE_EntryNum = "MRN12345";
		Assert("MRN is not empty; bill cannot be deleted.", !bill.CanDelete);
	}

	public void TestReasonForNotAbleToDelete()
	{
		AssertEquals("ReasonForNotAbleToDelete", "It is not allowed to remove a bill with the MRN filled.", bill.ReasonForNotAbleToDelete);
	}

	public void TestRegisterHeader()
	{
		var regHeader1 = Factory.New<CusTempStorageRegHeader>();
		regHeader1.SRH_InternalReference = "TS001";
		regHeader1.SRH_PreviousReference = "24ITQYH300268983U7";

		var regHeader2 = Factory.New<CusTempStorageRegHeader>();
		regHeader2.SRH_InternalReference = "TS002";
		regHeader2.SRH_PreviousReference = "24ITQYH300268984U7";

		var regHeader3 = Factory.New<CusTempStorageRegHeader>();
		regHeader3.SRH_InternalReference = "TS002";
		regHeader3.SRH_PreviousReference = "24ITQYH300268985U7";

		AssertNull("When no MRN CusEntryNumber", bill.RegisterHeader);

		var mrnEntryNumber = TemporaryStorageTestHelper.CreateCusEntryNumber(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber);

		mrnEntryNumber.CE_EntryNum = "24ITQYH300268983U7";
		header.AMA_JobReference = ZString.Empty;
		AssertNull("When AMA_JobReference is empty", bill.RegisterHeader);

		header.AMA_JobReference = "TS002";
		AssertNull("When AMA_JobReference is invalid", bill.RegisterHeader);

		header.AMA_JobReference = "TS001";
		mrnEntryNumber.CE_EntryNum = ZString.Empty;
		AssertNull("When Mrn is empty", bill.RegisterHeader);

		mrnEntryNumber.CE_EntryNum = "24ITQYH300268984U7";
		AssertNull("When Mrn is invalid", bill.RegisterHeader);

		header.AMA_JobReference = "TS002";
		CombineAssertions("When AMA_JobReference and Mrn are valid", () =>
		{
			AssertEquals(nameof(bill.RegisterHeader.SRH_InternalReference), "TS002", bill.RegisterHeader.SRH_InternalReference);
			AssertEquals(nameof(bill.RegisterHeader.SRH_PreviousReference), "24ITQYH300268984U7", bill.RegisterHeader.SRH_PreviousReference);
		});
	}

	public void TestRegistrationDate()
	{
		AssertEquals("When no MRN CusEntryNumber", ZDateTime.Empty, bill.RegistrationDate);

		var mrnEntryNumber = TemporaryStorageTestHelper.CreateCusEntryNumber(bill, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		mrnEntryNumber.CE_IssueDate = ZDateTime.BrettsBirthday;
		AssertEquals("When issue date is filled", ZDateTime.BrettsBirthday, bill.RegistrationDate);

		mrnEntryNumber.CE_IssueDate = ZDateTime.Empty;
		AssertEquals("When issue date is empty", ZDateTime.Empty, bill.RegistrationDate);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<TemporaryStorageHeader>();
		bill = header.Bills.AddNew();
	}

	TemporaryStorageHeader header;
	TemporaryStorageBill bill;
}
