using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(CustomsAndExciseReportOutboundMessageLookups))]
	sealed class CustomsAndExciseReportOutboundMessageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReportTypeList()
		{
			CombineAssertions(() =>
			{
				var reportMessage1 = Factory.New<CustomsAndExciseReportOutboundMessage>();
				var reportMessage2 = Factory.New<CustomsAndExciseReportOutboundMessage>();
				var lookups1 = new CustomsAndExciseReportOutboundMessageLookups(reportMessage1);
				var lookups2 = new CustomsAndExciseReportOutboundMessageLookups(reportMessage2);
				var list1 = lookups1.ReportTypeList;
				var list2 = lookups2.ReportTypeList;
				AssertEquals("Valid Codes", "PSR, PCT, PTT, PCI, DSR, DCT, DTT, UDR, BAL", list1.CodesAsString);
				AssertSame("Cached", list1, list2);
			});
		}
	}
}
