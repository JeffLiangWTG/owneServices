using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CA.Business.CARMStatementOfAccountStatementTypeList;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementHeader))]
	sealed class CusStatementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsCARMSOAAndStatementTypeDescription()
		{
			var header = Factory.New<CusStatementHeader>();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			{
				header.B2_StatementType = ShortCodes.LegalEntiry;
				Assert("IsCARMSOA", header.IsCARMSOA);
				AssertEquals(CARMStatementOfAccountStatementTypeList.Codes.LegalEntiry, header.ConvertedStatementType);
				header.B2_StatementType = ShortCodes.ProgramType;
				AssertEquals(CARMStatementOfAccountStatementTypeList.Codes.ProgramType, header.ConvertedStatementType);
				header.B2_StatementType = ShortCodes.ProgramAccount;
				AssertEquals(CARMStatementOfAccountStatementTypeList.Codes.ProgramAccount, header.ConvertedStatementType);
				header.B2_StatementType = "X";
				AssertEquals(ZString.Empty, header.ConvertedStatementType);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, false))
			{
				header.B2_StatementType = ShortCodes.LegalEntiry;
				Assert("Is Not CARMSOA", !header.IsCARMSOA);
				AssertEquals(ZString.Empty, header.ConvertedStatementType);
			}
		}

		public void TestCARMSOASummary()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = ShortCodes.LegalEntiry;
			header.B2_ImporterCustomsID = "123456";
			var cusStatementLineGroup = StatementMessageProcessorHelper.CreateLineGroupIfNeed(header, "123456");
			cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.PreviousStatementBalance, 111.12m);
			cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CorrectionsToPreviousStatementBalance, 112.23m);
			cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.PaymentsReceivedAfterPreviousSoA, 211.32m);
			cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.Disbursements, 30.12m);
			cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.InterestAndPenaltiesSumTotal, 4.5m);
			cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentPeriodCharges, 1.09m);
			cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentPeriodCredits, 400m);
			cusStatementLineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(CARMSOACusStatementLineGroupFinancialDetailTypeList.Codes.CurrentStatementBalance, 1000.09m);

			var cusStatementLineGroupDIST = StatementMessageProcessorHelper.CreateLineGroupIfNeed(header, "123456_DIST");
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Duties, 1.05m);
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.ExciseTax, 23.11m);
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.ExciseDuties, 39.1m);
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.SIMA, 3.4m);
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, 112.34m);
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax, 12m);
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax, 9.9m);
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Interest, 90.9m);
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Penalties, 123m);
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Payments, 13.67m);
			cusStatementLineGroupDIST.FinancialDetailCollection.UpdateFinancialDetailFor(CARMDailyNoticeChargeTypeList.Codes.Others, 98.1m);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Previous Statement Balance", 111.12m, header.PreviousStatementBalance);
					AssertEquals("Corrections Last Balance", 112.23m, header.CorrectionsLastBalance);
					AssertEquals("Payments After Last SOA", 211.32m, header.PaymentsAfterLastSOA);
					AssertEquals("Disbursements", 30.12m, header.Disbursements);
					AssertEquals("Interest Sum", 4.5m, header.InterestSum);
					AssertEquals("Debits", 1.09m, header.CurrentPeriodCharges);
					AssertEquals("Credits", 400m, header.CurrentPeriodCredits);
					AssertEquals("Total", 1000.09m, header.Total);
					AssertEquals("Duties", 1.05m, header.Duties);
					AssertEquals("Excise", 23.11m, header.Excise);
					AssertEquals("Excise Duties", 39.1m, header.ExciseDuties);
					AssertEquals("SIMA", 3.4m, header.SIMA);
					AssertEquals("GST", 112.34m, header.GST);
					AssertEquals("HST", 12m, header.HST);
					AssertEquals("PST", 9.9m, header.PST);
					AssertEquals("Interest", 90.9m, header.Interest);
					AssertEquals("Penalties", 123m, header.Penalties);
					AssertEquals("Payments", 13.67m, header.Payments);
					AssertEquals("Others", 98.1m, header.Others);
				});
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadHeader = newFactory.Load<CusStatementHeader>(header.PK);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, false))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Previous Statement Balance", 0m, reloadHeader.PreviousStatementBalance);
					AssertEquals("Corrections Last Balance", 0m, reloadHeader.CorrectionsLastBalance);
					AssertEquals("Payments After Last SOA", 0m, reloadHeader.PaymentsAfterLastSOA);
					AssertEquals("Disbursements", 0m, reloadHeader.Disbursements);
					AssertEquals("Interest Sum", 0m, reloadHeader.InterestSum);
					AssertEquals("Debits", 0m, reloadHeader.CurrentPeriodCharges);
					AssertEquals("Credits", 0m, reloadHeader.CurrentPeriodCredits);
					AssertEquals("Total", 0m, reloadHeader.Total);
					AssertEquals("Duties", 0m, reloadHeader.Duties);
					AssertEquals("Excise", 0m, reloadHeader.Excise);
					AssertEquals("Excise Duties", 0m, reloadHeader.ExciseDuties);
					AssertEquals("SIMA", 0m, reloadHeader.SIMA);
					AssertEquals("GST", 0m, reloadHeader.GST);
					AssertEquals("HST", 0m, reloadHeader.HST);
					AssertEquals("PST", 0m, reloadHeader.PST);
					AssertEquals("Interest", 0m, reloadHeader.Interest);
					AssertEquals("Penalties", 0m, reloadHeader.Penalties);
					AssertEquals("Payments", 0m, reloadHeader.Payments);
					AssertEquals("Others", 0m, reloadHeader.Others);
				});
			}
		}

		public void TestIsCARMDailyNotice()
		{
			var header = Factory.New<CusStatementHeader>();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				header.B2_StatementNumber = "123";
				Assert("Enable CAD Message should be true", !header.IsCARMDailyNotice);
				header.B2_StatementNumber = "DN-1";
				Assert("Enable CAD Message should be true", !header.IsCARMDailyNotice);
				header.B2_StatementNumber = "DN-123456789-1";
				Assert("Enable CAD Message should be true", !header.IsCARMDailyNotice);
				header.B2_StatementNumber = "DN-123456789RM2345-1";
				Assert("Enable CAD Message should be true", !header.IsCARMDailyNotice);
				header.B2_StatementNumber = "DN-123456789012345-1";
				Assert("Enable CAD Message should be true", !header.IsCARMDailyNotice);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				header.B2_StatementNumber = "123";
				Assert("The format should start with 'DN-BN9(BN15)-'", !header.IsCARMDailyNotice);
				header.B2_StatementNumber = "DN-1";
				Assert("The format should start with 'DN-BN9(BN15)-'", !header.IsCARMDailyNotice);
				header.B2_StatementNumber = "DN-123456789-1";
				Assert("The format should start with 'DN-BN9(BN15)-'", header.IsCARMDailyNotice);
				header.B2_StatementNumber = "DN-123456789RM2345-1";
				Assert("The format should start with 'DN-BN9(BN15)-'", header.IsCARMDailyNotice);
				header.B2_StatementNumber = "DN-123456789012345-1";
				Assert("The format should start with 'DN-BN9(BN15)-'", !header.IsCARMDailyNotice);
			}
		}

		public void TestResetCSFRSFData()
		{
			var rsf = Factory.New<CusStatementHeader>();
			rsf.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			ZDecimal debitAmount = 1.1m;
			ZDecimal creditAmount = 2.2m;
			ZDecimal interimAmount = 3.3m;
			foreach (CSARSFPayment payment in rsf.Debits)
			{
				payment.Amount = debitAmount;
			}
			foreach (CSARSFPayment payment in rsf.Credits)
			{
				payment.Amount = creditAmount;
			}
			foreach (CSARSFPayment payment in rsf.InterimPayments)
			{
				payment.Amount = interimAmount;
			}

			var assessment01 = rsf.CustomsAssessments.AddNew();
			assessment01.Type = "T01";
			assessment01.Amount = 4.4m;
			assessment01.PortCode = "P01";
			assessment01.ReferenceNumber = "R01";
			var assessment02 = rsf.CustomsAssessments.AddNew();
			assessment02.Type = "T02";
			assessment02.Amount = 5.5m;
			assessment02.PortCode = "P02";
			assessment02.ReferenceNumber = "R02";

			var statementLine01 = rsf.StatementLines.AddNew();
			statementLine01.B3_EntryType = JobMessageTypeList.Codes.Import;
			var statementLine02 = rsf.StatementLines.AddNew();
			statementLine02.B3_EntryType = JobMessageTypeList.Codes.XTypeEntry;

			rsf.ResetCSFRSFData();

			CombineAssertions(() =>
			{
				foreach (CSARSFPayment debit in rsf.Debits)
				{
					if (debit.IsCalculatedAutomatically())
					{
						AssertEquals(ZDecimal.Zero, debit.Amount);
					}
					else
					{
						AssertEquals(debitAmount, debit.Amount);
					}
				}
				foreach (CSARSFPayment credit in rsf.Credits)
				{
					if (credit.IsCalculatedAutomatically())
					{
						AssertEquals(ZDecimal.Zero, credit.Amount);
					}
					else
					{
						AssertEquals(creditAmount, credit.Amount);
					}
				}
				foreach (CSARSFPayment interim in rsf.InterimPayments)
				{
					if (interim.IsCalculatedAutomatically())
					{
						AssertEquals(ZDecimal.Zero, interim.Amount);
					}
					else
					{
						AssertEquals(interimAmount, interim.Amount);
					}
				}
				var assess01 = rsf.CustomsAssessments.Cast<CSARSFAssessment>().FirstOrDefault(x => x.Type == "T01");
				var assess02 = rsf.CustomsAssessments.Cast<CSARSFAssessment>().FirstOrDefault(x => x.Type == "T02");
				AssertNotNull(assess01);
				AssertNotNull(assess02);
				AssertEquals(4.4m, assess01.Amount);
				AssertEquals("P01", assess01.PortCode);
				AssertEquals("R01", assess01.ReferenceNumber);
				AssertEquals(5.5m, assess02.Amount);
				AssertEquals("P02", assess02.PortCode);
				AssertEquals("R02", assess02.ReferenceNumber);

				AssertEquals(0, rsf.CSARSFTransactions.Count);
			});
		}

		public void TestDefaultPeriodStartDate()
		{
			var date = new ZDate(2020, 01, 31);
			csaRSF.B2_PeriodEndDate = date;
			AssertEquals(new ZDate(2020, 01, 01), csaRSF.B2_PeriodStartDate);

			date = new ZDate(2020, 01, 18);
			csaRSF.B2_PeriodEndDate = date;
			AssertEquals(new ZDate(2019, 12, 19), csaRSF.B2_PeriodStartDate);

			date = new ZDate(2020, 02, 18);
			csaRSF.B2_PeriodEndDate = date;
			statementHeader.B2_PeriodEndDate = date;
			AssertEquals(ZDate.Empty, statementHeader.B2_PeriodStartDate);
			AssertEquals(1, csaRSF.B2_PeriodStartDate.Month);
			AssertEquals(2020, csaRSF.B2_PeriodStartDate.Year);

			date = new ZDate(2020, 02, 29);
			csaRSF.B2_PeriodEndDate = date;
			statementHeader.B2_PeriodEndDate = date;
			AssertEquals(ZDate.Empty, statementHeader.B2_PeriodStartDate);
			AssertEquals(2, csaRSF.B2_PeriodStartDate.Month);
			AssertEquals(2020, csaRSF.B2_PeriodStartDate.Year);
		}

		public void TestIsValidEndDateForRSF()
		{
			var date = new ZDate(2020, 01, 18);
			Assert(csaRSF.IsValidEndDateForRSF(date));

			date = new ZDate(2020, 01, 31);
			Assert(csaRSF.IsValidEndDateForRSF(date));

			date = new ZDate(2020, 02, 29);
			Assert(csaRSF.IsValidEndDateForRSF(date));

			date = new ZDate(2020, 02, 19);
			Assert(!csaRSF.IsValidEndDateForRSF(date));

			date = new ZDate(2020, 07, 18);
			Assert(csaRSF.IsValidEndDateForRSF(date));
		}

		public void TestSetB2_OH_ImporterWhenRSF()
		{
			AssertEquals(ZString.Empty, csaRSF.B2_StatementNumber);

			csaRSF.B2_PeriodEndDate = new ZDate(2020, 09, 18);
			AssertEquals(ZString.Empty, csaRSF.B2_StatementNumber);

			var org = Factory.New<OrgHeader>();
			org.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "TESTBN123456");
			csaRSF.B2_OH_Importer = org.PK;
			AssertEquals("TESTBN123456202009", csaRSF.B2_StatementNumber);
			AssertEquals("TESTBN123456", csaRSF.B2_ImporterCustomsID);
		}

		public void TestSetB2_ImporterCustomsIDWhenRSF()
		{
			AssertEquals(ZString.Empty, csaRSF.B2_StatementNumber);

			var org = Factory.New<OrgHeader>();
			csaRSF.B2_OH_Importer = org.PK;
			csaRSF.B2_PeriodEndDate = new ZDate(2020, 09, 18);
			AssertEquals(ZString.Empty, csaRSF.B2_StatementNumber);

			org.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "TESTBN123456");
			csaRSF.B2_OH_Importer = org.PK;
			AssertEquals("TESTBN123456202009", csaRSF.B2_StatementNumber);
		}

		public void TestDefaultStatementNumber()
		{
			AssertEquals(ZString.Empty, csaRSF.B2_StatementNumber);

			var org = Factory.New<OrgHeader>();
			csaRSF.B2_OH_Importer = org.PK;
			AssertEquals(ZString.Empty, csaRSF.B2_StatementNumber);

			org.SetCustomsCode(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "TESTBN123456");
			csaRSF.B2_OH_Importer = org.PK;
			AssertEquals(ZString.Empty, csaRSF.B2_StatementNumber);

			csaRSF.B2_PeriodEndDate = new ZDate(2020, 09, 17);
			AssertEquals(ZString.Empty, csaRSF.B2_StatementNumber);

			csaRSF.B2_PeriodEndDate = new ZDate(2020, 09, 18);
			AssertEquals("TESTBN123456202009", csaRSF.B2_StatementNumber);
		}

		public void TestIsCSARSF()
		{
			Assert(!statementHeader.IsCSARSF);
			Assert(csaRSF.IsCSARSF);
		}

		public void TestDefaultPeriodYearAndPeriodMonth()
		{
			var statement = Factory.New<CusStatementHeader>();
			AssertEquals(0, statement.PeriodMonth);
			AssertEquals(0, statement.PeriodYear);

			statement.B2_PeriodEndDate = new ZDate(2020, 09, 03);
			AssertEquals(0, statement.PeriodMonth);
			AssertEquals(0, statement.PeriodYear);

			statement.B2_PeriodEndDate = new ZDate(2020, 09, 18);
			AssertEquals(0, statement.PeriodMonth);
			AssertEquals(0, statement.PeriodYear);

			statement.B2_PeriodEndDate = new ZDate(2020, 09, 17);
			statement.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			statement.B2_PeriodEndDate = new ZDate(2020, 09, 18);
			AssertEquals(9, statement.PeriodMonth);
			AssertEquals(2020, statement.PeriodYear);

			statement.B2_PeriodEndDate = new ZDate(2020, 09, 30);
			AssertEquals(9, statement.PeriodMonth);
			AssertEquals(2020, statement.PeriodYear);
		}

		public void TestCalculatePeriodEndDate()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			Assert("Should be readonly", statement.PeriodMonthInfo.ReadOnly);
			Assert("Should be readonly", statement.PeriodYearInfo.ReadOnly);

			var org = Factory.New<OrgHeader>();
			var impAddInfo = OrgImpAddInfo.Get(org);
			impAddInfo.ZO_IsCSAApprovedImporter = true;
			impAddInfo.ZO_AccountingTimeOption = CSARSFAccountingOptionList.Codes.Option1;

			statement.B2_OH_Importer = org.PK;
			Assert("Should not be readonly", !statement.PeriodMonthInfo.ReadOnly);
			Assert("Should not be readonly", !statement.PeriodYearInfo.ReadOnly);

			statement.PeriodMonth = 12;
			statement.PeriodYear = 2021;
			AssertEquals(new ZDate(2021, 12, 31), statement.B2_PeriodEndDate);
			AssertEquals(new ZDate(2021, 12, 1), statement.B2_PeriodStartDate);

			impAddInfo.ZO_AccountingTimeOption = CSARSFAccountingOptionList.Codes.Option2;
			statement.PeriodMonth = 11;
			AssertEquals(new ZDate(2021, 11, 18), statement.B2_PeriodEndDate);
			AssertEquals(new ZDate(2021, 10, 19), statement.B2_PeriodStartDate);

			var org1 = Factory.New<OrgHeader>();
			var impAddInfo1 = OrgImpAddInfo.Get(org1);
			impAddInfo1.ZO_IsCSAApprovedImporter = true;
			impAddInfo1.ZO_AccountingTimeOption = CSARSFAccountingOptionList.Codes.Option1;

			statement.B2_OH_Importer = org1.PK;
			AssertEquals(new ZDate(2021, 11, 30), statement.B2_PeriodEndDate);
			AssertEquals(new ZDate(2021, 11, 1), statement.B2_PeriodStartDate);
		}

		public void TestPeriod()
		{
			var statement = Factory.New<CusStatementHeader>();
			AssertEquals(ZString.Empty, statement.Period);

			statement.B2_PeriodEndDate = new ZDate(2020, 09, 03);
			AssertEquals(ZString.Empty, statement.Period);

			statement.B2_PeriodStartDate = new ZDate(2020, 08, 03);
			AssertEquals(ZString.Empty, statement.Period);

			statement.B2_PeriodEndDate = new ZDate(2020, 09, 18);
			AssertEquals(ZString.Empty, statement.Period);

			statement.B2_PeriodEndDate = new ZDate(2020, 09, 17);
			statement.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			statement.B2_PeriodEndDate = new ZDate(2020, 09, 18);
			AssertEquals("20200819", statement.B2_PeriodStartDate.ToString("yyyyMMdd"));
			AssertEquals("09/2020", statement.Period);

			statement.B2_PeriodEndDate = new ZDate(2020, 09, 30);
			AssertEquals("20200901", statement.B2_PeriodStartDate.ToString("yyyyMMdd"));
			AssertEquals("09/2020", statement.Period);
		}

		public void TestCalcultedProperties()
		{
			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_IsMonthlyStatement = false;

			var line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;

			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.Others, 3425.26m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 18743.54m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalGSTAmount, 25848.45m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalDutyAmount, 135847.00m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalSIMAAmount, -1150.2m);

			line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;

			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.Others, 184.61m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 0.35m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalGSTAmount, 76873.32m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalDutyAmount, -1973.12m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalSIMAAmount, 823.42m);

			line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Other;

			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.Others, 12.65m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 987.45m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalGSTAmount, 25.32m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalDutyAmount, 0.254m);
			line.Charges.UpdateLineChargeFor(EntryChargeTypeList.Codes.TotalSIMAAmount, -2.58m);

			CombineAssertions(() =>
			{
				AssertEquals("B2_TotalOthers", 3609.87m, dailyStatement.B2_TotalOthers);
				AssertEquals("B2_TotalExciseTax", 18743.89m, dailyStatement.B2_TotalExciseTax);
				AssertEquals("B2_TotalGST", 102721.77m, dailyStatement.B2_TotalGST);
				AssertEquals("B2_TotalCustomsDuties", 133873.88m, dailyStatement.B2_TotalCustomsDuties);
				AssertEquals("B2_TotalSIMA", -326.78m, dailyStatement.B2_TotalSIMA);
				AssertEquals("B2_TotalInterests", 0m, dailyStatement.B2_TotalInterests);
			});
		}

		public void TestCARMDailyNoticeTotalChargeAmount_IsCARMDailyNotice()
		{
			var dailyStatement = CreateCusStatementHeaderForCARMDNTotalChargeAmount();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				CombineAssertions(() =>
				{
					AssertEquals("B2_TotalCustomsDuties", 114m, dailyStatement.B2_TotalCustomsDuties);
					AssertEquals("B2_TotalExciseTax", 34m, dailyStatement.B2_TotalExciseTax);
					AssertEquals("B2_TotalExciseDuties", 324m, dailyStatement.B2_TotalExciseDuties);
					AssertEquals("B2_TotalGST", -30m, dailyStatement.B2_TotalGST);
					AssertEquals("B2_TotalInterests", 735m, dailyStatement.B2_TotalInterests);
					AssertEquals("B2_TotalOthers", -699m, dailyStatement.B2_TotalOthers);
					AssertEquals("B2_TotalSIMA", 190m, dailyStatement.B2_TotalSIMA);
				});
			}
		}

		public void TestParentStatement()
		{
			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_IsMonthlyStatement = false;

			dailyStatement.B2_B2_PeriodicStatement = ZGuid.Empty;
			AssertNull("Should be null as the B2_B2_PeriodicStatement is not valid.", dailyStatement.ParentStatement);

			dailyStatement.B2_B2_PeriodicStatement = ZGuid.Invalid;
			AssertNull("Should be null as the B2_B2_PeriodicStatement is not valid.", dailyStatement.ParentStatement);

			var monthStatement = Factory.New<CusStatementHeader>();

			monthStatement.B2_IsMonthlyStatement = true;
			AssertNull("Should be null as the statement is not daily.", monthStatement.ParentStatement);

			dailyStatement.B2_B2_PeriodicStatement = monthStatement.PK;
			AssertNotNull("Should not be null as the statement is daily and it links to a valid month Statement.", dailyStatement.ParentStatement);
		}

		public void TestRefreshLineGroupStatementLines()
		{
			var header = Factory.New<CusStatementHeader>();
			var line1 = header.StatementLines.AddNew();
			line1.B3_ImporterCustomsID = "IMP1";
			var line2 = header.StatementLines.AddNew();
			line2.B3_ImporterCustomsID = "IMP2";
			var line3 = header.StatementLines.AddNew();
			line3.B3_ImporterCustomsID = "IMP1";
			var line4 = header.StatementLines.AddNew();
			line4.B3_ImporterCustomsID = "IMP2";
			var lineGroup1 = header.LineGroupCollection.AddNew();
			lineGroup1.B10_ImporterCustomsID = "IMP2";
			var lineGroup2 = header.LineGroupCollection.AddNew();
			lineGroup2.B10_ImporterCustomsID = "IMP1";
			AssertArrayEqualsByElements(new[] { line2, line4 }, lineGroup1.StatementLines);
			AssertArrayEqualsByElements(new[] { line1, line3 }, lineGroup2.StatementLines);
			var row = ((INeedRow)line2).Row;
			row[CusStatementLine.Schema.B3_ImporterCustomsID] = new ZString("IMP1");
			AssertArrayEqualsByElements(new[] { line2, line4 }, lineGroup1.StatementLines);
			AssertArrayEqualsByElements(new[] { line1, line3 }, lineGroup2.StatementLines);
			header.RefreshLineGroupStatementLines();
			AssertArrayEqualsByElements(new[] { line4 }, lineGroup1.StatementLines);
			AssertArrayEqualsByElements(new[] { line1, line2, line3 }, lineGroup2.StatementLines);
		}

		public void TestDailyStatementHeaders()
		{
			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_IsMonthlyStatement = false;

			var monthStatement = Factory.New<CusStatementHeader>();
			monthStatement.B2_IsMonthlyStatement = true;

			dailyStatement.B2_B2_PeriodicStatement = monthStatement.PK;

			var invalidDailyStatement = Factory.New<CusStatementHeader>();
			invalidDailyStatement.B2_IsMonthlyStatement = false;

			var invalidMonthStatement = Factory.New<CusStatementHeader>();
			invalidMonthStatement.B2_IsMonthlyStatement = true;

			invalidDailyStatement.B2_B2_PeriodicStatement = dailyStatement.PK;
			invalidMonthStatement.B2_B2_PeriodicStatement = monthStatement.PK;

			AssertEquals("Should load nothing for a Daily Statement.", 0, dailyStatement.DailyStatementHeaders.Count);
			AssertEquals("Should load all child Daily Statement for a SOA Statement.", 1, monthStatement.DailyStatementHeaders.Count);
			AssertEquals("Should load all child Daily Statement for a SOA Statement.", dailyStatement.PK, monthStatement.DailyStatementHeaders[0].PK);
		}

		public void TestHumanReadableName()
		{
			void AssertHumanReadableName(bool isMonthlyStatement, string statementNumber, string importerCustomsId, string expectedValue)
			{
				statementHeader.B2_IsMonthlyStatement = isMonthlyStatement;
				statementHeader.B2_StatementNumber = statementNumber;
				statementHeader.B2_ImporterCustomsID = importerCustomsId;

				AssertEquals(expectedValue, statementHeader.HumanReadableName);
			}

			AssertHumanReadableName(true, string.Empty, "BID1125", "Statement Of Account - BID1125");
			AssertHumanReadableName(true, "N181125", "BID1125", "Statement Of Account - N181125");
			AssertHumanReadableName(true, string.Empty, string.Empty, "Statement Of Account - ");

			AssertHumanReadableName(false, string.Empty, "BID1125", "Daily Notice - BID1125");
			AssertHumanReadableName(false, "N181125", "BID1125", "Daily Notice - N181125");
			AssertHumanReadableName(false, string.Empty, string.Empty, "Daily Notice - ");
		}

		public void TestARLMessageType()
		{
			statementHeader.B2_IsMonthlyStatement = true;
			AssertEquals(ARLMessageTypes.Descriptions.StatementOfAccount, statementHeader.ARLMessageType);

			statementHeader.B2_IsMonthlyStatement = false;
			AssertEquals(ARLMessageTypes.Descriptions.DailyNotice, statementHeader.ARLMessageType);
		}

		public void TestWorkflowType()
		{
			statementHeader.B2_IsMonthlyStatement = true;
			AssertEquals("STM", ((IWorkflowProvider)statementHeader).WorkflowType);

			statementHeader.B2_IsMonthlyStatement = false;
			AssertEquals("DNC", ((IWorkflowProvider)statementHeader).WorkflowType);
		}

		public void TestWorkflowItems()
		{
			statementHeader.B2_IsMonthlyStatement = true;
			AssertType<StatementProcessTaskCollection>(statementHeader.WorkflowItems);

			statementHeader.B2_IsMonthlyStatement = false;
			AssertType<DailyNoticeStatementProcessTaskCollection>(statementHeader.WorkflowItems);
		}

		public void TestProcessTaskType()
		{
			statementHeader.B2_IsMonthlyStatement = true;
			AssertEquals(typeof(StatementProcessTask), statementHeader.ProcessTaskType);

			statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_IsMonthlyStatement = false;
			AssertEquals(typeof(DailyNoticeStatementProcessTask), statementHeader.ProcessTaskType);
		}

		CusStatementHeader CreateCusStatementHeaderForCARMDNTotalChargeAmount()
		{
			var dailyStatement = Factory.New<CusStatementHeader>();
			dailyStatement.B2_IsMonthlyStatement = false;
			dailyStatement.B2_StatementNumber = "DN-123456789RM2345-1";

			var line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;

			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Duties, 12m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.SIMA, -23m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.ExciseTax, 34m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, 45m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.HarmonizedSalesTax, 56m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Interest, 78m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Others, 89m);

			line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Normal;

			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Duties, 112m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.SIMA, 223m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.ExciseDuties, 334m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, 445m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax, -556m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Interest, 667m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Others, -778m);

			line = dailyStatement.StatementLines.AddNew();
			line.B3_Status = TransactionBatchConstants.TransactionCategoryCodes.Other;

			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Duties, -10m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.SIMA, -10m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.ExciseDuties, -10m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.GoodsAndServicesTax, -10m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.ProvincialSalesTax, -10m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Interest, -10m);
			line.Charges.UpdateLineChargeFor(CARMDailyNoticeChargeTypeList.Codes.Others, -10m);

			return dailyStatement;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			statementHeader.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			return statementHeader;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Deleting object should have an exception/error because of a trigger.", true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			statementHeader = Factory.New<CusStatementHeader>();
			csaRSF = Factory.New<CusStatementHeader>();
			csaRSF.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
		}

		CusStatementHeader statementHeader;
		CusStatementHeader csaRSF;
	}
}
