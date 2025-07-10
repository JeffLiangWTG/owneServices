using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.JSON.Extensions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	public class MTDSubmissionDataColumnsAdapterTest : TestCaseWithFactory
	{
		public void TestLoadMTDSubmissionData()
		{
			var report = CreateFinalizedReport();

			var submissionData = MTDTestHelper.CreateSubmissionData(Factory, report);
			submissionData.Adjustments.Box1_VATDue = 45;
			submissionData.Adjustments.Box2_VATDueReverseChg = 15;
			submissionData.UpdateValuesToSubmitToHMRC();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reportInNewFactory = newFactory.Load<AccComplianceReport>(report.PK);
			var submissionDataInNewFactory = reportInNewFactory.LoadMTDSubmissionData();

			AssertNotEquals(submissionData, submissionDataInNewFactory);
			AssertEquals("Report", submissionData.ComplianceReport.PK, submissionDataInNewFactory.ComplianceReport.PK);
			AssertSubmissionDataColumn(new MTDSubmissionData(Factory), submissionDataInNewFactory.ComputedByCW1);
			AssertSubmissionDataColumn(new MTDSubmissionData(Factory), submissionDataInNewFactory.UnsubmitedPreviousValues);
			//Only Adjustment Data is saved 
			AssertSubmissionDataColumn(submissionData.Adjustments, submissionDataInNewFactory.Adjustments);
			AssertSubmissionDataColumn(new MTDSubmissionData(Factory), submissionDataInNewFactory.ValuesToSubmitToHMRC);
		}

		public void TestLoadMTDSubmissionData_AllColumns()
		{
			var company = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator);
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				SetupMock();

				var submissionData = MTDTestHelper.CreateSubmissionDataWithAllColumns(Factory, report);
				submissionData.ReturnDueDate = ZDate.Today;
				submissionData.Declaration = true;
				var result = submissionData.TryToSubmitVATDataToHMRC();
				AssertNullOrEmpty("Pre-Condition:ErrorMessage should be empty.", result.ErrorMessage);

				var newFactory = new BusinessObjectFactory();
				var reportInNewFactory = newFactory.Load<AccComplianceReport>(report.PK);
				var submissionDataInNewFactory = reportInNewFactory.LoadMTDSubmissionData();

				AssertNotEquals(submissionData, submissionDataInNewFactory);
				AssertEquals("Report", submissionData.ComplianceReport.PK, submissionDataInNewFactory.ComplianceReport.PK);
				AssertSubmissionDataColumn(submissionData.ComputedByCW1, submissionDataInNewFactory.ComputedByCW1);
				AssertSubmissionDataColumn(submissionData.UnsubmitedPreviousValues, submissionDataInNewFactory.UnsubmitedPreviousValues);
				AssertSubmissionDataColumn(submissionData.GroupMemberTotal, submissionDataInNewFactory.GroupMemberTotal);
				AssertSubmissionDataColumn(submissionData.Adjustments, submissionDataInNewFactory.Adjustments);
				AssertSubmissionDataColumn(submissionData.ValuesToSubmitToHMRC, submissionDataInNewFactory.ValuesToSubmitToHMRC);

				void SetupMock()
				{
					var helper = new MTDTestHelper();
					helper.SetupMockHttpClientForMTD();
					var client = MTDTestHelper.SetupClient(report, helper.MTDHttpClientHandler);

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

					helper.MTDHttpClientHandler.AddJsonResponse(
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

					helper.MTDHttpClientHandler.AddJsonResponse(
						submitVATReturnEndpoint
						, System.Net.HttpStatusCode.OK
						, responseData.ToJSON()
						, null
						, new KeyValuePair<string, IEnumerable<string>>[] { new KeyValuePair<string, IEnumerable<string>>("Receipt-ID", new string[] { "e0606fe6233348119cf18023c0fb5271" }) });
				}
			}
		}

		public void TestLoadAccTaxReturn()
		{
			var report = CreateFinalizedReport();

			var submissionData = MTDTestHelper.CreateSubmissionData(Factory, report);
			submissionData.Adjustments.Box1_VATDue = 45;
			submissionData.Adjustments.Box2_VATDueReverseChg = 15;
			submissionData.UpdateValuesToSubmitToHMRC();

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

		void AssertSubmissionDataColumn(MTDSubmissionData expected, MTDSubmissionData actual)
		{
			AssertEquals(expected.Box1_VATDue, actual.Box1_VATDue);
			AssertEquals(expected.Box2_VATDueReverseChg, actual.Box2_VATDueReverseChg);
			AssertEquals(expected.Box3_TotalVATDue, actual.Box3_TotalVATDue);
			AssertEquals(expected.Box4_VATReclaimed, actual.Box4_VATReclaimed);
			AssertEquals(expected.Box5_NetVAT, actual.Box5_NetVAT);
			AssertEquals(expected.Box6_TotalSalesExVAT, actual.Box6_TotalSalesExVAT);
			AssertEquals(expected.Box7_TotalPurchaseExVAT, actual.Box7_TotalPurchaseExVAT);
			AssertEquals(expected.Box8_GoodsSalesECMembersExVAT, actual.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(expected.Box9_GoodsPurchaseECMembersExVAT, actual.Box9_GoodsPurchaseECMembersExVAT);
		}

		AccComplianceReport CreateFinalizedReport()
		{
			var report = MTDTestHelper.SetupReportAndConfiguration(ObjectCreator, Factory, new ZDate(2017, 01, 01), new ZDate(2017, 03, 31));
			Factory.Save();
			report.Finalise();
			return report;
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
