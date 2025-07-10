using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InternetAddressRuleset))]
	sealed class InternetAddressRulesetTest : RegistryBusinessObjectCollectionTemplateTestCase<InternetAddressRuleset>
	{
		#region GetEnabledRules

		public void TestGetEnabledRules()
		{
			var ruleset = new InternetAddressRuleset();
			var rule1 = ruleset.AddNew();
			rule1.Text = "192.168.1.1";
			rule1.Enabled = true;

			var rule2 = ruleset.AddNew();
			rule2.Text = "192.168.1.2";
			rule2.Enabled = false;

			var enabledRuleset = ruleset.GetEnabledRules();
			AssertEquals(1, enabledRuleset.Count());
			AssertEquals("192.168.1.1", enabledRuleset.First().Text);
			AssertNotEquals("192.168.1.2", enabledRuleset.First().Text);
		}

		#endregion

		#region AllowNew

		public void TestAllowNew()
		{
			Assert(Collection.AllowNew);
		}

		#endregion

		#region Contains

		public void TestContains()
		{
			var ruleset = new InternetAddressRuleset();

			Assert(!ruleset.Contains("192.168.1.1 "));
			Assert(!ruleset.Contains("192.168.1.256"));
			Assert(!ruleset.Contains("192.168.1.*"));
			Assert(!ruleset.Contains("0.168.285.1"));
			Assert(!ruleset.Contains("192.168.1.*"));
			Assert(!ruleset.Contains("300.168.285.1"));
			Assert(!ruleset.Contains("0.168.285."));
			Assert(!ruleset.Contains("2001::0db8::1"));
			Assert(!ruleset.Contains("2001:0dg::1"));

			var rule = ruleset.AddNew();
			rule.Text = "192.168.0.0/24";
			rule.Enabled = true;

			Assert("should not contains 192.168.1.1 anymore", !ruleset.Contains("192.168.1.1"));
			Assert("should not contains 2001:0db8::1 anymore", !ruleset.Contains("2001:0db8::1"));

			Assert("should contains 192.168.0.100", ruleset.Contains("192.168.0.100"));
			Assert("should not contains 192.168.1.100", !ruleset.Contains("192.168.1.100"));

			rule = ruleset.AddNew();
			rule.Text = "192.168.1.1-192.168.1.254";
			Assert("should not contains 192.168.1.100", !ruleset.Contains("192.168.1.100"));

			rule.Enabled = true;
			Assert("should contains 192.168.1.100 now", ruleset.Contains("192.168.1.100"));

			rule = ruleset.AddNew();
			rule.Text = "2001:0db8::/64";
			rule.Enabled = true;
			Assert("should contains 2001:0db8::1 now", ruleset.Contains("2001:0db8::1"));
			Assert("should contains 2001:0DB8::1 now", ruleset.Contains("2001:0DB8::1"));
			Assert("should not contains 2001:0db9::1", !ruleset.Contains("2001:0db9::1"));

			rule = ruleset.AddNew();
			rule.Text = "2001:0db9::1";
			rule.Enabled = true;
			Assert("should contains 2001:0db9::1 now", ruleset.Contains("2001:0db9::1"));

			rule = ruleset.AddNew();
			rule.Text = "2001:0db9::1-2001:0db9::F";
			rule.Enabled = true;
			Assert("should contains 2001:0db9::4", ruleset.Contains("2001:0db9::4"));

			rule = ruleset.AddNew();
			rule.Text = "::1";
			rule.Enabled = true;
			Assert(ruleset.Contains("::1"));
			Assert(ruleset.Contains("::0001"));
			Assert(ruleset.Contains("0000:00:0::1"));
		}

		#endregion

		#region Overrides

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override InternetAddressRuleset GetCollectionToTest()
		{
			return new InternetAddressRuleset();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InternetAddressRule();
		}

		#endregion
	}
}
