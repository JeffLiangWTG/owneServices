using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityStage))]
	sealed class GlowOpportunityStageTest : CodeDescriptionBoolTest
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new GlowOpportunityStage();
			result.Code = "AAA";
			result.EnglishDescription = "AAA Description";
			result.Bool = true;
			result.WinProbability = 50;

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("AAA", clone.Code);
			AssertEquals("AAA Description", clone.Description);
			AssertEquals(true, ((GlowOpportunityStage)clone).Bool);
			AssertEquals(50, ((GlowOpportunityStage)clone).WinProbability);
		}

		public void TestRunPreSaveValidation()
		{
			TestBizObj.WinProbability = 0;
			TestBizObj.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.WinProbabilityInfo);

			TestBizObj.WinProbability = 123;
			TestBizObj.RunPreSaveValidation();
			AssertHasErrors(TestBizObj.WinProbabilityInfo);

			TestBizObj.WinProbability = 13;
			TestBizObj.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.WinProbabilityInfo);

			TestBizObj.WinProbability = -100;
			TestBizObj.RunPreSaveValidation();
			AssertHasErrors(TestBizObj.WinProbabilityInfo);
		}

		#region Implementation

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new GlowOpportunityStage();
		}

		GlowOpportunityStage TestBizObj
		{
			get { return (GlowOpportunityStage)BizObj; }
		}

		#endregion
	}
}
