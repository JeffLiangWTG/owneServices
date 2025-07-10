using System;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE404;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusTempStorageRegHeader = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegHeader;
using CusTempStorageRegLineTransaction = Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageRegLineTransaction;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class IE404ProcessorTest : DeltaIEBaseProcessorTest<CC404BType, IE404Processor>
	{
		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE404ResponseMessage.json");

		protected override ZString GetExpectedMessageInterpretation() => new ZString("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=\"font-size: 120%\"><strong>Customs request reference: </strong>REF123456789<br><strong>Operator request reference: </strong>OPREF123456789<br><strong>MRN: </strong>22FR1234567890123R1<br><strong>Amendment date and time: </strong>2023-10-01T12:34:56<br><strong>Amendment acceptance date and time: </strong>2023-10-01T12:34:56<br><strong>Amendment justification: </strong>This is a justification for the amendment.</p><p style=\"font-size: 120%\"><strong>Remarks</strong><br><strong>Remarks code and Reason: </strong>REMARK001This is remark 1.<br><strong>Remarks code and Reason: </strong>REMARK002This is remark 2.</p>");

		protected override ZString GetExpectedLRN() => "WTLDFRFRM0000000001";

		protected override ZString GetExpectedCRN() => ZString.Empty;

		protected override ZString GetExpectedMRN() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderMessageStatus() => MessageStatusCodeList.Codes.OK;

		protected override ZString GetMessageSubType() => DeltaIEResponseMessageSubTypeList.Codes.AmendmentConfirmation;

		protected override ZString GetMessageTextWithoutAdditionalRefs() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE404ResponseMessage.json");

		protected override ZString GetMessageTextWithoutImportOperation() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE404ResponseMessageWithoutLRN.json");

		protected override ZString GetExpectedMessageInterpretationWithoutImportOperationOrEntryHeaderLocatingRef() => new ZString(@"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><p style=""font-size: 120%""><strong>Customs request reference: </strong>REF123456789<br><strong>Operator request reference: </strong>OPREF123456789<br><strong>MRN: </strong>22FR1234567890123R1<br><strong>Amendment date and time: </strong>2023-10-01T12:34:56<br><strong>Amendment acceptance date and time: </strong>2023-10-01T12:34:56<br><strong>Amendment justification: </strong>This is a justification for the amendment.</p><p style=""font-size: 120%""><strong>Remarks</strong><br><strong>Remarks code and Reason: </strong>REMARK001This is remark 1.<br><strong>Remarks code and Reason: </strong>REMARK002This is remark 2.</p>");

		protected override ZString GetMessageTextForFees() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE404ResponseMessageWithTaxes.json");

		protected override ZInt GetExpectedFeesCountAfterProcessingMessageWithTaxes() => 3;

		protected override ZString GetEventReference() => ZString.Empty;

		protected override ZString GetExpectedEntryHeaderEntryStatus() => DeltaIEImportCusEntryStatusList.Codes.Amended;

		protected override ZString GetExpectedCESLogInfo() => "CES 2023-10-01 12:34:56";

		protected override ZString GetMessageTextForMissingMandatoryFields() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DeltaIE_IE404ResponseMessageForMissingField.json");

		public void TestUpdateTemporaryStorageIfApplicable()
		{
			var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist.SJH_JobReference = "FRJ_IST1";
			ist.DDTNumber = "DDT1";
			ist.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var istLine = ist.CusTempStorageDec.CusTempStorageLines.AddNew();
			istLine.TSL_PackageQty = 100;
			istLine.TSL_PackageType = "1A";
			istLine.TSL_GrossWeight = 1000;
			istLine.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var group = SetUpStaffAndGroup();
			Factory.Save();

			FRCustomsDataRegistry.Instance.DeltaGResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var registerHeader = Factory.New<CusTempStorageRegHeader>();
			registerHeader.SRH_Reference = ist.DDTNumber;
			registerHeader.SRH_InternalReference = ist.SJH_JobReference;

			var registerLine = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine.FillWithValidTestData();
			registerLine.SRL_LineNumber = 1;
			registerLine.SRL_PackagesRemaining = 100;

			var oblTransaction = registerLine.CusTempStorageRegLineTransactions.AddNew();
			oblTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			oblTransaction.SRT_GrossWeight = 2000m;
			oblTransaction.SRT_PackageQty = 100;
			oblTransaction.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
			oblTransaction.SRT_Reference = "FRJ_IST1";
			AssertEquals("Transaction count after adding an opening balance transaction.", 1, registerLine.CusTempStorageRegLineTransactions.Count);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "10P";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CorrelationID = "WTLDFRFRM0000000001";
			cusEntryHeader.CH_BGMReference = "5FR12345B00176178";
			cusEntryHeader.CH_SequenceNumber = 1;

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = cusEntryLine.PK;

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_ReferenceNumber = "FRJ_IST1";
			previousDocument.CSI_Code = FRConstants.PreviousDocuments.N337;
			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_PackQty = 10;
			previousDocument.CSI_Quantity = 500m;
			previousDocument.CSI_UnitOfQuantity = "KGM";

			var previousIST = cusEntryHeader.MergedLines[0].PreviousISTHeader;
			AssertSame("Find complementary IST from the previous IST job reference.", ist, previousIST);

			var logger = new LoggingInformation();
			registerHeader.AddNewRegisterTransaction(logger, previousDocument.CSI_ItemNumber, cusEntryHeader.EntryNumber, TempStorageTransactionRefTypeList.Codes.EntryHeader, cusEntryHeader.CH_BGMReference, ZString.Empty, 500m, -10, "Entry line 0");

			var reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			var reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			var createdTransaction = (CusTempStorageRegLineTransaction)reloadedRegisterLine.CusTempStorageRegLineTransactions.Last();
			AssertEquals("Transaction count after adding the manual transaction.", 2, reloadedRegisterLine.CusTempStorageRegLineTransactions.Count);

			CombineAssertions("Assert created transaction values.", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, createdTransaction.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, createdTransaction.TransactionTypeDescription);
				AssertEquals("5FR12345B00176178", createdTransaction.SRT_InternalReferenceNumber);
				AssertEquals(500m, createdTransaction.SRT_GrossWeight);
				AssertEquals(-10, createdTransaction.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.EntryHeader, createdTransaction.SRT_ReferenceType);
				AssertEquals("Entry line 0", createdTransaction.SRT_Comments);
			});

			previousDocument.CSI_PackQty = 15;
			previousDocument.CSI_Quantity = 700m;

			var processorIE404 = GetDeltaIEBaseProcessor();
			var messageIE404 = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			processorIE404.ProcessMessage(messageIE404);
			Factory.Save();

			reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			createdTransaction = (CusTempStorageRegLineTransaction)reloadedRegisterLine.CusTempStorageRegLineTransactions.Last();
			AssertEquals("Transaction count after processing the IE404 response.", 3, reloadedRegisterLine.CusTempStorageRegLineTransactions.Count);

			CombineAssertions("Assert amended transaction values.", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, createdTransaction.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, createdTransaction.TransactionTypeDescription);
				AssertEquals("5FR12345B00176178", createdTransaction.SRT_InternalReferenceNumber);
				AssertEquals(200m, createdTransaction.SRT_GrossWeight);
				AssertEquals(-5, createdTransaction.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.EntryHeader, createdTransaction.SRT_ReferenceType);
				AssertEquals("Entry line 0", createdTransaction.SRT_Comments);
			});

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("No email should have been sent.", 0, mails.Count);
		}

		public void TestUpdateTemporaryStorageIfApplicable_LogsAndEmailNotification()
		{
			var ist = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeIST);
			ist.SJH_JobReference = "FRJ_IST1";
			ist.DDTNumber = "DDT1";
			ist.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var istLine = ist.CusTempStorageDec.CusTempStorageLines.AddNew();
			istLine.TSL_PackageQty = 100;
			istLine.TSL_PackageType = "1A";
			istLine.TSL_GrossWeight = 1000;
			istLine.TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

			var group = SetUpStaffAndGroup();
			Factory.Save();

			FRCustomsDataRegistry.Instance.DeltaGResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var registerHeader = Factory.New<CusTempStorageRegHeader>();
			registerHeader.SRH_Reference = ist.DDTNumber;
			registerHeader.SRH_InternalReference = ist.SJH_JobReference;

			var registerLine = registerHeader.CusTempStorageRegLines.AddNew();
			registerLine.FillWithValidTestData();
			registerLine.SRL_LineNumber = 1;
			registerLine.SRL_PackagesRemaining = 100;

			var oblTransaction = registerLine.CusTempStorageRegLineTransactions.AddNew();
			oblTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			oblTransaction.SRT_GrossWeight = 2000m;
			oblTransaction.SRT_PackageQty = 100;
			oblTransaction.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
			oblTransaction.SRT_Reference = "FRJ_IST1";
			AssertEquals("Transaction count after adding an opening balance transaction.", 1, registerLine.CusTempStorageRegLineTransactions.Count);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "10P";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = cusEntryInstruction.PK;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CorrelationID = "WTLDFRFRM0000000001";
			cusEntryHeader.CH_BGMReference = "5FR12345B00176178";
			cusEntryHeader.CH_SequenceNumber = 1;

			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = cusEntryLine.PK;

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_ReferenceNumber = "FRJ_IST1";
			previousDocument.CSI_Code = FRConstants.PreviousDocuments.N337;
			previousDocument.CSI_ItemNumber = 1;
			previousDocument.CSI_PackQty = 10;
			previousDocument.CSI_Quantity = 500m;
			previousDocument.CSI_UnitOfQuantity = "KGM";

			var previousIST = cusEntryHeader.MergedLines[0].PreviousISTHeader;
			AssertSame("Find complementary IST from the previous IST job reference.", ist, previousIST);

			var logger = new LoggingInformation();
			registerHeader.AddNewRegisterTransaction(logger, previousDocument.CSI_ItemNumber, cusEntryHeader.EntryNumber, TempStorageTransactionRefTypeList.Codes.EntryHeader, cusEntryHeader.CH_BGMReference, ZString.Empty, 500m, -10, "Entry line 0");

			var reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			var reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			var createdTransaction = (CusTempStorageRegLineTransaction)reloadedRegisterLine.CusTempStorageRegLineTransactions.Last();
			AssertEquals("Transaction count after adding the manual transaction.", 2, reloadedRegisterLine.CusTempStorageRegLineTransactions.Count);

			CombineAssertions("Assert created transaction values.", () =>
			{
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Codes.Transaction, createdTransaction.SRT_TransactionType);
				AssertEquals(CusTempStorageRegLineTransactionTypeList.Descriptions.Transaction, createdTransaction.TransactionTypeDescription);
				AssertEquals("5FR12345B00176178", createdTransaction.SRT_InternalReferenceNumber);
				AssertEquals(500m, createdTransaction.SRT_GrossWeight);
				AssertEquals(-10, createdTransaction.SRT_PackageQty);
				AssertEquals(TempStorageTransactionRefTypeList.Codes.EntryHeader, createdTransaction.SRT_ReferenceType);
				AssertEquals("Entry line 0", createdTransaction.SRT_Comments);
			});

			previousDocument.CSI_PackQty = 200;
			previousDocument.CSI_Quantity = 700m;

			var processorIE404 = GetDeltaIEBaseProcessor();
			var messageIE404 = GetDeltaIEFREDIMessageWithMessageText(GetMessageText());
			processorIE404.ProcessMessage(messageIE404);
			Factory.Save();

			reloadedRegisterHeader = Factory.LoadTop1<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, previousIST.SJH_JobReference));
			reloadedRegisterLine = reloadedRegisterHeader.CusTempStorageRegLines.First(x => x.SRL_LineNumber == 1);
			AssertEquals("No amendment transaction should be created as previousDocument CSI_PackQty exceeds the available temporary storage capacity.", 2, reloadedRegisterLine.CusTempStorageRegLineTransactions.Count);

			Assert("CusEntryHeader logs should be captured.", cusEntryHeader.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference.Contains("Register " + reloadedRegisterHeader.SRH_Reference + " has not been updated")));

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			CombineAssertions("Assert email notification.", () =>
			{
				AssertEquals("An email should be sent.", 1, mails.Count);
				AssertEquals("Warning: New Delta I response received. Declaration: B00001000 IST Reference: DDT1", mails[0].Subject);
				AssertEquals(@"A Delta I response has been received. Temporary Storage: DDT1 has not been updated by Declaration: B00001000. There are not enough packages remaining.", mails[0].Body);
			});
		}
	}
}
