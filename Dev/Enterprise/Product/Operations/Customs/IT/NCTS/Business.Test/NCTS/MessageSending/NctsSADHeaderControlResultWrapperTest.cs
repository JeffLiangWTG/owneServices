using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSADHeaderControlResultWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when nctsDepartureMovement is null", () => new NctsSADHeaderControlResultWrapper(null));
	}

	public void TestDateLimitOfArrivalNotification()
	{
		nctsMovementHeader.BM_ExportDate = new ZDate(2021, 03, 03);
		AssertEquals("DateLimitOfArrivalNotification", new ZDate(2021, 03, 03), headerControlResultWrapper.DateLimitOfArrivalNotification);
	}

	public void TestDateLimitForTheExitFromEC()
	{
		AssertEquals("DateLimitForTheExitFromEC must be empty", ZDate.Empty, headerControlResultWrapper.DateLimitForTheExitFromEC);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsMovementHeader = nctsHeader.MovementHeader;
		headerControlResultWrapper = new NctsSADHeaderControlResultWrapper(nctsMovementHeader);
	}
	NctsDepartureMovementHeader nctsMovementHeader;
	NctsSADHeaderControlResultWrapper headerControlResultWrapper;
}
