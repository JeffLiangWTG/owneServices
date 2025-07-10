using System;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsIEOfficeCodeConfiguration))]
	sealed class NctsIEOfficeCodeConfigurationTest : NctsEuOfficeCodeConfigurationAbstractTest
	{
		protected override Type NctsEuOfficeCodeDeparturePhase5ValidationDeciderForTest => typeof(NctsIEOfficeCodeDeparturePhase5ValidationDecider);

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
}
