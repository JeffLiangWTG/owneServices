using System;
using System.IO;
using System.Reflection;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestsSubclassesOf(typeof(EMCSMessageProcessor<>))]
	abstract class EMCSMessageProcessorAbstractTest<TMessageProcessor, TDataProvider> : MessageProcessorAbstractTest<TMessageProcessor, EMCSInboundEDIMessage, TDataProvider>
		where TMessageProcessor : EMCSMessageProcessor<TDataProvider>
	{
		public void TestEndToEndProcessing()
		{
			CreateSetupData();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var responseDetail = IE.Business.ResponseMessageDetails.GetResponseDetail(incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, incomingMessage.EM_MessageSubType, incomingMessage.EM_MessageText);
				var processorType = responseDetail.ProcessorType;
				var processor = (TMessageProcessor)Activator.CreateInstance(processorType, logger, responseDetail.XmlObjectType);
				processor.PreProcessMessage(incomingMessage);
				CombineAssertions("PreProcess", () =>
				{
					AssertEquals("incomingMessage.EM_GB", Branch.PK, incomingMessage.EM_GB);
					AssertEquals("incomingMessage.EM_LinkUniqueID", declaration.PK, incomingMessage.EM_LinkUniqueID);
					AssertEquals("incomingMessage.EM_LinkTable", declaration.TableName, incomingMessage.EM_LinkTable);
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
				});
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertProcessResult(declaration, incomingMessage);
				});
			}
		}

		protected abstract void AssertProcessResult(EMCSJobDeclaration declaration, EMCSInboundEDIMessage incomingMessage);

		protected abstract void AssertEndToEndProcessing();

		public void TestEndToEndProcessing_FromMessageSample()
		{
			CreateSetupData();
			incomingMessage.EM_MessageText = ManifestResourceReadHelper.ReadManifestResourceContent(MessageSampleNameSpace);
			using (incomingMessage.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(incomingMessage);
				Processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () => AssertEndToEndProcessing());
			}
		}

		public void TestEnsureMessageTextIsValid()
		{
			var xmlObjectTypeProperty = typeof(TMessageProcessor).GetField("xmlObjectType", BindingFlags.NonPublic | BindingFlags.Instance);
			var xmlObjectType = (Type)xmlObjectTypeProperty.GetValue(Processor);
			var deserializeMethod = typeof(IEXmlObjectSerializer).GetMethod(nameof(IEXmlObjectSerializer.Deserialize), BindingFlags.Static | BindingFlags.Public);
			var deserializeMethodGeneric = deserializeMethod.MakeGenericMethod(xmlObjectType);

			using (var reader = new StringReader(MessageText))
			{
				var xmlObject = deserializeMethodGeneric.Invoke(null, new object[] { reader, false });
				AssertNotNull(xmlObject);
			}
		}

		protected virtual EMCSInboundEDIMessage CreateNewIncomingMessage()
		{
			var message = Factory.New<EMCSInboundEDIMessage>();
			message.EM_ApplicationReference = TransactionID;
			message.EM_GB = Branch.PK;
			message.EM_MessageType = MessageType;
			message.EM_MessageText = IE.Messaging.Testing.InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, MessageText, includeResponseWrap: false);
			return message;
		}

		protected virtual EMCSOutboundEDIMessage CreateNewOutgoingMessage()
		{
			var message = Factory.New<EMCSOutboundEDIMessage>();
			message.EM_ApplicationReference = TransactionID;
			message.EM_Status = EDIMessage.Status.Acknowledged;
			message.EM_GB = Branch.PK;
			message.EM_SystemCreateUser = Staff.GS_Code;
			return message;
		}

		protected virtual void CreateNewMrnNumber(EMCSJobDeclaration declaration, ZString mrnNumber, ZString mrnNumberSequenceNumber)
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = declaration.PK;
			entryNumber.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNumber.CE_EntryNum = mrnNumber;
			entryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNumber.CE_EntryLineReference = mrnNumberSequenceNumber;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
		}

		protected virtual void CreateSetupData()
		{
			declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_GB = Branch.PK;
			declaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
			declaration.JE_DeclarationReference = "E00000810";
			CreateNewMrnNumber(declaration, "MRN1234567", "1");
			incomingMessage = CreateNewIncomingMessage();
			outgoingMessage = CreateNewOutgoingMessage();
			declaration.Messages.Add(outgoingMessage);
			Factory.Save();
		}

		protected GlbStaff Staff => staff ?? (staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory));
		GlbStaff staff;

		protected const string TransactionID = "14AB547F-BA18-4138-A7BE-8AC625F834C4";

		protected EMCSJobDeclaration declaration;
		protected EMCSOutboundEDIMessage outgoingMessage;
		protected EMCSInboundEDIMessage incomingMessage;

		protected abstract string MessageSampleNameSpace { get; }

		protected abstract ZString MessageType { get; }

		protected abstract ZString MessageText { get; }

		protected virtual ZString MessageTextMRN { get; }

		protected ZString Serialize<T>(T data)
		{
			return IEXmlObjectSerializer.Serialize(data);
		}

		protected StmNote GetStmNote(EDIMessage message)
		{
			var stmNoteQuery = new ZQuery(StmNoteSchema.ST_Table, EDIMessageSchema.Constants.TableName);
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_ParentID, message.PK);
			stmNoteQuery.AddToFilter(StmNoteSchema.ST_Description, "Processing Log");
			return Factory.LoadTop1<StmNote>(stmNoteQuery);
		}
	}
}
