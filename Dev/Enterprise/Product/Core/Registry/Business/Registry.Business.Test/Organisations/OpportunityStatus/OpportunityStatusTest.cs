using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OpportunityStatus))]
	sealed class OpportunityStatusTest : CodeDescriptionBoolTest
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new OpportunityStatus();
			result.Code = "AAA";
			result.EnglishDescription = "AAA Description";
			result.Bool = true;
			result.EffectiveAgreement = true;
			result.Enabled = true;

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("AAA", clone.Code);
			AssertEquals("AAA Description", clone.Description);
			AssertEquals(true, ((OpportunityStatus)clone).Bool);
			AssertEquals(true, ((OpportunityStatus)clone).EffectiveAgreement);
			AssertEquals(true, ((OpportunityStatus)clone).Enabled);
		}

		public void TestRunPreSaveValidation()
		{
			TestBizObj.Bool = false;
			TestBizObj.EffectiveAgreement = true;
			TestBizObj.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.BoolInfo);
			AssertHasErrors(TestBizObj.EffectiveAgreementInfo);

			TestBizObj.Bool = true;
			TestBizObj.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.BoolInfo);
			AssertNoErrors(TestBizObj.EffectiveAgreementInfo);

			TestBizObj.Bool = false;
			TestBizObj.RunPreSaveValidation();
			AssertHasErrors(TestBizObj.BoolInfo);
			AssertNoErrors(TestBizObj.EffectiveAgreementInfo);
		}

		public void TestValidateEffectiveAgreement()
		{
			TestBizObj.Bool = false;
			TestBizObj.EffectiveAgreement = true;
			AssertHasErrors(TestBizObj.EffectiveAgreementInfo);

			TestBizObj.Bool = true;
			TestBizObj.EffectiveAgreement = true;
			AssertNoErrors(TestBizObj.EffectiveAgreementInfo);
		}

		public void TestValidateBool()
		{
			TestBizObj.Bool = true;
			TestBizObj.EffectiveAgreement = true;
			AssertNoErrors(TestBizObj.BoolInfo);

			TestBizObj.Bool = false;
			AssertHasErrors(TestBizObj.BoolInfo);
		}

		public void TestValidateValueAnalysisConversionStatus()
		{
			TestBizObj.TradeStatus = "AAA";
			AssertHasErrors(TestBizObj.TradeStatusInfo);

			TestBizObj.TradeStatus = OpportunityTradeStatus.Codes.Successful;
			AssertNoErrors(TestBizObj.TradeStatusInfo);
		}

		#region Implementation

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new OpportunityStatus();
		}

		OpportunityStatus TestBizObj
		{
			get { return (OpportunityStatus)BizObj; }
		}

		#endregion
	}
}
