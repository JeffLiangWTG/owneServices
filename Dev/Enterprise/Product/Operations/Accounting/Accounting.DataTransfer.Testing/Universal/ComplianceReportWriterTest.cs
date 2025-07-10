using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Lookups = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using UniversalOrgAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	public class ComplianceReportWriterTest : TestCaseWithFactory
	{
		public void TestWrite()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			Creator.CreateConfigurationForComplianceReport(report);

			var invoice = Creator.CreateAPInvoice<APInvoice>("I0001", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AT = Creator.GST1.PK;
			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(report, line, 1);

			var writer = new ComplianceReportWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, report)));
			var reportDataObject = writer.GetDataObject(report);
			AssertNotNull(reportDataObject);
			AssertReportWasWrittenCorrectly(report, reportDataObject);
		}

		public void TestWriteDayBook()
		{
			var reportConfigurations = new ComplianceReportConfigurationCollection();
			var reportConfig = reportConfigurations.AddNew();
			reportConfig.Country = Environment.Env.CurrentCompany.Country.Code;
			reportConfig.ReportCode = "LIB";
			reportConfig.ReportBaseTablePrefix = Lookups.ReportBaseTablePrefixListCodes.AllTransactions;
			reportConfig.ReportLineGrouping = Lookups.ReportLineGroupingListCodes.DayBook;
			reportConfig.ReportPeriodicity = Lookups.ReportPeriodicityCodes.DateRange;
			reportConfig.TaxRegistrationType = reportConfig.Lookups.TaxRegistrationTypeList[0].Code;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = "LIB";
			report.SetOpeningBalance_ForTestOnly(1000m);

			var invoice = Creator.CreateAPInvoice<APInvoice>("I0001", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, Creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AG = Creator.GLHeader1.PK;
			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(report, line, 1, "*AP*INV**");

			var writer = new ComplianceReportWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, report)));
			var reportDataObject = writer.GetDataObject(report);
			AssertNotNull(reportDataObject);
			AssertReportWasWrittenCorrectly(report, reportDataObject);
		}

		public void TestWriteHungaryTaxAudit()
		{
			var reportConfigurations = new ComplianceReportConfigurationCollection();
			var reportConfig = reportConfigurations.AddNew();
			reportConfig.Country = "AU"; // for test only, should be HU
			reportConfig.ReportCode = "RAF";
			reportConfig.ReportBaseTablePrefix = Lookups.ReportBaseTablePrefixListCodes.TransactionLine;
			reportConfig.ReportLineGrouping = Lookups.ReportLineGroupingListCodes.TransactionHeaderWithLines;
			reportConfig.ReportPeriodicity = Lookups.ReportPeriodicityCodes.DateRange;
			reportConfig.TaxRegistrationType = reportConfig.Lookups.TaxRegistrationTypeList[0].Code;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = "RAF";

			var originalInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0000", Creator.AUD, 1m, 100m, 0m, 100m, 0m);
			var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 200m, 20m, 200m, 20m);
			invoice.AH_OH = Creator.ABIGAS.PK;
			invoice.AH_InvoiceDate = invoice.AH_PostDate.AddDays(1);
			invoice.AH_TransactionBelongsToGroup = originalInvoice.PK;
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AT = Creator.GST1.PK;
			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(report, line, 1, "00001");

			var writer = new ComplianceReportWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, report)));
			var reportDataObject = writer.GetDataObject(report);
			AssertNotNull(reportDataObject);
			AssertReportWasWrittenCorrectly(report, reportDataObject);
			// Test additional header details
			AssertEquals("TransactionCollection.Count", 1, reportDataObject.TransactionCollection.Count);
			AssertNotNull("OSCUrrency", reportDataObject.TransactionCollection[0].OSCurrency);
			AssertEquals("OSCUrrency.Code", Creator.USD.Code, reportDataObject.TransactionCollection[0].OSCurrency.Code);
			AssertEquals("TransactionDate", invoice.AH_InvoiceDate.Date, reportDataObject.TransactionCollection[0].TransactionDate.Value.Date);
			AssertNotNull("OriginalReference", reportDataObject.TransactionCollection[0].OriginalReference);
			AssertEquals("OriginalReference.OriginalTransactionNumber", originalInvoice.AH_TransactionNum, reportDataObject.TransactionCollection[0].OriginalReference.OriginalTransactionNumber);
			// Test additional line details
			AssertEquals("PostingJournalCollection.Count", 1, reportDataObject.TransactionCollection[0].PostingJournalCollection.Count);
			AssertNotNull("GSTVATBasis", reportDataObject.TransactionCollection[0].PostingJournalCollection[0].GSTVATBasis);
			AssertEquals("GSTVATBasis.Code", line.AL_GSTVATBasis, reportDataObject.TransactionCollection[0].PostingJournalCollection[0].GSTVATBasis.Code);
			AssertEquals("Description", line.AL_Desc, reportDataObject.TransactionCollection[0].PostingJournalCollection[0].Description);
		}

		public void TestDayBookWithoutGrouping()
		{
			Creator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3)); // Need to set up Financial year to pass balance from report to report inside Financial Year

			var report = Factory.NewWithValidTestData<AccComplianceReport>();

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
			report.ACR_Periodicity = Lookups.ReportPeriodicityCodes.AccountingPeriod;
			report.AccountingPeriod = currentPeriod.AM_Period;
			AssertEquals("ACR_DateFrom", currentPeriod.AM_StartDate.Date, report.ACR_DateFrom);
			AssertEquals("ACR_DateTo", currentPeriod.AM_EndDate.Date, report.ACR_DateTo);
			Creator.CreateConfigurationForComplianceReport(report, Lookups.ReportBaseTablePrefixListCodes.AllTransactions, Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
			Factory.Save();

			var apSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value;
			var arSuspenseAccountPK = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseAccountPK);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseAccountPK);

			var openingPeriod = periodCalculator.GetPeriodFromDate(ZDateTime.Today.AddMonths(-2));
			var openingBalanceDR = Creator.CreateAccGLAggregate(123m, openingPeriod, arSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			var openingBalanceCR = Creator.CreateAccGLAggregate(-123m, openingPeriod, apSuspenseAccountPK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Factory.Save();

			var originalInvoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0000", Creator.AUD, 1m, 100m, 0m, 100m, 0m);
			var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.USD, 1m, 200m, 20m, 200m, 20m);
			invoice.AH_TransactionCategory = Enterprise.Core.Constants.TransactionCategory.Codes.SelfBilling;
			invoice.AH_OH = Creator.ABIGAS.PK;
			invoice.AH_InvoiceDate = invoice.AH_PostDate.AddDays(1);
			invoice.AH_TransactionBelongsToGroup = originalInvoice.PK;
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line = invoice.Lines[0];
			line.AL_AT = Creator.GST1.PK;
			line.AL_AG = Creator.GLHeader1.PK;
			var line2 = Creator.CreateInvoiceLine(invoice, Creator.CC1.PK, 100m, Creator.USD);
			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(report, invoice, 1, "*AR*INV*ARCtrl*Total");
			Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV**Rev-");
			Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*ARSusp*-");
			Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*ARSusp*Rev");
			Creator.CreateComplianceReportTransactionPivot(report, line, 2, "*AR*INV*GSTOut*-");
			Creator.CreateComplianceReportTransactionPivot(report, line2, 3, "*AR*INV**Rev-");
			Creator.CreateComplianceReportTransactionPivot(report, line2, 3, "*AR*INV*ARSusp*-");
			Creator.CreateComplianceReportTransactionPivot(report, line2, 3, "*AR*INV*ARSusp*Rev");
			Creator.CreateComplianceReportTransactionPivot(report, line2, 3, "*AR*INV*GSTOut*-");

			AssertEquals("GLOpeningBalanceDR", 123m, report.GLOpeningBalanceDR);
			AssertEquals("GLOpeningBalanceCR", 123m, report.GLOpeningBalanceCR);

			var writer = new ComplianceReportWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, report)));
			var reportDataObject = writer.GetDataObject(report);
			AssertNotNull(reportDataObject);
			AssertReportWasWrittenCorrectly(report, reportDataObject);

			AssertEquals("TransactionCollection.Count", 2, reportDataObject.TransactionCollection.Count);
			// Assert Opening Balance in the first line
			AssertEquals("PostingJournalCollection.Count", 2, reportDataObject.TransactionCollection[0].PostingJournalCollection.Count);

			var balanceLineDR = reportDataObject.TransactionCollection[0].PostingJournalCollection[0];
			AssertEquals("Sequence", 0, balanceLineDR.Sequence.Value);
			AssertEquals("LocalAmount", 123m, balanceLineDR.LocalAmount.Value);
			AssertNotNull("Has Detals Collection", balanceLineDR.PostingJournalDetailCollection);
			Assert("Has only Debit GL Accounts", balanceLineDR.PostingJournalDetailCollection.All(x => x.DebitGLAccount != null && x.CreditGLAccount == null));
			var balanceDetailLineDR = balanceLineDR.PostingJournalDetailCollection.FirstOrDefault(x => x.PostingAmount.Value == 123m);
			AssertNotNull(balanceDetailLineDR);
			var expectedGLAccountDR = Factory.Load<AccGLHeader>(arSuspenseAccountPK);
			AssertEquals("Expected GL Account number", expectedGLAccountDR.AG_AccountNum, balanceDetailLineDR.DebitGLAccount?.AccountCode);

			var balanceLineCR = reportDataObject.TransactionCollection[0].PostingJournalCollection[1];
			AssertEquals("Sequence", 0, balanceLineCR.Sequence.Value);
			AssertEquals("LocalAmount", -123m, balanceLineCR.LocalAmount.Value);
			AssertNotNull("Has Detals Collection", balanceLineCR.PostingJournalDetailCollection);
			Assert("Has only Credit GL Accounts", balanceLineCR.PostingJournalDetailCollection.All(x => x.DebitGLAccount == null && x.CreditGLAccount != null));
			var balanceDetailLineCR = balanceLineCR.PostingJournalDetailCollection.FirstOrDefault(x => x.PostingAmount.Value == 123m);
			AssertNotNull(balanceDetailLineCR);
			var expectedGLAccountCR = Factory.Load<AccGLHeader>(apSuspenseAccountPK);
			AssertEquals("Expected GL Account number", expectedGLAccountCR.AG_AccountNum, balanceDetailLineCR.CreditGLAccount?.AccountCode);

			// Test additional header details
			AssertNotNull("OSCurrency", reportDataObject.TransactionCollection[1].OSCurrency);
			AssertEquals(invoice.AH_RX_NKTransactionCurrency, (ZString)reportDataObject.TransactionCollection[1].OSCurrency.Code);
			AssertEquals(invoice.AH_ExchangeRate, (ZDecimal)reportDataObject.TransactionCollection[1].ExchangeRate);
			AssertEquals(invoice.AH_OSTotalAmount, (ZDecimal)reportDataObject.TransactionCollection[1].OSTotal);
			AssertEquals("As OB_ARCustomerSelfBillsRevenue was set", "CustomerSelfBill", reportDataObject.TransactionCollection[1].PlaceOfIssue);
			AssertCreateLastEditProperties(reportDataObject.TransactionCollection[1]);

			// Test additional line details
			AssertEquals("PostingJournalCollection.Count", 3, reportDataObject.TransactionCollection[1].PostingJournalCollection.Count);

			// First line represents total Control Account movement for transaction
			AssertNotNull(reportDataObject.TransactionCollection[1].PostingJournalCollection[0].ChargeCurrency);
			AssertEquals(report.Company.LocalCurrency.RX_Code, (ZString)reportDataObject.TransactionCollection[1].PostingJournalCollection[0].ChargeCurrency.Code);
			AssertEquals(invoice.AH_LocalExTaxAmount, (ZDecimal)reportDataObject.TransactionCollection[1].PostingJournalCollection[0].LocalAmount);
			AssertEquals(invoice.AH_LocalTaxAmount, (ZDecimal)reportDataObject.TransactionCollection[1].PostingJournalCollection[0].LocalGSTVATAmount);

			// Line 1
			AssertEquals("Sequence", line.AL_Sequence, (ZShort)reportDataObject.TransactionCollection[1].PostingJournalCollection[1].Sequence);
			AssertNull("ChargeCode", reportDataObject.TransactionCollection[1].PostingJournalCollection[1].ChargeCode);
			AssertNotNull(reportDataObject.TransactionCollection[1].PostingJournalCollection[1].GSTVATBasis);
			AssertEquals("GSTVATBasis", line.AL_GSTVATBasis, reportDataObject.TransactionCollection[1].PostingJournalCollection[1].GSTVATBasis.Code);
			AssertEquals("Description", line.AL_Desc, reportDataObject.TransactionCollection[1].PostingJournalCollection[1].Description);
			AssertEquals("OSAmount", line.AL_OSExTaxAmount, reportDataObject.TransactionCollection[1].PostingJournalCollection[1].OSAmount);
			// Line 2
			AssertEquals("Sequence", line2.AL_Sequence, (ZShort)reportDataObject.TransactionCollection[1].PostingJournalCollection[2].Sequence);
			AssertNotNull("ChargeCode", reportDataObject.TransactionCollection[1].PostingJournalCollection[2].ChargeCode);
			AssertEquals("ChargeCode.Code", line2.ChargeCode.AC_Code, reportDataObject.TransactionCollection[1].PostingJournalCollection[2].ChargeCode.Code.Value);
			AssertEquals("ChargeCode.Description", line2.ChargeCode.AC_Desc, reportDataObject.TransactionCollection[1].PostingJournalCollection[2].ChargeCode.Description.Value);
			AssertNotNull(reportDataObject.TransactionCollection[1].PostingJournalCollection[2].GSTVATBasis);
			AssertEquals("GSTVATBasis", line2.AL_GSTVATBasis, reportDataObject.TransactionCollection[1].PostingJournalCollection[2].GSTVATBasis.Code);
			AssertEquals("Description", line2.AL_Desc, reportDataObject.TransactionCollection[1].PostingJournalCollection[2].Description);
			AssertEquals("OSAmount", line2.AL_OSExTaxAmount, reportDataObject.TransactionCollection[1].PostingJournalCollection[2].OSAmount);
		}

		void AssertCreateLastEditProperties(TransactionInfo transaction)
		{
			AssertNotNull("CreateTime", transaction.CreateTime);
			AssertNotNull("CreateUser", transaction.CreateUser);
			AssertEquals("CreateUser.Code", "E", transaction.CreateUser.Code);
			AssertEquals("CreateUser.Name", "CargoWise Support", transaction.CreateUser.Name);
			AssertNotNull("LastEditTime", transaction.LastEditTime);
			AssertNotNull("LastEditUser", transaction.LastEditUser);
			AssertEquals("LastEditUser.Code", "E", transaction.LastEditUser.Code);
			AssertEquals("LastEditUser.Name", "CargoWise Support", transaction.LastEditUser.Name);
		}

		public void TestHeaderDetails_DayBook() => AssertHeaderDetails(Lookups.ReportLineGroupingListCodes.DayBook);
		public void TestHeaderDetails_DayBookWithoutGrouping() => AssertHeaderDetails(Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);

		void AssertHeaderDetails(string grouping)
		{
			var reportConfigurations = new ComplianceReportConfigurationCollection();
			var reportConfig = reportConfigurations.AddNew();
			reportConfig.Country = "AU"; // for test only, should be HU
			reportConfig.ReportCode = "TST";
			reportConfig.ReportBaseTablePrefix = Lookups.ReportBaseTablePrefixListCodes.AllTransactions;
			reportConfig.ReportLineGrouping = grouping;
			reportConfig.ReportPeriodicity = Lookups.ReportPeriodicityCodes.DateRange;
			reportConfig.TaxRegistrationType = reportConfig.Lookups.TaxRegistrationTypeList[0].Code;
			reportConfig.GoodsServiceType = Lookups.GoodsServiceTypeCodes.GoodsAndService;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Creator.CreateAPControlAccount().PK.ToGuid());

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = "TST";

			Creator.AUDBankAccount.AB_AG = Creator.InsertGLHeader().PK;
			var payment = Creator.CreateAPPayment(1, 100, ZDateTime.Today, ZDateTime.Today, Creator.ABIGAS.PK, Creator.AUDBankAccount.PK);
			payment.AH_RX_NKTransactionCurrency = "AUD";

			AccAPAccountDetails accDetails = Factory.NewWithValidTestData<AccAPAccountDetails>();
			accDetails.A1_IsDefaultAccount = true;
			accDetails.A1_PaymentMethod = "DDR";
			accDetails.A1_RX_NKAccountCurrency = "AUD";
			accDetails.A1_RN_NKCountryCode = "AU";
			accDetails.A1_BankAccount = "Number123";
			accDetails.A1_BankName = "Name123";
			accDetails.A1_OB = Creator.ABIGAS.CompanyData.PK;

			var shipment = Creator.CreateShipment("S01234", false);
			var job = Creator.CreateJob(shipment, false);

			var wip = Creator.CreateWIP(job, Creator.CC1, 1, "a new wip", 100, debtor: Creator.ABIGAS);
			wip.AL_AG = Creator.GLHeader1.PK;
			var accrual = Creator.CreateAccrual(job, Creator.CC2, 1, "a new accrual", 200, creditor: Creator.AALSHI);
			accrual.AL_AG = Creator.GLHeader2.PK;

			Factory.Save();

			Creator.CreateComplianceReportTransactionPivot(report, payment, 1, "*AP*PAY*Bank*-");
			Creator.CreateComplianceReportTransactionPivot(report, wip, 2, "*JC*WIP*");
			Creator.CreateComplianceReportTransactionPivot(report, accrual, 3, "*JC*ACR*");

			var writer = new ComplianceReportWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, report)));
			var reportDataObject = writer.GetDataObject(report);
			AssertNotNull(reportDataObject);
			AssertReportWasWrittenCorrectly(report, reportDataObject);

			AssertEquals("TransactionCollection.Count", report.SupportsDayBook ? 3 : 2, reportDataObject.TransactionCollection.Count);

			if (report.SupportsDayBook)
			{
				AssertEquals("Description", AccountingConstants.DefaultDayBookLineDescriptions.WIPAccrual, reportDataObject.TransactionCollection[2].Description);
			}
			AssertEquals("Description", payment.AH_Desc, reportDataObject.TransactionCollection[1].Description);
			AssertEquals("CreateTime", payment.AH_SystemCreateTimeUtc.ToString("yyyy-MM-ddTHH:mm:00"), reportDataObject.TransactionCollection[1].CreateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
			AssertEquals("CreateUser code", GlbStaff.CurrentUser.GS_Code, reportDataObject.TransactionCollection[1].CreateUser.Code);
			AssertEquals("CreateUser name", GlbStaff.CurrentUser.GS_FullName, reportDataObject.TransactionCollection[1].CreateUser.Name);

			if (grouping == Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping)
			{
				AssertEquals("TransactionDate", payment.AH_InvoiceDate.Date, reportDataObject.TransactionCollection[1].TransactionDate.Value.Date);
				AssertEquals("OSTotal", 100M, reportDataObject.TransactionCollection[1].OSTotal);
				AssertEquals("PaymentOrReceiptType", new PaymentOrReceiptTypeConverter().ToEnumValue(payment.AH_ReceiptType), reportDataObject.TransactionCollection[1].PaymentOrReceiptType);
				AssertEquals("CheckNumberOrPaymentRef", payment.AH_ChequeOrReference, reportDataObject.TransactionCollection[1].CheckNumberOrPaymentRef);
				AssertEquals("OSTotal", payment.AH_OSTotal, reportDataObject.TransactionCollection[1].OSTotal);
				AssertEquals("BankAccountCollection.Count", 2, reportDataObject.TransactionCollection[1].BankAccountCollection.Count);
				foreach (var bankAccount in reportDataObject.TransactionCollection[1].BankAccountCollection)
				{
					if (bankAccount.AccountType == BankAccountType.Debit)
					{
						AssertEquals("AccountNumber", Creator.AUDBankAccount.AB_AccountNum, bankAccount.AccountNumber);
						AssertEquals("BankName", Creator.AUDBankAccount.AB_BankName, bankAccount.BankName);
						AssertEquals("Country.Code", Creator.AUDBankAccount.BankAccountCountry.Code, bankAccount.Country.Code);
						AssertEquals("Country.Name", Creator.AUDBankAccount.BankAccountCountry.Description, bankAccount.Country.Name);
					}
					else if (bankAccount.AccountType == BankAccountType.Credit)
					{
						AssertEquals("AccountNumber", accDetails.A1_BankAccount, bankAccount.AccountNumber);
						AssertEquals("BankName", accDetails.A1_BankName, bankAccount.BankName);
						AssertEquals("Country.Code", accDetails.Country.Code, bankAccount.Country.Code);
						AssertEquals("Country.Name", accDetails.Country.Description, bankAccount.Country.Name);
					}
				}
			}
			else
			{
				AssertEquals("TransactionDate", null, reportDataObject.TransactionCollection[1].TransactionDate);
				AssertEquals("OSTotal", null, reportDataObject.TransactionCollection[1].OSTotal);
				AssertEquals("PaymentOrReceiptType", null, reportDataObject.TransactionCollection[1].PaymentOrReceiptType);
				AssertEquals("CheckNumberOrPaymentRef", null, reportDataObject.TransactionCollection[1].CheckNumberOrPaymentRef);
				AssertEquals("OSTotal", null, reportDataObject.TransactionCollection[1].OSTotal);
				AssertEquals("BankAccountCollection.Count", null, reportDataObject.TransactionCollection[1].BankAccountCollection);
			}
		}

		void AssertReportWasWrittenCorrectly(AccComplianceReport report, TransactionBatch reportDataObject)
		{
			CombineAssertions(delegate
			{
				AssertEquals("BatchType.Code", report.ACR_ReportType, reportDataObject.BatchType.Code);
				AssertEquals("BatchType.Description", report.ACR_Description, reportDataObject.BatchType.Description);
				AssertEquals("Periodicity.Code", report.ACR_Periodicity, reportDataObject.Periodicity.Code);
				AssertEquals("Periodicity.Description", report.Lookups.PeriodicityList.GetDescriptionFromCode(report.ACR_Periodicity), reportDataObject.Periodicity.Description);
				AssertEquals("DateFrom", report.ACR_DateFrom, reportDataObject.DateFrom);
				AssertEquals("DateTo", report.ACR_DateTo, reportDataObject.DateTo);

				AssertNotNull("OrganizationAddressCollection", reportDataObject.OrganizationAddressCollection);
				AssertNotNull("SendingCompanyOrganizationAddress", reportDataObject.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.HasValue && x.AddressType.Value == new ZString(TransactionBatchOrganizationType.SendingCompany)));

				var isDayBook = report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.DayBook;
				var isDayBookWithoutGrouping = report.ReportLineGrouping == Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;
				var expectedLineCount = isDayBook || isDayBookWithoutGrouping ? report.ReportLines.Cast<AccComplianceReportLine>().Select(x => x.AH_PK).Distinct().Count() + 1 : report.ReportLines.Count;

				AssertEquals("TransactionCollection.Count", expectedLineCount, reportDataObject.TransactionCollection.Count);
				if (isDayBook || isDayBookWithoutGrouping)
				{
					var openingBalancesLine = reportDataObject.TransactionCollection[0];

					AssertEquals("Ledger.HasValue", false, openingBalancesLine.Ledger.HasValue);
					AssertEquals("TransactionType.HasValue", false, openingBalancesLine.TransactionType.HasValue);
					AssertEquals("PostDate", report.ACR_DateFrom.AddDays(-1), openingBalancesLine.PostDate);
					AssertEquals("Number.HasValue", false, openingBalancesLine.Number.HasValue);
					AssertEquals("TransactionReference.HasValue", false, openingBalancesLine.TransactionReference.HasValue);
					AssertEquals("ComplianceSubType.HasValue", false, openingBalancesLine.ComplianceSubType.HasValue);
					AssertEquals("Description.HasValue", false, openingBalancesLine.Description.HasValue);

					AssertNotNull("LocalCurrency", openingBalancesLine.LocalCurrency);
					AssertEquals("LocalCurrency.Code", report.Company.LocalCurrency.RX_Code, openingBalancesLine.LocalCurrency.Code);
					AssertEquals("LocalCurrency.Description", report.Company.LocalCurrency.RX_Desc, openingBalancesLine.LocalCurrency.Description);

					AssertEquals("LocalExVATAmount.HasValue", false, openingBalancesLine.LocalExVATAmount.HasValue);
					AssertEquals("LocalVATAmount.HasValue", false, openingBalancesLine.LocalVATAmount.HasValue);

					AssertNull("OrganizationAddress", openingBalancesLine.OrganizationAddress);

					AssertNotNull("PostingJournalCollection", openingBalancesLine.PostingJournalCollection);
					AssertEquals("PostingJournalCollection.Count", 2, openingBalancesLine.PostingJournalCollection.Count);

					foreach (var postingJournal in openingBalancesLine.PostingJournalCollection)
					{
						Assert("LocalAmount.HasValue", postingJournal.LocalAmount.HasValue);
					}

					var subInfos = openingBalancesLine.PostingJournalCollection.OrderBy(x => x.LocalAmount.Value).ToArray();
					AssertEquals("LocalAmount equals -GLOpeningBalanceCR", -report.GLOpeningBalanceCR, subInfos[0].LocalAmount);
					AssertEquals("LocalAmount equals GLOpeningBalanceDR", report.GLOpeningBalanceDR, subInfos[1].LocalAmount);
				}

				var transactionIndex = isDayBook || isDayBookWithoutGrouping ? 1 : 0;
				TransactionInfo lineInfo = null;
				PostingJournal lineSubInfo = null;
				var headerPK = ZGuid.Empty;
				var lineIndex = 0;
				var reportLineSequence = 0;
				var lineDetailIndex = 0;

				for (int i = 0; i < report.ReportLines.Count; i++)
				{
					var line = report.ReportLines[i];

					if (lineInfo == null || headerPK != line.AH_PK)
					{
						lineInfo = reportDataObject.TransactionCollection[transactionIndex];
						headerPK = line.AH_PK;
						transactionIndex++;
						lineIndex = 0;

						AssertEquals("Ledger", line.AH_Ledger, lineInfo.Ledger);
						AssertEquals("TransactionType", line.AH_TransactionType.IsEmpty ? null : new TransactionTypeConverter().ToEnumValue(line.AH_TransactionType), lineInfo.TransactionType);
						AssertEquals("PostDate", line.PostDate.IsEmpty ? (ZDateTime?)null : line.PostDate, lineInfo.PostDate);
						AssertEquals("Number", line.AH_TransactionNum.IsEmpty ? null : (ZString?)line.AH_TransactionNum, lineInfo.Number);
						AssertEquals("TransactionReference", line.AH_TransactionReference.IsEmpty ? null : (ZString?)line.AH_TransactionReference, lineInfo.TransactionReference);
						AssertEquals("ComplianceSubType", line.AH_ComplianceSubType.IsEmpty ? null : (ZString?)line.AH_ComplianceSubType, lineInfo.ComplianceSubType);

						AssertNotNull("LocalCurrency", lineInfo.LocalCurrency);
						AssertEquals("LocalCurrency.Code", report.Company.LocalCurrency.RX_Code, lineInfo.LocalCurrency.Code);
						AssertEquals("LocalCurrency.Description", report.Company.LocalCurrency.RX_Desc, lineInfo.LocalCurrency.Description);

						AssertEquals("LocalExVATAmount", line.TotalExTaxAmount, lineInfo.LocalExVATAmount);
						AssertEquals("LocalVATAmount", line.TotalTaxAmount, lineInfo.LocalVATAmount);

						if (!line.OH_Code.IsEmpty)
						{
							var orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, line.OH_Code);
							AssertNotNull("Must be a valid Org", orgHeader);
							var officeAddress = orgHeader.Addresses.DefaultAddressOfType(OrgAddressType.Office);
							AssertNotNull("Must be a valid Office Address", officeAddress);

							AssertNotNull("OrganizationAddress", lineInfo.OrganizationAddress);
							AssertOrgAddress(officeAddress, lineInfo.OrganizationAddress);

							AssertNotNull("OrganizationAddress.GovRegNumType", lineInfo.OrganizationAddress.GovRegNumType);
							AssertEquals("OrganizationAddress.GovRegNumType.Code", report.TaxRegistrationType, lineInfo.OrganizationAddress.GovRegNumType.Code);
							AssertEquals("OrganizationAddress.GovRegNumType.Code", report.TaxRegistrationTypeList[report.TaxRegistrationType].Description, lineInfo.OrganizationAddress.GovRegNumType.Description);
							AssertEquals("OrganizationAddress.GovRegNum", line.OK_CustomsRegNo.IsEmpty ? null : (ZString?)line.OK_CustomsRegNo, lineInfo.OrganizationAddress.GovRegNum);
						}
						else
						{
							AssertNotNull("OrganizationAddress", lineInfo.OrganizationAddress);
						}
					}

					AssertNotNull("PostingJournalCollection", lineInfo.PostingJournalCollection);
					Assert("lineIndex <= PostingJournalCollection.Count", lineIndex <= lineInfo.PostingJournalCollection.Count);

					if (line.ACL_ReportSequence != reportLineSequence)
					{
						reportLineSequence = line.ACL_ReportSequence;
						lineSubInfo = lineInfo.PostingJournalCollection[lineIndex];
						lineIndex++;
						lineDetailIndex = 0;
					}

					var lineSubInfoDetail = isDayBookWithoutGrouping ? lineSubInfo.PostingJournalDetailCollection[lineDetailIndex] : null;
					lineDetailIndex++;

					if (isDayBook)
					{
						AssertGLPostingJournal(report, line, lineSubInfo);
					}
					else
					{
						var lineType = report.GoodsServiceType;
						if (isDayBookWithoutGrouping)
						{
							AssertGLPostingJournalAndDetail(report, line, lineSubInfo, lineSubInfoDetail);
							lineType = Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;
						}
						AssertPostingJournal(report, line, lineSubInfo, lineType);
					}
				}
			});
		}

		void AssertOrgAddress(OrgAddress officeAddress, UniversalOrgAddress orgAddress)
		{
			AssertEquals("AddressType", OrgAddressType.Office.ToString(), orgAddress.AddressType);
			AssertEquals("AddressOverride", false, orgAddress.AddressOverride);
			AssertEquals("OrganizationCode", officeAddress.Header.OH_Code, orgAddress.OrganizationCode);
			AssertEquals("AddressShortCode", officeAddress.OA_Code, orgAddress.AddressShortCode);

			if (!officeAddress.OA_RL_NKRelatedPortCode.IsEmpty)
			{
				AssertNotNull("Port", orgAddress.Port);
				AssertEquals("Port.Code", officeAddress.OA_RL_NKRelatedPortCode, orgAddress.Port.Code);
				AssertEquals("Port.Name", officeAddress.PortName, orgAddress.Port.Name);
			}
			else
			{
				AssertNull("Port", orgAddress.Port);
			}

			ZString? companyNameOverride = officeAddress.OA_CompanyNameOverride.IsEmpty ? null : officeAddress.OA_CompanyNameOverride;
			AssertEquals("orgAddress.CompanyName", companyNameOverride.HasValue && !companyNameOverride.Value.IsEmpty ? companyNameOverride : officeAddress.Header.OH_FullName, orgAddress.CompanyName);

			if (!officeAddress.OA_RN_NKCountryCode.IsEmpty)
			{
				AssertNotNull("Country", orgAddress.Country);
				AssertEquals("Country.Code", officeAddress.OA_RN_NKCountryCode, orgAddress.Country.Code);
				AssertEquals("Country.Name", officeAddress.Country != null ? officeAddress.Country.RN_Desc : null, orgAddress.Country.Name);
			}
			else
			{
				AssertNull("Country", orgAddress.Country);
			}

			if (!officeAddress.Header.OH_ScreeningStatus.IsEmpty)
			{
				AssertNotNull("ScreeningStatus", orgAddress.ScreeningStatus);
				AssertEquals("ScreeningStatus.Code", officeAddress.Header.OH_ScreeningStatus, orgAddress.ScreeningStatus.Code);
				AssertEquals("ScreeningStatus.Description", new ScreeningStatusesList().GetDescriptionFromCode(orgAddress.ScreeningStatus.Code), orgAddress.ScreeningStatus.Description);
			}
			else
			{
				AssertNull("ScreeningStatus", orgAddress.ScreeningStatus);
			}

			AssertEquals("AddressShortCode", officeAddress.OA_Code, orgAddress.AddressShortCode);
			AssertEquals("Address1", officeAddress.OA_Address1.IsEmpty ? null : (ZString?)officeAddress.OA_Address1, orgAddress.Address1);
			AssertEquals("Address2", officeAddress.OA_Address2.IsEmpty ? null : (ZString?)officeAddress.OA_Address2, orgAddress.Address2);
			AssertEquals("City", officeAddress.OA_City.IsEmpty ? null : (ZString?)officeAddress.OA_City, orgAddress.City);
			AssertEquals("Postcode", officeAddress.OA_PostCode.IsEmpty ? null : (ZString?)officeAddress.OA_PostCode, orgAddress.Postcode);
			AssertEquals("State", officeAddress.OA_State.IsEmpty ? null : (ZString?)officeAddress.OA_State, orgAddress.State);

			AssertEquals("Email", officeAddress.OA_Email.IsEmpty ? null : (ZString?)officeAddress.OA_Email, orgAddress.Email);
			AssertEquals("Fax", officeAddress.OA_Fax.IsEmpty ? null : (ZString?)officeAddress.OA_Fax, orgAddress.Fax);
			AssertEquals("Phone", officeAddress.OA_Phone.IsEmpty ? null : (ZString?)officeAddress.OA_Phone, orgAddress.Phone);

			if (officeAddress.Header.CustomsCodes.Any())
			{
				AssertNotNull("RegistrationNumberCollection", orgAddress.RegistrationNumberCollection);
				AssertEquals("TransactionCollection.Count", officeAddress.Header.CustomsCodes.Count, orgAddress.RegistrationNumberCollection.Count);

				for (int i = 0; i < officeAddress.Header.CustomsCodes.Count; i++)
				{
					var regNo = officeAddress.Header.CustomsCodes[i];
					var regNoInfo = orgAddress.RegistrationNumberCollection[i];

					AssertNotNull("CountryOfIssue", regNoInfo.CountryOfIssue);
					AssertEquals("CountryOfIssue.Code", regNo.OK_RN_NKCodeCountry.IsEmpty ? null : (ZString?)regNo.OK_RN_NKCodeCountry, regNoInfo.CountryOfIssue.Code);
					AssertEquals("CountryOfIssue.Name", regNo.CodeCountry.RN_Desc.IsEmpty ? null : (ZString?)regNo.CodeCountry.RN_Desc, regNoInfo.CountryOfIssue.Name);

					AssertNotNull("Type", regNoInfo.Type);
					AssertEquals("Type.Code", regNo.OK_CodeType.IsEmpty ? null : (ZString?)regNo.OK_CodeType, regNoInfo.Type.Code);
					AssertEquals("Type.Description", regNo.CustomsRegNoFieldType.IsEmpty ? null : (ZString?)regNo.CustomsRegNoFieldType, regNoInfo.Type.Description);

					AssertEquals("Value", regNo.OK_CustomsRegNo.IsEmpty ? null : (ZString?)regNo.OK_CustomsRegNo, regNoInfo.Value);
				}
			}
		}

		void AssertPostingJournal(AccComplianceReport report, AccComplianceReportLine line, PostingJournal lineSubInfo, ZString lineTypeCode)
		{
			AssertPostingJournalCore(report, line, lineSubInfo);

			switch (lineTypeCode)
			{
				case Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping:
					AssertEquals("LocalAmount", line.GoodsExTaxAmount + line.ServiceExTaxAmount, lineSubInfo.LocalAmount);
					AssertEquals("LocalGSTVATAmount", line.GoodsTaxAmount + line.ServiceTaxAmount, lineSubInfo.LocalGSTVATAmount);
					AssertEquals("TransactionCategory", Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping, lineSubInfo.TransactionCategory);
					break;
				case Lookups.GoodsServiceTypeCodes.GoodsOnly:
					AssertEquals("LocalAmount", line.GoodsExTaxAmount, lineSubInfo.LocalAmount);
					AssertEquals("LocalGSTVATAmount", line.GoodsTaxAmount, lineSubInfo.LocalGSTVATAmount);
					AssertEquals("TransactionCategory", Lookups.GoodsServiceTypeCodes.GoodsOnly, lineSubInfo.TransactionCategory);
					break;
				case Lookups.GoodsServiceTypeCodes.ServiceOnly:
					AssertEquals("LocalAmount", line.ServiceExTaxAmount, lineSubInfo.LocalAmount);
					AssertEquals("LocalGSTVATAmount", line.ServiceTaxAmount, lineSubInfo.LocalGSTVATAmount);
					AssertEquals("TransactionCategory", Lookups.GoodsServiceTypeCodes.ServiceOnly, lineSubInfo.TransactionCategory);
					break;
				default:
					break;
			}
		}

		void AssertGLPostingJournal(AccComplianceReport report, AccComplianceReportLine line, PostingJournal lineSubInfo)
		{
			AssertPostingJournalCore(report, line, lineSubInfo);

			AssertEquals("LocalGSTVATAmount.HasValue", false, lineSubInfo.LocalGSTVATAmount.HasValue);

			var groupByList = new ZString[] { Lookups.ReportLineGroupingListCodes.DayBook };
			AssertCollectionContains("TransactionCategory", lineSubInfo.TransactionCategory, groupByList);

			AssertNotNull("GLAccount", lineSubInfo.GLAccount);
			AssertEquals("GLAccount.AccountCode", line.AG_AccountNum, lineSubInfo.GLAccount.AccountCode);
			AssertEquals("GLAccount.Description", line.AG_Description, lineSubInfo.GLAccount.Description);

			AssertEquals("GLPostDate", line.PostDate, lineSubInfo.GLPostDate);

			if (!line.GB_Code.IsEmpty)
			{
				AssertNotNull("Branch", lineSubInfo.Branch);
				AssertEquals("Branch.Code", line.GB_Code, lineSubInfo.Branch.Code);
			}
			if (!line.GE_Code.IsEmpty)
			{
				AssertNotNull("Department", lineSubInfo.Department);
				AssertEquals("Department.Code", line.GE_Code, lineSubInfo.Department.Code);
			}

			if (!line.GeneralLedgerAmountCR.IsEmpty)
			{
				AssertEquals("LocalAmount", line.GeneralLedgerAmountCR, lineSubInfo.LocalAmount);
			}
			else
			{
				AssertEquals("LocalAmount", line.GeneralLedgerAmountDR, lineSubInfo.LocalAmount);
			}
		}

		void AssertGLPostingJournalAndDetail(AccComplianceReport report, AccComplianceReportLine line, PostingJournal lineSubInfo, PostingJournalDetail lineDetail)
		{
			AssertPostingJournalCore(report, line, lineSubInfo);

			var groupByList = new ZString[] { Lookups.ReportLineGroupingListCodes.DayBookWithoutGrouping };
			AssertCollectionContains("TransactionCategory", lineSubInfo.TransactionCategory, groupByList);

			if (!line.GB_Code.IsEmpty)
			{
				AssertNotNull("Branch", lineSubInfo.Branch);
				AssertEquals("Branch.Code", line.GB_Code, lineSubInfo.Branch.Code);
			}
			if (!line.GE_Code.IsEmpty)
			{
				AssertNotNull("Department", lineSubInfo.Department);
				AssertEquals("Department.Code", line.GE_Code, lineSubInfo.Department.Code);
			}

			if (!line.GeneralLedgerAmountCR.IsEmpty)
			{
				AssertNotNull("CreditGLAccount", lineDetail.CreditGLAccount);
				AssertEquals("CreditGLAccount.AccountCode", line.AG_AccountNum, lineDetail.CreditGLAccount.AccountCode);
				AssertEquals("CreditGLAccount.Description", line.AG_Description, lineDetail.CreditGLAccount.Description);

				AssertEquals("PostingDate", line.PostDate, lineDetail.PostingDate);
				AssertEquals("PostingAmount", line.GeneralLedgerAmountCR, lineDetail.PostingAmount);
			}
			else
			{
				AssertNotNull("DebitGLAccount", lineDetail.DebitGLAccount);
				AssertEquals("DebitGLAccount.AccountCode", line.AG_AccountNum, lineDetail.DebitGLAccount.AccountCode);
				AssertEquals("DebitGLAccount.Description", line.AG_Description, lineDetail.DebitGLAccount.Description);

				AssertEquals("PostingDate", line.PostDate, lineDetail.PostingDate);
				AssertEquals("PostingAmount", line.GeneralLedgerAmountDR, lineDetail.PostingAmount);
			}
		}

		void AssertPostingJournalCore(AccComplianceReport report, AccComplianceReportLine line, PostingJournal lineSubInfo)
		{
			AssertNotNull("ChargeCurrency", lineSubInfo.ChargeCurrency);
			AssertEquals("LocalCurrency.Code", report.Company.LocalCurrency.RX_Code, lineSubInfo.ChargeCurrency.Code);
			AssertEquals("LocalCurrency.Description", report.Company.LocalCurrency.RX_Desc, lineSubInfo.ChargeCurrency.Description);

			if (!line.OH_Code.IsEmpty)
			{
				AssertNotNull("Organization", lineSubInfo.Organization);
				AssertEquals("Organization.Key", line.OH_Code, lineSubInfo.Organization.Key);
				AssertEquals("Organization.Tupe", nameof(DataContextType.Organization), lineSubInfo.Organization.Type);
			}
			else
			{
				AssertNull("Organization", lineSubInfo.Organization);
			}

			if (!line.AT_Code.IsEmpty)
			{
				AssertNotNull("VATTaxID", lineSubInfo.VATTaxID);
				AssertEquals("VATTaxID.TaxCode", line.AT_Code, lineSubInfo.VATTaxID.TaxCode);
			}
			else
			{
				AssertNull("VATTaxID", lineSubInfo.VATTaxID);
			}

			if (!line.TaxMessage.IsEmpty)
			{
				AssertNotNull("TaxMessageID", lineSubInfo.TaxMessageID);
				AssertEquals("TaxMessageID.TaxMessageCode", line.TaxMessage, lineSubInfo.TaxMessageID.TaxMessageCode);
			}
			else
			{
				AssertNull("TaxMessageID", lineSubInfo.TaxMessageID);
			}
		}

		#region Implementation

		TestObjectCreator Creator
		{
			get { return creator ?? (creator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator creator;

		#endregion
	}
}
