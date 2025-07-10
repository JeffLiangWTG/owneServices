using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagRuleThrottlingHeader))]
	public class TagRuleThrottlingHeaderTest : RegistryBusinessObjectTemplateTestCase<TagRuleThrottlingHeader>
	{
		public void TestValidateIsThrottlingEnabled()
		{
			var header = NewPopulatedBusinessObject();

			header.IsThrottlingEnabled = false;
			AssertNoErrors(header.IsThrottlingEnabledInfo);

			header.IsThrottlingEnabled = true;
			AssertNoErrors(header.IsThrottlingEnabledInfo);
		}

		public void TestValidateThresholds()
		{
			var header = new TagRuleThrottlingHeaderForTest();
			header.RunPreSaveValidation_Exposed();

			AssertEquals(false, header.HasErrors);

			header.ThresholdCollection.AddNew(15, 30);
			header.RunPreSaveValidation_Exposed();

			AssertEquals(false, header.HasErrors);

			header.ThresholdCollection.AddNew(0, 0);

			AssertExceptionThrown<RegistryValidationException>(header.RunPreSaveValidation_Exposed);
			AssertEquals(true, header.HasErrors);
		}

		public void TestDefaultRegistryItem_ShouldHaveThrottlingEnabled()
		{
			var item = BMSRegistry.Instance.TagRuleThrottling.DefaultValue;

			AssertEquals(true, item.IsThrottlingEnabled);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override TagRuleThrottlingHeader GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override TagRuleThrottlingHeader GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		TagRuleThrottlingHeader NewPopulatedBusinessObject()
		{
			return new TagRuleThrottlingHeader(NewFallbackLevel(), Factory);
		}

		class TagRuleThrottlingHeaderForTest : TagRuleThrottlingHeader
		{
			public void RunPreSaveValidation_Exposed()
			{
				RunPreSaveValidation();
			}
		}

		#endregion
	}
}
