using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	public class NCTSResponseMessageDetailsTest : TestCaseWithFactory
	{
		public void TestCompareMessageType()
		{
			var testItem = new NCTSResponseMessageDetails();
			CombineAssertions("NCTSResponseMessageDetails CompareMessageType", () =>
			{
				AssertEquals("Returns 0 with same types.", 0, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE028, NCTSIncomingMessageTypeList.Codes.IE028));
				AssertEquals("Returns 0 with same types.", 0, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE928, NCTSIncomingMessageTypeList.Codes.IE928));
				AssertEquals("Returns 0 with same types.", 0, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE029, NCTSIncomingMessageTypeList.Codes.IE029));

				AssertEquals("IE028(on the left) earlier than IE928", -1, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE028, NCTSIncomingMessageTypeList.Codes.IE928));
				AssertEquals("IE028(on the left) earlier than IE029", -1, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE028, NCTSIncomingMessageTypeList.Codes.IE029));
				AssertEquals("IE028(on the left) earlier than unrecognized type", -1, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE028, "XXX"));

				AssertEquals("IE029(on the left) later than IE928", 1, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE029, NCTSIncomingMessageTypeList.Codes.IE028));
				AssertEquals("IE029(on the left) later than IE029", 1, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE029, NCTSIncomingMessageTypeList.Codes.IE928));
				AssertEquals("IE029(on the left) later than unrecognized type", 1, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE029, "XXX"));

				AssertEquals("IE028(on the right) earlier than IE928", 1, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE928, NCTSIncomingMessageTypeList.Codes.IE028));
				AssertEquals("IE028(on the right) earlier than IE029", 1, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE029, NCTSIncomingMessageTypeList.Codes.IE028));
				AssertEquals("IE028(on the right) earlier than unrecognized type", 1, testItem.CompareMessageType("XXX", NCTSIncomingMessageTypeList.Codes.IE028));

				AssertEquals("IE029(on the right) later than IE928", -1, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE928, NCTSIncomingMessageTypeList.Codes.IE029));
				AssertEquals("IE029(on the right) later than IE029", -1, testItem.CompareMessageType(NCTSIncomingMessageTypeList.Codes.IE028, NCTSIncomingMessageTypeList.Codes.IE029));
				AssertEquals("IE028(on the right) earlier than unrecognized type", -1, testItem.CompareMessageType("XXX", NCTSIncomingMessageTypeList.Codes.IE029));
			});
		}
	}
}
