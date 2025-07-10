using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;
namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(ValidationRuleConfiguration))]
	sealed class ValidationRuleConfigurationTest : ValidationRuleConfigurationAbstractTest<ValidationRuleConfiguration>
	{
		public override void TestIsRuleB1811Active()
		{
			AssertEquals(false, configuration.IsRuleB1811Active);
		}

		public override void TestIsRuleB1820_1Active()
		{
			AssertEquals(true, configuration.IsRuleB1820_1Active);
		}

		public override void TestIsRuleB1848_1Active()
		{
			AssertEquals(true, configuration.IsRuleB1848_1Active);
		}

		public override void TestIsRuleC0030Active()
		{
			AssertEquals(false, configuration.IsRuleC0030Active);
		}

		public override void TestIsRuleC0186Active()
		{
			AssertEquals(false, configuration.IsRuleC0186Active);
		}

		public override void TestIsRuleC0337Active()
		{
			AssertEquals(false, configuration.IsRuleC0337Active);
		}

		public override void TestIsRuleC0382Active()
		{
			AssertEquals(false, configuration.IsRuleC0382Active);
		}

		public override void TestIsRuleC0394Active()
		{
			AssertEquals(false, configuration.IsRuleC0394Active);
		}

		public override void TestIsRuleC0505Active()
		{
			AssertEquals(false, configuration.IsRuleC0505Active);
		}

		public override void TestIsRuleC0542Active()
		{
			AssertEquals(false, configuration.IsRuleC0542Active);
		}

		public override void TestIsRuleC0839Active()
		{
			AssertEquals(false, configuration.IsRuleC0839Active);
		}

		public override void TestIsRuleE1102Active()
		{
			AssertEquals(false, configuration.IsRuleE1102Active);
		}

		public override void TestIsRuleE1401_1Active()
		{
			AssertEquals(true, configuration.IsRuleE1401_1Active);
		}

		public override void TestIsRuleNR0002Active()
		{
			AssertEquals(true, configuration.IsRuleNR0002Active);
		}

		public override void TestIsRuleR0076Active()
		{
			AssertEquals(true, configuration.IsRuleR0076Active);
		}

		public override void TestIsRuleR0350Active()
		{
			AssertEquals(false, configuration.IsRuleR0350Active);
		}

		public override void TestIsRuleR0850Active()
		{
			AssertEquals(false, configuration.IsRuleR0850Active);
		}

		public override void TestIsRuleR0850_1Active()
		{
			AssertEquals(true, configuration.IsRuleR0850_1Active);
		}

		public override void TestIsRuleTR0052Active()
		{
			AssertEquals(true, configuration.IsRuleTR0052Active);
		}

		public override void TestIsRuleTR0084Active()
		{
			AssertEquals(false, configuration.IsRuleTR0084Active);
		}
	}
}
