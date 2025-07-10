using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OutboundSendLimitsRule))]
	sealed class OutboundSendLimitsRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("SendCountLimitDefault", (short)5, BizObj.SendCountLimit);
			AssertEquals("SendSizeLimitDefault", 1024, BizObj.SendSizeLimit);
		}

		public void TestRangeValidation()
		{
			BizObj.SendCountLimit = 0;
			AssertHasError(BizObj.SendCountLimitInfo, string.Format("Please enter a '{0}' within the range 1 to 9999.", BizObj.SendCountLimitInfo.HumanReadableName));
			BizObj.SendCountLimit = 1;
			AssertNoErrors(BizObj.SendCountLimitInfo);
			BizObj.SendCountLimit = 10000;
			AssertHasError(BizObj.SendCountLimitInfo, string.Format("Please enter a '{0}' within the range 1 to 9999.", BizObj.SendCountLimitInfo.HumanReadableName));

			BizObj.SendSizeLimit = 0;
			AssertHasError(BizObj.SendSizeLimitInfo, string.Format("Please enter a '{0}' within the range 1 to 999999.", BizObj.SendSizeLimitInfo.HumanReadableName));
			BizObj.SendSizeLimit = 1;
			AssertNoErrors(BizObj.SendSizeLimitInfo);
			BizObj.SendSizeLimit = 1000000;
			AssertHasError(BizObj.SendSizeLimitInfo, string.Format("Please enter a '{0}' within the range 1 to 999999.", BizObj.SendSizeLimitInfo.HumanReadableName));
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			OutboundSendLimitsRule result = new OutboundSendLimitsRule();

			result.SendCountLimit = 5;
			result.SendSizeLimit = 1024;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			OutboundSendLimitsRule result = new OutboundSendLimitsRule();

			result.SendCountLimit = 5;
			result.SendSizeLimit = 1024;

			return result;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new OutboundSendLimitsRule BizObj
		{
			get { return (OutboundSendLimitsRule)base.BizObj; }
		}

		#endregion
	}
}
