using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.FR.Registry.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	sealed class DeltaGMessageSenderTest : TestCaseWithFactory
	{
		public void TestInvalidCastExceptionShouldBeReported()
		{
			var declaration = CreateDeclaration(true);
			var additionalInfo = declaration.AdditionalInfos.AddNew(typeof(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo));
			var entry = declaration.CustomsEntryHeaders[0];
			var messageObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			messageObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAL;
			messageObject.SequenceNumber = 0;
			var errorCollector = new ErrorCollector();
			var sender = new DeltaGMessengerForTest(messageObject, errorCollector);
			var result = sender.Send();
			AssertEquals("No message is created.", 0, entry.Messages.Count);
			AssertContains("Failed to send message.", MessageSender<DeltaGJobDeclarationMessageSendingObject>.MessageSendFailure, result);
			AssertType<InvalidCastException>("An InvalidCastException is reported.", ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		public void TestCreateEntrySnapshotIfApplicable()
		{
			var declaration = CreateDeclaration(true, "", 0m);
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RepresentativeID = "TESTREPID";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;
			orgCusAccount.CZ_OH = declaration.Importer.PK;

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			declaration.JE_CustomsProfile = "TESTACC";

			Factory.Save();

			var invoice = declaration.Invoices[0];
			invoice.SupportingDocuments.RemoveAndDeleteAll();

			var invoiceLine = invoice.InvoiceLines[0];

			var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();
			invoiceSupportingDocument.CSI_Code = "HDOC";
			invoiceSupportingDocument.CSI_ReferenceNumber = "INVOICEHEADER_DOCUMENT";
			invoiceSupportingDocument.CSI_DateOfIssue = ZDate.Today;

			var invoiceLineSupportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			invoiceLineSupportingDocument1.CSI_Code = "LDOC";
			invoiceLineSupportingDocument1.CSI_ReferenceNumber = "INVOICELINE_DOCUMENT";
			invoiceLineSupportingDocument1.CSI_DateOfIssue = ZDate.Today;

			var invoiceLineDTP = invoiceLine.SupportingDocuments.AddNew();
			invoiceLineDTP.CSI_Code = "LDTP";
			invoiceLineDTP.CSI_ReferenceNumber = "";
			invoiceLineDTP.CSI_IsDTP = true;

			invoiceLine.JI_SupplementaryCode1 = "CAC1";
			invoiceLine.JI_SupplementaryCode2 = "CAC2";

			var cana1 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana1.CY_Code = "V910";

			var cana2 = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			cana2.CY_Code = "V911";

			Factory.Save();

			var merger = new Declaration.LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var messageObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = FR.Business.EntryActionCodeList.Codes.VAL;
			messageObject.SequenceNumber = 0;
			var errorCollector = new ErrorCollector();
			var sender = new DeltaGMessengerForTest(messageObject, errorCollector)
			{
				MessageDecorator = x => x.EM_MessageOwner = "The IMP"
			};
			sender.Send();

			AssertEquals(1, entryHeader.Snapshots.Count);

			var expectedSnapshotXml = @"<CusEntryLine><LineNumber>1</LineNumber><ChildData Type=""DOC""><Code>HDOC</Code><Reference>INVOICEHEADER_DOCUMENT</Reference></ChildData><ChildData Type=""DOC""><Code>LDOC</Code><Reference>INVOICELINE_DOCUMENT</Reference></ChildData><ChildData Type=""DOC""><Code>LDTP</Code></ChildData><ChildData Type=""CAN""><Code>V910</Code></ChildData><ChildData Type=""CAN""><Code>V911</Code></ChildData><ChildData Type=""CAC""><Code>CAC1</Code></ChildData><ChildData Type=""CAC""><Code>CAC2</Code></ChildData></CusEntryLine></FrenchEntryLineChildSnapshot>";
			AssertContains(expectedSnapshotXml, entryHeader.Snapshots[0].CES_SnapshotXml);
			AssertEquals(Customs.Business.AccumulativeAmendment.EntrySnapshotStatus.Current, entryHeader.Snapshots[0].CES_Status);
			AssertEquals(FR.Business.EntryActionCodeList.Codes.VAL, entryHeader.Snapshots[0].CES_MessageType);
		}

		[TestDate(2019, 12, 31)]
		public void TestSendMessage()
		{
			TestSendMessage(false, expectedMessageText);
		}

		[TestDate(2019, 12, 31)]
		public void TestSendMessageWithAI2Permit()
		{
			messageType = FR.Business.EntryActionCodeList.Codes.VAL;
			TestSendMessage(true, expectedPermitMessageText);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, cusEntryHeaderItem?.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending).Count());
			AssertEquals((ZDecimal)(-300), transactions[0].CPL_TranValue);
			AssertEquals("AI2", transactions[0].CPL_Procedure);
			AssertEquals("Customs Entry - AI2", transactions[0].CPL_Comment);
		}

		[TestDate(2019, 12, 31)]
		public void TestSendMessageWhenDeltaGFallBackIsActive()
		{
			TestSendMessage(true, expectedMessageText);
		}

		void TestSendMessage(bool fallBackIsActive, string expectedText, bool amountAndTypeGenerated = false, bool setupGuaranteesAi2 = true, string entrystyle = "", decimal lineprice = 50)
		{
			FRCustomsDataRegistryTest.SetDeltaGFallbackIsActive(fallBackIsActive);

			var message = CreateMessageObject(amountAndTypeGenerated, setupGuaranteesAi2, entrystyle, lineprice);

			var errorCollector = new ErrorCollector();
			var sender = new DeltaGMessengerForTest(message, errorCollector)
			{
				MessageDecorator = x => x.EM_MessageOwner = "The IMP"
			};
			Assert(sender.ShouldIncreaseSequenceNumber);

			var oldsequencenumber = cusEntryHeaderItem.CH_SequenceNumber;
			var result = sender.Send();
			AssertEquals("sequence number have increase", cusEntryHeaderItem.CH_SequenceNumber, oldsequencenumber + 1);
			var msg = cusEntryHeaderItem?.Messages[0];

			cusEntryHeaderItem.Reload();
			AssertNotNull("No message was created, but one should have been - refer to the following and ensure all mandatory data fields are set. Errors: " + errorCollector.GetErrorsAsString(), msg);

			var messageText = msg.EM_MessageText;

			AssertContains("<Message ", messageText);
			AssertContains(expectedText, messageText);
			AssertContains(cusEntryHeaderItem.Declaration.JobNumber, messageText);
			AssertEquals("The MessageDecorator should take effect on the message.", "The IMP", msg.EM_MessageOwner);

			if (fallBackIsActive)
			{
				AssertEquals("Entry should have a fallback entry number.", "0000000001", cusEntryHeaderItem.FRCustomsFallbackNumber);
				AssertEquals(ZDateTime.MaxSmallDateTime, msg.EM_HeldUntilDate);
				AssertEquals("PPW", cusEntryHeaderItem.DeltaGFallbackStatus);
				AssertEquals(ZDateTime.Now.Date, cusEntryHeaderItem.DeltaGFallbackIssueDate.Date);
			}
			else
			{
				cusEntryHeaderItem.DeltaGFallbackStatus = "PDS";
				sender.Send();
				AssertEquals("RGM", cusEntryHeaderItem.DeltaGFallbackStatus);
			}
		}

		[GuiTest]
		public void TestSendMessageForCheckCredit()
		{
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			FRCustomsDataRegistry.Instance.CheckAtEverySubmission.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All);

			var errorCollector = new ErrorCollector();
			var message = CreateMessageObject(false, true, "", 50);

			var sender = new DeltaGMessageSender(message, errorCollector)
			{
				MessageDecorator = x => x.EM_MessageOwner = "The IMP"
			};

			var declaration = cusEntryHeaderItem.Declaration;
			declaration.JE_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var result = sender.Send();
			AssertEquals(0, cusEntryHeaderItem.Messages.Count);

			errorCollector.WipeErrors();
			message.MessageType = FR.Business.EntryActionCodeList.Codes.VAL;
			result = sender.Send();
			AssertEquals(0, cusEntryHeaderItem.Messages.Count);

			errorCollector.WipeErrors();
			message.MessageType = FR.Business.EntryActionCodeList.Codes.REC;
			result = sender.Send();
			AssertEquals(0, cusEntryHeaderItem.Messages.Count);

			errorCollector.WipeErrors();
			message.MessageType = FR.Business.EntryActionCodeList.Codes.INV;
			result = sender.Send();
			AssertEquals(1, cusEntryHeaderItem.Messages.Count);
		}

		public void TestSendMessageAndEntrySubStyleManagement()
		{
			TestSendMessage(false, "<procedure2>D</procedure2>");
		}

		[TestDate(2019, 12, 31)]
		public void TestSendMessageWithAmountAndTypeToBeGuaranteed()
		{
			messageType = FR.Business.EntryActionCodeList.Codes.VAL;
			TestSendMessage(true, expectedPermitMessageText, true);
			AssertNotNull("Declaration Customs guarantee should not be null.", cusGuaranteeHeaderItem);
			var pendingTransactions = cusGuaranteeHeaderItem.CusGuaranteeLineTransactions.Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending).ToArray();
			AssertEquals(1, pendingTransactions.Length);
			AssertEquals((ZDecimal)(-40), pendingTransactions[0].CPL_TranValue);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, cusEntryHeaderItem?.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query)
				.Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending);
			AssertEquals(2, transactions.Count());
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { -300m, -40m }, transactions.Select(x => x.CPL_TranValue));
		}

		[TestDate(2019, 12, 31)]
		public void TestSendMessage_With_AmountAndTypeToBeGuaranteed_And_Without_Ai2Permit()
		{
			messageType = FR.Business.EntryActionCodeList.Codes.VAL;
			TestSendMessage(true, expectedPermitMessageText, true, false);
			AssertNotNull("Declaration Customs guarantee should not be null.", cusGuaranteeHeaderItem);
			var pendingTransactions = cusGuaranteeHeaderItem.CusGuaranteeLineTransactions.Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending).ToArray();
			AssertEquals(1, pendingTransactions.Length);
			AssertEquals((ZDecimal)(-40), pendingTransactions[0].CPL_TranValue);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, cusEntryHeaderItem?.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(1, transactions.Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending).Count());
			AssertEquals((ZDecimal)(-40), transactions[0].CPL_TranValue);
		}

		[TestDate(2019, 12, 31)]
		public void TestSendMessage_Without_AmountAndTypeToBeGuaranteed_And_Without_Ai2Permit()
		{
			messageType = FR.Business.EntryActionCodeList.Codes.VAL;
			TestSendMessage(true, expectedPermitMessageText, false, false);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, cusEntryHeaderItem?.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals(0, transactions.Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending).Count());
		}

		[TestDate(2021, 12, 31)]
		public void TestAddPermitIfApplicable_INV_Message()
		{
			messageType = FR.Business.EntryActionCodeList.Codes.VAL;
			ZString messageText = "<EnveloppeMessage><schemaID>MessageCDecImp</schemaID><schemaVersion>01032013</schemaVersion><partyId>A4F34833</partyId><transactionId>0000000001</transactionId><numseq>888</numseq></EnveloppeMessage><Declaration><DatasDec><Entete><codact>2</codact><refdos>1FR40159700500064-B00001000</refdos></Entete>";
			TestSendMessage(true, messageText, true, false);
			AssertNotNull("Declaration Customs guarantee should not be null.", cusGuaranteeHeaderItem);
			AssertEquals(1, cusGuaranteeHeaderItem.CusGuaranteeLineTransactions.Count(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending));
			AssertEquals(2000m, cusGuaranteeHeaderItem.CusGuaranteeLineTransactions[0].CPL_TranValue);

			var message = Factory.New<DeltaCImportFREDIMessage>();
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.IMC;
			message.EM_MessageSubType = MessageSubTypeList.Codes.IMC;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaCImportBAEResponseMessage.xml");
			message.EM_Status = EDIMessage.Status.Queued;
			var processor = new ImportDeltaCResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			AssertEquals(EntryStatusDescriptionCodeList.Codes.ES100, cusEntryHeaderItem.CH_EntryStatus);

			var sender = new DeltaGMessengerForTest(new DeltaGJobDeclarationMessageSendingObject(cusEntryHeaderItem) { MessageType = FR.Business.EntryActionCodeList.Codes.INV }, new ErrorCollector())
			{
				MessageDecorator = x => x.EM_MessageOwner = "The IMP"
			};

			var result = sender.Send();
			AssertNotNull("Declaration Customs guarantee should not be null.", cusGuaranteeHeaderItem);
			var pendingTransactions = cusGuaranteeHeaderItem.CusGuaranteeLineTransactions.Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending);
			AssertEquals(1, pendingTransactions.Count());
			AssertEquals(40m, pendingTransactions.FirstOrDefault().CPL_TranValue);
		}

		[TestDate(2021, 12, 31)]
		public void TestAddPermitIfApplicable_VAL_Message()
		{
			messageType = FR.Business.EntryActionCodeList.Codes.VAL;
			ZString messageText = "<EnveloppeMessage><schemaID>MessageCDecImp</schemaID><schemaVersion>01032013</schemaVersion><partyId>A4F34833</partyId><transactionId>0000000001</transactionId><numseq>888</numseq></EnveloppeMessage><Declaration><DatasDec><Entete><codact>2</codact><refdos>1FR40159700500064-B00001000</refdos></Entete>";
			TestSendMessage(true, messageText, true, false);
			AssertNotNull("Declaration Customs guarantee should not be null.", cusGuaranteeHeaderItem);
			AssertEquals(1, cusGuaranteeHeaderItem.CusGuaranteeLineTransactions.Count(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending));
			AssertEquals((ZDecimal)(2000), cusGuaranteeHeaderItem.CusGuaranteeLineTransactions[0].CPL_TranValue);
		}

		[TestDate(2021, 12, 31)]
		public void TestAddPermitIfApplicable_VAA_Message()
		{
			messageType = FR.Business.EntryActionCodeList.Codes.VAL;
			ZString messageText = "<EnveloppeMessage><schemaID>MessageCDecImp</schemaID><schemaVersion>01032013</schemaVersion><partyId>A4F34833</partyId><transactionId>0000000001</transactionId><numseq>888</numseq></EnveloppeMessage><Declaration><DatasDec><Entete><codact>2</codact><refdos>1FR40159700500064-B00001000</refdos></Entete>";
			TestSendMessage(true, messageText, true, false);
			AssertNotNull("Declaration Customs guarantee should not be null.", cusGuaranteeHeaderItem);
			AssertEquals(1, cusGuaranteeHeaderItem.CusGuaranteeLineTransactions.Count(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending));
			AssertEquals((ZDecimal)(2000), cusGuaranteeHeaderItem.CusGuaranteeLineTransactions[0].CPL_TranValue);
		}

		[TestDate(2021, 12, 31)]
		public void TestAddPermitIfApplicable_Other_Message()
		{
			messageType = FR.Business.EntryActionCodeList.Codes.ANT;
			ZString messageText = "<EnveloppeMessage><schemaID>MessageCDecImp</schemaID><schemaVersion>01032013</schemaVersion><partyId>A4F34833</partyId><transactionId>0000000001</transactionId><numseq>888</numseq></EnveloppeMessage><Declaration><DatasDec><Entete><codact>1</codact><refdos>1FR40159700500064-B00001000</refdos></Entete>";
			TestSendMessage(true, messageText, true, false);
			AssertNotNull("Declaration Customs guarantee should not be null.", cusGuaranteeHeaderItem);
			AssertEquals(0, cusGuaranteeHeaderItem.CusGuaranteeLineTransactions.Count(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending));
		}

		public void TestSendMessageWithShouldNotGenerateDv1Document_VAL_Message()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateTaxOrFee("DV1", 0, "FR", ZDateTime.Today, ZDateTime.MaxSmallDateTime, "D.V.1 Value", true, 10000);
			Factory.Save();

			messageType = FR.Business.EntryActionCodeList.Codes.VAL;
			TestSendMessage(true, "MessageCDecImp", false, false, "01", 10005);
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, cusEntryHeaderItem.PK));
			AssertEquals("There should not be any print job in the queue.", 0, printJobs.Length);
		}

		public void TestSendMessageWithShouldNotGenerateDv1Document_EAV_Message()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateTaxOrFee("DV1", 0, "FR", ZDateTime.Today, ZDateTime.MaxSmallDateTime, "D.V.1 Value", true, 10000);
			Factory.Save();
			messageType = FR.Business.EntryActionCodeList.Codes.EAV;
			TestSendMessage(true, "MessageCDecImp", false, false, "01", 10005);
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, cusEntryHeaderItem.PK));
			AssertEquals("There should not be any print job in the queue.", 0, printJobs.Length);
		}

		public void TestSendMessageWithShouldNotGenerateDv1DocumentBasedOnLinePrice()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			referenceDataHelper.CreateTaxOrFee("DV1", 0, "FR", ZDateTime.Today, ZDateTime.MaxSmallDateTime, "D.V.1 Value", true, 10000);
			Factory.Save();

			messageType = FR.Business.EntryActionCodeList.Codes.ANT;
			TestSendMessage(true, "MessageCDecImp", false, false, "01", 9000);
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, cusEntryHeaderItem.PK));
			AssertEquals("There should not be any print job in the queue.", 0, printJobs.Length);
		}

		public void TestSendMessageRECCreateATransaction()
		{
			messageType = FR.Business.EntryActionCodeList.Codes.REC;
			TestSendMessage(true, "MessageCDecImp", true);
			AssertNotNull("Declaration Customs guarantee should not be null.", cusGuaranteeHeaderItem);
			var pendingTransactions = cusGuaranteeHeaderItem.CusGuaranteeLineTransactions.Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending).ToArray();
			AssertEquals(1, pendingTransactions.Length);
			AssertEquals((ZDecimal)(-40), pendingTransactions[0].CPL_TranValue);

			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, Core.Constants.CountryCodes.France);

			var query = new ZDBOnlyQuery(typeof(BaseCusPermitLineTransaction));
			query.AddToFilter(CusPermitLineTransactionSchema.CPL_Reference, cusEntryHeaderItem?.CH_BGMReference);
			query.AddSubQuery(permitQuery, JoinCondition.And);

			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query)
				.Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Pending);
			AssertEquals(1, transactions.Count());
			AssertContainsExactElementsInAnyOrder(new ZDecimal[] { -40m }, transactions.Select(x => x.CPL_TranValue));
		}

		DeltaGJobDeclarationMessageSendingObject CreateMessageObject(bool amountAndTypeGenerated, bool setupGuaranteesAi2, string entryStyle, decimal lineprice)
		{
			var today = ZDate.Today;
			SetupTaxOrFees(today);

			GenerateProcedure("51", "00", "000", YesNoList.Codes.Yes, YesNoList.Codes.Yes, "Procedure description", "IMP");
			GenerateProcedure("71", "00", "000", YesNoList.Codes.Yes, YesNoList.Codes.Yes, "Procedure description", "IMP");

			D48FactoryInit();
			var declaration = CreateDeclaration(true, entryStyle, lineprice);
			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			declaration.ZG_VATDeferNumber = "FR123456";

			if (declaration.SupportingDocuments.Count > 0)
			{
				declaration.SupportingDocuments[0].CSI_DateOfIssue = ZDateTime.Today;
			}

			cusEntryHeaderItem = declaration.CustomsEntryHeaders[0];

			cusEntryHeaderItem.CH_CEI_Instruction = declaration.CustomsEntryInstructions[0].PK;

			CreateFees(declaration, today);

			var messageObject = new DeltaGJobDeclarationMessageSendingObject(cusEntryHeaderItem);
			messageObject.MessageType = messageType;
			messageObject.SequenceNumber = 888;
			if (setupGuaranteesAi2)
			{
				SetupAi2Guarantee(declaration.Declarant.Header, declaration.DeclarantAddress.OA_Address1, "FR123456", true);
			}

			if (amountAndTypeGenerated)
			{
				GenerateGuaranteeAndD48Documents(declaration);
			}

			return messageObject;
		}

		public JobDeclaration CreateDeclaration(bool isImport, string entryStyle = "", decimal linePrice = 50)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MasterBill = "UnitTest";
			declaration.ZG_VATDeferType = "L";

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			if (declarantAddress == null)
			{
				declarantAddress = Factory.NewWithValidTestData<OrgAddress>();
			}

			if (orgHeader == null)
			{
				orgHeader = Factory.NewWithValidTestData<OrgHeader>();

				orgHeader.OH_Code = OrgCusCode.FranceCodeTypes.Siret;

				declarantAddress.OA_OH = orgHeader.PK;
				declarantAddress.OA_Address1 = "Eugene Leroy Street ";

				declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

				#region Organisation Registration number : SRT

				var orgCusCodeSrt = Factory.NewWithValidTestData<OrgCusCode>();
				orgCusCodeSrt.OK_OA_PremisesAddress = declarantAddress.PK;
				orgCusCodeSrt.OK_CodeType = OrgCusCode.FranceCodeTypes.Siret;
				orgCusCodeSrt.OK_CustomsRegNo = "FR33159700500064";
				orgCusCodeSrt.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
				orgCusCodeSrt.OK_OH = declarantAddress.OA_OH;

				var orgCusCodeSrt2 = Factory.NewWithValidTestData<OrgCusCode>();
				orgCusCodeSrt2.OK_OA_PremisesAddress = declarantAddress.PK;
				orgCusCodeSrt2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				orgCusCodeSrt2.OK_CustomsRegNo = "FR40159700500064";
				orgCusCodeSrt2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
				orgCusCodeSrt2.OK_OH = declarantAddress.OA_OH;

				orgHeader.CustomsCodes.Add(orgCusCodeSrt);
				orgHeader.CustomsCodes.Add(orgCusCodeSrt2);

				#endregion

				#region Organisation Registration number : CBR

				var orgCusCodeCbr = Factory.NewWithValidTestData<OrgCusCode>();
				orgCusCodeCbr.OK_OA_PremisesAddress = declarantAddress.PK;
				orgCusCodeCbr.OK_CodeType = OrgCusCode.CodeTypes.BrokerageRegistration;
				orgCusCodeCbr.OK_CustomsRegNo = "FR33159700500064";
				orgCusCodeCbr.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
				orgCusCodeCbr.OK_OH = declarantAddress.OA_OH;

				orgHeader.CustomsCodes.Add(orgCusCodeCbr);

				#endregion

				#region Organisation Registration number : TVA

				var orgCusCodeTva = Factory.NewWithValidTestData<OrgCusCode>();
				orgCusCodeTva.OK_OA_PremisesAddress = declarantAddress.PK;
				orgCusCodeTva.OK_CodeType = OrgCusCode.FranceCodeTypes.TVA;
				orgCusCodeTva.OK_CustomsRegNo = "FR33159700500060";
				orgCusCodeTva.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
				orgCusCodeTva.OK_OH = declarantAddress.OA_OH;

				orgHeader.CustomsCodes.Add(orgCusCodeTva);

				#endregion
			}

			var customOffice = declaration.CustomsOffices.AddNew();
			customOffice.CY_Code = "ENT";
			customOffice.CY_Type = "EUO";
			customOffice.CY_Data = "FR000130";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = isImport ? "FRPAR" : "AUSYD";
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "A4F34833");

			declaration.JE_OH_Importer = importer.PK;
			declaration.Importer.MainAddress.OA_PostCode = "24130";

			var frOrgImpAddInfo = FROrgImpAddInfo.Get(importer);
			frOrgImpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;

			var cei = declaration.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = entryStyle;
			cei.CEI_SubStyle = EntrySubstyleCodePairList.Codes.A;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_Tariff = "2203001010";
			invoiceLine.JI_Description = "Unit Test";
			invoiceLine.JI_LinePrice = linePrice;
			invoiceLine.JI_CEI = cei.PK;
			invoiceLine.JI_Procedure = "5100000";
			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_NetWeight = 10;
			invoiceLine.JI_ValuationCode = "A";
			invoiceLine.JI_CustomsUnitQty = Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.FillWithValidTestData();

			if (declaration.SupportingDocuments.Count > 0)
			{
				declaration.SupportingDocuments[0].CSI_DateOfIssue = ZDateTime.Today;
			}

			Factory.Save();

			var org = declaration.Importer.MainAddress.Header.CustomsCodes.AddNew();
			org.OK_CodeType = OrgCusCode.FranceCodeTypes.TVA;
			org.OK_CustomsRegNo = "FR123456";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = isImport ? "AUSYD" : "FRPAR";
			declaration.JE_OH_Supplier = supplier.PK;
			if (!isImport)
			{
				if (testExporter == null)
				{
					testExporter = Factory.NewWithValidTestData<OrgHeader>();
					testExporter.OH_FullName = "Paris Test Exporter DHL Ltd.";
					testExporter.MainAddress.OA_Address1 = "CDG Airport";
					testExporter.MainAddress.OA_Address2 = "Location 1.2";
					testExporter.MainAddress.OA_PostCode = "75000";
					testExporter.MainAddress.OA_OH = orgHeader.PK;
					orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G1, "DGE001", ZString.Empty, ZString.Empty, "497F7750");
					orgHeader.SetupAccount(OrgCusAccountCodeList.Codes.DGE, OrgCusAccountDeltaGTypeList.Codes.G2, "DGE002", ZString.Empty, ZString.Empty, "497F7750");
					var frOrgExpAddInfo = FROrgImpAddInfo.Get(testExporter);
					frOrgExpAddInfo.ZO_DeltaG1SubProcedure = DeltaG1SubProcedureList.Codes.C;
					Factory.Save();
				}

				orgHeader.OH_IsWarehouseClient = true;
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				declaration.JE_OH_Exporter = testExporter.PK;
				declaration.JE_OH_Supplier = testExporter.PK;
				declaration.Declarant.OA_OH = orgHeader.PK;
			}

			declaration.Declarant.OA_OH = orgHeader.PK;
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			Factory.Save();

			var shutUp = new SendsMessagesToCustomsShutterUpperer();
			var mergeResult = declaration.DoMerge(shutUp);
			Assert("Merge failed", mergeResult);
			Factory.Save();

			return declaration;
		}

		void D48FactoryInit()
		{
			var helper1 = new UniversalReferenceTestDataHelper(Factory);
			helper1.CreateCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection,
				"Supporting Document of Import Direction");
			helper1.CreateCusCodeListWithAttribute("FR",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001",
				"statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6),
				UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0003",
				"statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6),
				UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044",
				"Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1),
				new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation,
				YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("US", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM,
				"GHI", "Anser fabalis/Bean goose", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6),
				UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper1.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUIATA,
				"DEF", "DDezful", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6),
				UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			var helper2 = new UniversalReferenceTestDataHelper(Factory);
			helper2.CreateCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection,
				"Supporting Document of Export Direction");
			helper2.CreateCusCodeListWithAttribute("FR",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003",
				"attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1),
				new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48,
				YesNoList.Codes.Yes);
			helper2.CreateCusCodeListWithAttribute("FR",
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045",
				"Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1),
				new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation,
				YesNoList.Codes.Yes);
			Factory.Save();
		}

		void GenerateGuaranteeAndD48Documents(JobDeclaration declaration)
		{
			var importer = declaration.Importer;
			importer.OH_Code = "TEST001";
			importer.MainAddress.Address1 = "ImporterAddress";
			importer.MainAddress.OA_Code = "ImporterAddress";
			importer.OH_RL_NKClosestPort = "FRPAR";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SUPPLIER";
			supplier.OH_FullName = "The supplier";
			supplier.MainAddress.Address1 = "SupplierAddress";
			supplier.MainAddress.OA_Code = "SupplierAddress";
			supplier.OH_RL_NKClosestPort = "AUSYD";

			cusGuaranteeHeaderItem = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.COD, "IGUA", importer.PK, EU.Business.PermitRuleCodeList.Codes.ADD, "ImporterAddress", OrgCusAccountDeltaGTypeList.Codes.G1, "IMPO", Core.Constants.CountryCodes.France);

			var transaction = cusGuaranteeHeaderItem.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "Entry Number";
			transaction.CPL_TranValue = 2000m;
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction.CPL_AppId = "Entry Reference";
			transaction.CPL_Comment = "Instruction Desc.";
			transaction.CPL_Procedure = "AAA";
			Factory.Save();

			declaration.JE_MessageType = "IMP";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			AssertEquals("Guarantee has been correctly setup.", cusGuaranteeHeaderItem, declaration.CustomsGuarantee);

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 10m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_CustomsQuantity = 10m;
			var suppDoc3 = invoiceLine1.SupportingDocuments.AddNew();
			suppDoc3.CSI_Code = "0001";
			suppDoc3.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc3.CSI_Status = "AN";
			suppDoc3.CSI_Quantity3 = 30;
			suppDoc3.CSI_Value = 20;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 10m;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_CustomsQuantity = 10m;
			var suppDoc4 = invoiceLine2.SupportingDocuments.AddNew();
			suppDoc4.CSI_Code = "DEF";
			suppDoc4.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc4.CSI_Status = "AN";
			suppDoc4.CSI_Quantity3 = 0;
			suppDoc4.CSI_Value = 10;
			var suppDoc5 = invoiceLine2.SupportingDocuments.AddNew();
			suppDoc5.CSI_Code = "GHI";
			suppDoc5.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc5.CSI_Status = "AN";
			suppDoc5.CSI_Quantity3 = 0;
			suppDoc5.CSI_Value = 30;

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Weight = 10m;
			invoiceLine3.JI_WeightUQ = "KG";
			invoiceLine3.JI_CustomsQuantity = 10m;
			var suppDoc6 = invoiceLine3.SupportingDocuments.AddNew();
			suppDoc6.CSI_Code = "0003";
			suppDoc6.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc6.CSI_Status = "AN";
			suppDoc6.CSI_Quantity3 = 5;
			suppDoc6.CSI_Value = 10;

			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_Weight = 10m;
			invoiceLine4.JI_WeightUQ = "KG";
			invoiceLine4.JI_CustomsQuantity = 10m;
			var suppDoc7 = invoiceLine4.SupportingDocuments.AddNew();
			suppDoc7.CSI_Code = "0003";
			suppDoc7.CSI_DateOfIssue = ZDateTime.Today;
			suppDoc7.CSI_Status = "AN";
			suppDoc7.CSI_Quantity3 = 5;
			suppDoc7.CSI_Value = 10;

			// After merge CSI_Code = "0001" so IsD48 = true and CSI_Value = 10
			var entryLine = cusEntryHeaderItem.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_Procedure = "5100000";

			// After merge CSI_Code = "DEF" | "GHI" so IsD48 = false and CSI_Value = 40 so value will not be counted
			var entryLine2 = cusEntryHeaderItem.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "5100000";

			// After merge CSI_Code = "0003" so IsD48 = true and CSI_Value = 10
			var entryLine3 = cusEntryHeaderItem.MergedLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Procedure = "5100000";

			// After merge CSI_Code = "0003" so IsD48 = true and CSI_Value = 10, however CSI_Status = 'Y' so therefore IsCompletee = false so value will not be counted
			var entryLine4 = cusEntryHeaderItem.MergedLines.AddNew();
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_Procedure = "5100000";

			if (declaration.SupportingDocuments.Count > 0)
			{
				declaration.SupportingDocuments[0].CSI_DateOfIssue = ZDateTime.Today;
			}

			AssertEquals(2, cusEntryHeaderItem.AmountAndTypeToBeGuaranteeds.Count());
			AssertEquals(0m, cusEntryHeaderItem.AmountAndTypeToBeGuaranteeds.ToArray()[0].AmountInDeclarationCurrency);
			AssertEquals(40m, cusEntryHeaderItem.AmountAndTypeToBeGuaranteeds.ToArray()[1].AmountInDeclarationCurrency);
		}

		public CusGuaranteeHeader SetupAi2Guarantee(OrgHeader org, string address, string permitNumber, bool useEndDate = true)
		{
			var guaranteeHeader = GuaranteeTestHelper.CreateGuaranteeHeader(Factory, GuaranteeTypeList.Codes.AI2, permitNumber, org.PK, EU.Business.PermitRuleCodeList.Codes.ADD, address, OrgCusAccountDeltaGTypeList.Codes.G1, "DECL", Core.Constants.CountryCodes.France);
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			guaranteeHeader.CPH_EndDate = useEndDate ? ZDate.Today.AddYears(20) : ZDate.Empty;
			Factory.Save();

			var openingBalanceTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			openingBalanceTransaction.CPL_TranValue = 10000;
			openingBalanceTransaction.CPL_Reference = "Opening Balance";
			openingBalanceTransaction.CPL_TransactionDate = ZDateTime.Today;
			openingBalanceTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			openingBalanceTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.VAL;
			openingBalanceTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = "Entry Number";
			transaction.CPL_TranValue = 2000m;
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.CUS;
			transaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			transaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			transaction.CPL_AppId = "Entry Reference";
			transaction.CPL_Comment = "Instruction Desc.";
			transaction.CPL_Procedure = "AAA";
			Factory.Save();

			return guaranteeHeader;
		}

		void SetupTaxOrFees(ZDate date)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("ZZ1", 10.0m, "FR", 0m, 0m, GuaranteeTypeList.Codes.AI2, date.AddDays(-1), date.AddDays(1), "Test AI2");
			helper.CreateTaxOrFee("ZZ2", 20.0m, "FR", 0m, 0m, Core.Constants.Customs.CusEntryFeeTypes.VAT, date.AddDays(-1), date.AddDays(1), "Test VAT");
			helper.CreateTaxOrFee("ZZ3", 30.0m, "FR", 0m, 0m, Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, date.AddDays(-1), date.AddDays(1), "Test DTY");
			Factory.Save();
		}

		void CreateFees(JobDeclaration declaration, ZDate today)
		{
			var invoiceHeader = declaration.Invoices[0];
			invoiceHeader.JZ_ValuationDateOverride = today;
			var cusEntryLine = cusEntryHeaderItem.MergedLines[0];

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.NationalFeeTypeCode = "ZZ1";
			fee1.CF_ChargeAmount = 100m;

			var fee2 = cusEntryLine.Fees.AddNew();
			fee2.NationalFeeTypeCode = "ZZ2";
			fee2.CF_ChargeAmount = 200m;

			var fee3 = cusEntryLine.Fees.AddNew();
			fee3.NationalFeeTypeCode = "ZZ3";
			fee3.CF_ChargeAmount = 400m;

			var fee4 = cusEntryLine.Fees.AddNew();
			fee4.NationalFeeTypeCode = "";
			fee4.CF_ChargeAmount = 1m;
		}

		RefCusProcedure GenerateProcedure(string procedureCode, string procedurePreviousCode, string concessionCode, string isGuaranteeConsumed, string isGuaranteeReleased, string description, string messagetype)
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = procedureCode;
			procedure.ZZ6_PreviousProcedureCode = procedurePreviousCode;
			procedure.ZZ6_Concession = concessionCode;
			procedure.ZZ6_ShipmentType = messagetype;
			procedure.ZZ6_IsGuaranteeConsumed = isGuaranteeConsumed;
			procedure.ZZ6_IsGuaranteeReleased = isGuaranteeReleased;
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.France;
			procedure.ZZ6_Description = description;
			return procedure;
		}

		CusEntryHeader cusEntryHeaderItem;
		CusGuaranteeHeader cusGuaranteeHeaderItem;
		OrgHeader orgHeader;
		OrgHeader testExporter;
		OrgAddress declarantAddress;

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string messageType = FR.Business.EntryActionCodeList.Codes.ANT;
		const string expectedMessageText = "<EnveloppeMessage><schemaID>MessageCDecImp</schemaID><schemaVersion>01032013</schemaVersion><partyId>A4F34833</partyId><transactionId>0000000001</transactionId><numseq>888</numseq></EnveloppeMessage><Declaration><DatasDec><Entete><codact>1</codact><refdos>9FR40159700500064-B00001000</refdos>";
		const string expectedPermitMessageText = "<EnveloppeMessage><schemaID>MessageCDecImp</schemaID><schemaVersion>01032013</schemaVersion><partyId>A4F34833</partyId><transactionId>0000000001</transactionId><numseq>888</numseq></EnveloppeMessage><Declaration><DatasDec><Entete><codact>2</codact><refdos>9FR40159700500064-B00001000</refdos>";
	}

	public class DeltaGMessengerForTest : DeltaGMessageSender
	{
		public DeltaGMessengerForTest(DeltaGJobDeclarationMessageSendingObject decWrapper, ErrorCollector errorCollector) : base(decWrapper, errorCollector)
		{
		}
		public new bool ShouldIncreaseSequenceNumber => base.ShouldIncreaseSequenceNumber;
	}
}
