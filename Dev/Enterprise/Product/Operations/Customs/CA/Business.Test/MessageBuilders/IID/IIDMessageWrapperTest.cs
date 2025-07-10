using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class IIDMessageWrapperTest : TestCaseWithFactory
	{
		public void TestPopulateEntrySubmittedDateIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			// with not set declaration
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			var wrapper = new IIDMessageWrapper(entryHeader);
			wrapper.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entryHeader.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate is set to ZDateTime.Now", true, declaration.JE_EntrySubmittedDate == entryHeader.CH_EntrySubmittedDate);

			// with already set declaration
			entryHeader.CH_EntrySubmittedDate = ZDateTime.Empty;
			var declarationTime = ZDateTime.Now.AddMinutes(-30);
			declaration.JE_EntrySubmittedDate = declarationTime;
			wrapper = new IIDMessageWrapper(entryHeader);
			wrapper.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entryHeader.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate is unchanged", true, declaration.JE_EntrySubmittedDate == declarationTime);
		}

		public void TestICAEDIFACTMessageAttacheeProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.CA_AmendReasonCode = EManifestAmendmentReasonCodes.Codes.CBSAOutage;
			declaration.IsCancelled = true;
			declaration.JE_DeclarationReference = "decRef1";
			entryHeader.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			var message = entryHeader.Messages.AddNew();

			var dataWrapper = new IIDMessageWrapper(entryHeader);
			CombineAssertions(delegate
			{
				AssertEquals("IsCancelled", true, ((ICAEDIFACTMessageAttachee)dataWrapper).IsCancelled);
				AssertEquals("HasChanges", true, ((ICAEDIFACTMessageAttachee)dataWrapper).HasChanges);
				AssertEquals("JobIdentification", "decRef1", ((ICAEDIFACTMessageAttachee)dataWrapper).JobIdentification);
				AssertEquals("JobStatus", EntryStatusList.Codes.Cancelled, ((ICAEDIFACTMessageAttachee)dataWrapper).JobStatus);
				AssertEquals("MessageStatus", MessageStatusList.Codes.AwaitingOriginal, ((ICAEDIFACTMessageAttachee)dataWrapper).MessageStatus);
				AssertEquals("TopLevelBusinessObject", declaration, ((ICAEDIFACTMessageAttachee)dataWrapper).TopLevelBusinessObject);
				AssertEquals("Factory", entryHeader.Factory, ((ICAEDIFACTMessageAttachee)dataWrapper).Factory);
				AssertEquals("Messages count", 1, ((ICAEDIFACTMessageAttachee)dataWrapper).Messages.Count);
				AssertEquals("Messages", message, ((ICAEDIFACTMessageAttachee)dataWrapper).Messages[0]);
			});
			AssertEquals("AddNew", typeof(IIDUniversalShipmentMessage), dataWrapper.AddNew().GetType());
		}

		public void TestHasCancellationMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_DeclarationReference = "B00001111";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "BRN";
			invoiceLine.JI_Description = "ABC";

			Factory.Save();

			var dataWrapper = new IIDMessageWrapper(entryHeader);
			var builder = new IIDMessageBuilder(IIDMessageSubTypeList.Codes.Original, dataWrapper);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			message.EM_Status = MessageStatusList.Codes.Sent;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-1);
			AssertEquals(false, dataWrapper.HasAcceptedWithdrawMessage);

			var builderCancel1 = new IIDMessageBuilder(IIDMessageSubTypeList.Codes.Cancellation, dataWrapper);
			var messageCancel1 = builderCancel1.PopulateMessages().GetBuilderResults().First().Message;
			messageCancel1.EM_Status = MessageStatusList.Codes.Sent;

			var messageMAA = Factory.New<UniversalEventMessage>();
			messageMAA.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			messageMAA.EM_MessageText = mmaMessageText;
			messageMAA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("EventyType", AutoEvents.MessageAcceptedCode, messageMAA.EventType);

			var stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = declaration.PK;
				stmAlog.SL_Table = JobDeclaration.Schema.TableName;
			}
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = messageMAA.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();

			AssertEquals(true, dataWrapper.HasAcceptedWithdrawMessage);
		}

		public void TestLatestAcceptedMessageWithDisplayMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_DeclarationReference = "B00001111";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_BrandName = "BRN";
			invoiceLine.JI_Description = "ABC";

			Factory.Save();

			var dataWrapper = new IIDMessageWrapper(entryHeader);
			var builder = new IIDMessageBuilder(IIDMessageSubTypeList.Codes.Original, dataWrapper);
			var message = builder.PopulateMessages().GetBuilderResults().First().Message;
			message.EM_Status = MessageStatusList.Codes.Sent;

			var messageMAA = Factory.New<UniversalEventMessage>();
			messageMAA.EM_MessageSubType = UniversalEventMessageTypes.Codes.IIDResponses;
			messageMAA.EM_MessageText = mmaMessageText;
			messageMAA.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("EventyType", AutoEvents.MessageAcceptedCode, messageMAA.EventType);

			var stmAlog = Factory.New<StmALog>();
			using (stmAlog.LockForUpdatingKeyFieldsForTesting())
			{
				stmAlog.SL_Parent = declaration.PK;
				stmAlog.SL_Table = JobDeclaration.Schema.TableName;
			}
			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_Relation1ID = stmAlog.PK;
			genPivot.XX_Relation2ID = messageMAA.PK;
			genPivot.XX_RelationType = Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage;

			Factory.Save();
			AssertEquals("Get the last accepted message", true, dataWrapper.LatestSentAcceptedMessage.EM_MessageText.Contains("<Description>ABC</Description>"));
		}

		readonly string mmaMessageText = @"<s0:UniversalEvent><s0:Event><s0:DataContext><s0:DataTargetCollection><s0:DataTarget><s0:Type>CAIntegratedImportDeclaration</s0:Type><s0:Key>10207000019090</s0:Key></s0:DataTarget></s0:DataTargetCollection><s0:RecipientRoleCollection><s0:RecipientRole><s0:Code>CD4</s0:Code><s0:Description>CA Customs IID/D4 Status Notice</s0:Description></s0:RecipientRole></s0:RecipientRoleCollection></s0:DataContext><s0:EventTime>2018-03-19T11:00:00</s0:EventTime><s0:EventType>MAA</s0:EventType><s0:EventReference>IID Accepted</s0:EventReference><s0:ContextCollection><s0:Context><s0:Type>InterchangeNumber</s0:Type><s0:Value>10484</s0:Value></s0:Context><s0:Context><s0:Type>MessageNumber</s0:Type><s0:Value>1</s0:Value></s0:Context><s0:Context><s0:Type>IsTest</s0:Type><s0:Value>Y</s0:Value></s0:Context><s0:Context><s0:Type>OrganizationReference</s0:Type><s0:Value>B00172152</s0:Value></s0:Context></s0:ContextCollection></s0:Event></s0:UniversalEvent>";

		public void TestIsMessageValidationPassed()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var dataWrapper = new IIDMessageWrapper(entryHeader);
			Assert("IsMessageValidationPassed", !dataWrapper.IsMessageValidationPassed);
			var cusEntryNum = CusEntryNumber.New(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada);
			cusEntryNum.CE_EntryStatus = MessageProcessors.MessageValidationPassedMessageProcessor.IsOnFile;
			Assert("IsMessageValidationPassed", dataWrapper.IsMessageValidationPassed);
		}
	}
}
