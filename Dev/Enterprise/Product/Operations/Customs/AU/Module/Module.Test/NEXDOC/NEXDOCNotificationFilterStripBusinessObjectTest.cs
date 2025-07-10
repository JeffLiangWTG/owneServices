using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Module.NEXDOC;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(NEXDOCNotificationFilterStripBusinessObject))]
	sealed class NEXDOCNotificationFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new NEXDOCNotificationFilterStripBusinessObject();
			AssertNotNull(filter["Rex Number"]);
			AssertNotNull(filter["Exporter Reference"]);
			AssertNotNull(filter["Notification Type"]);
			AssertNotNull(filter["Acknowledge Status"]);
			AssertNotNull(filter["Message Status"]);
			AssertNotNull(filter["Received Date"]);
		}

		public void TestFilterLookupNEXDOCNotificationType()
		{
			var filter = new NEXDOCNotificationFilterStripBusinessObject();
			AssertEquals(typeof(NEXDOCNotificationType), filter.NotificationType.GetType());
			AssertEquals("CRR, RA, RJ, CR, EU, ES, FA, ICS, LCA, LCJ, RC, RU, SRA, RFA, RF, RR, RI, RTA, RT, TR, RVW, RW, WC, TA", filter.NotificationType.CodesAsString);
		}

		public void TestFilterLookupNEXDOCMessageStatus()
		{
			var filter = new NEXDOCNotificationFilterStripBusinessObject();
			AssertEquals(typeof(NEXDOCMessageStatus), filter.MessageStatus.GetType());
			AssertEquals("CA, CR, CW, FOH, OP", filter.MessageStatus.CodesAsString);
		}

		public void TestFilterLookupNEXDOCAcknowledgeStatus()
		{
			var filter = new NEXDOCNotificationFilterStripBusinessObject();
			AssertEquals(typeof(NEXDOCAcknowledgeStatus), filter.AcknowledgeStatus.GetType());
			AssertEquals("ACC, ERR, NOT, PAC, PRJ, REJ", filter.AcknowledgeStatus.CodesAsString);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new NEXDOCNotificationFilterStripBusinessObject();
	}
}
