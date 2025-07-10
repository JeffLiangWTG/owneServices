using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM416;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using IM416Provider = Enterprise.Customs.IE.Messaging.UCC5.IM416Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class AISUCC5MessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetEmailGroupRegistryItem()
		{
			var processor = new AISMessageProcessorForTest(new LoggingInformation());
			AssertSame(IECustomsDataRegistry.Instance.SendImportAcknowledgements, processor.EmailGroupRegistryItem);
		}

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
			var incomingMessage = Factory.New<AISUCC5InboundEDIMessage>();
			incomingMessage.EM_ApplicationReference = transactionId;
			incomingMessage.EM_MessageType = AISInterchangeTypeList.Codes.IM416;
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, IEXmlObjectSerializer.Serialize(
				new Im416
				{
					Declaration = new DeclarationType()
					{
						AdditionalDeclarationType12 = "A",
						Lrn25 = "LRN001",
						RejectionDate = "20230810",
						RejectionMotivationText = "Rejection Motivation Text",
					},
					FunctionalError = new Collection<FunctionalErrorType>()
					{
						new FunctionalErrorType()
						{
							ErrorMessage = "Error Message",
							ErrorPointer = "Error Pointer",
							ErrorReason = "Error Reason",
							ErrorType = "Error Type"
						}
					}
				}), includeResponseWrap: false);
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			var outgoingMessage = Factory.New<AISUCC5OutboundEDIMessage>();
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
				var processor = new AISMessageProcessorForTest(logger);
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("entry.CH_EntryStatus", "CLR", entry.CH_EntryStatus);
					AssertEquals("incomingMessage.EM_MessageOwner", "DONE", incomingMessage.EM_MessageOwner);
				});
			}
		}

		public void TestApplicationCode()
		{
			var logger = new LoggingInformation();
			var processor = new AISMessageProcessorForTest(logger);
			AssertEquals(EDIMessage.ApplicationCodes.IECustomsUCC5Import, processor.ApplicationCode);
		}

		class AISMessageProcessorForTest : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM416Provider>
		{
			public AISMessageProcessorForTest(LoggingInformation logger) : base(logger, typeof(Im416)) { }

			public IRegistryItem EmailGroupRegistryItem => GetEmailGroupRegistryItem();

			protected override string MessageFriendlyNameCore => "HELLO WORLD";

			public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM416Provider provider) => "CLR";

			protected override void ProcessMessageCore(BusinessObjectFactory factory, AISUCC5InboundEDIMessage message, IM416Provider provider)
			{
				base.ProcessMessageCore(factory, message, provider);
				message.EM_MessageOwner = "DONE";
			}
		}
	}
}
