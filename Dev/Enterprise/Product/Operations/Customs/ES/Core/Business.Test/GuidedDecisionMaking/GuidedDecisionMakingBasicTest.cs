using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingBasic))]
	sealed class GuidedDecisionMakingBasicTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDestinationStateIsCanaryIsland()
		{
			CombineAssertions(() =>
			{
				var gdmBasic = GuidedDecisionMakingBasicTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory);
				AssertEquals("GDMBasic gets the DestinationStateIsCanaryIsland source, when source false", false, gdmBasic.DestinationStateIsCanaryIsland);

				gdmBasic = GuidedDecisionMakingBasicTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, isDestinationCanaryIsland: true);
				AssertEquals("GDMBasic gets the DestinationStateIsCanaryIsland source, when source true", true, gdmBasic.DestinationStateIsCanaryIsland);
			});
		}

		public void TestVATApplicabilities()
		{
			var guidedDecisionMakingBasic = (GuidedDecisionMakingBasic)GetNewBusinessObject();
			var vats = guidedDecisionMakingBasic.VATApplicabilities;
			AssertType<GuidedDecisionMakingVATCollection>(vats);
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			if (info.Name.Equals("TariffCode"))
			{
				return;
			}
			base.TestBizObjectField(info);
		}

		protected override BusinessObject GetNewBusinessObject() => GuidedDecisionMakingBasicTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory);
	}
}
