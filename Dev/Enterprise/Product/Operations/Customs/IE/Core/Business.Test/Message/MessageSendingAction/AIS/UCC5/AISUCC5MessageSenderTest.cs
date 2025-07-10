using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM414;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM415;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM432;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class AISUCC5MessageSenderTest : TestCaseWithFactory
	{
		public void TestSend_CustomsDeclaration()
		{
			var (declaration, entryHeader) = SetupData();

			CombineAssertions(() =>
			{
				(var xmlObject, var message) = AssertSend<Im415>(AISOutgoingMessageTypeList.Codes.CustomsDeclaration, entryHeader, false);
				AssertNotNull(xmlObject.Declaration);
			});
		}

		public void TestSend_InvalidationRequest()
		{
			var (declaration, entryHeader) = SetupData();

			CombineAssertions(() =>
			{
				(var xmlObject, _) = AssertSend<Im414>(AISOutgoingMessageTypeList.Codes.InvalidationRequest, entryHeader, false);
				AssertNotNull(xmlObject.Declaration);
			});
		}

		public void TestSend_PresentationNotification()
		{
			var (declaration, entryHeader) = SetupData();

			CombineAssertions(() =>
			{
				(var xmlObject, var message) = AssertSend<Im432>(AISOutgoingMessageTypeList.Codes.PresentationNotification, entryHeader, false);
				AssertNotNull(xmlObject.Declaration);
				AssertNotNull(xmlObject.GoodsShipment);
			});
		}

		(T xmlObj, AISUCC5OutboundEDIMessage message) AssertSend<T>(string messageType, CusEntryHeader entryHeader, bool isEndToEndTest, Action<AISUCC5MessageSendingAction> extraSetup = null)
		where T : IAISUCC5MessageXmlObject
		{
			var result = default(T);
			var sendingAction = new AISUCC5MessageSendingAction(entryHeader);
			sendingAction.MessageType = messageType;
			extraSetup?.Invoke(sendingAction);

			var sender = new AISUCC5MessageSender(sendingAction);
			sender.Send();
			Factory.Save();
			var createdMessage = (AISUCC5OutboundEDIMessage)entryHeader.Messages.LastOrDefault();
			if (isEndToEndTest)
			{
				using (var reader = createdMessage.GetEM_MessageTextReader())
				{
					result = IEXmlObjectSerializer.Deserialize<T>(reader);
					AssertNotNull(result);
				}
			}
			else
			{
				result = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<T>(createdMessage.EM_MessageText);
				AssertNotNull(result);
			}
			return (result, createdMessage);
		}

		(JobDeclaration, CusEntryHeader) SetupData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var credential = InterchangeProcessorTestHelper.CreateValidCredential(declaration.Company);
			credential.GP_MailBoxID = "MS12345";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			return (declaration, entryHeader);
		}
	}
}
