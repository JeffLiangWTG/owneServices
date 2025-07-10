using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityStageCollection))]
	sealed class GlowOpportunityStageCollectionTest : CodeDescriptionBoolCollectionAbstractTest<GlowOpportunityStageCollection>
	{
		public void TestMaxLength()
		{
			AssertEquals("The collection code max length does not match OrgOpportunitySchema.P8_Stage.MaxLength. Please update it and consider if a transformation is required.", OrgOpportunitySchema.P8_Stage.MaxLength, GetCollectionToTest().CodeMaxLength);
		}

		#region Implementation

		public override void TestDefaultBoolForNewChild()
		{
			AssertEquals(true, new GlowOpportunityStageCollection().AddNew().Bool);
		}

		protected override GlowOpportunityStageCollection GetCollectionToTest()
		{
			return new GlowOpportunityStageCollection();
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GlowOpportunityStage();
		}

		#endregion
	}
}
