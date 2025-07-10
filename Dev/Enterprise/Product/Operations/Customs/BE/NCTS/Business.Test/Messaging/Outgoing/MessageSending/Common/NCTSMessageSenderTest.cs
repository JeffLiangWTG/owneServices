using System.Linq;
using System.Reflection;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	public abstract class NCTSMessageSenderTest<TMessageSender, TProvider> : MessageSenderTest<TMessageSender, TProvider>
		where TMessageSender : NCTSMessageSender<TProvider>
		where TProvider : class, INCTSMessageHeader
	{
		protected override string MessageDirectory => @"Enterprise.Customs.BE.NCTS.Business.Testing.Messaging.Outgoing.TestFiles.";

		protected override Assembly XmlContentAssembly => Assembly.GetExecutingAssembly();

		protected override ZString MessageType => EDIInterchange.ApplicationCodes.EuNcts;

		protected override ZString ParentTableName => CusInBondHeader.Schema.TableName;

		protected abstract string MovementType { get; }

		protected virtual ZString ExpectedCustomsStatus => ZString.Empty;

		protected abstract ZString ExpectedPhaseStatus { get; }

		protected ZString ExpectedMessageStatus => LogicalStatusList.Codes.Sent;

		protected override ZGuid LinkedObjectId
		{
			get
			{
				ZGuid id;

				if (messageSender.MessageObject is NctsHeader header && header.IsDepartureMovement)
				{
					id = ((NctsHeader)messageSender.MessageObject).MovementHeader.PK;
				}
				else
				{
					id = ((NctsHeader)messageSender.MessageObject).PK;
				}

				return id;
			}
		}

		public void TestPostSendProcess()
		{
			messageSender.Send();
			BEMessage message;

			if (messageSender.MessageObject is NctsHeader header && header.IsDepartureMovement)
			{
				message = (BEMessage)((NctsHeader)messageSender.MessageObject).MovementHeader.Messages.First();
			}
			else
			{
				message = (BEMessage)((NctsHeader)messageSender.MessageObject).Messages.First();
			}

			var expectedMessageInterpretation = new NctsEdiMessagePrettier(message).MakeOutboundPrettyForInterpretation(nctsHeader);
			var movementHeader = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : (NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader;
			CombineAssertions(() =>
			{
				AssertEquals("Customs status", ExpectedCustomsStatus, movementHeader.BM_CustomsStatus);
				AssertEquals("Phase", ExpectedPhaseStatus, movementHeader.BM_Phase);
				AssertEquals("Message status", ExpectedMessageStatus, nctsHeader.EffectiveMessageStatus);
				AssertEquals("Message interpretation", expectedMessageInterpretation, message.EM_MessageInterpretation);
			});
		}

		public new void TestSendMessage()
		{
			SetUpMockProviderData(mockProvider);
			messageSender.Send();

			AssertEquals("Test-message is not checked in SendingObject (live)", false, messageSender.IsTestMessage);
			AssertMessageObject();
		}

		public new void TestSendMessage_TestDeclaration()
		{
			SetUp_TestDeclaration();
			SetUpMockProviderData(mockProvider);
			messageSender.Send();

			AssertEquals("Test-message is checked in SendingObject (test)", true, messageSender.IsTestMessage);
			AssertMessageObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			CommonSetup(false);
		}

		protected override void SetUp_TestDeclaration() => CommonSetup(true);

		void CommonSetup(bool isTestDeclaration)
		{
			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(MovementType);
			var movementHeader = nctsHeader.IsDepartureMovement ? nctsHeader.MovementHeader : (NctsCommonMovementHeader)nctsHeader.ArrivalMovementHeader;
			action = new MessageSendingAction(movementHeader) { EntryType = EntryType, IsTestDeclaration = isTestDeclaration };

			mockProvider = new Mock<TProvider> { CallBase = true };
			var mockMessageSender = new Mock<TMessageSender>(action) { CallBase = true };
			mockMessageSender.Protected().Setup<TProvider>("GetDataProvider", nctsHeader)
				.Returns(mockProvider.Object);
			messageSender = mockMessageSender.Object;
		}

		void AssertMessageObject()
		{
			if (messageSender.MessageObject is NctsHeader header && header.IsDepartureMovement)
			{
				AssertMessage((BEMessage)((NctsHeader)messageSender.MessageObject).MovementHeader.Messages.First());
			}
			else
			{
				AssertMessage((BEMessage)((NctsHeader)messageSender.MessageObject).Messages.First());
			}
		}

		protected MessageSendingAction action;
		protected NctsHeader nctsHeader;
	}
}
