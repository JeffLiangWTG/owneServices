using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class InternetAddressRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRule()
		{
			var invalidIPAddressErrorMessage = "Please enter a valid IP address, range, or subnet in CIDR notation. e.g. 169.254.32.102, 192.168.0.0-192.168.1.255, 10.0.0.0/8. IPv6 is also supported.";
			var hasDifferentIPAddressFamilyInIPRangeErrorMessage = "Please make sure that the start IP address and the end IP address of an IP range are of same protocol version.";
			var startIPAddressIsLowerThanEndIPAddressErrorMessage = "Please make sure that the start IP address of an IP range is lower than the end IP address.";

			var rule = new InternetAddressRule();
			rule.Text = "";
			AssertMandatoryValidationError(rule.TextInfo, true);

			rule.Text = "192.168.168.100/33";
			Assert(rule.TextInfo.Notifications.Contains(invalidIPAddressErrorMessage));

			rule.Text = "192.168.0.0/16";
			AssertEquals(IPAddressType.Subnet, rule.AddressType);
			Assert(!rule.TextInfo.HasErrors());

			rule.Text = "192.168.0.1/16";
			AssertEquals(IPAddressType.Subnet, rule.AddressType);
			Assert(!rule.TextInfo.HasErrors());

			rule.Text = "192.168.0.2/16";
			AssertEquals(IPAddressType.Subnet, rule.AddressType);
			Assert(rule.TextInfo.Notifications.Contains(invalidIPAddressErrorMessage));

			rule.Text = "2001:0db8::/129";
			Assert(rule.TextInfo.Notifications.Contains(invalidIPAddressErrorMessage));

			rule.Text = "2001:0db8::/32";
			Assert(!rule.TextInfo.HasErrors());

			rule.Text = "192.168.1.*";
			Assert(rule.TextInfo.Notifications.Contains(invalidIPAddressErrorMessage));

			rule.Text = "192.168.1.1";
			AssertEquals(IPAddressType.Single, rule.AddressType);
			Assert(!rule.TextInfo.HasErrors());

			rule.Text = "2001::0db8::1";
			Assert(rule.TextInfo.Notifications.Contains(invalidIPAddressErrorMessage));

			rule.Text = "2001:0db8::1";
			Assert(!rule.TextInfo.HasErrors());

			rule.Text = "192.168.1.0--192.168.1.*";
			Assert(rule.TextInfo.Notifications.Contains(invalidIPAddressErrorMessage));

			rule.Text = "192.168.1.0-2001:0db8::1";
			Assert(rule.TextInfo.Notifications.Contains(hasDifferentIPAddressFamilyInIPRangeErrorMessage));

			rule.Text = "192.168.1.11-192.168.1.10";
			Assert(rule.TextInfo.Notifications.Contains(startIPAddressIsLowerThanEndIPAddressErrorMessage));

			rule.Text = "192.168.1.0-192.168.1.20";
			AssertEquals(IPAddressType.Range, rule.AddressType);
			Assert(!rule.TextInfo.HasErrors());

			rule.Text = "2001:0db8::1-2001::0db8::1";
			Assert(rule.TextInfo.Notifications.Contains(invalidIPAddressErrorMessage));

			rule.Text = "2001:0db8::1-2001:0db8::F";
			Assert(!rule.TextInfo.HasErrors());
		}
	}
}
