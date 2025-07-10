using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRCusEntryCPDecValidationTest : TestCaseWithFactory
	{
		#region TestCheckON_Permit

		public void TestCheckON_PermitWhenIsRiskCalculatedFromTariff()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_IsConsignee = true;
			org.OH_RL_NKClosestPort = "AUSYD";

			CreateCMRLodgementQuestion(400, CMRCusEntryCPDec.LodgementQuestionTypes.CommunityProtectionQuestion);
			CreateRiskWithIndicator(false);

			CMRCommunityProtectionProfile profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_TariffClassificationNumberfield = "84716000";
			profile.CP_StatisticalClassificationCodefield = "55";
			profile.CP_CommunityProtectionRiskIdentifier = 500;
			profile.CP_LineNatureTypefield = "N10";

			OrganisationCPQA orgAttachee = new OrganisationCPQA(org);
			CMRCusEntryCPDec cPDec = orgAttachee.Questions.AddNew();
			cPDec.ON_CPDecNum = 400;
			cPDec.ON_CPDecStartDate = currentDate.AddDays(-1000);
			cPDec.ON_AnswerCode = "Y";
			cPDec.ON_Permit = "PERMIT";

			JobDeclaration dec = JobDeclaration.New(Factory);
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			dec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			dec.JE_OH_Importer = org.PK;

			JobComInvoiceLine line1 = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "8471.60.00 55";

			Factory.Save();
			dec.DoMerge();

			Assert("Precondition", dec.CustomsEntryHeaders.Count > 0 && dec.CustomsEntryHeaders[0].MergedLines.Count > 0);
			Assert("Precondition", dec.CustomsEntryHeaders[0].MergedLines[0].Questions.Count > 0);

			foreach (CusEntryHeader entryHeader in dec.CustomsEntryHeaders)
			{
				entryHeader.RunPreSaveValidation();
			}
			Assert(dec.CustomsEntryHeaders[0].MergedLines[0].Questions[0].ON_PermitInfo.HasMessageError(CMRCusEntryCPDecValidation.PermitNotRelevant));
		}

		public void TestCheckON_PermitWhenNoRisks()
		{
			CreateCMRLodgementQuestion(400, CMRCusEntryCPDec.LodgementQuestionTypes.CommunityProtectionQuestion);

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_IsConsignee = true;

			Factory.Save();

			ZQuery filter = new ZQuery(CMRCommunityProtectionRiskSchema.CK_LodgementQuestionIdentifier, SQLComparisonOperator.Equal, 400);
			CMRCommunityProtectionRisk[] risks = (CMRCommunityProtectionRisk[])Factory.Load(typeof(CMRCommunityProtectionRisk), filter);
			AssertEquals("Precondition: no risks for this question", 0, risks.Length);

			OrganisationCPQA orgAttachee = new OrganisationCPQA(org);
			CMRCusEntryCPDec cPDec = orgAttachee.Questions.AddNew();
			cPDec.ON_CPDecNum = 400;
			cPDec.ON_CPDecStartDate = currentDate.AddDays(-1000);
			cPDec.ON_AnswerCode = "Y";
			cPDec.ON_Permit = "PERMIT";

			AssertNotNull(cPDec.LodgementQuestion);
			Assert(cPDec.ON_PermitInfo.HasNotification(CMRCusEntryCPDecValidation.PermitNotRelevant));
		}

		public void TestCheckON_PermitWhenRiskHasNoPermitIndicator()
		{
			CreateCMRLodgementQuestion(400, CMRCusEntryCPDec.LodgementQuestionTypes.CommunityProtectionQuestion);
			CreateRiskWithIndicator(false);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_IsConsignee = true;

			Factory.Save();

			OrganisationCPQA orgAttachee = new OrganisationCPQA(org);
			CMRCusEntryCPDec cPDec = orgAttachee.Questions.AddNew();
			cPDec.ON_CPDecNum = 400;
			cPDec.ON_CPDecStartDate = currentDate.AddDays(-1000);
			cPDec.ON_AnswerCode = "Y";
			cPDec.ON_Permit = "PERMIT";

			AssertNotNull(cPDec.LodgementQuestion);
			Assert(cPDec.ON_PermitInfo.HasNotification(CMRCusEntryCPDecValidation.PermitNotRelevant));
		}

		public void CreateRiskWithIndicator(bool permitIndicator)
		{
			CMRCommunityProtectionRisk risk = CMRCommunityProtectionRisk.New(Factory);
			risk.CK_Identifier = 500;
			risk.CK_StartDate = currentDate.AddDays(-1000);
			risk.CK_EndDate = currentDate.AddDays(1000);
			risk.CK_PermitApplicationIndicator = permitIndicator;
			risk.CK_LodgementQuestionIdentifier = 400;
		}

		public void CreateCMRLodgementQuestion(int questionNumber, ZString questionType)
		{
			CMRLodgementQuestion lodgementQuestion = CMRLodgementQuestion.New(Factory);
			lodgementQuestion.CQ_LodgementQuestionIdentifier = questionNumber;
			lodgementQuestion.CQ_LodgementQuestionType = questionType;
			lodgementQuestion.CQ_LodgementQuestionText = "Question to be asked";
			lodgementQuestion.CQ_LodgementQuestionStartDate = currentDate.AddDays(-1000);
			Factory.Save();
		}

		#endregion

		public void TestValidateCPDecNumWhenThereIsNoRisk()
		{
			CMRLodgementQuestion question = CMRLodgementQuestion.New(Factory);
			question.CQ_LodgementQuestionIdentifier = 4000;
			question.CQ_LodgementQuestionStartDate = ZDateTime.Today;

			CMRLodgementQuestion question2 = CMRLodgementQuestion.New(Factory);
			question2.CQ_LodgementQuestionIdentifier = 1;
			question2.CQ_LodgementQuestionStartDate = ZDateTime.Today;

			CMRCommunityProtectionProfile profile = CMRCommunityProtectionProfile.New(Factory);
			profile.CP_CommunityProtectionRiskIdentifier = 5000;
			profile.CP_LineNatureTypefield = "N10";
			profile.CP_TariffClassificationNumberfield = "00000000";

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec ackQuestion = entryHeader.Questions.AddNew();
			ackQuestion.ON_CPDecNum = 1;
			ackQuestion.ON_CPDecStartDate = ZDateTime.Today;

			AssertEquals("Acknowledge question", true, ackQuestion.IsAcknowledge);
			AssertNoMessageError(ackQuestion.ON_CPDecNumInfo, CMRCusEntryCPDecValidation.NoRiskIDForCPDecQuestion);

			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CMRCusEntryCPDec cPDec = entryLine.Questions.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Tariff = "00000000 00";

			cPDec.ON_CPDecNum = 4000;
			cPDec.ON_CPDecStartDate = ZDateTime.Today;
			AssertHasMessageError(cPDec.ON_CPDecNumInfo, CMRCusEntryCPDecValidation.NoRiskIDForCPDecQuestion);

			CMRCommunityProtectionRisk risk = CMRCommunityProtectionRisk.New(Factory);
			risk.CK_Identifier = 5000;
			risk.CK_LodgementQuestionIdentifier = 4000;
			risk.CK_StartDate = ZDateTime.Today;

			cPDec.ON_CPDecNum = 4000;
			AssertNoMessageError(cPDec.ON_CPDecNumInfo, CMRCusEntryCPDecValidation.NoRiskIDForCPDecQuestion);
		}

		public void TestValidateQuestionID()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_IsConsignee = true;
			CreateCMRLodgementQuestion(400, CMRCusEntryCPDec.LodgementQuestionTypes.CommunityProtectionQuestion);
			CreateCMRLodgementQuestion(401, CMRCusEntryCPDec.LodgementQuestionTypes.GeneralLodgementQuestion);

			AUOrgSupplierPart part = AUOrgSupplierPart.New(Factory);
			var pivot = part.PivotsForBinding.AddNew();
			CMRCusEntryCPDec cPDec = pivot.Questions.AddNew();
			cPDec.QuestionID = "401";
			AssertEquals("QuestionID should not have notifications", false, cPDec.QuestionIDInfo.HasNotifications());

			cPDec.QuestionID = "400";
			AssertEquals("QuestionID should not have notifications", false, cPDec.QuestionIDInfo.HasNotifications());

			OrganisationCPQA orgAttachee = new OrganisationCPQA(org);
			CMRCusEntryCPDec cPDec1 = orgAttachee.Questions.AddNew();
			cPDec1.QuestionID = "401";
			AssertEquals("QuestionID should have an error", true, cPDec1.QuestionIDInfo.HasError(CMRCusEntryCPDecValidation.InvalidLodgementQuestion));

			cPDec1.QuestionID = "400";
			AssertEquals("QuestionID should not have notifications", false, cPDec1.QuestionIDInfo.HasNotifications());

			cPDec1.QuestionID = "1-" + currentDate.ToShortDateString();
			AssertEquals("QuestionID:", true, cPDec1.QuestionIDInfo.HasError(CMRCusEntryCPDecValidation.InvalidLodgementQuestion));
		}

		public void TestValidateQuestionIDWhenDuplicateQuestions()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.OH_IsConsignee = true;

			CreateCMRLodgementQuestion(1000, CMRCusEntryCPDec.LodgementQuestionTypes.CommunityProtectionQuestion);

			OrganisationCPQA orgAttachee = new OrganisationCPQA(org);
			CMRCusEntryCPDec cPDec1 = orgAttachee.Questions.AddNew();
			cPDec1.QuestionID = "1000";
			AssertEquals("QuestionIDInfo:", false, cPDec1.QuestionIDInfo.HasNotifications());

			CMRCusEntryCPDec cPDec2 = orgAttachee.Questions.AddNew();
			cPDec2.QuestionID = "1000";

			AssertEquals("QuestionIDInfo:", true, cPDec2.QuestionIDInfo.HasError(CMRCusEntryCPDecValidation.DuplicateLodgementQuestion));
		}

		public void TestValidateQuestions7()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			CusContainer container1 = testDec.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C1";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			CusContainer container2 = testDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C2";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_CPDecNum = 7;
			question.ON_AnswerCode = "N";
			AssertHasMessageError(question.ON_AnswerCodeInfo, "As you have answered 'No' to this question, at least one Quarantine Concern Type is required.");

			question.ON_AnswerCode = "Y";
			AssertNoMessageErrors("should not have any message errors", question.ON_AnswerCodeInfo);

			question.ON_AnswerCode = "N";
			AssertHasMessageError(question.ON_AnswerCodeInfo, "As you have answered 'No' to this question, at least one Quarantine Concern Type is required.");

			AQISConcernType concernType = testDec.AQISConcernTypes.AddNew();
			concernType.Code = "TTT";
			AssertNoMessageErrors("should not have any message errors", question.ON_AnswerCodeInfo);
		}

		public void TestValidateQuestion1()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_CPDecNum = 1;
			question.ON_AnswerCode = "N";
			AssertHasMessageError(question.ON_AnswerCodeInfo, "This is a required lodgement declaration (not a question), you must acknowledge this declaration by entering YES, otherwise your entry/amendment will be rejected.");
			question.ON_AnswerCode = "Y";
			AssertEquals("should not have any message errors", false, question.ON_AnswerCodeInfo.HasMessageErrors());
		}

		public void TestListValidation()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_AnswerCode = "YES";
			AssertEquals("Answer needs to be Y or N", true, question.ON_AnswerCodeInfo.HasErrors());

			question.ON_AnswerCode = "Y";
			AssertEquals("Answer needs to be Y or N", false, question.ON_AnswerCodeInfo.HasErrors());

			var entryLine = entryHeader.AllEntryLines.AddNew();
			var lineQuestion = entryLine.Questions.AddNew();
			lineQuestion.ON_AnswerCode = "Ys";
			AssertEquals("Answer needs to be Y or N", true, lineQuestion.ON_AnswerCodeInfo.HasErrors());

			lineQuestion.ON_AnswerCode = "Y";
			AssertEquals("Answer needs to be Y or N", false, lineQuestion.ON_AnswerCodeInfo.HasErrors());
		}

		public void TestMessageErrorIfAnsweredNoToSACWithLine()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.RunPreSaveValidation();
			AssertHasMessageErrors(question.ON_AnswerCodeInfo);
		}

		public void TestGoodsHaveBeenDealthWithPostPayment()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_CPDecNum = 14;
			question.ON_AnswerCode = "Y";
			AssertHasWarnings(question.ON_AnswerCodeInfo);
		}

		public void TestMessageErrorForNotAnsweredQuestion()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.RunPreSaveValidation();
			AssertHasMessageError(question.ON_AnswerCodeInfo, "You have not answered this Declaration or CP Dec question.");

			question.ON_CPDecNum = 6;
			AssertEquals("IsOptional", false, question.IsOptionalQuestion);
			question.RunPreSaveValidation();
			AssertHasMessageError(question.ON_AnswerCodeInfo, "You have not answered this Declaration or CP Dec question.");

			question.ON_AnswerCode = "Y";
			question.RunPreSaveValidation();
			Assert(!question.ON_AnswerCodeInfo.HasMessageErrors());

			question = entryHeader.Questions.AddNew();
			question.ON_CPDecNum = 1;
			question.ON_AnswerCode = ZString.Empty;
			question.RunPreSaveValidation();
			AssertHasMessageError(question.ON_AnswerCodeInfo, "This is a required lodgement declaration (not a question), you must acknowledge this declaration by entering YES, otherwise your entry/amendment will be rejected.");
			question.ON_AnswerCode = "Y";
			question.RunPreSaveValidation();
			Assert(!question.ON_AnswerCodeInfo.HasMessageErrors());
			question.ON_AnswerCode = "N";
			question.RunPreSaveValidation();
			AssertHasMessageError(question.ON_AnswerCodeInfo, "This is a required lodgement declaration (not a question), you must acknowledge this declaration by entering YES, otherwise your entry/amendment will be rejected.");

			question = entryHeader.Questions.AddNew();
			question.ON_CPDecNum = 375;
			question.ON_AnswerCode = ZString.Empty;
			question.RunPreSaveValidation();
			AssertNoMessageErrors(question.ON_AnswerCodeInfo);
			question.ON_AnswerCode = "Y";
			question.RunPreSaveValidation();
			AssertNoMessageErrors(question.ON_AnswerCodeInfo);
			question.ON_AnswerCode = "N";
			question.RunPreSaveValidation();
			AssertNoMessageErrors(question.ON_AnswerCodeInfo);
		}

		public void TestNoMessageErrorForExWarehouse()
		{
			JobDeclaration dec = JobDeclaration.New(Factory);
			dec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			CusEntryHeader entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.SetDeclarationForTesting(dec);
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.Validation.ValidateON_AnswerCode();
			Assert("No msg errrors but has errors: " + question.ON_AnswerCodeInfo.GetMessageErrors().ToUniqueMessageListString(), !question.ON_AnswerCodeInfo.HasMessageErrors());

			dec.JE_MessageType = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
			question.Validation.ValidateON_AnswerCode();
			Assert("No msg errrors but has errors: " + question.ON_AnswerCodeInfo.GetMessageErrors().ToUniqueMessageListString(), !question.ON_AnswerCodeInfo.HasMessageErrors());
		}

		public void TestCheckAQISInspectionLocation()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_CPDecNum = 19;

			testDec.AddInfo.ZA_AQISInspectLocation_Hidden = "";

			question.ON_AnswerCode = "Y";
			AssertEquals("AQIS reference", true, question.ON_AnswerCodeInfo.HasMessageErrors());

			question.Validation.ValidateAll();
			AssertEquals("AQIS reference still validated", true, question.ON_AnswerCodeInfo.HasMessageErrors());

			question.ON_AnswerCode = "N";
			AssertEquals("AQIS reference", false, question.ON_AnswerCodeInfo.HasMessageErrors());

			testDec.AddInfo.ZA_AQISInspectLocation_Hidden = "TEST";
			question.ON_AnswerCode = "Y";
			AssertEquals("AQIS reference", false, question.ON_AnswerCodeInfo.HasMessageErrors());

			testDec.AddInfo.ZA_AQISInspectLocation_Hidden = "";
			AssertEquals("AQIS reference, test CheckZA_AQISInspectLocation_Hidden validates CPDecs", true, question.ON_AnswerCodeInfo.HasMessageErrors());

			testDec.AddInfo.ZA_AQISInspectLocation_Hidden = "ANOTHER TEST";
			AssertEquals("AQIS reference, test CheckZA_AQISInspectLocation_Hidden validates CPDecs", false, question.ON_AnswerCodeInfo.HasMessageErrors());
		}

		public void TestCheckSAC()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CMRCusEntryCPDec question = entryHeader.Questions.AddNew();
			question.ON_CPDecNum = 18;
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;

			AssertEquals("IsSACWithoutLines", true, testDec.IsSACWithoutLines);

			question.ON_AnswerCode = "N";
			Assert(!question.ON_AnswerCodeInfo.HasMessageErrors());

			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			question.RunPreSaveValidation();
			Assert(question.ON_AnswerCodeInfo.HasMessageErrors());
			question.ON_AnswerCode = "Y";
			question.RunPreSaveValidation();
			Assert(!question.ON_AnswerCodeInfo.HasMessageErrors());
		}

		readonly ZDateTime currentDate = ZDateTime.Today;
	}
}
