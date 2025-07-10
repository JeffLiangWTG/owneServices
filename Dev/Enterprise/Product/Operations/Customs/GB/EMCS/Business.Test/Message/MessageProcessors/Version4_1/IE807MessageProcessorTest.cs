using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie807;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE807MessageProcessor))]
	sealed class IE807MessageProcessorTest : IE807MessageProcessorAbstractTest<Ie807Type>
	{
		protected override void UpdateAdministrativeReferenceCode()
		{
			ie807.Body.InterruptionOfMovement.Attributes.AdministrativeReferenceCode = "MRN98761236";
		}

		protected override void UpdateComplementaryInformation()
		{
			ie807.Body.InterruptionOfMovement.Attributes.ComplementaryInformation = new LsdComplementaryInformationType();
		}

		protected override void UpdateReferenceControlReport()
		{
			ie807.Body.InterruptionOfMovement.ReferenceControlReport = new System.Collections.ObjectModel.Collection<ReferenceControlReportType>();
		}

		protected override void UpdateReferenceEventReport()
		{
			ie807.Body.InterruptionOfMovement.ReferenceEventReport = new System.Collections.ObjectModel.Collection<ReferenceEventReportType>();
		}

		protected override Ie807Type CreateDefaultIE807Type()
		{
			return EMCSMessageProcessorTestHelper.GetStandardIE807Type();
		}
	}
}
