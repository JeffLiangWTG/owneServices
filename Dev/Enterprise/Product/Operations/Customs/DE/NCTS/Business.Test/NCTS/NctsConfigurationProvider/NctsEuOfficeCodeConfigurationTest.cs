using System;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsEuOfficeCodeConfiguration))]
	sealed class NctsEuOfficeCodeConfigurationTest : EU.NCTS.Business.Testing.NctsEuOfficeCodeConfigurationAbstractTest
	{
		protected override Type NctsEuOfficeCodeDeparturePhase5ValidationDeciderForTest => typeof(NctsEuOfficeCodeDeparturePhase5ValidationDecider);

		protected override Type NctsEuOfficeCodeArrivalPhase5ValidationDeciderForTest => typeof(NctsEuOfficeCodeArrivalPhase5ValidationDecider);

		public override void TestAutomaticSequenceNumberEnabled()
		{
			AssertEquals(false, configuration.AutomaticSequenceNumberEnabled);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
