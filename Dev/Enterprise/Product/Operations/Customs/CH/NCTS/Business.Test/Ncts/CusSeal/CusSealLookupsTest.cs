using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class CusSealLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestUnloadedStatesList() => CombineAssertions(() =>
	{
		const string ExpectedCodeListForNotNEW = "DAM, DEC, DIF, MIS";
		const string ExpectedCodeListForNEW = "NEW";

		var nctsHeader = new BusinessObjectFactory().New<NctsHeader>();
		nctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
		var container = nctsHeader.ArrivalHeaderContainers.AddNew();
		var seals = new NctsUnloadedStateList().GetAllCodes().Select(x => AddSeal(x)).ToArray();
		container.Factory.Save();

		container = Factory.Load<NctsArrivalHeaderContainer>(container.PK);
		foreach (var seal in container.Seals.Cast<CusSeal>())
		{
			var expectedCodeList = seal.BK_UnloadingState == NctsUnloadedStateList.Codes.NEW ? ExpectedCodeListForNEW : ExpectedCodeListForNotNEW;
			AssertEquals($"Codes for {seal.BK_UnloadingState}", expectedCodeList, seal.Lookups.UnloadedStates.CodesAsString);
		}

		var sealNotInDB = AddSeal(NctsUnloadedStateList.Codes.NEW);
		AssertEquals("Codes when not in DB", ExpectedCodeListForNEW, sealNotInDB.Lookups.UnloadedStates.CodesAsString);

		var sealInDB = container.Seals.Cast<CusSeal>().First();
		AssertSame("Cached when not NEW", sealInDB.Lookups.UnloadedStates, sealInDB.Lookups.UnloadedStates);
		AssertSame("Cached when NEW", sealNotInDB.Lookups.UnloadedStates, sealNotInDB.Lookups.UnloadedStates);

		CusSeal AddSeal(ZString unloadedState)
		{
			var seal = container.Seals.AddNew();
			seal.BK_UnloadingState = unloadedState;
			seal.BK_SealNumber = "1";
			return seal;
		}
	});
}
