using System;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using System.Linq;
	using System.Net;
	using CargoWise.Common.JSON.Extensions;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ComplianceReport.HMRC;
	using Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing;
	using Enterprise.Accounting.Business.Testing;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;
	using NUnit.Framework;
	using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

	internal class AccComplianceReportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestACR_GC_Company()
		{
			Assert(Report.ACR_GC_Company.IsValid);
			AssertNoErrors("Default Value is valid (CurrentCompany)", Report.ACR_GC_CompanyInfo);

			Report.ACR_GC_Company = ZGuid.Empty;
			AssertHasErrors("Is compulsory", Report.ACR_GC_CompanyInfo);

			Report.ACR_GC_Company = ZGuid.Invalid;
			AssertHasErrors("Should be valid Company PK", Report.ACR_GC_CompanyInfo);
		}

		public void TestACR_ReportType()
		{
			Assert("Default Value is empty", Report.ACR_ReportType.IsEmpty);
			AssertHasErrors("Is mandatory", Report.ACR_ReportTypeInfo);

			Report.ACR_ReportType = "ABC";
			AssertHasErrors("Should have same code configuration in the Registry", Report.ACR_ReportTypeInfo);

			AddReportConfiguration("BCD");
			Report.ACR_ReportType = "BCD";
			AssertNoErrors("Has same code configuration in the Registry", Report.ACR_ReportTypeInfo);

			var anotherReport = Factory.NewWithValidTestData<AccComplianceReport>();
			AddReportConfiguration(anotherReport.ACR_ReportType);

			Report.ACR_DateFrom = anotherReport.ACR_DateFrom.AddDays(-10);
			Report.ACR_DateTo = anotherReport.ACR_DateTo.AddDays(1);
			Report.ACR_ReportType = anotherReport.ACR_ReportType;
			AssertHasErrors("Should have error as the same code report has overlapping date range", Report.ACR_ReportTypeInfo);

			Report.ACR_ReportType = "BCD";
			AssertNoErrors("Different code with valid configuration in the Registry", Report.ACR_ReportTypeInfo);

			Report.ACR_DateTo = anotherReport.ACR_DateFrom.AddDays(-1);
			Report.ACR_ReportType = anotherReport.ACR_ReportType;
			AssertNoErrors("Should have no error as the same code report has non-overlapping date range", Report.ACR_ReportTypeInfo);

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TST";
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.TaxReporting;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
			reportConfig.IncludeQueuedForPreviousPeriod = false;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			var previousReport = Factory.NewWithValidTestData<AccComplianceReport>();
			previousReport.ACR_ReportType = "TST";
			previousReport.ACR_DateFrom = new ZDate(2018, 4, 1);
			previousReport.ACR_DateTo = new ZDate(2018, 4, 30);
			previousReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			Report.ACR_DateFrom = new ZDate(2018, 5, 1);
			Report.ACR_DateTo = new ZDate(2018, 5, 31);
			Report.ACR_ReportType = "TST";

			previousReport.ACR_Status = AccComplianceReport.Status.ReportFinalised;
			Report.ACR_DateFrom = new ZDate(2018, 5, 1);
			Report.ACR_DateTo = new ZDate(2018, 5, 31);
			Report.ACR_ReportType = "TST";

			Assert(!Report.IncludeQueuedForPreviousPeriod);
			Assert(Report.IsPreviousReportFinalized());
			AssertNoErrors(Report.ACR_ReportTypeInfo);

			reportConfig.IncludeQueuedForPreviousPeriod = true;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			Report.ACR_ReportType = "TST";

			Assert(Report.IncludeQueuedForPreviousPeriod);
			Assert(Report.IsPreviousReportFinalized());
			AssertNoErrors(Report.ACR_ReportTypeInfo);

			previousReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			Report.ACR_ReportType = "TST";

			Assert(Report.IncludeQueuedForPreviousPeriod);
			Assert(!Report.IsPreviousReportFinalized());
			AssertHasErrors("You cannot create a new report until the previous report is finalized.", Report.ACR_ReportTypeInfo);

			reportConfig.IncludeQueuedForPreviousPeriod = false;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
			Report.ACR_ReportType = "TST";

			Assert(!Report.IncludeQueuedForPreviousPeriod);
			Assert(!Report.IsPreviousReportFinalized());
			AssertNoErrors(Report.ACR_ReportTypeInfo);
		}

		public void TestACR_DateFrom()
		{
			Assert("Default Value is empty", Report.ACR_DateFrom.IsEmpty);
			AssertHasError(Report.ACR_DateFromInfo, "Please enter a Date From.");

			Report.ACR_DateTo = ZDate.Today;

			Report.ACR_DateFrom = ZDate.Today.AddDays(-1);
			AssertNoErrors(Report.ACR_DateFromInfo);

			Report.ACR_DateFrom = ZDate.Today;
			AssertNoErrors(Report.ACR_DateFromInfo);

			Report.ACR_DateFrom = ZDate.Today.AddDays(1);
			AssertHasError(Report.ACR_DateFromInfo, "The 'Date From' must be before or the same as the 'Date To'.");

			Report.ACR_ReportType = "TSN";
			Report.ACR_DateFrom = new ZDate(2020, 2, 2);
			Report.ACR_DateTo = new ZDate(2020, 2, 28);
			AssertHasError(Report.ACR_DateFromInfo, "The 'Date From' must be 'Start Date' of an Accounting Period.");

			var helper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			helper.SetupSinglePeriod(201802, new ZDateTime(2018, 2, 1), new ZDateTime(2018, 2, 28));

			Report.ACR_DateFrom = new ZDate(2018, 2, 2);
			Report.ACR_DateTo = new ZDate(2018, 2, 28);
			AssertHasError(Report.ACR_DateFromInfo, "The 'Date From' must be 'Start Date' of an Accounting Period.");

			UpdateReportConfiguration("BCD");
			Report.ACR_Periodicity = ReportPeriodicityCodes.RangeAccountingPeriod;

			Report.ACR_DateFrom = new ZDate(2018, 2, 5);
			Report.ACR_DateTo = new ZDate(2018, 2, 28);
			AssertNotEquals("ReportBaseTablePrefix is not ADH", ReportBaseTablePrefixListCodes.ComplianceDocumentHeader, Report.ReportBaseTablePrefix);
			AssertHasError(Report.ACR_DateFromInfo, "The 'Date From' must be 'Start Date' of an Accounting Period.");

			Report.ACR_DateFrom = new ZDate(2018, 2, 1);
			AssertNoErrors(Report.ACR_DateFromInfo);

			var reportConfigurations = new ComplianceReportConfigurationCollection();
			var reportConfig = reportConfigurations.AddNew();
			reportConfig.ReportCode = "TSN";
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "APC";
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.GeneralLedgerData;

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, reportConfigurations))
			{
				var testDate = new ZDate(2018, 2, 1);

				AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetTemporaryValue(Report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, testDate.AddDays(-1).ToDateTime());
				Report.ACR_DateFrom = testDate;
				AssertNoErrors(Report.ACR_DateFromInfo);

				AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetTemporaryValue(Report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, testDate.AddDays(1).ToDateTime());
				Report.ACR_DateFrom = testDate;
				AssertHasError(Report.ACR_DateFromInfo, "The Date From should be equal to or later than the date of the 'Journal Entries Last Processed Date' registry. Please ensure journal entries for all accounting transactions posted within the compliance reporting periods have been generated.");
			}
		}

		public void TestACR_DateTo()
		{
			Assert("Default Value is empty", Report.ACR_DateTo.IsEmpty);
			AssertHasError(Report.ACR_DateToInfo, "Please enter a Date To.");

			Report.ACR_DateFrom = ZDate.Today;

			Report.ACR_DateTo = ZDate.Today.AddDays(1);
			AssertNoErrors(Report.ACR_DateToInfo);

			Report.ACR_DateTo = ZDate.Today;
			AssertNoErrors(Report.ACR_DateToInfo);

			Report.ACR_DateTo = ZDate.Today.AddDays(-1);
			AssertHasError(Report.ACR_DateToInfo, String.Format("The 'Date To' must be after '{0}'.", ZDateTime.Today.ToLongTimeString()));

			Report.ACR_ReportType = "TSN";
			Report.ACR_DateTo = new ZDate(2020, 3, 22);
			Report.ACR_DateFrom = new ZDate(2020, 3, 1);
			AssertHasError(Report.ACR_DateToInfo, "The 'Date To' must be 'End Date' of an Accounting Period.");

			var helper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			helper.SetupSinglePeriod(201802, new ZDateTime(2018, 2, 1), new ZDateTime(2018, 2, 28));

			Report.ACR_DateFrom = new ZDate(2018, 2, 1);
			Report.ACR_DateTo = new ZDate(2018, 2, 22);
			AssertHasError(Report.ACR_DateToInfo, "The 'Date To' must be 'End Date' of an Accounting Period.");

			UpdateReportConfiguration("BCD");
			Report.ACR_Periodicity = ReportPeriodicityCodes.RangeAccountingPeriod;

			Report.ACR_DateFrom = new ZDate(2018, 2, 1);
			Report.ACR_DateTo = new ZDate(2018, 2, 25);
			AssertNotEquals("ReportBaseTablePrefix is not ADH", ReportBaseTablePrefixListCodes.ComplianceDocumentHeader, Report.ReportBaseTablePrefix);
			var message = Report.ACR_DateToInfo.Notifications;
			AssertHasError(Report.ACR_DateToInfo, "The 'Date To' must be 'End Date' of an Accounting Period.");

			Report.ACR_DateTo = new ZDate(2018, 2, 28);
			AssertNoErrors(Report.ACR_DateToInfo);
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;

		readonly ZDate DateFrom = new ZDate(2017, 05, 01);
		readonly ZDate DateTo = new ZDate(2017, 05, 31);

		[TestDate(2017, 6, 1)]
		public void TestValidHMRCDateRange()
		{
			Report.FillWithValidTestData();

			var helper = new MTDTestHelper();
			var clientHandlerMock = helper.MTDHttpClientHandler;
			helper.SetupMockHttpClientForMTD();
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				SetupReportAndConfiguration(Report, ObjectCreator);
				var client = MTDTestHelper.SetupClient(Report, clientHandlerMock);
				var expectedEndPoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={DateFrom.ToMTDCompliantFormat()}&to={DateTo.ToMTDCompliantFormat()}");

				var responseObligation = new MTDObligations()
				{
					obligations = new MTDObligation[]
					{
						new MTDObligation()
						{
							start = "2017-05-01",
							end = "2017-05-31",
							due = "2017-06-07",
							status = "O",
							periodKey = "18AD"
						}
					}
				};

				clientHandlerMock.AddJsonResponse(
					expectedEndPoint
					, System.Net.HttpStatusCode.OK
					, responseObligation.ToJSON());

				Report.ACR_DateFrom = DateFrom;
				Report.ACR_DateTo = DateTo;

				AssertNoErrors(Report.ACR_DateFromInfo);
				AssertNoErrors(Report.ACR_DateToInfo);

				Factory.Save();

				Assert(Report.IsInDatabase);
				AssertNoErrors(Report.ACR_DateFromInfo);
				AssertNoErrors(Report.ACR_DateToInfo);
			}
		}

		[TestDate(2017, 6, 1)]
		public void TestInvalidHMRCDateRangeWithoutMatchingObligation()
		{
			var responseObligation = new MTDObligations()
			{
				obligations = Array.Empty<MTDObligation>()
			};

			var expectedError = "The dates entered are not eligible for submission of a VAT return. They should be as per your open VAT reporting period.";

			AssertInvalidHMRCDateRange(responseObligation, expectedError);
		}

		[TestDate(2017, 6, 1)]
		public void TestInvalidHMRCDateRangeWithFulfilledMatchingObligation()
		{
			var responseObligation = new MTDObligations()
			{
				obligations = new MTDObligation[]
					{
						new MTDObligation()
						{
							start = "2017-05-01",
							end = "2017-05-31",
							due = "2017-06-07",
							status = "F",
							periodKey = "18AD"
						}
					}
			};

			var expectedError = "The dates entered are not eligible for submission of a VAT return. They should be as per your open VAT reporting period.";

			AssertInvalidHMRCDateRange(responseObligation, expectedError);
		}

		[TestDate(2017, 6, 1)]
		public void TestInvalidHMRCDateRangeWithMissingHRMCAuthorizationCode()
		{
			var responseObligation = new MTDObligations()
			{
				obligations = new MTDObligation[]
					{
						new MTDObligation()
						{
							start = "2017-05-01",
							end = "2017-05-31",
							due = "2017-06-07",
							status = "O",
							periodKey = "18AD"
						}
					}
			};

			var expectedError = "Error Code: No Authorization Code, Message: Unable to obtain an HMRC MTD Authorisation Code., Request path: \r\n";

			AssertInvalidHMRCDateRange(responseObligation, expectedError, false);
		}

		[TestDate(2017, 6, 1)]
		public void TestInvalidHMRCDateRangeWithLongDateRange()
		{
			var responseError = new MTDErrorInfo()
			{
				code = "INVALID_DATE_RANGE",
				message = "Invalid date range"
			};

			var expectedError = "The dates entered are not eligible for submission of a VAT return. They should be as per your open VAT reporting period.";

			AssertInvalidHMRCDateRangeWithError(responseError, expectedError, HttpStatusCode.BadRequest);
		}

		[TestDate(2017, 6, 1)]
		public void TestInvalidHMRCDateRangeWithInvalidVRN()
		{
			var responseError = new MTDErrorInfo()
			{
				code = "VRN_INVALID",
				message = "vrn invalid"
			};

			var expectedError = "The entered credentials may be for an incorrect VRN or CW1 was not granted access. Please try again.";

			AssertInvalidHMRCDateRangeWithError(responseError, expectedError, HttpStatusCode.BadRequest);
		}

		[TestDate(2017, 6, 1)]
		public void TestInvalidHMRCDateRangeWithNotFoundError()
		{
			var responseError = new MTDErrorInfo()
			{
				code = "NOT_FOUND",
				message = "The remote endpoint has indicated that no associated data is found"
			};

			var expectedError = responseError.ToString();

			AssertInvalidHMRCDateRangeWithError(responseError, expectedError, HttpStatusCode.NotFound);
		}

		void AssertInvalidHMRCDateRange(MTDObligations responseObligation, string expectedError, bool withAuthorizationCode = true)
		{
			Report.FillWithValidTestData();

			var helper = new MTDTestHelper();
			var clientHandlerMock = helper.MTDHttpClientHandler;
			helper.SetupMockHttpClientForMTD();

			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				SetupReportAndConfiguration(Report, ObjectCreator, withAuthorizationCode);
				var client = MTDTestHelper.SetupClient(Report, clientHandlerMock);
				var expectedEndPoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={DateFrom.ToMTDCompliantFormat()}&to={DateTo.ToMTDCompliantFormat()}");

				clientHandlerMock.AddJsonResponse(
					expectedEndPoint
					, System.Net.HttpStatusCode.OK
					, responseObligation.ToJSON());

				Report.ACR_DateFrom = DateFrom;
				Report.ACR_DateTo = DateTo;

				AssertNoErrors(Report.ACR_DateFromInfo);
				AssertNoErrors(Report.ACR_DateToInfo);

				Report.RunPreSaveValidation();

				AssertHasError(Report.ACR_DateFromInfo, expectedError);
				AssertHasError(Report.ACR_DateToInfo, expectedError);
			}
		}

		void AssertInvalidHMRCDateRangeWithError(MTDErrorInfo responseErrorInfo, string expectedError, HttpStatusCode statusCode)
		{
			Report.FillWithValidTestData();

			var helper = new MTDTestHelper();
			var clientHandlerMock = helper.MTDHttpClientHandler;
			helper.SetupMockHttpClientForMTD();

			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				SetupReportAndConfiguration(Report, ObjectCreator, true);
				var client = MTDTestHelper.SetupClient(Report, clientHandlerMock);
				var expectedEndPoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={DateFrom.ToMTDCompliantFormat()}&to={DateTo.ToMTDCompliantFormat()}");

				clientHandlerMock.AddJsonResponse(
					expectedEndPoint
					, statusCode
					, responseErrorInfo.ToJSON());

				Report.ACR_DateFrom = DateFrom;
				Report.ACR_DateTo = DateTo;

				AssertNoErrors(Report.ACR_DateFromInfo);
				AssertNoErrors(Report.ACR_DateToInfo);

				Report.RunPreSaveValidation();

				AssertHasError(Report.ACR_DateFromInfo, expectedError);
				AssertHasError(Report.ACR_DateToInfo, expectedError);
			}
		}

		[TestDate(2017, 6, 1)]
		public void TestInvalidHMRCDateRangeWithHRMCServiceNotFound()
		{
			Report.FillWithValidTestData();

			var helper = new MTDTestHelper();
			var clientHandlerMock = helper.MTDHttpClientHandler;
			helper.SetupMockHttpClientForMTD();

			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				SetupReportAndConfiguration(Report, ObjectCreator);
				var client = MTDTestHelper.SetupClient(Report, clientHandlerMock);
				var expectedEndPoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={DateFrom.ToMTDCompliantFormat()}&to={DateTo.ToMTDCompliantFormat()}");

				var responseObligation = new MTDObligations()
				{
					obligations = Array.Empty<MTDObligation>()
				};

				clientHandlerMock.AddJsonResponse(
					expectedEndPoint
					, System.Net.HttpStatusCode.NotFound
					, responseObligation.ToJSON());

				Report.ACR_DateFrom = DateFrom;
				Report.ACR_DateTo = DateTo;

				AssertNoErrors(Report.ACR_DateFromInfo);
				AssertNoErrors(Report.ACR_DateToInfo);

				Report.RunPreSaveValidation();

				var expectedError = "Error Code: , Message: , Request path: \r\n";
				AssertHasError(Report.ACR_DateFromInfo, expectedError);
				AssertHasError(Report.ACR_DateToInfo, expectedError);
			}
		}

		public void TestConsumptionTaxGroupReportingCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Report.FillWithValidTestData();
				ObjectCreator.CreateTestPeriods(ZDateTime.Today);
				MTDSubmissionDataTestEnvironmentCreator.CreateCustomsCode(ObjectCreator);

				var groupCompany = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator, "UKG", "UKG");
				var memberCompany = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator, "UK1", "UK1");

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, groupCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					AssertEquals("Precondition: Current Company should be UKG", "UKG", Env.CurrentCompany.Code);
					var report = ObjectCreator.CreateComplianceReport("MTD", AccComplianceReport.Status.ReportFinalised);
					report.ACR_DateFrom = ZDate.Today;
					report.ACR_DateTo = ZDate.Today.AddDays(10);
					Factory.Save();
				}

				using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					AssertEquals("Precondition: Current Company should be UK1", "UK1", Env.CurrentCompany.Code);
					Report.ACR_GC_Company = Env.CurrentCompanyPK;
					Report.ACR_ReportType = "MTD";
					Report.ACR_DateFrom = ZDate.Today.AddDays(-20);
					Report.ACR_DateTo = ZDate.Today.AddDays(-10);

					AssertNoErrors(Report.ACR_DateFromInfo);
					AssertNoErrors(Report.ACR_DateToInfo);

					Report.RunPreSaveValidation();

					var expectedError = "The Start and End dates for the compliance report for a VAT group member must match the dates for the compliance report created in the VAT group reporting company.";
					AssertHasError(Report.ACR_DateFromInfo, expectedError);
					AssertHasError(Report.ACR_DateToInfo, expectedError);

					Report.ACR_DateFrom = ZDate.Today;
					Report.ACR_DateTo = ZDate.Today.AddDays(10);

					Report.RunPreSaveValidation();

					AssertNoErrors(Report.ACR_DateFromInfo);
					AssertNoErrors(Report.ACR_DateToInfo);

					Factory.Save();

					Assert(Report.IsInDatabase);
					AssertNoErrors(Report.ACR_DateFromInfo);
					AssertNoErrors(Report.ACR_DateToInfo);
				}
			}
		}

		[TestDate(2020, 6, 1)]
		public void TestAccountingPeriodValidation()
		{
			var creator = new TestObjectCreator(Factory);
			var reportConfigurations = new ComplianceReportConfigurationCollection();
			var reportConfig = reportConfigurations.AddNew();
			reportConfig.Country = Enterprise.Core.Constants.CountryCodes.Australia;
			reportConfig.ReportCode = "TPR";
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
			reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.TransactionPayments;
			reportConfig.ReportPeriodicity = ReportPeriodicityCodes.FinancialYear;
			reportConfig.TaxRegistrationType = reportConfig.Lookups.TaxRegistrationTypeList[0].Code;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Report.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, reportConfigurations))
			{
				creator.CreateTestPeriods(ZDateTime.Today);
				Report.ACR_ReportType = "TPR";

				Report.AccountingPeriod = 0;
				AssertHasError(Report.AccountingPeriodInfo, "Please enter a 'Financial Year' to set 'Date From' and 'Date To' values.");
				AssertHasError(Report.ACR_DateFromInfo, "Please enter a Date From.");
				AssertHasError(Report.ACR_DateToInfo, "Please enter a Date To.");

				Report.AccountingPeriod = 4000;
				AssertHasError(Report.AccountingPeriodInfo, "Period management does not have defined period for year 4000 or value is not valid.");
				AssertHasError(Report.ACR_DateFromInfo, "Please enter a Date From.");
				AssertHasError(Report.ACR_DateToInfo, "Please enter a Date To.");

				Report.AccountingPeriod = 2021;
				AssertNoErrors(Report.AccountingPeriodInfo);
				AssertNoErrors(Report.ACR_DateFromInfo);
				AssertNoErrors(Report.ACR_DateToInfo);

				Report.AccountingPeriod = 202005;
				AssertHasError(Report.AccountingPeriodInfo, "Period management does not have defined period for year 202005 or value is not valid.");
				AssertNoErrors(Report.ACR_DateFromInfo);
				AssertNoErrors(Report.ACR_DateToInfo);

				Report.ACR_Periodicity = ReportPeriodicityCodes.CalendarMonth;
				Report.AccountingPeriod = 202101;
				AssertNoErrors(Report.AccountingPeriodInfo);
				AssertNoErrors(Report.ACR_DateFromInfo);
				AssertNoErrors(Report.ACR_DateToInfo);

				Report.ACR_Periodicity = ReportPeriodicityCodes.FinancialYear;
				Report.AccountingPeriod = 5000;
				AssertHasError(Report.AccountingPeriodInfo, "Period management does not have defined period for year 5000 or value is not valid.");
				AssertHasError(Report.ACR_DateFromInfo, "Please enter a Date From.");
				AssertHasError(Report.ACR_DateToInfo, "Please enter a Date To.");
			}
		}

		#region ReportingBook

		#region CheckPeriod

		public void TestCheckPeriodWithReportingBook_NotPER_ShouldNotHaveThisError()
		{
			Report.ACR_Periodicity = ReportPeriodicityCodes.CalendarMonth;
			Report.ACR_DateFrom = new ZDate(2018, 5, 1);
			Report.ACR_DateTo = new ZDate(2018, 5, 31);
			Report.ACR_ReportType = "TST";

			Factory.Save();

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = "XXA";
			reportingBook.ARB_Description = "Test";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;

			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			reportingBook.ARB_GC_CompanyOfPeriod = testCompany.PK;

			var period1 = Factory.NewWithValidTestData<AccPeriodManagement>();
			period1.AM_StartDate = new ZDateTime(2021, 01, 01);
			period1.AM_EndDate = new ZDateTime(2021, 01, 31);
			period1.AM_Year = 2021;
			period1.AM_Period = 202101;
			period1.AM_GC_Company = testCompany.PK;

			var period2 = Factory.NewWithValidTestData<AccPeriodManagement>();
			period2.AM_StartDate = new ZDateTime(2020, 01, 01);
			period2.AM_EndDate = new ZDateTime(2020, 01, 31);
			period2.AM_Year = 2020;
			period2.AM_Period = 202001;
			period2.AM_GC_Company = Report.Company.PK;
			Factory.Save();

			Report.ACR_ARB_ReportingBook = reportingBook.PK;
			Report.AccountingPeriod = 201901;
			Report.Validation.ValidateAccountingPeriod();
			Assert(!Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));
			AssertNotEquals(ReportPeriodicityCodes.AccountingPeriod, Report.ACR_Periodicity);
			Assert("Periodicity is not PER should not have this error.", !Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{reportingBook.CompanyOfPeriod.GC_Code}' system company."));
		}

		public void TestCheckPeriodWithReportingBook_WithoutReportingBook_ShouldNotHaveThisError()
		{
			SetUpReportWithPER();

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = "XXA";
			reportingBook.ARB_Description = "Test";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;

			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			Report.AccountingPeriod = 2021;
			Report.Validation.ValidateAccountingPeriod();
			Assert(!Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));

			Report.AccountingPeriod = -32769;
			Report.Validation.ValidateAccountingPeriod();
			Assert(!Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));

			Report.AccountingPeriod = 32768;
			Report.Validation.ValidateAccountingPeriod();
			Assert(!Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));
		}

		public void TestCheckPeriodWithReportingBook_BothDoNotHavePeriod()
		{
			SetUpReportWithPER();

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = "XXA";
			reportingBook.ARB_Description = "Test";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;

			Factory.Save();

			Report.AccountingPeriod = 202101;
			Report.ACR_ARB_ReportingBook = reportingBook.PK;
			Report.Validation.ValidateAccountingPeriod();
			Assert("Reporting Book don't have period and AccountingPeriod is excluded by Compliance Report period.", Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));

			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			reportingBook.ARB_GC_CompanyOfPeriod = testCompany.PK;
			Report.ACR_ARB_ReportingBook = ZGuid.Empty;
			Factory.Save();
			Report.ACR_ARB_ReportingBook = reportingBook.PK;

			Report.AccountingPeriod = 202101;
			Report.Validation.ValidateAccountingPeriod();
			Assert("AccountingPeriod is excluded by Reporting Book period.", Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{reportingBook.CompanyOfPeriod.GC_Code}' system company."));
		}

		public void TestCheckPeriodWithReportingBook_OnlyHaveReportingBookPeriod()
		{
			SetUpReportWithPER();

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = "XXA";
			reportingBook.ARB_Description = "Test";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;

			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			reportingBook.ARB_GC_CompanyOfPeriod = testCompany.PK;

			var period = Factory.NewWithValidTestData<AccPeriodManagement>();
			period.AM_StartDate = new ZDateTime(2021, 01, 01);
			period.AM_EndDate = new ZDateTime(2021, 01, 31);
			period.AM_Year = 2021;
			period.AM_Period = 202101;
			period.AM_GC_Company = testCompany.PK;
			Factory.Save();

			Report.ACR_ARB_ReportingBook = reportingBook.PK;

			Report.AccountingPeriod = 202101;
			Report.Validation.ValidateAccountingPeriod();
			Assert(!Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));
			Assert("AccountingPeriod is included by Reporting Book Period.", !Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{reportingBook.CompanyOfPeriod.GC_Code}' system company."));

			Report.AccountingPeriod = 202001;
			Report.Validation.ValidateAccountingPeriod();
			Assert(!Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));
			Assert("AccountingPeriod is excluded by Reporting Book Period.", Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{reportingBook.CompanyOfPeriod.GC_Code}' system company."));
		}

		public void TestCheckPeriodWithReportingBook_OnlyHaveComplianceReportPeriod()
		{
			SetUpReportWithPER();

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = "XXA";
			reportingBook.ARB_Description = "Test";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;

			var testCompany = Factory.NewWithValidTestData<GlbCompany>();

			var period = Factory.NewWithValidTestData<AccPeriodManagement>();
			period.AM_StartDate = new ZDateTime(2021, 01, 01);
			period.AM_EndDate = new ZDateTime(2021, 01, 31);
			period.AM_Year = 2021;
			period.AM_Period = 202101;
			period.AM_GC_Company = Report.Company.PK;
			Factory.Save();

			Report.ACR_ARB_ReportingBook = reportingBook.PK;

			Report.AccountingPeriod = 202101;
			Report.Validation.ValidateAccountingPeriod();
			Assert("AccountingPeriod is included by Compliance Report Period.", !Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));

			Report.AccountingPeriod = 202001;
			Report.Validation.ValidateAccountingPeriod();
			Assert("AccountingPeriod is excluded by Compliance Report Period.", Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));
		}

		public void TestCheckPeriodWithReportingBook_BothHavePeriod()
		{
			SetUpReportWithPER();

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = "XXA";
			reportingBook.ARB_Description = "Test";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;

			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			reportingBook.ARB_GC_CompanyOfPeriod = testCompany.PK;

			var period1 = Factory.NewWithValidTestData<AccPeriodManagement>();
			period1.AM_StartDate = new ZDateTime(2021, 01, 01);
			period1.AM_EndDate = new ZDateTime(2021, 01, 31);
			period1.AM_Year = 2021;
			period1.AM_Period = 202101;
			period1.AM_GC_Company = testCompany.PK;

			var period2 = Factory.NewWithValidTestData<AccPeriodManagement>();
			period2.AM_StartDate = new ZDateTime(2020, 01, 01);
			period2.AM_EndDate = new ZDateTime(2020, 01, 31);
			period2.AM_Year = 2020;
			period2.AM_Period = 202001;
			period2.AM_GC_Company = Report.Company.PK;
			Factory.Save();

			Report.ACR_ARB_ReportingBook = reportingBook.PK;

			Report.AccountingPeriod = 202101;
			Report.Validation.ValidateAccountingPeriod();
			Assert(!Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));
			Assert("AccountingPeriod is included by Reporting Book Period.", !Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{reportingBook.CompanyOfPeriod.GC_Code}' system company."));

			Report.AccountingPeriod = 202001;
			Report.Validation.ValidateAccountingPeriod();
			Assert(!Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));
			Assert("AccountingPeriod is excluded by Reporting Book Period.", Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{reportingBook.CompanyOfPeriod.GC_Code}' system company."));

			Report.AccountingPeriod = 201901;
			Report.Validation.ValidateAccountingPeriod();
			Assert(!Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{Report.Company.GC_Code}' system company."));
			Assert("AccountingPeriod is excluded by Reporting Book Period.", Report.AccountingPeriodInfo.HasError($"The Period value specified does not exist in '{reportingBook.CompanyOfPeriod.GC_Code}' system company."));
		}

		void SetUpReportWithPER()
		{
			Report.ACR_Periodicity = ReportPeriodicityCodes.AccountingPeriod;
			Report.ACR_DateFrom = new ZDate(2018, 5, 1);
			Report.ACR_DateTo = new ZDate(2018, 5, 31);
			Report.ACR_ReportType = "TST";

			Factory.Save();
		}

		#endregion

		public void TestCheckBackLog()
		{
			var creator = new AccountingTestDataCreator();
			creator.CreatePeriods(202201, new ZDateTime(2022, 01, 01), new ZDateTime(2022, 02, 01), false);
			Report.ACR_Periodicity = ReportPeriodicityCodes.CalendarMonth;
			Report.ACR_DateFrom = new ZDate(2018, 5, 1);
			Report.ACR_DateTo = new ZDate(2018, 5, 31);
			Report.ACR_ReportType = "TST";

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = "XXA";
			reportingBook.ARB_Description = "Test";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;

			Factory.Save();

			Report.Validation.ValidateACR_ARB_ReportingBook();
			Assert(!Report.ACR_ARB_ReportingBookInfo.HasError("Reporting Book can only be selected after all backlog accounting transactions have been processed."));

			Report.ACR_ARB_ReportingBook = reportingBook.PK;

			using (AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.MinValue))
			{
				Report.Validation.ValidateACR_ARB_ReportingBook();
				Assert("Add error when JournalEntriesLastProcessedDate is min date time.", Report.ACR_ARB_ReportingBookInfo.HasError("Reporting Book can only be selected after all backlog accounting transactions have been processed."));
			}

			using (AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2022, 01, 02)))
			{
				Report.Validation.ValidateACR_ARB_ReportingBook();
				Assert("Add error when JournalEntriesLastProcessedDate is bigger than first period start date.", Report.ACR_ARB_ReportingBookInfo.HasError("Reporting Book can only be selected after all backlog accounting transactions have been processed."));
			}

			using (AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2022, 01, 01)))
			{
				Report.Validation.ValidateACR_ARB_ReportingBook();
				Assert(!Report.ACR_ARB_ReportingBookInfo.HasError("Reporting Book can only be selected after all backlog accounting transactions have been processed."));
			}
		}

		public void TestCheckReportingBookCurrency()
		{
			Report.ACR_Periodicity = ReportPeriodicityCodes.CalendarMonth;
			Report.ACR_DateFrom = new ZDate(2018, 5, 1);
			Report.ACR_DateTo = new ZDate(2018, 5, 31);
			Report.ACR_ReportType = "TST";

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = "XXA";
			reportingBook.ARB_Description = "Test";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;
			reportingBook.ARB_RX_NKCurrency = "USD";

			Factory.Save();

			Report.Validation.ValidateACR_ARB_ReportingBook();
			Assert("Reporting Book in Local Reporting Currency.", !Report.ACR_ARB_ReportingBookInfo.HasError("Only Reporting Book in Local Reporting Currency can be selected."));

			Report.ACR_ARB_ReportingBook = reportingBook.PK;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "USD";
			Assert(reportingBook.IsLocalCurrency);

			Report.Validation.ValidateACR_ARB_ReportingBook();
			Assert("Reporting Book in Local Reporting Currency.", !Report.ACR_ARB_ReportingBookInfo.HasError("Only Reporting Book in Local Reporting Currency can be selected."));

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			Assert(!reportingBook.IsLocalCurrency);
			Report.Validation.ValidateACR_ARB_ReportingBook();
			Assert("Reporting Book not in Local Reporting Currency.", Report.ACR_ARB_ReportingBookInfo.HasError("Only Reporting Book in Local Reporting Currency can be selected."));
		}

		public void TestCheckReportingBookNonGlobal()
		{
			Report.ACR_Periodicity = ReportPeriodicityCodes.CalendarMonth;
			Report.ACR_DateFrom = new ZDate(2018, 5, 1);
			Report.ACR_DateTo = new ZDate(2018, 5, 31);
			Report.ACR_ReportType = "TST";

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = "XXA";
			reportingBook.ARB_Description = "Test";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;

			Factory.Save();

			Report.Validation.ValidateACR_ARB_ReportingBook();
			Assert("Non Reporting Book should not have this error message.", !Report.ACR_ARB_ReportingBookInfo.HasError("Only Non-Global Reporting Book can be selected."));

			Report.ACR_ARB_ReportingBook = reportingBook.PK;
			Report.Validation.ValidateACR_ARB_ReportingBook();
			Assert("Non-Global Reporting Book can be selected.", !Report.ACR_ARB_ReportingBookInfo.HasError("Only Non-Global Reporting Book can be selected."));

			Report.ReportingBook.ARB_IsGlobal = true;
			Report.Validation.ValidateACR_ARB_ReportingBook();
			Assert("Global Reporting Book can not be selected.", Report.ACR_ARB_ReportingBookInfo.HasError("Only Non-Global Reporting Book can be selected."));
		}

		public void TestCheckEDWServer()
		{
			Report.ACR_Periodicity = ReportPeriodicityCodes.CalendarMonth;
			Report.ACR_DateFrom = new ZDate(2018, 5, 1);
			Report.ACR_DateTo = new ZDate(2018, 5, 31);
			Report.ACR_ReportType = "TST";

			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_Code = "1";
			chart.AAC_IsGlobal = false;

			var reportingBook = Factory.New<AccReportingBook>();
			reportingBook.ARB_Code = "XXA";
			reportingBook.ARB_Description = "Test";
			reportingBook.ARB_AAC_AlternateChart = chart.PK;

			Factory.Save();

			Report.Validation.ValidateACR_ARB_ReportingBook();
			Assert("Have DataWareHouse server, should not have error message.", !Report.ACR_ARB_ReportingBookInfo.HasError("No Enterprise Data Warehouse server found. Please raise an eRequest."));

			Report.ACR_ARB_ReportingBook = reportingBook.PK;
			Report.Validation.ValidateACR_ARB_ReportingBook();
			Assert("Have DataWareHouse server, should not have error message.", !Report.ACR_ARB_ReportingBookInfo.HasError("No Enterprise Data Warehouse server found. Please raise an eRequest."));

			using (AccountingUtils.TemporarilySetDataWarehouseServerToNull())
			{
				Report.Validation.ValidateACR_ARB_ReportingBook();
				Assert("Don't have DataWareHouse server, should have error message.", Report.ACR_ARB_ReportingBookInfo.HasError("No Enterprise Data Warehouse server found. Please raise an eRequest."));
			}
		}

		#endregion

		AccComplianceReport SetupReportAndConfiguration(AccComplianceReport report, TestObjectCreator objectCreator, bool withAuthCode = true)
		{
			report.ACR_ReportType = "MTD";
			report.ACR_Periodicity = ReportPeriodicityCodes.CalendarMonth;
			report.ACR_GC_Company = Env.CurrentCompanyPK;
			if (withAuthCode)
			{
				report.OAuthClientAuthorisation += (o, e) => e.AuthorisationCode = MTDTestHelper.MockAuthorizationCode;
			}
			objectCreator.CreateConfigurationForComplianceReport(report, "AL");
			return report;
		}

		#region Implementation

		void AddReportConfiguration(string reportType)
		{
			var configs = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var config = configs.AddNew();
			config.Country = "AU";
			config.ReportCode = reportType;
			config.ReportBaseTablePrefix = "AL";
			config.ReportLineGrouping = "";
			config.ReportPeriodicity = "RNG";
			config.TaxRegistrationType = "ABN";

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configs);
		}
		void UpdateReportConfiguration(string reportType)
		{
			var configs = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var config = (ComplianceReportConfiguration)configs.First();
			config.Country = "AU";
			config.ReportCode = reportType;
			config.ReportBaseTablePrefix = "AL";
			config.ReportLineGrouping = "";
			config.ReportPeriodicity = "RNG";
			config.TaxRegistrationType = "ABN";

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configs);
		}

		AccComplianceReport Report;

		protected override void SetUp()
		{
			base.SetUp();
			Report = Factory.New<AccComplianceReport>();
			Report.Validation.ValidateAll();

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TSN";
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
			reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			reportConfig.TaxRegistrationType = "APC";
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
		}

		#endregion
	}
}

