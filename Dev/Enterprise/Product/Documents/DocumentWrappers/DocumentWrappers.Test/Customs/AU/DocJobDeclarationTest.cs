using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.Business.JobMessageTypeList;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocDeclaration))]
	sealed class DocJobDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
	{
		#region Wrapper Fields

		public void TestJobInvoicingJob()
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_ParentID = Declaration.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			AssertNotNull("JobInvoicingJob", DeclarationWrapper.JobInvoicingJob);
		}

		public void TestEntryHeaderCollection()
		{
			AssertEquals("EntryHeader Collection type", typeof(DocCusEntryHeaderCollection), DeclarationWrapper.RateEntryHeaders.GetType());
		}

		public void TestWarehouseInvoiceLinesForNature20()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();

			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			JobComInvoiceLine line3 = declaration.FilteredInvoiceLines.AddNew();
			line3.JI_LineNo = 3;

			DocDeclaration docDeclaration = DocDeclaration.New(declaration, Factory);
			AssertEquals("Count", 0, docDeclaration.WarehouseInvoiceLines.Count);

			line1.JI_IsPackToBondForLine = true;
			line2.JI_IsPackToBondForLine = true;
			AssertEquals("Count", 2, docDeclaration.WarehouseInvoiceLines.Count);
			AssertEquals("Line1", line1, docDeclaration.WarehouseInvoiceLines[0].WrappedObject);
			AssertEquals("Line2", line2, docDeclaration.WarehouseInvoiceLines[1].WrappedObject);
		}

		public void TestWarehouseInvoiceLinesForNature30()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;

			JobComInvoiceLine line2 = declaration.FilteredInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			JobComInvoiceLine line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			JobComInvoiceLine line3 = declaration.FilteredInvoiceLines.AddNew();
			line3.JI_LineNo = 3;

			DocDeclaration docDeclaration = DocDeclaration.New(declaration, Factory);
			AssertEquals("Count", 3, docDeclaration.WarehouseInvoiceLines.Count);
			AssertEquals("Line1", line1, docDeclaration.WarehouseInvoiceLines[0].WrappedObject);
			AssertEquals("Line2", line2, docDeclaration.WarehouseInvoiceLines[1].WrappedObject);
			AssertEquals("Line3", line3, docDeclaration.WarehouseInvoiceLines[2].WrappedObject);
		}

		#endregion

		#region ZBool Fields

		public void TestForcePrimeEnclosure()
		{
			Declaration.JE_ForcePrimeEnclosure = ZBool.False;
			Assert("!ForcePrimeEnclosure", !DeclarationWrapper.ForcePrimeEnclosure);

			Declaration.JE_ForcePrimeEnclosure = ZBool.True;
			Assert("ForcePrimeEnclosure", DeclarationWrapper.ForcePrimeEnclosure);
		}

		public void TestNeedsConfirmation()
		{
			AssertEquals("NeedsConfirmation", Declaration.NeedsConfirmation, DeclarationWrapper.NeedsConfirmation);
		}

		#endregion

		#region ZString Fields

		public void TestOutstandingDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;

			StmALog log = declaration.OutstandingAmendmentLogManger.AddANewOutstandingAmendmentLog("TESTTEST", new ZDateTime(2005, 1, 1, 1, 1, 1));
			DocDeclaration docDeclaration = DocDeclaration.New(declaration, Factory);
			AssertEquals("(Amendment detected, NOT Lodged.) ", docDeclaration.OutstandingDescription);
			log.Cancel();

			StmALog log2 = declaration.OutstandingAmendmentLogManger.AddANewLogForSaveWithoutEntryChanges(new ZDateTime(2005, 1, 1, 1, 1, 1));
			docDeclaration = DocDeclaration.New(declaration, Factory);
			AssertEquals("(Saved without sending amendment, NOT Lodged.) ", docDeclaration.OutstandingDescription);
			log2.Cancel();

			StmALog log3 = declaration.Logs.AddNew(Events.ManualMatchDone, "Reply to CI Amendment Received", new ZDateTimeOffset(2005, 1, 1, 1, 1, 1));
			docDeclaration = DocDeclaration.New(declaration, Factory);
			AssertEquals("(Amended through CI, Details may not match ICS.)", docDeclaration.OutstandingDescription);
			log3.Cancel();

			StmALog log4 = declaration.Logs.AddNew(Events.DeclarationAmendmentRejected, "Amendment rejected, no changes lodged with Customs", new ZDateTimeOffset(2005, 1, 1, 1, 1, 1));
			docDeclaration = DocDeclaration.New(declaration, Factory);
			AssertEquals("(Amendment Failed, Details may not match ICS.)", docDeclaration.OutstandingDescription);
		}

		public void TestVoyageNo()
		{
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			Declaration.JE_VoyageFlightNo = "123";
			AssertEquals("MasterBills", "123", DeclarationWrapper.VoyageNo);

			Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.Default;
			Declaration.JE_VoyageFlightNo = "456";
			AssertEquals("MasterBills", "456", DeclarationWrapper.VoyageNo);

			Declaration.JE_VoyageFlightNo = "000";
			AssertEquals("MasterBills", ZString.Empty, DeclarationWrapper.VoyageNo);
		}

		public void TestMasterBills()
		{
			Declaration.JE_MasterBill = "123";
			AssertEquals("MasterBills", "123", DeclarationWrapper.MasterBills);
		}

		public void TestDeclarationReferenceOrHashesIfEmpty()
		{
			AssertEquals("DeclarationReferenceOrHashesIfEmpty", Declaration.DeclarationReferenceOrHashesIfEmpty, DeclarationWrapper.DeclarationReferenceOrHashesIfEmpty);
		}

		public void TestEFTPaymentAdviceDocumentType()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.PeriodDeclarationType1;
			AssertEquals("Doc type FID", "FID", DeclarationWrapper.EFTPaymentAdviceDocumentType);

			Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals("Doc type SAC", "SAC", DeclarationWrapper.EFTPaymentAdviceDocumentType);
		}

		#endregion

		#region AUCusEntryHeaderEntryPrintLines

		public void TestAUCusEntryHeaderEntryPrintLines()
		{
			SetupEntryPrintTestData();
			AssertEquals("Landscape entry print lines MUST be a multiple of 68 lines for the document", 68, DeclarationWrapper.AUCusEntryHeaderEntryPrintLines.Length);
		}

		public void TestAUCusEntryHeaderEntryPrintLinesPortrait()
		{
			SetupEntryPrintTestData();
			AssertEquals("Portrait entry print lines MUST be a multiple of 94 lines for the document", 98, DeclarationWrapper.AUCusEntryHeaderEntryPrintLinesPortrait.Length);
		}

		void SetupEntryPrintTestData()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DeclarationReference = "B00148999";

			JobComInvoiceHeader invoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_OH_Supplier = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			invoiceHeader.JZ_InvoiceAmount = 7221.26m;
			invoiceHeader.JZ_InvoiceCurrExRate = 1.000000000m;
			invoiceHeader.JZ_InvoiceNumber = "4X00092";

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 12.00000m;
			invoiceLine.JI_LinePrice = 128.1600m;

			Declaration.MessageInitiator = new Enterprise.Customs.AU.Declaration.Business.SendsMessagesToCustomsShutterUpperer(false);
			Declaration.JE_MergeBy = "NON";
			Declaration.DoMerge();
		}

		#endregion

		#region Summary Commercial Invoice
		public void TestTotalBuyingCommissionIncludedExcluded()
		{
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_ApplicationCode = "LEG";
			AssertTotalIncludedExcludedCharge(AUChargeCodeList.Codes.BuyingCommission, "TotalIncludedBuyingCommission", "TotalExcludedBuyingCommission");
		}

		public void TestTotalOtherCommissionIncludedExcluded()
		{
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_ApplicationCode = "LEG";
			AssertTotalIncludedExcludedCharge(AUChargeCodeList.Codes.OtherCommission, "TotalIncludedOtherCommission", "TotalExcludedOtherCommission");
		}
		#endregion

		#region TestAllInvoiceHeaders

		public void TestAllInvoiceHeaders_SortedByLineNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			for (short i = 0; i < 20; i += 1)
			{
				BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			}

			declaration.MessageInitiator = new Enterprise.Customs.AU.Declaration.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			DocDeclaration docDeclaration = DocDeclaration.New(declaration, Factory);
			for (short i = 0; i < 20; i++)
			{
				AssertEquals("Invoice lines should be presented in correct order", i + 1, docDeclaration.AllInvoiceLines[i].LineNo);
			}
		}

		#endregion

		public void TestIsDrawbackDeclarationMethodA()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsDrawbackDeclarationMethodA", "", DeclarationWrapper.IsDrawbackDeclarationMethodA);
			Declaration.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.ActualShipment;
			AssertEquals("IsDrawbackDeclarationMethodA", "Y", DeclarationWrapper.IsDrawbackDeclarationMethodA);
		}

		public void TestIsDrawbackDeclarationMethodB()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsDrawbackDeclarationMethodB", "", DeclarationWrapper.IsDrawbackDeclarationMethodB);
			Declaration.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			AssertEquals("IsDrawbackDeclarationMethodB", "Y", DeclarationWrapper.IsDrawbackDeclarationMethodB);
		}

		public void TestIsDrawbackDeclarationMethodC()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsDrawbackDeclarationMethodC", "", DeclarationWrapper.IsDrawbackDeclarationMethodC);
			Declaration.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.Imputation;
			AssertEquals("IsDrawbackDeclarationMethodC", "Y", DeclarationWrapper.IsDrawbackDeclarationMethodC);
		}

		public void TestIsDrawbackDeclarationMethodOther()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsDrawbackDeclarationMethodOther", "", DeclarationWrapper.IsDrawbackDeclarationMethodA);
			AssertEquals("IsDrawbackDeclarationMethodOther", "", DeclarationWrapper.IsDrawbackDeclarationMethodB);
			AssertEquals("IsDrawbackDeclarationMethodOther", "", DeclarationWrapper.IsDrawbackDeclarationMethodC);
			Declaration.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.OtherMethod;
			AssertEquals("IsDrawbackDeclarationMethodOther", "", DeclarationWrapper.IsDrawbackDeclarationMethodA);
			AssertEquals("IsDrawbackDeclarationMethodOther", "", DeclarationWrapper.IsDrawbackDeclarationMethodB);
			AssertEquals("IsDrawbackDeclarationMethodOther", "", DeclarationWrapper.IsDrawbackDeclarationMethodC);
		}

		public void TestIsDrawbackDeclarationAmberReasonC()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsDrawbackDeclarationAmberReasonC", "", DeclarationWrapper.IsDrawbackDeclarationAmberReasonC);
			Declaration.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.Calculation;
			AssertEquals("IsDrawbackDeclarationAmberReasonC", "Y", DeclarationWrapper.IsDrawbackDeclarationAmberReasonC);
		}

		public void TestIsDrawbackDeclarationAmberReasonD()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsDrawbackDeclarationAmberReasonD", "", DeclarationWrapper.IsDrawbackDeclarationAmberReasonD);
			Declaration.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.Declaration;
			AssertEquals("IsDrawbackDeclarationAmberReasonD", "Y", DeclarationWrapper.IsDrawbackDeclarationAmberReasonD);
		}

		public void TestIsDrawbackDeclarationAmberReasonT()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsDrawbackDeclarationAmberReasonT", "", DeclarationWrapper.IsDrawbackDeclarationAmberReasonT);
			Declaration.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.Time;
			AssertEquals("IsDrawbackDeclarationAmberReasonT", "Y", DeclarationWrapper.IsDrawbackDeclarationAmberReasonT);
		}

		public void TestIsDrawbackDeclarationAmberReasonOther()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsDrawbackDeclarationAmberReasonOther", "", DeclarationWrapper.IsDrawbackDeclarationAmberReasonC);
			AssertEquals("IsDrawbackDeclarationAmberReasonOther", "", DeclarationWrapper.IsDrawbackDeclarationAmberReasonD);
			AssertEquals("IsDrawbackDeclarationAmberReasonOther", "", DeclarationWrapper.IsDrawbackDeclarationAmberReasonT);
			Declaration.AddInfo.ZA_DARC_Hidden = JobDeclaration.DrawbackAmberReasonTypes.LegacyMigration;
			AssertEquals("IsDrawbackDeclarationAmberReasonOther", "", DeclarationWrapper.IsDrawbackDeclarationAmberReasonC);
			AssertEquals("IsDrawbackDeclarationAmberReasonOther", "", DeclarationWrapper.IsDrawbackDeclarationAmberReasonD);
			AssertEquals("IsDrawbackDeclarationAmberReasonOther", "", DeclarationWrapper.IsDrawbackDeclarationAmberReasonT);
		}

		public void TestAmberReasonStatement()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("AmberReasonStatement", "", DeclarationWrapper.AmberReasonStatement);
			Declaration.JE_AmberStatement = "Amber Reason";
			AssertEquals("AmberReasonStatement", "Amber Reason", DeclarationWrapper.AmberReasonStatement);
		}

		public void TestDrawbackCliamContactName()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			GlbStaff.CurrentUser.GS_FullName = "Baldric";
			AssertEquals("DrawbackCliamContactName", "Baldric", DeclarationWrapper.DrawbackCliamContactName);
		}

		public void TestDrawbackContactPhoneNumber()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			GlbStaff.CurrentUser.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			GlbStaff.CurrentUser.GS_WorkPhone = "0255559999";
			AssertEquals("DrawbackContactPhoneNumber", "61255559999", DeclarationWrapper.DrawbackContactPhoneNumber);
		}

		public void TestDrawbackContactEMail()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("DrawbackContactEMail", "", DeclarationWrapper.DrawbackContactEMail);
			GlbStaff.CurrentUser.GS_EmailAddress = "fred@nerk.com.au";
			AssertEquals("DrawbackContactEMail", "fred@nerk.com.au", DeclarationWrapper.DrawbackContactEMail);
		}

		public void TestDrawbackClaimAmounts()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.ActualShipment;
			line1.AddInfo.ZA_DDT_Hidden = 1M;
			JobComInvoiceLine line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line2.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment;
			line2.AddInfo.ZA_DDT_Hidden = 2M;
			JobComInvoiceLine line3 = invoiceHeader.JobComInvoiceLines.AddNew();
			line3.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.Imputation;
			line3.AddInfo.ZA_DDT_Hidden = 3M;
			JobComInvoiceLine line4 = invoiceHeader.JobComInvoiceLines.AddNew();
			line4.AddInfo.ZA_DAM_Hidden = JobDeclaration.DrawbackAssessmentMethods.OtherMethod;
			line4.AddInfo.ZA_DDT_Hidden = 4M;
			AssertEquals("TotalDrawbackClaimAmount", 10M, DeclarationWrapper.TotalDrawbackClaimAmount);
			AssertEquals("TotalDrawbackMethodAAmount", 1M, DeclarationWrapper.TotalDrawbackMethodAAmount);
			AssertEquals("TotalDrawbackMethodBAmount", 2M, DeclarationWrapper.TotalDrawbackMethodBAmount);
			AssertEquals("TotalDrawbackMethodCAmount", 3M, DeclarationWrapper.TotalDrawbackMethodCAmount);
		}

		public void TestBranchID()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("BranchID", Enterprise.Environment.Env.Registry.AUCustoms.LocalCustomsBranchIdentifier, DeclarationWrapper.BranchID);
		}

		public void TestIsPaymentPartyBroker()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsPaymentPartyBroker", "", DeclarationWrapper.IsPaymentPartyBroker);
			Declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Broker;
			AssertEquals("IsPaymentPartyBroker", "Y", DeclarationWrapper.IsPaymentPartyBroker);
		}

		public void TestIsPaymentPartyDrawbackClaimant()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsPaymentPartyDrawbackClaimant", "", DeclarationWrapper.IsPaymentPartyDrawbackClaimant);
			Declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.DrawbackClaimant;
			AssertEquals("IsPaymentPartyDrawbackClaimant", "Y", DeclarationWrapper.IsPaymentPartyDrawbackClaimant);
		}

		public void TestIsPaymentPartyOther()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsPaymentPartyOther", "", DeclarationWrapper.IsPaymentPartyBroker);
			AssertEquals("IsPaymentPartyOther", "", DeclarationWrapper.IsPaymentPartyDrawbackClaimant);
			Declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Cash;
			AssertEquals("IsPaymentPartyOther", "", DeclarationWrapper.IsPaymentPartyBroker);
			AssertEquals("IsPaymentPartyOther", "", DeclarationWrapper.IsPaymentPartyDrawbackClaimant);
		}

		public void TestIsDrawbackQIDs()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			AssertEquals("IsDrawbackQID283Yes", "", DeclarationWrapper.IsDrawbackQID283Yes);
			AssertEquals("IsDrawbackQID283No", "", DeclarationWrapper.IsDrawbackQID283No);
			AssertEquals("IsDrawbackQID284Yes", "", DeclarationWrapper.IsDrawbackQID284Yes);
			AssertEquals("IsDrawbackQID284No", "", DeclarationWrapper.IsDrawbackQID284No);
			AssertEquals("IsDrawbackQID285Yes", "", DeclarationWrapper.IsDrawbackQID285Yes);
			AssertEquals("IsDrawbackQID285No", "", DeclarationWrapper.IsDrawbackQID285No);
			AssertEquals("IsDrawbackQID286Yes", "", DeclarationWrapper.IsDrawbackQID286Yes);
			AssertEquals("IsDrawbackQID286No", "", DeclarationWrapper.IsDrawbackQID286No);
			AssertEquals("IsDrawbackQID287Yes", "", DeclarationWrapper.IsDrawbackQID287Yes);
			AssertEquals("IsDrawbackQID287No", "", DeclarationWrapper.IsDrawbackQID287No);
			AssertEquals("IsDrawbackQID999", "", DeclarationWrapper.IsDrawbackQID999);

			CMRCusEntryCPDec cPDec283 = Declaration.DrawbackQuestions.AddNew();
			cPDec283.ON_CPDecNum = 283;
			cPDec283.ON_AnswerCode = "Y";
			AssertEquals("IsDrawbackQID283Yes", "Y", DeclarationWrapper.IsDrawbackQID283Yes);
			AssertEquals("IsDrawbackQID283No", "", DeclarationWrapper.IsDrawbackQID283No);
			cPDec283.ON_AnswerCode = "N";
			AssertEquals("IsDrawbackQID283Yes", "", DeclarationWrapper.IsDrawbackQID283Yes);
			AssertEquals("IsDrawbackQID283No", "Y", DeclarationWrapper.IsDrawbackQID283No);
			cPDec283.ON_AnswerCode = "";
			AssertEquals("IsDrawbackQID283Yes", "", DeclarationWrapper.IsDrawbackQID283Yes);
			AssertEquals("IsDrawbackQID283No", "", DeclarationWrapper.IsDrawbackQID283No);

			CMRCusEntryCPDec cPDec284 = Declaration.DrawbackQuestions.AddNew();
			cPDec284.ON_CPDecNum = 284;
			cPDec284.ON_AnswerCode = "Y";
			AssertEquals("IsDrawbackQID284Yes", "Y", DeclarationWrapper.IsDrawbackQID284Yes);
			AssertEquals("IsDrawbackQID284No", "", DeclarationWrapper.IsDrawbackQID284No);
			cPDec284.ON_AnswerCode = "N";
			AssertEquals("IsDrawbackQID284Yes", "", DeclarationWrapper.IsDrawbackQID284Yes);
			AssertEquals("IsDrawbackQID284No", "Y", DeclarationWrapper.IsDrawbackQID284No);
			cPDec284.ON_AnswerCode = "";
			AssertEquals("IsDrawbackQID284Yes", "", DeclarationWrapper.IsDrawbackQID284Yes);
			AssertEquals("IsDrawbackQID284No", "", DeclarationWrapper.IsDrawbackQID284No);

			CMRCusEntryCPDec cPDec285 = Declaration.DrawbackQuestions.AddNew();
			cPDec285.ON_CPDecNum = 285;
			cPDec285.ON_AnswerCode = "Y";
			AssertEquals("IsDrawbackQID285Yes", "Y", DeclarationWrapper.IsDrawbackQID285Yes);
			AssertEquals("IsDrawbackQID285No", "", DeclarationWrapper.IsDrawbackQID285No);
			cPDec285.ON_AnswerCode = "N";
			AssertEquals("IsDrawbackQID285Yes", "", DeclarationWrapper.IsDrawbackQID285Yes);
			AssertEquals("IsDrawbackQID285No", "Y", DeclarationWrapper.IsDrawbackQID285No);
			cPDec285.ON_AnswerCode = "";
			AssertEquals("IsDrawbackQID285Yes", "", DeclarationWrapper.IsDrawbackQID285Yes);
			AssertEquals("IsDrawbackQID285No", "", DeclarationWrapper.IsDrawbackQID285No);

			CMRCusEntryCPDec cPDec286 = Declaration.DrawbackQuestions.AddNew();
			cPDec286.ON_CPDecNum = 286;
			cPDec286.ON_AnswerCode = "Y";
			AssertEquals("IsDrawbackQID286Yes", "Y", DeclarationWrapper.IsDrawbackQID286Yes);
			AssertEquals("IsDrawbackQID286No", "", DeclarationWrapper.IsDrawbackQID286No);
			cPDec286.ON_AnswerCode = "N";
			AssertEquals("IsDrawbackQID286Yes", "", DeclarationWrapper.IsDrawbackQID286Yes);
			AssertEquals("IsDrawbackQID286No", "Y", DeclarationWrapper.IsDrawbackQID286No);
			cPDec286.ON_AnswerCode = "";
			AssertEquals("IsDrawbackQID286Yes", "", DeclarationWrapper.IsDrawbackQID286Yes);
			AssertEquals("IsDrawbackQID286No", "", DeclarationWrapper.IsDrawbackQID286No);

			CMRCusEntryCPDec cPDec287 = Declaration.DrawbackQuestions.AddNew();
			cPDec287.ON_CPDecNum = 287;
			cPDec287.ON_AnswerCode = "Y";
			AssertEquals("IsDrawbackQID287Yes", "Y", DeclarationWrapper.IsDrawbackQID287Yes);
			AssertEquals("IsDrawbackQID287No", "", DeclarationWrapper.IsDrawbackQID287No);
			cPDec287.ON_AnswerCode = "N";
			AssertEquals("IsDrawbackQID287Yes", "", DeclarationWrapper.IsDrawbackQID287Yes);
			AssertEquals("IsDrawbackQID287No", "Y", DeclarationWrapper.IsDrawbackQID287No);
			cPDec287.ON_AnswerCode = "";
			AssertEquals("IsDrawbackQID287Yes", "", DeclarationWrapper.IsDrawbackQID287Yes);
			AssertEquals("IsDrawbackQID287No", "", DeclarationWrapper.IsDrawbackQID287No);

			CMRCusEntryCPDec cPDec999 = Declaration.DrawbackQuestions.AddNew();
			cPDec999.ON_CPDecNum = 999;
			cPDec999.ON_AnswerCode = "Y";
			AssertEquals("IsDrawbackQID999", "Y", DeclarationWrapper.IsDrawbackQID999);
			cPDec999.ON_AnswerCode = "N";
			AssertEquals("IsDrawbackQID999", "", DeclarationWrapper.IsDrawbackQID999);
			cPDec999.ON_AnswerCode = "";
			AssertEquals("IsDrawbackQID999", "", DeclarationWrapper.IsDrawbackQID999);
		}

		#region Implementation

		protected override void SetupDeclarationForDeclarationNumberTests(ZString declarationNumber)
		{
			Declaration.DeclarationNumber = declarationNumber;
		}

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.Australia; }
		}

		#endregion
	}
}
