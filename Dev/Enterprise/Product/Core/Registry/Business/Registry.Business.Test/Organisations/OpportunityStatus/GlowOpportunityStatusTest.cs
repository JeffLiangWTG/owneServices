using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityStatus))]
	sealed class GlowOpportunityStatusTest : CodeDescriptionBoolTest
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new GlowOpportunityStatus();
			result.Code = "AAA";
			result.EnglishDescription = "AAA Description";
			result.Bool = true;

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("AAA", clone.Code);
			AssertEquals("AAA Description", clone.Description);
			AssertEquals(true, ((GlowOpportunityStatus)clone).Bool);
		}

		public void TestDefaultTradeStatusIsSetToActive()
		{
			var result = new GlowOpportunityStatus();
			AssertEquals(result.TradeStatus, OpportunityTradeStatus.Codes.Active);
		}

		public void TestValidateValueAnalysisConversionStatus()
		{
			TestBizObj.TradeStatus = "AAA";
			AssertHasErrors(TestBizObj.TradeStatusInfo);

			TestBizObj.TradeStatus = OpportunityTradeStatus.Codes.Successful;
			AssertNoErrors(TestBizObj.TradeStatusInfo);

			TestBizObj.TradeStatus = ZString.Empty;
			AssertHasError(TestBizObj.TradeStatusInfo, "Please enter a value.");

			TestBizObj.TradeStatus = OpportunityTradeStatus.Codes.Unsuccessful;
			AssertNoErrors(TestBizObj.TradeStatusInfo);

			TestBizObj.TradeStatus = OpportunityTradeStatus.Codes.Active;
			AssertNoErrors(TestBizObj.TradeStatusInfo);
		}

		public void TestRunPreSaveValidation()
		{
			TestBizObj.Code = "";
			TestBizObj.EnglishDescription = "";
			TestBizObj.RunPreSaveValidation();
			AssertHasErrors(TestBizObj.CodeInfo);
			AssertHasErrors(TestBizObj.DescriptionInfo);

			TestBizObj.Code = "TST";
			TestBizObj.EnglishDescription = "";
			TestBizObj.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.CodeInfo);
			AssertHasErrors(TestBizObj.DescriptionInfo);

			TestBizObj.Code = "";
			TestBizObj.EnglishDescription = "Test Description";
			TestBizObj.RunPreSaveValidation();
			AssertHasErrors(TestBizObj.CodeInfo);
			AssertNoErrors(TestBizObj.DescriptionInfo);

			TestBizObj.Code = "TST";
			TestBizObj.EnglishDescription = "Test Description";
			TestBizObj.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.CodeInfo);
			AssertNoErrors(TestBizObj.DescriptionInfo);
		}

		#region Implementation

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new GlowOpportunityStatus();
		}

		GlowOpportunityStatus TestBizObj
		{
			get { return (GlowOpportunityStatus)BizObj; }
		}

		#endregion
	}
}
