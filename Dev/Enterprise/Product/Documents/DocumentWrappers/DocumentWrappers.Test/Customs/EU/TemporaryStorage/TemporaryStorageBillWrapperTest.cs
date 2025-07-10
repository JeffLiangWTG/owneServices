using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.Testing;

[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
sealed class TemporaryStorageBillWrapperTest : DocBaseWrapperTest
{
	public void TestBillNumber()
	{
		bill.ABL_BillNumber = "001122";
		AssertEquals("BillNumber", "001122", Wrapper.BillNumber);
	}

	public void TestConsignor()
	{
		SetUpAndAssertOrganization(nameof(Wrapper.Consignor), x => x.ABL_OA_ShipperInfo, x => x.Consignor);
	}

	public void TestConsignee()
	{
		SetUpAndAssertOrganization(nameof(Wrapper.Consignee), x => x.ABL_OA_ConsigneeInfo, x => x.Consignee);
	}

	public void TestNotifyParty()
	{
		SetUpAndAssertOrganization(nameof(Wrapper.NotifyParty), x => x.ABL_OA_NotifyPartyInfo, x => x.NotifyParty);
	}

	public void TestMarksAndNumbersDefaultEmpty()
	{
		AssertEquals("MarksAndNumbers", "", Wrapper.MarksAndNumbers);
	}

	public void TestMarksAndNumbers()
	{
		var packedItem = bill.PackedItems.AddNew();

		var pack1 = bill.Packs.AddNew();
		pack1.APA_MarksAndNumbers = "Mark 1";
		var linkPackage1 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage1.Package = pack1;
		linkPackage1.IsLinked = true;

		var pack2 = bill.Packs.AddNew();
		pack2.APA_MarksAndNumbers = "Mark 2";
		var linkPackage2 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage2.Package = pack2;
		linkPackage2.IsLinked = true;

		var pack3 = bill.Packs.AddNew();
		pack3.APA_MarksAndNumbers = "Mark 3";
		var linkPackage3 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage3.Package = pack3;
		linkPackage3.IsLinked = false;

		AssertEquals("MarksAndNumbers", "Mark 1, Mark 2", Wrapper.MarksAndNumbers);
	}

	public void TestPackQuantityDefaultEmpty()
	{
		AssertEquals("PackQuantity", 0, Wrapper.PackQuantity);
	}

	public void TestPackQuantity()
	{
		var packedItem = bill.PackedItems.AddNew();

		var pack1 = bill.Packs.AddNew();
		pack1.APA_PackQty = 10;
		var linkPackage1 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage1.Package = pack1;
		linkPackage1.IsLinked = true;

		var pack2 = bill.Packs.AddNew();
		pack2.APA_PackQty = 20;
		var linkPackage2 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage2.Package = pack2;
		linkPackage2.IsLinked = true;

		var pack3 = bill.Packs.AddNew();
		pack3.APA_PackQty = 30;
		var linkPackage3 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage3.Package = pack3;
		linkPackage3.IsLinked = false;

		AssertEquals("PackQuantity", 30, Wrapper.PackQuantity);
	}

	public void TestPackQuantityUnitDefaultEmpty()
	{
		AssertEquals("PackQuantityUnit", "", Wrapper.PackQuantityUnit);
	}

	public void TestPackQuantityUnitWhenAllUnitsAreEqual()
	{
		var packedItem = bill.PackedItems.AddNew();

		var pack1 = bill.Packs.AddNew();
		pack1.APA_PackUQ = "KG";
		var linkPackage1 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage1.Package = pack1;
		linkPackage1.IsLinked = true;

		var pack2 = bill.Packs.AddNew();
		pack2.APA_PackUQ = "KG";
		var linkPackage2 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage2.Package = pack2;
		linkPackage2.IsLinked = true;

		var pack3 = bill.Packs.AddNew();
		pack3.APA_PackUQ = "T";
		var linkPackage3 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage3.Package = pack3;
		linkPackage3.IsLinked = false;

		AssertEquals("PackQuantityUnit", "KG", Wrapper.PackQuantityUnit);
	}

	public void TestPackQuantityUnitWhenNotAllUnitsAreEqual()
	{
		var packedItem = bill.PackedItems.AddNew();

		var pack1 = bill.Packs.AddNew();
		pack1.APA_PackUQ = "KG";
		var linkPackage1 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage1.Package = pack1;
		linkPackage1.IsLinked = true;

		var pack2 = bill.Packs.AddNew();
		pack2.APA_PackUQ = "T";
		var linkPackage2 = packedItem.TemporaryStorageLinkPackages.AddNew();
		linkPackage2.Package = pack2;
		linkPackage2.IsLinked = true;

		AssertEquals("PackQuantityUnit", "", Wrapper.PackQuantityUnit);
	}

	public void TestGrossWeightInKilogramsDefaultEmpty()
	{
		AssertEquals("GrossWeightInKilograms", 0m, Wrapper.GrossWeightInKilograms);
	}

	public void TestGrossWeightInKilograms()
	{
		var packedItem1 = bill.PackedItems.AddNew();
		packedItem1.API_GrossWeight = 0.1234567m;
		packedItem1.API_GrossWeightUQ = "G";
		var packedItem2 = bill.PackedItems.AddNew();
		packedItem2.API_GrossWeight = 1m;
		packedItem2.API_GrossWeightUQ = "T";
		AssertEquals("GrossWeightInKilograms", 1000.000123m, Wrapper.GrossWeightInKilograms);
	}

	public void TestMrn()
	{
		AssertEquals("Mrn", "", Wrapper.Mrn);

		bill.Header.MRN = "MRN123";
		AssertEquals("Mrn", "MRN123", Wrapper.Mrn);
	}

	new TemporaryStorageBillWrapper Wrapper => (TemporaryStorageBillWrapper)base.Wrapper;

	protected override DocBaseWrapper GetNewDocumentWrapper() => TemporaryStorageBillWrapper.New(bill, Factory);

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<TemporaryStorageHeader>();
		bill = header.Bills.AddNew();
	}

	TemporaryStorageBill bill;

	void SetUpAndAssertOrganization(string message, Func<TemporaryStorageBill, ZPropertyInfo> bizObjPtyInfo, Func<TemporaryStorageBillWrapper, ZString> wrapperPty)
	{
		const string expectedWrapperValue =
			"Company Name\r\n" +
			"Company Address1\r\n" +
			"Company Address2\r\n" +
			"12345\r\n" +
			"Company City\r\n" +
			"IT";

		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Company Name";
		orgHeader.MainAddress.OA_Address1 = "Company Address1";
		orgHeader.MainAddress.OA_Address2 = "Company Address2";
		orgHeader.MainAddress.OA_PostCode = "12345";
		orgHeader.MainAddress.OA_City = "Company City";
		orgHeader.MainAddress.OA_RN_NKCountryCode = "IT";

		bizObjPtyInfo(bill).Value = orgHeader.MainAddress.PK;
		AssertEquals(message, expectedWrapperValue, wrapperPty(Wrapper));
	}
}
