using System;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsEuOfficeCodeConfiguration))]
sealed class NctsEuOfficeCodeConfigurationTest : NctsEuOfficeCodeConfigurationAbstractTest
{
	protected override Type NctsEuOfficeCodeDeparturePhase5ValidationDeciderForTest => typeof(NctsEuOfficeCodeDeparturePhase5ValidationDecider);

	protected override Type NctsEuOfficeCodeArrivalPhase5ValidationDeciderForTest => typeof(EU.NCTS.Business.NctsEuOfficeCodeArrivalPhase5ValidationDecider);

	public override void TestAutomaticSequenceNumberEnabled()
	{
		AssertEquals(true, configuration.AutomaticSequenceNumberEnabled);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
	}
	NctsHeader header;
}
