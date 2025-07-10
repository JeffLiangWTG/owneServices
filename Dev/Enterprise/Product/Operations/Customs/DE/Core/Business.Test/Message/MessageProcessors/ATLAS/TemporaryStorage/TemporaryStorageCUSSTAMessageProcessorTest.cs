using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageCUSSTAMessageProcessor))]
	sealed class TemporaryStorageCUSSTAMessageProcessorTest : MessageProcessorAbstractTest<TemporaryStorageCUSSTAMessageProcessor, AtlasInboundEDIMessage<ICUSSTA>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSSTA)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestShouldUpdateRegister_RegLinesCustodianIsCW1MessagingOrganisationAndInterchangeRecipient()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Message status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("LogbookRegNum", ExpectedLogbookRegNumWithAllRegistrationNumbers, message.GetLogbookRegistrationNumber());
				AssertEquals("ReglineTransaction created", 1, regLine.CusTempStorageRegLineTransactions.Count);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
			});
		}

		public void TestShouldUpdateRegister_RegLinesCustodianIsNotCW1MessagingOrganisation()
		{
			regLine.SRL_CustodianIdentifier = "FR123456";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Message status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("LogbookRegNum", ExpectedLogbookRegNumWithAllRegistrationNumbers, message.GetLogbookRegistrationNumber());
				AssertEquals("ReglineTransaction created", 1, regLine.CusTempStorageRegLineTransactions.Count);
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
			});
		}

		public void TestLoadCusTempStorageRegHeaderByMrn()
		{
			regHeader.SRH_Reference = MRN;
			ProcessMessage(message);
			AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
		}

		public void TestShouldNotUpdateRegister_RegLinesCustodianIsCW1MessagingOrganisationAndNotInterchangeRecipient()
		{
			CombineAssertions(() =>
			{
				dataProviderMock.Setup(m => m.InterchangeRecipientReferenceNumber).Returns("FR123456");
				ProcessMessage(message);
				AssertEquals("InterchangeRecipients EORInumber does not match RegLineCustodians EORInumber: message status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("InterchangeRecipients EORInumber does not match RegLineCustodians EORInumber: no reglineTransaction created", 0, regLine.CusTempStorageRegLineTransactions.Count);
				AssertEquals("InterchangeRecipients EORInumber does not match RegLineCustodians EORInumber: LogbookRegNum", ExpectedLogbookRegNumWithAllRegistrationNumbers, message.GetLogbookRegistrationNumber());
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });

				dataProviderMock.Setup(m => m.InterchangeRecipientReferenceNumber).Returns("GR123456");
				dataProviderMock.Setup(m => m.InterchangeRecipientSubsidiaryNumber).Returns("0002");
				message.EM_Status = ZString.Empty;
				ProcessMessage(message);
				AssertEquals("InterchangeRecipients EORIBranch does not match RegLineCustodians EORIBranch", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("InterchangeRecipients EORIBranch does not match RegLineCustodians EORIBranch: no reglineTransaction created", 0, regLine.CusTempStorageRegLineTransactions.Count);
				AssertEquals("InterchangeRecipients EORIBranch does not match RegLineCustodians EORIBranch: LogbookRegNum", ExpectedLogbookRegNumWithAllRegistrationNumbers, message.GetLogbookRegistrationNumber());
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
			});
		}

		public void TestMissingRegister()
		{
			regLine.SRL_LineNumber = 2;
			ProcessMessage(message);
			var stmNote = GetStmNote(message);

			CombineAssertions(() =>
			{
				AssertEquals("This is the list of Reference Number and Sequence Number system could not locate matching Register records for:\r\nReference Numbers: ATB150001400420195875, DE12345678901234; Sequence Number: 1", stmNote.ST_NoteText);
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("EM_MessageNum", MessageIdentifier, message.EM_MessageNum);
			});
		}

		public void TestMissingRegister_EmptyMrn()
		{
			regLine.SRL_LineNumber = 2;
			goodsItemMock.Setup(m => m.MRN).Returns<string>(null);
			ProcessMessage(message);
			var stmNote = GetStmNote(message);

			CombineAssertions(() =>
			{
				AssertEquals("This is the list of Reference Number and Sequence Number system could not locate matching Register records for:\r\nReference Numbers: ATB150001400420195875; Sequence Number: 1", stmNote.ST_NoteText);
			});
		}

		public void TestMissingRegister_EmptyReferencedRegistrationNumber()
		{
			regLine.SRL_LineNumber = 2;
			goodsItemMock.Setup(m => m.ReferencedRegistrationNumber).Returns(ZString.Empty);
			ProcessMessage(message);
			var stmNote = GetStmNote(message);

			CombineAssertions(() =>
			{
				AssertEquals("This is the list of Reference Number and Sequence Number system could not locate matching Register records for:\r\nReference Numbers: DE12345678901234; Sequence Number: 1", stmNote.ST_NoteText);
			});
		}

		public void TestValidMessageSuccessful()
		{
			ProcessMessage(message);
			var stmNote = GetStmNote(message);

			CombineAssertions(() =>
			{
				AssertNull("StmNote Processing Log exists", stmNote);
				var createdRegLineTransaction = regLine.CusTempStorageRegLineTransactions.Single();
				AssertEquals("SRT_PackageQty incorrect", 0, createdRegLineTransaction.SRT_PackageQty);
				AssertEquals("SRT_GrossWeight incorrect", 0m, createdRegLineTransaction.SRT_GrossWeight);
				AssertEquals("SRT_ReferenceType incorrect", ZString.Empty, createdRegLineTransaction.SRT_ReferenceType);
				AssertEquals("SRT_Reference incorrect", ZString.Empty, createdRegLineTransaction.SRT_Reference);
				AssertEquals("SRT_TransactionType incorrect", TransactionTypes.Codes.StatusChange, createdRegLineTransaction.SRT_TransactionType);
				AssertEquals("SRT_InternalReferenceNumber incorrect", MessageIdentifier, createdRegLineTransaction.SRT_InternalReferenceNumber);
				AssertEquals("SRT_Comments incorrect", "Re-Export Authorized for: 10 items", createdRegLineTransaction.SRT_Comments);
				AssertEquals("EM_Status incorrect", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("EM_MessageNum incorrect", MessageIdentifier, message.EM_MessageNum);
				AssertEquals("LogbookRegNum incorrect", ExpectedLogbookRegNumWithAllRegistrationNumbers, message.GetLogbookRegistrationNumber());
				AssertDocumentLinkingSubscribers(new BusinessObject[] { regHeader });
			});
		}

		public void TestPopulateLogbookRegNumWithAdditionalReferenceNumberEmpty()
		{
			dataProviderMock.Setup(m => m.AdditionalReferenceNumber).Returns(ZString.Empty);
			ProcessMessage(message);
			AssertEquals($"{ReferencedRegistrationNumber}, {MRN}", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNum_EmptyMrn()
		{
			goodsItemMock.Setup(m => m.MRN).Returns<string>(null);
			ProcessMessage(message);
			AssertEquals($"{ReferencedRegistrationNumber}, {RegistrationNumber}", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNum_EmptyReferencedRegistrationNumber()
		{
			goodsItemMock.Setup(m => m.ReferencedRegistrationNumber).Returns(ZString.Empty);
			regHeader.SRH_Reference = MRN;
			ProcessMessage(message);
			AssertEquals($"{MRN}, {RegistrationNumber}", message.GetLogbookRegistrationNumber());
			AssertEquals("Message status", EDIMessage.Status.ProcessedOK, message.EM_Status);
		}

		public void TestProhibitedComment()
		{
			goodsItemMock.Setup(m => m.CancellationFlag).Returns(true);
			ProcessMessage(message);
			var createdRegLineTransaction = regLine.CusTempStorageRegLineTransactions.Single();
			AssertEquals("SRT_Comments prohibited", "Re-Export Prohibited for: 10 items", createdRegLineTransaction.SRT_Comments);
		}

		public void TestSendUnsolicitedMessageEMail_NominatedGroup()
		{
			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file_CAU.pdf",
					Type = new DocumentType { Code = "CAU", Description = "CAU Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				},
				new AttachedDocument
				{
					FileName = "file_AAA.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			});

			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageUnsolicitedGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				var subject = $"SumA CUSSTA - Loading permit status message Response";
				var bodyMessageTitle = $"<title>{subject}</title>";
				var bodyMessageHeader = $"<strong>{subject}</strong>";
				var bodyMessageSummary = $"You received a Loading permit status message.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
										+ $"<tr><td>Registration Number</td><td>{RegistrationNumber}</td></tr>"
										+ "</table>";
				CombineAssertions(() =>
				{
					AssertEmailWithTable("E-Mail", email, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" }, subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
					AssertEquals("File has been attached", "file_CAU.pdf", email.Attachments.Cast<AttachmentDef>().SingleOrDefault(x => x.DisplayName.StartsWith("file_")).DisplayName);
				});
			}
			message.AttachedDocuments[0].ImageData.Dispose();
		}

		protected override ZString MessageFriendlyName => "Temporary Storage CUSSTA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSSTA>> Processor => new TemporaryStorageCUSSTAMessageProcessor(logger);

		protected override bool ExpectedMustHaveLinkedObject => false;

		protected override void SetUp()
		{
			base.SetUp();

			regHeader = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			regHeader.SRH_Reference = ReferencedRegistrationNumber;
			regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_PackagesRemaining = 10;
			regLine.SRL_LimitDate = ZDate.Today;
			regLine.SRL_LocationOfGoods = "SYD";
			regLine.SRL_CustodianIdentifier = "GR123456";
			regLine.SRL_CustodianIdentifierBranchNo = "0001";

			var custodianOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			custodianOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.Greece);
			custodianOrgHeader.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "0001", Core.Constants.CountryCodes.Germany);
			custodianOrgHeader.MainAddress.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "API", Core.Constants.CountryCodes.Germany);

			goodsItemMock = new Mock<ICUSSTAGoodsItem>();
			goodsItemMock.Setup(m => m.ReferencedRegistrationNumber).Returns(ReferencedRegistrationNumber);
			goodsItemMock.Setup(m => m.MRN).Returns(MRN);
			goodsItemMock.Setup(m => m.ReferencedSequenceNumber).Returns("1");
			goodsItemMock.Setup(m => m.Quantity).Returns(10);
			goodsItemMock.Setup(m => m.CancellationFlag).Returns(ZBool.False);

			dataProviderMock = new Mock<ICUSSTA>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns(MessageIdentifier);
			dataProviderMock.Setup(m => m.CustodianReferenceNumber).Returns("DE8999783");
			dataProviderMock.Setup(m => m.CustodianSubsidiaryNumber).Returns("0001");
			dataProviderMock.Setup(m => m.InterchangeRecipientReferenceNumber).Returns("GR123456");
			dataProviderMock.Setup(m => m.InterchangeRecipientSubsidiaryNumber).Returns("0001");
			dataProviderMock.Setup(m => m.AdditionalReferenceNumber).Returns(RegistrationNumber);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSSTAGoodsItem[] { goodsItemMock.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSSTA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		const string MessageIdentifier = "CUSSTA58750000000381119050419125839";
		const string ReferencedRegistrationNumber = "ATB150001400420195875";
		const string MRN = "DE12345678901234";
		const string ExpectedLogbookRegNumWithAllRegistrationNumbers = $"{ReferencedRegistrationNumber}, {MRN}, {RegistrationNumber}";
		const string RegistrationNumber = "19DE587500003774X7";
		CusTempStorageRegHeader regHeader;
		CusTempStorageRegLine regLine;
		Mock<ICUSSTAGoodsItem> goodsItemMock;
		Mock<ICUSSTA> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSSTA>> messageMock;
		AtlasInboundEDIMessage<ICUSSTA> message;
	}
}
