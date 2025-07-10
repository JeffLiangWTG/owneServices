using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM413;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM414;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM415;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM432;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM433;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class AISMessageSenderTest : TestCaseWithFactory
	{
		public void TestLogCustomsCommencedEvent()
		{
			var (declaration, entryHeader) = SetupData();

			AssertLogCustomsCommencedEvent(AISOutgoingMessageTypeList.Codes.AmendmentRequest, "IM413: Amendment Request");

			void AssertLogCustomsCommencedEvent(ZString messageType, ZString description)
			{
				var sendingAction = new AISMessageSendingAction(entryHeader);
				sendingAction.MessageType = messageType;
				var sender = new AISMessageSender(sendingAction);
				var importerAddress = Factory.NewWithValidTestData<OrgAddress>();
				declaration.JE_OA_ImporterAddress = importerAddress.PK;
				sender.Send();
				Factory.Save();
				var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));

				CombineAssertions(() =>
				{
					AssertEquals("CCC (Customs Commenced) Log Entry event should have been created", true, logEntries.Any());
					AssertEquals("SL_Reference", description, logEntries.LastOrDefault().SL_Reference);
				});
			}
		}

		public void TestSend_AmendmentRequest()
		{
			var (declaration, entryHeader) = SetupData();

			CombineAssertions(() =>
			{
				(var xmlObject, _) = AssertSend<Im413>(AISOutgoingMessageTypeList.Codes.AmendmentRequest, entryHeader, false);
				AssertNotNull(xmlObject.GoodsShipment);
				AssertNotNull(xmlObject.Importer);
				AssertNotNull(xmlObject.Declarant);
			});
		}

		public void TestSend_InvalidationRequest()
		{
			var (declaration, entryHeader) = SetupData();

			CombineAssertions(() =>
			{
				(var xmlObject, _) = AssertSend<Im414>(AISOutgoingMessageTypeList.Codes.InvalidationRequest, entryHeader, false);
				AssertNotNull(xmlObject.ImportOperation);
				AssertNotNull(xmlObject.Declarant);
			});
		}

		public void TestSend_CustomsDeclaration()
		{
			var (declaration, entryHeader) = SetupData();

			CombineAssertions(() =>
			{
				(var xmlObject, var message) = AssertSend<Im415>(AISOutgoingMessageTypeList.Codes.CustomsDeclaration, entryHeader, false);
				AssertNotNull(xmlObject.GoodsShipment);
				AssertNotNull(xmlObject.Importer);
				AssertNotNull(xmlObject.Declarant);
			});
		}

		public void TestSend_PresentationNotification()
		{
			var (_, entryHeader) = SetupData();
			var (xmlObject, _) = AssertSend<Im432>(AISOutgoingMessageTypeList.Codes.PresentationNotification, entryHeader, false);
			AssertNotNull(xmlObject.GoodsShipment);
		}

		public void TestSend_EntryIntoTheDeclarantRecords()
		{
			var (_, entryHeader) = SetupData();
			var (xmlObject, _) = AssertSend<Im433>(AISOutgoingMessageTypeList.Codes.EntryIntoTheDeclarantRecords, entryHeader, false);
			AssertNotNull(xmlObject.GoodsShipment);
		}

		(T xmlObj, AISOutboundEDIMessage message) AssertSend<T>(string messageType, CusEntryHeader entryHeader, bool isEndToEndTest, Action<AISMessageSendingAction> extraSetup = null)
		where T : IAISMessageXmlObject
		{
			var result = default(T);
			var sendingAction = new AISMessageSendingAction(entryHeader);
			sendingAction.MessageType = messageType;
			extraSetup?.Invoke(sendingAction);

			var sender = new AISMessageSender(sendingAction);
			sender.Send();
			Factory.Save();
			var createdMessage = (AISOutboundEDIMessage)entryHeader.Messages.LastOrDefault();
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

			var importerAddress = Factory.NewWithValidTestData<OrgAddress>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
			var declarantAddress = Factory.NewWithValidTestData<OrgAddress>();
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.DefermentPartyDocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var dutyPayer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_DutyPayer = dutyPayer.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			var address = orgHeader.MainAddress;
			declaration.JE_OA_Representative = address.PK;
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			var contact = orgHeader.Contacts.AddNew();
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;

			return (declaration, entryHeader);
		}
	}
}
