using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.NctsMovementHeader.Departure;

sealed class TransportAtBorderTypeOfIdLookupsHelperTest : TestCaseWithFactory
{
	public void TestGuardClause()
	{
		AssertExceptionThrown<ArgumentNullException>("When header is null", () => TransportAtBorderTypeOfIdLookupsHelper.GetCachedTransportAtBorderTypeOfIdList(null));
	}

	public void TestListForPhase4()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		var list = TransportAtBorderTypeOfIdLookupsHelper.GetCachedTransportAtBorderTypeOfIdList(movementHeader);

		AssertEquals("Expected codes should be same", "10, 11, 21, 30, 40, 41, 80, 81, 99", list.CodesAsString);
		AssertSame("Cached", list, TransportAtBorderTypeOfIdLookupsHelper.GetCachedTransportAtBorderTypeOfIdList(movementHeader));
	}

	public void TestListForPhase5()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		CombineAssertions(() =>
		{
			TestCase("1", "10, 11, 99");
			TestCase("2", "21, 99");
			TestCase("3", "30, 99");
			TestCase("4", "40, 41, 99");
			TestCase("5", "");
			TestCase("6", "");
			TestCase("7", "10, 11, 21, 30, 40, 41, 80, 81, 99");
			TestCase("8", "80, 81, 99");
			TestCase("9", "10, 11, 21, 30, 40, 41, 80, 81, 99");
		});

		void TestCase(string exportTransportMode, string expectedLookupsList)
		{
			using (TemporarilySetTransitionPeriod(true))
			{
				nctsHeader.MovementHeader.BM_ExportTransportMode = exportTransportMode;
				var list = TransportAtBorderTypeOfIdLookupsHelper.GetCachedTransportAtBorderTypeOfIdList(movementHeader);
				AssertEquals($"exportTransportMode = {exportTransportMode}", expectedLookupsList, list.CodesAsString);
				AssertSame($"Cached for exportTransportMode = {exportTransportMode}", list, TransportAtBorderTypeOfIdLookupsHelper.GetCachedTransportAtBorderTypeOfIdList(movementHeader));
			}

			using (TemporarilySetTransitionPeriod(false))
			{
				var list = TransportAtBorderTypeOfIdLookupsHelper.GetCachedTransportAtBorderTypeOfIdList(movementHeader);
				AssertCollectionNotContains("code 99 should not be present", "99", list.GetAllCodes());
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
	}

	IDisposable TemporarilySetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
}
