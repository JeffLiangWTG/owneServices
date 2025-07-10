using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.JSON.Extensions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	public partial class MTDSubmissionDataColumnsTest
	{
		public void TestVATReturnSubmissionValidation_WhenReportIsNotFinalized()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = MTDTestHelper.SetupReportAndConfiguration(ObjectCreator, Factory, new ZDate(2017, 01, 01), new ZDate(2017, 03, 31));
				Factory.Save();
				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				var result = data.TryToSubmitVATDataToHMRC();
				AssertEquals("No Response Object", string.Empty, result.ResponseFromHMRC);
				AssertEquals("Error Message"
					, FormattableString.Invariant($"{report.ACR_Description} is in status: {report.ACR_Status}. Report must be in {AccComplianceReport.Status.ReportFinalised} status to allow submission of VAT data to HMRC")
					, result.ErrorMessage);
			}
		}

		public void TestVATReturnSubmissionValidation_WhenReportIsNotFinalized_GroupMemberCompany()
		{
			(var groupCompany, var memberCompany) = MTDSubmissionDataTestEnvironmentCreator.CreateUKGroupAndMemberCompany(Factory, "UKG", "UK1");

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = MTDTestHelper.SetupReportAndConfiguration(ObjectCreator, Factory, new ZDate(2017, 01, 01), new ZDate(2017, 03, 31));
				Factory.Save();
				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				var result = data.SubmitVATDataToGroup();
				AssertEquals("Success Message", string.Empty, result.SuccessMessage);
				AssertEquals("Error Message"
					, FormattableString.Invariant($"{report.ACR_Description} is in status: {report.ACR_Status}. Report must be in {AccComplianceReport.Status.ReportFinalised} status to allow submission of VAT data to Group")
					, result.ErrorMessage);
			}
		}

		public void TestVATReturnSubmissionValidation_NotInDatabase()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = MTDTestHelper.SetupReportAndConfiguration(ObjectCreator, Factory, new ZDate(2017, 01, 01), new ZDate(2017, 03, 31));

				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				var result = data.TryToSubmitVATDataToHMRC();
				AssertEquals("No Response Object", string.Empty, result.ResponseFromHMRC);
				AssertEquals("Error Message"
					, FormattableString.Invariant($"{report.ACR_Description} is in status: {report.ACR_Status}. Report must be in {AccComplianceReport.Status.ReportFinalised} status to allow submission of VAT data to HMRC")
					, result.ErrorMessage);
			}
		}

		public void TestVATReturnSubmissionValidation_NotInDatabase_GroupMemberCompany()
		{
			(var groupCompany, var memberCompany) = MTDSubmissionDataTestEnvironmentCreator.CreateUKGroupAndMemberCompany(Factory, "UKG", "UK1");

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = MTDTestHelper.SetupReportAndConfiguration(ObjectCreator, Factory, new ZDate(2017, 01, 01), new ZDate(2017, 03, 31));
				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				var result = data.SubmitVATDataToGroup();
				AssertEquals("Success Message", string.Empty, result.SuccessMessage);
				AssertEquals("Error Message"
					, FormattableString.Invariant($"{report.ACR_Description} is in status: {report.ACR_Status}. Report must be in {AccComplianceReport.Status.ReportFinalised} status to allow submission of VAT data to Group")
					, result.ErrorMessage);
			}
		}

		public void TestVATReturnSubmissionValidation_DeclarationIsNotTicked()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				data.Declaration = false;
				var result = data.TryToSubmitVATDataToHMRC();
				AssertEquals("No Response Object", string.Empty, result.ResponseFromHMRC);
				AssertEquals("Error Message"
					, "Please confirm declaration before continuing."
					, result.ErrorMessage);
			}
		}

		public void TestVATReturnSubmissionValidation_ValidateGroupMemberSubmission()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				var data = MTDTestHelper.CreateSubmissionData(Factory, report);

				var result = data.SubmitVATDataToGroup();
				AssertEquals("Success Message", string.Empty, result.SuccessMessage);
				AssertEquals("Error Message", "Cannot submit to the Group on a Group Head company.", result.ErrorMessage);
				AssertEquals(ErrorReporter.LastKeyReported, "MTDSubmissionDataColumns|ValidateGroupMemberSubmission");
				AssertEquals(ErrorReporter.LastMessageReported, "'ValidateGroupMemberSubmission' method should not be called for Group Head company.");
				ErrorReporter.Clear();
			}
		}

		public void TestVATReturnSubmissionValidation_ValidateSubmission()
		{
			(var groupCompany, var memberCompany) = MTDSubmissionDataTestEnvironmentCreator.CreateUKGroupAndMemberCompany(Factory, "UKG", "UK1");

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				var data = MTDTestHelper.CreateSubmissionData(Factory, report);

				var result = data.TryToSubmitVATDataToHMRC();
				AssertEquals("No Response Object", string.Empty, result.ResponseFromHMRC);
				AssertEquals("Error Message", "Cannot submit to the HMRC on a Group Member company.", result.ErrorMessage);
				AssertEquals(ErrorReporter.LastKeyReported, "MTDSubmissionDataColumns|ValidateSubmission");
				AssertEquals(ErrorReporter.LastMessageReported, "'ValidateSubmission' method should not be called for Group Member company.");
				ErrorReporter.Clear();
			}
		}

		public void TestCanBeSubmittedToHMRC_ReturnDueDate()
		{
			(var groupCompany, var memberCompany) = MTDSubmissionDataTestEnvironmentCreator.CreateUKGroupAndMemberCompany(Factory, "UKG", "UK1");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, groupCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				var submissionDataColumns = new MTDSubmissionDataColumns(Factory, report);

				AssertPreConditions(report);

				submissionDataColumns.ReturnDueDate = ZDate.Today;
				AssertEquals("CanBeSubmittedToHMRC should be true if date is valid for group company.", true, submissionDataColumns.CanBeSubmittedToHMRC);

				submissionDataColumns.ReturnDueDate = ZDate.Invalid;
				AssertEquals("CanBeSubmittedToHMRC should be false if date is invalid for group company.", false, submissionDataColumns.CanBeSubmittedToHMRC);
			}

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				var submissionDataColumns = new MTDSubmissionDataColumns(Factory, report);

				AssertPreConditions(report);

				submissionDataColumns.ReturnDueDate = ZDate.Today;
				AssertEquals("CanBeSubmittedToHMRC should be true if date is valid for member company.", true, submissionDataColumns.CanBeSubmittedToHMRC);

				submissionDataColumns.ReturnDueDate = ZDate.Invalid;
				AssertEquals("CanBeSubmittedToHMRC should be true if date is invalid for member company.", true, submissionDataColumns.CanBeSubmittedToHMRC);
			}

			void AssertPreConditions(AccComplianceReport report)
			{
				Assert("Pre-condition: Report should be in DB.", report.IsInDatabase);
				Assert("Pre-condition: Report should be in Finalise status.", report.ACR_IsFinalised);
			}
		}

		public void TestCanBeSubmittedToHMRC_HaveAllGroupMembersSubmittedReturn()
		{
			ObjectCreator.CreateTestPeriods(ZDateTime.Today);
			MTDSubmissionDataTestEnvironmentCreator.CreateCustomsCode(ObjectCreator);

			var groupCompany = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator, "UKG", "UKG");
			var memberCompanyUK1 = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator, "UK1", "UK1");
			var memberCompanyUK2 = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator, "UK2", "UK2");
			var independentCompany = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator, "UKI", "UKI");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, groupCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompanyUK1.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompanyUK2.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			{
				var fromDate = new ZDate(2017, 1, 1);
				var toDate = new ZDate(2017, 3, 31);

				var independentCompanyReport = CreateReport(independentCompany, fromDate, toDate);
				var independentCompanyDataColumns = CreateSubmissionDataColumns(Factory, independentCompanyReport);
				AssertEquals("For Independent Company, CanBeSubmittedToHMRC should be true as there is no member company setup in registry.", true, independentCompanyDataColumns.CanBeSubmittedToHMRC);

				var groupCompanyReport = CreateReport(groupCompany, fromDate, toDate);
				var groupDataColumns = CreateSubmissionDataColumns(Factory, groupCompanyReport);
				AssertEquals("CanBeSubmittedToHMRC should be false as there are two member companies setup in registry but there is no report.", false, groupDataColumns.CanBeSubmittedToHMRC);

				CreateAndSubmitReport(memberCompanyUK1, fromDate, toDate, "XYZ");
				CreateAndSubmitReport(memberCompanyUK2, fromDate, toDate, "XYZ");
				groupDataColumns = CreateSubmissionDataColumns(Factory, groupCompanyReport);
				AssertEquals("CanBeSubmittedToHMRC should be false as member company have finalized report and submitted but not for MTD report type.", false, groupDataColumns.CanBeSubmittedToHMRC);

				CreateAndSubmitReport(memberCompanyUK1, fromDate, toDate);
				groupDataColumns = CreateSubmissionDataColumns(Factory, groupCompanyReport);
				AssertEquals("CanBeSubmittedToHMRC should be false as member company UK2 has not yet created report.", false, groupDataColumns.CanBeSubmittedToHMRC);

				CreateAndSubmitReport(memberCompanyUK2, new ZDate(2017, 4, 1), new ZDate(2017, 6, 30));
				groupDataColumns = CreateSubmissionDataColumns(Factory, groupCompanyReport);
				AssertEquals("CanBeSubmittedToHMRC should be false as member company UK2 has created MTD report but for a different date period.", false, groupDataColumns.CanBeSubmittedToHMRC);

				var memberCompanyReport = CreateReport(memberCompanyUK2, fromDate, toDate);
				AssertEquals("CanBeSubmittedToHMRC should be false as member company UK2 has created MTD report but not yet submitted return.", false, groupDataColumns.CanBeSubmittedToHMRC);

				SubmitMemberCompanyReport(memberCompanyReport);
				groupDataColumns = CreateSubmissionDataColumns(Factory, groupCompanyReport);
				AssertEquals("CanBeSubmittedToHMRC should be true as all member company have submitted return to Group.", true, groupDataColumns.CanBeSubmittedToHMRC);
			}

			void CreateAndSubmitReport(GlbCompany glbCompany, ZDate fromDate, ZDate toDate, string reportType = "MTD")
			{
				SubmitMemberCompanyReport(CreateReport(glbCompany, fromDate, toDate, reportType));
			}

			AccComplianceReport CreateReport(GlbCompany glbCompany, ZDate fromDate, ZDate toDate, string reportType = "MTD")
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var report = Factory.New<AccComplianceReport>();
					report.FillWithValidTestData();
					report.ACR_DateFrom = fromDate;
					report.ACR_DateTo = toDate;
					report.ACR_ReportType = reportType;
					report.Factory.Save();
					report.Finalise();
					return report;
				}
			}

			void SubmitMemberCompanyReport(AccComplianceReport report)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, report.Company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var memberDataColumns = CreateSubmissionDataColumns(Factory, report);
					memberDataColumns.SubmitVATDataToGroup();
				}
			}

			MTDSubmissionDataColumns CreateSubmissionDataColumns(BusinessObjectFactory businessObjectFactory, AccComplianceReport report)
			{
				var submissionDataColumns = new MTDSubmissionDataColumns(businessObjectFactory, report);
				submissionDataColumns.ReturnDueDate = ZDate.Today;
				return submissionDataColumns;
			}
		}

		public void TestSuccessfulVATReturnSubmission()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();

				var client = MTDTestHelper.SetupClient(report, ClientHandlerMock);
				AddResponses();

				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				var result = data.TryToSubmitVATDataToHMRC();

				AssertEquals("Response", "VAT Return has been submitted successfully.", result.ResponseFromHMRC);
				AssertEquals("Error", string.Empty, result.ErrorMessage);

				var accTaxReturn = report.LoadAccTaxReturn();
				AssertNotNull("Tax Return has been saved", accTaxReturn);
				AssertEquals("5 x 9 = 45 columns should be saved", 45, accTaxReturn.Columns.Count);

				AssertEquals("ATR_ACR_ComplianceReport", report.PK, accTaxReturn.ATR_ACR_ComplianceReport);
				AssertEquals("ATR_GovtReturnIdentifier", "18AD", accTaxReturn.ATR_GovtReturnIdentifier);
				AssertEquals("ATR_GovtReceiptInformation", "e0606fe6233348119cf18023c0fb5271", accTaxReturn.ATR_GovtReceiptInformation);
				AssertEquals("ATR_Status", "SUB", accTaxReturn.ATR_Status);
				AssertEquals("ATR_Version", 2, accTaxReturn.ATR_Version);
				AssertEquals("ATR_CompanyName", company.GC_Name, accTaxReturn.ATR_CompanyName);
				AssertEquals("ATR_VATRegNo", MTDSubmissionDataTestEnvironmentCreator.MockVRN, accTaxReturn.ATR_VATRegNo);

				var computedByCW1Columns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "RPR").ToArray();
				AssertATRColumns(computedByCW1Columns, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);

				var unsubmittedPreviousPeriodColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ERR").ToArray();
				AssertATRColumns(unsubmittedPreviousPeriodColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var adjustedAmountColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ADJ").ToArray();
				AssertATRColumns(adjustedAmountColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var groupMemberTotalColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "GMC").ToArray();
				AssertATRColumns(groupMemberTotalColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var amountsToBeSubmittedToHMRC = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "SUB").ToArray();
				AssertATRColumns(amountsToBeSubmittedToHMRC, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);
			}

			void AddResponses()
			{
				var obligationsEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={new ZDate(2017, 01, 01).ToMTDCompliantFormat()}&to={new ZDate(2017, 03, 31).ToMTDCompliantFormat()}");
				var submitVATReturnEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/returns/");

				//Obligations Response
				var responseObligation = new MTDObligations()
				{
					obligations = new MTDObligation[]
					{
						new MTDObligation()
						{
							start = "2017-01-01",
							end = "2017-03-31",
							due = "2017-03-07",
							status = "O",
							periodKey = "18AD"
						},
					}
				};

				ClientHandlerMock.AddJsonResponse(
					obligationsEndpoint
					, System.Net.HttpStatusCode.OK
					, responseObligation.ToJSON());

				//VAT Return submit response
				var responseData = new MTDVATSubmitResponseContent()
				{
					processingDate = "2018-01-16T08:20:27.895+0000",
					paymentIndicator = "BANK",
					formBundleNumber = "256660290587",
					chargeRefNumber = "aCxFaNx0FZsCvyWF"
				};

				ClientHandlerMock.AddJsonResponse(
					submitVATReturnEndpoint
					, System.Net.HttpStatusCode.OK
					, responseData.ToJSON()
					, null
					, new KeyValuePair<string, IEnumerable<string>>[] { new KeyValuePair<string, IEnumerable<string>>("Receipt-ID", new string[] { "e0606fe6233348119cf18023c0fb5271" }) });
			}
		}

		public void TestSuccessfulVATReturnSubmission_GroupMemberCompany()
		{
			(var groupCompany, var memberCompany) = MTDSubmissionDataTestEnvironmentCreator.CreateUKGroupAndMemberCompany(Factory, "UKG", "UK1");

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				var result = data.SubmitVATDataToGroup();

				AssertEquals("Success Message", "VAT Return has been submitted successfully to Group.", result.SuccessMessage);
				AssertEquals("No Error Message", string.Empty, result.ErrorMessage);

				var accTaxReturn = report.LoadAccTaxReturn();
				AssertNotNull("Tax Return has been saved", accTaxReturn);
				AssertEquals("4 x 9 = 36 columns should be saved", 36, accTaxReturn.Columns.Count);

				AssertEquals("ATR_ACR_ComplianceReport", report.PK, accTaxReturn.ATR_ACR_ComplianceReport);
				AssertEquals("ATR_Status", "SUB", accTaxReturn.ATR_Status);
				AssertEquals("ATR_Version", 1, accTaxReturn.ATR_Version);
				AssertEquals("ATR_CompanyName", string.Empty, accTaxReturn.ATR_CompanyName);

				var computedByCW1Columns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "RPR").ToArray();
				AssertATRColumns(computedByCW1Columns, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);

				var unsubmittedPreviousPeriodColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ERR").ToArray();
				AssertATRColumns(unsubmittedPreviousPeriodColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var adjustedAmountColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ADJ").ToArray();
				AssertATRColumns(adjustedAmountColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var amountsToBeSubmittedToHMRC = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "SUB").ToArray();
				AssertATRColumns(amountsToBeSubmittedToHMRC, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);
			}
		}

		public void TestErrorIsHandledWhenHMRCSendsErrorRepsonseWhileSubmittingVATReturn()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();

				var client = MTDTestHelper.SetupClient(report, ClientHandlerMock);
				AddResponses();

				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				var result = data.TryToSubmitVATDataToHMRC();

				AssertEquals("Response", string.Empty, result.ResponseFromHMRC);
				AssertEquals("Error", "Could not submit VAT Return. Error details:\r\nError Code: PERIOD_KEY_INVALID, Message: Invalid period key, Request path: \r\n", result.ErrorMessage);

				var accTaxReturn = report.LoadAccTaxReturn();
				AssertNotNull("Tax Return has been saved", accTaxReturn);
				AssertEquals("5 x 9 = 45 columns should be saved", 45, accTaxReturn.Columns.Count);

				AssertEquals("ATR_ACR_ComplianceReport", report.PK, accTaxReturn.ATR_ACR_ComplianceReport);
				AssertEquals("ATR_GovtReturnIdentifier", "18AD", accTaxReturn.ATR_GovtReturnIdentifier);
				AssertEquals("ATR_GovtReceiptInformation", string.Empty, accTaxReturn.ATR_GovtReceiptInformation);
				AssertEquals("ATR_Status", "SAV", accTaxReturn.ATR_Status);
				AssertEquals("ATR_Version", 1, accTaxReturn.ATR_Version);
				AssertEquals("ATR_CompanyName", string.Empty, accTaxReturn.ATR_CompanyName);
				AssertEquals("ATR_VATRegNo", string.Empty, accTaxReturn.ATR_VATRegNo);

				var computedByCW1Columns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "RPR").ToArray();
				AssertATRColumns(computedByCW1Columns, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);

				var unsubmittedPreviousPeriodColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ERR").ToArray();
				AssertATRColumns(unsubmittedPreviousPeriodColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var adjustedAmountColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ADJ").ToArray();
				AssertATRColumns(adjustedAmountColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var groupMemberTotalColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "GMC").ToArray();
				AssertATRColumns(groupMemberTotalColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var amountsToBeSubmittedToHMRC = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "SUB").ToArray();
				AssertATRColumns(amountsToBeSubmittedToHMRC, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);
			}

			void AddResponses()
			{
				var obligationsEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={new ZDate(2017, 01, 01).ToMTDCompliantFormat()}&to={new ZDate(2017, 03, 31).ToMTDCompliantFormat()}");
				var submitVATReturnEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/returns/");

				//Obligations Response
				var responseObligation = new MTDObligations()
				{
					obligations = new MTDObligation[]
					{
						new MTDObligation()
						{
							start = "2017-01-01",
							end = "2017-03-31",
							due = "2017-03-07",
							status = "O",
							periodKey = "18AD"
						},
					}
				};

				ClientHandlerMock.AddJsonResponse(
					obligationsEndpoint
					, System.Net.HttpStatusCode.OK
					, responseObligation.ToJSON());

				//VAT Return submit response
				var responseErrorInfo = new MTDErrorInfo()
				{
					code = "PERIOD_KEY_INVALID",
					message = "Invalid period key",
				};

				ClientHandlerMock.AddJsonResponse(
					submitVATReturnEndpoint
					, System.Net.HttpStatusCode.BadRequest
					, responseErrorInfo.ToJSON());
			}
		}

		public void TestSimultaneousSubmissionOfVATReturnIsNotAllowed()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				var client = MTDTestHelper.SetupClient(report, ClientHandlerMock);
				AddResponses();

				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				using (var mutex = new ZGlobalMutex(MutexIDs.MTDVATSubmission, FormattableString.Invariant($"MTD_18AD_{MTDSubmissionDataTestEnvironmentCreator.MockVRN}")))
				{
					if (mutex.Lock())
					{
						var result = data.TryToSubmitVATDataToHMRC();
						AssertEquals("Response", string.Empty, result.ResponseFromHMRC);
						AssertEquals("Error", "Another user is already submitting VAT return to HMRC.", result.ErrorMessage);
					}
				}

				var accTaxReturn = report.LoadAccTaxReturn();
				AssertNull("Tax Return has been saved", accTaxReturn);
			}

			void AddResponses()
			{
				var obligationsEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={new ZDate(2017, 01, 01).ToMTDCompliantFormat()}&to={new ZDate(2017, 03, 31).ToMTDCompliantFormat()}");

				//Obligations Response
				var responseObligation = new MTDObligations()
				{
					obligations = new MTDObligation[]
					{
						new MTDObligation()
						{
							start = "2017-01-01",
							end = "2017-03-31",
							due = "2017-03-07",
							status = "O",
							periodKey = "18AD"
						},
					}
				};

				ClientHandlerMock.AddJsonResponse(
					obligationsEndpoint
					, System.Net.HttpStatusCode.OK
					, responseObligation.ToJSON());
			}
		}

		public void TestResubmissionAttemptOfVATReturnIsHandled_WhenVATDataCanBeRetrievedFromHMRC()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();

				var client = MTDTestHelper.SetupClient(report, ClientHandlerMock);
				AddResponses();

				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				var result = data.TryToSubmitVATDataToHMRC();

				AssertEquals("Response", string.Empty, result.ResponseFromHMRC);
				AssertEquals("Error"
, @"VAT return has already been submitted for this period and HMRC received it on 10 Apr 2017.
VAT Return data retrieved from HMRC for period 01-Jan-17 00:00:00 to 31-Mar-17 00:00:00:
a. VAT due on sales and other outputs                                                             : 10
b. VAT due on acquisitions from other EC Member States                                            : 5
c. Total VAT due (a + b)                                                                          : 15 
d. VAT reclaimed on purchases and other inputs (including acquisitions from the EC)               : 0
e. Net VAT Due (c - d)                                                                            : 15
f. Total value of sales and all other outputs excluding any VAT                                   : 70
g. Total value of purchases and all other inputs excluding any VAT (including exempt purchases)   : 75
h. Total value of all supplies of goods and related costs, excluding any VAT, to other EC states  : 55
i. Total value of acquisitions of goods and related costs excluding any VAT, from other EC states : 45"
, result.ErrorMessage);

				var accTaxReturn = report.LoadAccTaxReturn();
				AssertNull("No Acc Tax Return is saved", accTaxReturn);
			}

			void AddResponses()
			{
				var obligationsEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={new ZDate(2017, 01, 01).ToMTDCompliantFormat()}&to={new ZDate(2017, 03, 31).ToMTDCompliantFormat()}");
				var getVATReturnEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/returns/18AD");

				//Obligations Response
				var responseObligation = new MTDObligations()
				{
					obligations = new MTDObligation[]
					{
						new MTDObligation()
						{
							start = "2017-01-01",
							end = "2017-03-31",
							status = "F",
							periodKey = "18AD",
							received = "2017-04-10"
						},
					}
				};

				ClientHandlerMock.AddJsonResponse(
					obligationsEndpoint
					, System.Net.HttpStatusCode.OK
					, responseObligation.ToJSON());

				//Get VAT Return Response
				var vatData = new MTDVATData()
				{
					finalised = true,
					netVatDue = 15M,
					periodKey = "18AD",
					totalAcquisitionsExVAT = 45M,
					totalValueGoodsSuppliedExVAT = 55M,
					totalValuePurchasesExVAT = 75M,
					totalValueSalesExVAT = 70M,
					totalVatDue = 15M,
					vatDueAcquisitions = 5M,
					vatDueSales = 10M,
					vatReclaimedCurrPeriod = 0M
				};

				ClientHandlerMock.AddJsonResponse(
					getVATReturnEndpoint
					, System.Net.HttpStatusCode.OK
					, MTDTestHelper.GetSampleSubmittedVATData().ToJSON());
			}
		}

		public void TestResubmissionAttemptOfVATReturnIsHandled_WhenVATDataCannotBeRetrievedFromHMRC()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();

				var client = MTDTestHelper.SetupClient(report, ClientHandlerMock);
				AddResponses();

				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				var result = data.TryToSubmitVATDataToHMRC();

				AssertEquals("Response", string.Empty, result.ResponseFromHMRC);
				AssertEquals("Error"
, @"VAT return has already been submitted for this period and HMRC received it on 10 Apr 2017.
Could not retrieve VAT Return data from HMRC.
Status Code:NotFound"
, result.ErrorMessage);

				var accTaxReturn = report.LoadAccTaxReturn();
				AssertNull("No Acc Tax Return is saved", accTaxReturn);
			}

			void AddResponses()
			{
				var obligationsEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={new ZDate(2017, 01, 01).ToMTDCompliantFormat()}&to={new ZDate(2017, 03, 31).ToMTDCompliantFormat()}");
				var getVATReturnEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/returns/18AD");

				//Obligations Response
				var responseObligation = new MTDObligations()
				{
					obligations = new MTDObligation[]
					{
						new MTDObligation()
						{
							start = "2017-01-01",
							end = "2017-03-31",
							status = "F",
							periodKey = "18AD",
							received = "2017-04-10"
						},
					}
				};

				ClientHandlerMock.AddJsonResponse(
					obligationsEndpoint
					, System.Net.HttpStatusCode.OK
					, responseObligation.ToJSON());

				//Get VAT Return Response
				ClientHandlerMock.AddJsonResponse(
					getVATReturnEndpoint
					, System.Net.HttpStatusCode.NotFound
					, string.Empty);
			}
		}

		public void TestResubmissionAttemptOfVATReturnIsHandledWhenObligationIsAlreadyFulfilled()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();

				var client = MTDTestHelper.SetupClient(report, ClientHandlerMock);
				AddResponses();

				var data = MTDTestHelper.CreateSubmissionData(Factory, report);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();

				var reportInNewFactory = newFactory.Load<AccComplianceReport>(report.PK);
				var accTaxReturn = report.LoadAccTaxReturn();
				AssertNotNull(accTaxReturn);

				accTaxReturn.ATR_Status = AccTaxReturn.Status.Submitted;
				newFactory.Save();

				var result = data.TryToSubmitVATDataToHMRC();

				AssertEquals("Response", string.Empty, result.ResponseFromHMRC);
				AssertEquals("Error"
, "The dates entered are not eligible for submission of a VAT return. They should be as per your open VAT reporting period."
, result.ErrorMessage);
			}

			void AddResponses()
			{
				var obligationsEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={new ZDate(2017, 01, 01).ToMTDCompliantFormat()}&to={new ZDate(2017, 03, 31).ToMTDCompliantFormat()}");
				var getVATReturnEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/returns/18AD");

				//Obligations Response
				var responseObligation = new MTDObligations()
				{
					obligations = new MTDObligation[]
					{
						new MTDObligation()
						{
							start = "2017-01-01",
							end = "2017-03-31",
							status = "F",
							periodKey = "18AD",
							received = "2017-04-10"
						},
					}
				};

				ClientHandlerMock.AddJsonResponse(
					obligationsEndpoint
					, System.Net.HttpStatusCode.OK
					, responseObligation.ToJSON());
			}
		}

		public void TestAccTaxReturnIsCreatedOnFactorySaving()
		{
			var report = CreateFinalizedReport();
			var submissionData = MTDTestHelper.CreateSubmissionData(Factory, report);

			submissionData.Adjustments.Box1_VATDue = 45;
			submissionData.Adjustments.Box2_VATDueReverseChg = 15;
			Factory.Save();

			var accTaxReturn = report.LoadAccTaxReturn();
			AssertNotNull("Tax Return has been saved", accTaxReturn);
			AssertEquals("9 columns should be saved", 9, accTaxReturn.Columns.Count);

			AssertEquals("ATR_ACR_ComplianceReport", report.PK, accTaxReturn.ATR_ACR_ComplianceReport);
			AssertEquals("ATR_GovtReturnIdentifier", "", accTaxReturn.ATR_GovtReturnIdentifier);
			AssertEquals("ATR_GovtReceiptInformation", "", accTaxReturn.ATR_GovtReceiptInformation);
			AssertEquals("ATR_Status", "SAV", accTaxReturn.ATR_Status);
			AssertEquals("ATR_Version", 1, accTaxReturn.ATR_Version);

			var columns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ADJ").ToArray();
			AssertEquals("B1_VatDueSales", 45M, columns.First(c => c.ATC_ColumnName == "B1_VatDueSales").ATC_Amount);
			AssertEquals("B2_VatDueAcquisitions", 15M, columns.First(c => c.ATC_ColumnName == "B2_VatDueAcquisitions").ATC_Amount);
			AssertEquals("B3_TotalVatDue", 60M, columns.First(c => c.ATC_ColumnName == "B3_TotalVatDue").ATC_Amount);
		}

		public void TestAccTaxReturnIsCreatedOnFactorySaving_GroupMemberCompany_ReportGenerated()
		{
			(var groupCompany, var memberCompany) = MTDSubmissionDataTestEnvironmentCreator.CreateUKGroupAndMemberCompany(Factory, "UKG", "UK1");

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateReport();
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				var submissionData = MTDTestHelper.CreateSubmissionData(Factory, report);
				Factory.Save();

				var accTaxReturn = report.LoadAccTaxReturn();
				AssertNotNull("Tax Return has been saved", accTaxReturn);
				AssertEquals("4 x 9 = 36 columns should be saved", 36, accTaxReturn.Columns.Count);

				AssertEquals("ATR_ACR_ComplianceReport", report.PK, accTaxReturn.ATR_ACR_ComplianceReport);
				AssertEquals("ATR_Status", "SAV", accTaxReturn.ATR_Status);
				AssertEquals("ATR_Version", 1, accTaxReturn.ATR_Version);
				AssertEquals("ATR_CompanyName", string.Empty, accTaxReturn.ATR_CompanyName);

				var computedByCW1Columns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "RPR").ToArray();
				AssertATRColumns(computedByCW1Columns, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);

				var unsubmittedPreviousPeriodColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ERR").ToArray();
				AssertATRColumns(unsubmittedPreviousPeriodColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var adjustedAmountColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ADJ").ToArray();
				AssertATRColumns(adjustedAmountColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var amountsToBeSubmittedToHMRC = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "SUB").ToArray();
				AssertATRColumns(amountsToBeSubmittedToHMRC, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);

				submissionData.Adjustments.Box1_VATDue = 45;
				submissionData.Adjustments.Box2_VATDueReverseChg = 15;
				Factory.Save();

				accTaxReturn = report.LoadAccTaxReturn();
				AssertNotNull("Tax Return has been saved", accTaxReturn);
				AssertEquals("4 x 9 = 36 columns should be saved", 36, accTaxReturn.Columns.Count);

				AssertEquals("ATR_ACR_ComplianceReport", report.PK, accTaxReturn.ATR_ACR_ComplianceReport);
				AssertEquals("ATR_Status", "SAV", accTaxReturn.ATR_Status);
				AssertEquals("ATR_Version", 2, accTaxReturn.ATR_Version);
				AssertEquals("ATR_CompanyName", string.Empty, accTaxReturn.ATR_CompanyName);

				computedByCW1Columns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "RPR").ToArray();
				AssertATRColumns(computedByCW1Columns, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);

				unsubmittedPreviousPeriodColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ERR").ToArray();
				AssertATRColumns(unsubmittedPreviousPeriodColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				adjustedAmountColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ADJ").ToArray();
				AssertATRColumns(adjustedAmountColumns, 45M, 15M, 60M, 0M, 60M, 0M, 0M, 0M, 0M);

				amountsToBeSubmittedToHMRC = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "SUB").ToArray();
				AssertATRColumns(amountsToBeSubmittedToHMRC, 1045M, 515M, 1560M, 200M, 1360M, 2500M, 3500M, 700M, 100M);
			}
		}

		public void TestAccTaxReturnIsCreatedOnFactorySaving_GroupMemberCompany_ReportFinalised()
		{
			(var groupCompany, var memberCompany) = MTDSubmissionDataTestEnvironmentCreator.CreateUKGroupAndMemberCompany(Factory, "UKG", "UK1");

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				var submissionData = MTDTestHelper.CreateSubmissionData(Factory, report);
				Factory.Save();

				var accTaxReturn = report.LoadAccTaxReturn();
				AssertNotNull("Tax Return has been saved", accTaxReturn);
				AssertEquals("4 x 9 = 36 columns should be saved", 36, accTaxReturn.Columns.Count);

				AssertEquals("ATR_ACR_ComplianceReport", report.PK, accTaxReturn.ATR_ACR_ComplianceReport);
				AssertEquals("ATR_Status", "SAV", accTaxReturn.ATR_Status);
				AssertEquals("ATR_Version", 1, accTaxReturn.ATR_Version);
				AssertEquals("ATR_CompanyName", string.Empty, accTaxReturn.ATR_CompanyName);

				var computedByCW1Columns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "RPR").ToArray();
				AssertATRColumns(computedByCW1Columns, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);

				var unsubmittedPreviousPeriodColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ERR").ToArray();
				AssertATRColumns(unsubmittedPreviousPeriodColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var adjustedAmountColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ADJ").ToArray();
				AssertATRColumns(adjustedAmountColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				var amountsToBeSubmittedToHMRC = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "SUB").ToArray();
				AssertATRColumns(amountsToBeSubmittedToHMRC, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);

				submissionData.Adjustments.Box1_VATDue = 45;
				submissionData.Adjustments.Box2_VATDueReverseChg = 15;
				Factory.Save();

				accTaxReturn = report.LoadAccTaxReturn();
				AssertNotNull("Tax Return has been saved", accTaxReturn);
				AssertEquals("4 x 9 = 36 columns should be saved", 36, accTaxReturn.Columns.Count);

				AssertEquals("ATR_ACR_ComplianceReport", report.PK, accTaxReturn.ATR_ACR_ComplianceReport);
				AssertEquals("ATR_Status", "SAV", accTaxReturn.ATR_Status);
				AssertEquals("ATR_Version", 2, accTaxReturn.ATR_Version);
				AssertEquals("ATR_CompanyName", string.Empty, accTaxReturn.ATR_CompanyName);

				computedByCW1Columns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "RPR").ToArray();
				AssertATRColumns(computedByCW1Columns, 1000M, 500M, 1500M, 200M, 1300M, 2500M, 3500M, 700M, 100M);

				unsubmittedPreviousPeriodColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ERR").ToArray();
				AssertATRColumns(unsubmittedPreviousPeriodColumns, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);

				adjustedAmountColumns = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "ADJ").ToArray();
				AssertATRColumns(adjustedAmountColumns, 45M, 15M, 60M, 0M, 60M, 0M, 0M, 0M, 0M);

				amountsToBeSubmittedToHMRC = accTaxReturn.Columns.Cast<AccTaxReturnColumn>().Where(c => c.ATC_GroupCode == "SUB").ToArray();
				AssertATRColumns(amountsToBeSubmittedToHMRC, 1045M, 515M, 1560M, 200M, 1360M, 2500M, 3500M, 700M, 100M);
			}
		}

		void AssertATRColumns(AccTaxReturnColumn[] columns
			, ZDecimal b1_VatDueSales_expected, ZDecimal b2_VatDueAcquisitions_expected, ZDecimal b3_TotalVatDue_expected
			, ZDecimal b4_VatReclaimedCurrPeriod_expected, ZDecimal b5_netVatDue_expected, ZDecimal b6_totalValueSalesExVAT_expected
			, ZDecimal b7_TotalValuePurchasesExVAT_expected, ZDecimal b8_TotalValueGoodsSuppliedExVAT_expected, ZDecimal b9_TotalAcquisitionsExVAT_expected)
		{
			var expectedColumnNames = new[]
			{
				"B1_VatDueSales",
				"B2_VatDueAcquisitions",
				"B3_TotalVatDue",
				"B4_VatReclaimedCurrPeriod",
				"B5_netVatDue",
				"B6_totalValueSalesExVAT",
				"B7_TotalValuePurchasesExVAT",
				"B8_TotalValueGoodsSuppliedExVAT",
				"B9_TotalAcquisitionsExVAT"
			};
			AssertContainsExactElementsInAnyOrder(expectedColumnNames, columns.Select(c => c.ATC_ColumnName).ToArray());

			AssertEquals("B1_VatDueSales", b1_VatDueSales_expected, columns.First(c => c.ATC_ColumnName == "B1_VatDueSales").ATC_Amount);
			AssertEquals("B2_VatDueAcquisitions", b2_VatDueAcquisitions_expected, columns.First(c => c.ATC_ColumnName == "B2_VatDueAcquisitions").ATC_Amount);
			AssertEquals("B3_TotalVatDue", b3_TotalVatDue_expected, columns.First(c => c.ATC_ColumnName == "B3_TotalVatDue").ATC_Amount);
			AssertEquals("B4_VatReclaimedCurrPeriod", b4_VatReclaimedCurrPeriod_expected, columns.First(c => c.ATC_ColumnName == "B4_VatReclaimedCurrPeriod").ATC_Amount);
			AssertEquals("B5_netVatDue", b5_netVatDue_expected, columns.First(c => c.ATC_ColumnName == "B5_netVatDue").ATC_Amount);
			AssertEquals("B6_totalValueSalesExVAT", b6_totalValueSalesExVAT_expected, columns.First(c => c.ATC_ColumnName == "B6_totalValueSalesExVAT").ATC_Amount);
			AssertEquals("B7_TotalValuePurchasesExVAT", b7_TotalValuePurchasesExVAT_expected, columns.First(c => c.ATC_ColumnName == "B7_TotalValuePurchasesExVAT").ATC_Amount);
			AssertEquals("B8_TotalValueGoodsSuppliedExVAT", b8_TotalValueGoodsSuppliedExVAT_expected, columns.First(c => c.ATC_ColumnName == "B8_TotalValueGoodsSuppliedExVAT").ATC_Amount);
			AssertEquals("B9_TotalAcquisitionsExVAT", b9_TotalAcquisitionsExVAT_expected, columns.First(c => c.ATC_ColumnName == "B9_TotalAcquisitionsExVAT").ATC_Amount);
		}

		AccComplianceReport CreateFinalizedReport()
		{
			var report = MTDTestHelper.SetupReportAndConfiguration(ObjectCreator, Factory, new ZDate(2017, 01, 01), new ZDate(2017, 03, 31));
			Factory.Save();
			report.Finalise();
			return report;
		}

		AccComplianceReport CreateReport()
		{
			var report = MTDTestHelper.SetupReportAndConfiguration(ObjectCreator, Factory, new ZDate(2017, 01, 01), new ZDate(2017, 03, 31));
			Factory.Save();
			return report;
		}

		internal MTDHttpClientHandlerMock ClientHandlerMock { get; private set; }

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
