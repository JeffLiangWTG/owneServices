using System;
using CargoWise.Types;
using Enterprise.Customs.HK.Business;
using Enterprise.Edifact;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.HK.ServiceTasks.Testing
{
	class InterchangeProviderTest : InterchangeProviderTestCase
	{
		public void TestSendViaeHub()
		{
			using (Registry.Business.eHubMessagingRegistry.Instance.SendHKISACEViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var mock = Factory.NewMoq<EDIMessage>();
				mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

				var message = mock.Object;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.Traxon;
				message.EM_HeldUntilDate = ZDateTime.Now.AddDays(-1);
				Factory.Save();
				AssertEquals(EDIInterchange.Status.Queued, message.EM_Status);

				var messageCollection = new NonDependentEDIMessageCollection(Factory);
				messageCollection.Add(message);
				var provider = new TraxonInterchangeProvider(messageCollection);
				AssertEquals("Interchanges.Count", 1, provider.Interchanges.Length);
				AssertEquals(EDIInterchange.Status.eHubQueued, provider.Interchanges[0].EI_Status);
				AssertEquals(EDIInterchange.TransportType.eHub, provider.Interchanges[0].EI_TransportType);
			}
		}

		public void TestSendDirectly()
		{
			using (Registry.Business.eHubMessagingRegistry.Instance.SendHKISACEViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var mock = Factory.NewMoq<EDIMessage>();
				mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

				var message = mock.Object;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message.EM_ApplicationCode = EDIMessage.ApplicationCodes.Traxon;
				message.EM_HeldUntilDate = ZDateTime.Now.AddDays(-1);
				Factory.Save();

				var messageCollection = new NonDependentEDIMessageCollection(Factory);
				messageCollection.Add(message);
				var provider = new TraxonInterchangeProvider(messageCollection);
				AssertEquals("Interchanges.Count", 1, provider.Interchanges.Length);
				AssertEquals(EDIInterchange.Status.Queued, provider.Interchanges[0].EI_Status);
			}
		}

		[TestDate(2008, 6, 22, 14, 32, 0)]
		public override void TestMessagesPopulateNewInterchange()
		{
			var messages = new NonDependentEDIMessageCollection(Factory);
			var message1 = messages.AddNew();
			message1.EM_ApplicationCode = "TRX";
			var message2 = messages.AddNew();
			message2.EM_ApplicationCode = "TRX";

			var copyOfMessages = MakeCopyOfMessages(messages);
			var provider = new TraxonInterchangeProvider(messages);

			var interchanges = new EDIInterchangeCollection(Factory);
			interchanges.AddRange(provider.Interchanges);

			AssertEquals("Interchanges.Count", 2, interchanges.Count);
			AssertEquals("Header", "UNA:+.? 'UNB+UNOA:1+" + HKDataRegistry.Instance.HKTraxonSenderID.Value + ":PIMA+" + TraxonInterchangeProvider.TraxonMailbox + ":PIMA+080622:1432+<<INTERCHANGENUMBERPLACEHOLDER>>+" + HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.Value + "'", UNOACharacterSet.FromUNOB(interchanges[0].EI_HeaderText));
			AssertEquals("Footer", "UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'", UNOACharacterSet.FromUNOB(interchanges[0].EI_FooterText));
			AssertEquals(copyOfMessages[0].EM_MessageText, interchanges[0].EI_BodyText);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection) => new TraxonInterchangeProvider(collection);

		protected override void SetUp()
		{
			base.SetUp();
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "54321");
		}
	}
}
