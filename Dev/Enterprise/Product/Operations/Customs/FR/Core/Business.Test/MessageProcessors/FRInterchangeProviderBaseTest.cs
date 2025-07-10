using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	abstract class FRInterchangeProviderBaseTest<TInterchangeProvider> : TestCaseWithFactory
		where TInterchangeProvider : FRInterchangeProviderBase
	{
		public void TestInterchangesCanBePopulatedCorrectly()
		{
			var message = GetMessageForOutgoing();
			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.Add(message);

			var provider = (TInterchangeProvider)Activator.CreateInstance(typeof(TInterchangeProvider), messages);
			provider.PackCollatedMessagesIntoInterchanges();

			var loadedInterchanges = Factory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GenericMessageDelivery));
			AssertEquals("One interchange should be created to input message.", 1, loadedInterchanges.Length);

			var interchangeCreated = loadedInterchanges.Cast<EDIInterchange>().First();
			MessageProcessorTestHelper.AssertEDIInterchangeProperties(interchangeCreated,
			expectedApplicationCode: "GMD",
			expectedInterchangeType: ExpectedInterchangeType,
			expectedReceiveTransmit: "TRX",
			expectedStatus: "HQU",
			expectedFrom: GlbCompany.CurrentCompany.LicenceKeyIdentifier,
			expectedTo: FRCustomsDataRegistry.Instance.RecipientID.Value,
			expectedBodyText: ExpectedInterchangeBodyText(message),
			expectedHeaderText: $"<SenderID>EDIEDIDAT</SenderID><RecipientID>EASYLO2TEST_EAD</RecipientID><InterchangeType>{ExpectedInterchangeType}</InterchangeType><InterchangeNumber>",
			message: $"MessageType:{MessageType}&MessageSubType:{MessageSubType} Interchange properties");
		}

		TestEDIMessage GetMessageForOutgoing()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CorrelationID = "0000005856";

			var message = Factory.NewWithValidTestData<TestEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.FRCustomsMessage;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = MessageText;
			message.EM_MessageType = MessageType;
			message.EM_LinkedObject = entryHeader;
			message.EM_MessageSubType = MessageSubType;

			return message;
		}

		protected virtual ZString MessageText => "MessageText";

		protected abstract ZString MessageType { get; }

		protected abstract ZString MessageSubType { get; }

		protected abstract ZString ExpectedInterchangeType { get; }

		protected virtual ZString ExpectedInterchangeBodyText(TestEDIMessage message) => message.EM_MessageText;
	}
}
