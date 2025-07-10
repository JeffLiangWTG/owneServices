using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsBillCollection))]
sealed class NctsBillCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsBillCollection>
{
	protected override NctsBillCollection GetCollectionToTest()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		return header.Bills;
	}

	public void TestMaxCount_Departure_DuringTransitionPeriod()
	{
		TestMaxCount(1999, true, NctsMovementType.Codes.Departure);
	}

	public void TestMaxCount_Departure_AfterTransitionPeriod()
	{
		TestMaxCount(1999, false, NctsMovementType.Codes.Departure);
	}

	public void TestMaxCount_Arrival_DuringTransitionPeriod()
	{
		TestMaxCount(1999, true, NctsMovementType.Codes.Arrival);
	}

	public void TestMaxCount_Arrival_AfterTransitionPeriod()
	{
		TestMaxCount(1999, false, NctsMovementType.Codes.Arrival);
	}

	public void TestAllowNewDeparture()
	{
		AssertEquals(true, ((IBindingList)Collection).AllowNew);
	}

	public void TestAllowNewArrival()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertEquals(false, ((IBindingList)header.Bills).AllowNew);
	}

	void TestMaxCount(int expectedMaxCount, bool isNcts5TransitionPeriod, string movementType)
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isNcts5TransitionPeriod))
		{
			var header = CreateNctsHeader(movementType);
			var maxCountValidator = ((ISupportMaxCountValidation)header.Bills).MaxCountValidator;
			AssertEquals($"TransitionPeriod={isNcts5TransitionPeriod} MovementType={movementType}", expectedMaxCount, maxCountValidator.MaxCount);
		}
	}

	NctsHeader CreateNctsHeader(string movementType)
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(movementType);
		return header;
	}
}
