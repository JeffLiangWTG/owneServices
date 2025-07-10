using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	class IcsOfficeCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var customsOffice = header.EUCustomsOffices.AddNew();
			AssertContainsExactElementsInAnyOrder("Purpose code list elements",
				new ZString[]
				{
					OfficeCodes_ICS.Codes.OfficeOfActualEntryDiversion,
					OfficeCodes_ICS.Codes.OfficeOfFirstEntry,
					OfficeCodes_ICS.Codes.OfficeOfSubsequentEntry
				},
				customsOffice.Lookups.CY_CodeList.GetAllCodesZString());
		}
	}
}
