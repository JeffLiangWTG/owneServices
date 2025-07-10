using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Messaging.Testing
{
	sealed class EDIInterchangeBaseOnlyTest : TestCaseWithFactory
	{
		public void TestShouldShowInterchangeEventsTab()
		{
			AssertEquals("Precondition: Current user is CWSupport", true, Env.CurrentUser.IsSupportUser);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange.EI_XTInternalMsgID = 101;
			AssertEquals("Show xT event tab", true, interchange.ShouldShowInterchangeEventsTab);

			interchange.EI_XTInternalMsgID = 0;
			AssertEquals("no XTInternalMsgID", false, interchange.ShouldShowInterchangeEventsTab);

			interchange.EI_TransportType = EDIInterchange.TransportType.eHub;
			interchange.EI_XTInternalMsgID = 101;
			AssertEquals("Not xT interchange", false, interchange.ShouldShowInterchangeEventsTab);

			GlbStaff.CurrentUser.GS_LoginName = "test1";
			AssertEquals("Precondition: Current user is not Support user", false, Env.CurrentUser.IsSupportUser);
			interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange.EI_XTInternalMsgID = 101;
			AssertEquals("not Support user", false, interchange.ShouldShowInterchangeEventsTab);
		}

		public void TestCheckInterchangeTypeIsSupportedForGMD()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "ABC";
			interchange.EI_To = "DEF";
			Factory.Save();
			AssertEquals("", ErrorReporter.LastMessageReported);
			interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "ABC";
			interchange.EI_To = "DEF";
			interchange.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAtlasSystem;
			Factory.Save();
			AssertEquals("", ErrorReporter.LastMessageReported);
			interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "ABC";
			interchange.EI_To = "DEF";
			interchange.EI_InterchangeType = "@#1";
			Factory.Save();
			var typeFullname = typeof(GenericMessageDeliveryInterchangeTypeList).FullName;
			AssertEquals($"Unknown Interchange Type '@#1'; receive GMD Interchange must be in the supported list '{typeFullname}'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_From = "ABC";
			interchange.EI_To = "DEF";
			interchange.EI_InterchangeType = "@#1";
			Factory.Save();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}
	}
}
