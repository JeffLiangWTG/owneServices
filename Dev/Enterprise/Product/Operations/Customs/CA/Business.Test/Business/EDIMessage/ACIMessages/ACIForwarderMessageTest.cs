using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ACIForwarderMessage))]
	public class ACIForwarderMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMessageSubTypeDescription()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = EntryStatusList.Codes.Clear;
			AssertEquals("EM_MessageSubTypeDescription", EntryStatusList.Descriptions.Clear, message.EM_MessageSubTypeDescription);

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageSubType = ACIForwarderMessageTypes.Codes.ChangeAfterArrival;
			AssertEquals("EM_MessageSubTypeDescription", ACIForwarderMessageTypes.Descriptions.ChangeAfterArrival, message.EM_MessageSubTypeDescription);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (ACIForwarderMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<ACIForwarderMessage>();
		}

		public virtual void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.CAACI, message.EM_ApplicationCode);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		public void TestMessageNumberFilledIn()
		{
			var number = Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIMessage.ApplicationCodes.CAACI).PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("MessageNumberFilledIn", "Message Number = " + number, message.EM_MessageText);
		}

		public void TestNoticesColumns()
		{
			var messageText1 = @"UNH+1+GOVCBR:D:11B:UN'
BGM+23+10207000007531'
DTM+9:201411250601:203'
RFF+AGO:857477707RM0001'
STS++2:::0001'
UNS+D'
HYN+3'
UNS+S'
UNT+9+1'
".Replace("\r\n", "");

			var messageText2 = @"UNH+1+GOVCBR:D:11B:UN'
BGM+23+10207000007531'
DTM+9:201411260601:203'
RFF+AGO:857477707RM0001'
STS++2:::0002'
UNS+D'
HYN+3'
UNS+S'
UNT+9+1'
".Replace("\r\n", "");

			AssertMessageTextMatch(messageText1, "MATCHED", new ZDateTime(2014, 11, 25, 6, 1, 0), true);
			AssertMessageTextMatch(messageText2, "NOT MATCHED", new ZDateTime(2014, 11, 26, 6, 1, 0), false);
		}

		void AssertMessageTextMatch(string messageText, string expectedStatusDescription, ZDateTime expectedProccessingDate, ZBool isMatched)
		{
			var testMessage = (EDIMessage)GetNewBusinessObject();
			testMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			testMessage.EM_MessageText = messageText;
			var wrapper = new EManifestResponseWrapper(testMessage as ACIForwarderMessage);
			AssertEquals("Is matched notice", isMatched, wrapper.IsMatchedNotice);
			AssertEquals("Is not matched notice", !isMatched, wrapper.IsNOTMatchedNotice);
			AssertEquals("StatusDescription", expectedStatusDescription, testMessage.StatusDescription);
			AssertEquals("RNSProcessingDate", expectedProccessingDate, testMessage.RNSProcessingDate);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			message = (EDIMessage)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}

		protected EDIMessage message;

		#endregion
	}
}
