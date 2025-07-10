using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.Business.Testing
{
	public class ChargeValidationTest : TestCaseWithFactory
	{
		public void TestInternalBranchDeptCombinationValidation()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = Env.CurrentCompany.PK;
			branch.AllowedDepartments.DeleteAll();

			var allowedDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var disallowedDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(branch, new GlbDepartment[] { allowedDepartment });

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var charge = job.Charges.AddNew();
				charge.JR_OSSellAmt = 1000;
				charge.JR_OH_SellAccount = GlbBranch.CurrentBranch.OrgProxy.PK;
				Assert("Precondition:", !charge.JR_GE_InternalDeptInfo.ReadOnly);

				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GB_InternalBranch = branch.PK;

				charge.JR_GE_InternalDept = disallowedDepartment.PK;
				charge.Validation.ValidateJR_GE_InternalDept();
				AssertHasError(charge.JR_GE_InternalDeptInfo, $@"The department {disallowedDepartment.GE_Code} cannot be used with the branch {branch.GB_Code}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.");

				charge.JR_GE_InternalDept = allowedDepartment.PK;
				charge.Validation.ValidateJR_GE_InternalDept();
				AssertNoErrors(charge.JR_GE_InternalDeptInfo);
			}
		}

		public void TestCheckJR_JH_InternalJob()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var sellAccount = GlbCompany.CurrentCompany.OrgProxy;
			var branch = TestObjectCreator.CreateBranch("NEW", GlbCompany.CurrentCompany, sellAccount);
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);

			using (var consolJob = TestObjectCreator.CreateJob(consol))
			using (TestObjectCreator.CreateJob(shipment))
			{
				Factory.Save();

				var charge = TestObjectCreator.CreateCharge(consolJob, TestObjectCreator.FRT, 100, 100);
				Assert(!charge.JR_JH_InternalJobInfo.HasErrors());

				using (charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
				{
					charge.JR_Calc_RelatedJobNumber = shipment.JobNumber;
					charge.JR_OH_SellAccount = sellAccount.PK;
					charge.JR_JH_InternalJob = consolJob.PK;
					charge.JR_GB = branch.PK;
					charge.JR_GB_InternalBranch = branch.PK;
					charge.JR_GE = TestObjectCreator.FEADepartment.PK;
					charge.JR_GE_InternalDept = TestObjectCreator.FEADepartment.PK;
				}
				charge.Validation.ValidateJR_JH_InternalJob();
				Assert("Precondition: charge is synchronisable", GatewaySellToCostSynchroniser.IsGatewaySynchronizable(charge));
				AssertHasWarning("Internal Job has error when Synchronisable and has related job", charge.JR_JH_InternalJobInfo, "This charge relates to a single job S0001, but setting Internal Job to the current consol will post gateway revenue as cost on multiple shipments. Please review the ‘Related Job Number’ of this charge and confirm the selection is correct.");

				charge.JR_GE_InternalDept = TestObjectCreator.FESDepartment.PK;
				charge.Validation.ValidateJR_JH_InternalJob();
				Assert("Precondition: charge is no longer synchronisable", !GatewaySellToCostSynchroniser.IsGatewaySynchronizable(charge));
				Assert("No error when charge is not syncrhonisable", !charge.JR_JH_InternalJobInfo.HasErrors());

				charge.JR_GE_InternalDept = TestObjectCreator.FEADepartment.PK;
				charge.Validation.ValidateJR_JH_InternalJob();
				AssertHasWarning("Precondition: charge has error again", charge.JR_JH_InternalJobInfo, "This charge relates to a single job S0001, but setting Internal Job to the current consol will post gateway revenue as cost on multiple shipments. Please review the ‘Related Job Number’ of this charge and confirm the selection is correct.");
				var line = Factory.NewWithValidTestData<ARInvoiceLine>();
				line.AL_LineType = TransactionLineTypes.Revenue;
				charge.JR_AL_ARLine = line.PK;
				Assert("Precondition: charge is now posted", charge.IsRevenuePosted);

				charge.Validation.ValidateJR_JH_InternalJob();
				Assert("No error when sell portion is posted", !charge.JR_JH_InternalJobInfo.HasErrors());
			}
		}

		public void TestCheckJR_JH_InternalJob_ValidateClosedJob_WhenInternalJobIsInvalid()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();

			var sellAccount = GlbCompany.CurrentCompany.OrgProxy;
			var branch = TestObjectCreator.CreateBranch("NEW", GlbCompany.CurrentCompany, sellAccount);
			var shipment = TestObjectCreator.CreateShipment("S0001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				job.SetupDependency(mockClosedJobReopener.Object);

				AssertNotNull("Precondition: job ClosedReopener" ,job.ClosedJobReopener);

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100, 100);
				using (charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
				{
					charge.JR_Calc_RelatedJobNumber = shipment.JobNumber;
					charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
					charge.JR_JH_InternalJob = Guid.Empty;
					charge.JR_GB = branch.PK;
					charge.JR_GB_InternalBranch = branch.PK;
					charge.JR_GE = TestObjectCreator.FEADepartment.PK;
					charge.JR_GE_InternalDept = TestObjectCreator.FEADepartment.PK;
				}

				Assert("Precondition: JR_JH_InternalJob.IsValid", !charge.JR_JH_InternalJob.IsValid);
				Assert("Precondition: ShouldCreateJRJ", !charge.ShouldCreateJRJ);
				mockClosedJobReopener.Verify(x => x.ValidateClosedJob(It.IsAny<ZPropertyInfo>(), It.IsAny<Job>()), Times.Never());
			}
		}

		public void TestCheckJR_JH_InternalJob_ValidateClosedJob_WhenInternalJobIsValidAndNoAutoJRJ()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var currentCompanyJob = TestObjectCreator.CreateJobHeader();
			currentCompanyJob.JH_GC = GlbCompany.CurrentCompany.PK;

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();

			var sellAccount = GlbCompany.CurrentCompany.OrgProxy;
			var branch = TestObjectCreator.CreateBranch("NEW", GlbCompany.CurrentCompany, sellAccount);
			var shipment = TestObjectCreator.CreateShipment("S0001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100, 100);
				charge.JR_JH_InternalJob = currentCompanyJob.PK;

				Assert("Precondition: ShouldCreateJRJ", !charge.ShouldCreateJRJ);
				Assert("Precondition: JR_JH_InternalJob.IsValid", charge.JR_JH_InternalJob.IsValid);

				using (charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
				{
					charge.JR_Calc_RelatedJobNumber = shipment.JobNumber;
					charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
					charge.JR_GB = branch.PK;
					charge.JR_GB_InternalBranch = branch.PK;
					charge.JR_GE = TestObjectCreator.FEADepartment.PK;
					charge.JR_GE_InternalDept = TestObjectCreator.FEADepartment.PK;
				}

				Assert("Precondition: ShouldCreateJRJ", charge.ShouldCreateJRJ);
				mockClosedJobReopener.Verify(x => x.ValidateClosedJob(It.IsAny<ZPropertyInfo>(), It.IsAny<Job>()), Times.Never());

				job.SetupDependency(mockClosedJobReopener.Object);

				charge.Validation.ValidateJR_JH_InternalJob();
				mockClosedJobReopener.Verify(x => x.ValidateClosedJob(charge.JR_JH_InternalJobInfo, job), Times.Once());
			}
		}

		public void TestCheckJR_JH_InternalJob_ValidateClosedJob_WhenInternalJobIsValidAndShouldCreateSellJRJIsTrue_ClosedJobReopener()
		{
			TestValidateClosedJob_WhenInternalJobIsValidAndCreateJRJIsTrue_ClosedJobReopener(true);
		}

		public void TestCheckJR_JH_InternalJob_ValidateClosedJob_WhenInternalJobIsValidAndShouldCreateCostJRJIsTrue_ClosedJobReopener()
		{
			TestValidateClosedJob_WhenInternalJobIsValidAndCreateJRJIsTrue_ClosedJobReopener(false);
		}

		void TestValidateClosedJob_WhenInternalJobIsValidAndCreateJRJIsTrue_ClosedJobReopener(bool shouldCreateSellJRJ)
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			var currentCompanyJob = TestObjectCreator.CreateJobHeader();
			currentCompanyJob.JH_GC = GlbCompany.CurrentCompany.PK;

			var mockClosedJobReopener = new Mock<IClosedJobReopener>();

			var account = shouldCreateSellJRJ
				? GlbCompany.CurrentCompany.OrgProxy
				: GlbCompany.CurrentCompany.OrgProxy;

			var branch = TestObjectCreator.CreateBranch("NEW", GlbCompany.CurrentCompany, account);
			var shipment = TestObjectCreator.CreateShipment("S0001");

			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, 100, 100);
				using (charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
				{
					charge.JR_Calc_RelatedJobNumber = shipment.JobNumber;
					charge.JR_OH_SellAccount = shouldCreateSellJRJ ? account.PK : Guid.Empty;
					charge.JR_OH_CostAccount = shouldCreateSellJRJ ? Guid.Empty : account.PK;
					charge.JR_JH_InternalJob = currentCompanyJob.PK;
					charge.JR_GB = branch.PK;
					charge.JR_GB_InternalBranch = branch.PK;
					charge.JR_GE = TestObjectCreator.FEADepartment.PK;
					charge.JR_GE_InternalDept = TestObjectCreator.FEADepartment.PK;
				}

				Assert("Precondition: Internal Job Error", !charge.JR_JH_InternalJobInfo.HasErrors());
				AssertNoWarnings("Precondition: Internal Job Warning", charge.JR_JH_InternalJobInfo);
				Assert("Precondition: ShouldCreateJRJ", charge.ShouldCreateJRJ);
				Assert(shouldCreateSellJRJ == charge.ShouldCreateSellJRJ);
				Assert(shouldCreateSellJRJ != charge.ShouldCreateCostJRJ);

				charge.Validation.ValidateJR_JH_InternalJob();

				mockClosedJobReopener.Verify(x => x.ValidateClosedJob(It.IsAny<ZPropertyInfo>(), It.IsAny<Job>()), Times.Never());

				job.SetupDependency(mockClosedJobReopener.Object);

				charge.Validation.ValidateJR_JH_InternalJob();
				mockClosedJobReopener.Verify(x => x.ValidateClosedJob(charge.JR_JH_InternalJobInfo, job), Times.Once());
			}
		}

		public void TestPropertyInfosNeedToBeValidateWhenPostCost()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S0000001"), false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge", TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI, "INV001", TestObjectCreator.USD, 100M);

			AssertEquals(31, charge.Validation.PropertyInfosNeedToBeValidateWhenPostCost.Count);

			int allValidationMethodCount = 0;
			foreach (ZPropertyInfo info in charge.ZPropertyInfoHash)
			{
				if (charge.Validation.GetType().GetMethod("Validate" + info.Name) != null)
				{
					allValidationMethodCount++;
				}
			}

			AssertEquals(@"Note to developer: if you add some validate methods for charge, please check whether the related property should be add to PropertyInfosNeedToBeValidateWhenPostCost
Properties need to be validate:
	JR_GBInfo,
	JR_GEInfo,
	JR_ACInfo,
	JR_DescInfo,
	JR_OSCostAmtInfo,
	JR_LocalCostAmtInfo,
	JR_OH_CostAccountInfo,
	JR_OSCostExRateInfo,
	JR_AT_CostGSTRateInfo,
	JR_APInvoiceDateInfo,
	JR_APInvoiceNumInfo,
	JR_PaymentDateInfo,
	JR_RX_NKCostCurrencyInfo,
	JR_PaymentTypeInfo,
	JR_AKInfo,
	JR_ABInfo,
	JR_PreventInvoicePrintGroupingInfo,
	JR_OSCostGSTAmt_CalcInfo,
	JR_CostGovtChargeCodeInfo,
	JR_A9_CostVATClassInfo,
	JR_CostPlaceOfSupplyInfo,
	JR_CostPlaceOfSupplyTypeInfo,
	JR_AW_CostWHTRateInfo,
	JR_CostReferenceInfo,
	JR_APDocumentReceivedDateInfo,
	JR_CostTaxDateInfo,
	JR_AgentDeclaredCostAmtInfo,
	JR_CostSupplyTypeInfo,
	JR_GB_CostTaxBranchInfo,
	JR_IsAPCashAdvanceInfo,
	JR_CAL_APLineInfo

We have not added validation for Revenue related Properties Since BasePostManager.ProcessEligibleCharges will call charge.ValidateAll", 96, allValidationMethodCount);
		}

		public void TestValidateCostPropertyInfos()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S0000001"), false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge", TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI, "INV001", TestObjectCreator.USD, 100M);
			var result = charge.Validation.ValidateCostPropertyInfos();
			AssertEquals(0, result.Length);

			charge.JR_GB = ZGuid.Empty;
			charge.JR_GE = ZGuid.Empty;
			result = charge.Validation.ValidateCostPropertyInfos();
			AssertEquals(1, result.Length);
			AssertEquals("Please enter a Branch.", result[0]);
		}

		#region Implementation

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
