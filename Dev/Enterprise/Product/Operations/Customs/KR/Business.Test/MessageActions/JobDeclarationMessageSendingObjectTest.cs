using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObject))]
	sealed class JobDeclarationMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			return new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._929);
		}

		public void TestEntryNumber()
		{
			var declaration = GetDeclarationAndEntry(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._929);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.EntryNumber = "ABC123";
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._929);
			AssertEquals("ABC123", sendingObj.EntryNumber);
		}

		public string[] OrginalMessageStatusTypeList => new string[] { string.Empty, CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal, CustomsMessageStatusTypeList.Codes.OriginalRejected };
		public string[] AmendmentMessageStatusTypeList => new string[] { CustomsMessageStatusTypeList.Codes.OriginalAccepted, CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment, CustomsMessageStatusTypeList.Codes.AmendmentRejected, CustomsMessageStatusTypeList.Codes.AmendmentAccepted, CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation, CustomsMessageStatusTypeList.Codes.CancellationRejected, CustomsMessageStatusTypeList.Codes.CancellationAccepted, CustomsMessageStatusTypeList.Codes.CancellationDeclined };
		public string[] AmendmentMessageStatusReqReview => new string[] { CustomsMessageStatusTypeList.Codes.AmendmentAccepted, CustomsMessageStatusTypeList.Codes.CancellationAccepted };
		public string[] AmendmentMessageStatusNotReqReviewAll => new string[] { CustomsMessageStatusTypeList.Codes.AmendmentRejected, CustomsMessageStatusTypeList.Codes.AmendmentSent, CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment, CustomsMessageStatusTypeList.Codes.CancellationRejected, CustomsMessageStatusTypeList.Codes.CancellationSent, CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation, };
		public string[] AmendmentMessageStatusNotReqReviewAmendment => new string[] { CustomsMessageStatusTypeList.Codes.AmendmentRejected, CustomsMessageStatusTypeList.Codes.AmendmentSent, CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment, };

		#region Export
		public void TestStatusValidationFor830()
		{
			var declaration = GetDeclarationAndEntry(JobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._830);
			var entry = declaration.CustomsEntryHeaders[0];

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._830
				, (string status) => entry.CH_Status = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);
		}

		public void TestStatusValidationFor5AS()
		{
			var declaration = GetDeclarationAndEntry(JobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._5AS);
			var entry = declaration.CustomsEntryHeaders[0];

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5AS
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusTypeList
				, CannotSendAmendmentOrCancellationMessage);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5AS
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5AS
				, ElectronicDocumentTypeList.Codes._5DT
				, 1);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5AS
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5DT);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			sendingObj.ShouldSend = true;
			Assert("830 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationForDKJ()
		{
			var declaration = GetDeclarationAndEntry(JobMessageTypeList.Codes.Export, ElectronicDocumentTypeList.Codes._DKJ);
			var entry = declaration.CustomsEntryHeaders[0];

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._DKJ
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusTypeList
				, CannotSendAmendmentOrCancellationMessage);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._DKJ
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._DKJ
				, ElectronicDocumentTypeList.Codes._5DT
				, 1);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._DKJ
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5DT);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DKJ);
			sendingObj.ShouldSend = true;
			Assert("830 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}
		#endregion

		#region LocalExport
		public void TestStatusValidationFor5DP()
		{
			var declaration = GetDeclarationAndEntry(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DP);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5DP
				, (string status) => entry.CH_Status = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);
		}

		public void TestStatusValidationFor5DQ()
		{
			var declaration = GetDeclarationAndEntry(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DQ);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DQ;

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5DQ
				, (string status) => entry.CH_Status = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);
		}

		public void TestStatusValidationFor5DR()
		{
			var declaration = GetDeclarationAndEntry(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DR);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5DR
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusTypeList
				, CannotSendAmendmentOrCancellationMessage);

			AssertLocalExportOriginalMessageForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5DR
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted
				, ElectronicDocumentTypeList.Codes._5DP
				, ElectronicDocumentTypeList.Codes._RR3
				, 1);

			AssertLocalExportAmendmentMessageForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5DR
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5DR
				, ElectronicDocumentTypeList.Codes._RR3
				, 2);

			AssertLocalExportAmendmentMessageForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5DR
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5DR
				, ElectronicDocumentTypeList.Codes._RR3
				, 3);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5DR
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._RR3);
		}

		public void TestStatusValidationFor5DS()
		{
			var declaration = GetDeclarationAndEntry(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DS);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DQ;

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5DS
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusTypeList
				, CannotSendAmendmentOrCancellationMessage);

			AssertLocalExportOriginalMessageForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5DS
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted
				, ElectronicDocumentTypeList.Codes._5DQ
				, ElectronicDocumentTypeList.Codes._RR3
				, 1);

			AssertLocalExportAmendmentMessageForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5DS
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5DS
				, ElectronicDocumentTypeList.Codes._RR3
				, 2);

			AssertLocalExportAmendmentMessageForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5DS
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5DS
				, ElectronicDocumentTypeList.Codes._RR3
				, 3);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5DS
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._RR3);
		}

		public void TestStatusValidationForDF3()
		{
			var declaration = GetDeclarationAndEntry(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._DF3);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			var entryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._DF3);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._DF3
				, (string status) => entryNumber.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._DF3
				, ElectronicDocumentTypeList.Codes._5DQ
				, AmendmentMessageStatusTypeList);

			AssertLocalExportOriginalMessageForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._DF3
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted
				, ElectronicDocumentTypeList.Codes._5DQ
				, ElectronicDocumentTypeList.Codes._RR3
				, 1);

			AssertLocalExportAmendmentMessageForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._DF3
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5DS
				, ElectronicDocumentTypeList.Codes._RR3
				, 2);

			AssertLocalExportAmendmentMessageForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._DF3
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5DS
				, ElectronicDocumentTypeList.Codes._RR3
				, 3);

			var sendingObj = new JobDeclarationLoadingCompletionMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DF3);
			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._07;
			sendingObj.ShouldSend = true;
			AssertNoError(sendingObj.ShouldSendInfo, "If the 'Declaration Type is '08', 'DF3 - Local Export Completion Declaration' is not relevant and you should not send this message.");

			declaration.JE_MessageSubType = LocalExportTransactionNatureCodeList.Codes._08;
			sendingObj.ShouldSend = true;
			AssertHasError(sendingObj.ShouldSendInfo, "If the 'Declaration Type is '08', 'DF3 - Local Export Completion Declaration' is not relevant and you should not send this message.");
		}
		#endregion

		#region Import
		public void TestStatusValidationFor929()
		{
			var declaration = GetDeclarationAndEntry(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._929);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._929
				, (string status) => entry.CH_Status = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);
		}

		public void TestStatusValidationFor5FE()
		{
			var declaration = GetDeclarationAndEntry(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5FE);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5FE
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusTypeList
				, CannotSendAmendmentOrCancellationMessage);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5FE
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5FE
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5FE
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationFor934()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._934);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._934
				, (string status) => entryNumber.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			string mainMessageOriginal = string.Format(CannotSendSupplemetaryMessageDueToMainOriginalMessageNeverAccepted, ElectronicDocumentTypeList.Codes._934, ElectronicDocumentTypeList.Codes._929);

			foreach (string code in OrginalMessageStatusTypeList)
			{
				entry.CH_Status = code;
				var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._934);
				sendingObj.ShouldSend = true;
				Assert("934 does not have to wait until 929 is accepted", !sendingObj.ShouldSendInfo.HasError(mainMessageOriginal));
			}
		}

		public void TestStatusValidationFor5SC()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5SC);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SC);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5SC
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._5SC
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5SC
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5SC
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5SC
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5SC);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationFor105()
		{
			var declaration = GetDeclarationAndEntry(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._105);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SC);

			var exceptList = new string[] { CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation, CustomsMessageStatusTypeList.Codes.CancellationRejected };
			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._105
				, (string status) => entryNum.CE_EntryStatus = status
				, AmendmentMessageStatusTypeList.Except(exceptList).ToArray()
				, CannotSendAmendmentOrCancellationMessage);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._105
				, () => entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._105
				, ElectronicDocumentTypeList.Codes._106
				, 1);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._105
				, (string status) => entryNum.CE_EntryStatus = status
				, AmendmentMessageStatusNotReqReviewAmendment
				, ElectronicDocumentTypeList.Codes._106);
		}

		public void TestStatusValidationForDHR()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.DetailedFTACountries, "Detailed FTA Countries (DHR)");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "CN", "중국", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._DHR);
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine = entry.MergedLines.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._DHR);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CountryOfOrigin = "CN";

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._DHR
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._DHR
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._DHR
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._DHR
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._DHR
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DHR);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationForDHS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.DetailedFTACountries, "Detailed FTA Countries (DHR)");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "CN", "중국", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._DHS);
			var entry = declaration.CustomsEntryHeaders[0];
			var entryLine = entry.MergedLines.AddNew();
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._DHR);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CountryOfOrigin = "CN";

			var exceptList = new string[] { CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation, CustomsMessageStatusTypeList.Codes.CancellationRejected };
			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._DHS
				, (string status) => entryNum.CE_EntryStatus = status
				, AmendmentMessageStatusTypeList.Except(exceptList).ToArray()
				, CannotSendAmendmentOrCancellationMessage);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._DHS
				, () => entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._DHS
				, ElectronicDocumentTypeList.Codes._106
				, 1);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._DHS
				, (string status) => entryNum.CE_EntryStatus = status
				, AmendmentMessageStatusNotReqReviewAmendment
				, ElectronicDocumentTypeList.Codes._106);
		}

		public void TestStatusValidationFor5BF()
		{
			var declaration = GetDeclarationAndEntry(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5BF);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5BF
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusTypeList
				, CannotSendAmendmentOrCancellationMessage);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5BF
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5BF
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5BF
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BF);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationFor5BD()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5BD);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BD);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5BD
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._5BD
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5BD
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5BD
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5BD
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BD);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationFor5SG()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5SG);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SG);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5SG
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._5SG
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5SG
				, () => entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted
				, ElectronicDocumentTypeList.Codes._5SG
				, ElectronicDocumentTypeList.Codes._5SH
				, 0);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5SG
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5SG
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5SG
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5SG);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationFor5SI()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5SI);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5SI);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5SI
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._5SI
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5SI
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5SI
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5SI
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5SI);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationFor008()
		{
			var declaration = GetDeclarationAndEntry(ElectronicDocumentTypeList.Codes._008, ElectronicDocumentTypeList.Codes._008);
			var entry = declaration.CustomsEntryHeaders[0];

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._008
				, (string status) => entry.CH_Status = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);
		}

		public void TestStatusValidationFor5BA()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5BA);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BA);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5BA
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._5BA
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5BA
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5BA
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5BA
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BA);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationFor5BB()
		{
			var declaration = GetDeclarationAndEntry(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5BB);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BA);

			var exceptList = new string[] { CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation, CustomsMessageStatusTypeList.Codes.CancellationRejected };
			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5BB
				, (string status) => entryNum.CE_EntryStatus = status
				, AmendmentMessageStatusTypeList.Except(exceptList).ToArray()
				, CannotSendAmendmentOrCancellationMessage);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5BB
				, () => entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted
				, ElectronicDocumentTypeList.Codes._5BB
				, ElectronicDocumentTypeList.Codes._5BC
				, 1);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5BB
				, (string status) => entryNum.CE_EntryStatus = status
				, AmendmentMessageStatusNotReqReviewAmendment
				, ElectronicDocumentTypeList.Codes._5BC);
		}

		public void TestStatusValidationFor5SM()
		{
			var declaration = GetDeclarationAndEntry(ElectronicDocumentTypeList.Codes._5SM, ElectronicDocumentTypeList.Codes._5SM);
			var entry = declaration.CustomsEntryHeaders[0];

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5SM
				, (string status) => entry.CH_Status = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);
		}

		public void TestStatusValidationFor5TE()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5TE);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5TE);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5TE
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._5TE
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5TE
				, () => entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted
				, ElectronicDocumentTypeList.Codes._5TE
				, ElectronicDocumentTypeList.Codes._5TF
				, 0);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5TE
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5TE
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5TE
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5TE);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationFor5TM()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5TM);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5TM);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5TM
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._5TM
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5TM
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5TM
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5TM
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5TM);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationFor5UL()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5UL);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UL);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5UL
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5UL
				, () => entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted
				, ElectronicDocumentTypeList.Codes._5UL
				, ElectronicDocumentTypeList.Codes._RCA
				, 1);

			entryNum.CE_EntryStatus = string.Empty;

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._5UL
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5UL
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5UL
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._5UL
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5UL);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationForD72()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._D72);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._D72);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._D72
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._D72
				, () => entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted
				, ElectronicDocumentTypeList.Codes._D72
				, ElectronicDocumentTypeList.Codes._R43
				, 1);

			entryNum.CE_EntryStatus = string.Empty;

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._D72
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._D72
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK
				, 1);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._D72
				, () => entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationAccepted
				, ElectronicDocumentTypeList.Codes._5BF
				, ElectronicDocumentTypeList.Codes._5BG
				, 2);

			AssertWithCustomsReviewMessageIsNotExpected(entry
				, ElectronicDocumentTypeList.Codes._D72
				, (string status) => entry.CH_Status = status
				, AmendmentMessageStatusNotReqReviewAll
				, ElectronicDocumentTypeList.Codes._5FE
				, ElectronicDocumentTypeList.Codes._5FK);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._D72);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationFor5UA()
		{
			var declaration = GetDeclarationAndEntryAndEntryNum(JobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5UA);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UA);

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._5UA
				, (string status) => entryNum.CE_EntryStatus = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);

			AssertForCustomsReviewMessage(entry
				, ElectronicDocumentTypeList.Codes._5UA
				, () => entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted
				, ElectronicDocumentTypeList.Codes._5UA
				, ElectronicDocumentTypeList.Codes._5UB
				, 1);

			entryNum.CE_EntryStatus = string.Empty;

			AssertForSupplementaryMessage(entry
				, ElectronicDocumentTypeList.Codes._5UA
				, ElectronicDocumentTypeList.Codes._929
				, AmendmentMessageStatusTypeList);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var sendingObj = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5UA);
			sendingObj.ShouldSend = true;
			Assert("929 does not have a review message.", !sendingObj.ShouldSendInfo.GetMessageErrors().Any(x => x.Message.Contains("Customs' review message of type ")));
		}

		public void TestStatusValidationForD87()
		{
			var declaration = GetDeclarationAndEntry(ElectronicDocumentTypeList.Codes._D87, ElectronicDocumentTypeList.Codes._D87);
			var entry = declaration.CustomsEntryHeaders[0];

			AssertStatusValidation(entry
				, ElectronicDocumentTypeList.Codes._D87
				, (string status) => entry.CH_Status = status
				, OrginalMessageStatusTypeList
				, CannotSendOriginalMessage);
		}
		#endregion

		public void TestMessageErrorsAreCollectedCorrectly()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CustomsUnitQty = "";
			invoiceLine2.JI_CustomsQuantity = 10m;
			Assert("PreCondition", invoiceLine2.HasMessageErrors);
			var errorMessage = invoiceLine2.JI_CustomsQuantityInfo.GetMessageErrors().ToUniqueMessageListString();

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = KRJobMessageTypeList.Codes.Export;
			var entryLine1 = entry1.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = KRJobMessageTypeList.Codes.Export;
			var entryLine2 = entry2.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;

			var sendingObject1 = new JobDeclarationMessageSendingObject(entry1, ElectronicDocumentTypeList.Codes._830);
			sendingObject1.ShouldSend = true;
			AssertNotContains(errorMessage, sendingObject1.BizObjValidationMessageErrors);

			var sendingObject2 = new JobDeclarationMessageSendingObject(entry2, ElectronicDocumentTypeList.Codes._830);
			sendingObject2.ShouldSend = true;
			AssertContains(errorMessage, sendingObject2.BizObjValidationMessageErrors);
		}

		public void TestWarningsAreNotIncludedInMessageErrors()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345");

			var warningMsg = "Please check if this Manufacturer has a Industrial Park Code assigned. As there is no Industrial Park Code, '999' will be sent in a declaration.";
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			invoice.Validation.ValidateAll();
			Assert("Pre-condition", invoice.GetWarnings().Any(x => x.Message.Contains(warningMsg)));

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Pre-condition", 1, declaration.CustomsEntryHeaders.Count);

			var entry = declaration.CustomsEntryHeaders[0];
			var sendingObject = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._830);
			sendingObject.ShouldSend = true;
			AssertNotContains(warningMsg, sendingObject.BizObjValidationMessageErrors);
		}

		void AssertStatusValidation(CusEntryHeader entry, string messageType, Action<string> setStatus, string[] listOfAcceptedStatusCodes, string expectedMessageError)
		{
			var originalMessageType = ElectronicDocumentTypeList.GetOriginalType(messageType);
			if (string.IsNullOrEmpty(originalMessageType))
			{
				originalMessageType = messageType;
			}

			var messageStatusList = new CustomsMessageStatusTypeList();

			if (!ElectronicDocumentTypeList.SupportsCancellation(originalMessageType))
			{
				messageStatusList.RemoveCode(CustomsMessageStatusTypeList.Codes.CancellationAccepted);
				messageStatusList.RemoveCode(CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms);
				messageStatusList.RemoveCode(CustomsMessageStatusTypeList.Codes.CancellationByCustoms);
				messageStatusList.RemoveCode(CustomsMessageStatusTypeList.Codes.CancellationRejected);
				messageStatusList.RemoveCode(CustomsMessageStatusTypeList.Codes.CancellationSent);
				messageStatusList.RemoveCode(CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation);
				messageStatusList.RemoveCode(CustomsMessageStatusTypeList.Codes.CancellationDeclined);
			}

			var sendingObj = new JobDeclarationMessageSendingObject(entry, messageType);

			foreach (CodeDescriptionPair statusCode in messageStatusList)
			{
				setStatus(statusCode.Code);
				sendingObj.ShouldSend = true;
				if (listOfAcceptedStatusCodes.Contains(statusCode.Code))
				{
					AssertNoError(statusCode.Code, sendingObj.ShouldSendInfo, expectedMessageError);
				}
				else if (statusCode.Code == CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms)
				{
					var errorMsg = string.Format(CancellationApprovedByCustoms, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(entry.CH_MessageType));
					AssertHasError(statusCode.Code, sendingObj.ShouldSendInfo, errorMsg);
					sendingObj.ShouldSend = false;
					AssertNoError(statusCode.Code, sendingObj.ShouldSendInfo, errorMsg);
				}
				else if (statusCode.Code == CustomsMessageStatusTypeList.Codes.CancellationByCustoms)
				{
					if (!entry.Declaration.IsExport)
					{
						var errorMsg = string.Format(CancellationByCustoms, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(entry.CH_MessageType));
						AssertHasError(statusCode.Code, sendingObj.ShouldSendInfo, errorMsg);
						sendingObj.ShouldSend = false;
						AssertNoError(statusCode.Code, sendingObj.ShouldSendInfo, errorMsg);
					}
				}
				else if (CustomsMessageStatusTypeList.IsWaitingForResponse(statusCode.Code))
				{
					AssertHasError(statusCode.Code, sendingObj.ShouldSendInfo, WaitingForResponse);
					sendingObj.ShouldSend = false;
					AssertNoError(statusCode.Code, sendingObj.ShouldSendInfo, WaitingForResponse);
				}
				else
				{
					AssertHasError(statusCode.Code, sendingObj.ShouldSendInfo, expectedMessageError);
					sendingObj.ShouldSend = false;
					AssertNoError(statusCode.Code, sendingObj.ShouldSendInfo, expectedMessageError);
				}
			}

			setStatus(string.Empty);
			sendingObj.ShouldSend = true;
			if (listOfAcceptedStatusCodes.Contains(string.Empty))
			{
				AssertNoError("for an empty status code", sendingObj.ShouldSendInfo, expectedMessageError);
			}
			else
			{
				AssertHasError("for non-empty status code", sendingObj.ShouldSendInfo, expectedMessageError);
				sendingObj.ShouldSend = false;
				AssertNoError("for an empty status code", sendingObj.ShouldSendInfo, expectedMessageError);
			}
		}

		void AssertForSupplementaryMessage(CusEntryHeader entry, string messageType, string mainOriginalMessage, string[] listOfMainMessagingStatusCodesOriginalAllowed)
		{
			var sendingObj = new JobDeclarationMessageSendingObject(entry, messageType);
			var messageStatusList = entry.Factory.GetCachedValue<CustomsMessageStatusTypeList>();

			var expectedMessageError = string.Format(CannotSendSupplemetaryMessageDueToMainOriginalMessageNeverAccepted, messageType, mainOriginalMessage);
			foreach (CodeDescriptionPair statusCode in messageStatusList)
			{
				entry.CH_Status = statusCode.Code;
				sendingObj.ShouldSend = true;
				if (listOfMainMessagingStatusCodesOriginalAllowed.Contains(statusCode.Code))
				{
					AssertNoError(statusCode.Code, sendingObj.ShouldSendInfo, expectedMessageError);
				}
				else if (CustomsMessageStatusTypeList.IsWaitingForResponse(statusCode.Code))
				{
					AssertHasError(statusCode.Code, sendingObj.ShouldSendInfo, WaitingForResponse);
					sendingObj.ShouldSend = false;
					AssertNoError(statusCode.Code, sendingObj.ShouldSendInfo, WaitingForResponse);
				}
				else if (statusCode.Code != CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms && statusCode.Code != CustomsMessageStatusTypeList.Codes.CancellationByCustoms)
				{
					AssertHasError(statusCode.Code, sendingObj.ShouldSendInfo, expectedMessageError);
					sendingObj.ShouldSend = false;
					AssertNoError(statusCode.Code, sendingObj.ShouldSendInfo, expectedMessageError);
				}
			}
		}

		void AssertForCustomsReviewMessage(CusEntryHeader entry, string messageType, Action setStatus, string outgoingMessageType, string reviewMessageType, int order)
		{
			var sendingObj = new JobDeclarationMessageSendingObject(entry, messageType);
			var expectedMessage = string.Format(WaitForCustomsApprovalMessage, reviewMessageType, outgoingMessageType);

			var outgoingMsg = entry.Messages.AddNew();
			outgoingMsg.EM_MessageType = outgoingMessageType;
			outgoingMsg.EM_MessageNum = "1111" + order;
			outgoingMsg.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddHours(order);

			setStatus();

			sendingObj.ShouldSend = true;
			AssertHasError(sendingObj.ShouldSendInfo, expectedMessage);

			sendingObj.ShouldSend = false;
			AssertNoError(sendingObj.ShouldSendInfo, expectedMessage);

			var customsReviewMessageForMsg = entry.Messages.AddNew();
			customsReviewMessageForMsg.EM_MessageType = reviewMessageType;
			customsReviewMessageForMsg.EM_MessageNum = "1112" + order;
			customsReviewMessageForMsg.EM_ApplicationReference = "1111" + order;
			customsReviewMessageForMsg.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.DMS;
			sendingObj.ShouldSend = true;
			AssertNoError(sendingObj.ShouldSendInfo, expectedMessage);

			customsReviewMessageForMsg.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			sendingObj.ShouldSend = true;
			AssertNoError(sendingObj.ShouldSendInfo, expectedMessage);
		}

		void AssertLocalExportOriginalMessageForCustomsReviewMessage(CusEntryHeader entry, string messageType, Action setStatus, string outgoingMessageType, string reviewMessageType, int order)
		{
			var sendingObj = new JobDeclarationMessageSendingObject(entry, messageType);
			var outgoingMsg = entry.Messages.AddNew();
			outgoingMsg.EM_MessageType = outgoingMessageType;
			outgoingMsg.EM_MessageNum = "1111" + order;
			outgoingMsg.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddHours(order);
			setStatus();
			sendingObj.ShouldSend = true;
			AssertHasErrorContaining(sendingObj.ShouldSendInfo, "Customs' review message of type 'RR3' has not arrived yet");
			var customsReviewMessageForMsg = entry.Messages.AddNew();
			customsReviewMessageForMsg.EM_MessageType = reviewMessageType;
			customsReviewMessageForMsg.EM_MessageNum = "1112" + order;
			customsReviewMessageForMsg.EM_ApplicationReference = "1111" + order;
			var errorMsg = string.Format(CancellationByCustoms, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(entry.CH_MessageType));
			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ANT;
			sendingObj.ShouldSend = true;
			AssertNoErrorContaining("RR3 response is D1", sendingObj.ShouldSendInfo, errorMsg);
			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.OGJ;
			sendingObj.ShouldSend = true;
			AssertNoErrorContaining("RR3 response is B3", sendingObj.ShouldSendInfo, errorMsg);
			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CNR;
			sendingObj.ShouldSend = true;
			AssertNoErrorContaining("RR3 response is D5", sendingObj.ShouldSendInfo, errorMsg);
			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.MFI;
			sendingObj.ShouldSend = true;
			AssertNoErrorContaining("RR3 response is E7", sendingObj.ShouldSendInfo, errorMsg);
			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.DMS;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
			sendingObj.ShouldSend = true;
			AssertHasError("RR3 response is D3", sendingObj.ShouldSendInfo, errorMsg);
			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCL;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
			sendingObj.ShouldSend = true;
			AssertHasError("RR3 response is E1", sendingObj.ShouldSendInfo, errorMsg);
		}

		void AssertLocalExportAmendmentMessageForCustomsReviewMessage(CusEntryHeader entry, string messageType, Action setStatus, string outgoingMessageType, string reviewMessageType, int order)
		{
			var sendingObj = new JobDeclarationMessageSendingObject(entry, messageType);
			var expectedMessageNoExist = string.Format(WaitForCustomsApprovalMessage, reviewMessageType, outgoingMessageType);

			var outgoingMsg = entry.Messages.AddNew();
			outgoingMsg.EM_MessageType = outgoingMessageType;
			outgoingMsg.EM_MessageNum = "1111" + order;
			outgoingMsg.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddHours(order);

			setStatus();

			sendingObj.ShouldSend = true;
			AssertHasError(sendingObj.ShouldSendInfo, expectedMessageNoExist);

			var customsReviewMessageForMsg = entry.Messages.AddNew();
			customsReviewMessageForMsg.EM_MessageType = reviewMessageType;
			customsReviewMessageForMsg.EM_MessageNum = "1112" + order;
			customsReviewMessageForMsg.EM_ApplicationReference = "1111" + order;
			var errorMsg = string.Format(CancellationApprovedByCustoms, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(entry.CH_MessageType));
			sendingObj.ShouldSend = true;
			AssertNoError(sendingObj.ShouldSendInfo, expectedMessageNoExist);

			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ANT;
			sendingObj.ShouldSend = true;
			AssertNoError("RR3 response is D1 and outgoing message EM_SubType is 1", sendingObj.ShouldSendInfo, errorMsg);

			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.OGJ;
			sendingObj.ShouldSend = true;
			AssertNoError("RR3 response is B3", sendingObj.ShouldSendInfo, errorMsg);

			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CNR;
			sendingObj.ShouldSend = true;
			AssertNoError("RR3 response is D5", sendingObj.ShouldSendInfo, errorMsg);

			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.MFI;
			sendingObj.ShouldSend = true;
			AssertNoError("RR3 response is E7", sendingObj.ShouldSendInfo, errorMsg);

			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.DMS;
			sendingObj.ShouldSend = true;
			AssertNoError("RR3 response is D3", sendingObj.ShouldSendInfo, errorMsg);

			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.CCL;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms;
			sendingObj.ShouldSend = true;
			AssertHasError("RR3 response is D1 and outgoing message EM_SubType is 2", sendingObj.ShouldSendInfo, errorMsg);
		}

		void AssertWithCustomsReviewMessageIsNotExpected(CusEntryHeader entry, string messageType, Action<string> setStatus, string[] listOfStatusCodesNotReqReviewMessage, string reviewMessageType)
		{
			AssertWithCustomsReviewMessageIsNotExpected(entry, messageType, setStatus, listOfStatusCodesNotReqReviewMessage, messageType, reviewMessageType);
		}

		void AssertWithCustomsReviewMessageIsNotExpected(CusEntryHeader entry, string messageType, Action<string> setStatus, string[] listOfStatusCodesNotReqReviewMessage, string outgoingMessageType, string reviewMessageType)
		{
			var sendingObj = new JobDeclarationMessageSendingObject(entry, messageType);
			var outgoingMessage = entry.Messages.AddNew();
			outgoingMessage.EM_MessageType = outgoingMessageType;
			outgoingMessage.EM_MessageNum = "1111AAA";

			var expectedError = string.Format(WaitForCustomsApprovalMessage, reviewMessageType, messageType);

			foreach (var code in listOfStatusCodesNotReqReviewMessage)
			{
				setStatus(code);
				sendingObj.ShouldSend = true;

				Assert(!sendingObj.ShouldSendInfo.HasMessageError(expectedError));
			}
		}

		JobDeclaration GetDeclarationAndEntry(string shipmentType, string messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = shipmentType;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = shipmentType;
			var outgoingMessage = entry.Messages.AddNew();
			outgoingMessage.EM_MessageType = messageType;
			outgoingMessage.EM_ReceiveTransmit = "TRX";
			outgoingMessage.EM_MessageNum = "999";
			return declaration;
		}

		JobDeclaration GetDeclarationAndEntryAndEntryNum(string shipmentType, string type)
		{
			var declaration = GetDeclarationAndEntry(shipmentType, type);
			var entryNum = declaration.CustomsEntryHeaders[0].EntryNumbers.AddNew();
			entryNum.CE_EntryType = type;

			return declaration;
		}

		public void TestSetValidationModeInMessageSendingPopup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_ValuationDecAttachCode = YesNo.Yes;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entry.MergedLines.AddNew().PK;
			Assert(!declaration.IsValidationModeSetFor934);
			invoice.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrorContaining(invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var messageSendingObjects = (ValuationDeclarationMessageSendingObjectParent)JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._934, MessageFunctions.MessageFunctionCode.Original);
			AssertEquals(1, messageSendingObjects.SendingObjectsCollection.Count);
			var messageSendingObject934 = messageSendingObjects.SendingObjectsCollection[0];
			messageSendingObject934.ShouldSend = true;
			Assert(declaration.IsValidationModeSetFor934);
			invoice.Validation.ValidateJZ_ValuationCode();
			AssertHasMessageErrorContaining(invoice.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertContains("You have not entered a Valuation Code.", messageSendingObject934.BizObjValidationMessageErrors);
		}

		const string CannotSendOriginalMessage = "You cannot send this message. Its status indicates Customs has already accepted a message of this type.";
		const string WaitingForResponse = "You cannot send this message. Its status indicates the last electronic document is still waiting for a response.";
		public const string CancellationApprovedByCustoms = "You cannot send this message. Its status indicates the cancellation of a declaration has been accepted by Customs.Please refer to the recent {0} message";
		const string CannotSendAmendmentOrCancellationMessage = "You cannot send this message now. Its status indicates Customs has never accepted an original message of this type.";
		const string WaitForCustomsApprovalMessage = "Customs' review message of type '{0}' has not arrived yet. Please wait until it arrives as a response to the last electronic document, '{1}'";
		const string CannotSendSupplemetaryMessageDueToMainOriginalMessageNeverAccepted = "You cannot send this message. The status of '{0}' indicates Customs has never accepted an original message of '{1}'. Please send a message of '{1}' before trying to send this message.";
		public const string CancellationByCustoms = "This entry has been declined by Customs. Please refer to the recent {0} message. You can no longer send further messages on this entry.";
	}
}
