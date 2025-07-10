using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE881MessageProcessor))]
	sealed class IE881MessageProcessorTest : IE881MessageProcessorAbstractTest<Ie881Type>
	{
		protected override void UpdateAdministrativeReferenceCode()
		{
			ie881.Body.ManualClosureResponse.Attributes.AdministrativeReferenceCode = "MRN98761236";
		}

		protected override void UpdateRequestRejectedReason2_1()
		{
			ie881.Body.ManualClosureResponse.Attributes.ManualClosureRequestAccepted = Flag.Item0;
			ie881.Body.ManualClosureResponse.Attributes.ManualClosureRejectionReasonCode = "1";
		}

		protected override void UpdateRejectedReason2_0()
		{
			var complementReason = "Just because we can muahahaha!";
			ie881.Body.ManualClosureResponse.Attributes.ManualClosureRequestAccepted = Flag.Item0;
			ie881.Body.ManualClosureResponse.Attributes.ManualClosureRejectionReasonCode = "0";
			ie881.Body.ManualClosureResponse.Attributes.ManualClosureRejectionComplement = new LsdManualClosureRejectionComplementType() { Language = "en", Value = complementReason };
		}

		protected override Ie881Type CreateDefaultIE881Type()
		{
			return EMCSMessageProcessorTestHelper.GetStandardIE881Type();
		}
	}
}
