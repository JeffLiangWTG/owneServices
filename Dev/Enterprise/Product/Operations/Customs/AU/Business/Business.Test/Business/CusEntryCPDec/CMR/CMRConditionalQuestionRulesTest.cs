using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRConditionalQuestionRulesTest : TestCaseWithFactory
	{
		public void TestGeneralDeclarationGenerationForTestLowValueDeclaration()
		{
			SetUpQuestionsAndDeclaration(true);
			entryHeader.GetNatureForImportCMRReturns = CusEntryHeader.NatureTypesForImportCMR.Nature10;
			entryLine.CL_CustomsValue = 0;
			RunGeneralDeclarationGenerationTestCase(false, "M", "", "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "C");
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "M", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "C", "C", "C");
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "", "", "M", "C", "M", "M", "", "", "", "", "", "", "");
		}

		public void TestGeneralDeclarationGenerationForProdLowValueDeclaration()
		{
			SetUpQuestionsAndDeclaration(false);
			entryHeader.GetNatureForImportCMRReturns = CusEntryHeader.NatureTypesForImportCMR.Nature10;
			entryLine.CL_CustomsValue = 0;
			entryHeader.CH_Status = "";
			RunGeneralDeclarationGenerationTestCase(false, "M", "", "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "C");
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "M", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "C", "C", "C");
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "", "", "M", "C", "M", "M", "", "", "", "", "", "", "");
		}

		public void TestGeneralDeclarationGenerationForNormalDeclaration()
		{
			SetUpQuestionsAndDeclaration();
			entryHeader.GetNatureForImportCMRReturns = CusEntryHeader.NatureTypesForImportCMR.Nature10;
			RunGeneralDeclarationGenerationTestCase(false, "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.EntryNumber = "AAAAAAA";
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "C", "C", "", "", "", "", "C", "C");
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "C", "M", "C", "C", "", "", "", "", "", "");
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "C", "C");
			entryLine.RefundReasonCode = "126A";
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "M", "C");
			entryHeader.IsARefundDueReturns = true;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "M", "M", "", "", "M", "M", "", "", "", "", "M", "C");
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "M", "C");
			var importer = Factory.New<OrgHeader>();
			importer.MiscServ.OM_IMIsGSTDeferred = true;
			testDec.JE_OH_Importer = importer.PK;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "C", "", "", "", "", "M", "C");
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "M", "", "M", "C", "", "", "", "", "", "");
			importer.MiscServ.OM_IMIsGSTDeferred = false;
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "M", "", "M", "M", "", "", "", "", "", "");
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "M", "", "M", "", "M", "M", "", "", "", "", "", "");
		}

		public void TestGeneralDeclarationGenerationForN20Declaration()
		{
			SetUpQuestionsAndDeclaration();
			entryHeader.GetNatureForImportCMRReturns = CusEntryHeader.NatureTypesForImportCMR.Nature20;
			RunGeneralDeclarationGenerationTestCase(false, "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.EntryNumber = "AAAAAAA";
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "C", "", "", "", "", "", "C", "C");
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "C", "M", "C", "", "", "", "", "", "", "");
		}

		public void TestGeneralDeclarationGenerationForN10N20Declaration()
		{
			SetUpQuestionsAndDeclaration();
			entryHeader.GetNatureForImportCMRReturns = CusEntryHeader.NatureTypesForImportCMR.Nature1020;
			RunGeneralDeclarationGenerationTestCase(false, "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.EntryNumber = "AAAAAAA";
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "C", "C", "", "", "", "", "C", "C");
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "C", "M", "C", "C", "", "", "", "", "", "");
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "C", "C");
			entryLine.RefundReasonCode = "126A";
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "M", "C");
			entryHeader.IsARefundDueReturns = true;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "M", "M", "", "", "M", "M", "", "", "", "", "M", "C");
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "M", "C");
			var importer = Factory.New<OrgHeader>();
			importer.MiscServ.OM_IMIsGSTDeferred = true;
			testDec.JE_OH_Importer = importer.PK;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "C", "", "", "", "", "M", "C");
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "M", "", "M", "C", "", "", "", "", "", "");
			importer.MiscServ.OM_IMIsGSTDeferred = false;
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "M", "", "M", "M", "", "", "", "", "", "");
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "M", "", "M", "", "M", "M", "", "", "", "", "", "");
		}

		public void TestGeneralDeclarationGenerationForN30Declaration()
		{
			SetUpQuestionsAndDeclaration();
			entryHeader.GetNatureForImportCMRReturns = CusEntryHeader.NatureTypesForImportCMR.Nature30;
			RunGeneralDeclarationGenerationTestCase(false, "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			entryHeader.EntryNumber = "AAAAAAA";
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "C", "C", "", "", "", "", "C", "C");
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "C", "M", "C", "C", "", "", "", "", "", "");
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "C", "C");
			entryLine.RefundReasonCode = "126A";
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "M", "C");
			entryHeader.IsARefundDueReturns = true;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "M", "M", "", "", "M", "M", "", "", "", "", "M", "C");
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Importer;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "M", "", "", "", "", "M", "C");
			var importer = Factory.New<OrgHeader>();
			importer.MiscServ.OM_IMIsGSTDeferred = true;
			testDec.JE_OH_Importer = importer.PK;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "", "", "", "", "C", "M", "", "", "M", "C", "", "", "", "", "M", "C");
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "M", "", "M", "C", "", "", "", "", "", "");
			importer.MiscServ.OM_IMIsGSTDeferred = false;
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "M", "", "M", "M", "", "", "", "", "", "");
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "M", "", "M", "", "M", "M", "", "", "", "", "", "");
		}

		public void TestGeneralDeclarationGenerationForSACDeclaration()
		{
			SetUpQuestionsAndDeclaration();
			entryHeader.GetNatureForImportCMRReturns = CusEntryHeader.NatureTypesForImportCMR.Nature10;
			entryLine.CL_CustomsValue = 0;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "M", "M", "M", "M", "", "");
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "", "", "M", "C", "M", "", "", "", "", "", "", "");
		}

		public void TestGeneralDeclarationGenerationForSACWithLinesDeclaration()
		{
			SetUpQuestionsAndDeclaration();
			entryHeader.GetNatureForImportCMRReturns = CusEntryHeader.NatureTypesForImportCMR.Nature10;
			entryLine.CL_CustomsValue = 0;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "C", "", "C", "M", "C", "C", "", "", "", "", "", "");
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			RunGeneralDeclarationGenerationTestCase(true, "", "", "", "", "", "", "", "", "", "M", "", "M", "", "M", "M", "", "", "", "", "", "");
		}

		public void TestGeneralDeclarationGenerationForAQISContainers()
		{
			SetUpQuestionsAndDeclaration();
			entryHeader.GetNatureForImportCMRReturns = CusEntryHeader.NatureTypesForImportCMR.Nature10;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container1 = testDec.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			RunGeneralDeclarationGenerationTestCase(false, "M", "", "", "", "", "M", "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			RunGeneralDeclarationGenerationTestCase(false, "M", "", "", "", "", "", "", "M", "M", "", "", "", "", "", "", "", "", "", "", "", "");
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCLMixedShipper;
			RunGeneralDeclarationGenerationTestCase(false, "M", "", "", "", "", "M", "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
			var container2 = testDec.CusContainers.AddNew();
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			RunGeneralDeclarationGenerationTestCase(false, "M", "", "", "", "", "M", "M", "M", "M", "", "", "", "", "", "", "", "", "", "", "", "");
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "M", "M", "M", "M", "C", "M", "", "", "C", "C", "", "", "", "", "C", "C");
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			entryHeader.IsGoodsDeliveredReturns = true;
			RunGeneralDeclarationGenerationTestCase(false, "", "", "", "C", "", "C", "C", "C", "C", "C", "M", "", "", "M", "M", "", "", "", "", "C", "C");
		}

		public void TestGeneralDeclarationGenerationForPUPandQuoting()
		{
			SetUpQuestionsAndDeclaration();
			entryHeader.GetNatureForImportCMRReturns = CusEntryHeader.NatureTypesForImportCMR.Nature10;
			invoiceLine.AddInfo.ZA_PUP = "Y";
			RunGeneralDeclarationGenerationTestCase(false, "M", "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
			invoiceLine.AddInfo.ZA_WETQ = "Y";
			RunGeneralDeclarationGenerationTestCase(false, "M", "M", "", "", "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
			invoiceLine.AddInfo.ZA_WETQ = "";
			invoiceLine.AddInfo.ZA_LCTQ = "Y";
			RunGeneralDeclarationGenerationTestCase(false, "M", "M", "", "", "M", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");
		}

		void RunGeneralDeclarationGenerationTestCase(bool isWithdrawl, string q1, string q2, string q3, string q4, string q5,
	string q6, string q7, string q8, string q9, string q10, string q11, string q12, string q13, string q14, string q15,
	string q16, string q17, string q18, string q19, string q282, string q326)
		{
			RunGeneralDeclarationGenerationTestCase(isWithdrawl, q1, q2, q3, q4, q5, q6, q7, q8, q9, q10, q11, q12, q13, q14, q15,
				q16, q17, q18, q19, q282, q326, "");
		}

		void RunGeneralDeclarationGenerationTestCase(bool isWithdrawl, string q1, string q2, string q3, string q4, string q5,
			string q6, string q7, string q8, string q9, string q10, string q11, string q12, string q13, string q14, string q15,
			string q16, string q17, string q18, string q19, string q282, string q326, string q375)
		{
			var testGenerator = new CMRLodgementQuestionGenerator(testDec);
			testGenerator.GenerateQuestions(isWithdrawl);
			TestOneGeneralDeclaration(1, q1);
			TestOneGeneralDeclaration(2, q2);
			TestOneGeneralDeclaration(3, q3);
			TestOneGeneralDeclaration(4, q4);
			TestOneGeneralDeclaration(5, q5);
			TestOneGeneralDeclaration(6, q6);
			TestOneGeneralDeclaration(7, q7);
			TestOneGeneralDeclaration(8, q8);
			TestOneGeneralDeclaration(9, q9);
			TestOneGeneralDeclaration(10, q10);
			TestOneGeneralDeclaration(11, q11);
			TestOneGeneralDeclaration(12, q12);
			TestOneGeneralDeclaration(13, q13);
			TestOneGeneralDeclaration(14, q14);
			TestOneGeneralDeclaration(15, q15);
			TestOneGeneralDeclaration(16, q16);
			TestOneGeneralDeclaration(17, q17);
			TestOneGeneralDeclaration(18, q18);
			TestOneGeneralDeclaration(19, q19);
			TestOneGeneralDeclaration(282, q282);
			TestOneGeneralDeclaration(326, q326);
			TestOneGeneralDeclaration(375, q375);
		}

		void TestOneGeneralDeclaration(ZInt decNo, string expectedStatus)
		{
			entryHeader.ResetTotalsAndCachedValues();
			if (expectedStatus == "M")
			{
				Assert("Should have question " + decNo.ToString(), entryHeader.Questions.HasQuestionWithID(decNo));
				var cPDec = entryHeader.Questions.GetQuestionWithID(decNo);
				Assert("Qhestion " + decNo.ToString() + " should be mandatory", !cPDec.IsOptionalQuestion);
			}
			else if (expectedStatus == "C")
			{
				Assert("Should have question " + decNo.ToString(), entryHeader.Questions.HasQuestionWithID(decNo));
				var cPDec = entryHeader.Questions.GetQuestionWithID(decNo);
				Assert("Qhestion " + decNo.ToString() + " should be optional", cPDec.IsOptionalQuestion);
			}
			else
			{
				Assert("Should NOT have question " + decNo.ToString(), !entryHeader.Questions.HasQuestionWithID(decNo));
			}
		}

		void SetUpQuestionsAndDeclaration()
		{
			SetUpQuestionsAndDeclaration(true);
		}

		void SetUpQuestionsAndDeclaration(bool testMode)
		{
			Env.Registry.CMRTestMode = testMode;
			testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 1);
			testDec.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;

			entryHeader = Factory.New<CusEntryHeaderForTest>();

			testDec.CustomsEntryHeaders.Add(entryHeader);

			entryLine = entryHeader.MergedLines.AddNew();

			TaxOrFeeTestHelper.SetUp();
			var deminimus = UniversalReferenceHelper.GetDeminimus(new BusinessObjectFactory());
			entryLine.CL_CustomsValue = deminimus + 1;
			invoiceLine = testDec.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			for (int i = 1; i <= 22; i++)
			{
				var decNo = i < 20 ? i : (i == 20 ? 282 : (i == 21 ? 326 : (i == 22 ? 375 : 0)));
				var originalQuestion = Factory.New<CMRLodgementQuestion>();
				originalQuestion.CQ_LodgementQuestionIdentifier = decNo;
				originalQuestion.CQ_LodgementQuestionStartDate = new ZDateTime(2005, 1, 1);
			}
		}
		JobDeclaration testDec;
		CusEntryHeaderForTest entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceLine invoiceLine;

		sealed class CusEntryHeaderForTest : CusEntryHeader
		{
			public CusEntryHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString GetNatureForImportCMRReturns { get; set; } = string.Empty;

			public bool IsGoodsDeliveredReturns { get; set; }

			public bool IsARefundDueReturns { get; set; }

			public override ZString GetNatureForImportCMR() => GetNatureForImportCMRReturns;

			public override bool IsGoodsDelivered => IsGoodsDeliveredReturns;

			public override ZBool IsARefundDue => IsARefundDueReturns;
		}
	}
}

