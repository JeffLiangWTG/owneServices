using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementLineGroup))]
	sealed class CusStatementLineGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCalculateTotalAmountForCARMDailyNotice()
		{
			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_IsMonthlyStatement = false;
			dailyStatement.B2_StatementNumber = "DN-123456789RM2345-1";
			var lineGroup = dailyStatement.LineGroupCollection.AddNew();
			lineGroup.B10_ImporterCustomsID = "105759013RM000A";
			var line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;
			line.B3_ImporterCustomsID = "105759013RM000A";

			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Others, 3425.26m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.ExciseTax, 18743.54m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, 25848.45m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Duties, 135847.00m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.SIMA, -1150.2m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Interest, -123.2m);

			line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Other;
			line.B3_ImporterCustomsID = "105759013RM000A";

			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Others, 184.61m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.ExciseDuties, 0.35m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax, 76873.32m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Duties, -1973.12m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.SIMA, 823.42m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Interest, 456.2m);

			line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;
			line.B3_ImporterCustomsID = "105759013RM000B";

			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Others, 32.08m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.ExciseTax, 23.00m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax, 6871.33m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Duties, 254.64m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.SIMA, 3.04m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Interest, 56.2m);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				CombineAssertions(() =>
				{
					AssertEquals("TotalOthers", 3609.87m, lineGroup.TotalOthers);
					AssertEquals("TotalExcise", 18743.54m, lineGroup.TotalExcise);
					AssertEquals("TotalExciseDuties", 0.35m, lineGroup.TotalExciseDuties);
					AssertEquals("TotalGSTOrGSD", 102721.77m, lineGroup.TotalGSTOrGSD);
					AssertEquals("TotalDuties", 133873.88m, lineGroup.TotalDuties);
					AssertEquals("TotalSIMA", -326.78m, lineGroup.TotalSIMA);
					AssertEquals("TotalInterests", 333m, lineGroup.TotalInterests);
					AssertEquals("Total", 258955.63m, lineGroup.Total);
				});
			}
		}

		public void TestHumanReadableName()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP00";

			var statement = Factory.New<CusStatementHeader>();

			var group = statement.LineGroupCollection.AddNew();
			AssertEquals("Statement Line Group - ", group.HumanReadableName);

			group.B10_OH_Importer = ZGuid.Empty;
			group.B10_ImporterCustomsID = "002200RM0001";
			AssertEquals("Statement Line Group - 002200RM0001", group.HumanReadableName);

			group.B10_OH_Importer = importer.PK;
			AssertEquals("Statement Line Group - (IMP00)002200RM0001", group.HumanReadableName);
		}

		public void TestCalculatedProperties()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_IsMonthlyStatement = false;

			var lineGroup = dailyStatement.LineGroupCollection.AddNew();
			lineGroup.B10_OH_Importer = importer.PK;

			CombineAssertions(() =>
			{
				AssertEquals("B10_ImporterCode", importer.OH_Code, lineGroup.B10_ImporterCode);
				AssertEquals("B10_ImporterName", importer.OH_FullName, lineGroup.B10_ImporterName);
			});

			lineGroup.B10_OH_Importer = ZGuid.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("B10_ImporterCode", string.Empty, lineGroup.B10_ImporterCode);
				AssertEquals("B10_ImporterName", string.Empty, lineGroup.B10_ImporterName);
			});

			lineGroup.B10_ImporterCustomsID = "105759013RM000A";

			var line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;
			line.B3_ImporterCustomsID = "105759013RM000A";

			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.Others, 3425.26m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 18743.54m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalGSTAmount, 25848.45m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalDutyAmount, 135847.00m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalSIMAAmount, -1150.2m);

			line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;
			line.B3_ImporterCustomsID = "105759013RM000A";

			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.Others, 184.61m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 0.35m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalGSTAmount, 76873.32m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalDutyAmount, -1973.12m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalSIMAAmount, 823.42m);

			line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;
			line.B3_ImporterCustomsID = "105759013RM000B";

			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.Others, 32.08m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 23.00m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalGSTAmount, 6871.33m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalDutyAmount, 254.64m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalSIMAAmount, 3.04m);

			CombineAssertions(() =>
			{
				AssertEquals("TotalOthers", 3609.87m, lineGroup.TotalOthers);
				AssertEquals("TotalExcise", 18743.89m, lineGroup.TotalExcise);
				AssertEquals("TotalGSTOrGSD", 102721.77m, lineGroup.TotalGSTOrGSD);
				AssertEquals("TotalDuties", 133873.88m, lineGroup.TotalDuties);
				AssertEquals("TotalSIMA", -326.78m, lineGroup.TotalSIMA);
				AssertEquals("Total", 258622.63m, lineGroup.Total);
			});
		}

		public void TestStatementLinesRelatedData()
		{
			var header = Factory.New<CusStatementHeader>();

			var line1 = header.StatementLines.AddNew();
			line1.B3_ImporterCustomsID = "105759013RM000A";
			line1.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;

			var line1Charge = line1.Charges.AddNew();
			line1Charge.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Importer;
			line1Charge.B4_ChargeAmount = 1;

			var line2 = header.StatementLines.AddNew();
			line2.B3_ImporterCustomsID = "105759013RM000B";
			line2.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;

			var line2Charge = line2.Charges.AddNew();
			line2Charge.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Broker;
			line2Charge.B4_ChargeAmount = 10;

			var line3 = header.StatementLines.AddNew();
			line3.B3_ImporterCustomsID = "105759013RM000A";
			line3.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;

			var line3Charge = line3.Charges.AddNew();
			line3Charge.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Importer;
			line3Charge.B4_ChargeAmount = 100;

			var line4 = header.StatementLines.AddNew();
			line4.B3_ImporterCustomsID = "105759013RM000B";
			line4.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Other;

			var line4Charge = line4.Charges.AddNew();
			line4Charge.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Broker;
			line4Charge.B4_ChargeAmount = 5;

			var lineGroup1 = header.LineGroupCollection.AddNew();
			lineGroup1.B10_ImporterCustomsID = "105759013RM000B";

			AssertArrayEqualsByElements(new[] { line2, line4 }, lineGroup1.StatementLines);
			AssertEquals("PaymentMethod", ZString.Empty, lineGroup1.PaymentMethod);
			AssertEquals("TotalPayableByBroker", 10m, lineGroup1.TotalPayableByBrokerOnDailyStatement);
			AssertEquals("TotalPayableByImporter", ZDecimal.Zero, lineGroup1.TotalPayableByImporterOnDailyStatement);

			lineGroup1.B10_ImporterCustomsID = "105759013RM000A";
			AssertArrayEqualsByElements(new[] { line1, line3 }, lineGroup1.StatementLines);
			AssertEquals("PaymentMethod", PaymentPartyCodeDescriptionList.Codes.Importer, lineGroup1.PaymentMethod);
			AssertEquals("TotalPayableByBroker", ZDecimal.Zero, lineGroup1.TotalPayableByBrokerOnDailyStatement);
			AssertEquals("TotalPayableByImporter", 101m, lineGroup1.TotalPayableByImporterOnDailyStatement);

			line2.B3_ImporterCustomsID = "105759013RM000A";
			AssertArrayEqualsByElements(new[] { line1, line2, line3 }, lineGroup1.StatementLines);
			AssertEquals("PaymentMethod", "MULTIPLE", lineGroup1.PaymentMethod);
			AssertEquals("TotalPayableByBroker", 10m, lineGroup1.TotalPayableByBrokerOnDailyStatement);
			AssertEquals("TotalPayableByImporter", 101m, lineGroup1.TotalPayableByImporterOnDailyStatement);

			var header2 = Factory.New<CusStatementHeader>();

			line4.B3_B2 = header2.PK;
			line4.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;
			line4.B3_ImporterCustomsID = "105759013RM000A";

			line4Charge.B4_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Importer;
			line4Charge.B4_ChargeType = EntryChargeTypeList.Codes.TotalGSTDirectAmount;
			line4Charge.B4_ChargeAmount = 1000;

			lineGroup1.B10_B2 = header2.PK;
			AssertArrayEqualsByElements(new[] { line4 }, lineGroup1.StatementLines);
			AssertEquals("PaymentMethod", PaymentPartyCodeDescriptionList.Codes.GST, lineGroup1.PaymentMethod);
			AssertEquals("TotalPayableByBroker", ZDecimal.Zero, lineGroup1.TotalPayableByBrokerOnDailyStatement);
			AssertEquals("TotalPayableByImporter", 1000m, lineGroup1.TotalPayableByImporterOnDailyStatement);
		}

		public void TestUpdateImporter()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "#A1";
			org1.OH_FullName = "#A1 DESC";
			org1.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "105759013RM000A", Core.Constants.CountryCodes.Canada);
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "#A2";
			org2.OH_FullName = "#A2 DESC";
			org2.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "105759013RM000A", Core.Constants.CountryCodes.Canada);
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "#A3";
			org3.OH_FullName = "#A3 DESC";
			org3.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "105759013RM000B", Core.Constants.CountryCodes.Canada);
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_OH_Importer = org1.PK;
			dec1.ImporterOfRecordAddress.E2_OA_Address = org2.MainAddress.PK;
			dec1.JE_DeclarationReference = "BM00001A";
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_OH_Importer = org2.PK;
			dec2.JE_DeclarationReference = "BM00001B";
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_OH_Importer = org2.PK;
			dec3.JE_DeclarationReference = "BM00002B";
			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_OH_Importer = org2.PK;
			dec4.JE_DeclarationReference = "BM00003B";
			Factory.Save();

			var header = Factory.New<CusStatementHeader>();
			var line1 = header.StatementLines.AddNew();
			line1.B3_BrokerReference = "BM00001";
			line1.B3_ImporterCustomsID = "105759013RM000A";
			var line2 = header.StatementLines.AddNew();
			line2.B3_BrokerReference = "BM00002";
			line2.B3_ImporterCustomsID = "105759013RM000A";
			var line3 = header.StatementLines.AddNew();
			line3.B3_BrokerReference = "BM00003";
			line3.B3_ImporterCustomsID = "105759013RM000B";
			var lineGroup = header.LineGroupCollection.AddNew();
			lineGroup.B10_ImporterCustomsID = "105759013RM000A";
			AssertEquals(ZGuid.Empty, lineGroup.B10_OH_Importer);
			lineGroup.UpdateImporter();
			AssertEquals(org2.PK, lineGroup.B10_OH_Importer);
			lineGroup.B10_ImporterCustomsID = "105759013RM000B";
			AssertEquals(org2.PK, lineGroup.B10_OH_Importer);
			lineGroup.UpdateImporter();
			AssertEquals(org3.PK, lineGroup.B10_OH_Importer);
		}

		public void TestImporterSummaryProperties()
		{
			var header = Factory.New<CusStatementHeader>();

			var lineGroup = header.LineGroupCollection.AddNew();
			lineGroup.B10_ImporterCustomsID = "002200RM0001";

			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.PreviousMonthlyStatementTotal, 135.321m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.PaymentReceivedSinceLastMonthlyStatement, 5.00m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.Refund, 0m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.UnpaidBalanceForward, 232.44m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.ArrearsInterest, 90m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TransactionTotal, 0.5m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.OtherCharges, 33m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.InstalmentCurrentAmount, 34m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.InstalmentLastAmount, 38m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalCredits, 35m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.InterestAmount, 0m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalPaymentReceived, 18.687m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalPayableForBroker, 19687m);
			lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalPayableForImporter, 9687m);

			CombineAssertions(() =>
			{
				AssertEquals("PreviousMonthlyStatementTotal", lineGroup.B10_PreviousMonthlyStatementTotal, 135.32m);
				AssertEquals("PaymentReceivedSinceLastMonthlyStatement", lineGroup.B10_PaymentReceivedSinceLastMonthlyStatement, 5m);
				AssertEquals("Refund", lineGroup.B10_Refund, 0m);
				AssertEquals("UnpaidBalanceForward", lineGroup.B10_UnpaidBalanceForward, 232.44m);
				AssertEquals("ArrearsInterest", lineGroup.B10_ArrearsInterest, 90m);
				AssertEquals("TransactionTotal", lineGroup.B10_TransactionTotal, 0.5m);
				AssertEquals("OtherCharges", lineGroup.B10_OtherCharges, 33m);
				AssertEquals("TotalCredits", lineGroup.B10_TotalCredits, 35m);
				AssertEquals("InterestAmount", lineGroup.B10_InterestAmount, 0m);
				AssertEquals("InstalmentLastAmount", lineGroup.B10_GIPastTotal, 38m);
				AssertEquals("InstalmentCurrentAmount", lineGroup.B10_GICurrentTotal, 34m);
				AssertEquals("TotalPaymentReceived", lineGroup.B10_TotalPaymentReceived, 18.69m);
				AssertEquals("B10_TotalPayableForBrokerSoAStatement", lineGroup.B10_TotalPayableForBrokerSoAStatement, 19687m);
				AssertEquals("B10_TotalPayableForImporterSoAStatement", lineGroup.B10_TotalPayableForImporterSoAStatement, 9687m);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusStatementHeader>();
			return header.LineGroupCollection.AddNew();
		}
	}
}
