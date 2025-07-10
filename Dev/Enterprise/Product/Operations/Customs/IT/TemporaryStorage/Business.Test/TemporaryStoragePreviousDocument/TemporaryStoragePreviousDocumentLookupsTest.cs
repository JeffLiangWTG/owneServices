using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Testing;

sealed class TemporaryStoragePreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestUnitOfQuantityList()
	{
		var lookups = previousDocument.Lookups;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Units of Quantity");

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGMG", "Kilogram Gross", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MTQ", "Cubic meter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TNE", "Tonne", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		Factory.Save();

		AssertEquals("Unit of Quantity List Count", 3, lookups.UnitOfQuantityList.Count);
		CombineAssertions("UnitOfQuantityList should contains these UOMs", () =>
		{
			Assert(lookups.UnitOfQuantityList.ContainsCode("KGMG"));
			Assert(lookups.UnitOfQuantityList.ContainsCode("MTQ"));
			Assert(lookups.UnitOfQuantityList.ContainsCode("TNE"));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
		temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
		storageBill = temporaryStorageHeader.Bills.AddNew();
		previousDocument = storageBill.PreviousDocuments.AddNew();
	}

	TemporaryStorageHeader temporaryStorageHeader;
	TemporaryStoragePreviousDocument previousDocument;
	TemporaryStorageBill storageBill;
}
