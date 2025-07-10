using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM917;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.Business.Testing
{
	class MessageAttacheeMessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestEndToEndProcessing()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "CIE";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BIE";
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			declaration.JE_GB = branch.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			const string transactionId = "E7CDA07D-7EF1-4380-8130-D584734E984B";
			var incomingMessage = Factory.New<AESInboundEDIMessage>();
			incomingMessage.EM_ApplicationReference = transactionId;
			incomingMessage.EM_MessageType = "X!@";
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, IEXmlObjectSerializer.Serialize(
				new Im917()
				{
					XmlNegativeAcknowledgement = new System.Collections.ObjectModel.Collection<XmlNegativeAcknowledgement>(new[]
					{
						new XmlNegativeAcknowledgement()
						{
							ErrorLineNumber = "1",
							ErrorColumnNumber = "1",
							ErrorReason = "BAD"
						}
					})
				}), includeResponseWrap: false);
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = branch.PK;
			var staff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			entry.Messages.Add(outgoingMessage);
			Factory.Save();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var logger = new LoggingInformation();
				var processor = new MessageAttacheeMessageProcessorForTest(logger);
				processor.PreProcessMessage(incomingMessage);
				CombineAssertions("PreProcess", () =>
				{
					AssertEquals("incomingMessage.EM_GB", branch.PK, incomingMessage.EM_GB);
					AssertEquals("incomingMessage.EM_LinkUniqueID", entry.PK, incomingMessage.EM_LinkUniqueID);
					AssertEquals("incomingMessage.EM_LinkTable", entry.TableName, incomingMessage.EM_LinkTable);
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.PreProcessedOK, incomingMessage.EM_Status);
				});
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("entry.CH_EntryStatus", "CLR", entry.CH_EntryStatus);
					AssertEquals("incomingMessage.EM_MessageOwner", "DONE", incomingMessage.EM_MessageOwner);
				});
			}
		}
	}

	class MessageAttacheeMessageProcessorForTest : MessageAttacheeMessageProcessor<AESInboundEDIMessage, IM917Provider>
	{
		public MessageAttacheeMessageProcessorForTest(LoggingInformation logger) : base(logger, typeof(Im917))
		{
		}

		protected override string MessageFriendlyNameCore => "Test Processor";

		protected override string ApplicationCodeCore => EDIInterchange.ApplicationCodes.IECustomsExport;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AESInboundEDIMessage message, IM917Provider provider)
		{
			base.ProcessMessageCore(factory, message, provider);
			message.EM_MessageOwner = "DONE";
			((CusEntryHeader)message.EM_LinkedObject).CH_EntryStatus = "CLR";
		}

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, IM917Provider provider) => "CLR";
	}
}
