using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	sealed class JobDeclarationDocumentSupporterTest : BaseJobDeclarationDocumentSupportTest
	{
		public override void TestGetFilterValueMSGBKR()
		{
			//Filter = MSGBKR
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("For filter 'MSGBKR' result is 'EXP'", "EXP", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("For filter 'MSGBKR' result is 'IMP'", "IMP", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));

			declaration.JE_MessageType = "LEX";
			AssertEquals("For filter 'MSGBKR' result is 'LEX'", "LEX", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));
		}

		public override void TestGetFilterValueMSGBKRCTY()
		{
			//Filter = MSGBKRCTY
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("For filter 'MSGBKRCTY' result is 'EXP' + Current Country Code", "EXP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("For filter 'MSGBKRCTY' result is 'IMP' + Current Country Code", "IMP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));

			declaration.JE_MessageType = "LEX";
			AssertEquals("For filter 'MSGBKRCTY' result is 'LEX' + Current Country Code", "LEX" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
		}

		public override void TestGetFilterValueMSGBKRCTYMOD()
		{
			//Filter = MSGBKRCTYMOD
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'EXP' + Current Country Code + 'AIR'", "EXP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AIR", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'EXP' + Current Country Code + 'SEA'", "EXP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "SEA", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMP' + Current Country Code + 'AIR'", "IMP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AIR", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMP' + Current Country Code + 'SEA'", "IMP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "SEA", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			declaration.JE_MessageType = "LEX";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMP' + Current Country Code + 'AIR'", "LEX" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AIR", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));

			declaration.JE_MessageType = "LEX";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMP' + Current Country Code + 'SEA'", "LEX" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "SEA", declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));
		}

		public override void TestGetDocBusinessObjects()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();

			AssertNotNull(declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null)[0]);
		}

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			return result;
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForRunningDocumentsTest
		{
			get
			{
				var declaration = (JobDeclaration)GetDocumentSupportableBusinessObjectForRunningDocuments();
				// Uncomment the below line to check if documents for import declaration are generated properly. Once all errors are resolved, this line will remain uncommented permanently.
				//KRCustomsRegistry.Instance.EnableToSaveImportDeclaration.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
				var messageTypeList = new CodeDescriptionPairList(declaration.Lookups.MessageTypeList);

				foreach (ICodeDescription messageType in messageTypeList)
				{
					declaration = (JobDeclaration)GetDocumentSupportableBusinessObjectForRunningDocuments();
					declaration.JE_MessageType = messageType.Code;
					if (messageType.Code == KRJobMessageTypeList.Codes.Export)
					{
						declaration.JE_MessageSubType = "E";
					}
					else
					{
						declaration.JE_MessageSubType = "01";
					}
					declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));
					SetupAdditionalData(ref declaration);
					yield return declaration;
				}
			}
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObjectForRunningDocuments()
		{
			var result = (JobDeclaration)base.GetDocumentSupportableBusinessObjectForRunningDocuments();
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(result.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			foreach (JobComInvoiceHeader invoice in result.Invoices)
			{
				invoice.JZ_ValuationDateOverride = ZDateTime.Empty;//It is important to keep one entry for KR. DocumentGeneratingActions defaults ToBeDelivered only when there is only one element = one entry
			}
			result.DoMerge();
			return result;
		}

		void SetupAdditionalData(ref JobDeclaration declaration)
		{
			var testData = new TestDataSetupHelper(Factory);
			if (declaration.IsExport)
			{
				var entryHeader = declaration.ActiveEntryHeaders[0];
				entryHeader.CH_VersionID = 1;
				var export830 = new ExportEntryHeaderCreator().Create(entryHeader);
				using (var stream = KRXmlObjectSerializer.Serialize(export830))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entryHeader, ElectronicDocumentTypeList.Codes._830, stream);
					AccumulativeAmendmentManager.AcceptCurrentSnapshot(entryHeader, ElectronicDocumentTypeList.Codes._830);
					Factory.Save();
				}

				var message5AS = entryHeader.Messages.AddNew();
				message5AS.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message5AS.EM_MessageType = ElectronicDocumentTypeList.Codes._5AS;
				message5AS.EM_ApplicationReference = "2";

				var fileReader = new TestFileReader(typeof(JobDeclarationDocumentSupporterTest));
				var messageText = fileReader.GetEmbeddedFileText(TestExportOutgoingFilesPath, "GOVCBR5AS_Amend.xml");
				message5AS.EM_MessageText = messageText;
				message5AS.EM_MessageSubType = "A";

				var messageDKJ = entryHeader.Messages.AddNew();
				messageDKJ.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				messageDKJ.EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
				messageDKJ.EM_ApplicationReference = "2";

				messageText = fileReader.GetEmbeddedFileText(TestExportOutgoingFilesPath, "GOVCBRDKJ_Test.xml");
				messageDKJ.EM_MessageText = messageText;
				messageDKJ.EM_MessageSubType = "B";

				Factory.Save();
			}
			else if (declaration.IsImport)
			{
				var entryHeader = declaration.ActiveEntryHeaders[0];

				var entryInstruction1 = (CusEntryInstruction)Declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_AgreedDutyRatePreferenceCode = "A";
				entryHeader.CH_CEI_Instruction = entryInstruction1.PK;

				var message = entryHeader.Messages.AddNew();
				message.EM_MessageType = ElectronicDocumentTypeList.Codes._5FV;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

				var fileReader = new TestFileReader(typeof(JobDeclarationDocumentSupporterTest));
				var messageText = fileReader.GetEmbeddedFileText(TestImportFilesPath, "GOVCBR5FV_0.xml");
				message.EM_MessageText = messageText;

				Factory.Save();

				var entryNum5UL = entryHeader.EntryNumbers.AddNew();
				entryNum5UL.CE_EntryType = "5UL";
				testData.Create5ULSnapshot(entryHeader);
			}
			else if (declaration.IsLocalExport)
			{
				var entryHeader = declaration.ActiveEntryHeaders[0];
				entryHeader.CH_MessageType = ElectronicDocumentTypeList.Codes._5DQ;
				entryHeader.CH_VersionID = 1;
				var originalHeader = new LocalExport5DQEntryHeaderCreator().Create(entryHeader);
				using (var stream = KRXmlObjectSerializer.Serialize(originalHeader))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entryHeader, ElectronicDocumentTypeList.Codes._5DQ, stream);
					AccumulativeAmendmentManager.AcceptCurrentSnapshot(entryHeader, ElectronicDocumentTypeList.Codes._5DQ);
					Factory.Save();
				}

				var message5DS = entryHeader.Messages.AddNew();
				message5DS.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message5DS.EM_MessageType = ElectronicDocumentTypeList.Codes._5DS;
				message5DS.EM_ApplicationReference = "2";

				var fileReader = new TestFileReader(typeof(JobDeclarationDocumentSupporterTest));
				var messageText = fileReader.GetEmbeddedFileText(TestLocalExportFilesPath, "GOVCBR5DS.xml");
				message5DS.EM_MessageText = messageText;
				message5DS.EM_MessageSubType = "1";
				Factory.Save();
			}
		}
		protected override string[] TablesToCollectQueriesFor => new string[]
			{
				CusEntrySnapshotSchema.Constants.TableName,
			};

		void AssertGetBODocDataProviders(string dataContexts, string menuName = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var contextValue = new DataContextValue(dataContexts);
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = menuName;

			AssertNull(supporter.GetBODocDataProviders(contextValue, menuItem));

			var declarationOneEntries = Factory.New<JobDeclaration>();
			declarationOneEntries.CustomsEntryHeaders.AddNew();
			supporter = (JobDeclarationDocumentSupporter)declarationOneEntries.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			var providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 1, providers.Length);

			var declarationWithMultipleEntries = Factory.New<JobDeclaration>();
			declarationWithMultipleEntries.CustomsEntryHeaders.AddNew();
			declarationWithMultipleEntries.CustomsEntryHeaders.AddNew();
			supporter = (JobDeclarationDocumentSupporter)declarationWithMultipleEntries.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 2, providers.Length);
		}

		public void TestCancellationMessageDataProviderTakesLastMessage()
		{
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryMessageBO);
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.CancellationOfExportDeclaration;

			var declarationOneEntriesMultipleMessage = Factory.New<JobDeclaration>();
			var entryWithMultipleMessages = declarationOneEntriesMultipleMessage.CustomsEntryHeaders.AddNew();
			var multipleMessage1 = entryWithMultipleMessages.Messages.AddNew();
			multipleMessage1.EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
			multipleMessage1.EM_MessageNum = "multiple1";
			Factory.Save();
			var multipleMessage2 = entryWithMultipleMessages.Messages.AddNew();
			var fileReader = new TestFileReader(typeof(JobDeclarationDocumentSupporterTest));
			var messageText = fileReader.GetEmbeddedFileText(TestExportOutgoingFilesPath, "GOVCBRDKJ_Test.xml");
			multipleMessage2.EM_MessageText = messageText;
			multipleMessage2.EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
			multipleMessage2.EM_MessageNum = "multiple2";
			Factory.Save();
			var multipleMessage3 = entryWithMultipleMessages.Messages.AddNew();
			multipleMessage3.EM_MessageType = "XXX";
			multipleMessage3.EM_MessageNum = "different message type";
			var supporter = (JobDeclarationDocumentSupporter)declarationOneEntriesMultipleMessage.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			var provider = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 1, provider.Length);
			AssertEquals("D", ((ExportCancellationDetails)provider[0].ParentBusinessObject).FaultParty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		void AssertGetBODocMessageDataProviders_AmendmentOfLocalExportDeclaration(string entryType, string messageType, string fileName)
		{
			var emptyDeclaration = Factory.New<JobDeclaration>();
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AmendmentOfLocalExportDeclaration;
			var supporter = (JobDeclarationDocumentSupporter)emptyDeclaration.DocumentSupporter;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryMessageBO);
			AssertNull(supporter.GetBODocDataProviders(contextValue, menuItem));

			var declarationOneMessage = new TestDataSetupHelper(Factory).SetLocalExportEntryData(Factory.New<JobDeclaration>(), entryType, "3271420001710", messageType, fileName);
			supporter = (JobDeclarationDocumentSupporter)declarationOneMessage.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			var providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 1, providers.Length);

			var declaratioMmultipleMessages = new TestDataSetupHelper(Factory).SetLocalExportEntryData(Factory.New<JobDeclaration>(), entryType, "3271420001711", messageType, fileName);
			var entry = declaratioMmultipleMessages.CustomsEntryHeaders[0];
			new TestDataSetupHelper(Factory).SetLocalExportMessageData(entry, messageType, fileName);
			new TestDataSetupHelper(Factory).SetLocalExportMessageData(entry, "XXX", fileName);
			AssertEquals(3, declaratioMmultipleMessages.CustomsEntryHeaders[0].Messages.Count);

			supporter = (JobDeclarationDocumentSupporter)declaratioMmultipleMessages.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 1, providers.Length);
		}

		void AssertGetBODocDataProvidersNotFoundMessage(ZString dataContexts, ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var contextValue = new DataContextValue(dataContexts);
			AssertEquals("No entry exists.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));

			declaration.CustomsEntryHeaders.AddNew();

			if (!messageType.IsEmpty)
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				AssertEquals("There are no relevant entries to print a document from.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));

				entry.Messages.AddNew().EM_MessageType = messageType;
				AssertEquals("There are no relevant entries to print a document from.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));
			}

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			AssertEquals(ZString.Empty, supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));
		}

		void AssertGetBODocSnapShotDataProviders(string englishMenuName, string snapshotType, string dataContexts)
		{
			var declaration = Factory.New<JobDeclaration>();

			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = englishMenuName;

			var contextValue = new DataContextValue(dataContexts);
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			AssertNull(supporter.GetBODocDataProviders(contextValue, menuItem));

			var declarationOneEntries = Factory.New<JobDeclaration>();
			var entry = declarationOneEntries.CustomsEntryHeaders.AddNew();
			Create5ULSnapshot(entry);

			supporter = (JobDeclarationDocumentSupporter)declarationOneEntries.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			var providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 1, providers.Length);

			var declarationWithMultipleEntries = Factory.New<JobDeclaration>();
			var entry1 = declarationWithMultipleEntries.CustomsEntryHeaders.AddNew();
			var entry2 = declarationWithMultipleEntries.CustomsEntryHeaders.AddNew();
			Create5ULSnapshot(entry1);
			Create5ULSnapshot(entry2);

			supporter = (JobDeclarationDocumentSupporter)declarationWithMultipleEntries.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 2, providers.Length);

			void Create5ULSnapshot(CusEntryHeader entry)
			{
				var entryNumber = entry.EntryNumbers.AddNew();
				entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
				entryNumber.CE_EntryNum = "AAA111";

				var sendingObject = new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
				var refundDetails = new GOVCBR5ULDetails(sendingObject);
				var header = new Import5ULCreator().Create(entry, refundDetails);
				using (var stream = KRXmlObjectSerializer.Serialize(header))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, snapshotType, stream);
					Factory.Save();
				}
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, snapshotType);
				Factory.Save();
			}
		}

		void AssertGetBODocLastMessageDataProviders(string dataContexts, string path, string xml, string menuName, string messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = menuName;
			var contextValue = new DataContextValue(dataContexts);
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals("No entry exists.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));
			AssertNull(supporter.GetBODocDataProviders(contextValue, menuItem));

			var entry = declaration.CustomsEntryHeaders.AddNew();
			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			AssertEquals("There are no relevant entries to print a document from.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));

			var fileReader = new TestFileReader(typeof(JobDeclarationDocumentSupporterTest));
			var messageText = fileReader.GetEmbeddedFileText(path, xml);

			var firstMessage = entry.Messages.AddNew();
			firstMessage.EM_MessageType = messageType;
			firstMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow;
			firstMessage.EM_MessageText = messageText;
			if (messageType == ElectronicDocumentTypeList.Codes._5TW)
			{
				firstMessage.EM_MessageSubType = "ONE";
			}

			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			supporter.DocumentGenerationActions[0].ToBeDelivered = true;

			var providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 1, providers.Length);
			AssertEquals(ZString.Empty, supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));

			var secondMessage = entry.Messages.AddNew();
			secondMessage.EM_MessageType = messageType;
			secondMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(1);
			secondMessage.EM_MessageText = messageText;
			if (messageType == ElectronicDocumentTypeList.Codes._5TW)
			{
				secondMessage.EM_MessageSubType = "ONE";
			}

			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 1, providers.Length);
			AssertEquals(ZString.Empty, supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));
		}

		public void TestForExportDeclarationCertificate()
		{
			AssertGetBODocDataProvidersNotFoundMessage(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO, ZString.Empty);
			AssertGetBODocDataProviders(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO, JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate);
		}

		public void TestForExportDeclarationCertificateEnglish()
		{
			AssertGetBODocDataProviders(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO, JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate_English);
		}

		public void TestGoodsRemovalPriorToCustomsRelease()
		{
			AssertGetBODocDataProvidersNotFoundMessage(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, ElectronicDocumentTypeList.Codes._5BD);
		}

		public void TestAgreedRateForAllLines()
		{
			AssertGetBODocDataProvidersNotFoundMessage(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, ElectronicDocumentTypeList.Codes._5BA);
		}

		public void TestGoldVATDeclaration()
		{
			AssertGetBODocDataProvidersNotFoundMessage(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, ElectronicDocumentTypeList.Codes._5TM);
		}

		public void TestApplyingTaxExemptionOrSpecificUseDutyRate()
		{
			AssertGetBODocDataProvidersNotFoundMessage(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, ElectronicDocumentTypeList.Codes._5FN);
		}

		public void TestImportTaxInvoiceForIndividualDeclaredCase()
		{
			var declaration = Factory.New<JobDeclaration>();
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ImportTaxInvoiceForIndividualDeclaredCase;

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryMessageBO);
			AssertNull(supporter.GetBODocDataProviders(contextValue, menuItem));

			var declarationOneEntries = Factory.New<JobDeclaration>();
			var message = declarationOneEntries.CustomsEntryHeaders.AddNew().Messages.AddNew();
			message.EM_MessageType = ElectronicDocumentTypeList.Codes._5FV;
			message.EM_MessageNum = "OneMessage";
			message.EM_MessageText = ZString.Empty;
			supporter = (JobDeclarationDocumentSupporter)declarationOneEntries.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			var providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 1, providers.Length);

			var declarationOneEntriesMultipleMessage = Factory.New<JobDeclaration>();
			var entryWithMultipleMessages = declarationOneEntriesMultipleMessage.CustomsEntryHeaders.AddNew();
			var multipleMessage1 = entryWithMultipleMessages.Messages.AddNew();
			multipleMessage1.EM_MessageText = ZString.Empty;
			multipleMessage1.EM_MessageType = ElectronicDocumentTypeList.Codes._5FV;
			multipleMessage1.EM_MessageNum = "multiple1";
			var multipleMessage2 = entryWithMultipleMessages.Messages.AddNew();
			multipleMessage2.EM_MessageText = ZString.Empty;
			multipleMessage2.EM_MessageType = ElectronicDocumentTypeList.Codes._5FV;
			multipleMessage2.EM_MessageNum = "multiple2";
			var multipleMessage3 = entryWithMultipleMessages.Messages.AddNew();
			multipleMessage3.EM_MessageText = ZString.Empty;
			multipleMessage3.EM_MessageType = "XXX";
			multipleMessage3.EM_MessageNum = "different message type";
			supporter = (JobDeclarationDocumentSupporter)declarationOneEntriesMultipleMessage.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 2, providers.Length);
		}
		public void TestCancellationOfImportDeclaration()
		{
			AssertGetBODocDataProvidersNotFoundMessage(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, ElectronicDocumentTypeList.Codes._5BF);
		}
		public void TestCorrectionNoticeOfCountryOfOrigin()
		{
			AssertGetBODocDataProvidersNotFoundMessage(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, ElectronicDocumentTypeList.Codes._5GU);
		}
		public void TestExemptionRequestOfPenaltyNotice()
		{
			AssertGetBODocDataProvidersNotFoundMessage(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, ElectronicDocumentTypeList.Codes._5UB);
		}
		public void TestImportSupplementNotice()
		{
			AssertGetBODocDataProvidersNotFoundMessage(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, ElectronicDocumentTypeList.Codes._5GV);
		}

		public void TestRefundRequest()
		{
			AssertGetBODocSnapShotDataProviders(JobDeclarationDocumentSupporter.MenuNames.RefundRequest, ElectronicDocumentTypeList.Codes._5UL, JobDeclarationDocumentSupporter.DataContexts.EntrySnapshot);
		}

		public void TestAmendmentOfExportDeclarationMessages()
		{
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration;

			var declaration = Factory.New<JobDeclaration>();
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO);
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals("No entry exists.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			var entry = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals("No entry accepted.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			using (var stream = KRXmlObjectSerializer.Serialize(new ExportEntryHeaderCreator().Create(entry)))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));
				entry.Factory.Save();
			}

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals(ZString.Empty, supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAmendmentOfExportDeclaration()
		{
			var factory = Factory;
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "22926");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			entry.EntryNumber = "2292620004191X";

			Create830Snapshot();
			var message830 = entry.Messages.AddNew();
			message830.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message830.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message830.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = declaration.CustomsEntryHeaders[0];
			invoice = declaration.Invoices[0];

			invoice.JZ_IncoTerm = IncotermList.Codes.FreeOnBoard;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			factory.Save();

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration;

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO);
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);

			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(true, supporter.DocumentGenerationActions[0].IncludingCurrentDifference);

			var dataProviders = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals(1, dataProviders.Length);

			var amendmentDetails_1 = (ExportAmendmentDetails)dataProviders[0].ParentBusinessObject;
			AssertEquals(1, amendmentDetails_1.HeaderAmendedItems.Count);
			AssertEquals("A705", amendmentDetails_1.HeaderAmendedItems[0].AmendmentItem.AmendDataItemID);
			AssertEquals("", amendmentDetails_1.HeaderAmendedItems[0].AmendmentItem.BeforeDescription);
			AssertEquals("FOB", amendmentDetails_1.HeaderAmendedItems[0].AmendmentItem.AfterDescription);

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = declaration.CustomsEntryHeaders[0];
			invoice = declaration.Invoices[0];
			entry.CH_VersionID = 1;

			Create830Snapshot();
			var message5AS_1 = entry.Messages.AddNew();
			message5AS_1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message5AS_1.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5AS_1.EM_MessageType = ElectronicDocumentTypeList.Codes._5AS;
			message5AS_1.EM_ApplicationReference = "2";
			var fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Export\Outgoing\GOVCBR5AS_Amend_1.xml"));
			message5AS_1.SetEM_MessageTextOrDataSource(fileStream);
			factory.Save();

			invoice.JZ_IncoTerm = IncotermList.Codes.CostInsuranceAndFreight;
			invoice.JZ_PaymentTerms = SettlementMethodCodeList.Codes.LS;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			factory.Save();

			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);

			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(false, supporter.DocumentGenerationActions[0].IncludingCurrentDifference);

			dataProviders = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals(1, dataProviders.Length);

			var amendmentDetails_2 = (ExportAmendmentDetails)dataProviders[0].ParentBusinessObject;
			AssertEquals(1, amendmentDetails_2.HeaderAmendedItems.Count);
			AssertEquals("A705", amendmentDetails_2.HeaderAmendedItems[0].AmendmentItem.AmendDataItemID);
			AssertEquals("", amendmentDetails_2.HeaderAmendedItems[0].AmendmentItem.BeforeDescription);
			AssertEquals("FOB", amendmentDetails_2.HeaderAmendedItems[0].AmendmentItem.AfterDescription);

			supporter.DocumentGenerationActions[0].IncludingCurrentDifference = true;

			dataProviders = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals(2, dataProviders.Length);

			var amendmentDetails_3 = (ExportAmendmentDetails)dataProviders[0].ParentBusinessObject;
			AssertEquals(1, amendmentDetails_3.HeaderAmendedItems.Count);
			AssertEquals("A705", amendmentDetails_3.HeaderAmendedItems[0].AmendmentItem.AmendDataItemID);
			AssertEquals("", amendmentDetails_3.HeaderAmendedItems[0].AmendmentItem.BeforeDescription);
			AssertEquals("FOB", amendmentDetails_3.HeaderAmendedItems[0].AmendmentItem.AfterDescription);

			var amendmentDetails_4 = (ExportAmendmentDetails)dataProviders[1].ParentBusinessObject;
			AssertEquals(2, amendmentDetails_4.HeaderAmendedItems.Count);
			AssertEquals("A107", amendmentDetails_4.HeaderAmendedItems[0].AmendmentItem.AmendDataItemID);
			AssertEquals("", amendmentDetails_4.HeaderAmendedItems[0].AmendmentItem.BeforeDescription);
			AssertEquals("LS", amendmentDetails_4.HeaderAmendedItems[0].AmendmentItem.AfterDescription);
			AssertEquals("A705", amendmentDetails_4.HeaderAmendedItems[1].AmendmentItem.AmendDataItemID);
			AssertEquals("FOB", amendmentDetails_4.HeaderAmendedItems[1].AmendmentItem.BeforeDescription);
			AssertEquals("CIF", amendmentDetails_4.HeaderAmendedItems[1].AmendmentItem.AfterDescription);

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = declaration.CustomsEntryHeaders[0];
			entry.CH_VersionID = 2;
			invoice = declaration.Invoices[0];

			Create830Snapshot();
			var message5AS_2 = entry.Messages.AddNew();
			message5AS_2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message5AS_2.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5AS_2.EM_MessageType = ElectronicDocumentTypeList.Codes._5AS;
			message5AS_2.EM_ApplicationReference = "3";
			fileStream = File.OpenRead(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\KR\Business.Test\TestFiles\Export\Outgoing\GOVCBR5AS_Amend_2.xml"));
			message5AS_2.SetEM_MessageTextOrDataSource(fileStream);
			factory.Save();

			invoice.JZ_PaymentTerms = SettlementMethodCodeList.Codes.LH;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			factory.Save();

			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);

			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			AssertEquals(false, supporter.DocumentGenerationActions[0].IncludingCurrentDifference);

			dataProviders = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals(2, dataProviders.Length);

			var amendmentDetails_5 = (ExportAmendmentDetails)dataProviders[0].ParentBusinessObject;
			AssertEquals(1, amendmentDetails_5.HeaderAmendedItems.Count);
			AssertEquals("A705", amendmentDetails_5.HeaderAmendedItems[0].AmendmentItem.AmendDataItemID);
			AssertEquals("", amendmentDetails_5.HeaderAmendedItems[0].AmendmentItem.BeforeDescription);
			AssertEquals("FOB", amendmentDetails_5.HeaderAmendedItems[0].AmendmentItem.AfterDescription);

			var amendmentDetails_6 = (ExportAmendmentDetails)dataProviders[1].ParentBusinessObject;
			AssertEquals(2, amendmentDetails_6.HeaderAmendedItems.Count);
			AssertEquals("A107", amendmentDetails_6.HeaderAmendedItems[0].AmendmentItem.AmendDataItemID);
			AssertEquals("", amendmentDetails_6.HeaderAmendedItems[0].AmendmentItem.BeforeDescription);
			AssertEquals("LS", amendmentDetails_6.HeaderAmendedItems[0].AmendmentItem.AfterDescription);
			AssertEquals("A705", amendmentDetails_6.HeaderAmendedItems[1].AmendmentItem.AmendDataItemID);
			AssertEquals("FOB", amendmentDetails_6.HeaderAmendedItems[1].AmendmentItem.BeforeDescription);
			AssertEquals("CIF", amendmentDetails_6.HeaderAmendedItems[1].AmendmentItem.AfterDescription);

			supporter.DocumentGenerationActions[0].IncludingCurrentDifference = true;

			dataProviders = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals(3, dataProviders.Length);

			var amendmentDetails_7 = (ExportAmendmentDetails)dataProviders[0].ParentBusinessObject;
			AssertEquals(1, amendmentDetails_7.HeaderAmendedItems.Count);
			AssertEquals("A705", amendmentDetails_7.HeaderAmendedItems[0].AmendmentItem.AmendDataItemID);
			AssertEquals("", amendmentDetails_7.HeaderAmendedItems[0].AmendmentItem.BeforeDescription);
			AssertEquals("FOB", amendmentDetails_7.HeaderAmendedItems[0].AmendmentItem.AfterDescription);

			var amendmentDetails_8 = (ExportAmendmentDetails)dataProviders[1].ParentBusinessObject;
			AssertEquals(2, amendmentDetails_8.HeaderAmendedItems.Count);
			AssertEquals("A107", amendmentDetails_8.HeaderAmendedItems[0].AmendmentItem.AmendDataItemID);
			AssertEquals("", amendmentDetails_8.HeaderAmendedItems[0].AmendmentItem.BeforeDescription);
			AssertEquals("LS", amendmentDetails_8.HeaderAmendedItems[0].AmendmentItem.AfterDescription);
			AssertEquals("A705", amendmentDetails_8.HeaderAmendedItems[1].AmendmentItem.AmendDataItemID);
			AssertEquals("FOB", amendmentDetails_8.HeaderAmendedItems[1].AmendmentItem.BeforeDescription);
			AssertEquals("CIF", amendmentDetails_8.HeaderAmendedItems[1].AmendmentItem.AfterDescription);

			var amendmentDetails_9 = (ExportAmendmentDetails)dataProviders[2].ParentBusinessObject;
			AssertEquals(1, amendmentDetails_9.HeaderAmendedItems.Count);
			AssertEquals("A107", amendmentDetails_9.HeaderAmendedItems[0].AmendmentItem.AmendDataItemID);
			AssertEquals("LS", amendmentDetails_9.HeaderAmendedItems[0].AmendmentItem.BeforeDescription);
			AssertEquals("LH", amendmentDetails_9.HeaderAmendedItems[0].AmendmentItem.AfterDescription);

			void Create830Snapshot()
			{
				var export830 = new ExportEntryHeaderCreator().Create(entry);
				using (var stream = KRXmlObjectSerializer.Serialize(export830))
				{
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._830, stream);
					AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._830);
					factory.Save();
				}
			}
		}

		public void TestLocalExportDeclaration()
		{
			AssertGetBODocDataProviders(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO, JobDeclarationDocumentSupporter.MenuNames.LocalExportDeclaration);
		}

		public void TestForAmendmentOfLocalExportDeclaration_5DS()
		{
			AssertGetBODocMessageDataProviders_AmendmentOfLocalExportDeclaration(ElectronicDocumentTypeList.Codes._5DQ, ElectronicDocumentTypeList.Codes._5DS, "GOVCBR5DS_Test.xml");
		}

		public void TestForAmendmentOfLocalExportDeclaration_5DR()
		{
			AssertGetBODocMessageDataProviders_AmendmentOfLocalExportDeclaration(ElectronicDocumentTypeList.Codes._5DP, ElectronicDocumentTypeList.Codes._5DR, "GOVCBR5DR_Test.xml");
		}

		public void TestGetEntryHeaderBODataProvider()
		{
			AssertGetBODocDataProviders(JobDeclarationDocumentSupporter.DataContexts.EXPEntryHeaderBO);
			AssertGetBODocDataProviders(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO);
			AssertGetBODocDataProviders(JobDeclarationDocumentSupporter.DataContexts.LEXEntryHeaderBO);
		}

		public void TestExportVehicleNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals("No entry exists.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals("There is no entry which has second-hand vehicles under any of its invoice lines.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			invoiceLine.VehicleNumbers.AddNew();
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertNotEquals("There is no entry which has second-hand vehicles under any of its invoice lines.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			AssertEquals(ZString.Empty, supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			AssertGetBODocDataProviders(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO, JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo);
		}

		public void TestReImportOfExportedGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals("No entry exists.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);

			AssertEquals("There is no entry having goods which are previously exported.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			entryLine.PreviousExpDecLineCollection.AddNew();
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertNotEquals("There is no entry having goods which are previously exported.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			AssertEquals(ZString.Empty, supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			AssertGetBODocDataProviders(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO, JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods);
		}

		public void TestExportDeclarationCertificate()
		{
			var entry = new TestDataSetupHelper(Factory).GetExportEntryWithFullData();
			var supporter = (JobDeclarationDocumentSupporter)entry.Declaration.DocumentSupporter;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate;

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			var providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			var wrapper = (ExportEntryHeaderWrapper)providers[0].ParentBusinessObject;

			AssertEquals("11", wrapper.Header.TransactionType);
			AssertEquals("B", wrapper.Header.ExportTypeCode);

			using (var stream = KRXmlObjectSerializer.Serialize(new ExportEntryHeaderCreator().Create(entry)))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));
				entry.Factory.Save();
			}

			entry.Declaration.JE_ExportGoodsType = "20";
			entry.Declaration.JE_MessageSubType = "A";

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			wrapper = (ExportEntryHeaderWrapper)providers[0].ParentBusinessObject;

			AssertEquals("11", wrapper.Header.TransactionType);
			AssertEquals("B", wrapper.Header.ExportTypeCode);

			using (var stream = KRXmlObjectSerializer.Serialize(new ExportEntryHeaderCreator().Create(entry)))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
				snapshot.CES_VersionNumber = (ZShort)2;
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));
				entry.Factory.Save();
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			wrapper = (ExportEntryHeaderWrapper)providers[0].ParentBusinessObject;

			AssertEquals("20", wrapper.Header.TransactionType);
			AssertEquals("A", wrapper.Header.ExportTypeCode);
		}

		public void TestImportDeclarationCertificate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ReImportOfExportedGoods;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_PaymentMethod = "13";
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;

			var supporter = (JobDeclarationDocumentSupporter)entry.Declaration.DocumentSupporter;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO);

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			var providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			var wrapper = (ImportEntryHeaderWrapper)providers[0].ParentBusinessObject;

			AssertEquals("13", wrapper.Header.PaymentType);
			AssertEquals("B", wrapper.Header.ImporterType);

			using (var stream = KRXmlObjectSerializer.Serialize(new ImportEntryHeaderCreator().Create(entry)))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._929);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));
				entry.Factory.Save();
			}

			declaration.JE_PaymentMethod = "20";
			declaration.JE_PaidBy = PaidByCodeList.Codes.CLI;

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			wrapper = (ImportEntryHeaderWrapper)providers[0].ParentBusinessObject;

			AssertEquals("13", wrapper.Header.PaymentType);
			AssertEquals("B", wrapper.Header.ImporterType);

			using (var stream = KRXmlObjectSerializer.Serialize(new ImportEntryHeaderCreator().Create(entry)))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._929);
				snapshot.CES_VersionNumber = (ZShort)2;
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));
				entry.Factory.Save();
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			wrapper = (ImportEntryHeaderWrapper)providers[0].ParentBusinessObject;

			AssertEquals("20", wrapper.Header.PaymentType);
			AssertEquals("A", wrapper.Header.ImporterType);
		}

		public void TestFTAHeader()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.DetailedFTACountries, "Detailed FTA Countries (DHR)");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "IN", "인도", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ApplicationofFTARate;

			var invoiceHeaderSupplier = Factory.NewWithValidTestData<OrgHeader>();
			invoiceHeaderSupplier.CustomsCodes.AddNew(Constants.IdentificationType.CertificateOfOriginExporterNumber, "P641150121378");

			var declaration = Factory.New<JobDeclaration>();

			var importer = CreateImporter();
			importer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;
			var supplier = CreateOrganisation("PARAGON INTERNATIONAL", "MARK CONNEL", "18444893773", "18444893772", Core.Constants.CountryCodes.UnitedStates);
			declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;
			var manufacturer = CreateOrganisation("BOSSY GLOBAL", "BRETT PAINE", "67358901235", "67358901234", Core.Constants.CountryCodes.UnitedKingdom);
			declaration.JE_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_ExportDate = new ZDateTime(2021, 01, 01);
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_TransshipmentDate = new ZDateTime(2021, 01, 02);
			declaration.JE_TransshipmentPort = "SGSIN";
			AssertEquals("SG", declaration.TransshipmentCountryCode);
			AssertEquals(YesNo.Yes, declaration.TransshipmentYN);

			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_FTARelationArticleCode = "4";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Supplier = invoiceHeaderSupplier.PK;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.CusEntryLine.CL_LineNumber = 1;
			invoiceLine.CusEntryLine.CL_FTASequenceNumber = 1;

			invoiceLine.CertificateOfOriginIssueStatus = "G";
			var certificateOfOrigin = invoiceLine.CertificateOfOriginData;
			certificateOfOrigin.CSI_RN_NKCountryCode = Core.Constants.CountryCodes.Belarus;
			certificateOfOrigin.CSI_DateOfIssue = new ZDateTime(2021, 4, 1);
			certificateOfOrigin.CSI_ReferenceNumber = "800324356053";
			certificateOfOrigin.CSI_IssuerType = "0";
			certificateOfOrigin.CSI_Description = "FIRST CERTIFICATE OF ORIGIN";
			certificateOfOrigin.CSI_Quantity3 = 12.34;
			certificateOfOrigin.CSI_UnitOfQuantity = Core.Constants.Weight.Grams;

			var supporter = (JobDeclarationDocumentSupporter)entry.Declaration.DocumentSupporter;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO);

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			var providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			var wrapper = (FTAHeaderWrapper)providers[0].ParentBusinessObject;

			AssertNotNull(wrapper.Header);
			AssertNull(wrapper.DHRHeader);
			AssertEquals("4", wrapper.Header.LawCode);

			using (var stream = KRXmlObjectSerializer.Serialize(new ImportFTACreator().Create(entry)))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._5SC);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));
				entry.Factory.Save();
			}

			entryInstruction.CEI_FTARelationArticleCode = "5";

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			wrapper = (FTAHeaderWrapper)providers[0].ParentBusinessObject;

			AssertEquals("4", wrapper.Header.LawCode);

			entry.Snapshots.RemoveAndDeleteAll();
			invoiceLine.JI_CountryOfOrigin = Enterprise.Core.Constants.CountryCodes.India;

			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			wrapper = (FTAHeaderWrapper)providers[0].ParentBusinessObject;

			AssertNotNull(wrapper.Header);
			AssertNotNull(wrapper.DHRHeader);
			AssertEquals("5", wrapper.Header.LawCode);

			using (var stream = KRXmlObjectSerializer.Serialize(new ImportDHRCreator().Create(entry)))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._DHR);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));
				entry.Factory.Save();
			}

			entryInstruction.CEI_FTARelationArticleCode = "1";
			AssertEquals("5", wrapper.Header.LawCode);
		}

		public void TestCreateNewSnapshotAndDistinguishToCurrentJob()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];

			using (var stream = KRXmlObjectSerializer.Serialize(new ExportEntryHeaderCreator().Create(entry)))
			{
				var snapshot = entry.Snapshots.AddNew(ElectronicDocumentTypeList.Codes._830);
				snapshot.CES_VersionNumber = (ZShort)1;
				snapshot.CES_Status = EntrySnapshotStatus.Lodged;
				snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;
				snapshot.SetCES_SnapshotXmlSource(new TextReaderSource(stream));
				entry.Factory.Save();
			}

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.ExportVehicleNo;

			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryOrSnapshotBO);
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);

			AssertEquals("There is no entry which has second-hand vehicles under any of its invoice lines.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			VehicleNumber vehicle = invoiceLine.VehicleNumbers.AddNew();
			vehicle.CY_Order = 1;
			vehicle.CY_Data = "KN3HNP6N18K283119";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);

			AssertEquals(true, entry.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.VehicleNumbers.Any()));
			AssertEquals("There is no entry which has second-hand vehicles under any of its invoice lines.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));
		}
		public void TestNoticeOfTaxAdjustment()
		{
			AssertGetBODocLastMessageDataProviders(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, TestImportFilesPath, "GOVCBR5WN_CUS.xml", JobDeclarationDocumentSupporter.MenuNames.NoticeOfTaxAdjustment, ElectronicDocumentTypeList.Codes._5WN);
		}
		public void TestNoticeOfFinalizedRefund()
		{
			AssertGetBODocLastMessageDataProviders(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, TestImportFilesPath, "GOVCBR5UO_CUS.xml", JobDeclarationDocumentSupporter.MenuNames.NoticeOfFinalizedRefund, ElectronicDocumentTypeList.Codes._5UO);
		}
		public void TestNoticeOfCustomsMandatedAmendment()
		{
			AssertGetBODocLastMessageDataProviders(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, TestImportFilesPath, "GOVCBR5TV_CUS.xml", JobDeclarationDocumentSupporter.MenuNames.NoticeOfCustomsMandatedAmendment, ElectronicDocumentTypeList.Codes._5TV);
		}

		public void TestNoticeofcorrectionReviewResults()
		{
			AssertGetBODocLastMessageDataProviders(JobDeclarationDocumentSupporter.DataContexts.IMPEntryHeaderBO, TestImportFilesPath, "GOVCBR5TW_ONE.xml", JobDeclarationDocumentSupporter.MenuNames.NoticeOfCorrectionReviewResults, ElectronicDocumentTypeList.Codes._5TW);
		}

		public void TestCancellationOfExportDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.CancellationOfExportDeclaration;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryMessageBO);
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals("No entry exists.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));
			AssertNull(supporter.GetBODocDataProviders(contextValue, menuItem));

			var entry = declaration.CustomsEntryHeaders.AddNew();
			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals("Cancellation has never been sent.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			var fileReader = new TestFileReader(typeof(JobDeclarationDocumentSupporterTest));
			var messageText = fileReader.GetEmbeddedFileText(TestExportIncomingFilesPath, "GOVCBR5AF_DKJ.xml");
			var message5AF = entry.Messages.AddNew();
			message5AF.EM_MessageType = ElectronicDocumentTypeList.Codes._5AF;
			message5AF.EM_SystemCreateTimeUtc = DateTime.UtcNow;
			message5AF.EM_MessageText = messageText;
			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals("Cancellation has never been sent.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));

			messageText = fileReader.GetEmbeddedFileText(TestExportOutgoingFilesPath, "GOVCBRDKJ_Test.xml");
			var messageDKJ = entry.Messages.AddNew();
			messageDKJ.EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
			messageDKJ.EM_SystemCreateTimeUtc = DateTime.UtcNow;
			messageDKJ.EM_MessageText = messageText;
			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			supporter.DocumentGenerationActions[0].ToBeDelivered = true;

			var providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 1, providers.Length);
			AssertEquals(ZString.Empty, supporter.GetBODocDataProvidersNotFoundMessage(contextValue, menuItem));
		}

		public void TestRequestToExtendReExportDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = JobDeclarationDocumentSupporter.MenuNames.RequestToExtendReExportDate;
			var contextValue = new DataContextValue(JobDeclarationDocumentSupporter.DataContexts.EntryMessageBO);
			var supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals("No entry exists.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));
			AssertNull(supporter.GetBODocDataProviders(contextValue, menuItem));

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = "D72";
			entryNum.CE_EntryLineReference = "2";

			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			AssertEquals("There are no relevant entries to print a document from.", supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));

			var fileReader = new TestFileReader(typeof(JobDeclarationDocumentSupporterTest));
			var messageText = fileReader.GetEmbeddedFileText(TestImportOutgoingFilesPath, "GOVCBRD72_Result_D1.xml");

			var firstMessage = entry.Messages.AddNew();
			firstMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._D72;
			firstMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow;
			firstMessage.EM_MessageText = messageText;
			firstMessage.EM_ApplicationReference = "1";

			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			supporter.DocumentGenerationActions.InitialiseFor(menuItem);
			AssertEquals(1, supporter.DocumentGenerationActions.Count);
			supporter.DocumentGenerationActions[0].ToBeDelivered = true;

			var providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 1, providers.Length);
			AssertEquals(ZString.Empty, supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));

			var secondMessage = entry.Messages.AddNew();
			secondMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._D72;
			secondMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(1);
			secondMessage.EM_MessageText = messageText;
			secondMessage.EM_ApplicationReference = "2";

			var threeMessage = entry.Messages.AddNew();
			threeMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._D72;
			threeMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(1);
			threeMessage.EM_MessageText = messageText;
			threeMessage.EM_ApplicationReference = "2";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);

			supporter = (JobDeclarationDocumentSupporter)declaration.DocumentSupporter;
			foreach (DocumentGeneratingAction action in supporter.DocumentGenerationActions)
			{
				action.ToBeDelivered = true;
			}
			providers = supporter.GetBODocDataProviders(contextValue, menuItem);
			AssertEquals("Document Provider", 2, providers.Length);
			AssertEquals(ZString.Empty, supporter.GetBODocDataProvidersNotFoundMessage(contextValue, null));
		}

		OrgHeader CreateImporter()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "윤민용";

			var contact = importer.Contacts.AddNew();
			contact.OC_ContactName = "윤민용";

			var mainAddress = importer.MainAddress;
			mainAddress.OA_CompanyNameOverride = importer.OH_FullName;
			mainAddress.Address1 = "인천광역시 남동구 청능대로718번길 7 (논현동 소래마을풍림아파트) 105동 503호";
			mainAddress.OA_Address2 = "상세주소";
			mainAddress.OA_PostCode = "21670";
			mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			mainAddress.OA_Fax = "01056987349";
			mainAddress.OA_Phone = "01044587479";
			mainAddress.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.RoadNameCode, "101010");
			mainAddress.OA_Email = "test@skorea.com";
			mainAddress.CustomsCodes.AddNew(MasterFiles.Business.CountryCompliance.KoreaSouthComplianceInfo.CodeTypes.BuildingNumber, "10201");

			importer.CustomsCodes.AddNew(Constants.IdentificationType.UnipassIDForIndividual, "ID345876");
			return importer;
		}

		OrgHeader CreateOrganisation(string companyName, string contactName, string fax, string phone, string countryCode)
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Category = OrgConstants.Category.Business;
			supplier.OH_FullName = companyName;

			var suppliercontact = supplier.Contacts.AddNew();
			suppliercontact.OC_ContactName = contactName;

			var supplierAddress = supplier.MainAddress;
			supplierAddress.OA_Fax = fax;
			supplierAddress.OA_Phone = phone;
			supplierAddress.OA_RN_NKCountryCode = countryCode;
			return supplier;
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = base.MaxDBHitCounts;
				maxHits.Add("TariffView", 3);
				maxHits.Add("CusCodeData", 2);
				maxHits.Add("JobComInvLineRefs", 3);
				maxHits.Add("CusHouseContPackInvoiceLinePivot", 2);
				maxHits.Add("OrgCusCode", 3);
				maxHits.Add("EDIMessage", 2);
				return maxHits;
			}
		}

		const string TestExportOutgoingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing";
		const string TestExportIncomingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
		const string TestImportFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
		const string TestImportOutgoingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
		const string TestLocalExportFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.LocalExport.Outgoing";
	}
}
