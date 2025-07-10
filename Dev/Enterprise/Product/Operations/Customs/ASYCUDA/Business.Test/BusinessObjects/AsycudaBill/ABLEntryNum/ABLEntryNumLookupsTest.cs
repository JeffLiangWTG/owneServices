using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class ABLEntryNumLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsEntryNumberTypes()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "Customs Entry Number Types");
			var pRV = helper.CreateNewOrGetExistingCusCodeList("ER", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "PRV", "Customs Entry Number Types", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNumber = bill.CustomsEntryNumbers.AddNew();

			AssertEquals(true, entryNumber.Lookups.CustomsEntryNumberTypes.ContainsCode("PRV"));
			AssertEquals(entryNumber.CE_EntryType, entryNumber.Lookups.CustomsEntryNumberTypes.GetAllCodes()[0]);
		}
	}
}
