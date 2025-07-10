using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InterchangeProviderTest : InterchangeProviderTestCase
	{
		public override void TestMessagesPopulateNewInterchange()
		{
			eHubMessagingRegistry.Instance.SendCAViaEHub.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			NonDependentEDIMessageCollection collection = new NonDependentEDIMessageCollection(Factory);
			Enterprise.Messaging.Business.EDIMessage message1 = collection.AddNew();
			message1.EM_MessageType = MessageTypeList.Codes.DataLoadingModule;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageText = "PPermit1";

			Enterprise.Messaging.Business.EDIMessage message2 = collection.AddNew();
			message2.EM_MessageType = MessageTypeList.Codes.DataLoadingModule;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageText = "PPermit2";

			EDIInterchange[] interchanges = new InterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message2.EM_Status);

			AssertEquals(2, interchanges.Length);
			EDIInterchange interchange1;
			EDIInterchange interchange2;
			if (interchanges[0].EI_BodyText == "PPermit1")
			{
				interchange1 = interchanges[0];
				interchange2 = interchanges[1];
			}
			else
			{
				interchange1 = interchanges[1];
				interchange2 = interchanges[0];
			}

			AssertInterchange(interchange1, "PPermit1", message1);
			AssertInterchange(interchange2, "PPermit2", message2);

			Assert("Should never send DLM via eHub", !interchange1.ShouldSendViaEHub);
			Assert("Should never send DLM via eHub", !interchange2.ShouldSendViaEHub);
		}

		#region Implementation
		void AssertInterchange(EDIInterchange interchange, string bodyText, Enterprise.Messaging.Business.EDIMessage message)
		{
			AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.CACustoms, interchange.EI_ApplicationCode);
			AssertEquals("EI_HeaderText", "", interchange.EI_HeaderText);
			AssertMultilineASCIIEquals("EI_BodyText", bodyText, interchange.EI_BodyText);
			AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_From", GlbCompany.CurrentCompany.GC_Code, interchange.EI_From);
			AssertEquals("EI_To", "CAC", interchange.EI_To);
			AssertEquals("messages", 1, interchange.ContainedMessages.Count);
			AssertCollectionContains(message, interchange.ContainedMessages);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new InterchangeProvider(collection);
		}
		#endregion
	}
}
