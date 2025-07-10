using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class MessageProcessorBaseOnlyTest : TestCaseWithFactory
	{
		public void TestAttachDocuments()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "19DE485154386041M4";
			Factory.Save();

			var dataProviderMock = new Mock<ICURREL>();
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("DE302989100000000000000000000487287");
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("0624123347");
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("19DE485154386041M4");

			var goodsItem = new Mock<ICURRELGoodsItem> { CallBase = true };
			goodsItem.Setup(x => x.SequenceNumber).Returns("1");
			goodsItem.Setup(x => x.DirectiveFlag).Returns("0");
			goodsItem.Setup(x => x.IssuingFlag).Returns("N");
			goodsItem.Setup(x => x.AcceptanceFlag).Returns("N");
			goodsItem.Setup(m => m.RejectionFlag).Returns("J");

			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object });

			var messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICURREL>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX="))
				}
			});
			var message = messageMock.Object;

			Factory.Save();

			var docManagerSupport = (IDocManagerSupport)entryHeader;
			AssertEquals(0, docManagerSupport.DocManagerInfo.AllEDocs.Count);

			using (Factory.AddDisposableService())
			{
				new ImportCURRELMessageProcessor(new LoggingInformation()).PreProcessMessage(message);
				new ImportCURRELMessageProcessor(new LoggingInformation()).ProcessMessage(message);
				Factory.Save();
			}

			entryHeader.Reload();
			AssertEquals(1, docManagerSupport.DocManagerInfo.AllEDocs.Count);
			var eDoc = docManagerSupport.DocManagerInfo.AllEDocs[0];

			AssertEquals("FileName", "file1.pdf", eDoc.FileName);
			AssertEquals("DocType", "AAA", eDoc.DocType);
			AssertEquals("Description", "AAA Desc", eDoc.Description);
			AssertEquals("ImageData", Convert.FromBase64String("XXX="), eDoc.GetImageDataReader().ConvertToByteArrayAndCloseStream());
		}

		public void TestProcessedPreviously()
		{
			var message_processedPreviously = ATLASMessageHelper.CreateAnyKnownATLASMessage(Factory);
			message_processedPreviously.EM_Status = EDIMessage.Status.ProcessedOK;
			var message = ATLASMessageHelper.CreateAnyKnownATLASMessage(Factory);
			Factory.Save();

			RunMessageProcessor(new[] { message }, new LoggingInformation());
			message.Reload();
			AssertEquals("EM_Status", EDIMessage.Status.Discarded, message.EM_Status);

			message.AttachedDocuments<ICUSTST>()[0].ImageData.Dispose();
			message_processedPreviously.AttachedDocuments<ICUSTST>()[0].ImageData.Dispose();
		}

		public void TestDuplicateInBatch()
		{
			var message = ATLASMessageHelper.CreateAnyKnownATLASMessage(Factory);
			var message_Duplicate = ATLASMessageHelper.CreateAnyKnownATLASMessage(Factory);
			Factory.Save();

			var logger = new LoggingInformation();
			RunMessageProcessor(new[] { message, message_Duplicate }, logger);

			message.Reload();
			message_Duplicate.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("First Message EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("First Message EM_MessageNum", "CUSTST58750000000375302250219160050", message.EM_MessageNum);
				AssertEquals("Duplicate Message EM_Status", EDIMessage.Status.Discarded, message_Duplicate.EM_Status);
				AssertEquals("Duplicate Message EM_MessageNum", "CUSTST58750000000375302250219160050", message_Duplicate.EM_MessageNum);
				AssertCollectionContains("Duplicate Warning Log", "\tMessage with message number CUSTST58750000000375302250219160050 exists already.(Type:TST, Sub:SAE, Ref:SCTSTJ); message status set to DISCARDED.", logger.UserLogStrings);
			});

			message.AttachedDocuments<IUnderCustomsControl>()[0].ImageData.Dispose();
			message_Duplicate.AttachedDocuments<IUnderCustomsControl>()[0].ImageData.Dispose();
		}

		public void TestPreviouslyProcessedDoesNotAddMoreEDocs()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
			header.SRH_Reference = "ATB150002930220195875";

			var message = ATLASMessageHelper.CreateAnyKnownATLASMessage(Factory);
			var message_Duplicate = ATLASMessageHelper.CreateAnyKnownATLASMessage(Factory);
			Factory.Save();

			var logger = new LoggingInformation();
			RunMessageProcessor(new[] { message, message_Duplicate }, logger);
			header.Reload();
			AssertEquals("Single eDoc", 1, header.DocManagerInfo.AllEDocs.Count);

			message.AttachedDocuments<IUnderCustomsControl>()[0].ImageData.Dispose();
			message_Duplicate.AttachedDocuments<IUnderCustomsControl>()[0].ImageData.Dispose();
		}

		public void TestGetCusReconDeclarationFromMRN()
		{
			var testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "AAA";
			testCompany.GC_RN_NKCountryCode = "AU";
			var testBranch = testCompany.Branches.AddNew();
			testBranch.GB_RL_NKHomePort = "AUSYD";
			testBranch.GB_Code = "BBB";

			var cusReconDeclaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			cusReconDeclaration.CRD_GB_Branch = testBranch.PK;
			CreateMovementReferenceNumber(cusReconDeclaration);

			var cusReconDeclaration2 = Factory.NewWithValidTestData<CusReconDeclaration>();
			cusReconDeclaration2.CRD_GB_Branch = GlbBranch.CurrentBranch.PK;
			CreateMovementReferenceNumber(cusReconDeclaration2);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_GB = testBranch.PK;
			CreateMovementReferenceNumber(declaration);

			Factory.Save();

			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
			CombineAssertions(() =>
			{
				AssertNull("MovementReferenceNumber and mrn is empty/null", processor.GetCusReconDeclarationFromMRNExposed(Factory, testCompany.PK, string.Empty));
				AssertEquals("MovementReferenceNumber provided: cusReconDeclaration", cusReconDeclaration.PK, processor.GetCusReconDeclarationFromMRNExposed(Factory, testCompany.PK, "ATB150000620520195875", null).PK);
				AssertEquals("MovementReferenceNumber provided: cusReconDeclaration2", cusReconDeclaration2.PK, processor.GetCusReconDeclarationFromMRNExposed(Factory, GlbCompany.CurrentCompany.PK, "ATB150000620520195875", null).PK);
				AssertEquals("AtlasReferenceNumber provided: cusReconDeclaration", cusReconDeclaration.PK, processor.GetCusReconDeclarationFromMRNExposed(Factory, testCompany.PK, null, "ATB150000620520195875").PK);
				AssertEquals("AtlasReferenceNumber provided: cusReconDeclaration2", cusReconDeclaration2.PK, processor.GetCusReconDeclarationFromMRNExposed(Factory, GlbCompany.CurrentCompany.PK, null, "ATB150000620520195875").PK);
			});

			void CreateMovementReferenceNumber(BusinessObject businessObject)
			{
				var mrnEntryNumber = CusEntryNumber.LoadOrCreate(businessObject, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";
			}
		}

		public void TestGetLinkedObjectFromOriginalMessageOrMrn_OriginalMessage()
		{
			const string messageIdentifier = "12345";
			var entryHeader = Factory.New<CusEntryHeader>();
			var originalMessage = Factory.New<AtlasEDIMessage>();
			originalMessage.EM_MessageNum = messageIdentifier;
			originalMessage.EM_LinkedObject = entryHeader;
			originalMessage.EM_Status = EDIMessage.Status.Sent;

			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
			var result = processor.GetLinkedObjectFromOriginalMessageOrMrnExposed(Factory, messageIdentifier, string.Empty);
			AssertSame(entryHeader, result);
		}

		public void TestGetLinkedObjectFromOriginalMessageOrMrn_MRN()
		{
			const string mrn = "21DE123050554788M5";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = mrn;
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
			var result = processor.GetLinkedObjectFromOriginalMessageOrMrnExposed(Factory, string.Empty, mrn);
			AssertSame(entryHeader, result);
		}

		public void TestGetLinkedObjectFromOriginalMessageOrMrn_Failed()
		{
			var logger = new LoggingInformation();
			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(logger);
			var result = processor.GetLinkedObjectFromOriginalMessageOrMrnExposed(Factory, "12345", "MRN_TEST");
			CombineAssertions(() =>
			{
				AssertEquals("LinkedObject", null, result);
				AssertEquals("Warning", true, logger.UserLogStrings.Contains("\tUnable to get linked object from the original message or MRN (Message Number:12345, MRN:MRN_TEST)."));
			});
		}

		public void TestGetLinkedObjectFromMRN()
		{
			var linkedObject = Factory.New<TaxChangeAssessment>();
			var entry = Factory.New<CusEntryNumber>();
			entry.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entry.CE_EntryNum = "referenceNumber";
			entry.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			entry.Parent = linkedObject;

			var nonMatchinglinkedObject = Factory.New<JobDeclaration>();
			var nonMatchingEntry = Factory.New<CusEntryNumber>();
			nonMatchingEntry.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			nonMatchingEntry.CE_EntryNum = "referenceNumber";
			nonMatchingEntry.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			nonMatchingEntry.Parent = nonMatchinglinkedObject;

			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
			CombineAssertions(() =>
			{
				var result = processor.GetLinkedObjectFromMRNExposed<TaxChangeAssessment>(Factory, "referenceNumber", null);
				AssertSame("movementReferenceNumber", linkedObject, result);

				result = processor.GetLinkedObjectFromMRNExposed<TaxChangeAssessment>(Factory, null, "referenceNumber");
				AssertSame("atlasReferenceNumber", linkedObject, result);
			});
		}

		public void TestGetEntryHeaderFromMRN_MRNAndRefNrAreEmptyOrNull()
		{
			CombineAssertions(() =>
			{
				var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
				AssertNull("Empty", processor.GetEntryHeaderFromMRNExposed(Factory, string.Empty, string.Empty));
				AssertNull("null", processor.GetEntryHeaderFromMRNExposed(Factory, null, null));
			});
		}

		public void TestGetEntryHeaderFromMRN_RefNrNull()
		{
			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entry, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "MRN";
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			Factory.Save();

			AssertEquals(entry, processor.GetEntryHeaderFromMRNExposed(Factory, "MRN", null));
		}

		public void TestGetEntryHeaderFromMRN_MRNNull()
		{
			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entry, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB";
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			Factory.Save();

			AssertEquals(entry, processor.GetEntryHeaderFromMRNExposed(Factory, null, "ATB"));
		}

		public void TestGetEntryHeaderFromMRN_RefNrAndMRN_MRNExists()
		{
			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entry, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "MRN";
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			Factory.Save();

			AssertEquals(entry, processor.GetEntryHeaderFromMRNExposed(Factory, "MRN", "ATB"));
		}

		public void TestGetEntryHeaderFromMRN_RefNrAndMRN_RefNrExists()
		{
			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entry, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB";
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			Factory.Save();

			AssertEquals(entry, processor.GetEntryHeaderFromMRNExposed(Factory, "MRN", "ATB"));
		}

		public void TestGetEntryHeaderFromMRN_RefNrAmdMRNIsEmpty_EmptyCusEntryNumsExist()
		{
			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber1 = CusEntryNumber.LoadOrCreate(entry, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber1.CE_EntryNum = "";
			mrnEntryNumber1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			var mrnEntryNumber2 = CusEntryNumber.LoadOrCreate(entry, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber2.CE_EntryNum = "";
			mrnEntryNumber2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			Factory.Save();

			AssertNull(processor.GetEntryHeaderFromMRNExposed(Factory, null, null));
		}

		public void TestProcessNonMRNEntryTypes()
		{
			var germanySpecificEntryTypes = CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(Factory, "DE", isImport: false).GetAllCodes();
			var euTransitStatusCodes = CusEntryNumberTypes.EU.CommunityTransitStatusCodesList_Export.GetAllCodes();

			var entryTypes = germanySpecificEntryTypes.Except(euTransitStatusCodes).Where(type => type != CusEntryNumberTypes.Standard.MovementReferenceNumber);

			CombineAssertions(() =>
			{
				foreach (var entryType in entryTypes)
				{
					ProcessEntryType(entryType);
				}
			});
		}

		void ProcessEntryType(string entryType)
		{
			CusEntryHeader result = null;

			var processor = new DEBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation());
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entry, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryType = (ZString)entryType;
			mrnEntryNumber.CE_EntryNum = "MRN";
			Factory.Save();

			result = processor.GetEntryHeaderFromMRNExposed(Factory, "MRN", ZString.Empty);

			AssertNull($"{entryType}: No EntryHeader", result);
		}

		void RunMessageProcessor(EDIMessage[] messages, LoggingInformation logger)
		{
			using (Factory.AddDisposableService())
			{
				foreach (var message in messages)
				{
					new TemporaryStorageCUSTSTMessageProcessor(logger).PreProcessMessage(message);
					new TemporaryStorageCUSTSTMessageProcessor(logger).ProcessMessage(message);
				}
				Factory.Save();
			}
		}
	}

	class DEBranchCustomsApplicationTypeMessageProcessorForTest : DEBranchCustomsApplicationTypeMessageProcessor<AtlasEDIMessage>
	{
		public DEBranchCustomsApplicationTypeMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public CusReconDeclaration GetCusReconDeclarationFromMRNExposed(BusinessObjectFactory factory, ZGuid companyPK, string movementReferenceNumber, string atlasReferenceNumber = null) => GetCusReconDeclarationFromMRN(factory, companyPK, movementReferenceNumber, atlasReferenceNumber);

		public CusEntryHeader GetEntryHeaderFromMRNExposed(BusinessObjectFactory factory, string movementReferenceNumber, string atlasReferenceNumber) => GetEntryHeaderFromMRN(factory, movementReferenceNumber, atlasReferenceNumber);

		public CusEntryHeader GetLinkedObjectFromOriginalMessageOrMrnExposed(BusinessObjectFactory factory, string referencedMessageIdentifier, ZString movementReferenceNumber)
			=> GetLinkedObjectFromOriginalMessageOrMrn(factory, referencedMessageIdentifier, movementReferenceNumber);

		public T GetLinkedObjectFromMRNExposed<T>(BusinessObjectFactory factory, string movementReferenceNumber, string atlasReferenceNumber) where T : BusinessObject => GetLinkedObjectFromMRN<T>(factory, movementReferenceNumber, atlasReferenceNumber);

		protected override string MessageFriendlyNameCore => throw new NotImplementedException();

		protected override string ApplicationCodeCore => ApplicationCodes.DECustomsAtlasSystem;

		protected override bool DelayStatusError => throw new NotImplementedException();

		protected override List<AttachedDocument> GetAttachedDocuments(AtlasEDIMessage message)
		{
			throw new NotImplementedException();
		}

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			throw new NotImplementedException();
		}

		protected override BusinessObject GetLinkedObject(AtlasEDIMessage message)
		{
			throw new NotImplementedException();
		}

		protected override ZString GetMessageIdentifier(AtlasEDIMessage message)
		{
			throw new NotImplementedException();
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasEDIMessage message)
		{
			throw new NotImplementedException();
		}
	}
}
