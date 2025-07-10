using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	internal class ApportionSplitChargeValidationTest : BaseChargeValidationTest
	{
		public void TestCheckJR_AC()
		{
			var chargeCMT = TestObjectCreator.CreateChargeCode("CMT1", "CMT Test", Core.Constants.ChargeType.Comment, 1m, null, null);
			var chargeDSB = TestObjectCreator.CreateChargeCode("DSB1", "DSB Test", Core.Constants.ChargeType.Disbursement, 1m, null, null);
			var chargeMJA = TestObjectCreator.CreateChargeCode("MJA1", "MJA Test", Core.Constants.ChargeType.ManualJobAccrual, 1m, null, null);
			var chargeMRG = TestObjectCreator.CreateChargeCode("MRG1", "MRG Test", Core.Constants.ChargeType.Margin, 1m, null, null);
			var chargeNON = TestObjectCreator.CreateChargeCode("NON1", "NON Test", Core.Constants.ChargeType.NonAccrual, 1m, null, null);
			var chargeOVR = TestObjectCreator.CreateChargeCode("OVR1", "OVR Test", Core.Constants.ChargeType.Overhead, 1m, null, null);
			var chargeREV = TestObjectCreator.CreateChargeCode("REV1", "REV Test", Core.Constants.ChargeType.Revenue, 1m, null, null);

			var testSplitCharge1 = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge1.JR_AC = chargeCMT.PK;
			AssertHasError("Charge Code should have an error", testSplitCharge1.JR_ACInfo, "This charge code has a charge type of 'CMT' and cannot be used in consol costing.");

			var testSplitCharge2 = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge2.JR_AC = chargeDSB.PK;
			AssertNoErrors("Charge Code should NOT have an error", testSplitCharge2.JR_ACInfo);

			var testSplitCharge3 = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge3.JR_AC = chargeMJA.PK;
			AssertNoErrors("Charge Code should NOT have an error", testSplitCharge3.JR_ACInfo);

			var testSplitCharge4 = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge4.JR_AC = chargeMRG.PK;
			AssertNoErrors("Charge Code should NOT have an error", testSplitCharge4.JR_ACInfo);

			var testSplitCharge5 = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge5.JR_AC = chargeNON.PK;
			AssertHasError("Charge Code should have an error", testSplitCharge5.JR_ACInfo, "This charge code has a charge type of 'NON' and cannot be used in consol costing.");

			var testSplitCharge6 = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge6.JR_AC = chargeOVR.PK;
			AssertHasError("Charge Code should have an error", testSplitCharge6.JR_ACInfo, "This charge code has a charge type of 'OVR' and cannot be used in consol costing.");

			var testSplitCharge7 = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge7.JR_AC = chargeREV.PK;
			AssertHasError("Charge Code should have an error", testSplitCharge7.JR_ACInfo, "This charge code has a charge type of 'REV' and cannot be used in consol costing.");

			var shipment = TestObjectCreator.CreateShipment("S00001234", "AUSYD", "NZAKL");
			var job = TestObjectCreator.CreateJob(shipment, false);

			testSplitCharge4.JR_ChargeType = Core.Constants.ChargeType.Revenue; // simulate someone has posted revenue using a REV charge code, then changed the charge code to MRG.
			testSplitCharge4.Validation.ValidateJR_AC();
			AssertNoErrors("Charge Code should NOT have an error", testSplitCharge4.JR_ACInfo); // make sure we're not using the saved value of JR_ChargeType

			var chargeMRGWithRevOverride = TestObjectCreator.CreateChargeCode("MRG2", "MRG Test", Core.Constants.ChargeType.Margin, 1m, null, null);

			var chargeMRGTypeOverride = new BusinessObjectFactory().Load<AccChargeCode>(chargeMRGWithRevOverride.PK).ChargeTypeOverrides.AddNew();
			chargeMRGTypeOverride.AN_JobDirection = "ALL";
			chargeMRGTypeOverride.AN_JobType = "ALL";
			chargeMRGTypeOverride.AN_ChargeType = Core.Constants.ChargeType.Revenue;
			chargeMRGTypeOverride.Factory.Save();

			var testSplitCharge8 = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge8.JR_JH = job.PK;
			testSplitCharge8.JR_AC = chargeMRGWithRevOverride.PK;
			AssertHasError("Charge Code should have an error", testSplitCharge8.JR_ACInfo, "This charge code has a charge type of 'REV' and cannot be used in consol costing.");
		}

		public void TestCheckJR_JobNumberNoWarning()
		{
			bool oldAllowed = Env.Security.ReopenJob.IsAllowed;

			try
			{
				Job shipmentJob = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				ApportionSplitCharge testSplitCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
				testSplitCharge.JR_JH = shipmentJob.PK;
				testSplitCharge.JR_OSCostAmt = 10;
				testSplitCharge.JR_IsUsedForApportionment = true;

				((ApportionSplitChargeValidation)testSplitCharge.Validation).ValidateJR_JobNumber();
				AssertEquals(false, testSplitCharge.JR_JobNumberInfo.HasWarnings());

				((ApportionSplitChargeValidation)testSplitCharge.Validation).ValidateJR_JobNumber();
				AssertEquals("Job Status = Working, Should have NO warning", false, testSplitCharge.JR_JobNumberInfo.HasWarnings());
			}
			finally
			{
				Env.Security.ReopenJob.IsAllowed = oldAllowed;
			}
		}

		public void TestCheckJR_IsUsedForApportionmentNoShipmentInfoError()
		{
			ApportionSplitCharge testSplitCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge.JR_OSCostAmt = 10;
			testSplitCharge.JR_IsUsedForApportionment = true;

			((ApportionSplitChargeValidation)testSplitCharge.Validation).ValidateJR_IsUsedForApportionment();
			AssertHasError($"{ApportionSplitCharge.Schema.JR_IsUsedForApportionment} should have error",
							testSplitCharge.JR_IsUsedForApportionmentInfo,
							"This apportioned charge belongs to job: " + testSplitCharge.JR_JobNumber + " which is no longer attached to the consol or is inactive. Please untick Is Used or re-attach job to consol/activate job.");
		}

		public void TestCheckJR_IsUsedForApportionmentNoShipmentInfoWarning()
		{
			ApportionSplitCharge testSplitCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge.JR_OSCostAmt = 10;
			testSplitCharge.JR_IsUsedForApportionment = true;
			testSplitCharge.JR_AL_APLine = Factory.New<APInvoiceLine>().PK;

			((ApportionSplitChargeValidation)testSplitCharge.Validation).ValidateJR_IsUsedForApportionment();
			AssertHasWarning($"{ApportionSplitCharge.Schema.JR_IsUsedForApportionment} should have warning",
							 testSplitCharge.JR_IsUsedForApportionmentInfo,
							 "This posted apportioned charge belongs to job: " + testSplitCharge.JR_JobNumber + " which is no longer attached to the consol or is inactive.");
		}

		public void TestValidateAllGeneratesErrorOnJR_IsUsedForApportionment()
		{
			ApportionSplitCharge testSplitCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge.JR_OSCostAmt = 10;
			testSplitCharge.JR_IsUsedForApportionment = true;

			testSplitCharge.Validation.ValidateAll();
			AssertEquals($"{ApportionSplitCharge.Schema.JR_IsUsedForApportionment} should have error", true, testSplitCharge.JR_IsUsedForApportionmentInfo.HasErrors());
		}

		public void TestCheckJR_IsUsedForApportionmentNoShipmentInfoErrorOnAPInvoice()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001004", "AUSYD", "NZAKL", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S00001005", "AUSYD", "NZAKL", consol);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100);
			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			var consolInOtherFactory = otherFactory.Load<ForwardingConsol>(consol.PK);
			var shipmentInOtherFactory = otherFactory.Load<ForwardingShipment>(shipment1.PK);
			consolInOtherFactory.Shipments.Remove(shipmentInOtherFactory);
			otherFactory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var apInvoice = newFactory.New<APInvoice>();
			var consolCostInNewFactory = newFactory.Load<JobConsolCost>(consolCost.PK);

			apInvoice.ConsolCosting.ConsolCosts.Add(consolCostInNewFactory);

			apInvoice.ConsolCosting.ConsolCosts.RunPreSaveValidation();
			var restoredConsolCost = apInvoice.ConsolCosting.ConsolCosts[0];
			var charge = restoredConsolCost.ApportionmentCharges.FindChargeForJob(shipment1);
			AssertHasError(charge.JR_IsUsedForApportionmentInfo, "This apportioned charge belongs to job: " + charge.JR_JobNumber + " which is no longer attached to the consol or is inactive. Please untick Is Used or re-attach job to consol/activate job.");
		}

		public void TestCheckJR_IsUsedForApportionmentHasNoErrorsAfterUnticking()
		{
			ApportionSplitCharge testSplitCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge.JR_OSCostAmt = 10;
			testSplitCharge.JR_IsUsedForApportionment = true;

			String message = "This apportioned charge belongs to job: " + testSplitCharge.JR_JobNumber + " which is no longer attached to the consol or is inactive. Please untick Is Used or re-attach job to consol/activate job.";
			((ApportionSplitChargeValidation)testSplitCharge.Validation).ValidateJR_IsUsedForApportionment();
			AssertHasError($"{ApportionSplitCharge.Schema.JR_IsUsedForApportionment} should have error",
							testSplitCharge.JR_IsUsedForApportionmentInfo,
							message);

			testSplitCharge.JR_IsUsedForApportionment = false;
			((ApportionSplitChargeValidation)testSplitCharge.Validation).ValidateJR_IsUsedForApportionment();
			AssertNoError($"{ApportionSplitCharge.Schema.JR_IsUsedForApportionment} should not have error",
							testSplitCharge.JR_IsUsedForApportionmentInfo,
							message);
		}

		public void TestCheckJR_JobNumberWarning()
		{
			bool oldAllowed = Env.Security.ReopenJob.IsAllowed;

			try
			{
				Job shipmentJob = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Closed.Code);
				ApportionSplitCharge testSplitCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
				testSplitCharge.JR_JH = shipmentJob.PK;
				testSplitCharge.JR_OSCostAmt = 10;
				testSplitCharge.JR_IsUsedForApportionment = true;

				Env.Security.ReopenJob.IsAllowed = true;
				ApportionSplitChargeValidation testValidation = (ApportionSplitChargeValidation)testSplitCharge.Validation;
				testValidation.ValidateJR_JobNumber();
				AssertEquals(false, testSplitCharge.JR_JobNumberInfo.HasWarning(testValidation.ReopenClosedJobSecurityMessage));
				AssertEquals(true, testSplitCharge.JR_JobNumberInfo.HasWarning(testValidation.ReopenClosedJobWarningMessage));

				Env.Security.ReopenJob.IsAllowed = false;
				testValidation.ValidateJR_JobNumber();
				AssertEquals(true, testSplitCharge.JR_JobNumberInfo.HasWarning(testValidation.ReopenClosedJobSecurityMessage));
				AssertEquals(false, testSplitCharge.JR_JobNumberInfo.HasWarning(testValidation.ReopenClosedJobWarningMessage));
			}
			finally
			{
				Env.Security.ReopenJob.IsAllowed = oldAllowed;
			}
		}

		public void TestCheckJR_AB()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			TestObjectCreator objectCreator = new TestObjectCreator(Factory);

			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_GB = objectCreator.NonCurrentCompanyBranch.PK;

			ForwardingShipment shipment1 = consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();

				ApportionSplitCharge testSplitCharge = cost.ApportionmentCharges[0];

				cost.E6_AB_BankAccount = objectCreator.USDBankAccount.PK;
				AssertNoError("All currency Bank Accounts is allowed now.", testSplitCharge.JR_ABInfo, E6_AB_BankAccountInvalidBranchError);

				cost.E6_AB_BankAccount = objectCreator.AUDBankAccount.PK;
				cost.E6_AB_BankAccount = bankAccount.PK;
				AssertHasError(testSplitCharge.JR_ABInfo, E6_AB_BankAccountInvalidBranchError);

				cost.E6_AB_BankAccount = objectCreator.AUDBankAccount.PK;
				AssertNoErrors(testSplitCharge.JR_ABInfo);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestBranchLevelPostingValidationShouldOnlyValidateApportionChargesOfUnpostedConsolCosts()
		{
			var expectedError = @"Please review the Consol Cost allocations and ensure all charges within each apportionment are assigned branch from the same Posting Group. All charges posted in one transaction must be within the same Branch Posting Group.
Saving is prevented because charges for the same apportionment have been entered using a mix of branch Posting Groups.";

			var consol = TestObjectCreator.CreateConsol();
			Factory.Save();

			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var apps = new ApportionmentListing(Factory, consol);

			try
			{
				var consolCost1 = apps.CostsCollection.TryAddNew();
				consolCost1.E6_OSCostAmount = 100M;

				var charge1 = consolCost1.ApportionmentCharges[0];
				var charge2 = consolCost1.ApportionmentCharges[1];

				var consolCost2 = apps.CostsCollection.TryAddNew();
				consolCost2.E6_OSCostAmount = 200M;

				var charge3 = consolCost2.ApportionmentCharges[0];
				var charge4 = consolCost2.ApportionmentCharges[1];

				var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
				AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

				charge1.JR_GB = branch1.PK;
				charge2.JR_GB = branch2.PK;
				charge3.JR_GB = branch1.PK;
				charge4.JR_GB = branch2.PK;

				Assert(!consolCost1.IsPosted);
				Assert(!consolCost2.IsPosted);

				AssertHasError("Charge1 should have error because consolCost1 is not posted", charge1.JR_GBInfo, expectedError);
				AssertHasError("Charge2 should have error because consolCost1 is not posted", charge2.JR_GBInfo, expectedError);
				AssertHasError("Charge3 should have error because consolCost2 is not posted", charge3.JR_GBInfo, expectedError);
				AssertHasError("Charge4 should have error because consolCost2 is not posted", charge4.JR_GBInfo, expectedError);

				var apInvoice = Factory.NewWithValidTestData<APInvoice>();
				consolCost2.E6_AH_APInvoice = apInvoice.PK;

				Assert(!consolCost1.IsPosted);
				Assert(consolCost2.IsPosted);

				consolCost1.RunPreSaveValidation();
				consolCost2.RunPreSaveValidation();

				AssertHasError("Charge1 should have error because consolCost1 is not posted", charge1.JR_GBInfo, expectedError);
				AssertHasError("Charge2 should have error because consolCost1 is not posted", charge2.JR_GBInfo, expectedError);
				AssertNoError("Charge3 should not have error because consolCost2 is posted", charge3.JR_GBInfo, expectedError);
				AssertNoError("Charge4 should not have error because consolCost2 is posted", charge4.JR_GBInfo, expectedError);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCheckJR_GB_ValidationFor_BranchLevelPostingEnabled()
		{
			var expectedError = @"Please review the Consol Cost allocations and ensure all charges within each apportionment are assigned branch from the same Posting Group. All charges posted in one transaction must be within the same Branch Posting Group.
Saving is prevented because charges for the same apportionment have been entered using a mix of branch Posting Groups.";

			ForwardingConsol consol = TestObjectCreator.CreateConsol();
			Factory.Save();

			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_OSCostAmount = 100M;

				var splitCharge1 = cost.ApportionmentCharges[0];
				var splitCharge2 = cost.ApportionmentCharges[1];

				foreach (var registryValue in new bool[] { true, false })
				{
					var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = registryValue };
					AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

					splitCharge1.JR_GB = branch1.PK;
					splitCharge2.JR_GB = branch2.PK;

					if (registryValue)
					{
						AssertHasError("splitCharge2 should have error as the two apportionemnts have different branches when the registry is turned on", splitCharge2.JR_GBInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", splitCharge2.JR_GBInfo, expectedError);
					}

					splitCharge2.JR_IsUsedForApportionment = false;

					if (registryValue)
					{
						AssertNoError("splitCharge2 should no longer have error as the second apportioned charge is not in use when the registry is turned on", splitCharge2.JR_GBInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", splitCharge2.JR_GBInfo, expectedError);
					}

					splitCharge2.JR_IsUsedForApportionment = true;

					if (registryValue)
					{
						AssertHasError("splitCharge2 should have error as the two apportionemnts have different branches when the registry is turned on", splitCharge2.JR_GBInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", splitCharge2.JR_GBInfo, expectedError);
					}

					splitCharge2.JR_GB = branch1.PK;

					if (registryValue)
					{
						AssertNoError("splitCharge2 should no longer have error as the two apportionemnts now have same branches when the registry is turned on", splitCharge2.JR_GBInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", splitCharge2.JR_GBInfo, expectedError);
					}

					splitCharge2.JR_GB = branch2.PK;

					if (registryValue)
					{
						AssertHasError("splitCharge2 should have error as the two apportionemnts have different branches when the registry is turned on", splitCharge2.JR_GBInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", splitCharge2.JR_GBInfo, expectedError);
					}

					branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
					var settings1 = new BranchGroupSettings();
					settings1.BranchPK = branch1.PK;
					settings1.GroupNumber = 1;
					settings1.IsParentBranch = false;

					var settings2 = new BranchGroupSettings();
					settings2.BranchPK = branch2.PK;
					settings2.GroupNumber = 1;
					settings2.IsParentBranch = true;

					branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
					branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

					AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

					splitCharge2.RunPreSaveValidation();

					if (registryValue)
					{
						AssertNoError("splitCharge2 should no longer have error as the two apportionemnts now have same branches when the registry is turned on", splitCharge2.JR_GBInfo, expectedError);
					}
					else
					{
						AssertNoError("splitCharge2 should not have any error since the registry is turned off", splitCharge2.JR_GBInfo, expectedError);
					}
				}
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCheckJR_GB_ConsolCosting_ValidationFor_AllChragesAreSameEligibleAutoJRJ()
		{
			const string expectedError = @"At least one apportioned charge contains a Branch with a different tax registration to the Creditor.
Please clear the Internal Job/Branch/Department to allow posting this apportioned charge as an AP Invoice.";

			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly();

			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var registeredNumberCodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			AssertEquals("PreCondition", CountryCodes.Australia, currentCountryCode);

			const string registeredNumberGroup1 = "21 003 980 130";
			const string registeredNumberGroup2 = "21 003 980 130 123";

			var branchForRegisteredNumberGroup1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			branchForRegisteredNumberGroup1.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.ABIGAS, registeredNumberCodeType, currentCountryCode, registeredNumberGroup1);

			var branchForRegisteredNumberGroup2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);
			branchForRegisteredNumberGroup2.GB_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.SetCustomsCodeForOrgHeader(TestObjectCreator.AALSHI, registeredNumberCodeType, currentCountryCode, registeredNumberGroup2);

			var consol = TestObjectCreator.CreateConsol();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			GlbBranch.CurrentBranch.Factory.Save();
			Factory.Save();

			var apps = new ApportionmentListing(Factory, consol);
			try
			{
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_OSCostAmount = 100M;

				var splitCharge1 = cost.ApportionmentCharges[0];
				var splitCharge2 = cost.ApportionmentCharges[1];

				cost.E6_OH_Creditor = ZGuid.Empty;
				splitCharge1.JR_GB = branchForRegisteredNumberGroup1.PK;
				splitCharge2.JR_GB = branchForRegisteredNumberGroup2.PK;
				Assert("PreCondition", cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_IsUsedForApportionment));
				CombineAssertions("Do not validate when missing one of criterias, the consol costing creditor is empty.", () => {
					splitCharge1.Validation.ValidateJR_GB();
					AssertNoError(splitCharge1.JR_GBInfo, expectedError);
					splitCharge2.Validation.ValidateJR_GB();
					AssertNoError(splitCharge2.JR_GBInfo, expectedError);
				});

				cost.E6_OH_Creditor = branchForRegisteredNumberGroup1.OrgProxy.PK;
				splitCharge1.JR_GB = ZGuid.Empty;
				splitCharge2.JR_GB = branchForRegisteredNumberGroup2.PK;
				Assert("PreCondition", cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_IsUsedForApportionment));
				Assert("PreCondition, all charges are not eligible.", cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.IsInternalJobInfoDisabled));
				CombineAssertions("Do not validate when missing one of criterias, all charges are not JRJ eligible.", () => {
					splitCharge1.Validation.ValidateJR_GB();
					AssertNoError(splitCharge1.JR_GBInfo, expectedError);
					splitCharge2.Validation.ValidateJR_GB();
					AssertNoError(splitCharge2.JR_GBInfo, expectedError);
				});

				cost.E6_OH_Creditor = branchForRegisteredNumberGroup1.OrgProxy.PK;
				splitCharge1.JR_GB = branchForRegisteredNumberGroup1.PK;
				splitCharge2.JR_GB = branchForRegisteredNumberGroup1.PK;
				Assert("PreCondition", cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_IsUsedForApportionment));
				Assert("PreCondition, all charges are eligible.", cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => !x.IsInternalJobInfoDisabled));
				Assert("PreCondition, charges have internal fields value", cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => !splitCharge1.AreInternalFieldsEmpty()));
				CombineAssertions("Do not validate when missing one of criterias, all the charges are JRJ eligible and have internal fields value.", () => {
					splitCharge1.Validation.ValidateJR_GB();
					AssertNoError(splitCharge1.JR_GBInfo, expectedError);
					splitCharge2.Validation.ValidateJR_GB();
					AssertNoError(splitCharge2.JR_GBInfo, expectedError);
				});

				cost.E6_OH_Creditor = branchForRegisteredNumberGroup1.OrgProxy.PK;
				splitCharge1.JR_GB = branchForRegisteredNumberGroup2.PK;
				splitCharge2.JR_GB = branchForRegisteredNumberGroup2.PK;
				Assert("PreCondition", cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_IsUsedForApportionment));
				Assert("PreCondition, all charges are not JRJ eligible.", cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.IsInternalJobInfoDisabled));
				CombineAssertions("Do not validate when missing one of criterias, all the charges are not JRJ eligible.", () => {
					splitCharge1.Validation.ValidateJR_GB();
					AssertNoError(splitCharge1.JR_GBInfo, expectedError);
					splitCharge2.Validation.ValidateJR_GB();
					AssertNoError(splitCharge2.JR_GBInfo, expectedError);
				});

				cost.E6_OH_Creditor = branchForRegisteredNumberGroup1.OrgProxy.PK;
				splitCharge1.JR_GB = branchForRegisteredNumberGroup1.PK;
				splitCharge2.JR_GB = branchForRegisteredNumberGroup2.PK;
				Assert("PreCondition, charge1 is JRJ eligible.", !splitCharge1.IsInternalJobInfoDisabled);
				Assert("PreCondition, charge2 is not JRJ eligible.", splitCharge2.IsInternalJobInfoDisabled);
				CombineAssertions("Do the validation and only get error for charge which is JRJ eligible but internal fields are not all blank, when any other apportionment charge is not JRJ eligible.", () => {
					using (splitCharge1.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
					{
						splitCharge1.JR_JH_InternalJob = splitCharge1.JR_JH;
						splitCharge1.JR_GB_InternalBranch = splitCharge1.JR_GB;
						splitCharge1.JR_GE_InternalDept = splitCharge1.JR_GE;
					}
					Assert("PreCondition, charge have internal fields value.", !splitCharge1.AreInternalFieldsEmpty());

					splitCharge2.JR_IsUsedForApportionment = false;
					splitCharge1.JR_IsUsedForApportionment = true;
					splitCharge1.Validation.ValidateJR_GB();
					AssertNoError("No error because we only consider apportionment charges in use.", splitCharge1.JR_GBInfo, expectedError);

					splitCharge2.JR_IsUsedForApportionment = true;
					splitCharge1.JR_IsUsedForApportionment = false;
					splitCharge1.Validation.ValidateJR_GB();
					AssertNoError("No error because charge is not used.", splitCharge1.JR_GBInfo, expectedError);

					splitCharge1.JR_IsUsedForApportionment = true;
					splitCharge2.JR_IsUsedForApportionment = true;
					Assert("PreCondition", cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_IsUsedForApportionment));

					var dummyGuidForGatewaySellHeader = new ZGuid("C7236D15-9B6F-4C45-90CB-0D98DBBD9136");
					splitCharge1.JR_E6_GatewaySellHeader = dummyGuidForGatewaySellHeader;
					splitCharge2.JR_E6_GatewaySellHeader = dummyGuidForGatewaySellHeader;
					AssertEquals("PreCondition", true, splitCharge1.IsGatewaySellApportionmentCharge);
					AssertEquals("PreCondition", true, splitCharge2.IsGatewaySellApportionmentCharge);
					splitCharge1.Validation.ValidateJR_GB();
					AssertNoError("[Gateway Sell Apportionment] Do not run this validation for gateway sell apportionment charge, since it have orginal validation.", splitCharge1.JR_GBInfo, expectedError);

					splitCharge1.JR_E6_GatewaySellHeader = ZGuid.Empty;
					splitCharge2.JR_E6_GatewaySellHeader = ZGuid.Empty;
					AssertEquals("PreCondition", false, splitCharge1.IsGatewaySellApportionmentCharge);
					AssertEquals("PreCondition", false, splitCharge2.IsGatewaySellApportionmentCharge);
					splitCharge1.Validation.ValidateJR_GB();
					AssertHasError("[ConsolCosting]Have error because it is JRJ eligible and internal fields are not all blank.", splitCharge1.JR_GBInfo, expectedError);

					splitCharge2.Validation.ValidateJR_GB();
					AssertNoError(splitCharge2.JR_GBInfo, expectedError);

					using (splitCharge1.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
					{
						splitCharge1.JR_JH_InternalJob = ZGuid.Empty;
						splitCharge1.JR_GB_InternalBranch = ZGuid.Empty;
						splitCharge1.JR_GE_InternalDept = ZGuid.Empty;
					}
					Assert("PreCondition", cost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => x.JR_IsUsedForApportionment));
					Assert("PreCondition, charge do not have internal fields value.", splitCharge1.AreInternalFieldsEmpty());
					splitCharge1.Validation.ValidateJR_GB();
					AssertNoError("No error because it is JRJ eligible but internal field are all blank.", splitCharge1.JR_GBInfo, expectedError);

					splitCharge2.Validation.ValidateJR_GB();
					AssertNoError(splitCharge2.JR_GBInfo, expectedError);
				});
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCheckJR_ABIsValidZGuid()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			TestObjectCreator objectCreator = new TestObjectCreator(Factory);

			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_GB = objectCreator.NonCurrentCompanyBranch.PK;

			ForwardingShipment shipment1 = consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();

				ApportionSplitCharge testSplitCharge = cost.ApportionmentCharges[0];

				cost.E6_AB_BankAccount = ZGuid.Missing;
				AssertHasError(testSplitCharge.JR_ABInfo, E6_AB_BankAccountInvalidBranchError);

				cost.E6_AB_BankAccount = ZGuid.Invalid;
				AssertHasError(testSplitCharge.JR_ABInfo, "Enter a valid Bank Account.");

				cost.E6_AB_BankAccount = objectCreator.AUDBankAccount.PK;
				AssertNoErrors(testSplitCharge.JR_ABInfo);
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}
		const string E6_AB_BankAccountInvalidBranchError = "Please select bank account with appropriate branch and the same currency as the Consol Costing's AP Invoice.";

		public void TestCheckJR_OSCostAmt()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();

			ApportionSplitCharge testSplitCharge = cost.ApportionmentCharges[0];

			cost.E6_OSCostAmount = 2;
			testSplitCharge.JR_OSCostAmt = 1;

			ApportionSplitChargeValidation testValidation = (ApportionSplitChargeValidation)testSplitCharge.Validation;
			testValidation.ValidateJR_OSCostAmt();
			AssertHasError(testSplitCharge.JR_OSCostAmtInfo, testValidation.ApportionedAmountSumErrorMessage);

			cost.E6_OSCostAmount = 1;
			testSplitCharge.Validation.ValidateJR_OSCostAmt();
			AssertNoError(testSplitCharge.JR_OSCostAmtInfo, testValidation.ApportionedAmountSumErrorMessage);

			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;

			Factory.Save();

			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_LineType = "CST";
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			cost.ParentAPInvoice = invoice;
			cost.E6_AH_APInvoice = invoice.PK;
			cost.ApportionmentCharges[0].ReverseAccrual(ZDateTime.Now);
			cost.ApportionmentCharges[0].JR_AL_APLine = line.PK;
			cost.ApportionmentCharges[0].SetAmountsToLinkedLinesForTests();

			testSplitCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			testSplitCharge.JR_OSCostAmt = 100;

			Factory.Save();

			testSplitCharge.JR_OSCostAmt = 0;
			testSplitCharge.JR_E6 = cost.PK;

			testSplitCharge.Validation.ValidateJR_OSCostAmt();
			AssertHasError(testSplitCharge.JR_OSCostAmtInfo, "The value can not be set to zero. The original value was not equal zero.");
		}

		public void TestCheckAportionSplitChargeJR_LocalCostAmt()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();

			ApportionSplitCharge testSplitCharge = cost.ApportionmentCharges[0];

			cost.E6_LocalCostAmount = 2;
			testSplitCharge.JR_LocalCostAmt = 1;

			ApportionSplitChargeValidation testValidation = (ApportionSplitChargeValidation)testSplitCharge.Validation;
			testValidation.ValidateJR_LocalCostAmt();
			AssertHasError(testSplitCharge.JR_LocalCostAmtInfo, testValidation.ApportionedLocalAmountSumErrorMessage);

			cost.E6_LocalCostAmount = 1;
			testSplitCharge.Validation.ValidateJR_LocalCostAmt();
			AssertNoError(testSplitCharge.JR_LocalCostAmtInfo, testValidation.ApportionedLocalAmountSumErrorMessage);

			cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;

			Factory.Save();
		}

		public void TestDepartmentValidationOnChargeCode()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);

			AccTaxRate gST = objectCreator.CreateTaxRate("GST", "GSTRate", 10);
			AccWithholding wHT = objectCreator.CreateOrLoadWithholdingTax("WHT", "WHTRate", 5);

			AccChargeCode currentDepartmentChargeCode = objectCreator.CreateChargeCode("MRG100_1", "Margin100%", Constants.ChargeType.Margin, 100, gST, wHT, GlbDepartment.CurrentDepartment.GE_Code);
			AccChargeCode nonCurrentDepartmentChargeCode = objectCreator.CreateChargeCode("MRG100_2", "Margin100%", Constants.ChargeType.Margin, 100, gST, wHT, "TE, OSEC");
			AccChargeCode allDepartmentsChargeCode = objectCreator.CreateChargeCode("MRG100_3", "Margin100%", Constants.ChargeType.Margin, 100, gST, wHT, "ALL");

			ApportionSplitCharge testCharge = Factory.New<ApportionSplitCharge>();
			testCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			testCharge.JR_LocalSellAmt = 50.00m;
			testCharge.JR_IsUsedForApportionment = true;

			testCharge.JR_AC = currentDepartmentChargeCode.PK;
			Assert("Department should not be in error", !testCharge.JR_GEInfo.HasErrors());

			testCharge.JR_AC = nonCurrentDepartmentChargeCode.PK;
			Assert("Department should be in error", testCharge.JR_GEInfo.HasErrors());

			testCharge.JR_AC = allDepartmentsChargeCode.PK;
			Assert("Department should not be in error", !testCharge.JR_GEInfo.HasErrors());
		}

		public void TestDepartmentValidationWhenChargeCodeChange()
		{
			var consol = TestObjectCreator.CreateConsol("C0001");
			var shipment = TestObjectCreator.CreateShipment("S00001004", consol);
			var apportionmentListing = new ApportionmentListing(Factory, consol);
			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.DSBChargeCode, 100);
			Factory.Save();

			var testSplitCharge = cost.ApportionmentCharges[0];

			testSplitCharge.JR_GE = TestObjectCreator.FEADepartment.PK;
			Factory.Save();

			var checkErrorMessage = "This department is not valid for the charge code specified on this Job.";
			var nonCurrentDepartmentChargeCode = TestObjectCreator.CreateChargeCode("MRG100_2", "Margin100%", Constants.ChargeType.Margin, 100, TestObjectCreator.GST1, null, "TE, OSEC");

			testSplitCharge = cost.ApportionmentCharges[0];
			testSplitCharge.JR_AC = TestObjectCreator.DSBChargeCode1.PK;
			Assert("Department should not be in error", !testSplitCharge.JR_GEInfo.HasError(checkErrorMessage));

			testSplitCharge.JR_AC = nonCurrentDepartmentChargeCode.PK;
			Assert("Department should be in error", testSplitCharge.JR_GEInfo.HasError(checkErrorMessage));
		}

		public void TestValidationDoesNotRunWhenApportionedChargeIsNotUsed()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			AccChargeCode nonCurrentDepartmentChargeCode = objectCreator.CreateChargeCode("MRG100_2", "Margin100%", Constants.ChargeType.Margin, 100, objectCreator.GST1, null, "TE, OSEC");

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();

				ApportionSplitCharge testSplitCharge = cost.ApportionmentCharges[0];
				ApportionSplitCharge testSplitCharge2 = cost.ApportionmentCharges[1];

				testSplitCharge.JR_AC = nonCurrentDepartmentChargeCode.PK;
				testSplitCharge2.JR_AC = nonCurrentDepartmentChargeCode.PK;
				AssertHasError("Department should be in error", testSplitCharge.JR_GEInfo, "This department is not valid for the charge code specified on this Job.");
				AssertHasError("Department should be in error", testSplitCharge2.JR_GEInfo, "This department is not valid for the charge code specified on this Job.");

				cost.E6_OSCostAmount = 4;
				testSplitCharge.JR_OSCostAmt = 2;
				testSplitCharge2.JR_OSCostAmt = 1;
				AssertHasError("Cost should be in error", testSplitCharge.JR_OSCostAmtInfo, "The sum of the apportioned OS amounts must be equal to the consol cost OS amount.");
				AssertHasError("Cost should be in error", testSplitCharge2.JR_OSCostAmtInfo, "The sum of the apportioned OS amounts must be equal to the consol cost OS amount.");

				testSplitCharge.JR_IsUsedForApportionment = false;
				testSplitCharge.JR_AC = GlbDepartment.CurrentDepartment.PK;
				testSplitCharge.JR_AC = nonCurrentDepartmentChargeCode.PK;
				AssertNoErrors("Should be no errors when Charge is not in use", testSplitCharge.JR_GEInfo);
				AssertHasError("Department should be in error", testSplitCharge2.JR_GEInfo, "This department is not valid for the charge code specified on this Job.");
				AssertNoErrors("Should be no errors when Charge is not in use", testSplitCharge.JR_OSCostAmtInfo);
				AssertHasError("Cost should be in error", testSplitCharge2.JR_OSCostAmtInfo, "The sum of the apportioned OS amounts must be equal to the consol cost OS amount.");
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}

		public void TestCheckIsFinal_AllowPayablesInvoiceFinalFlag()
		{
			const string expectError = @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice";

			AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			var apportionSplitCharge = consolCost.ApportionmentCharges[0];
			apportionSplitCharge.JR_AC = TestObjectCreator.CC1.PK;
			apportionSplitCharge.JR_OSCostAmt = 100m;

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;
			apportionSplitCharge.IsFinal = true;
			AssertHasError("Should have error", apportionSplitCharge.IsFinalInfo, expectError);
			apportionSplitCharge.IsFinal = false;
			AssertNoErrors("Should not have error", apportionSplitCharge.IsFinalInfo);

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
			apportionSplitCharge.IsFinal = true;
			AssertNoErrors("Should not have error", apportionSplitCharge.IsFinalInfo);
			apportionSplitCharge.IsFinal = false;
			AssertNoErrors("Should not have error", apportionSplitCharge.IsFinalInfo);
		}

		public void TestDepartmentValidation_MiscellaneousDepartment()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C00001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var job = TestObjectCreator.CreateJob(shipment, false);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1);
			var apportionSplitCharge = consolCost.ApportionmentCharges[0];

			string expectedError = @"Cannot issue job charges for a miscellaneous department.";
			var miscDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, true));
			var nonMiscDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false));

			apportionSplitCharge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			apportionSplitCharge.JR_GE = miscDepartment.PK;
			apportionSplitCharge.Validation.ValidateJR_GE();
			Assert("Department should have error", apportionSplitCharge.JR_GEInfo.HasErrors());
			Assert("Department Error", apportionSplitCharge.JR_GEInfo.HasError(expectedError));

			apportionSplitCharge.JR_JH = job.PK;
			apportionSplitCharge.JR_GE = nonMiscDepartment.PK;
			apportionSplitCharge.Validation.ValidateJR_GE();
			Assert("Department should not have error", !apportionSplitCharge.JR_GEInfo.HasErrors());

			apportionSplitCharge.JR_GE = miscDepartment.PK;
			apportionSplitCharge.Validation.ValidateJR_GE();
			Assert("Department should have error", apportionSplitCharge.JR_GEInfo.HasErrors());
			Assert("Department Error", apportionSplitCharge.JR_GEInfo.HasError(expectedError));

			job.PlugInData = new JobValidationTest.MockJobInvoicingPlugIn(false);
			apportionSplitCharge.Validation.ValidateJR_GE();
			Assert("Department should not have error", !apportionSplitCharge.JR_GEInfo.HasErrors());

			job.PlugInData = new JobValidationTest.MockJobInvoicingPlugIn(true);
			apportionSplitCharge.Validation.ValidateJR_GE();
			Assert("Department should have error", apportionSplitCharge.JR_GEInfo.HasErrors());
			Assert("Department Error", apportionSplitCharge.JR_GEInfo.HasError(expectedError));
		}

		public void TestValidationHandlesDeactivateJob()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var consol = objectCreator.CreateConsol();
			var shipment1 = objectCreator.CreateShipment("S1111", consol);
			var shipment1Job = objectCreator.CreateJob(shipment1);
			var shipment2 = objectCreator.CreateShipment("S1112", consol);
			var shipment2Job = objectCreator.CreateJob(shipment2);
			var apportionmentListing = consol.GetApportionments();
			Factory.Save();

			var cost = objectCreator.CreateConsolCost(consol, objectCreator.CC1, objectCreator.Creditor1, 250M, true);
			var charge = cost.ApportionmentCharges.OfType<ApportionSplitCharge>().FirstOrDefault(c => c.JR_JH == shipment1Job.PK);

			charge.Validation.ValidateJR_JH();
			AssertNoError(charge.JR_JHInfo, "This Job # is inactive - it may not be used.");

			shipment1Job.MarkAsInactive();
			charge.Validation.ValidateJR_JH();
			AssertHasError(charge.JR_JHInfo, "This Job # is inactive - it may not be used.");
		}

		public void TestCheckJR_SellGovtChargeCode_ApportionChargePosted_RegistryEnabled()
		{
			AssertJR_SellGovtChargeCode_ApportionChargePosted(true);
		}

		public void TestCheckJR_SellGovtChargeCode_ApportionChargePosted_RegistryDisabled()
		{
			AssertJR_SellGovtChargeCode_ApportionChargePosted(false);
		}

		protected virtual void AssertJR_SellGovtChargeCode_ApportionChargePosted(bool isRegistryEnabled)
		{
			const string errorMsg = "Please enter a Sell Government Charge Code.";
			var taxRate = CreateTaxRate("GST1", AccTaxRate.Types.Rated, AccTaxRate.ExtraTypes.StateGST);
			var chargeCode = CreateChargeCode(Core.Constants.ChargeType.Margin);

			var charge = CreateJobCharge();
			charge.JR_AC = chargeCode.PK;
			charge.JR_AT_SellGSTRate = taxRate.PK;
			charge.JR_SellGovtChargeCode = string.Empty;

			CreateParentConsolCost(charge);
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isRegistryEnabled);

			charge.Validation.ValidateJR_SellGovtChargeCode();
			if (isRegistryEnabled)
			{
				AssertNoWarnings(charge.JR_SellGovtChargeCodeInfo);
				AssertHasError("Pre-condition", charge.JR_SellGovtChargeCodeInfo, errorMsg);
			}
			else
			{
				AssertNoWarnings(charge.JR_SellGovtChargeCodeInfo);
				AssertNoErrors(charge.JR_SellGovtChargeCodeInfo);
			}

			PostCostCharge(charge);

			charge.Validation.ValidateJR_SellGovtChargeCode();
			AssertNoWarnings(charge.JR_SellGovtChargeCodeInfo);
			AssertNoErrors(charge.JR_SellGovtChargeCodeInfo);
		}

		public void TestCheckGovtChargeCode_ApportionChargeNotUsed()
		{
			const string warningMsgSell = "Sell Government Charge Code is empty.";
			const string warningMsgCost = "Cost Government Charge Code is empty.";
			var taxRate = CreateTaxRate("GST1", AccTaxRate.Types.Rated, AccTaxRate.ExtraTypes.StateGST);
			var chargeCode = CreateChargeCode(Core.Constants.ChargeType.Margin);

			var charge = CreateJobCharge() as ApportionSplitCharge;
			charge.JR_AC = chargeCode.PK;
			charge.JR_AT_CostGSTRate = taxRate.PK;
			charge.JR_CostGovtChargeCode = string.Empty;
			charge.JR_AT_SellGSTRate = taxRate.PK;
			charge.JR_SellGovtChargeCode = string.Empty;
			charge.JR_IsUsedForApportionment = false;

			CreateParentConsolCost(charge);

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			charge.Validation.ValidateJR_CostGovtChargeCode();
			charge.Validation.ValidateJR_SellGovtChargeCode();

			AssertNoWarnings(charge.JR_CostGovtChargeCodeInfo);
			AssertNoWarnings(charge.JR_SellGovtChargeCodeInfo);
			AssertNoErrors(charge.JR_CostGovtChargeCodeInfo);
			AssertNoErrors(charge.JR_SellGovtChargeCodeInfo);

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			charge.Validation.ValidateJR_CostGovtChargeCode();
			charge.Validation.ValidateJR_SellGovtChargeCode();

			AssertHasWarning(charge.JR_CostGovtChargeCodeInfo, warningMsgCost);
			AssertHasWarning(charge.JR_SellGovtChargeCodeInfo, warningMsgSell);
			AssertNoErrors(charge.JR_CostGovtChargeCodeInfo);
			AssertNoErrors(charge.JR_SellGovtChargeCodeInfo);
		}

		public void TestCheckJR_JH_InternalJobValidation_HasError_ClosedJob()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var job = TestObjectCreator.CreateJobHeader();
			var apportionSplitCharge = CreateJobCharge();
			apportionSplitCharge.JR_JH = job.PK;
			apportionSplitCharge.JR_GE = TestObjectCreator.FISDepartment.PK;
			apportionSplitCharge.JR_OH_SellAccount = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			apportionSplitCharge.JR_GB_InternalBranch = apportionSplitCharge.JR_GB;
			apportionSplitCharge.JR_GE_InternalDept = ZGuid.Empty;
			apportionSplitCharge.JR_JH_InternalJob = ZGuid.Empty;

			Assert("Precondition: JR_JH_InternalJob.IsValid", !apportionSplitCharge.JR_JH_InternalJob.IsValid);
			apportionSplitCharge.Validation.ValidateJR_JH_InternalJob();
			AssertNoError(apportionSplitCharge.JR_JH_InternalJobInfo, "This job is closed. You cannot set the internal job to be a closed job.");

			apportionSplitCharge.JR_JH_InternalJob = job.PK;
			Assert("Precondition: JR_JH_InternalJob.IsValid", apportionSplitCharge.JR_JH_InternalJob.IsValid);
			Assert("Precondition: ShouldCreateJRJ", !((BaseCharge)apportionSplitCharge).ShouldCreateJRJ);
			apportionSplitCharge.Validation.ValidateJR_JH_InternalJob();
			AssertNoError(apportionSplitCharge.JR_JH_InternalJobInfo, "This job is closed. You cannot set the internal job to be a closed job.");

			apportionSplitCharge.JR_LocalSellAmt = 10M;
			apportionSplitCharge.JR_OSSellAmt = 10m;
			Assert("Precondition: ShouldCreateJRJ", ((BaseCharge)apportionSplitCharge).ShouldCreateJRJ);
			AssertNoErrors("Precondition: Internal Job Error", apportionSplitCharge.JR_JH_InternalJobInfo);
			Assert("Precondition: Internal Job status", !((BaseCharge)apportionSplitCharge).InternalInvoicingJob.IsClosed);
			apportionSplitCharge.Validation.ValidateJR_JH_InternalJob();
			AssertNoError(apportionSplitCharge.JR_JH_InternalJobInfo, "This job is closed. You cannot set the internal job to be a closed job.");

			apportionSplitCharge.InternalJob.JH_Status = JobHeaderStatus.Closed.Code;
			Assert("Precondition: Internal Job status", ((BaseCharge)apportionSplitCharge).InternalInvoicingJob.IsClosed);
			apportionSplitCharge.Validation.ValidateJR_JH_InternalJob();
			AssertHasError(apportionSplitCharge.JR_JH_InternalJobInfo, "This job is closed. You cannot set the internal job to be a closed job.");
		}

		#region Implementation

		protected override BaseCharge GetNewParentBusinessObject(BusinessObjectFactory factory)
		{
			return (BaseCharge)factory.New(GetExpectedBusinessObjectType());
		}

		protected override Type GetExpectedBusinessObjectType() => typeof(ApportionSplitCharge);

		protected override JobCharge CreateJobCharge()
		{
			var charge = base.CreateJobCharge() as ApportionSplitCharge;
			charge.JR_IsUsedForApportionment = true;
			return charge;
		}

		#endregion
	}
}
