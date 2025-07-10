using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationTransportValidationTest : BusinessObjectValidationTestCase
{
	public void TestDischargePortIsMandatory()
	{
		transport.JW_RL_NKDiscPort = "";
		transport.Validation.ValidateJW_RL_NKDiscPort();
		AssertHasMessageErrorContaining("When DischargePort is empty", transport.JW_RL_NKDiscPortInfo, MandatoryValidation.YouHaveNotEntered);

		transport.JW_RL_NKDiscPort = "ITVCE";
		AssertNoMessageErrorContaining("When DischargePort is not empty", transport.JW_RL_NKDiscPortInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestConsecutivelyTransportsHaveSameDischargePortCountry()
	{
		var expectedMessageError = "Several consecutive lines are entered with the same country";
		var transport2 = transportParent.Transports.AddNew();

		CombineAssertions("[PRE-CONDITION] Transport must be Consecutive", () =>
		{
			AssertEquals("Transport1 LegOrder", (byte)1, transport.JW_LegOrder);
			AssertEquals("Transport1 LegOrder", (byte)2, transport2.JW_LegOrder);
		});

		transport.JW_RL_NKDiscPort = "ITVCE";
		transport2.JW_RL_NKDiscPort = "ITTRS";
		AssertHasMessageErrorContaining("When Consecutive Discharge Ports have same country", transport2.JW_RL_NKDiscPortInfo, expectedMessageError);

		transport2.JW_LegOrder = 5;
		transport2.Validation.ValidateJW_RL_NKDiscPort();
		AssertNoMessageErrorContaining("When Discharge Ports are not consecutive", transport2.JW_RL_NKDiscPortInfo, expectedMessageError);

		transport2.JW_LegOrder = 2;
		transport2.JW_RL_NKDiscPort = "ESBCN";
		AssertNoMessageErrorContaining("When Consecutive Discharge Ports have different countries", transport2.JW_RL_NKDiscPortInfo, expectedMessageError);
	}

	public void TestTwoOrMoreTransportsHaveSameLegOrder()
	{
		var expectedMessageError = "Several lines are entered with the same number";

		var transport2 = transportParent.Transports.AddNew();

		transport.JW_LegOrder = 1;
		transport2.JW_LegOrder = 1;
		AssertHasMessageErrorContaining("When Transport uses a duplicated leg order", transport2.JW_LegOrderInfo, expectedMessageError);

		transport2.JW_LegOrder = 2;
		AssertNoMessageErrorContaining("When Transport uses a unique leg order", transport2.JW_LegOrderInfo, expectedMessageError);
	}

	public void TestZeroLegOrder()
	{
		transport.JW_LegOrder = 0;
		AssertHasMessageErrorContaining("When Leg Order is zero", transport.JW_LegOrderInfo, MandatoryValidation.ValueCannotBeZero);

		transport.JW_LegOrder = 1;
		AssertNoMessageErrorContaining("When Leg Order is greater than zero", transport.JW_LegOrderInfo, MandatoryValidation.ValueCannotBeZero);
	}

	public void TestLegOrderAreNotSequential()
	{
		var expectedMessageError = "Numbering is non sequential";

		var transport2 = transportParent.Transports.AddNew();

		transport.JW_LegOrder = 1;
		transport2.JW_LegOrder = 3;
		AssertNoMessageErrorContaining("When Leg Order is 1", transport.JW_LegOrderInfo, expectedMessageError);
		AssertHasMessageErrorContaining("When Leg Order is not sequential", transport2.JW_LegOrderInfo, expectedMessageError);

		transport2.JW_LegOrder = 9;
		var transport3 = transportParent.Transports.AddNew();
		transport3.JW_LegOrder = 10;
		AssertNoMessageErrorContaining("When Leg Order is sequential", transport3.JW_LegOrderInfo, expectedMessageError);
	}

	protected override void SetUp()
	{
		base.SetUp();

		transportParent = Factory.New<JobDeclaration>();
		transport = transportParent.Transports.AddNew();
	}

	Transport transport;
	ITransportParent transportParent;
}
