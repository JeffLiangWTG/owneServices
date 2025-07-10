using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Environment;
	using Enterprise.MasterFiles.Business;
	using NUnit.Framework;

	[TestedType(typeof(MTDSubmissionDataColumns))]
	public partial class MTDSubmissionDataColumnsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new MTDSubmissionDataColumns(Factory, Factory.New<AccComplianceReport>());
		}

		public void TestAmounts()
		{
			Columns.ComputedByCW1.Box1_VATDue = 1500m;
			Columns.ComputedByCW1.Box2_VATDueReverseChg = 100m;
			Columns.ComputedByCW1.Box4_VATReclaimed = 1000m;
			Columns.ComputedByCW1.Box6_TotalSalesExVAT = 200m;
			Columns.ComputedByCW1.Box7_TotalPurchaseExVAT = 200m;
			Columns.ComputedByCW1.Box8_GoodsSalesECMembersExVAT = 300m;
			Columns.ComputedByCW1.Box9_GoodsPurchaseECMembersExVAT = 369m;

			Columns.UnsubmitedPreviousValues.Box1_VATDue = 500m;
			Columns.UnsubmitedPreviousValues.Box2_VATDueReverseChg = 0m;
			Columns.UnsubmitedPreviousValues.Box4_VATReclaimed = 0m;
			Columns.UnsubmitedPreviousValues.Box6_TotalSalesExVAT = 2800m;
			Columns.UnsubmitedPreviousValues.Box7_TotalPurchaseExVAT = 0m;
			Columns.UnsubmitedPreviousValues.Box8_GoodsSalesECMembersExVAT = 0m;
			Columns.UnsubmitedPreviousValues.Box9_GoodsPurchaseECMembersExVAT = 31m;

			Columns.Adjustments.Box1_VATDue = -150m;
			Columns.Adjustments.Box4_VATReclaimed = 50m;

			Assert(!Columns.OverThresholdWarning);
			AssertEquals(1850m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(100m, Columns.ValuesToSubmitToHMRC.Box2_VATDueReverseChg);
			AssertEquals(1950m, Columns.ValuesToSubmitToHMRC.Box3_TotalVATDue);
			AssertEquals(1050m, Columns.ValuesToSubmitToHMRC.Box4_VATReclaimed);
			AssertEquals(900m, Columns.ValuesToSubmitToHMRC.Box5_NetVAT);
			AssertHasWarning(Columns.ValuesToSubmitToHMRC.Box5_NetVATInfo, "VAT is payable to HMRC");
			AssertEquals(3000m, Columns.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);
			AssertEquals(200m, Columns.ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT);
			AssertEquals(300m, Columns.ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(400m, Columns.ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT);

			Columns.Adjustments.Box4_VATReclaimed = 950m;

			Assert(!Columns.OverThresholdWarning);
			AssertEquals(1850m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(100m, Columns.ValuesToSubmitToHMRC.Box2_VATDueReverseChg);
			AssertEquals(1950m, Columns.ValuesToSubmitToHMRC.Box3_TotalVATDue);
			AssertEquals(1950m, Columns.ValuesToSubmitToHMRC.Box4_VATReclaimed);
			AssertEquals(0m, Columns.ValuesToSubmitToHMRC.Box5_NetVAT);
			AssertNoNotifications(Columns.ValuesToSubmitToHMRC.Box5_NetVATInfo);
			AssertEquals(3000m, Columns.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);
			AssertEquals(200m, Columns.ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT);
			AssertEquals(300m, Columns.ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(400m, Columns.ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT);

			Columns.Adjustments.Box4_VATReclaimed = 951m;

			Assert(!Columns.OverThresholdWarning);
			AssertEquals(1850m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(100m, Columns.ValuesToSubmitToHMRC.Box2_VATDueReverseChg);
			AssertEquals(1950m, Columns.ValuesToSubmitToHMRC.Box3_TotalVATDue);
			AssertEquals(1951m, Columns.ValuesToSubmitToHMRC.Box4_VATReclaimed);
			AssertEquals(1m, Columns.ValuesToSubmitToHMRC.Box5_NetVAT);
			AssertHasWarning(Columns.ValuesToSubmitToHMRC.Box5_NetVATInfo, "VAT is recoverable from HMRC");
			AssertEquals(3000m, Columns.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);
			AssertEquals(200m, Columns.ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT);
			AssertEquals(300m, Columns.ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(400m, Columns.ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT);
		}

		public void TestAmounts_GroupMemberTotal()
		{
			Columns.ComputedByCW1.Box1_VATDue = 1500m;
			Columns.ComputedByCW1.Box2_VATDueReverseChg = 100m;
			Columns.ComputedByCW1.Box4_VATReclaimed = 1000m;
			Columns.ComputedByCW1.Box6_TotalSalesExVAT = 200m;
			Columns.ComputedByCW1.Box7_TotalPurchaseExVAT = 200m;
			Columns.ComputedByCW1.Box8_GoodsSalesECMembersExVAT = 300m;
			Columns.ComputedByCW1.Box9_GoodsPurchaseECMembersExVAT = 369m;

			Columns.UnsubmitedPreviousValues.Box1_VATDue = 500m;
			Columns.UnsubmitedPreviousValues.Box2_VATDueReverseChg = 0m;
			Columns.UnsubmitedPreviousValues.Box4_VATReclaimed = 0m;
			Columns.UnsubmitedPreviousValues.Box6_TotalSalesExVAT = 2800m;
			Columns.UnsubmitedPreviousValues.Box7_TotalPurchaseExVAT = 0m;
			Columns.UnsubmitedPreviousValues.Box8_GoodsSalesECMembersExVAT = 0m;
			Columns.UnsubmitedPreviousValues.Box9_GoodsPurchaseECMembersExVAT = 31m;

			Columns.UpdateValuesToSubmitToHMRC();

			Assert(!Columns.OverThresholdWarning);
			AssertEquals(2000m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(100m, Columns.ValuesToSubmitToHMRC.Box2_VATDueReverseChg);
			AssertEquals(2100m, Columns.ValuesToSubmitToHMRC.Box3_TotalVATDue);
			AssertEquals(1000m, Columns.ValuesToSubmitToHMRC.Box4_VATReclaimed);
			AssertEquals(1100m, Columns.ValuesToSubmitToHMRC.Box5_NetVAT);
			AssertHasWarning(Columns.ValuesToSubmitToHMRC.Box5_NetVATInfo, "VAT is payable to HMRC");
			AssertEquals(3000m, Columns.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);
			AssertEquals(200m, Columns.ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT);
			AssertEquals(300m, Columns.ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(400m, Columns.ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT);

			Columns.GroupMemberTotal.Box1_VATDue = 6000m;
			Columns.GroupMemberTotal.Box2_VATDueReverseChg = 400m;
			Columns.GroupMemberTotal.Box4_VATReclaimed = 4000m;
			Columns.GroupMemberTotal.Box6_TotalSalesExVAT = 800m;
			Columns.GroupMemberTotal.Box7_TotalPurchaseExVAT = 800m;
			Columns.GroupMemberTotal.Box8_GoodsSalesECMembersExVAT = 1200m;
			Columns.GroupMemberTotal.Box9_GoodsPurchaseECMembersExVAT = 1000m;

			Columns.UpdateValuesToSubmitToHMRC();

			Assert(!Columns.OverThresholdWarning);
			AssertEquals(8000m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(500m, Columns.ValuesToSubmitToHMRC.Box2_VATDueReverseChg);
			AssertEquals(8500m, Columns.ValuesToSubmitToHMRC.Box3_TotalVATDue);
			AssertEquals(5000m, Columns.ValuesToSubmitToHMRC.Box4_VATReclaimed);
			AssertEquals(3500m, Columns.ValuesToSubmitToHMRC.Box5_NetVAT);
			AssertHasWarning(Columns.ValuesToSubmitToHMRC.Box5_NetVATInfo, "VAT is payable to HMRC");
			AssertEquals(3800m, Columns.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);
			AssertEquals(1000m, Columns.ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT);
			AssertEquals(1500m, Columns.ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(1400m, Columns.ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT);

			Columns.GroupMemberTotal.Box4_VATReclaimed = 7500m;
			Columns.UpdateValuesToSubmitToHMRC();

			Assert(!Columns.OverThresholdWarning);
			AssertEquals(8000m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(500m, Columns.ValuesToSubmitToHMRC.Box2_VATDueReverseChg);
			AssertEquals(8500m, Columns.ValuesToSubmitToHMRC.Box3_TotalVATDue);
			AssertEquals(8500m, Columns.ValuesToSubmitToHMRC.Box4_VATReclaimed);
			AssertEquals(0m, Columns.ValuesToSubmitToHMRC.Box5_NetVAT);
			AssertNoWarnings(Columns.ValuesToSubmitToHMRC.Box5_NetVATInfo);
			AssertEquals(3800m, Columns.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);
			AssertEquals(1000m, Columns.ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT);
			AssertEquals(1500m, Columns.ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(1400m, Columns.ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT);

			Columns.GroupMemberTotal.Box4_VATReclaimed = 7501m;
			Columns.UpdateValuesToSubmitToHMRC();

			Assert(!Columns.OverThresholdWarning);
			AssertEquals(8000m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(500m, Columns.ValuesToSubmitToHMRC.Box2_VATDueReverseChg);
			AssertEquals(8500m, Columns.ValuesToSubmitToHMRC.Box3_TotalVATDue);
			AssertEquals(8501m, Columns.ValuesToSubmitToHMRC.Box4_VATReclaimed);
			AssertEquals(1m, Columns.ValuesToSubmitToHMRC.Box5_NetVAT);
			AssertHasWarning(Columns.ValuesToSubmitToHMRC.Box5_NetVATInfo, "VAT is recoverable from HMRC");
			AssertEquals(3800m, Columns.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);
			AssertEquals(1000m, Columns.ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT);
			AssertEquals(1500m, Columns.ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(1400m, Columns.ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT);
		}

		public void TestAllUnsubmittedAmountsAreZero()
		{
			Columns.ComputedByCW1.Box1_VATDue = 2000m;
			Columns.ComputedByCW1.Box2_VATDueReverseChg = 100m;
			Columns.ComputedByCW1.Box4_VATReclaimed = 1000m;
			Columns.ComputedByCW1.Box6_TotalSalesExVAT = 3000m;
			Columns.ComputedByCW1.Box7_TotalPurchaseExVAT = 200m;
			Columns.ComputedByCW1.Box8_GoodsSalesECMembersExVAT = 300m;
			Columns.ComputedByCW1.Box9_GoodsPurchaseECMembersExVAT = 400m;

			Columns.UnsubmitedPreviousValues.Box1_VATDue = 0m;
			Columns.UnsubmitedPreviousValues.Box2_VATDueReverseChg = 0m;
			Columns.UnsubmitedPreviousValues.Box4_VATReclaimed = 0m;
			Columns.UnsubmitedPreviousValues.Box6_TotalSalesExVAT = 0m;
			Columns.UnsubmitedPreviousValues.Box7_TotalPurchaseExVAT = 0m;
			Columns.UnsubmitedPreviousValues.Box8_GoodsSalesECMembersExVAT = 0m;
			Columns.UnsubmitedPreviousValues.Box9_GoodsPurchaseECMembersExVAT = 0m;

			Columns.Adjustments.Box1_VATDue = -150m;
			Columns.Adjustments.Box4_VATReclaimed = 50m;

			Assert(!Columns.OverThresholdWarning);

			AssertEquals(1850m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(100m, Columns.ValuesToSubmitToHMRC.Box2_VATDueReverseChg);
			AssertEquals(1950m, Columns.ValuesToSubmitToHMRC.Box3_TotalVATDue);
			AssertEquals(1050m, Columns.ValuesToSubmitToHMRC.Box4_VATReclaimed);
			AssertEquals(900m, Columns.ValuesToSubmitToHMRC.Box5_NetVAT);
			AssertHasWarning(Columns.ValuesToSubmitToHMRC.Box5_NetVATInfo, "VAT is payable to HMRC");
			AssertEquals(3000m, Columns.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);
			AssertEquals(200m, Columns.ValuesToSubmitToHMRC.Box7_TotalPurchaseExVAT);
			AssertEquals(300m, Columns.ValuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT);
			AssertEquals(400m, Columns.ValuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT);
		}

		public void TestIfErrorsArentIncludedInTotalOver50000()
		{
			Columns.UnsubmitedPreviousValues.Box1_VATDue = 30000m;
			Columns.Adjustments.Box1_VATDue = 30000m;

			Assert(Columns.OverThresholdWarning);
			AssertEquals(30000m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
		}

		public void TestIfErrorsArentIncludedInTotalOver1Percent()
		{
			Columns.ComputedByCW1.Box6_TotalSalesExVAT = 400000m;
			Columns.UnsubmitedPreviousValues.Box1_VATDue = 10000m;
			Columns.Adjustments.Box1_VATDue = 10000m;

			Assert(Columns.OverThresholdWarning);
			AssertEquals(10000m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
		}

		public void TestIfErrorsAreIncludedInTotalUnder1Percent()
		{
			Columns.ComputedByCW1.Box6_TotalSalesExVAT = 4000000m;
			Columns.UnsubmitedPreviousValues.Box1_VATDue = 10000m;
			Columns.Adjustments.Box1_VATDue = 10000m;

			Assert(!Columns.OverThresholdWarning);
			AssertEquals(20000m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
		}

		public void TestIfErrorsAreIncludedInTotalUnder10000()
		{
			Columns.UnsubmitedPreviousValues.Box1_VATDue = 500m;
			Columns.Adjustments.Box1_VATDue = 20m;

			Assert(!Columns.OverThresholdWarning);
			AssertEquals(520m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
		}

		public void TestOverThresholdHasNoImpactWithGroupMemberTotal_IfNotApplicable()
		{
			Columns.UnsubmitedPreviousValues.Box1_VATDue = 500m;
			Columns.UpdateValuesToSubmitToHMRC();

			AssertEquals(500m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			Assert(!Columns.OverThresholdWarning);

			Columns.GroupMemberTotal.Box1_VATDue = 50000m;
			Columns.UpdateValuesToSubmitToHMRC();

			AssertEquals(50500m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			Assert(!Columns.OverThresholdWarning);
		}

		public void TestOverThresholdHasNoImpactWithGroupMemberTotal_IfAppliacable()
		{
			Columns.ComputedByCW1.Box6_TotalSalesExVAT = 400000m;
			Columns.UnsubmitedPreviousValues.Box1_VATDue = 10000m;
			Columns.UpdateValuesToSubmitToHMRC();

			Assert(Columns.OverThresholdWarning);
			AssertEquals(0m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(400000m, Columns.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);

			Columns.GroupMemberTotal.Box6_TotalSalesExVAT = 4000000m;
			Columns.UpdateValuesToSubmitToHMRC();

			Assert(Columns.OverThresholdWarning);
			AssertEquals(0m, Columns.ValuesToSubmitToHMRC.Box1_VATDue);
			AssertEquals(4400000m, Columns.ValuesToSubmitToHMRC.Box6_TotalSalesExVAT);
		}

		public void TestDeclarationHeaderText()
		{
			Columns.PopulateDataFromReport();
			var vatRegistration = Report.Company.OrgProxy.CustomsCodes.AddNew();
			vatRegistration.OK_CodeType = "VAT";
			vatRegistration.OK_CustomsRegNo = "12345678";
			AssertEquals("12345678", Columns.GSTRegNo);

			var expectedText = "Declaration to HMRC by CargoWise Support for Eagle Datamation International (VRN - 12345678):";
			AssertEquals("DeclarationHeaderText", expectedText, Columns.DeclarationHeaderText);
		}

		public void TestReadOnly_ReturnDueDate()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today);
			MTDSubmissionDataTestEnvironmentCreator.CreateCustomsCode(testObjectCreator);

			var groupCompany = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(testObjectCreator, "UKG", "UKG");
			var memberCompany = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(testObjectCreator, "UK1", "UK1");

			AssertEquals("Precondition: IsSubmitted should be false.", false, Columns.IsSubmitted);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, groupCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Columns.ReturnDueDate = ZDate.Today;
				AssertEquals("Readonly should be false if date is valid for group company.", false, Columns.ReadOnly);

				Columns.ReturnDueDate = ZDate.Invalid;
				AssertEquals("Readonly should be true if date is invalid for group company.", true, Columns.ReadOnly);
			}

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Columns.ReturnDueDate = ZDate.Today;
				AssertEquals("Readonly should be false if date is valid for member company.", false, Columns.ReadOnly);

				Columns.ReturnDueDate = ZDate.Invalid;
				AssertEquals("Readonly should be false if date is invalid for member company.", false, Columns.ReadOnly);
			}
		}

		public void TestPopulateDataFromReport_GroupMemberTotal()
		{
			ObjectCreator.CreateTestPeriods(ZDateTime.Today);
			MTDSubmissionDataTestEnvironmentCreator.CreateCustomsCode(ObjectCreator);

			var groupCompany = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator, "UKG", "UKG");
			var memberCompanyUK1 = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator, "UK1", "UK1");
			var memberCompanyUK2 = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator, "UK2", "UK2");
			var independentCompany = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(ObjectCreator, "UKI", "UKI");

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompanyUK1.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompanyUK2.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			{
				var fromDate = new ZDate(2017, 1, 1);
				var toDate = new ZDate(2017, 3, 31);

				var groupCompanySubmissionData1 = CreateMTDReportWithSubmissionData(groupCompany, fromDate, toDate, 1000m, 100, false);

				PopulateDataFromReport(groupCompanySubmissionData1);
				AssertEquals("Should be zero before Group Members Report Generation", 0m, groupCompanySubmissionData1.GroupMemberTotal.Box1_VATDue);
				AssertEquals("Should be zero before Group Members Report Generation", 0m, groupCompanySubmissionData1.GroupMemberTotal.Box2_VATDueReverseChg);

				var memberCompanyUK1SubmissionData1 = CreateMTDReportWithSubmissionData(memberCompanyUK1, fromDate, toDate, 500m, 30, false);

				PopulateDataFromReport(groupCompanySubmissionData1);
				AssertEquals("After UK1 Report Generation: Total of UK1 = 500", 500m, groupCompanySubmissionData1.GroupMemberTotal.Box1_VATDue);
				AssertEquals("After Generating Report: Total of UK1 = 30", 30m, groupCompanySubmissionData1.GroupMemberTotal.Box2_VATDueReverseChg);

				var memberCompanyUK2SubmissionData1 = CreateMTDReportWithSubmissionData(memberCompanyUK2, fromDate, toDate, 800m, 90, false);

				PopulateDataFromReport(groupCompanySubmissionData1);
				AssertEquals("After Generating Report for both: Total of UK1 and UK2. 500 + 800 = 1300", 1300m, groupCompanySubmissionData1.GroupMemberTotal.Box1_VATDue);
				AssertEquals("After Generating Report for both: Total of UK1 and UK2. 30 + 90 = 120", 120m, groupCompanySubmissionData1.GroupMemberTotal.Box2_VATDueReverseChg);

				var independentCompanySubmissionData1 = CreateMTDReportWithSubmissionData(independentCompany, fromDate, toDate, 347m, 24m, false);
				var xyzTypeReportSubmissionData1 = CreateReportWithSubmissionData(memberCompanyUK1, "XYZ", fromDate, toDate, 1857m, 150, false);

				PopulateDataFromReport(groupCompanySubmissionData1);
				AssertEquals("Group Member Total should not affect after generating Report for independent company and XYZ report.", 1300m, groupCompanySubmissionData1.GroupMemberTotal.Box1_VATDue);
				AssertEquals("Group Member Total should not affect after generating Report for independent company and XYZ report.", 120m, groupCompanySubmissionData1.GroupMemberTotal.Box2_VATDueReverseChg);

				groupCompanySubmissionData1.ComplianceReport.Finalise();
				memberCompanyUK1SubmissionData1.ComplianceReport.Finalise();
				memberCompanyUK2SubmissionData1.ComplianceReport.Finalise();
				independentCompanySubmissionData1.ComplianceReport.Finalise();
				xyzTypeReportSubmissionData1.ComplianceReport.Finalise();

				PopulateDataFromReport(groupCompanySubmissionData1);
				AssertEquals("After Finalizing Report: Total of UK1 and UK2. 500 + 800 = 1300", 1300m, groupCompanySubmissionData1.GroupMemberTotal.Box1_VATDue);
				AssertEquals("After Finalizing Report: Total of UK1 and UK2. 30 + 90 = 120", 120m, groupCompanySubmissionData1.GroupMemberTotal.Box2_VATDueReverseChg);

				SetValue(memberCompanyUK1SubmissionData1, x => memberCompanyUK1SubmissionData1.Adjustments.Box1_VATDue = x, 200m);

				AssertEquals("After Adjustments for Member1(Before calling PopulateDataFromReport): Total of UK1 and UK2. 500 + 800 = 1300", 1300m, groupCompanySubmissionData1.GroupMemberTotal.Box1_VATDue);
				PopulateDataFromReport(groupCompanySubmissionData1);
				AssertEquals("After Adjustments for Member1(After calling PopulateDataFromReport): Total of UK1 and UK2. (500 + 200 = 700) + 800 = 1500", 1500m, groupCompanySubmissionData1.GroupMemberTotal.Box1_VATDue);

				SubmitMemberCompanyReport(memberCompanyUK1SubmissionData1);
				SubmitMemberCompanyReport(memberCompanyUK2SubmissionData1);

				PopulateDataFromReport(groupCompanySubmissionData1);
				AssertEquals("After Submitting Report: Total of UK1 and UK2. 700 + 800 = 1500", 1500m, groupCompanySubmissionData1.GroupMemberTotal.Box1_VATDue);
				AssertEquals("After Submitting Report: Total of UK1 and UK2. 30 + 90 = 120", 120m, groupCompanySubmissionData1.GroupMemberTotal.Box2_VATDueReverseChg);

				fromDate = new ZDate(2017, 4, 1);
				toDate = new ZDate(2017, 6, 30);

				var groupCompanySubmissionData2 = CreateMTDReportWithSubmissionData(groupCompany, fromDate, toDate, 1200m, 120);
				var memberCompanyUK1SubmissionData2 = CreateMTDReportWithSubmissionData(memberCompanyUK1, fromDate, toDate, 520m, 32);
				var memberCompanyUK2SubmissionData2 = CreateMTDReportWithSubmissionData(memberCompanyUK2, fromDate, toDate, 820m, 92);
				var independentCompanySubmissionData2 = CreateMTDReportWithSubmissionData(independentCompany, fromDate, toDate, 367m, 26m);
				var xyzTypeReportSubmissionData2 = CreateReportWithSubmissionData(memberCompanyUK2, "XYZ", fromDate, toDate, 1947m, 102);

				PopulateDataFromReport(groupCompanySubmissionData2);
				AssertEquals("After Finalizing Report: Total of UK1 and UK2. 520 + 820 = 1340", 1340m, groupCompanySubmissionData2.GroupMemberTotal.Box1_VATDue);
				AssertEquals("After Finalizing Report: Total of UK1 and UK2. 32 + 92 = 124", 124m, groupCompanySubmissionData2.GroupMemberTotal.Box2_VATDueReverseChg);

				SubmitMemberCompanyReport(memberCompanyUK1SubmissionData2);
				SubmitMemberCompanyReport(memberCompanyUK2SubmissionData2);

				PopulateDataFromReport(groupCompanySubmissionData2);
				AssertEquals("Total of UK1 and UK2. 520 + 820 = 1340", 1340m, groupCompanySubmissionData2.GroupMemberTotal.Box1_VATDue);
				AssertEquals("Total of UK1 and UK2. 32 + 92 = 124", 124m, groupCompanySubmissionData2.GroupMemberTotal.Box2_VATDueReverseChg);
			}

			void PopulateDataFromReport(MTDSubmissionDataColumns dataColumns)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, dataColumns.ComplianceReport.Company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					dataColumns.PopulateDataFromReport();
				}
			}

			void SetValue(MTDSubmissionDataColumns dataColumns, Action<ZDecimal> action, ZDecimal value)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, dataColumns.ComplianceReport.Company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					action(value);
					Factory.Save();
				}
			}

			void SubmitMemberCompanyReport(MTDSubmissionDataColumns dataColumns)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, dataColumns.ComplianceReport.Company.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					(var successMessage, var errorMessage) = dataColumns.SubmitVATDataToGroup();
					AssertEquals("Pre-Condition: errorMessage should be empty", "", errorMessage);
					AssertEquals("Pre-Condition: status should be SUB", "SUB", dataColumns.Status);
				}
			}

			MTDSubmissionDataColumns CreateMTDReportWithSubmissionData(GlbCompany glbCompany, ZDate fromDate, ZDate toDate, ZDecimal box1_VATDue, ZDecimal box2_VATDueReverseChg, bool isFinalized = true)
			{
				return CreateReportWithSubmissionData(glbCompany, "MTD", fromDate, toDate, box1_VATDue, box2_VATDueReverseChg, isFinalized);
			}

			MTDSubmissionDataColumns CreateReportWithSubmissionData(GlbCompany glbCompany, ZString reportType, ZDate fromDate, ZDate toDate, ZDecimal box1_VATDue, ZDecimal box2_VATDueReverseChg, bool isFinalized = true)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, glbCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var report = MTDTestHelper.SetupReportAndConfiguration(ObjectCreator, Factory, fromDate, toDate);
					report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
					report.ACR_ReportType = reportType;
					report.Factory.Save();

					if (isFinalized)
					{
						report.Finalise();
					}

					var submissionData = new MTDSubmissionDataColumns(Factory, report);
					submissionData.ComputedByCW1.Box1_VATDue = box1_VATDue;
					submissionData.ComputedByCW1.Box2_VATDueReverseChg = box2_VATDueReverseChg;
					submissionData.UpdateValuesToSubmitToHMRC();
					submissionData.ReturnDueDate = toDate.AddDays(1);
					Factory.Save();

					AssertEquals("Pre-Condition: status should be SAV", "SAV", submissionData.Status);

					return submissionData;
				}
			}
		}

		public void TestPopulateDataFromReport_HasChanges_GroupCompany()
		{
			(var groupCompany, var memberCompany) = MTDSubmissionDataTestEnvironmentCreator.CreateUKGroupAndMemberCompany(Factory, "UKG", "UK1");

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, groupCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				var submissionDataColumns = new MTDSubmissionDataColumns(Factory, report);
				submissionDataColumns.PopulateDataFromReport();
				AssertEquals("submissionDataColumns.HasChanges should be false for Group Company", false, submissionDataColumns.HasChanges);
			}
		}

		public void TestPopulateDataFromReport_HasChanges_GroupMemberCompany()
		{
			(var groupCompany, var memberCompany) = MTDSubmissionDataTestEnvironmentCreator.CreateUKGroupAndMemberCompany(Factory, "UKG", "UK1");

			using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = CreateFinalizedReport();
				var submissionDataColumns = new MTDSubmissionDataColumns(Factory, report);

				submissionDataColumns.PopulateDataFromReport();
				AssertEquals("submissionDataColumns.HasChanges should be true for Group Member Company", true, submissionDataColumns.HasChanges);
				AssertEquals("submissionDataColumns.ReadOnly should be false", false, submissionDataColumns.ReadOnly);

				submissionDataColumns.SubmitVATDataToGroup();
				AssertEquals("After Submission submissionDataColumns.HasChanges should be false", false, submissionDataColumns.HasChanges);
				AssertEquals("After Submission submissionDataColumns.ReadOnly should be true", true, submissionDataColumns.ReadOnly);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Report = Factory.New<AccComplianceReport>();
			Report.FillWithValidTestData();
			Columns = new MTDSubmissionDataColumns(Factory, Report);
			ClientHandlerMock = Helper.MTDHttpClientHandler;
			Helper.SetupMockHttpClientForMTD();
		}

		MTDTestHelper Helper => helper ?? (helper = new MTDTestHelper());
		MTDTestHelper helper;

		AccComplianceReport Report;
		MTDSubmissionDataColumns Columns;
	}
}
