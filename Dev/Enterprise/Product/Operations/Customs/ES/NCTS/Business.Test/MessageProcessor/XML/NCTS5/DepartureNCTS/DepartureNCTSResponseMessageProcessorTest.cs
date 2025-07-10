using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC015C_v515.CC015CV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.Constants;
using TS = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

class DepartureNCTSResponseMessageProcessorTest : NCTS5CommonResponseMessageProcessorTest<DepartureNCTSResponseMessageProcessor, DepartureNCTSMessagePrettyFormatter, Cc015Cv1Sal>
{
	public void TestProcessAcceptedMessage_PDA()
	{
		ProcessAndAssertAcceptedNoCircuitDeclaration();
	}

	public void TestProcessAcceptedMessage_DRL_GreenCircuit()
	{
		ProcessAndAssertAcceptedGreenCircuitDeclaration();
	}

	public void TestProcessAcceptedMessage_DCC_OrangeCircuit()
	{
		ProcessAndAssertAcceptedOrangeCircuitDeclaration();
	}

	public void TestProcessAcceptedMessage_DGP_RedCircuit()
	{
		ProcessAndAssertAcceptedRedCircuitDeclaration();
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
	{
		ProcessAndAssertAcceptedGreenCircuitDeclaration();

		CombineAssertions(() =>
		{
			nctsHeader.Messages.Reload(true);
			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 1, docMessages.Length);
			AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (mrnEntryNumber + "_NCTS_AEAT_DAT.pdf", ClearanceReferenceNumber) });
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
	{
		var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnEntryNumber + "_NCTS_AEAT_DAT.pdf", "CAU");
		docManagerInfo.Save();

		ProcessAndAssertAcceptedGreenCircuitDeclaration();

		CombineAssertions(() =>
		{
			var eDocs = docManagerInfo.GetRelatedEDocs();
			AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

			var eDocNames = new List<ZString>() { mrnEntryNumber + "_NCTS_AEAT_DAT.pdf" };
			AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

			nctsHeader.Messages.Reload(true);
			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenDepartureAndNoCSVClearance_AllDocs()
	{
		ProcessAndAssertAcceptedRedCircuitDeclaration();

		CombineAssertions(() =>
		{
			var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
			var eDocs = docManagerInfo.GetRelatedEDocs();
			AssertEquals("Number of eDocs is correct", 0, eDocs.Count());

			nctsHeader.Messages.Reload(true);
			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		});
	}

	public void TestProcessMessageAcceptedNoCircuit_PendingTransactionsAsConfirmed()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		ProcessAndAssertAcceptedNoCircuitDeclaration();

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has been changed since it was pending", mrnEntryNumber, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has been changed since it was pending", mrnEntryNumber, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessMessageAcceptedGreenCircuit_PendingTransactionsAsConfirmed()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		ProcessAndAssertAcceptedGreenCircuitDeclaration();

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has been changed since it was pending", mrnEntryNumber, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has been changed since it was pending", mrnEntryNumber, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessMessageAcceptedOrangeCircuit_PendingTransactionsAsConfirmed()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		ProcessAndAssertAcceptedOrangeCircuitDeclaration();

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has been changed since it was pending", mrnEntryNumber, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has been changed since it was pending", mrnEntryNumber, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessMessageAcceptedRedCircuit_PendingTransactionsAsConfirmed()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		ProcessAndAssertAcceptedRedCircuitDeclaration();

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has been changed since it was pending", mrnEntryNumber, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has been changed since it was pending", mrnEntryNumber, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessMessageRejectedDeclaration_PendingTransactionsAsDeleted()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		ProcessAndAssertRejectedDeclaration();

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now deleted", PermitTransactionStatusList.Codes.Deleted, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has not been changed since it was set to DEL", ApplicationReference, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now deleted", PermitTransactionStatusList.Codes.Deleted, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has not been changed since it was set to DEL", ApplicationReference, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessMessageErrorDeclaration_PendingTransactionsAsDeleted()
	{
		var guaranteeHeader = SetUpGuaranteeTransactions();

		ProcessAndAssertErrorDeclaration();

		CombineAssertions(() =>
		{
			var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
			AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
			AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

			var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
			AssertEquals("Second transaction is now deleted", PermitTransactionStatusList.Codes.Deleted, secondTransaction.CPL_TransactionStatus);
			AssertEquals("Second transaction's reference has not been changed since it was set to DEL", ApplicationReference, secondTransaction.CPL_Reference);

			var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
			AssertEquals("Third transaction is now deleted", PermitTransactionStatusList.Codes.Deleted, thirdTransaction.CPL_TransactionStatus);
			AssertEquals("Third transaction's reference has not been changed since it was set to DEL", ApplicationReference, thirdTransaction.CPL_Reference);
		});
	}

	public void TestProcessMessageAcceptedDeclaration_NoResetDocumentsLineNo_TransitionPeriod()
	{
		ProcessAndAssertResponse_ResetDocumentsLineNo(ProcessAndAssertAcceptedGreenCircuitDeclaration, transitionPeriod: true, shouldResetLineNo: false);
	}

	public void TestProcessMessageAcceptedDeclaration_NoResetDocumentsLineNo_NoTransitionPeriod()
	{
		ProcessAndAssertResponse_ResetDocumentsLineNo(ProcessAndAssertAcceptedGreenCircuitDeclaration, transitionPeriod: false, shouldResetLineNo: false);
	}

	public void TestProcessMessageRejectedDeclaration_ResetDocumentsLineNo_TransitionPeriod()
	{
		ProcessAndAssertResponse_ResetDocumentsLineNo(ProcessAndAssertRejectedDeclaration, transitionPeriod: true, shouldResetLineNo: true);
	}

	public void TestProcessMessageRejectedDeclaration_ResetDocumentsLineNo_NoTransitionPeriod()
	{
		ProcessAndAssertResponse_ResetDocumentsLineNo(ProcessAndAssertRejectedDeclaration, transitionPeriod: false, shouldResetLineNo: false);
	}

	public void TestProcessMessageErrorDeclaration_ResetDocumentsLineNo_TransitionPeriod()
	{
		ProcessAndAssertResponse_ResetDocumentsLineNo(ProcessAndAssertErrorDeclaration, transitionPeriod: true, shouldResetLineNo: true);
	}

	public void TestProcessMessageErrorDeclaration_ResetDocumentsLineNo_NoTransitionPeriod()
	{
		ProcessAndAssertResponse_ResetDocumentsLineNo(ProcessAndAssertErrorDeclaration, transitionPeriod: false, shouldResetLineNo: false);
	}

	public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageRegisterNotEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForDPTAndDPN();

			nctsHeader.MovementHeader.BM_CustomsStatus = ZString.Empty;

			AddMessageProcessAndAssertResult_RejectedMessage(true);

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was not changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefNum)", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestProcessRejectedMessage_EntryStatusEmpty_TemporaryStorageEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForDPTAndDPN();

			nctsHeader.MovementHeader.BM_CustomsStatus = ZString.Empty;

			AddMessageProcessAndAssertResult_RejectedMessage(true);

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefNum)", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestProcessRejectedMessage_EntryStatusNotEmpty_TemporaryStorageEnabled()
	{
		var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
		using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			var (regLineTransaction1, regLineTransaction2, regLineTransaction3) = SetUpTransactionsForRejectedTestForDPTAndDPN();

			AddMessageProcessAndAssertResult_RejectedMessage(false);

			CombineAssertions(() =>
			{
				AssertEquals("regLineTransaction1's SRT_TransactionType was changed", TS.CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction1.SRT_TransactionStatus);
				AssertEquals("regLineTransaction2's SRT_TransactionType was not changed (not PND)", TS.CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction2.SRT_TransactionStatus);
				AssertEquals("regLineTransaction3's SRT_TransactionType was not changed (wrong internalRefNum)", TS.CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction3.SRT_TransactionStatus);
			});
		}
	}

	public void TestLoggerWriteOffTransactionError()
	{
		AssertLoggerWriteOffTransactionError();
	}

	void ProcessAndAssertResponse_ResetDocumentsLineNo(Action processAndAssertResponse, ZBool transitionPeriod, ZBool shouldResetLineNo)
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
				Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, transitionPeriod))
		{
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			var movementHeader = nctsHeader.MovementHeader;

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "INF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "REF";
			var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo3.CSI_SubType = "TRA";

			var additionalInfo4 = bill.AdditionalDocuments.AddNew();
			additionalInfo4.CSI_SubType = "INF";
			var additionalInfo5 = bill.AdditionalDocuments.AddNew();
			additionalInfo5.CSI_SubType = "REF";
			var additionalInfo6 = bill.AdditionalDocuments.AddNew();
			additionalInfo6.CSI_SubType = "TRA";

			var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo7.CSI_SubType = "INF";
			var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo8.CSI_SubType = "REF";
			var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo9.CSI_SubType = "TRA";

			additionalInfo1.CSI_LineNo = 8;
			additionalInfo2.CSI_LineNo = 5;
			additionalInfo3.CSI_LineNo = 6;
			additionalInfo4.CSI_LineNo = 4;
			additionalInfo5.CSI_LineNo = 3;
			additionalInfo6.CSI_LineNo = 7;
			additionalInfo7.CSI_LineNo = 2;
			additionalInfo8.CSI_LineNo = 1;
			additionalInfo9.CSI_LineNo = 9;

			var supportingDocument1 = movementHeader.SupportingDocuments.AddNew();
			var supportingDocument2 = movementHeader.SupportingDocuments.AddNew();
			var supportingDocument3 = bill.SupportingDocuments.AddNew();
			var supportingDocument4 = bill.SupportingDocuments.AddNew();
			var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
			var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

			supportingDocument1.CSI_LineNo = 8;
			supportingDocument2.CSI_LineNo = 5;
			supportingDocument3.CSI_LineNo = 6;
			supportingDocument4.CSI_LineNo = 4;
			supportingDocument5.CSI_LineNo = 3;
			supportingDocument6.CSI_LineNo = 7;

			CombineAssertions(() =>
			{
				AssertEquals("Prereq, additionalInfo1.CSI_LineNo is not 0 before processing", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("Prereq, additionalInfo2.CSI_LineNo is not 0 before processing", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("Prereq, additionalInfo3.CSI_LineNo is not 0 before processing", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("Prereq, additionalInfo4.CSI_LineNo is not 0 before processing", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("Prereq, additionalInfo5.CSI_LineNo is not 0 before processing", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("Prereq, additionalInfo6.CSI_LineNo is not 0 before processing", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("Prereq, additionalInfo7.CSI_LineNo is not 0 before processing", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("Prereq, additionalInfo8.CSI_LineNo is not 0 before processing", 1, additionalInfo8.CSI_LineNo);
				AssertEquals("Prereq, additionalInfo9.CSI_LineNo is not 0 before processing", 9, additionalInfo9.CSI_LineNo);

				AssertEquals("Prereq, supportingDocument1.CSI_LineNo is not 0 before processing", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("Prereq, supportingDocument2.CSI_LineNo is not 0 before processing", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("Prereq, supportingDocument3.CSI_LineNo is not 0 before processing", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("Prereq, supportingDocument4.CSI_LineNo is not 0 before processing", 4, supportingDocument4.CSI_LineNo);
				AssertEquals("Prereq, supportingDocument5.CSI_LineNo is not 0 before processing", 3, supportingDocument5.CSI_LineNo);
				AssertEquals("Prereq, supportingDocument6.CSI_LineNo is not 0 before processing", 7, supportingDocument6.CSI_LineNo);
			});

			processAndAssertResponse.Invoke();

			CombineAssertions(() =>
			{
				if (shouldResetLineNo)
				{
					AssertEquals("additionalInfo1.CSI_LineNo is 0 after processing", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is 0 after processing", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is 0 after processing", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is 0 after processing", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is 0 after processing", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is 0 after processing", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is 0 after processing", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is 0 after processing", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is 0 after processing", 0, additionalInfo9.CSI_LineNo);

					AssertEquals("supportingDocument1.CSI_LineNo is 0 after processing", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("supportingDocument2.CSI_LineNo is 0 after processing", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is 0 after processing", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument4.CSI_LineNo is 0 after processing", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is 0 after processing", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is 0 after processing", 0, supportingDocument6.CSI_LineNo);
				}
				else
				{
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after processing", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after processing", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after processing", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after processing", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after processing", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after processing", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after processing", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after processing", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after processing", 9, additionalInfo9.CSI_LineNo);

					AssertEquals("supportingDocument1.CSI_LineNo is not 0 after processing", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after processing", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after processing", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument4.CSI_LineNo is not 0 after processing", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is not 0 after processing", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after processing", 7, supportingDocument6.CSI_LineNo);
				}
			});
		}
	}

	void ProcessAndAssertAcceptedNoCircuitDeclaration()
	{
		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceResponseCodePTestFile(), InterchangeID);
		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 18:56:00</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PA - Pre-Declaration</td></tr></table>";

		AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.PreLodged, mrnEntrynum: mrnEntryNumber, mrnIssueDate: AdmissionDate);
	}

	void ProcessAndAssertAcceptedGreenCircuitDeclaration()
	{
		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceGreenCircuitResponseCodeLFile(), InterchangeID);
		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 18:56:00</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>23-03-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>A198543129CBF854</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>DE - Dispatch</td></tr></table>";

		var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

		var queryCLR = new ZQuery(CusEntryNumSchema.CE_EntryNum, ClearanceReferenceNumber);
		queryCLR.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ClearanceCSV);
		queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberClearance = message.Factory.Load<CusEntryNumber>(queryCLR).Single();

		AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberClearance: cusEntryNumberClearance, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.GREEN, mrnIssueDate: AdmissionDate, clearanceExpiryDate: limitDate, clearanceReferenceNumber: ClearanceReferenceNumber, clearanceIssueDate: ExpiryDate);
	}

	void ProcessAndAssertAcceptedOrangeCircuitDeclaration()
	{
		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceOrangeCircuitResponseCodeBTestFile(), InterchangeID);
		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 18:56:00</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>23-03-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>A198543129CBF854</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PD - Pending Dispatch</td></tr></table>";

		var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

		AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl, cusEntryNumberMRN: cusEntryNumberMRN, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.ORANGE, mrnIssueDate: AdmissionDate);
	}

	void ProcessAndAssertAcceptedRedCircuitDeclaration()
	{
		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceRedCircuitResponseCodeGTestFile(), InterchangeID);
		ProcessMessageForTest(message);
		var expectedMessageInterpretationText = "<H3>Accepted Declaration</H3>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 18:56:00</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>22ES000101500659J7</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>23-03-2022</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>A198543129CBF854</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>PG - Pending Guarantee</td></tr></table>";

		var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

		AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, commonCustomsStatus: ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance, cusEntryNumberMRN: cusEntryNumberMRN, mrnEntrynum: mrnEntryNumber, mrnEntryStatus: CircuitCodeList.Codes.RED, mrnIssueDate: AdmissionDate);
	}

	void ProcessAndAssertRejectedDeclaration()
	{
		var responseMessage = CreateNewEDIMessage(ApplicationReference, GetRejectedTestFile(), InterchangeID);

		ProcessMessageForTest(responseMessage);
		var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
		"<H4>List of Errors:</H4>" +
		"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
		"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
		"<tr><td>14</td><td>/CC014C/TransitOperation/MRN</td><td>(1403) No existe declaración para el valor del MRN indicado.</td><td>22ES000101500651J4</td></tr></table>";

		AssertNCTSDeclaration(responseMessage, messageSubType: "REJ", messageStatus: MessageStatusWhenRejectedOrError, emStatus: EMStatusWhenRejectedOrError, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, commonCustomsStatus: OriginalEntryStatus, phaseStatus: PhaseStatusWhenErrorOrRejected);
	}

	void ProcessAndAssertErrorDeclaration()
	{
		var responseMessage = CreateNewEDIMessage(ApplicationReference, GetErrorTestFile(), InterchangeID);

		ProcessMessageForTest(responseMessage);
		var expectedMessageInterpretationTextRejected = "<H3>Rejected Declaration</H3>" +
			"<H4>List of Errors:</H4>" +
			"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
			"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
			"<tr><td>9 / 34</td><td>642</td><td>Item15</td><td>1207 - Se esperaba nodo {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}TransitOperation y ha venido {https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adtr/jdit/ws/ncts5/CC014CV1Ent.xsd}Invalidation</td><td>&nbsp;</td></tr>" +
			"</table>";

		AssertNCTSDeclaration(responseMessage, messageSubType: "REJ", messageStatus: MessageStatusWhenRejectedOrError, emStatus: EMStatusWhenRejectedOrError, expectedMessageInterpretation: expectedMessageInterpretationTextRejected, commonCustomsStatus: OriginalEntryStatus, phaseStatus: PhaseStatusWhenErrorOrRejected);
	}

	protected override ZString GetExpectedProcessorFriendlyName() => "NCTS Departure Declaration Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5Departure, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration };
	string GetAcceptanceResponseCodePTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureNCTSTestFilePath, "AcceptedMessageP.txt");
	string GetAcceptanceGreenCircuitResponseCodeLFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureNCTSTestFilePath, "AcceptedMessageGreenCircuitL.txt");
	string GetAcceptanceOrangeCircuitResponseCodeBTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureNCTSTestFilePath, "AcceptedMessageOrangeCircuitB.txt");
	string GetAcceptanceRedCircuitResponseCodeGTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureNCTSTestFilePath, "AcceptedMessageRedCircuitG.txt");
	protected override string GetRejectedTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureNCTSTestFilePath, "RejectedMessage.txt");
	protected override string GetErrorTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureNCTSTestFilePath, "ErrorMessage.txt");
	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	protected override DepartureNCTSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new DepartureNCTSResponseMessageProcessor(logger);
	protected new readonly ZDateTime AdmissionDate = new ZDateTime(2020, 11, 20);
	readonly ZDateTime limitDate = new ZDateTime(2022, 3, 23);
	readonly ZString mrnEntryNumber = "22ES000101500659J7";
}
