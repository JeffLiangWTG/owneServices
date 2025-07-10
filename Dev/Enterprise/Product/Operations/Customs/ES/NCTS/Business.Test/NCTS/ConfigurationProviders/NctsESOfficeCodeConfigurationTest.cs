using System;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsESOfficeCodeConfiguration))]
	sealed class NctsESOfficeCodeConfigurationTest : EU.NCTS.Business.Testing.NctsEuOfficeCodeConfigurationAbstractTest
	{
		protected override Type NctsEuOfficeCodeDeparturePhase5ValidationDeciderForTest => typeof(NctsESOfficeCodeDeparturePhase5ValidationDecider);

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
