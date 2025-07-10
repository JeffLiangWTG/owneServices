using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED801ConsignorMessageProcessor))]
	class ED801ConsignorMessageProcessorTest : MessageProcessorAbstractTest<ED801ConsignorMessageProcessor, EmcsInboundEDIMessage<IED801>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IED801)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(declaration, message.EM_LinkedObject);
		}

		public void TestProcessMessageCore_MessageProcessed()
		{
			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_CustomsQuantity = 100;
			line1.ZG_DeclaredValue = 0;
			var line2 = declaration.InvoiceLines.AddNew();
			line2.JI_CustomsQuantity = 200;
			line2.ZG_DeclaredValue = 0;

			CombineAssertions(() =>
			{
				ProcessMessage(message);
				AssertEquals("Entry Status", EU.EMCS.Business.EntryStatusList.Codes.REG, declaration.JE_EntryStatus);
				AssertEquals("Declaration Message Status", EDIMessage.Status.Received, declaration.JE_MessageStatus);
				AssertEquals("Message Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { declaration });

				var query = new ZQuery();
				query.AddToFilter(CusEntryNumSchema.CE_ParentTable, EMCSJobDeclaration.Schema.TableName);
				query.AddToFilter(CusEntryNumSchema.CE_ParentID, declaration.PK);
				var entryNumber = message.Factory.Load<CusEntryNumber>(query).Single();
				AssertEquals("CE_Category", CusEntryNumber.Categories.CustomsPermitClearanceNumber, entryNumber.CE_Category);
				AssertEquals("CE_EntryNum", "MRN98761234", entryNumber.CE_EntryNum);
				AssertEquals("CE_EntryLineReference", "1", entryNumber.CE_EntryLineReference);
				AssertEquals("CE_EntryLineReference", new ZDateTime(2020, 6, 11, 16, 59, 59), entryNumber.CE_IssueDate);
				AssertEquals("CE_EntryType", CusEntryNumberTypes.Standard.MovementReferenceNumber, entryNumber.CE_EntryType);

				AssertEquals("Line1 DeclaredValue", (ZDecimal)100, line1.ZG_DeclaredValue);
				AssertEquals("Line2 DeclaredValue", (ZDecimal)200, line2.ZG_DeclaredValue);
			});
		}

		public void TestPopulateLogbookLocalReferenceNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookLocalReferenceNumber", "B000222547896254786321", message.GetLogbookLocalReferenceNumber());
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals("LogbookRegistrationNumber", "MRN98761234", message.GetLogbookRegistrationNumber());
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			var lastOutgoingMessage = CreateOriginalMessageLinkedToParent<EmcsEDIMessage>(declaration, message.EM_MessageNum);
			lastOutgoingMessage.EM_SystemCreateUser = user.GS_Code;
			declaration.Messages.Add(lastOutgoingMessage);

			ProcessMessage(message);
			AssertEmail("Generate Email", declaration);
		}

		void AssertEmail(string testCase, EMCSJobDeclaration declaration)
		{
			var reference = declaration.JE_DeclarationReference;
			var subject = $"EMCS e-AD registered Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = @"<strong>EMCS e-AD registered Response for <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=EMCS&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $"Your EMCS Declaration for Job {reference} has been registered. For details please follow the Link to the Job."
				+ "<br /><br />ARC: MRN98761234";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(reference));

			AssertEmailForSingleRecipient(testCase, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary);
		}

		protected override ZString MessageFriendlyName => "EMCS ED801 Consignor Message Processor";

		protected override DE.Business.DEBranchCustomsApplicationTypeMessageProcessor<EmcsInboundEDIMessage<IED801>> Processor => new ED801ConsignorMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_DeclarantType = EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
			declaration.JE_OwnerRef = "B000222547896254786321";
			declaration.JE_DeclarationReference = "MRN98761234";

			var emcsEvent = new Mock<IEMCSEvent>();
			emcsEvent.Setup(m => m.AdministrativeReferenceCode).Returns(declaration.JE_DeclarationReference);
			emcsEvent.Setup(m => m.SequenceNumber).Returns("1");

			dataProviderMock = new Mock<IED801>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("DE90003480001003");
			dataProviderMock.Setup(m => m.MessageGroup).Returns(EmcsMessageSubTypeList.Codes.Eme);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("B000222547896254786321");
			dataProviderMock.Setup(m => m.DateAndTimeOfValidationOfEadEsad).Returns(new ZDateTime(2020, 6, 11, 16, 59, 59));
			dataProviderMock.Setup(m => m.ExciseMovement).Returns(emcsEvent.Object);

			messageMock = Factory.NewMoq<EmcsInboundEDIMessage<IED801>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;
			Factory.Save();
		}
		EMCSJobDeclaration declaration;
		Mock<EmcsInboundEDIMessage<IED801>> messageMock;
		Mock<IED801> dataProviderMock;
		EmcsInboundEDIMessage<IED801> message;
	}
}
