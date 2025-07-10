using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(CustomsAndExciseReportsFilterStripBusinessObject))]
	sealed class CustomsAndExciseReportsFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CustomsAndExciseReportsFilterStripBusinessObject();
		}

		public void TestMessageFilter()
		{
			var msg1 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			var msg2 = Factory.New<AISOutboundEDIMessage>();
			var msg3 = Factory.New<CustomsAndExciseReportOutboundMessage>();
			msg3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var filterStripBO = GetNewFilterStripBusinessObject();
			var items = Factory.Load(typeof(Enterprise.Messaging.Business.EDIMessage), filterStripBO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("Length", 1, items.Length);
				AssertEquals("PK", msg1.PK, items[0].PK);
			});
		}

		public void TestGetModuleFiltersCore()
		{
			var stripBO = GetNewFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				var reportTypeFilter = (ModuleTextFilter)stripBO["Report Type"];
				AssertNotNull("Report Type", reportTypeFilter);
				var messageNumberFilter = (ModuleTextFilter)stripBO["Message Number"];
				AssertNotNull("Message Number", messageNumberFilter);
				var statusFilter = (ModuleTextFilter)stripBO["Status"];
				AssertNotNull("Status", statusFilter);
			});
		}
	}
}
