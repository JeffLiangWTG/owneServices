using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie871;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing.Version4_1
{
	[TestedType(typeof(IE871MessageProcessor))]
	sealed class IE871MessageProcessorTest : IE871MessageProcessorAbstractTest<Ie871Type>
	{
		protected override void UpdateAdministrativeReferenceCode()
		{
			ie871.Body.ExplanationOnReasonForShortage.ExciseMovement.AdministrativeReferenceCode = "MRN98761236";
		}

		protected override void UpdateGlobalExplanation()
		{
			ie871.Body.ExplanationOnReasonForShortage.Analysis.GlobalExplanation = new LsdGlobalExplanationType();
		}

		protected override void UpdateBodyAnalysis()
		{
			ie871.Body.ExplanationOnReasonForShortage.BodyAnalysis = new System.Collections.ObjectModel.Collection<BodyAnalysisType>();
		}

		protected override Ie871Type CreateDefaultIE871Type()
		{
			return EMCSMessageProcessorTestHelper.GetStandardIE871Type();
		}
	}
}
