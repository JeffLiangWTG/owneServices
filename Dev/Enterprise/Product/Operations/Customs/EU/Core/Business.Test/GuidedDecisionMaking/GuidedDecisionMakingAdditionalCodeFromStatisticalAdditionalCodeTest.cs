using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingAdditionalCodeFromStatisticalAdditionalCode))]
	class GuidedDecisionMakingAdditionalCodeFromStatisticalAdditionalCodeTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var gDMBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			return new GuidedDecisionMakingAdditionalCodeFromStatisticalAdditionalCode(gDMBasic);
		}
	}
}
