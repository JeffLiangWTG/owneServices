using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie840;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE840MessageProcessor))]
	sealed class IE840MessageProcessorTest : IE840MessageProcessorAbstractTest<Ie840Type>
	{
		protected override Ie840Type CreateDefaultIE840Type()
		{
			return EMCSMessageProcessorTestHelper.GetStandardIE840Type();
		}

		protected override void UpdateAdministrativeRefCode()
		{
			ie840.Body.EventReportEnvelope.ExciseMovement.AdministrativeReferenceCode = "MRN98761236";
		}
	}
}
