using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class AddInfoCusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestParticipantTypeList()
	{
		CombineAssertions(() =>
		{
			var addInfoLookups = GetNewAddInfoCusEntryInstruction().Lookups;
			AssertType<ParticipantTypeList>("Type", addInfoLookups.ParticipantTypeList);
			AssertSame("Cache", addInfoLookups.ParticipantTypeList, addInfoLookups.ParticipantTypeList);
		});
	}

	public void TestCurrencyList()
	{
		var addInfoLookups = GetNewAddInfoCusEntryInstruction().Lookups;
		AssertType<RefCurrencyCollection>("Type", addInfoLookups.CurrencyList);
	}

	AddInfoCusEntryInstruction GetNewAddInfoCusEntryInstruction() => new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>());
}
