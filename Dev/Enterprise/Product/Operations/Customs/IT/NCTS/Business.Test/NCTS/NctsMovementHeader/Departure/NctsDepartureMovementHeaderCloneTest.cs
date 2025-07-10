using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderCloneTest : TestCaseWithFactory
{
	public void TestCloneUseElectronicFolder()
	{
		movementHeader.UseElectronicFolder = true;
		var clonedMovementHeader = ((NctsHeader)nctsHeader.Clone()).MovementHeader;
		AssertEquals("UseElectronicFolder", true, clonedMovementHeader.UseElectronicFolder);
	}

	public void TestClonePaymentParty()
	{
		movementHeader.PaymentParty = "Z";
		var clonedMovementHeader = ((NctsHeader)nctsHeader.Clone()).MovementHeader;
		AssertEquals("PaymentParty", "Z", clonedMovementHeader.PaymentParty);
	}

	public void TestCloneDefermentAccountNumber()
	{
		movementHeader.DefermentAccountNumber = "XY";
		var clonedMovementHeader = ((NctsHeader)nctsHeader.Clone()).MovementHeader;
		AssertEquals("DefermentAccountNumber", "XY", clonedMovementHeader.DefermentAccountNumber);
	}

	public void TestCloneDefermentAccountNumberRepresentationTypeTriggersDisabled()
	{
		nctsHeader.RepresentationType = "1";
		movementHeader.DefermentAccountNumber = "XY";
		var clonedMovementHeader = ((NctsHeader)nctsHeader.Clone()).MovementHeader;
		AssertEquals("When cloning an NctsHeader, RepresentationType setter-triggers should be disabled and should not override copied value. DefermentAccountNumber", "XY", clonedMovementHeader.DefermentAccountNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
		movementHeader = nctsHeader.MovementHeader;
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;
}
