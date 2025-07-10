using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusReconEntry))]
	sealed class CusReconEntryTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var reconEntry = factory.New<CusReconEntry>();
			reconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = factory.NewWithValidTestData<OrgAddress>().PK;
			return reconEntry;
		}

		public void TestFirstSnapShot()
		{
			var cusReconEntry = Factory.New<CusReconEntry>();
			AssertEquals(0, cusReconEntry.CusReconSnapshots.Count);
			AssertNull(cusReconEntry.FirstSnapShot);

			var firstSnapShot = cusReconEntry.CusReconSnapshots.AddNew();
			AssertEquals(1, cusReconEntry.CusReconSnapshots.Count);
			AssertNotNull(cusReconEntry.FirstSnapShot);

			var secondSnapShot = cusReconEntry.CusReconSnapshots.AddNew();
			AssertEquals(2, cusReconEntry.CusReconSnapshots.Count);
			AssertEquals(firstSnapShot, cusReconEntry.FirstSnapShot);
			AssertNotEquals(secondSnapShot, cusReconEntry.FirstSnapShot);
		}

		public void TestCustomsBillNumber()
		{
			SetupViewData(out var branch, out var entry);

			var view = Factory.LoadTop1<KREntryCustomsBillsView>(new ZQuery(KREntryCustomsBillsViewSchema.KEB_CustomsDisbursementBillNumber, "0127012112100053179"));
			AssertEquals("0127012112100053179", view.KEB_CustomsDisbursementBillNumber);

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var reconEntry = (CusReconEntry)reconDeclaration.CusReconEntries.AddNew();
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			AssertEquals(0, reconEntry.CusReconSnapshots.Count);

			reconEntry.SaveSnapshotByCustomsBillNumber(view);
			AssertEquals(1, reconEntry.CusReconSnapshots.Count);

			AssertEquals("KEB_CustomsDisbursementBillNumber", "012112100053179", reconEntry.CRE_CustomsBillNumber);
			AssertEquals("KEB_ImportEntryNum", "1234567890M", reconEntry.CRE_OriginalEntryNumber);
			AssertEquals("KEB_ImportIssueDate", new ZDate(2025, 7, 30), reconEntry.CRE_EntryDate);
			AssertEquals("IMP", reconEntry.CRE_EntryType);
			AssertEquals("KEB_BranchPK", branch.PK, reconEntry.CRE_GB_Branch);

			var importEntryOrEntryLine = reconEntry.FirstSnapShot.ImportEntryOrEntryLine;

			AssertEquals("KEB_PaymentAuthorizationDate", new ZDateTime(2025, 6, 30), importEntryOrEntryLine.PaidDate);
			AssertEquals("KEB_CustomsFeesTotal", 55000m, importEntryOrEntryLine.PaidAmounts.TotalPaid);
			AssertEquals("KEB_Duty", 1000m, importEntryOrEntryLine.PaidAmounts.DutyAmount);
			AssertEquals("KEB_ValueAddedTax", 2000m, importEntryOrEntryLine.PaidAmounts.VATAmount);
			AssertEquals("KEB_LiquorTax", 3000m, importEntryOrEntryLine.PaidAmounts.LiquorTaxAmount);
			AssertEquals("KEB_AgricultureTax", 4000m, importEntryOrEntryLine.PaidAmounts.AgricultureTaxAmount);
			AssertEquals("KEB_SpecialConsumptionTax", 5000m, importEntryOrEntryLine.PaidAmounts.SpecialConsumptionTaxAmount);
			AssertEquals("KEB_TransportationTax", 6000m, importEntryOrEntryLine.PaidAmounts.TransportTaxAmount);
			AssertEquals("KEB_EducationTax", 7000m, importEntryOrEntryLine.PaidAmounts.EducationTaxAmount);
			AssertEquals("KEB_PenaltyAndInterest", 8000m, importEntryOrEntryLine.PaidAmounts.TotalPenalty);
			AssertEquals("KEB_LatePenalty", 9000m, importEntryOrEntryLine.PaidAmounts.LatePaymentPenalty);
			AssertEquals("KEB_ValueForVAT", 10000m, importEntryOrEntryLine.PaidAmounts.ValueForVAT);

			AssertExceptionThrown<NullReferenceException>("KREntryCustomsBillsView should not be null.", () => { reconEntry.SaveSnapshotByCustomsBillNumber(null); });
		}
		void SetupViewData(out GlbBranch branch, out CusEntryHeader entry)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			branch = company.Branches.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GC = company.PK;
			declaration.JE_GB = branch.PK;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;

			entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_EntryReleaseDate = new ZDateTime(2025, 7, 4);
			entry.EntryNumber = "1234567890M";
			entry.CusEntryNumber.CE_IssueDate = new ZDateTime(2025, 7, 30);

			var cusStatementHeader1 = Factory.New<CusStatementHeader>();
			cusStatementHeader1.B2_GC = company.PK;
			cusStatementHeader1.B2_PaymentAuthorizationDate = new ZDateTime(2025, 6, 30);
			cusStatementHeader1.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			var cusStatementLine1 = cusStatementHeader1.StatementLines.AddNew();
			cusStatementLine1.B3_EntryNum = entry.EntryNumber;
			cusStatementLine1.B3_CustomsFeesTotal = 55000m;
			cusStatementLine1.B3_AssociatedEntry = "0127012112100053179";

			AddCharge(cusStatementLine1, ChargeTypeList.Codes.Duty, 1000m);
			AddCharge(cusStatementLine1, ChargeTypeList.Codes.VAT, 2000m);
			AddCharge(cusStatementLine1, ChargeTypeList.Codes.LiquorTax, 3000m);
			AddCharge(cusStatementLine1, ChargeTypeList.Codes.AgricultureTax, 4000m);
			AddCharge(cusStatementLine1, ChargeTypeList.Codes.SpecialConsumptionTax, 5000m);
			AddCharge(cusStatementLine1, ChargeTypeList.Codes.TransportationTax, 6000m);
			AddCharge(cusStatementLine1, ChargeTypeList.Codes.EducationTax, 7000m);
			AddCharge(cusStatementLine1, ChargeTypeList.Codes.PenaltyAndInterest, 8000m);
			AddCharge(cusStatementLine1, ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, 9000m);
			AddCharge(cusStatementLine1, ChargeTypeList.Codes.ValueForVAT, 10000m);

			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_EntryReleaseDate = new ZDateTime(2025, 8, 4);
			entry2.EntryNumber = "1234567891M";
			entry2.CusEntryNumber.CE_IssueDate = new ZDateTime(2025, 8, 30);

			var cusStatementHeader2 = Factory.New<CusStatementHeader>();
			cusStatementHeader2.B2_GC = company.PK;
			cusStatementHeader2.B2_PaymentAuthorizationDate = new ZDateTime(2025, 7, 30);
			cusStatementHeader2.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			var cusStatementLine2 = cusStatementHeader2.StatementLines.AddNew();
			cusStatementLine2.B3_EntryNum = entry2.EntryNumber;
			cusStatementLine2.B3_CustomsFeesTotal = 56000m;
			cusStatementLine2.B3_AssociatedEntry = "0127012112100053180";

			AddCharge(cusStatementLine2, ChargeTypeList.Codes.Duty, 10000m);
			AddCharge(cusStatementLine2, ChargeTypeList.Codes.VAT, 20000m);
			AddCharge(cusStatementLine2, ChargeTypeList.Codes.LiquorTax, 30000m);
			AddCharge(cusStatementLine2, ChargeTypeList.Codes.AgricultureTax, 40000m);
			AddCharge(cusStatementLine2, ChargeTypeList.Codes.SpecialConsumptionTax, 50000m);
			AddCharge(cusStatementLine2, ChargeTypeList.Codes.TransportationTax, 60000m);
			AddCharge(cusStatementLine2, ChargeTypeList.Codes.EducationTax, 70000m);
			AddCharge(cusStatementLine2, ChargeTypeList.Codes.PenaltyAndInterest, 80000m);
			AddCharge(cusStatementLine2, ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, 90000m);
			AddCharge(cusStatementLine2, ChargeTypeList.Codes.ValueForVAT, 100000m);

			Factory.Save();
			void AddCharge(CusStatementLine line, string type, decimal amount)
			{
				var charge = line.Charges.AddNew();
				charge.B4_ChargeType = type;
				charge.B4_ChargeAmount = amount;
			}
		}

		public void TestSaveSnapshotByCustomsBillNumber()
		{
			SetupViewData(out var branch, out var entry);

			var view = Factory.LoadTop1<KREntryCustomsBillsView>(new ZQuery(KREntryCustomsBillsViewSchema.KEB_CustomsDisbursementBillNumber, "0127012112100053179"));
			var message5WN1 = entry.Messages.AddNew();
			message5WN1.EM_MessageType = "5WN";
			message5WN1.EM_ReceiveTransmit = "RCV";
			message5WN1.EM_MessageOwner = "1";
			var testMsgFile = new EmbeddedResourceRetriever().GetBytes(TestFilesPath + ".GOVCBR5WN_1.xml");
			message5WN1.EM_MessageData = testMsgFile;

			view.Messages5WN = [message5WN1];

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var reconEntry = (CusReconEntry)reconDeclaration.CusReconEntries.AddNew();
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			reconEntry.CRE_EntryDate = ZDate.Today;
			reconEntry.CRE_EntryType = "AA";
			reconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			reconEntry.SaveSnapshotByCustomsBillNumber(view);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var snapshot = newFactory.Load<CusReconSnapshot>(reconEntry.FirstSnapShot.PK);
			using var textReader = snapshot.GetCRS_SnapshotXmlReader();
			var importEntryOrEntryLine = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<ImportEntryOrEntryLineSerializable>(textReader);
			var importEntryOrEntryLineFromView = view.GetImportEntryOrEntryLineSerializable();

			AssertEquals(55000m, importEntryOrEntryLine.PaidAmounts.TotalPaid);
			AssertEquals(1000m, importEntryOrEntryLine.PaidAmounts.DutyAmount);
			AssertEquals(2000m, importEntryOrEntryLine.PaidAmounts.VATAmount);
			AssertEquals(3000m, importEntryOrEntryLine.PaidAmounts.LiquorTaxAmount);
			AssertEquals(4000m, importEntryOrEntryLine.PaidAmounts.AgricultureTaxAmount);
			AssertEquals(5000m, importEntryOrEntryLine.PaidAmounts.SpecialConsumptionTaxAmount);
			AssertEquals(6000m, importEntryOrEntryLine.PaidAmounts.TransportTaxAmount);
			AssertEquals(7000m, importEntryOrEntryLine.PaidAmounts.EducationTaxAmount);
			AssertEquals(8000m, importEntryOrEntryLine.PaidAmounts.TotalPenalty);
			AssertEquals(9000m, importEntryOrEntryLine.PaidAmounts.LatePaymentPenalty);
			AssertEquals(10000m, importEntryOrEntryLine.PaidAmounts.ValueForVAT);
			importEntryOrEntryLine.RefundAmounts[0].VersionDescription = "2024-01-01 (2)";
		}

		public void TestDefaultRefundValuesFromSnapshot()
		{
			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KR1", "TestCompany");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "1234525000001M";

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var reconEntry = (CusReconEntry)reconDeclaration.CusReconEntries.AddNew();
			reconEntry.CRE_EntryType = KRJobMessageTypeList.Codes.Import;
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			reconEntry.CRE_OriginalEntryNumber = entry.EntryNumber;
			reconEntry.CRE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var reconEntryLine = (CusReconEntryLine)reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_CRE = reconEntry.PK;
			reconEntryLine.CRL_LineNumber = 1;
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;

			var fileReader = new TestFileReader(typeof(CusReconEntryLineValidationTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "ImportEntryOrEntryLine.xml");
			var snapshot = reconEntry.CusReconSnapshots.AddNew();
			snapshot.CRS_SnapshotXml = messageText;
			Factory.Save();

			reconEntry.CRE_Amendment5WNVersionNumber = 1;

			AssertEquals("Duty To Refund is", 1m, reconEntryLine.DutyToRefund);
			AssertEquals("Duty To Refund is", 2m, reconEntryLine.LQTToRefund);
			AssertEquals("Duty To Refund is", 4m, reconEntryLine.SCTToRefund);
			AssertEquals("Duty To Refund is", 8m, reconEntryLine.TRTToRefund);
			AssertEquals("Duty To Refund is", 16m, reconEntryLine.EDTToRefund);
			AssertEquals("Duty To Refund is", 32m, reconEntryLine.AGTToRefund);
			AssertEquals("Duty To Refund is", 64m, reconEntryLine.VATToRefund);
			AssertEquals("Duty To Refund is", 512m, reconEntryLine.PenaltyLateDecToRefund);
			AssertEquals("Duty To Refund is", 1024m, reconEntryLine.PenaltyMissedDecToRefund);
			AssertEquals("Duty To Refund is", 2048m, reconEntryLine.PenaltyLatePaymentToRefund);
			AssertEquals("Duty To Refund is", 4096m, reconEntryLine.NonDutyTaxRevenueToRefund);

			reconEntry.CRE_Amendment5WNVersionNumber = 2;

			AssertEquals("Duty To Refund is", 10m, reconEntryLine.DutyToRefund);
			AssertEquals("Duty To Refund is", 20m, reconEntryLine.LQTToRefund);
			AssertEquals("Duty To Refund is", 40m, reconEntryLine.SCTToRefund);
			AssertEquals("Duty To Refund is", 80m, reconEntryLine.TRTToRefund);
			AssertEquals("Duty To Refund is", 160m, reconEntryLine.EDTToRefund);
			AssertEquals("Duty To Refund is", 320m, reconEntryLine.AGTToRefund);
			AssertEquals("Duty To Refund is", 640m, reconEntryLine.VATToRefund);
			AssertEquals("Duty To Refund is", 5120m, reconEntryLine.PenaltyLateDecToRefund);
			AssertEquals("Duty To Refund is", 10240m, reconEntryLine.PenaltyMissedDecToRefund);
			AssertEquals("Duty To Refund is", 20480m, reconEntryLine.PenaltyLatePaymentToRefund);
			AssertEquals("Duty To Refund is", 40960m, reconEntryLine.NonDutyTaxRevenueToRefund);
		}

		public void TestZPropertyInfoChange()
		{
			var cusReconEntry = Factory.New<CusReconEntry>();
			AssertEquals(0, cusReconEntry.CusReconSnapshots.Count);
			
			AssertEquals(false, cusReconEntry.CRE_EntryDateInfo.ReadOnly);
			AssertEquals(false, cusReconEntry.CRE_EntryTypeInfo.ReadOnly);
			AssertEquals(false, cusReconEntry.CRE_GB_BranchInfo.ReadOnly);
			AssertEquals(false, cusReconEntry.CRE_OriginalEntryNumberInfo.ReadOnly);
			AssertEquals(false, cusReconEntry.CRE_CustomsBillNumberInfo.ReadOnly);

			var firstSnapShot = cusReconEntry.CusReconSnapshots.AddNew();
			AssertEquals(1, cusReconEntry.CusReconSnapshots.Count);

			AssertEquals(true, cusReconEntry.CRE_OriginalEntryNumberInfo.ReadOnly);
			AssertEquals(true, cusReconEntry.CRE_CustomsBillNumberInfo.ReadOnly);
			AssertEquals(true, cusReconEntry.CRE_EntryDateInfo.ReadOnly);
			AssertEquals(true, cusReconEntry.CRE_EntryTypeInfo.ReadOnly);
			AssertEquals(true, cusReconEntry.CRE_GB_BranchInfo.ReadOnly);
		}

		public void TestSequenceNumber()
		{
			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var reconEntry1 = (CusReconEntry)reconDeclaration.CusReconEntries.AddNew();
			Assert(reconEntry1.CRE_SequenceNumberInfo.ReadOnly);
			AssertEquals(1u, reconEntry1.CRE_SequenceNumber);
			var reconEntry2 = (CusReconEntry)reconDeclaration.CusReconEntries.AddNew();
			AssertEquals(2u, reconEntry2.CRE_SequenceNumber);
			var reconEntry3 = (CusReconEntry)reconDeclaration.CusReconEntries.AddNew();
			AssertEquals(3u, reconEntry3.CRE_SequenceNumber);
			reconDeclaration.CusReconEntries.Delete(reconEntry2);
			AssertEquals(2u, reconEntry3.CRE_SequenceNumber);
			var reconEntry4 = (CusReconEntry)reconDeclaration.CusReconEntries.AddNew();
			AssertEquals(3u, reconEntry4.CRE_SequenceNumber);
			reconDeclaration.CRD_MessageStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			reconDeclaration.CusReconEntries.Delete(reconEntry3);
			AssertEquals(3u, reconEntry4.CRE_SequenceNumber);
			var reconEntry5 = (CusReconEntry)reconDeclaration.CusReconEntries.AddNew();
			AssertEquals(0u, reconEntry5.CRE_SequenceNumber);
		}

		public void TestCRE_OriginalEntryNumber()
		{
			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "KR1", "TestCompany");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.EntryNumber = "1234525000001M";

			var reconDeclaration = Factory.New<CusReconDeclaration>();
			var reconEntry = (CusReconEntry)reconDeclaration.CusReconEntries.AddNew();
			reconEntry.CRE_EntryType = KRJobMessageTypeList.Codes.Import;
			reconEntry.CRE_CH_OriginalEntry = entry.PK;
			reconEntry.CRE_OriginalEntryNumber = entry.EntryNumber;
			reconEntry.CRE_OA_DeclarantAddress = orgHeader.MainAddress.PK;
			var reconEntryLine = (CusReconEntryLine)reconEntry.CusReconEntryLines.AddNew();
			reconEntryLine.CRL_CRE = reconEntry.PK;
			reconEntryLine.CRL_LineNumber = 1;
			reconEntryLine.CRL_OriginalEntryLineNumber = 1;
			AssertEquals("CRE_OriginalEntryNumber is", "1", reconEntryLine.CRL_OriginalEntryLineNumber.ToString());

			reconEntry.CRE_OriginalEntryNumber = "1234567890M";
			AssertEquals("CRE_OriginalEntryNumber is", "1", reconEntryLine.CRL_OriginalEntryLineNumber.ToString());

			reconEntry.CRE_OriginalEntryNumber = ZString.Empty;
			AssertEquals("CRE_OriginalEntryNumber is", ZShort.Zero, reconEntryLine.CRL_OriginalEntryLineNumber);
		}

		public void TestAmendment5WNVersionNumber()
		{
			var cusReconEntry = Factory.New<CusReconEntry>();

			cusReconEntry.Amendment5WNVersionNumber = "1";
			AssertEquals(1u, cusReconEntry.CRE_Amendment5WNVersionNumber);

			cusReconEntry.Amendment5WNVersionNumber = "";
			AssertEquals(0u, cusReconEntry.CRE_Amendment5WNVersionNumber);
		}

		const string TestFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
