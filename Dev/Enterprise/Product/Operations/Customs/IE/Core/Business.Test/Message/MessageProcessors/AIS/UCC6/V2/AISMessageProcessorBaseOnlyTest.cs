using System.Collections.Generic;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM917;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	class AISMessageProcessorBaseOnlyTest : TestCaseWithFactory
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
			var incomingMessage = Factory.New<AISInboundEDIMessage>();
			incomingMessage.EM_ApplicationReference = transactionId;
			incomingMessage.EM_MessageType = AISInterchangeTypeList.Codes.IM917;
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(transactionId, IEXmlObjectSerializer.Serialize(
				new Im917
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
			var outgoingMessage = Factory.New<AISOutboundEDIMessage>();
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

		public void TestNeedToSendEmailNotification_WhenLinkedObjectIsIAISMessageAttacheeWithEmailLogic()
		{
			var logger = new LoggingInformation();
			var processor = new AISMessageProcessorForTest(logger);

			var bizoShouldSendEmail = new AISMessageAttacheeWithEmailLogicForTest(true);
			var message = Factory.New<AISInboundEDIMessage>();
			message.EM_LinkedObject = bizoShouldSendEmail;
			Assert(processor.NeedToSendEmailNotification_Exposed(message));

			var bizoShouldNotSendEmail = new AISMessageAttacheeWithEmailLogicForTest(false);
			message.EM_LinkedObject = bizoShouldNotSendEmail;
			Assert(!processor.NeedToSendEmailNotification_Exposed(message));
		}
	}

	class AISMessageProcessorForTest : AISMessageProcessor<AISInboundEDIMessage, IM917Provider>
	{
		public AISMessageProcessorForTest(LoggingInformation logger) : base(logger, typeof(Im917)) { }

		public IRegistryItem EmailGroupRegistryItem => GetEmailGroupRegistryItem();

		protected override string MessageFriendlyNameCore => "HELLO WORLD";

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM917Provider provider) => "CLR";

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AISInboundEDIMessage message, IM917Provider provider)
		{
			base.ProcessMessageCore(factory, message, provider);
			message.EM_MessageOwner = "DONE";
		}

		public bool NeedToSendEmailNotification_Exposed(EDIMessage message) => NeedToSendEmailNotification(message);
	}

	class AISMessageAttacheeWithEmailLogicForTest : NonPersistentBusinessObject, IAISMessageAttacheeWithEmailLogic
	{
		public AISMessageAttacheeWithEmailLogicForTest(bool shouldSendEmail)
		{
			this.shouldSendEmail = shouldSendEmail;
		}

		readonly bool shouldSendEmail;

		public Logs Logs => null;

		public IRequestedDocumentsProvider RequestedDocumentsProvider => null;

		public GlbBranch Branch => null;

		public IRelatedJob RelatedJob => null;

		public GlbStaff CustomsAgent => null;

		public ZString LogicalStatus { get; set; }

		public ZString EntryStatus { get; set; }

		public ZString MovementReferenceNumber => "";

		public IEnumerable<Enterprise.Messaging.Business.EDIMessage> Messages => null;

		public void MovementReferenceNumberSetter(ZString mrn, ZDateTime? issueDate = null, ZString? entryStatus = null, ZDateTime? expiryDate = null) { }

		public void PopulateConfirmedDutiesAndTaxes(IEnumerable<IGoodsItemProvider> goodsItems) { }

		public void SetCustomsRegistrationNumber(ZString crn) { }

		public void SetEntryReleaseDate(ZDateTime releaseDate) { }

		public void SetSimplifiedDeclarationMRN(ZString mrn) { }

		public bool ShouldSendEmailNotification(Enterprise.Messaging.Business.EDIMessage message) => shouldSendEmail;
	}
}
