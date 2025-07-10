using System.Linq;

using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	internal class FraudPreventionDataProviderTest : TestCaseWithFactory
	{
		[TestDate(2020, 9, 21, 14, 30, 05, 123)]
		public void TestFraudPreventionDataProviderGet()
		{
			var fraudPreventionDataProvider = MTDTestHelper.SetUpFraudPreventionDataProviderForTest();
			var fraudPreventionData = fraudPreventionDataProvider.Get().ToArray();

			AssertEquals(14, fraudPreventionData.Length);

			var expected = new[]
			{
				// Connection Method
				new { Key = "Gov-Client-Connection-Method", Value = "DESKTOP_APP_DIRECT" },
				// System Management Data
				new { Key = "Gov-Client-Device-ID", Value = "0A61AF61-CFED-4102-AFBD-A65573EADFAA" },
				new { Key = "Gov-Client-Timezone", Value = "UTC+10:00" },
				new { Key = "Gov-Client-User-Agent", Value = "os-family=Operating%20System%2099&os-version=99.9.99999&device-manufacturer=WiseTechGlobal&device-model=The%20Ultimate%20Machine" },
				// Network Data
				new { Key = "Gov-Client-Local-IPs", Value = "192.168.0.1,10.61.220.57" },
				new { Key = "Gov-Client-Local-IPs-Timestamp", Value = "2020-09-21T02:30:05.123Z" },
				new { Key = "Gov-Client-MAC-Addresses", Value = "01%3A23%3A45%3A67%3A89%3AAB%3ACD%3AEF,FE%3ADC%3ABA%3A98%3A76%3A54%3A32%3A10" },
				// Screen Data
				new { Key = "Gov-Client-Window-Size", Value = "width=1024&height=768" },
				new { Key = "Gov-Client-Screens", Value = "width=1920&height=1080&scaling-factor=1.75&colour-depth=32,width=3840&height=2160&scaling-factor=1.5&colour-depth=32" },
				// Software Data
				new { Key = "Gov-Client-User-IDs", Value = "os=TLA" },
				new { Key = "Gov-Client-Multi-Factor", Value = "type=OTHER&timestamp=2020-09-21T02%3A25%3A05.123Z&unique-reference=9A80E58C1FDF000E" },
				new { Key = "Gov-Vendor-License-IDs", Value = "WTG%20Test=3786A3714623116B2449FB2F5F4CBC0A6D89DA15AAF153BEBBF012141AF58D51" },
				new { Key = "Gov-Vendor-Product-Name", Value = "WTG%20Test" },
				new { Key = "Gov-Vendor-Version", Value = "WTG%20Test=99.99.9999.9999" },
			};

			var actual = expected.Select(x => new { x.Key, fraudPreventionData.Single(item => item.Key == x.Key).Value });

			AssertSequencesEqual(expected, actual);
		}
	}
}
