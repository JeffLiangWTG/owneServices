using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class PostManagerValidationTest : TransactionCreatorBaseTest
	{
		#region Validations for charge which disallowed to view

		public void TestDebtorValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				Debtor.OH_IsActive = false;
				Factory.Save();

				string expectedError = "Job S00001234 has error: The Debtor is inactive - it may not be used.";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunDebtorValidation_ForTestOnly);

				validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunDebtorValidation_ForTestOnly);
			}
		}

		public void TestInvoiceTypeValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				using (charge1.GetValidationSuspender())
				{
					charge1.JR_InvoiceType = "";
				}
				newFactory.Save();

				string expectedError = "Job S00001234 has error: Invalid invoice type: Please enter an Invoice Type.";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				Globals.IsUserInteractive = false;
				AssertEquals(expectedError, validation.RunInvoiceTypeValidation_ForTestOnly());

				var newFactory1 = new BusinessObjectFactory();
				job = newFactory1.Load<Job>(job.PK);

				jobs = new[] { job };
				validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				Globals.IsUserInteractive = true;
				AssertValidationsForChargeDisallowedToViewCore(validation, expectedError, validation.RunInvoiceTypeValidation_ForTestOnly());
			}
		}

		public void TestBranchDepartmentCombinationsValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				var allowedDepartment = Factory.NewWithValidTestData<AccAllowedBranchDepartmentCombo>();
				TestObjectCreator.NonCurrentBranch.AllowedDepartments.Add(allowedDepartment);
				Factory.Save();

				string expectedError = @"This job cannot be posted. The department CEA cannot be used with the branch SYD.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunBranchDepartmentCombinationsValidation_ForTestOnly);
			}
		}

		public void TestGSTApplicabilityValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			var taxRate = TestObjectCreator.CreateTaxRate("GST", "GST", 19);
			var branch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "TS1");

			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge1.JR_AT_SellGSTRate = taxRate.PK;

			var charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			TestObjectCreator.CC2.AC_ChargeType = Constants.ChargeType.Comment;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

				string expectedError = "Job S00001234 has error: Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the debtor \"Tax is Applicable\" flag.\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunGSTApplicabilityValidation_ForTestOnly);
			}
		}

		public void TestGSTApplicabilityValidation()
		{
			SetupTest();
			SetupSecurity();

			var taxRate = TestObjectCreator.CreateTaxRate("GST", "GST", 19);
			var branch = GlbBranch.CurrentBranch;

			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_JobNum = "S00001234";
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Factory.Save();

			var taxBranchConflictErrorMessage = @"Tax branch values on unposted charges in the billing tab conflict with at least one of the following settings:
- 'Enabled Tax Branch Reporting' registry value
- Debtor / Creditor 'Tax is Applicable' flag
- Current Login Company 'VAT Registered' flag";
			var sellTaxIdAndTaxBranchErrorMessage = $"Job S00001234 has error: Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the debtor \"Tax is Applicable\" flag.\r\n\r\n{taxBranchConflictErrorMessage}\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.";
			var sellTaxIdErrorMessage = "Job S00001234 has error: Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the debtor \"Tax is Applicable\" flag.\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.";
			var sellTaxBranchErrorMessage = $"Job S00001234 has error: Tax Data Validation error.\r\n\r\n{taxBranchConflictErrorMessage}\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.";
			var costTaxIdAndTaxBranchErrorMessage = $"Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the creditor \"Tax is Applicable\" flag.\r\n\r\n{taxBranchConflictErrorMessage}\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.";
			var costTaxIdErrorMessage = "Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the creditor \"Tax is Applicable\" flag.\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.";
			var costTaxBranchMessge = $"Tax Data Validation error.\r\n\r\n{taxBranchConflictErrorMessage}\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.";

			var testCases = new List<GSTApplicabilityValidationTestCase>()
			{
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxBranchMessge },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = costTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = string.Empty },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxBranchMessge },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = costTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = string.Empty },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = true, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = true, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = true, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = true, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxBranchMessge },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = true, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = true, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = costTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = true, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = string.Empty },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = true, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = false, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = string.Empty },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = costTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = string.Empty },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = string.Empty },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = false, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxBranchMessge },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = costTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = string.Empty },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = string.Empty },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxBranchMessge },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = true, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = true, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = costTaxIdAndTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = true, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxBranchErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = true, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = ZGuid.Empty, errorMessage = costTaxBranchMessge },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = true, isAPTaxApplicable = false, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = sellTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = true, sellTaxID = ZGuid.Empty, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = costTaxIdErrorMessage },
				new GSTApplicabilityValidationTestCase { isAR = true, isGSTRegistered = true, isARTaxApplicable = true, isAPTaxApplicable = false, sellTaxID = taxRate.PK, costTaxID = ZGuid.Empty, enableTaxBranchReport = true, sellTaxBranch = branch.PK, costTaxBranch = ZGuid.Empty, errorMessage = string.Empty },
				new GSTApplicabilityValidationTestCase { isAR = false, isGSTRegistered = true, isARTaxApplicable = false, isAPTaxApplicable = true, sellTaxID = ZGuid.Empty, costTaxID = taxRate.PK, enableTaxBranchReport = true, sellTaxBranch = ZGuid.Empty, costTaxBranch = branch.PK, errorMessage = string.Empty },
			};

			var testCaseNum = 0;

			foreach (var testCase in testCases)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = testCase.isGSTRegistered;

				if (testCase.isAR)
				{
					charge1.JR_OH_CostAccount = ZGuid.Empty;
					charge1.JR_APInvoiceNum = string.Empty;
					charge1.JR_OSCostAmt = 0m;
					charge1.JR_AT_CostGSTRate = ZGuid.Empty;
					charge1.JR_GB_CostTaxBranch = ZGuid.Empty;
					charge1.JR_OH_SellAccount = Debtor.PK;
					charge1.JR_OSSellAmt = 100m;
					charge1.JR_AT_SellGSTRate = testCase.sellTaxID;
					charge1.JR_GB_SellTaxBranch = testCase.sellTaxBranch;
					SetTaxApplicable(Debtor, true, testCase.isARTaxApplicable);
				}
				else
				{
					charge1.JR_OH_SellAccount = ZGuid.Empty;
					charge1.JR_OSSellAmt = 0m;
					charge1.JR_AT_SellGSTRate = ZGuid.Empty;
					charge1.JR_GB_SellTaxBranch = ZGuid.Empty;
					charge1.JR_OH_CostAccount = Creditor.PK;
					charge1.JR_APInvoiceNum = "TestAP1";
					charge1.JR_OSCostAmt = 100m;
					charge1.JR_AT_CostGSTRate = testCase.costTaxID;
					charge1.JR_GB_CostTaxBranch = testCase.costTaxBranch;
					SetTaxApplicable(Creditor, false, testCase.isAPTaxApplicable);
				}

				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testCase.enableTaxBranchReport))
				{
					var newFactory = new BusinessObjectFactory();
					job = newFactory.Load<Job>(job.PK);
					var jobs = new[] { job };
					var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
					var result = validation.RunGSTApplicabilityValidation_ForTestOnly();

					AssertEquals($"Scenario {testCaseNum} : isAR = {testCase.isAR}, isGSTRegistered = {testCase.isGSTRegistered}, isARTaxApplicable = {testCase.isARTaxApplicable}, isAPTaxApplicable = {testCase.isAPTaxApplicable}, costTaxIDIsEmpty = {testCase.costTaxID.IsEmpty}, sellTaxIDIsEmpty = {testCase.sellTaxID.IsEmpty}, enableTaxBranchReport = {testCase.enableTaxBranchReport}, costTaxBranchIsEmpty = {testCase.costTaxBranch.IsEmpty}, sellTaxBranchIsEmpty = {testCase.sellTaxBranch.IsEmpty}", testCase.errorMessage, result);
				}
				testCaseNum++;
			}
		}

		void SetTaxApplicable(OrgHeader orgHeader, bool isAR, bool isTaxApplicable)
		{
			var vatConfig = isTaxApplicable ? AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code : AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			if (isAR)
			{
				orgHeader.CompanyData.OB_ARVATConfig = vatConfig;
			}
			else
			{
				orgHeader.CompanyData.OB_APVATConfig = vatConfig;
			}
		}

		class GSTApplicabilityValidationTestCase
		{
			public bool isAR { get; set; }
			public bool isGSTRegistered { get; set; }
			public bool isARTaxApplicable { get; set; }
			public bool isAPTaxApplicable { get; set; }
			public ZGuid costTaxID { get; set; }
			public ZGuid sellTaxID { get; set; }
			public bool enableTaxBranchReport { get; set; }
			public ZGuid costTaxBranch { get; set; }
			public ZGuid sellTaxBranch { get; set; }
			public string errorMessage { get; set; }
		}

		public void TestChargesHaveCFXAccountValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(job, USD, .7M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			job.Parent = shipment;
			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", USD, 100M, Creditor1, USD, 150M, LocalClient);
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Charge charge2 = CreateCharge(job, CC1, "Charge Code 2", USD, 100M, Creditor1, USD, 150M, Debtor1);

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.CFXAccount.DataType.SuspendValidation())
			using (AccountingConfigurationRegistry.Instance.CFXAccount.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Guid.Empty))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);
				var department = " - " + "CEA" + "\n";
				string expectedError = $"Posting cannot occur on job.  There are charge(s) with department(s) that do not have a CFX Account set in the registry.\r\n\r\nThe following departments do not have a CFX Account set:\r\n{department}\r\nEither set the system or department level CFX Account in the registry under Accounting -> General Ledger Defaults -> Link Account -> CFX Account.";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunChargesHaveCFXAccountValidation_ForTestOnly);
			}
		}

		public void TestStampDutyValidationForChargesDisallowedToView()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.SetCountry(Constants.CountryCodes.Italy);

			SetupTest();
			SetupSecurity();

			var taxRate = AccTaxRate.FindExistingTaxRate(Factory, "ART7", AccTaxRate.Types.Rated, Constants.CountryCodes.Italy);

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 2m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge1.JR_AT_SellGSTRate = taxRate.PK;
			charge1.JR_LocalSellAmt = 2m;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty))
			using (AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxRate.PK.ToString()))
			using (AccountingConfigurationRegistry.Instance.StampDutyThreshold.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1m))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				Debtor.UNLOCO.RL_RN_NKCountryCode = Constants.CountryCodes.Italy;
				Factory.Save();

				string expectedError = "The invoice being posted attracts stamp duty, but there is no 'Stamp Duty Charge Code' defined in the registry. Please define an appropriate charge code in the registry under Accounting > Receivable Defaults > Default Settings > Stamp Duty Charge Code.";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunStampDutyValidation_ForTestOnly);
			}
		}

		[TestDate(2021, 04, 15)]
		public void TestRevenueRecognitionDateValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			shipment.JS_E_DEP = new ZDateTime(2020, 01, 01);
			shipment.JS_E_ARV = new ZDateTime(2020, 01, 01);

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			AccChargeRevRecOverride chargeRevRecOverride = TestObjectCreator.CC1.RevenueRecOverrides.AddNew();
			chargeRevRecOverride.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			chargeRevRecOverride.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate;
			chargeRevRecOverride.Offset = 0;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				string expectedError = "Posting is prevented. The following information has not been recorded for this job. It is required for revenue recognition purposes and must be recorded before posting can occur: 'Estimated Arrival Date'";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunRevenueRecognitionDateValidation_ForTestOnly);
			}
		}

		public void TestChargesCanBePostedValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			AssertEquals("Precondition", JobInvoicingConsumerTypes.Shipment, ((IJobInvoicingPlugIn)shipment).InvoicingSupporter.ConsumerType);

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.DoNotPost;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				job.Parent = shipment;
				charge1 = newFactory.Load<Charge>(charge1.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);

				string expectedError = "You cannot currently post charges on this job due to the following reason(s):" + System.Environment.NewLine + (char)8226 + "The Invoice Type for charges is set to 'Do Not Post'.";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunChargesCanBePostedValidation_ForTestOnly);
			}
		}

		public void TestChargeCodeValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = chargeCode.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC2.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			Factory.Save();

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			var invalidChargeCode = Factory.Load<AccChargeCode>(chargeCode.PK);
			invalidChargeCode.AC_IsActive = false;
			Factory.Save();

			AssertNoErrors(job);

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				string expectedError = "Invalid Charge Code(s): ZZCC1.";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunChargeCodeValidation_ForTestOnly);
			}
		}

		[TestDate(2021, 04, 15)]
		public void TestExchangeRateValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge1.JR_RX_NKSellCurrency = Constants.CurrencyCodes.China;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				string expectedError = @"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the CNY exchange rate is not set for the date 15-Apr-21. Please check your data and try again.";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunExchangeRateValidation_ForTestOnly);
			}
		}

		[TestDate(2021, 04, 15)]
		public void TestOperationalTaxDateErrorValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption1 = collection.AddNew();
			taxDateOption1.JobType = "SHP";
			taxDateOption1.DirectionCode = "ALL";
			taxDateOption1.Mode = "ALL";
			taxDateOption1.Ledger = "AR";
			taxDateOption1.TaxDateOption = TaxDateDefaultingOption.Code.EstimatedArrivalDate;

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge1.JR_RX_NKSellCurrency = Constants.CurrencyCodes.China;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				job.PlugInData = shipment;
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				string expectedError = "Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Estimated Arrival Date'.";

				charge2.JR_SellTaxDate = ZDate.Today;

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedError, validation.RunOperationalTaxDateErrorValidation_ForTestOnly);
			}
		}

		public void TestCreditLimitValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			Debtor.CompanyData.OB_AROnCreditHold = true;
			Debtor.CompanyData.OB_ARCreditLimit = 200m;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				var allowedDepartment = Factory.NewWithValidTestData<AccAllowedBranchDepartmentCombo>();
				TestObjectCreator.NonCurrentBranch.AllowedDepartments.Add(allowedDepartment);
				Factory.Save();

				string expectedWarning = @"TESDEBBNE is on Credit Hold.

The Credit Limit for TESDEBBNE is set to 200.00 AUD. Credit approved.
The Total Outstanding Balance is 110.00 AUD, which has not exceeded the credit limit threshold.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 0.00 AUD
  *  the Current Transaction Amount, 110.00 AUD";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedWarning, validation.RunCreditLimitValidation_ForTestOnly);
			}
		}

		public void TestCreditLimitValidation_GlobalCreditLimitIsEmpty()
		{
			SetupTest();
			SetupSecurity();

			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;

			var charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			Debtor.CompanyData.OB_ARCreditLimit = 200m;
			Debtor.MiscServ.OM_RX_NKARGlobalCreditCurrency = Env.CurrentCompany.LocalCurrency.Code;
			var companyPKs = Debtor.CompanyDataCollection.Select(x => x.OB_GC);

			foreach (var pk in companyPKs)
			{
				var globalExRate = Factory.NewWithValidTestData<RefExchangeRate>();
				globalExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.GlobalCreditControl;    // Rate type is used for Global Credit
				globalExRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
				globalExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
				globalExRate.RE_RX_NKExCurrency = Env.CurrentCompany.LocalCurrency.Code;
				globalExRate.RE_SellRate = 1m;
				globalExRate.RE_GC = pk;
			}

			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized))
			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInGlobalCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				Assert("Global Credit Limit is empty.", Debtor.MiscServ.ARGlobalCreditLimit.IsEmpty);
				AssertValidationsForChargeDisallowedToView(validation, ZString.Empty, validation.RunCreditLimitValidation_ForTestOnly);
			}
		}

		public void TestCompanyAndOrgsRegistrationNumbersValidationForChargesDisallowedToView()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.SetCountry(Constants.CountryCodes.Portugal);

			SetupTest();
			SetupSecurity();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 2m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge1.JR_LocalSellAmt = 2m;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = TestObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = Debtor1.PK;
			charge2.JR_OSSellAmt = 100m;

			Debtor.PrimaryRegistrationNumber.Number = string.Empty;
			Debtor1.PrimaryRegistrationNumber.Number = "123";

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				charge1 = newFactory.Load<Charge>(charge1.PK);
				charge2 = newFactory.Load<Charge>(charge2.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);
				AssertEquals("User is allowed to view charge 2", true, charge2.IsAllowedToViewThisCharge);

				string expectedWarning = @"The transaction is missing information that is mandatory for your country/region reporting:
Tax Registration Number of the Debtor [TESDEBBNE]";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedWarning, validation.RunCompanyAndOrgsRegistrationNumbersValidation_ForTestOnly);
			}
		}

		[TestDate(2021, 04, 15)]
		public void TestOperationalTaxDateWarningValidationForChargesDisallowedToView()
		{
			SetupTest();
			SetupSecurity();

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption1 = collection.AddNew();
			taxDateOption1.JobType = "SHP";
			taxDateOption1.DirectionCode = "ALL";
			taxDateOption1.Mode = "ALL";
			taxDateOption1.Ledger = "AR";
			taxDateOption1.TaxDateOption = TaxDateDefaultingOption.Code.EstimatedArrivalDate;

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			shipment.JS_E_ARV = ZDateTime.Today.AddYears(2);
			shipment.JS_E_DEP = ZDateTime.Today;

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = shipment;
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge1.JR_RX_NKSellCurrency = Constants.CurrencyCodes.China;

			Factory.Save();

			using (Env.SetTemporaryUserContext(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			using (AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				job = newFactory.Load<Job>(job.PK);
				job.PlugInData = shipment;
				charge1 = newFactory.Load<Charge>(charge1.PK);

				AssertEquals("User is disallowed to view charge 1", false, charge1.IsAllowedToViewThisCharge);

				string expectedWarning = "Tax Date is more than 1 year from now, based on 'Estimated Arrival Date'. Do you wish to continue?";

				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				AssertValidationsForChargeDisallowedToView(validation, expectedWarning, (x) => validation.RunOperationalTaxDateErrorValidation_ForTestOnly(x), validation.RunOperationalTaxDateWarningValidation_ForTestOnly);
			}
		}

		#region Assert

		void AssertValidationsForChargeDisallowedToView(PostManagerValidation validation, string expectedMessage, Func<string> validate)
		{
			Globals.IsUserInteractive = false;
			var result = validate();
			AssertEquals(expectedMessage, result);

			Globals.IsUserInteractive = true;
			result = validate();
			AssertValidationsForChargeDisallowedToViewCore(validation, expectedMessage, result);
		}

		void AssertValidationsForChargeDisallowedToView(PostManagerValidation validation, string expectedMessage, Func<bool, string> validate)
		{
			AssertValidationsForChargeDisallowedToView(validation, expectedMessage, null, validate);
		}

		void AssertValidationsForChargeDisallowedToView(PostManagerValidation validation, string expectedMessage, Action<bool> preValidate, Func<bool, string> validate)
		{
			Globals.IsUserInteractive = false;
			preValidate?.Invoke(true);
			var result = validate(true);
			AssertEquals(expectedMessage, result);

			Globals.IsUserInteractive = true;
			preValidate?.Invoke(true);
			result = validate(true);
			AssertValidationsForChargeDisallowedToViewCore(validation, expectedMessage, result);
		}

		void AssertValidationsForChargeDisallowedToViewCore(PostManagerValidation validation, string expectedMessage, string actualMessage)
		{
			if (validation is PeriodicInvoicePostManagerValidation || validation is JobChargeQueuePostManagerValidation)
			{
				AssertEquals(expectedMessage, actualMessage);
			}
			else
			{
				AssertEquals(string.Empty, actualMessage);
			}
		}

		#endregion

		#endregion

		#region RunChargesCanBePostedValidation

		public void TestNoChargesCanBePosted()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var oneOffSpotQuote = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);

			AssertEquals("Precondition", JobInvoicingConsumerTypes.OneOffQuotation, ((IJobInvoicingPlugIn)oneOffSpotQuote).InvoicingSupporter.ConsumerType);

			using (var job = Job.CreateWithMutex(Factory, oneOffSpotQuote))
			{
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				var charge = job.Charges.AddNew();
				charge.JR_OSSellAmt = 100m;

				var jobs = new[] { job };

				var validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
				var expectedError = "You cannot currently post charges on this job due to the following reason(s):" + System.Environment.NewLine + (char)8226 + "Spot Quotes/Quoted Bookings do not allow charges to be posted. Charges should be posted on the Shipment or standalone Booking only.";
				AssertEquals(expectedError, validation.RunChargesCanBePostedValidation_ForTestOnly());
			}
		}

		public void TestNoChargesCanBePosted_AgencyConsumerType()
		{
			var billOfLading = Factory.New<BillOfLading>();

			using (Job job = Job.CreateWithMutex(Factory, billOfLading))
			{
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = TestObjectCreator.NonCurrentDepartment.PK;
				var charge1 = job.Charges.AddNew();
				charge1.JR_AC = TestObjectCreator.CC1.PK;
				charge1.JR_OSSellAmt = 100m;
				charge1.JR_OSCostAmt = 100m;
				charge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
				charge1.JR_APInvoiceNum = "1";
				charge1.JR_InvoiceType = AgencyInvoiceTypesList.Codes.LocalPrePaid;
				charge1.JR_APInvoiceDate = ZDateTime.Now;
				var charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_OSSellAmt = 100m;
				charge2.JR_OSCostAmt = 100m;
				charge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
				charge1.JR_APInvoiceNum = "2";
				charge2.JR_InvoiceType = AgencyInvoiceTypesList.Codes.LocalCollect;
				charge1.JR_APInvoiceDate = ZDateTime.Now;

				var jobs = new[] { job };

				var validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
				var expectedError = "You cannot currently post charges on this job due to the following reason(s):";
				expectedError += System.Environment.NewLine + (char)8226 + "The Invoice Type is Pre-Paid however the origin on this job is not in the same country/region as the current company.";
				expectedError += System.Environment.NewLine + (char)8226 + "The Invoice Type is Collect however the destination on this job is not in the same country/region as the current company.";

				var registry = new Mock<IAgencyRegistry>(MockBehavior.Strict);

				using (ObjectFactory.Substitute(registry.Object))
				{
					registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(true);
					registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(false);
					AssertEquals(string.Empty, validation.RunChargesCanBePostedValidation_ForTestOnly());

					registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(false);
					registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(true);
					AssertEquals(string.Empty, validation.RunChargesCanBePostedValidation_ForTestOnly());

					registry.Setup(m => m.PostBothPrepaidAndCollectShipmentRevenueCharges).Returns(false);
					registry.Setup(m => m.PostBothPrepaidAndCollectShipmentCostCharges).Returns(false);
					AssertEquals(expectedError, validation.RunChargesCanBePostedValidation_ForTestOnly());
				}
			}
		}

		class PostManagerValidationDummy : PostManagerValidation
		{
			public PostManagerValidationDummy(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
				: base(jobs, postingOption, originalJobs)
			{
			}

			public bool IsCostEligible { get; set; }
			protected override bool IsCostEligibleToPost(Charge charge)
			{
				return IsCostEligible;
			}

			public bool IsSellEligible { get; set; }
			protected override bool IsSellEligibleToPost(Charge charge)
			{
				return IsSellEligible;
			}
		}

		public void TestRunChargesCanBePostedValidation()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			var taseCases = new (Action<Job> PrepareInValidCharge, string Error)[]
			{
				((job) => TestCaseForCommonProperty_JR_GBInfo(job), "Please enter a Branch."),
				((job) => TestCaseForCommonProperty_JR_GEInfo(job), "Please enter a Department."),
				((job) => TestCaseForCommonProperty_JR_ACInfo(job), "Please enter a Charge Code."),
				((job) => TestCaseForCommonProperty_JR_DescInfo(job), "Description cannot be empty."),
				((job) => TestCaseForCostRelatedProperty_JR_LocalCostAmtInfo(job), "Local amount cannot be zero when Overseas Cost Amount is non zero."),
				((job) => TestCaseForCostRelatedProperty_JR_OSCostAmtInfo(job), "OS Cost Amount and Local Cost Amount should have the same sign."),
			};

			CombineAssertions(() =>
			{
				for (int i = 0; i < taseCases.Length; i++)
				{
					var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S000000" + i), false);
					var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge", TestObjectCreator.USD, 100M, TestObjectCreator.AALSHI, "INV00" + i, TestObjectCreator.USD, 100M);
					taseCases[i].PrepareInValidCharge(job);
					Assert(job.Charges.Any());

					foreach (JobInvoicingPostingOption postingOption in Enum.GetValues(typeof(JobInvoicingPostingOption)))
					{
						var validation = new PostManagerValidationDummy(new Job[] { job }, postingOption, new Job[] { job });
						validation.IsCostEligible = true;
						validation.IsSellEligible = true;
						RunChargesCanBePostedValidationAndCheckResult(validation, true, taseCases[i].Error);

						validation = new PostManagerValidationDummy(new Job[] { job }, postingOption, new Job[] { job });
						validation.IsCostEligible = false;
						validation.IsSellEligible = true;
						RunChargesCanBePostedValidationAndCheckResult(validation, false, taseCases[i].Error);

						validation = new PostManagerValidationDummy(new Job[] { job }, postingOption, new Job[] { job });
						validation.IsCostEligible = true;
						validation.IsSellEligible = false;
						RunChargesCanBePostedValidationAndCheckResult(validation, true, taseCases[i].Error);

						validation = new PostManagerValidationDummy(new Job[] { job }, postingOption, new Job[] { job });
						validation.IsCostEligible = false;
						validation.IsSellEligible = false;
						RunChargesCanBePostedValidationAndCheckResult(validation, false, taseCases[i].Error);
					}
				}
			});

			void RunChargesCanBePostedValidationAndCheckResult(PostManagerValidation validation, bool shouldShowError, string error)
			{
				var validationResult = validation.RunChargesCanBePostedValidation_ForTestOnly();

				if (shouldShowError)
				{
					AssertContains("You cannot currently post charges on this job due to the following reason(s):", validationResult);
					AssertContains(error, validationResult);
				}
				else
				{
					AssertNotContains("You cannot currently post charges on this job due to the following reason(s):", validationResult);
				}
			}

			Job TestCaseForCostRelatedProperty_JR_OSCostAmtInfo(Job job)
			{
				var charge = job.Charges[0];
				using (charge.Calculations.SuspendCalculations())
				{
					job.ExchangeRates.RemoveAndDeleteAll();
					charge.JR_LocalCostAmt = 1m;
					charge.JR_OSCostAmt = -1m;
				}
				return job;
			}

			Job TestCaseForCommonProperty_JR_ACInfo(Job job)
			{
				job.Charges[0].JR_AC = ZGuid.Empty;
				return job;
			}

			Job TestCaseForCommonProperty_JR_DescInfo(Job job)
			{
				job.Charges[0].JR_Desc = string.Empty;
				return job;
			}

			Job TestCaseForCommonProperty_JR_GBInfo(Job job)
			{
				job.Charges[0].JR_GB = ZGuid.Empty;
				return job;
			}

			Job TestCaseForCommonProperty_JR_GEInfo(Job job)
			{
				job.Charges[0].JR_GE = ZGuid.Empty;
				return job;
			}

			Job TestCaseForCostRelatedProperty_JR_LocalCostAmtInfo(Job job)
			{
				var charge = job.Charges[0];
				using (charge.Calculations.SuspendCalculations())
				{
					job.ExchangeRates.RemoveAndDeleteAll();
					charge.JR_LocalCostAmt = 0m;
				}
				AssertEquals(0m, charge.JR_LocalCostAmt);
				AssertNotEquals(0m, charge.JR_LocalSellAmt);
				AssertNotEquals(0m, charge.JR_OSCostAmt);
				AssertNotEquals(0m, charge.JR_OSSellAmt);

				return job;
			}
		}

		#endregion

		public void TestRunJobChargeSupplyTypeValidation_ForApportionmentCharges()
		{
			var consol = TestObjectCreator.CreateConsol();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			using (var job1 = TestObjectCreator.CreateJob(consol.Shipments[0]))
			using (var job2 = TestObjectCreator.CreateJob(consol.Shipments[1]))
			{
				job1.JH_GE = TestObjectCreator.FISDepartment.PK;
				job2.JH_GE = TestObjectCreator.FISDepartment.PK;
				var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, 500M);
				Factory.Save();

				var jobs = new[] { job1, job2 };
				var validator = NewPostManagerValidation(jobs, consol, JobInvoicingPostingOption.All);

				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Precondition", false, cost.IsPosted);
					AssertNullOrEmpty(validator.RunJobChargeSupplyTypeValidation_ForTestOnly());

					cost.ApportionmentCharges[0].JR_CostSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOA;
					AssertEquals(expectedErrorWhenRunJobChargeSupplyTypeValidation_ForApportionmentCharges, validator.RunJobChargeSupplyTypeValidation_ForTestOnly());

					var invoice = Factory.NewWithValidTestData<APInvoice>();
					cost.E6_AH_APInvoice = invoice.PK;
					AssertEquals("Precondition", true, cost.IsPosted);
					AssertNullOrEmpty(validator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				}

				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					cost.E6_AH_APInvoice = Guid.Empty;
					AssertEquals("Precondition", false, cost.IsPosted);
					AssertNullOrEmpty(validator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				}
			}
		}

		public virtual void TestRunJobChargeSupplyTypeValidation()
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();

			var plugIn = TestObjectCreator.GetTestShipmentPlugIn();
			var job = new Job.Loader(plugIn).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Debtor1);
			charge.JR_APInvoiceNum = "1234567";
			charge.JR_APInvoiceDate = ZDateTime.Now;
			job.RunPreSaveValidation();
			AssertEquals("Precondition", false, job.HasErrors);
			Factory.Save();

			var jobs = new[] { job };
			var revenueValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
			var costValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			var allValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

			AssertEquals("Precondition", true, allValidator.ShouldMandatorySellSupplyType_ForTestOnly(charge.JR_InvoiceType));
			AssertEquals("Precondition", true, allValidator.IsCostEligibleToPost_ForTestOnly(jobs[0].Charges[0]));
			AssertEquals("Precondition", true, allValidator.IsSellEligibleToPost_ForTestOnly(jobs[0].Charges[0]));
			AssertNullOrEmpty("Precondition", charge.JR_CostSupplyType);
			AssertNullOrEmpty("Precondition", charge.JR_SellSupplyType);

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Sell Supply Type must be entered.", revenueValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertEquals("Cost Supply Type must be entered.", costValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertEquals("Sell Supply Type must be entered.", allValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());

				jobs[0].Charges[0].JR_SellSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				Factory.Save();
				AssertNullOrEmpty(revenueValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertEquals("Cost Supply Type must be entered.", costValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertEquals("Cost Supply Type must be entered.", allValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());

				jobs[0].Charges[0].JR_CostSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
				Factory.Save();
				AssertNullOrEmpty(revenueValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertNullOrEmpty(costValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertNullOrEmpty(allValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertNullOrEmpty(revenueValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertNullOrEmpty(costValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertNullOrEmpty(allValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertNullOrEmpty(revenueValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertNullOrEmpty(costValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertNullOrEmpty(allValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertNullOrEmpty(revenueValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertNullOrEmpty(costValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
				AssertNullOrEmpty(allValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
			}
		}

		public void TestRunJobChargeSupplyTypeValidation_CostChargeInvoiceTypeNON()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();

			var plugIn = TestObjectCreator.GetTestShipmentPlugIn();
			var job = new Job.Loader(plugIn).TryCreateWithoutMutexForTestOnly();
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Debtor1);
			charge.JR_APInvoiceNum = "1234567";
			charge.JR_APInvoiceDate = ZDateTime.Now;
			job.RunPreSaveValidation();
			AssertEquals("Precondition", false, job.HasErrors);
			Factory.Save();

			var jobs = new[] { job };
			var revenueValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);

			AssertNullOrEmpty("Precondition", charge.JR_SellSupplyType);
			if (!(revenueValidator is PeriodicInvoicePostManagerValidation))
			{
				AssertEquals("Sell Supply Type must be entered.", revenueValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
			}

			jobs[0].Charges[0].JR_InvoiceType = "NON";
			Factory.Save();
			AssertNullOrEmpty(revenueValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
		}

		public void TestRunJobChargeSupplyTypeValidationWithDeferredInvoiceTypes()
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();

			TestObjectCreator.Debtor1.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.Debtor1.CompanyData.InvoiceTypes[0].PI_Module = JobInvoicingConsumerTypes.Shipment.Code;

			Factory.Save();

			var plugIn = TestObjectCreator.GetTestShipmentPlugIn();
			var job = new Job.Loader(plugIn).TryCreateWithoutMutexForTestOnly();
			var jobs = new[] { job };
			var allValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Charge With Deferred Invoice Type", TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 100m, TestObjectCreator.Debtor1);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			job.RunPreSaveValidation();
			AssertEquals("Precondition", false, allValidator.ShouldMandatorySellSupplyType_ForTestOnly(charge.JR_InvoiceType));
			AssertEquals("Precondition", false, job.HasErrors);
			Factory.Save();

			AssertEquals("Precondition", true, allValidator.IsSellEligibleToPost_ForTestOnly(jobs[0].Charges[0]));
			AssertNullOrEmpty("Precondition", charge.JR_SellSupplyType);

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertNullOrEmpty("No error added since it has a deferred invoice type.", allValidator.RunJobChargeSupplyTypeValidation_ForTestOnly());
			}
		}

		[TestDate(2020, 10, 12)]
		public virtual void TestRunOperationalTaxDateValidation()
		{
			var testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption1 = collection.AddNew();
			taxDateOption1.JobType = "SHP";
			taxDateOption1.DirectionCode = "ALL";
			taxDateOption1.Mode = "ALL";
			taxDateOption1.Ledger = "AR";
			taxDateOption1.TaxDateOption = TaxDateDefaultingOption.Code.EstimatedArrivalDate;

			var taxDateOption2 = collection.AddNew();
			taxDateOption2.JobType = "SHP";
			taxDateOption2.DirectionCode = "ALL";
			taxDateOption2.Mode = "ALL";
			taxDateOption2.Ledger = "AP";
			taxDateOption2.TaxDateOption = TaxDateDefaultingOption.Code.EstimatedDepartureDate;

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				ForwardingShipment shipment1;
				Job job;
				Charge charge1;
				var creator = new TestObjectCreator(Factory);
				shipment1 = creator.CreateShipment("S1");
				job = creator.CreateJob(shipment1, false);
				job.AgentCollectPK = creator.ABIGAS.PK;
				creator.ABIGAS.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				charge1 = creator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
				using (charge1.GetValidationSuspender())
				{
					if (this is ARAP.Invoicing.Testing.PeriodicInvoicePostManagerValidationTest)
					{
						charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
					}
					else
					{
						charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
					}
					charge1.JR_AT_SellGSTRate = GST1.PK;
					charge1.JR_AT_CostGSTRate = GST1.PK;
					charge1.JR_OSCostAmt = 100m;
					charge1.JR_APInvoiceNum = "1234567";
					charge1.JR_APInvoiceDate = ZDateTime.Now;
					Factory.Save();

					var jobs = new[] { job };
					var revenueValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
					var costValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
					var agentValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Agent);
					var allValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

					AssertHasValidationError(@"Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Estimated Arrival Date'.", revenueValidator);
					AssertHasValidationError(@"Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Estimated Departure Date'.", costValidator);
					AssertHasValidationError(@"Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Estimated Arrival Date'.", agentValidator);
					AssertHasValidationError(@"Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Estimated Arrival Date'.", allValidator);

					shipment1.JS_E_ARV = new ZDateTime(2020, 10, 13);
					shipment1.JS_E_DEP = new ZDateTime(2020, 10, 15);
					Factory.Save();
					AssertHasNoValidationErrors(revenueValidator);
					AssertHasNoValidationErrors(costValidator);
					AssertHasNoValidationErrors(agentValidator);
					AssertHasNoValidationErrors(allValidator);

					shipment1.JS_E_ARV = new ZDateTime(2027, 10, 13);
					shipment1.JS_E_DEP = new ZDateTime(2027, 10, 15);
					Factory.Save();
					AssertHasValidationError(@"Tax Date is not valid. Tax Date is more than 5 years from now, based on 'Estimated Arrival Date'.", revenueValidator);
					AssertHasValidationError(@"Tax Date is not valid. Tax Date is more than 5 years from now, based on 'Estimated Departure Date'.", costValidator);
					AssertHasValidationError(@"Tax Date is not valid. Tax Date is more than 5 years from now, based on 'Estimated Arrival Date'.", agentValidator);
					AssertHasValidationError(@"Tax Date is not valid. Tax Date is more than 5 years from now, based on 'Estimated Arrival Date'.", allValidator);

					shipment1.JS_E_ARV = new ZDateTime(2009, 10, 13);
					shipment1.JS_E_DEP = new ZDateTime(2009, 10, 15);
					Factory.Save();
					AssertHasValidationError(@"Tax Date is not valid. Tax Date is more than 10 years old, based on 'Estimated Arrival Date'.", revenueValidator);
					AssertHasValidationError(@"Tax Date is not valid. Tax Date is more than 10 years old, based on 'Estimated Departure Date'.", costValidator);
					AssertHasValidationError(@"Tax Date is not valid. Tax Date is more than 10 years old, based on 'Estimated Arrival Date'.", agentValidator);
					AssertHasValidationError(@"Tax Date is not valid. Tax Date is more than 10 years old, based on 'Estimated Arrival Date'.", allValidator);

					shipment1.JS_E_ARV = new ZDateTime(2022, 10, 13);
					shipment1.JS_E_DEP = new ZDateTime(2022, 10, 15);
					Factory.Save();
					AssertHasNoValidationErrors(revenueValidator);
					AssertHasNoValidationErrors(costValidator);
					AssertHasNoValidationErrors(agentValidator);
					AssertHasNoValidationErrors(allValidator);

					AssertHasValidationWarning(@"Tax Date is more than 1 year from now, based on 'Estimated Arrival Date'. Do you wish to continue?", revenueValidator, PostManagerValidationType.TaxDateValidationForAR);
					AssertHasValidationWarning(@"Tax Date is more than 1 year from now, based on 'Estimated Departure Date'. Do you wish to continue?", costValidator, PostManagerValidationType.TaxDateValidationForAP);
					AssertHasValidationWarning(@"Tax Date is more than 1 year from now, based on 'Estimated Arrival Date'. Do you wish to continue?", agentValidator, PostManagerValidationType.TaxDateValidationForAR);
					AssertHasValidationWarning(@"Tax Date is more than 1 year from now, based on 'Estimated Arrival Date'. Do you wish to continue?", allValidator, PostManagerValidationType.TaxDateValidationForAR);

					shipment1.JS_E_ARV = new ZDateTime(2018, 10, 13);
					shipment1.JS_E_DEP = new ZDateTime(2018, 10, 15);
					Factory.Save();
					AssertHasNoValidationErrors(revenueValidator);
					AssertHasNoValidationErrors(costValidator);
					AssertHasNoValidationErrors(agentValidator);
					AssertHasNoValidationErrors(allValidator);

					AssertHasValidationWarning(@"Tax Date is more than 1 year old, based on 'Estimated Arrival Date'. Do you wish to continue?", revenueValidator, PostManagerValidationType.TaxDateValidationForAR);
					AssertHasValidationWarning(@"Tax Date is more than 1 year old, based on 'Estimated Departure Date'. Do you wish to continue?", costValidator, PostManagerValidationType.TaxDateValidationForAP);
					AssertHasValidationWarning(@"Tax Date is more than 1 year old, based on 'Estimated Arrival Date'. Do you wish to continue?", agentValidator, PostManagerValidationType.TaxDateValidationForAR);
					AssertHasValidationWarning(@"Tax Date is more than 1 year old, based on 'Estimated Arrival Date'. Do you wish to continue?", allValidator, PostManagerValidationType.TaxDateValidationForAR);

					var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
					taxRate.SetRate_ForTestOnly(1, 10, ZDate.Today.AddDays(-20), ZDate.Today.AddDays(-10));
					using (charge1.GetValidationSuspender())
					{
						charge1.JR_AT_SellGSTRate = taxRate.PK;
						shipment1.JS_E_ARV = ZDate.Today;
						charge1.JR_AT_CostGSTRate = taxRate.PK;
						shipment1.JS_E_DEP = ZDate.Today;
						Factory.Save();
						AssertHasValidationError(@"Tax Date is not valid. No rate is found for the selected date, based on 'Estimated Arrival Date'.", revenueValidator);
						AssertHasValidationError(@"Tax Date is not valid. No rate is found for the selected date, based on 'Estimated Departure Date'.", costValidator);
						AssertHasValidationError(@"Tax Date is not valid. No rate is found for the selected date, based on 'Estimated Arrival Date'.", agentValidator);
						AssertHasValidationError(@"Tax Date is not valid. No rate is found for the selected date, based on 'Estimated Arrival Date'.", allValidator);
					}
				}
			}
		}

		public void TestTaxDateValidation_PickupDateDeliveryDate()
		{
			var creator = new TestObjectCreator(Factory);
			var testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var collection = new TaxDateDefaultingOptionCollection();
			var taxDateOption1 = collection.AddNew();
			taxDateOption1.JobType = "SHP";
			taxDateOption1.DirectionCode = "ALL";
			taxDateOption1.Mode = "ALL";
			taxDateOption1.Ledger = "AR";
			taxDateOption1.TaxDateOption = TaxDateDefaultingOption.Code.PickupDate;

			var taxDateOption2 = collection.AddNew();
			taxDateOption2.JobType = "SHP";
			taxDateOption2.DirectionCode = "ALL";
			taxDateOption2.Mode = "ALL";
			taxDateOption2.Ledger = "AP";
			taxDateOption2.TaxDateOption = TaxDateDefaultingOption.Code.DeliveryDate;

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var shipment = creator.CreateShipment("S1");
				var job = CreateJob(shipment);

				var jobs = new[] { job };
				var revenueValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
				var costValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);

				AssertHasValidationError(@"Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Actual/Estimated Pickup Date'.", revenueValidator);
				AssertHasValidationError(@"Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Actual/Estimated Delivery Date'.", costValidator);
			}

			taxDateOption1.JobType = "TRN";
			taxDateOption2.JobType = "TRN";

			using (AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection))
			{
				var cartage = creator.CreateCartage();
				var job = CreateJob(cartage);
				var jobs = new[] { job };
				var revenueValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
				var costValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);

				AssertHasValidationError(@"Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Estimated Pickup Date'.", revenueValidator);
				AssertHasValidationError(@"Posting is prevented. The following information has not been recorded for this job. It is required in order to set the Tax Date and must be recorded before posting can occur: 'Estimated Delivery Date'.", costValidator);
			}

			Job CreateJob(IJobInvoicingPlugIn jobParent)
			{
				var job = creator.CreateJob(jobParent, false);
				job.AgentCollectPK = creator.ABIGAS.PK;
				creator.ABIGAS.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				var charge1 = creator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
				charge1.SuspendValidation();
				if (this is ARAP.Invoicing.Testing.PeriodicInvoicePostManagerValidationTest)
				{
					charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				}
				else
				{
					charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				}
				charge1.JR_AT_SellGSTRate = GST1.PK;
				charge1.JR_AT_CostGSTRate = GST1.PK;
				charge1.JR_OSCostAmt = 100m;
				charge1.JR_APInvoiceNum = "1234567";
				charge1.JR_APInvoiceDate = ZDateTime.Now;
				Factory.Save();

				return job;
			}
		}

		public void TestRunRoundingRegistryItemValidation()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Japan);
			AccountingConfigurationRegistry.Instance.JapanIATAImportAirLocalClientFRTChargeGroupRounding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Constants.RoundingRules.Codes.JapanYenWithCharge);

			Create2MonthPeriod();

			AccBankAccount bank = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			bank.AB_Code = "Bank";
			bank.AB_ChequeNumDigits = 6;
			AccChequeBook chequeBook = TestObjectCreator.InsertChequeBook(100, 100, 200, bank.PK);

			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			debtor.CompanyData.SetAPTaxApplicable(false);
			debtor.CompanyData.OB_ARWHTApplicable = false;
			debtor.CompanyData.OB_APWHTApplicable = false;

			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_IsCreditor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			debtor.CompanyData.SetAPTaxApplicable(false);
			debtor.CompanyData.OB_ARWHTApplicable = false;
			debtor.CompanyData.OB_APWHTApplicable = false;

			SetupInvoiceStyles(debtor, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			Factory.Save();

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.LocalChargesPK = debtor.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = debtor.PK;
			testCharge.JR_AC = Env.Registry.FreightChargeCode;

			Factory.Save();

			var jobs = new[] { testJob };
			PostManagerValidation revenueValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
			PostManagerValidation costValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			PostManagerValidation allValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			PostManagerValidation localClientValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.LocalClient);
			AssertHasValidationError(@"The registry 'Japan IATA Import Air Local Client FRT Charge Group Rounding' is configured to add a charge line when posting charges on this shipment.
The following registry must be configured with an appropriate charge code before posting:
'Accounting > Job Invoicing > Rounding > Rounding Charge Code'", revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasValidationError(@"The registry 'Japan IATA Import Air Local Client FRT Charge Group Rounding' is configured to add a charge line when posting charges on this shipment.
The following registry must be configured with an appropriate charge code before posting:
'Accounting > Job Invoicing > Rounding > Rounding Charge Code'", allValidator);
			AssertHasValidationError(@"The registry 'Japan IATA Import Air Local Client FRT Charge Group Rounding' is configured to add a charge line when posting charges on this shipment.
The following registry must be configured with an appropriate charge code before posting:
'Accounting > Job Invoicing > Rounding > Rounding Charge Code'", localClientValidator);

			AccountingConfigurationRegistry.Instance.RoundingChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.Registry.FreightChargeCode);

			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);
			AssertHasNoValidationErrors(localClientValidator);
		}

		public void TestChequeBookValidation()
		{
			Create2MonthPeriod();

			AccBankAccount bank = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			bank.AB_Code = "Bank";
			bank.AB_ChequeNumDigits = 6;
			AccChequeBook chequeBook = TestObjectCreator.InsertChequeBook(100, 100, 200, bank.PK);

			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			debtor.CompanyData.SetAPTaxApplicable(false);
			debtor.CompanyData.OB_ARWHTApplicable = false;
			debtor.CompanyData.OB_APWHTApplicable = false;

			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_IsCreditor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			debtor.CompanyData.SetAPTaxApplicable(false);
			debtor.CompanyData.OB_ARWHTApplicable = false;
			debtor.CompanyData.OB_APWHTApplicable = false;

			SetupInvoiceStyles(debtor, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			Factory.Save();

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn();

			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.LocalChargesPK = debtor.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = debtor.PK;
			testCharge.JR_AC = Env.Registry.FreightChargeCode;

			Factory.Save();

			var jobs = new[] { testJob };
			PostManagerValidation revenueValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
			PostManagerValidation costValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			PostManagerValidation allValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);

			testCharge.JR_APInvoiceNum = "555";
			testCharge.JR_LocalCostAmt = 100;
			testCharge.JR_OH_CostAccount = creditor.PK;
			testCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testCharge.JR_APInvoiceDate = ZDateTime.Today;
			testCharge.JR_PaymentDate = ZDateTime.Today;
			testCharge.JR_AB = bank.PK;
			testCharge.JR_AK = chequeBook.PK;

			Env.Security.NewPayablesPaymentCheque.IsAllowed = true;
			testCharge.JR_PaymentType = ReceiptTypes.Cheque;
			Factory.Save();
			AssertNoErrors(testJob);

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			AssertEquals("There should be no error", string.Empty, validator.RunChequeBookValidation_ForTestOnly());

			chequeBook.AK_SQ = Factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>().PK;
			chequeBook.AK_AutoPrintCheque = true;
			AssertEquals("There should be error", "Auto printing of check is not configured properly.", validator.RunChequeBookValidation_ForTestOnly());

			chequeBook.BankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			AssertEquals("There should be no error", "", validator.RunChequeBookValidation_ForTestOnly());

			Env.Security.PrintCheque.IsAllowed = false;
			AssertEquals("There should be error", "You do not have the permission to print Check. Please Contact System Administrator.", validator.RunChequeBookValidation_ForTestOnly());

			chequeBook.AK_AutoPrintCheque = false;
			AssertEquals("There should be no error", "", validator.RunChequeBookValidation_ForTestOnly());

			chequeBook.AK_AutoPrintCheque = true;
			AssertEquals("There should be error", "You do not have the permission to print Check. Please Contact System Administrator.", validator.RunChequeBookValidation_ForTestOnly());

			Env.Security.PrintCheque.IsAllowed = true;
			AssertEquals("There should be no error", "", validator.RunChequeBookValidation_ForTestOnly());

			GlbBranch otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_Code = "ZZZ";
			chequeBook.AK_GB = otherBranch.PK;

			string expected = @"You are trying to post a payment using a check book from the 'ZZZ' branch while logged into the 'BNE' branch.
There is no appropriate security right to allow posting. Please contact your system administrator.";
			try
			{
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

				AssertEquals("There should be error", expected, validator.RunChequeBookValidation_ForTestOnly());

				AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
				AssertContains(@"Type Castle.Proxies.IJobInvoicingPlugInProxy does not have a 'Billing > Allow posting of payments where check book branch is different to login branch' security checkpoint.", ExceptionReporterTestListener.Instance[0].Message);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}

			SecurityCheckpoint billing = new SecurityCheckpoint("Root", (NoResString)"Root", null, null, false);
			SecurityCheckpoint invoicing = new SecurityCheckpoint("Root" + SecurityCore.Invoicing, (NoResString)SecurityCore.Invoicing, billing, null, false);
			SecurityCheckpoint allowPosting = new SecurityCheckpoint("Root" + SecurityCore.AllowPostingOfPaymentsWhereCheckBookBranchIsDifferentToLoginBranch, (NoResString)SecurityCore.AllowPostingOfPaymentsWhereCheckBookBranchIsDifferentToLoginBranch, invoicing, null, false);

			Mock<IJobInvoicingSupporter> mockSupporter = TestObjectCreator.GetIJobInvoicingSupporterMock();
			mockSupporter.Setup(m => m.JobInvoicingSecurity).Returns(billing);
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn("S0000001", mockSupporter);

			allowPosting.IsAllowed = true;
			AssertEquals("There should be no error", "", validator.RunChequeBookValidation_ForTestOnly());

			allowPosting.IsAllowed = false;
			expected = @"You are trying to post a payment using a check book from the 'ZZZ' branch while logged into the 'BNE' branch.
You do not have sufficient security rights to post checks from another branch.
Please use a check book from the 'BNE' branch to post this payment or contact your system administrator to give you the following security rights: Root -> Inv -> InvAllowCheckPost.";
			AssertEquals("There should be error", expected, validator.RunChequeBookValidation_ForTestOnly());
		}

		public void TestJobParentSavedValidation()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = Job.CreateWithMutex(Factory, shipment);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var jobs = new[] { job };

			job.HasChanges = false;
			shipment.HasChanges = false;

			PostManagerValidation validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
			AssertContains("Please save this form before posting costs and/or charges.", validation.ValidateJobAndParentSavedOnly());

			Factory.Save();

			job.HasChanges = false;
			shipment.HasChanges = true;

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
			AssertContains("Please save this form before posting costs and/or charges.", validation.ValidateJobAndParentSavedOnly());

			job.HasChanges = false;
			shipment.HasChanges = true;

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
			AssertContains("Please save this form before posting costs and/or charges.", validation.ValidateJobAndParentSavedOnly());

			shipment.HasChanges = false;
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
			AssertEquals("", validation.ValidateJobAndParentSavedOnly());
		}

		public void TestSuppressJobParentSaveValidation()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = Job.CreateWithMutex(Factory, shipment);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var jobs = new[] { job };

			job.HasChanges = false;
			shipment.HasChanges = false;

			PostManagerValidation validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
			AssertContains("Please save this form before posting costs and/or charges.", validation.ValidateJobAndParentSavedOnly());

			Factory.Save();

			job.HasChanges = false;
			shipment.HasChanges = true;

			using (Factory.SetTempContext(BusinessContext.PostingChargesFromLogWalker))
			{
				validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
				AssertContains("Please save this form before posting costs and/or charges.", validation.ValidateJobAndParentSavedOnly());
			}

			using (AccountingMasterFilesRegistry.Instance.AllowTriggersToSkipSaveEverythingBeforePostingValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
				AssertContains("Please save this form before posting costs and/or charges.", validation.ValidateJobAndParentSavedOnly());

				using (Factory.SetTempContext(BusinessContext.PostingChargesFromLogWalker))
				{
					validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
					AssertNullOrEmpty(validation.ValidateJobAndParentSavedOnly());
				}
			}
		}

		public void TestJobParentSavedValidation_AccessingPropertyOnADeletedJob()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (Job job = Job.CreateWithMutex(Factory, shipment))
			{
				job.Delete();
				var jobs = new[] { job };
				PostManagerValidation validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
				AssertNoExceptionThrown(() => validation.ValidateJobAndParentSavedOnly());
			}
		}

		public virtual void TestGatewaySellEligibleToPost()
		{
			var receivingGatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", false, true);
			var receivingGatewayCompany = TestObjectCreator.CreateNewCompany("CGW", orgProxy: receivingGatewayCompanyOrgProxy);

			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: receivingGatewayCompany);
			var shipment = TestObjectCreator.CreateShipment("S00123", consol);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Factory.Save();

			using (var shipmentJob = TestObjectCreator.CreateJob(shipment))
			{
				var charge = shipmentJob.Charges.AddNew();
				charge.JR_OH_SellAccount = receivingGatewayCompanyOrgProxy.PK;
				Assert("Pre-condition: charge is valid gateway charge", consol.IsGatewayCharge(charge));

				PostManagerValidation validator = NewPostManagerValidation(new[] { shipmentJob }, consol, JobInvoicingPostingOption.Gateway);
				Assert("Gateway posting cannot be done in this class", !validator.IsSellEligibleToPost_ForTestOnly(charge));
			}
		}

		public void TestRunPaymentTypeSecurityValidation()
		{
			Create2MonthPeriod();

			AccBankAccount bank = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			bank.AB_Code = "Bank";
			bank.AB_ChequeNumDigits = 6;
			AccChequeBook chequeBook = TestObjectCreator.InsertChequeBook(100, 100, 200, bank.PK);

			OrgHeader debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			debtor.CompanyData.SetAPTaxApplicable(false);
			debtor.CompanyData.OB_ARWHTApplicable = false;
			debtor.CompanyData.OB_APWHTApplicable = false;

			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.CompanyData.OB_IsCreditor = true;
			debtor.CompanyData.SetARTaxApplicable(false);
			debtor.CompanyData.SetAPTaxApplicable(false);
			debtor.CompanyData.OB_ARWHTApplicable = false;
			debtor.CompanyData.OB_APWHTApplicable = false;

			SetupInvoiceStyles(debtor, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			Factory.Save();

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.LocalChargesPK = debtor.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = debtor.PK;
			testCharge.JR_AC = Env.Registry.FreightChargeCode;

			Factory.Save();

			var jobs = new[] { testJob };
			PostManagerValidation revenueValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
			PostManagerValidation costValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			PostManagerValidation allValidator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);

			testCharge.JR_APInvoiceNum = "555";
			testCharge.JR_LocalCostAmt = 100;
			testCharge.JR_OH_CostAccount = creditor.PK;
			testCharge.CostGSTRate.SetRate_ForTestOnly(0, 1, ZDate.Today.AddMonths(-1), ZDate.Today.AddMonths(1));
			testCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testCharge.JR_APInvoiceDate = ZDateTime.Today;
			testCharge.JR_PaymentDate = ZDateTime.Today;
			testCharge.JR_AB = bank.PK;
			testCharge.JR_AK = chequeBook.PK;

			Env.Security.NewPayablesPaymentCheque.IsAllowed = false;
			Env.Security.APPaymentProcessingNewCheque.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.Cheque;
			Factory.Save();
			string expectedError = "You do not have appropriate security rights to post charges with payment type '" + testCharge.JR_PaymentType + "'.";
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasValidationError(expectedError, costValidator);
			AssertHasValidationError(expectedError, allValidator);
			Env.Security.NewPayablesPaymentCheque.IsAllowed = true;
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);
			Env.Security.APPaymentProcessingNew.IsAllowed = false;
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);

			testCharge.JR_AK = ZGuid.Empty;

			Env.Security.NewPayablesPaymentCash.IsAllowed = false;
			Env.Security.APPaymentProcessingNewCash.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.Cash;
			Factory.Save();
			expectedError = "You do not have appropriate security rights to post charges with payment type '" + testCharge.JR_PaymentType + "'.";
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasValidationError(expectedError, costValidator);
			AssertHasValidationError(expectedError, allValidator);
			Env.Security.NewPayablesPaymentCash.IsAllowed = true;
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);
			Env.Security.APPaymentProcessingNew.IsAllowed = false;
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);

			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = false;
			Env.Security.APPaymentProcessingNewCreditCard.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.CreditCard;
			testCharge.JR_ChequeNo = "555";
			Factory.Save();
			expectedError = "You do not have appropriate security rights to post charges with payment type '" + testCharge.JR_PaymentType + "'.";
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasValidationError(expectedError, costValidator);
			AssertHasValidationError(expectedError, allValidator);
			Env.Security.NewPayablesPaymentCreditCard.IsAllowed = true;
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);
			Env.Security.APPaymentProcessingNew.IsAllowed = false;
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);

			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = false;
			Env.Security.APPaymentProcessingNewDirectDebit.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.DirectDebit;
			testCharge.JR_ChequeNo = "555";
			Factory.Save();
			expectedError = "You do not have appropriate security rights to post charges with payment type '" + testCharge.JR_PaymentType + "'.";
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasValidationError(expectedError, costValidator);
			AssertHasValidationError(expectedError, allValidator);
			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = true;
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);
			Env.Security.APPaymentProcessingNew.IsAllowed = false;
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);

			Env.Security.NewPayablesPaymentEFT.IsAllowed = false;
			Env.Security.APPaymentProcessingNewEFT.IsAllowed = false;
			testCharge.JR_PaymentType = ReceiptTypes.EFT;
			testCharge.JR_ChequeNo = "555";
			Factory.Save();
			expectedError = "You do not have appropriate security rights to post charges with payment type '" + testCharge.JR_PaymentType + "'.";
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasValidationError(expectedError, costValidator);
			AssertHasValidationError(expectedError, allValidator);
			Env.Security.NewPayablesPaymentEFT.IsAllowed = true;
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);
			Env.Security.APPaymentProcessingNew.IsAllowed = false;
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);

			Env.Security.NewPayablesPaymentEFT.IsAllowed = true;
			AccTransactionLines aPLine = Factory.NewWithValidTestData<APInvoice>().Lines.AddNew();
			testCharge.ReverseAccrual(ZDateTime.Now);
			testCharge.JR_AL_APLine = aPLine.PK;
			testCharge.APLine.AL_OSAmount = testCharge.APLine.AL_LineAmount = -testCharge.JR_OSCostAmt;
			aPLine.AL_LineType = TransactionLineTypes.Cost;
			aPLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();
			AssertHasNoValidationErrors(revenueValidator);
			AssertHasNoValidationErrors(costValidator);
			AssertHasNoValidationErrors(allValidator);
		}

		[ExpectNoExceptions]
		public void TestGSTValidationWontBreakWhenDebtorNotSet()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader org1 = TestObjectCreator.AALSHI;
			org1.CompanyData.OB_IsDebtor = true;
			org1.OH_FullName = "Test Debtor";
			org1.CompanyData.SetARTaxApplicable(true);
			OrgAddress localClientAddr = org1.Addresses.AddNew();
			localClientAddr.OA_Address1 = "TST";

			SetupInvoiceStyles(org1, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			OrgHeader org2 = TestObjectCreator.ABIGAS;
			org2.CompanyData.OB_IsDebtor = true;
			org2.CompanyData.SetARTaxApplicable(true);
			OrgAddress agentCollectAddr = org2.Addresses.AddNew();
			agentCollectAddr.OA_Address1 = "test";

			SetupInvoiceStyles(org2, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			var receivingGatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", false, true);
			var receivingGatewayCompany = TestObjectCreator.CreateNewCompany("CGW", orgProxy: receivingGatewayCompanyOrgProxy);
			receivingGatewayCompanyOrgProxy.CompanyData.SetARTaxApplicable(true);
			SetupInvoiceStyles(receivingGatewayCompanyOrgProxy, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: receivingGatewayCompany);
			var shipment = TestObjectCreator.CreateShipment("S00123", consol);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			Job testJob = TestObjectCreator.CreateJob(shipment);
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = localClientAddr.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_AC = chargeCode.PK;
			ZGuid rate = testCharge.JR_AT_SellGSTRate;
			testCharge.JR_OH_SellAccount = ZGuid.Empty;
			testCharge.JR_AT_SellGSTRate = rate;
			testCharge.JR_OSSellAmt = 100m;
			Factory.Save();

			var jobs = new[] { testJob };

			PostManagerValidation validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.LocalClient);
			AssertHasNoValidationErrors(validation);

			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Agent);
			AssertHasNoValidationErrors(validation);

			validation = NewPostManagerValidation(jobs, consol, JobInvoicingPostingOption.Gateway);
			AssertHasNoValidationErrors(validation);
		}

		public void TestChargeWithInvalidGSTForDebtorWillNotValidateWhenARnotPosting()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader org1 = TestObjectCreator.AALSHI;
			org1.CompanyData.OB_IsDebtor = true;
			org1.OH_FullName = "Test Debtor";
			org1.CompanyData.SetARTaxApplicable(true);
			OrgAddress localClientAddr = org1.Addresses.AddNew();
			localClientAddr.OA_Address1 = "TST";

			SetupInvoiceStyles(org1, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			OrgHeader org2 = TestObjectCreator.ABIGAS;
			org2.CompanyData.OB_IsDebtor = true;
			org2.CompanyData.SetARTaxApplicable(true);
			OrgAddress agentCollectAddr = org2.Addresses.AddNew();
			agentCollectAddr.OA_Address1 = "test";

			SetupInvoiceStyles(org2, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			var receivingGatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", false, true);
			var receivingGatewayCompany = TestObjectCreator.CreateNewCompany("CGW", orgProxy: receivingGatewayCompanyOrgProxy);
			receivingGatewayCompanyOrgProxy.CompanyData.SetARTaxApplicable(true);
			SetupInvoiceStyles(receivingGatewayCompanyOrgProxy, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: receivingGatewayCompany);
			var shipment = TestObjectCreator.CreateShipment("S00123", consol);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			Job testJob = TestObjectCreator.CreateJob(shipment);
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = localClientAddr.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = org1.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_OSSellAmt = 100m;

			Factory.Save();
			var jobs = new[] { testJob };

			org1.CompanyData.SetARTaxApplicable(false);

			Assert("Precondition: Invalid State exists - TAX ID is set on charge but debtor is not gst applicable", !testCharge.JR_AT_SellGSTRate.IsEmpty && !org1.CompanyData.IsARTaxApplicable);

			string expectedError = string.Format("Job {0} has error: Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the debtor \"Tax is Applicable\" flag.\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.", testJob.Parent.JobNumber);

			var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			AssertHasValidationError(expectedError, validation);

			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
			AssertHasValidationError(expectedError, validation);

			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_OH_SellAccount = org1.PK;
			Factory.Save();
			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Agent);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_OH_SellAccount = receivingGatewayCompanyOrgProxy.PK;
			Factory.Save();
			validation = NewPostManagerValidation(jobs, consol, JobInvoicingPostingOption.Gateway);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_OH_SellAccount = org2.PK;
			Factory.Save();
			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.LocalClient);
			AssertHasNoValidationErrors(validation);

			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.CustomsDSBChargeAPOnly);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_InvoiceType = DisbursementInvoiceType;
			Factory.Save();
			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.CustomsDSBChargeAROnly);
			AssertHasNoValidationErrors(validation);

			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Disbursement);
			AssertHasNoValidationErrors(validation);
		}

		public void TestChargeWithInvalidGSTForDebtorWillNotValidateWhenChargeAmountIsZero()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader localClient = TestObjectCreator.AALSHI;
			localClient.CompanyData.OB_IsDebtor = true;
			localClient.OH_FullName = "Test Debtor";
			localClient.CompanyData.SetARTaxApplicable(true);
			OrgAddress localClientAddr = localClient.Addresses.AddNew();
			localClientAddr.OA_Address1 = "TST";

			SetupInvoiceStyles(localClient, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			OrgHeader agent = TestObjectCreator.ABIGAS;
			agent.CompanyData.OB_IsDebtor = true;
			agent.CompanyData.SetARTaxApplicable(true);
			OrgAddress agentCollectAddr = agent.Addresses.AddNew();
			agentCollectAddr.OA_Address1 = "test";

			SetupInvoiceStyles(agent, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			var receivingGatewayCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("GTWORG", false, true);
			var receivingGatewayCompany = TestObjectCreator.CreateNewCompany("CGW", orgProxy: receivingGatewayCompanyOrgProxy);
			receivingGatewayCompanyOrgProxy.CompanyData.SetARTaxApplicable(true);
			SetupInvoiceStyles(receivingGatewayCompanyOrgProxy, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: receivingGatewayCompany);
			var shipment = TestObjectCreator.CreateShipment("S00123", consol);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			Job testJob = TestObjectCreator.CreateJob(shipment);
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = localClientAddr.PK;
			testJob.JH_OA_AgentCollectAddr = agentCollectAddr.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = localClient.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_OSSellAmt = 0;

			Factory.Save();
			var jobs = new[] { testJob };

			localClient.CompanyData.SetARTaxApplicable(false);
			agent.CompanyData.SetARTaxApplicable(false);

			Assert("Precondition: Invalid State exists - TAX ID is set on charge and debtor is not gst applicable BUT sell amount is zero so Validation should not occur", !testCharge.JR_AT_SellGSTRate.IsEmpty && !localClient.CompanyData.IsARTaxApplicable && testCharge.JR_LocalSellAmt == 0);

			var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			AssertHasNoValidationErrors(validation);

			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
			AssertHasNoValidationErrors(validation);

			agent.CompanyData.SetARTaxApplicable(true);
			testCharge.JR_OH_SellAccount = agent.PK;
			agent.CompanyData.SetARTaxApplicable(false);
			Assert("Precondition: Debtor is Agent, agent is not tax applicable and charge has tax id", testCharge.JR_OH_SellAccount == testJob.AgentCollectPK && !agent.CompanyData.IsARTaxApplicable && !testCharge.JR_AT_SellGSTRate.IsEmpty);
			Factory.Save();
			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Agent);
			AssertHasNoValidationErrors(validation);

			receivingGatewayCompanyOrgProxy.CompanyData.SetARTaxApplicable(true);
			testCharge.JR_OH_SellAccount = receivingGatewayCompanyOrgProxy.PK;
			receivingGatewayCompanyOrgProxy.CompanyData.SetARTaxApplicable(false);
			Assert("Precondition: Debtor is Gateway Agent, receivingGatewayCompanyOrgProxy is not tax applicable and charge has not tax id", consol.IsGatewayCharge(testCharge) && !receivingGatewayCompanyOrgProxy.CompanyData.IsARTaxApplicable && !testCharge.JR_AT_SellGSTRate.IsEmpty);
			Factory.Save();
			validation = NewPostManagerValidation(jobs, consol, JobInvoicingPostingOption.Gateway);
			AssertHasNoValidationErrors(validation);

			localClient.CompanyData.SetARTaxApplicable(true);
			testCharge.JR_OH_SellAccount = localClient.PK;
			localClient.CompanyData.SetARTaxApplicable(false);
			Factory.Save();
			Assert("Precondition: Debtor is LocalClient, localclient is not tax applicable and charge has not tax id", testCharge.JR_OH_SellAccount == testJob.LocalChargesPK && !localClient.CompanyData.IsARTaxApplicable && !testCharge.JR_AT_SellGSTRate.IsEmpty);
			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.LocalClient);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			testCharge.PostARWhenInvokedByCustomInvoiceCreator = true;
			Factory.Save();
			Assert("Precondition: If charge is DSB and PostARWhenInvokedByCu... is true, the charge is a CustomsDSBAROnly charge",
				InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(testCharge.JR_InvoiceType) && testCharge.PostARWhenInvokedByCustomInvoiceCreator);
			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.CustomsDSBChargeAROnly);
			AssertHasNoValidationErrors(validation);

			Assert("PreCondition: Invoice type is Disbursement", InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(testCharge.JR_InvoiceType));
			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Disbursement);
			AssertHasNoValidationErrors(validation);
		}

		public void TestChargeWithInvalidGSTForCreditorWillNotValidateWhenChargeAmountIsZero()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader localClient = TestObjectCreator.AALSHI;
			localClient.CompanyData.OB_IsCreditor = true;
			localClient.CompanyData.OB_IsDebtor = true;
			localClient.OH_FullName = "Test Creditor";
			localClient.CompanyData.SetAPTaxApplicable(true);
			OrgAddress localClientAddr = localClient.Addresses.AddNew();
			localClientAddr.OA_Address1 = "TST";

			SetupInvoiceStyles(localClient, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			OrgHeader agent = TestObjectCreator.ABIGAS;
			agent.CompanyData.OB_IsCreditor = true;
			agent.CompanyData.OB_IsDebtor = true;
			agent.CompanyData.SetAPTaxApplicable(true);
			OrgAddress agentCollectAddr = agent.Addresses.AddNew();
			agentCollectAddr.OA_Address1 = "test";

			SetupInvoiceStyles(agent, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = localClientAddr.PK;
			testJob.JH_OA_AgentCollectAddr = agentCollectAddr.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_CostAccount = localClient.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_OSCostAmt = 0;

			Factory.Save();
			var jobs = new[] { testJob };

			localClient.CompanyData.SetAPTaxApplicable(false);
			agent.CompanyData.SetAPTaxApplicable(false);

			Assert("Precondition: Invalid State exists - TAX ID is set on charge and Creditor is not gst applicable BUT Cost amount is zero so Validation should not occur", !testCharge.JR_AT_CostGSTRate.IsEmpty && !localClient.CompanyData.IsAPTaxApplicable && testCharge.JR_LocalCostAmt == 0);

			testJob.Validation.ValidateAll();
			AssertNoErrors(testJob);

			PostManagerValidation validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
			AssertHasNoValidationErrors(validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Costs, jobs);
			AssertHasNoValidationErrors(validation);

			agent.CompanyData.SetAPTaxApplicable(true);
			testCharge.JR_OH_CostAccount = agent.PK;
			agent.CompanyData.SetAPTaxApplicable(false);
			Assert("Precondition: Creditor is Agent, agent is not tax applicable and charge has tax id", testCharge.JR_OH_CostAccount == testJob.AgentCollectPK && !agent.CompanyData.IsAPTaxApplicable && !testCharge.JR_AT_CostGSTRate.IsEmpty);
			Factory.Save();
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Agent, jobs);
			AssertHasNoValidationErrors(validation);

			localClient.CompanyData.SetAPTaxApplicable(true);
			testCharge.JR_OH_CostAccount = localClient.PK;
			localClient.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();
			Assert("Precondition: Creditor is LocalClient, localclient is not tax applicable and charge has not tax id", testCharge.JR_OH_CostAccount == testJob.LocalChargesPK && !localClient.CompanyData.IsAPTaxApplicable && !testCharge.JR_AT_CostGSTRate.IsEmpty);
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.LocalClient, jobs);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_InvoiceType = InvoiceTypesList.Codes.DisbursementInvoice;
			testCharge.PostAPWhenInvokedByCustomInvoiceCreator = true;
			Factory.Save();
			Assert("Precondition: If charge is DSB and PostARWhenInvokedByCu... is true, the charge is a CustomsDSBAROnly charge", InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(testCharge.JR_InvoiceType) && testCharge.PostAPWhenInvokedByCustomInvoiceCreator);
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.CustomsDSBChargeAPOnly, jobs);
			AssertHasNoValidationErrors(validation);

			Assert("PreCondition: Charge is Disbursement", InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(testCharge.JR_InvoiceType));
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Disbursement, jobs);
			AssertHasNoValidationErrors(validation);
		}

		public void TestChargeWithInvalidGSTForCreditorWillNotValidateWhenAPnotPosting()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader org1 = TestObjectCreator.AALSHI;
			org1.CompanyData.OB_IsCreditor = true;
			org1.OH_FullName = "Test Debtor";
			org1.CompanyData.SetAPTaxApplicable(true);
			OrgAddress localClientAddr = org1.Addresses.AddNew();
			localClientAddr.OA_Address1 = "TST";

			SetupInvoiceStyles(org1, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			OrgHeader org2 = TestObjectCreator.ABIGAS;
			org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.SetAPTaxApplicable(true);
			OrgAddress agentCollectAddr = org2.Addresses.AddNew();
			agentCollectAddr.OA_Address1 = "test";

			SetupInvoiceStyles(org2, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = localClientAddr.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_CostAccount = org1.PK;
			testCharge.JR_OH_SellAccount = org2.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_OSCostAmt = 100m;
			testCharge.JR_APInvoiceNum = "ABC1";
			testCharge.JR_APInvoiceDate = ZDateTime.Now;
			Assert(!testCharge.JR_AT_CostGSTRate.IsEmpty);

			AssertNoErrors(testJob);
			AssertNoErrors(testCharge);

			Factory.Save();
			var jobs = new[] { testJob };

			org1.CompanyData.SetAPTaxApplicable(false);

			AssertNoErrors(testJob);
			AssertNoErrors(testCharge);

			PostManagerValidation validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
			AssertHasValidationError("Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the creditor \"Tax is Applicable\" flag.\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.", validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Costs, jobs);
			AssertHasValidationError("Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the creditor \"Tax is Applicable\" flag.\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.", validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Revenue, jobs);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_APInvoiceNum = ZString.Empty;
			testCharge.JR_APInvoiceDate = ZDateTime.Empty;
			testCharge.JR_PaymentDate = ZDateTime.Empty;
			Factory.Save();

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
			AssertHasNoValidationErrors(validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Costs, jobs);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_OH_CostAccount = org1.PK;
			Factory.Save();
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Agent, jobs);
			AssertHasNoValidationErrors(validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.LocalClient, jobs);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_OH_CostAccount = org2.PK;
			testCharge.JR_APInvoiceNum = "ABC2";
			testCharge.JR_APInvoiceDate = ZDateTime.Now;
			org2.CompanyData.SetAPTaxApplicable(false);
			testCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			testJob.JH_OA_AgentCollectAddr = agentCollectAddr.PK;
			Factory.Save();
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.LocalClient, jobs);
			AssertHasNoValidationErrors(validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Agent, jobs);
			AssertHasNoValidationErrors(validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.CustomsDSBChargeAROnly, jobs);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_InvoiceType = DisbursementInvoiceType;
			Factory.Save();
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.CustomsDSBChargeAPOnly, jobs);
			AssertHasNoValidationErrors(validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Disbursement, jobs);
			AssertHasNoValidationErrors(validation);
		}

		public void TestChargeWithInvalidGSTForSelfBilledCostsCreditorWillNotValidateWhenAPnotPosting()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader org1 = TestObjectCreator.AALSHI;
			org1.CompanyData.OB_IsCreditor = true;
			org1.CompanyData.OB_APCostsSelfBilled = true;
			org1.OH_FullName = "Test Debtor";
			org1.CompanyData.SetAPTaxApplicable(true);
			OrgAddress localClientAddr = org1.Addresses.AddNew();
			localClientAddr.OA_Address1 = "TST";

			SetupInvoiceStyles(org1, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			OrgHeader org2 = TestObjectCreator.ABIGAS;
			org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.OB_APCostsSelfBilled = true;
			org2.CompanyData.SetAPTaxApplicable(true);
			OrgAddress agentCollectAddr = org2.Addresses.AddNew();
			agentCollectAddr.OA_Address1 = "test";

			SetupInvoiceStyles(org2, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = localClientAddr.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_CostAccount = org1.PK;
			testCharge.JR_OH_SellAccount = org2.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_OSCostAmt = 100m;
			Assert(!testCharge.JR_AT_CostGSTRate.IsEmpty);

			AssertNoErrors(testJob);
			AssertNoErrors(testCharge);

			Factory.Save();
			var jobs = new[] { testJob };

			org1.CompanyData.SetAPTaxApplicable(false);

			AssertNoErrors(testJob);
			AssertNoErrors(testCharge);

			PostManagerValidation validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
			AssertHasValidationError("Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the creditor \"Tax is Applicable\" flag.\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.", validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Costs, jobs);
			AssertHasValidationError("Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the creditor \"Tax is Applicable\" flag.\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.", validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Revenue, jobs);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_OH_CostAccount = org1.PK;
			Factory.Save();
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Agent, jobs);
			AssertHasNoValidationErrors(validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.LocalClient, jobs);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_OH_CostAccount = org2.PK;
			org2.CompanyData.SetAPTaxApplicable(false);
			testCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			testJob.JH_OA_AgentCollectAddr = agentCollectAddr.PK;
			Factory.Save();
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.LocalClient, jobs);
			AssertHasNoValidationErrors(validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Agent, jobs);
			AssertHasNoValidationErrors(validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.CustomsDSBChargeAROnly, jobs);
			AssertHasNoValidationErrors(validation);

			testCharge.JR_InvoiceType = DisbursementInvoiceType;
			Factory.Save();
			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.CustomsDSBChargeAPOnly, jobs);
			AssertHasNoValidationErrors(validation);

			validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.Disbursement, jobs);
			AssertHasNoValidationErrors(validation);
		}

		public void TestRunGSTApplicabilityValidation()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OrgHeader org1 = TestObjectCreator.AALSHI;
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.SetARTaxApplicable(true);
			org1.CompanyData.OB_ARWHTApplicable = false;
			org1.OH_FullName = "Test Debtor";

			SetupInvoiceStyles(org1, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			OrgHeader org2 = TestObjectCreator.ABIGAS;
			org2.CompanyData.OB_IsCreditor = true;
			org2.CompanyData.SetAPTaxApplicable(true);
			org2.CompanyData.OB_APWHTApplicable = false;

			OrgAddress localChargesAddr = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress agentCollectAddr = Factory.NewWithValidTestData<OrgAddress>();

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = localChargesAddr.PK;
			testJob.JH_OA_AgentCollectAddr = agentCollectAddr.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_OH_SellAccount = org1.PK;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_OSSellAmt = 100m;

			Factory.Save();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			Factory.Save();

			var jobs = new[] { testJob };

			string expectedError = string.Format("Job {0} has error: Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the debtor \"Tax is Applicable\" flag.\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.", testJob.Parent.JobNumber);

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);

			Assert("Sell GST rate should not be actual", !testCharge.IsSellGSTRateActual);
			AssertHasValidationError(expectedError, validator);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org1.CompanyData.SetARTaxApplicable(false);
			Factory.Save();

			Assert("Sell GST rate should not be actual", !testCharge.IsSellGSTRateActual);
			AssertHasValidationError(expectedError, validator);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org1.CompanyData.SetARTaxApplicable(true);
			Factory.Save();

			Assert("Sell GST rate should be actual", testCharge.IsSellGSTRateActual);
			AssertHasNoValidationErrors(validator);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			Factory.Save();

			AssertHasValidationError(expectedError, validator);

			testCharge.JR_OH_CostAccount = org2.PK;
			testCharge.JR_APInvoiceNum = "ABC1";
			testCharge.JR_APInvoiceDate = ZDateTime.Now;
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_OSCostAmt = 100m;
			testCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;

			Factory.Save();

			Assert("Cost GST rate should not be actual", !testCharge.IsCostGSTRateActual);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org2.CompanyData.SetAPTaxApplicable(false);
			Factory.Save();

			Assert("Cost GST rate should not be actual", !testCharge.IsCostGSTRateActual);
			validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			expectedError = "Tax Data Validation error.\r\n\r\nTax IDs on unposted charges in the billing tab conflict with the creditor \"Tax is Applicable\" flag.\r\n\r\nTo resolve this, go into the \"Job Invoicing\" menu and click the \"Reset Unposted lines Tax Default\" option.";
			AssertHasValidationError(expectedError, validator);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			org2.CompanyData.SetAPTaxApplicable(true);
			Factory.Save();

			Assert("Cost GST rate should be actual", testCharge.IsCostGSTRateActual);
			AssertHasNoValidationErrors(validator);
		}

		public void TestDebtorValidationValidatesActiveDebtor_Shipment()
		{
			SetupTest();

			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge", TestObjectCreator.AUD, 100, TestObjectCreator.AALSHI, "INV11", TestObjectCreator.AUD, 120, Debtor);

			Factory.Save();

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Debtor.OH_IsActive = false;
			Factory.Save();

			string expectedError = string.Format("Job {0} has error: The Debtor is inactive - it may not be used.", job.Parent.JobNumber);

			var jobs = new[] { job };

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
			AssertHasValidationError(expectedError, validator);
		}

		public void TestDebtorValidationValidatesActiveDebtor_Consol()
		{
			SetupTest();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100M, Creditor);

			Job job = cost.ApportionmentCharges[0].InvoicingJob;
			cost.ApportionmentCharges[0].JR_GE = TestObjectCreator.FIADepartment.PK;
			cost.ApportionmentCharges[0].JR_OH_SellAccount = Debtor.PK;
			job.JH_GE = TestObjectCreator.FIADepartment.PK;
			job.JH_OA_AgentCollectAddr = Agent.MainAddress.PK;
			job.JH_OA_LocalChargesAddr = Creditor1.MainAddress.PK;

			Factory.Save();

			AssertNoErrors(job);

			Debtor.OH_IsActive = false;
			Factory.Save();

			var jobs = new[] { job };

			string expectedError = string.Format("Job {0} has error: The Debtor is inactive - it may not be used.", job.Parent.JobNumber);

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
			AssertHasValidationError(expectedError, validator);
		}

		public void TestCreditorValidationValidatesActiveCreditor_Shipment()
		{
			SetupTest();

			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge", TestObjectCreator.AUD, 100, Creditor, "INV11", TestObjectCreator.AUD, 120, Debtor);

			Factory.Save();

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			Creditor.OH_IsActive = false;
			Factory.Save();

			string expectedError = string.Format("Job {0} has error: The Creditor is inactive - it may not be used.", job.Parent.JobNumber);

			var jobs = new[] { job };

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			AssertHasValidationError(expectedError, validator);
		}

		public void TestCreditorValidationValidatesActiveCreditor_Consol()
		{
			SetupTest();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var shipment = TestObjectCreator.CreateShipment("S001001", consol);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100M, Creditor);
			cost.E6_InvoiceNum = "AP123";

			Job job = cost.ApportionmentCharges[0].InvoicingJob;
			cost.ApportionmentCharges[0].JR_GE = TestObjectCreator.FIADepartment.PK;
			cost.ApportionmentCharges[0].JR_OH_CostAccount = Creditor.PK;
			job.JH_GE = TestObjectCreator.FIADepartment.PK;
			job.JH_OA_AgentCollectAddr = Agent.MainAddress.PK;
			job.JH_OA_LocalChargesAddr = Creditor1.MainAddress.PK;

			Factory.Save();

			AssertNoErrors(job);

			Creditor.OH_IsActive = false;
			Factory.Save();

			var jobs = new[] { job };

			string expectedError = string.Format("Job {0} has error: The Creditor is inactive - it may not be used.", job.Parent.JobNumber);

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			AssertHasValidationError(expectedError, validator);
		}

		[TestDate(2021, 10, 23)]
		public void TestRunPeriodValidation()
		{
			SetupTest(false);

			var job = CreateJobForTest();
			CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			var validation = NewPostManagerValidation(new[] { job }, JobInvoicingPostingOption.All);

			AssertHasValidationError(@"There is no period set up for 23-Oct-21.
Please go to General Ledger >> Period Management to setup periods.", validation);

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			Factory.Save();

			AssertHasNoValidationErrors(validation);
			AssertHasNoValidationWarnings(validation);
		}

		public void TestRunJobNotFoundValidation()
		{
			SetupTest();

			var jobs = Array.Empty<Job>();
			var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

			AssertHasValidationError("You cannot post because no Job Invoices have been created.", validation);

			var job = CreateJobForTest();
			CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			jobs = new[] { job };
			validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			AssertHasNoValidationErrors(validation);
			AssertHasNoValidationWarnings(validation);
		}

		public void TestRunNoChargesValidation()
		{
			SetupTest();

			var job = CreateJobForTest();
			Factory.Save();

			var validationCosts = NewPostManagerValidation(new[] { job }, JobInvoicingPostingOption.Costs);
			var validationAll = NewPostManagerValidation(new[] { job }, JobInvoicingPostingOption.All);

			AssertNoChargesValidation(validationCosts);
			AssertNoChargesValidation(validationAll);

			CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			AssertHasNoValidationErrors(validationCosts);
			AssertHasNoValidationWarnings(validationCosts);
			AssertHasNoValidationErrors(validationAll);
			AssertHasNoValidationWarnings(validationAll);
		}

		public void TestRunJobsContainErrorsValidation()
		{
			SetupTest();

			var job = CreateJobForTest();
			var charge = CreateCharge(job, CC1, "Charge Code 1", AUD, -100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			var validation = NewPostManagerValidation(new[] { job }, JobInvoicingPostingOption.All);
			AssertHasValidationError($@"You cannot post because job {job.JH_JobNum} has errors. Please fix errors before posting.
 - Overseas Cost Amount: A negative cost can only be entered if you specify an AP Invoice Date and Invoice Number or if the Creditor is setup to issue self billing invoices (Organization -> A/P -> Configuration -> Issue Self Billing Invoice).
Note that negative accruals are not created.", validation);

			charge.JR_OSCostAmt = 100M;
			Factory.Save();

			AssertHasNoValidationErrors(validation);
			AssertHasNoValidationWarnings(validation);
		}

		[TestDate(2021, 11, 04)]
		public void TestRunConsolCostRelativeValidation()
		{
			SetupTest();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			consol.Shipments.AddNew();
			var apps = new ApportionmentListing(Factory, consol);
			var cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 251m;
			cost.E6_OH_Creditor = Creditor1.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_AB_BankAccount = AUDBankAccount.PK;
			cost.E6_ChequeOrReference = "32423";
			cost.E6_PaymentDate = ZDateTime.Now.AddDays(-1);
			cost.E6_InvoiceDate = ZDateTime.Now.AddDays(-1);
			cost.UpdateApportionmentChargesListing();
			Factory.Save();

			Job job = cost.ApportionmentCharges[0].InvoicingJob;

			job.JH_OA_AgentCollectAddr = Agent.MainAddress.PK;
			job.JH_OA_LocalChargesAddr = Creditor1.MainAddress.PK;
			job.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			Factory.Save();

			var charge = job.Charges.Cast<Charge>().FirstOrDefault();
			AssertNotNull(charge);

			var validation = NewPostManagerValidation(new[] { job }, JobInvoicingPostingOption.All);

			AssertHasNoValidationErrors(validation);
			AssertHasNoValidationWarnings(validation);

			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				var newFactory = new BusinessObjectFactory();
				var chargeInNewFactory = newFactory.Load<Charge>(charge.PK);
				chargeInNewFactory.JR_PaymentDate = TestDateAttribute.Date;
				newFactory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			using (AccountingConfigurationRegistry.Instance.EnableValidationForChargeWhenPostTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertHasValidationError("Charge invoice date and payment date must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.", validation);
			}
		}

		public void TestChargeCodeValidation()
		{
			SetupTest();

			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			testJob.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			var testCharge = testJob.Charges.AddNew();
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_OH_SellAccount = Debtor.PK;
			testCharge.JR_OSSellAmt = 100m;

			Factory.Save();

			testJob.RunPreSaveValidation();
			AssertNoErrors(testJob);

			var invalidChargeCode = Factory.Load<AccChargeCode>(chargeCode.PK);
			invalidChargeCode.AC_IsActive = false;
			Factory.Save();

			AssertNoErrors(testJob);

			string expectedError = "Invalid Charge Code(s): ZZCC1.";

			var jobs = new[] { testJob };

			var validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);

			AssertHasValidationError(expectedError, validator);

			invalidChargeCode.AC_IsActive = true;
			Factory.Save();

			AssertContains("Revenue Recognition Info", "\r\nRevenueRecognitionTypeFromJobDuringPreSaveValidation: There is no data collected for this PK.",
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(testCharge.ChargeCode.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringPreSaveValidation));
			var validationResult = validator.Validate();
			AssertNull("Validation result should be null", validationResult);
			AssertEquals("Revenue Recognition Info", FormattableString.Invariant($@"
RevenueRecognitionTypeFromJobDuringPreSaveValidation:
ChargeCode PK: {testCharge.ChargeCode.PK}
Job PK: {testJob.PK}

Revenue Recognition Type Details: 
Revenue Recognition Type: IMM 
JobType: SHP
Direction: EXP
Mode: AIR
Broker: 

isToBeCheckedForRevenueRecognitionValidationError: True
GetRevenueRecognitionValidationError result: "),
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(testCharge.ChargeCode.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeFromJobDuringPreSaveValidation));
		}

		public void TestChargeCodeValidation_UseTVPParameter()
		{
			var testJob = Factory.NewJobWithValidTestDataForTesting<Job>();

			var validation = new PostManagerValidation(new[] { testJob }, JobInvoicingPostingOption.Costs, null);
			validation.RunChargeCodeValidation_ForTestOnly();

			var lastQuery = SqlEventTracker.Instance.LastSqlQuery;
			AssertContains("Should use TVP for JobPKs", "JR_JH in (SELECT Value FROM @JobPKs)", lastQuery);
		}

		public void TestDebtorValidation()
		{
			SetupTest();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			testJob.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;

			Factory.Save();

			testJob.RunPreSaveValidation();
			AssertNoErrors(testJob);

			BusinessObjectFactory updateFactory = new BusinessObjectFactory();
			updateFactory.RefreshEnabled = false;
			OrgHeader org1InNewFactory = updateFactory.Load<OrgHeader>(Debtor.PK);
			org1InNewFactory.CompanyData.OB_IsDebtor = false;
			Assert(!org1InNewFactory.CompanyData.OB_IsDebtor);
			updateFactory.Save();

			AssertNoErrors(testJob);

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_OH_SellAccount = Debtor.PK;
			testCharge.JR_OSSellAmt = 100m;

			Factory.Save();

			string expectedError = string.Format(@"You cannot post because job {0} has errors. Please fix errors before posting.
 - Debtor: Enter a valid Debtor.", testJob.Parent.JobNumber);

			var jobs = new[] { testJob };

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			AssertHasValidationError(expectedError, validator);

			validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
			AssertHasValidationError(expectedError, validator);
		}

		public void TestInvoiceTypeValidation()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			OrgHeader org1 = TestObjectCreator.AALSHI;
			org1.CompanyData.OB_IsDebtor = true;
			org1.CompanyData.SetARTaxApplicable(true);
			org1.CompanyData.OB_ARWHTApplicable = false;
			org1.OH_FullName = "Test Debtor";

			SetupInvoiceStyles(org1, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			OrgAddress localChargesAddr = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress agentCollectAddr = Factory.NewWithValidTestData<OrgAddress>();

			Factory.Save();

			AccChargeCode chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AT_GSTRate = TestObjectCreator.GST1.PK;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			testJob.JH_OA_LocalChargesAddr = localChargesAddr.PK;
			testJob.JH_OA_AgentCollectAddr = agentCollectAddr.PK;

			Charge testCharge = testJob.Charges.AddNew();
			testCharge.JR_AC = chargeCode.PK;
			testCharge.JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			testCharge.JR_OSSellAmt = 100m;

			Factory.Save();

			testJob.RunPreSaveValidation();
			AssertNoErrors(testJob);

			using (testCharge.GetValidationSuspender())
			{
				testCharge.JR_InvoiceType = ""; //simulate invalid invoice type
			}

			AssertNoErrors(testJob);
			Factory.Save();

			string expectedError = string.Format("Job {0} has error: Invalid invoice type: Please enter an Invoice Type.", testJob.Parent.JobNumber);

			var jobs = new[] { testJob };
			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			AssertHasNoValidationErrors(validator);

			if (ShouldErrorWhenInvoiceTypeIsEmpty)
			{
				validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);
				AssertHasValidationError(expectedError, validator);
			}
		}

		public void TestValidate()
		{
			SetupInvoiceStyles(LocalClient, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);

			LocalClient.Factory.Save();

			var jobs = new List<Job>();

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

			TestCaseHelper.ClearTable("AccPeriodManagement");
			AssertHasValidationError(string.Format("There is no period set up for {0}.\r\nPlease go to General Ledger >> Period Management to setup periods.", ZDateTime.Today.ToShortDateString()), validator);
			Create2MonthPeriod();

			AssertHasValidationError("You cannot post because no Job Invoices have been created.", validator);

			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			jobs.Add(testJob);
			AssertHasValidationError("Please save job Z00001000 before posting costs and/or charges.\n", validator);
			Factory.Save();

			AssertNoChargesValidation(validator);
			CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);

			testJob.LocalChargesPK = ZGuid.Empty;
			Factory.Save();
			AssertHasNoValidationErrors(validator);

			testJob.LocalChargesPK = LocalClient.PK;
			Factory.Save();
			AssertHasNoValidationErrors(validator);
		}

		public void TestRunRevenueRecognitionArrivalAndDepartureDateValidation()
		{
			Create2MonthPeriod();
			var jobs = new List<Job>();
			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);

			AccChargeRevRecOverride override1 = CC1.RevenueRecOverrides.AddNew();
			override1.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			override1.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			override1.Mode = Core.Constants.TransportModes.Air;
			override1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;

			AccChargeRevRecOverride override2 = CC2.RevenueRecOverrides.AddNew();
			override2.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			override2.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			override2.Mode = Core.Constants.TransportModes.Air;
			override2.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;

			Factory.Save();

			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			testJob.Parent = shipment;
			jobs.Add(testJob);
			CreateCharge(testJob, CC1, "FIRST CHARGE", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			CreateCharge(testJob, CC2, "SECOND CHARGE", AUD, 100M, null, AUD, 150M, LocalClient);

			testJob.LocalChargesPK = ZGuid.Empty;

			testJob.Charges[0].JR_APInvoiceNum = "1001010";
			testJob.Charges[0].JR_APInvoiceDate = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now;

			Factory.Save();

			AssertHasNoValidationErrors(validator);
		}

		public void TestRunRevenueRecognitionDateValidation()
		{
			Create2MonthPeriod();

			var jobs = new List<Job>();
			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			testJob.Parent = shipment;
			jobs.Add(testJob);
			CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			testJob.LocalChargesPK = ZGuid.Empty;
			Factory.Save();

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting1 = valuesForTest.AddNew();
			setting1.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting1.Mode = Core.Constants.TransportModes.Air;
			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			string expectedErrorMessage = "Posting is prevented. The following information has not been recorded for this job. It is required for revenue recognition purposes and must be recorded before posting can occur: 'Delivery Date'";
			AssertHasValidationError(expectedErrorMessage, validator);

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertHasValidationError(expectedErrorMessage, validator);

			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			AssertHasNoValidationErrors(validator);

			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			expectedErrorMessage = "Posting is prevented. The following information has not been recorded for this job. It is required for revenue recognition purposes and must be recorded before posting can occur: 'Customs Clearance Date'";
			AssertHasValidationError(expectedErrorMessage, validator);

			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			expectedErrorMessage = "Posting is prevented. The following information has not been recorded for this job. It is required for revenue recognition purposes and must be recorded before posting can occur: 'Pickup Date'";
			AssertHasValidationError(expectedErrorMessage, validator);
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertHasNoValidationErrors(validator);
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Now.AddDays(10);
			Factory.Save();
			AssertHasNoValidationErrors(validator);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.BrettsBirthday;
			Factory.Save();
			INotification notification = validator.Validate();
			AssertNotNull("Should be Notification", notification);
			AssertEquals("There should be an error", CargoWise.EntityFramework.NotificationType.Error, notification.Type);
			Assert("There should be specific error", notification.Message.Contains(ZDateTime.BrettsBirthday.Date.ToShortDateString()));
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			notification = validator.Validate();
			AssertNotNull("Should be Notification", notification);
			AssertEquals("There should be an error", CargoWise.EntityFramework.NotificationType.Error, notification.Type);
			Assert("There should be period error for deferred recognition also.", notification.Message.Contains(ZDateTime.BrettsBirthday.Date.ToShortDateString()));
			AccountingConfigurationRegistry.Instance.EnableDeferredRevenueRecognition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			Factory.Save();
			AssertHasNoValidationErrors(validator);
		}

		public void TestBillOfLadingRevenueRecognitionValidationForVADAndVDD()
		{
			Create2MonthPeriod();

			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "USLAX";
			var origin = Factory.NewWithValidTestData<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var destn = Factory.NewWithValidTestData<VoyageDestination>();
			destn.JB_RL_NKPortOfDischarge = "USLAX";

			var sailing = billOfLading.Sailings.AddNew();
			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destn.PK;

			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			testJob.Parent = billOfLading;
			CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient, ZArchitecture.Core.AgencyInvoiceTypesList.Codes.LocalPrePaid, GlbBranch.CurrentBranch.PK);
			testJob.LocalChargesPK = ZGuid.Empty;

			Factory.Save();

			var validator = NewPostManagerValidation(new[] { testJob }, JobInvoicingPostingOption.All);
			var valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting1 = valuesForTest.AddNew();
			setting1.JobType = JobInvoicingConsumerTypes.AgencyBillOfLading.Code;
			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.All;

			#region VesselArrivalDate

			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselArrivalDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			string vadExpectedErrorMessage = "Posting is prevented. The following information has not been recorded for this job. It is required for revenue recognition purposes and must be recorded before posting can occur: 'Vessel Arrival Date'";
			AssertHasValidationError(vadExpectedErrorMessage, validator);

			destn.JB_A_ARV = DateTime.Today.AddDays(2);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertHasNoValidationErrors(validator);

			#endregion

			#region VesselDepartureDate

			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselDepartureDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			string vddExpectedErrorMessage = "Posting is prevented. The following information has not been recorded for this job. It is required for revenue recognition purposes and must be recorded before posting can occur: 'Vessel Departure Date'";
			AssertHasValidationError(vddExpectedErrorMessage, validator);

			origin.JA_A_DEP = DateTime.Today.AddDays(-5);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			AssertHasNoValidationErrors(validator);

			#endregion
		}

		public void TestRunRevenueRecognitionDateValidationForDateBeforeFirstPeriod()
		{
			Create2MonthPeriod();

			var jobs = new List<Job>();
			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testJob.Parent = shipment;
			jobs.Add(testJob);
			CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			testJob.LocalChargesPK = ZGuid.Empty;
			Factory.Save();

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting1 = valuesForTest.AddNew();
			setting1.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting1.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting1.Mode = Core.Constants.TransportModes.Air;
			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.BrettsBirthday;
			Factory.Save();
			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			INotification notification = validator.Validate();
			AssertNotNull("Should be Notification", notification);
			AssertEquals("There should be an error", CargoWise.EntityFramework.NotificationType.Error, notification.Type);
			Assert("There should be specific error", notification.Message.Contains(ZDateTime.BrettsBirthday.Date.ToShortDateString()));

			testJob.ShouldUseImmediateRevenueRecognisedDate += new EventHandler<UserQueryEventArgs>(testJob_ShouldUseImmediateRevenueRecognisedDate);
			testJob.AskShouldUseImmediateRevenueRecognisedDate(testJob.Charges[0].ChargeCode);
			testJob.ShouldUseImmediateRevenueRecognisedDate -= new EventHandler<UserQueryEventArgs>(testJob_ShouldUseImmediateRevenueRecognisedDate);

			AssertHasNoValidationErrors(validator);
		}

		public void TestRunRevenueRecognitionDateValidation_ZeroCharges()
		{
			Create2MonthPeriod();

			var jobs = new List<Job>();
			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			testJob.Parent = shipment;
			jobs.Add(testJob);
			Charge charge = CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			testJob.LocalChargesPK = ZGuid.Empty;
			Factory.Save();

			RevenueRecognitionCollection settings = new RevenueRecognitionCollection();
			RevenueRecognition setting = settings.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Constants.TransportModes.Air;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, settings);
			testJob.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			string expectedErrorMessage = "Posting is prevented. The following information has not been recorded for this job. It is required for revenue recognition purposes and must be recorded before posting can occur: 'Delivery Date'";
			AssertEquals("There should be an error", expectedErrorMessage, validator.Validate().Message);

			charge.JR_LocalSellAmt = charge.JR_LocalCostAmt = 0m;
			Factory.Save();

			AssertHasNoValidationErrors(validator);
		}

		public void TestRunRevenueRecognitionDateValidation_ChargeRelatedOtherJobHaveError()
		{
			Create2MonthPeriod();

			var jobs = new List<Job>();
			var validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			var testJob1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			testJob1.Parent = shipment1;
			jobs.Add(testJob1);
			Factory.Save();

			var testJob2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			var declaration = TestObjectCreator.CreateDeclaration("B0001");
			testJob2.JH_ParentID = declaration.PK;
			CreateCharge(testJob2, CC2, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			RevenueRecognitionByChargeGroupCollection newValue = new RevenueRecognitionByChargeGroupCollection();
			RevenueRecognitionByChargeGroup copy = newValue.AddNew();
			copy.ChargeGroup = testJob2.Charges[0].ChargeCode.AC_ChargeGroup;
			RevenueRecognition setting = copy.ChargeGroupSettings.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

			AccountingConfigurationRegistry.Instance.RevenueRecognitionByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty, Guid.Empty, newValue);

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			testJob2.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			testJob1.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			Factory.Save();

			testJob1.Charges.IncludeChargesFromJobs(testJob2.PK);
			testJob1.Charges.Load();

			testJob1.Charges.Add(testJob2.Charges[0]);

			AssertEquals("Precondition", testJob2.Charges[0], testJob1.Charges[0]);
			AssertEquals("Precondition", testJob2.PK, testJob1.Charges[0].JR_JH);
			AssertEquals("Precondition", JobInvoicingConsumerTypes.Shipment, testJob1.JobType);
			AssertEquals("Precondition", JobInvoicingConsumerTypes.Brokerage, testJob2.JobType);

			var expectedErrorMessage = "You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.";
			AssertHasValidationError(expectedErrorMessage, validator);
		}

		public void TestRunRevenueRecognitionDateValidation_AllJobsExcludeLastOneMayHaveError()
		{
			Create2MonthPeriod();

			var jobs = new List<Job>();
			var validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			var testJob1 = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Sea;
			testJob1.Parent = shipment1;
			jobs.Add(testJob1);
			CreateCharge(testJob1, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			var testJob2 = CreateJob("Z00001001", LocalClient, 5M, Agent, 10M);
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			testJob2.Parent = shipment2;
			jobs.Add(testJob2);
			CreateCharge(testJob2, CC2, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			Factory.Save();

			shipment1.JS_RL_NKOrigin = "USLAX";
			shipment1.JS_RL_NKDestination = "AUSYD";
			shipment1.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Now.AddDays(10);
			shipment2.JS_RL_NKOrigin = "USLAX";
			shipment2.JS_RL_NKDestination = "AUSYD";
			shipment2.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Now.AddDays(10);
			Factory.Save();

			var settings = new RevenueRecognitionCollection();
			var setting = settings.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Constants.TransportModes.Air;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, settings);
			testJob1.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			testJob2.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			var expectedErrorMessage = "You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.";
			AssertHasValidationError(expectedErrorMessage, validator);
		}

		public void TestEmptyRevenueRecognitionWithContainerDetention()
		{
			SetupTest();

			var revenueRecognitionConfig = AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.Value;
			revenueRecognitionConfig.RemoveAll();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, revenueRecognitionConfig);

			var principal = Factory.NewWithValidTestData<OrgHeader>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var detention = Factory.New<ContainerDetention>();
			detention.NC_OH_Client = client.PK;
			detention.NC_OH_Principal = principal.PK;
			var plugin = (IJobInvoicingPlugIn)detention;

			var jobHeader = new JobHeader.Loader(detention).TryCreateWithMutex();
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var job = (Job)jobHeader;
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

			var charge = job.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = Debtor.PK;
			charge.JR_OSSellAmt = 100m;
			charge.JR_LocalSellAmt = 100;
			charge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			TestObjectCreator.CC1.RevenueRecOverrides.RemoveAndDeleteAll();
			Factory.Save();

			var jobs = new[] { job };
			var validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

			AssertEquals("Precondition", charge.CostRecognition, "");
			AssertEquals("Precodition", job.GetRevenueRecognitionType(TestObjectCreator.CC1), ZString.Empty);
			AssertEquals("Precodition", false, job.ConsumerTypeShouldCreateWIP(string.Empty));
			AssertEquals("Precodition", false, job.ConsumerTypeShouldCreateAccrual(string.Empty));

			var validationResult1 = validator.Validate();
			var expectedWarningMessage = "You have not setup Revenue Recognition for this job type. Go to Registry -> Accounting -> Job Invoicing -> Revenue Recognition Setup to configure Revenue Recognition.";
			AssertNotNull("PostManagerNotification", validationResult1);
			AssertEquals("Should be an Error", CargoWise.ComponentModel.NotificationType.Error, validationResult1.Type);
			AssertEquals("Error Message", expectedWarningMessage, validationResult1.Message);

			var chargeRevRecOverride = TestObjectCreator.CC1.RevenueRecOverrides.AddNew();
			chargeRevRecOverride.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			chargeRevRecOverride.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			Factory.Save();

			AssertEquals("Precondition", charge.CostRecognition, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate);
			AssertEquals("Precodition", job.GetRevenueRecognitionType(TestObjectCreator.CC1), RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate);
			AssertEquals("Precodition", false, job.ConsumerTypeShouldCreateWIP(string.Empty));
			AssertEquals("Precodition", false, job.ConsumerTypeShouldCreateAccrual(string.Empty));

			var validationResult2 = validator.Validate();
			AssertNull("PostManagerNotification", validationResult2);
		}

		public void TestRunChargesHaveCFXAccountValidation()
		{
			using (AccountingConfigurationRegistry.Instance.CFXAccount.DataType.SuspendValidation())
			{
				AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			}
			var jobs = new List<Job>();
			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			ExchangeRate rate1 = CreateExchangeRate(testJob, USD, .7M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			testJob.Parent = shipment;
			jobs.Add(testJob);
			Charge charge = CreateCharge(testJob, CC1, "Charge Code 1", USD, 100M, Creditor1, USD, 150M, LocalClient);
			Factory.Save();

			List<ZString> departmentList = new List<ZString>();
			GlbDepartment department = Factory.Load<GlbDepartment>(charge.JR_GE);
			departmentList.Add(department.GE_Code);
			string departments = "";
			foreach (ZString dept in departmentList)
			{
				departments += " - " + dept + "\n";
			}
			string expectedErrorMessage = string.Format("Posting cannot occur on job.  There are charge(s) with department(s) that do not have a CFX Account set in the registry.\r\n\r\nThe following departments do not have a CFX Account set:\r\n{0}\r\nEither set the system or department level CFX Account in the registry under Accounting -> General Ledger Defaults -> Link Account -> CFX Account.", departments);

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			AssertEquals("There should be an error", expectedErrorMessage, validator.RunChargesHaveCFXAccountValidation_ForTestOnly());
		}

		public void TestCheckJobsContainWarnings()
		{
			Create2MonthPeriod();

			OrgHeader localCharges = Factory.NewWithValidTestData<OrgHeader>();
			localCharges.OH_IsDebtor = true;
			localCharges.MiscServ.OM_ARCreditLimit = 100m;
			Factory.Save();

			Job otherJob = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"));

			ARInvoice aRInv = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.AUD, 1m, 90m, 9m, 90m, 9m);
			aRInv.AH_OH = localCharges.PK;
			aRInv.AH_PostDate = ZDateTime.Now;

			ARInvoiceLine line = (ARInvoiceLine)aRInv.Lines[0];
			line.AL_JH = otherJob.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			JobCharge jobCharge = TestObjectCreator.CreateCharge(line);
			jobCharge.JR_OH_SellAccount = localCharges.PK;

			Factory.Save();

			Job testJob = CreateJob("Z00001000", localCharges, 0M, Agent, 0M);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			testJob.Parent = shipment;

			Charge charge = CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, localCharges);
			Factory.Save();

			Job unrecognisedJob = CreateJob("Z00001001", localCharges, 0M, Agent, 0M);
			JobCharge unrecognisedCharge = Factory.NewWithValidTestData<JobCharge>();
			unrecognisedCharge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			unrecognisedCharge.JR_JH = unrecognisedJob.PK;
			unrecognisedCharge.JR_GB = unrecognisedJob.JH_GB;
			unrecognisedCharge.JR_GE = unrecognisedJob.JH_GE;
			unrecognisedCharge.JR_OSSellAmt = 15m;
			unrecognisedCharge.JR_LocalSellAmt = 15m;
			unrecognisedCharge.JR_OH_SellAccount = localCharges.PK;

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Factory.Save();
			Assert("Unrecognised", unrecognisedCharge.JR_AL_ARLine.IsEmpty);

			localCharges.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();

			var jobs = new[] { testJob };
			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

			AssertEquals("Default Registry Value", Constants.CreditLimitChecking.Posted, AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.Value);
			string expectedWarningMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 100.00 AUD. Credit approved.
The Total Outstanding Balance is 264.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 99.00 AUD
  *  the Current Transaction Amount, 165.00 AUD";
			INotification validationResult = validator.Validate();
			AssertNotNull(validationResult);
			AssertEquals("Should be warning", CargoWise.ComponentModel.NotificationType.Warning, validationResult.Type);
			AssertEquals("Warning Message", expectedWarningMessage, validator.Validate().Message);

			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedAndRecognized);
			expectedWarningMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 100.00 AUD. Credit approved.
The Total Outstanding Balance is 249.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 99.00 AUD
  *  the Unposted Recognized Revenue, 150.00 AUD";
			validationResult = validator.Validate();
			AssertNotNull(validationResult);
			AssertEquals("Should be warning", CargoWise.ComponentModel.NotificationType.Warning, validationResult.Type);
			AssertEquals("Warning Message", expectedWarningMessage, validator.Validate().Message);

			AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized);
			expectedWarningMessage = @"The Credit Limit for XVBQP68SIYXQ is set to 100.00 AUD. Credit approved.
The Total Outstanding Balance is 264.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 99.00 AUD
  *  the Unposted Recognized Revenue, 150.00 AUD
  *  the Unposted Unrecognized Revenue, 15.00 AUD";
			validationResult = validator.Validate();
			AssertNotNull(validationResult);
			AssertEquals("Should be warning", CargoWise.ComponentModel.NotificationType.Warning, validationResult.Type);
			AssertEquals("Warning Message", expectedWarningMessage, validator.Validate().Message);
		}

		public void TestRunPaymentTypeForUnapprovedInvoicesValidation_ForCharge()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var upTo = valuesForTest.AddNew();
			upTo.Amount = 250M;
			upTo.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo.Range = RangeCodes.UpTo;
			var above = valuesForTest.AddNew();
			above.Amount = 250M;
			above.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			above.Range = RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;

			Job job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			Charge charge = CreateCharge(job, CC1, "Charge Code 1", AUD, 150M, Creditor1, AUD, 150M, LocalClient);
			charge.JR_APInvoiceNum = "INV";
			charge.JR_APInvoiceDate = ZDateTime.Now;
			charge.JR_PaymentDate = ZDateTime.Now;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.EFT;
			charge.JR_AB = AUDBankAccount.PK;
			charge.JR_ChequeNo = "32423";
			charge = CreateCharge(job, CC5, "Charge Code 5", AUD, 150M, Creditor1, AUD, 150M, LocalClient);
			charge.JR_APInvoiceNum = "INV";
			charge.JR_APInvoiceDate = ZDateTime.Now;
			charge.JR_PaymentDate = ZDateTime.Now;
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.EFT;
			charge.JR_AB = AUDBankAccount.PK;
			charge.JR_ChequeNo = "32423";

			Factory.Save();

			var jobs = new[] { job };
			AssertHasValidationError(ChargeWithCostValidation.UnapprovedInvoicesWithPaymentWarningMessage, NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs));
			AssertHasNoValidationErrors(NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue));
		}

		public void TestRunPaymentTypeForUnapprovedInvoicesValidation_ForConsolCost()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var upTo = valuesForTest.AddNew();
			upTo.Amount = 250M;
			upTo.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			upTo.Range = RangeCodes.UpTo;
			var above = valuesForTest.AddNew();
			above.Amount = 250M;
			above.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			above.Range = RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.UpdateApportionmentChargesListing();
			cost.E6_AC_ChargeCode = CC1.PK;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 251m;
			cost.E6_OH_Creditor = Creditor1.PK;
			cost.E6_InvoiceNum = "ABC123";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			cost.E6_PaymentType = ZArchitecture.Core.ReceiptTypes.EFT;
			cost.E6_AB_BankAccount = AUDBankAccount.PK;
			cost.E6_ChequeOrReference = "32423";

			Factory.Save();

			Job job = cost.ApportionmentCharges[0].InvoicingJob;
			job.JH_OA_AgentCollectAddr = Agent.MainAddress.PK;
			job.JH_OA_LocalChargesAddr = Creditor1.MainAddress.PK;
			job.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			Job job2 = cost.ApportionmentCharges[1].InvoicingJob;
			job2.JH_OA_AgentCollectAddr = Agent.MainAddress.PK;
			job2.JH_OA_LocalChargesAddr = Creditor1.MainAddress.PK;
			job2.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var jobs = new[] { job, job2 };

			AssertHasValidationError(ForwardingConsolCostingValidation.UnapprovedInvoicesWithPaymentWarningMessage, NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs));
			AssertHasNoValidationErrors(NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue));
		}

		public void TestRunStampDutyValidation()
		{
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);

				var shipment = TestObjectCreator.CreateShipment("S00001000");
				var job = TestObjectCreator.CreateJob(shipment, false);

				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;

				OrgHeader orgHeader = TestObjectCreator.CreateOrgHeader("ROMORGROM", false, true, "ITROM");
				AccTaxRate taxRate1 = TestObjectCreator.CreateTaxRate("ART2", "Italian tax ART2", 10);
				AccTaxRate taxRate2 = TestObjectCreator.CreateTaxRate("ART9", "Italian tax ART9", 10);

				Factory.Save();

				string taxIDsAttractingStampDuty = taxRate1.PK.ToString() + "," + taxRate2.PK.ToString();
				AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxIDsAttractingStampDuty);

				Charge charge1 = job.Charges.AddNew();
				charge1.JR_AC = TestObjectCreator.CC1.PK;
				charge1.JR_LocalSellAmt = 70.00m;
				charge1.JR_OH_SellAccount = orgHeader.PK;
				charge1.JR_AT_SellGSTRate = taxRate1.PK;
				Charge charge2 = job.Charges.AddNew();
				charge2.JR_AC = TestObjectCreator.CC1.PK;
				charge2.JR_LocalSellAmt = 7.48m;
				charge2.JR_OH_SellAccount = orgHeader.PK;
				charge2.JR_AT_SellGSTRate = taxRate2.PK;

				Factory.Save();

				var jobs = new[] { job };

				var validation = new PostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
				var expectedError = "The invoice being posted attracts stamp duty, but there is no 'Stamp Duty Charge Code' defined in the registry. Please define an appropriate charge code in the registry under Accounting > Receivable Defaults > Default Settings > Stamp Duty Charge Code.";
				AssertEquals(expectedError, validation.RunStampDutyValidation_ForTestOnly());

				AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, TestObjectCreator.CC2.PK.ToGuid());
				AssertEquals(string.Empty, validation.RunStampDutyValidation_ForTestOnly());
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
				AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		public void TestIsRevenueBeingPosted()
		{
			var jobs = new List<Job>();
			var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

			AssertEquals(true, validation.IsRevenueBeingPosted_ForTestOnly(JobInvoicingPostingOption.All));
			AssertEquals(true, validation.IsRevenueBeingPosted_ForTestOnly(JobInvoicingPostingOption.LocalClient));
			AssertEquals(true, validation.IsRevenueBeingPosted_ForTestOnly(JobInvoicingPostingOption.Agent));
			AssertEquals(true, validation.IsRevenueBeingPosted_ForTestOnly(JobInvoicingPostingOption.Gateway));
			AssertEquals(true, validation.IsRevenueBeingPosted_ForTestOnly(JobInvoicingPostingOption.Revenue));
			AssertEquals(false, validation.IsRevenueBeingPosted_ForTestOnly(JobInvoicingPostingOption.Costs));
			AssertEquals(true, validation.IsRevenueBeingPosted_ForTestOnly(JobInvoicingPostingOption.Disbursement));
			AssertEquals(false, validation.IsRevenueBeingPosted_ForTestOnly(JobInvoicingPostingOption.CustomsDSBChargeAPOnly));
			AssertEquals(true, validation.IsRevenueBeingPosted_ForTestOnly(JobInvoicingPostingOption.CustomsDSBChargeAROnly));
		}

		public virtual void TestDifferencesInReloadedChargesValidation()
		{
			var otherUserFactory = new BusinessObjectFactory() { RefreshEnabled = false }; // Factory for interfering user who will add additional charge(s) just before our main Factory attempts to post
			var transactionFactory = new BusinessObjectFactory(); // Factory emulating what a consumer of this class should do, i.e. reload the jobs from DB just before posting.

			SetupInvoiceStyles(LocalClient, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			LocalClient.Factory.Save();

			TestCaseHelper.ClearTable("AccPeriodManagement");
			Create2MonthPeriod();
			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			var originalJobs = new[] { testJob };
			Factory.Save();
			CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			testJob.LocalChargesPK = ZGuid.Empty;
			Factory.Save();

			PostManagerValidation validator = NewPostManagerValidation(originalJobs, JobInvoicingPostingOption.All);
			AssertHasNoValidationErrors(validator);

			Job testJobLoadedByOtherUser = otherUserFactory.Load<Job>(testJob.PK);
			CreateCharge(testJobLoadedByOtherUser, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			otherUserFactory.Save();

			Job jobReloadedForPosting = transactionFactory.Load<Job>(testJob.PK);
			var jobs = new[] { jobReloadedForPosting };

			validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs, originalJobs);
			AssertHasValidationError(@"This job cannot be posted because changes have been made by another user since you have saved. These changes have been reloaded.", validator);
		}

		public void TestNo_DifferencesInReloadedChargesValidation_ErrorIfInMemoryChargesGetCreatedByGUI()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "AUMEL", "C0010010");

			var shipment1 = TestObjectCreator.CreateShipment("S010110", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S010111", consol);

			Job.Loader loader = new Job.Loader(Factory, shipment1);

			Job shipmentJob1 = loader.TryLoadOrCreateWithMutex();
			CreateCharge(shipmentJob1, CC1, "Charge Code 1", AUD, 10M, Creditor1, AUD, 10M, LocalClient);

			loader = new Job.Loader(Factory, shipment2);

			Job shipmentJob2 = loader.TryLoadOrCreateWithMutex();
			CreateCharge(shipmentJob2, CC1, "Charge Code 1", AUD, 0M, Creditor1, AUD, 0M, LocalClient);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 10m);

			Create2MonthPeriod();

			Factory.Save();

			var firstCharge = consolCost.ApportionmentCharges[0];
			firstCharge.JR_OSCostAmt = 10m; //sets secondCharge.JR_IsUsedForApportionment = true

			var secondCharge = consolCost.ApportionmentCharges[1];
			secondCharge.JR_OSCostAmt = 0m; //sets secondCharge.JR_IsUsedForApportionment = false

			Factory.Save();

			ApportionmentListing listing = new ApportionmentListing(Factory, consol);
			listing.PrepareForConsolCosting();

			JobCollection jobs = new JobCollection(Factory);
			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				loader = new Job.Loader(Factory, shipment);
				Job jobToAdd = loader.TryLoadOrCreateWithMutex();
				jobs.Add(jobToAdd);
			}

			JobCollection originalJobs = new JobCollection(Factory);
			jobs = LoadJobsInNewFactory(jobs, originalJobs);

			PostManagerValidation validator = NewPostManagerValidation(jobs.Cast<Job>(), JobInvoicingPostingOption.Costs, originalJobs.Cast<Job>());
			string resultString = validator.RunDifferencesInReloadedChargesValidation_ForTestOnly();
			Assert("No validation error should occur", string.IsNullOrEmpty(resultString));
		}

		public void TestCustomsDisbursementChargesDuplicateCheckValidation_NormalJob()
		{
			var job = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			AssertCustomsDisbursementChargesDuplicateCheckValidation(job, "There are multiple customs disbursement charges with the same charge code: ZZCC1");
		}

		public void TestCustomsDisbursementChargesDuplicateCheckValidation_GatewayJob()
		{
			var consol = TestObjectCreator.CreateGatewayConsol(consolNum: "C55555", receivingGatewayCompany: GlbCompany.CurrentCompany);
			TestObjectCreator.CreateShipment("S12345", consol);
			var job = TestObjectCreator.CreateJob(consol, false);
			AssertCustomsDisbursementChargesDuplicateCheckValidation(job, "There are multiple customs disbursement charges with the same charge code for the same Related Job Number: ZZCC1, {0}");
		}

		public void TestRunCompanyAndOrgsRegistrationNumbersValidation()
		{
			SetupTest();
			SetupInvoiceStyles(LocalClient, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			LocalClient.Factory.Save();

			TestCaseHelper.ClearTable("AccPeriodManagement");
			Create2MonthPeriod();
			Job testJob = CreateJob("Z00001000", LocalClient, 5M, Agent, 10M);
			testJob.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			testJob.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
			var jobs = new[] { testJob };
			Factory.Save();
			var charge1 = CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			var charge2 = CreateCharge(testJob, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			AssertNotNull(charge1.SellAccount);
			AssertNotNull(charge2.SellAccount);
			Creditor1.PrimaryRegistrationNumber.Number = "";
			LocalClient.PrimaryRegistrationNumber.Number = "";
			Factory.Save();

			PostManagerValidation validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				GlbCompany.CurrentCompany.GC_BusinessRegNo = ZString.Empty;
				AssertHasNoValidationErrors(validator);
				AssertHasNoValidationWarnings(validator);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				GlbCompany.CurrentCompany.GC_BusinessRegNo = ZString.Empty;
				validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
				AssertHasValidationWarning(@"The transaction is missing information that is mandatory for your country/region reporting:
Tax Registration Number of the Login Company [EDI]
Tax Registration Number of the Debtor [ZLOCCLT]", validator, PostManagerValidationType.MissingTaxRegistrationNumberValidation);

				GlbCompany.CurrentCompany.GC_BusinessRegNo = "ABC12345";
				AssertHasValidationWarning(@"The transaction is missing information that is mandatory for your country/region reporting:
Tax Registration Number of the Debtor [ZLOCCLT]", validator, PostManagerValidationType.MissingTaxRegistrationNumberValidation);

				LocalClient.PrimaryRegistrationNumber.Number = "xyz8888";
				AssertHasNoValidationWarnings(validator);
			}
		}

		public void TestBranchDepartmentCombinationsValidation_PostManagerValidation()
		{
			SetupTest();

			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge", TestObjectCreator.AUD, 100, Creditor, "INV11", TestObjectCreator.AUD, 120, Debtor);

			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
			var aaaDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			AssertNotEquals(job.JH_GB, charge.Branch);
			Factory.Save();

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			var jobs = new[] { job };
			var validatorCost = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			var validatorRevenue = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);

			//ALL allowed
			GlbBranch.CurrentBranch.AllowedDepartments.DeleteAll();
			AssertHasNoValidationErrors(validatorCost);
			AssertHasNoValidationErrors(validatorRevenue);

			//valid combination
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(charge.Branch, new GlbDepartment[] { charge.Department });
			AssertHasNoValidationErrors(validatorCost);
			AssertHasNoValidationErrors(validatorRevenue);

			//invalid combination
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(charge.Branch, new GlbDepartment[] { aaaDepartment });
			var expectedError = string.Format(@"This job cannot be posted. The department {1} cannot be used with the branch {0}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.", charge.Branch.GB_Code, charge.Department.GE_Code);
			AssertHasValidationError(expectedError, validatorCost);
			AssertHasValidationError(expectedError, validatorRevenue);
		}

		[TestDate(2020, 7, 24)]
		public virtual void TestRunExchangeRateValidation_ValidateErrorNotificationTakePrecedenceOverWarningNotification()
		{
			using (AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.CreditLimitChecking.Posted))
			{
				ExchangeRateReader.GetReaderInstance().ClearCache();
				SetupTest();

				Debtor.CompanyData.OB_ARWHTApplicable = false;
				Debtor.CompanyData.OB_ARCreditLimit = 1m;
				Debtor.CompanyData.OB_AROnCreditHold = true;
				Creditor.CompanyData.AccAPExchangeRateConfigurations.RemoveAndDeleteAll();
				Creditor.CompanyData.AccAPExchangeRateConfigurations.SetExRate("AP", "ALL", "ALL", "ALL", "SEL", "TDR", 0, false);
				AssertEquals("Precondition", 1, Creditor.CompanyData.AccAPExchangeRateConfigurations.Count);

				TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 100m, Debtor);

				var shipment = TestObjectCreator.CreateShipment("S001001");
				var job = TestObjectCreator.CreateJob(shipment, false);
				job.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
				job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
				TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge", TestObjectCreator.USD, 100, Creditor, "INV01", TestObjectCreator.USD, 100, Debtor);

				Factory.Save();

				job.RunPreSaveValidation();
				AssertNoErrors(job);

				var jobs = new[] { job };
				var postManagerValidation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);

				using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV"))
				{
					var expectedError = @"This job cannot be posted. The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the USD exchange rate is not set for the date 24-Jul-20. Please check your data and try again.";
					AssertHasValidationError(expectedError, postManagerValidation);

					TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.71m, ZDateTime.Today, ZDateTime.Today);
					AssertHasNoValidationErrors(postManagerValidation);
				}

				var expectedWarning = @"TESDEBBNE is on Credit Hold.

The Credit Limit for TESDEBBNE is set to 1.00 AUD. Credit approved.
The Total Outstanding Balance is 110.00 AUD, which is over the credit limit.

The Total Outstanding Balance is calculated by summing the following:
  *  the Posted Outstanding Balance, 0.00 AUD
  *  the Current Transaction Amount, 110.00 AUD";
				AssertHasValidationWarning(expectedWarning, postManagerValidation, PostManagerValidationType.CreditLimitValidation);

				ExchangeRateReader.GetReaderInstance().ClearCache();
			}
		}

		[TestDate(2015, 5, 1)]
		public virtual void TestRunExchangeRateValidation_PostManagerValidation()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			SetupTest();

			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge", TestObjectCreator.USD, 100, Creditor, "INV11", TestObjectCreator.USD, 100, Debtor);
			job.ExchangeRates[0].JF_BaseRate = 1.1234m;
			job.ExchangeRates[1].JF_BaseRate = 1.1234m;

			Factory.Save();
			Assert(!charge.BillInInvoiceCurrency);

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			var jobs = new[] { job };
			var validatorCost = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			var validatorRevenue = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			var expectedErrorRev = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
			AssertHasNoValidationErrors(validatorCost);
			AssertHasValidationError(expectedErrorRev, validatorRevenue);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			var expectedErrorCost = string.Format(@"This job cannot be posted. The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
			expectedErrorRev = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
			AssertHasValidationError(expectedErrorCost, validatorCost);
			AssertHasValidationError(expectedErrorRev, validatorRevenue);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			expectedErrorCost = string.Format(@"This job cannot be posted. The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
			expectedErrorRev = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
			AssertHasValidationError(expectedErrorCost, validatorCost);
			AssertHasValidationError(expectedErrorRev, validatorRevenue);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			expectedErrorCost = string.Format(@"This job cannot be posted. The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
			AssertHasValidationError(expectedErrorCost, validatorCost);
			AssertHasNoValidationErrors(validatorRevenue);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.CNY.RX_Code;
			Factory.Save();
			Assert(charge.BillInInvoiceCurrency);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			AssertHasNoValidationErrors(validatorRevenue);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			expectedErrorRev = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the CNY exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
			AssertHasValidationError(expectedErrorRev, validatorRevenue);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			expectedErrorRev = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the CNY exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
			AssertHasValidationError(expectedErrorRev, validatorRevenue);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			expectedErrorRev = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the CNY exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
			AssertHasValidationError(expectedErrorRev, validatorRevenue);
			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 1)]
		public virtual void TestRunExchangeRateValidation_ARPostingExRateRegistry()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			SetupTest();

			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge", TestObjectCreator.USD, 100, Creditor, "INV11", TestObjectCreator.USD, 100, Debtor);
			job.ExchangeRates[0].JF_BaseRate = 1.1234m;
			job.ExchangeRates[1].JF_BaseRate = 1.1234m;

			Factory.Save();
			Assert(!charge.BillInInvoiceCurrency);

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			var jobs = new[] { job };
			var validatorCost = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			var validatorRevenue = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);

			var foreignInvoicePostingExRateOption = ExRateOption.ExchangeRateBasedOnPostDate;
			var localInvoicePostingExRateOption = ExRateOption.ExchangeRateBasedOnInvoiceDate;
			var localCurrency = charge.Company.GC_RX_NKLocalCurrency;
			var foreignCurrency = TestObjectCreator.USD.RX_Code;

			var collectionAR = new InvoicePostingExRateOptionCollection();
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, foreignInvoicePostingExRateOption.Code, 0));
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, localInvoicePostingExRateOption.Code, 0));
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);

			AssertHasValidationError(GetErrorMessage(localInvoicePostingExRateOption.Description), validatorRevenue);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellCurrency = localCurrency;
			charge.JR_RX_NKSellInvoiceCurrency = foreignCurrency;
			Factory.Save();
			AssertNotEquals(localCurrency, foreignCurrency);
			//if charge sell invoice currency is foreign, then we get the registry option where currency type is foreign
			AssertHasValidationError(GetErrorMessage(foreignInvoicePostingExRateOption.Description), validatorRevenue);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FreightInvoice;
			charge.JR_RX_NKSellCurrency = foreignCurrency;
			charge.JR_RX_NKSellInvoiceCurrency = localCurrency;
			Factory.Save();
			//if charge sell invoice currency is local, then we get the registry option where currency type is local
			AssertHasValidationError(GetErrorMessage(localInvoicePostingExRateOption.Description), validatorRevenue);

			charge.JR_RX_NKSellInvoiceCurrency = string.Empty;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			Factory.Save();
			//if charge sell invoice currency is empty, the registry currency type depends on the charge invoice type
			AssertHasValidationError(GetErrorMessage(localInvoicePostingExRateOption.Description), validatorRevenue);

			//charge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Factory.Save();
			//if charge sell invoice currency is empty, the registry currency type depends on the charge invoice type
			AssertHasValidationError(GetErrorMessage(foreignInvoicePostingExRateOption.Description), validatorRevenue);

			ExchangeRateReader.GetReaderInstance().ClearCache();

			string GetErrorMessage(string exRateOptionDescription)
				=> string.Format($@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""{exRateOptionDescription}"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.");
		}

		[TestDate(2015, 5, 1)]
		public virtual void TestRunExchangeRateValidation_PostManagerValidation_ARExchangeRateConfiguration()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			SetupTest();

			var debtor = TestObjectCreator.ABIGAS;
			debtor.CompanyData.OB_IsDebtor = true;
			debtor.CompanyData.SetARTaxApplicable(true);
			debtor.CompanyData.OB_ARWHTApplicable = false;
			debtor.OH_FullName = "Test debtor";

			SetupInvoiceStyles(debtor, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			debtor.CompanyData.AccARExchangeRateConfigurations.RemoveAndDeleteAll();
			debtor.CompanyData.AccARExchangeRateConfigurations.SetExRate("AR", "ALL", "ALL", "ALL", "SEL", "TDR", 0, false);

			AssertEquals("Pre-condition", 1, debtor.CompanyData.AccARExchangeRateConfigurations.Count);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 6.73m, new DateTime(2015, 4, 1), new DateTime(2015, 6, 1));
			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge", TestObjectCreator.USD, 100, Creditor, "INV11", TestObjectCreator.USD, 100, debtor);

			Factory.Save();
			Assert(!charge.BillInInvoiceCurrency);

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			var jobs = new[] { job };
			var validatorRevenue = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Revenue);

			using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF"))
			{
				AssertHasNoValidationErrors(validatorRevenue);
			}

			using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD"))
			{
				var expectedError = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
				AssertHasValidationError(expectedError, validatorRevenue);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.71m, new ZDateTime(2015, 5, 1), new ZDateTime(2015, 5, 1));
				AssertHasNoValidationErrors(validatorRevenue);
				TestObjectCreator.USD.ExchangeRates.Cast<RefExchangeRate>().FirstOrDefault(x => x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate).Delete();
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}

			using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV"))
			{
				var expectedError = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
				AssertHasValidationError(expectedError, validatorRevenue);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.71m, new ZDateTime(2015, 5, 1), new ZDateTime(2015, 5, 1));
				AssertHasNoValidationErrors(validatorRevenue);
				TestObjectCreator.USD.ExchangeRates.Cast<RefExchangeRate>().FirstOrDefault(x => x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate).Delete();
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}

			using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST"))
			{
				var expectedError = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
				AssertHasValidationError(expectedError, validatorRevenue);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.71m, new ZDateTime(2015, 5, 1), new ZDateTime(2015, 5, 1));
				AssertHasNoValidationErrors(validatorRevenue);
				TestObjectCreator.USD.ExchangeRates.Cast<RefExchangeRate>().FirstOrDefault(x => x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate).Delete();
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.CNY.RX_Code;
			Factory.Save();
			Assert(charge.BillInInvoiceCurrency);

			using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF"))
			{
				AssertHasNoValidationErrors(validatorRevenue);
			}

			using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD"))
			{
				var expectedError = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the CNY exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
				AssertHasValidationError(expectedError, validatorRevenue);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, Core.Constants.ExchangeRateTypes.Code.SellRate, 5.1m, new ZDateTime(2015, 5, 1), new ZDateTime(2015, 5, 1));
				AssertHasNoValidationErrors(validatorRevenue);
				TestObjectCreator.CNY.ExchangeRates.Cast<RefExchangeRate>().FirstOrDefault(x => x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate).Delete();
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}

			using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV"))
			{
				var expectedError = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the CNY exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
				AssertHasValidationError(expectedError, validatorRevenue);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, Core.Constants.ExchangeRateTypes.Code.SellRate, 5.1m, new ZDateTime(2015, 5, 1), new ZDateTime(2015, 5, 1));
				AssertHasNoValidationErrors(validatorRevenue);
				TestObjectCreator.CNY.ExchangeRates.Cast<RefExchangeRate>().FirstOrDefault(x => x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate).Delete();
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}

			using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST"))
			{
				var expectedError = string.Format(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the CNY exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
				AssertHasValidationError(expectedError, validatorRevenue);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, Core.Constants.ExchangeRateTypes.Code.SellRate, 5.1m, new ZDateTime(2015, 5, 1), new ZDateTime(2015, 5, 1));
				AssertHasNoValidationErrors(validatorRevenue);
				TestObjectCreator.CNY.ExchangeRates.Cast<RefExchangeRate>().FirstOrDefault(x => x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate).Delete();
				Factory.Save();
			}

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 1)]
		public void TestRunExchangeRateValidation_PostManagerValidation_APExchangeRateConfiguration()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			SetupTest();

			var creditor = TestObjectCreator.ABIGAS;
			creditor.CompanyData.OB_IsCreditor = true;
			creditor.CompanyData.SetARTaxApplicable(true);
			creditor.CompanyData.OB_ARWHTApplicable = false;
			creditor.OH_FullName = "Test creditor";

			SetupInvoiceStyles(creditor, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			creditor.CompanyData.AccAPExchangeRateConfigurations.RemoveAndDeleteAll();
			creditor.CompanyData.AccAPExchangeRateConfigurations.SetExRate("AP", "ALL", "ALL", "ALL", "SEL", "TDR", 0, false);

			AssertEquals("Pre-condition", 1, creditor.CompanyData.AccAPExchangeRateConfigurations.Count);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 6.73m, new DateTime(2015, 4, 1), new DateTime(2015, 6, 1));
			var shipment = TestObjectCreator.CreateShipment("S0010001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test charge", TestObjectCreator.USD, 100, creditor, "INV11", TestObjectCreator.USD, 100, Debtor);

			Factory.Save();

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			var jobs = new[] { job };
			var validatorRevenue = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);

			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF"))
			{
				AssertHasNoValidationErrors(validatorRevenue);
			}

			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD"))
			{
				var expectedError = string.Format(@"This job cannot be posted. The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
				AssertHasValidationError(expectedError, validatorRevenue);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.71m, new ZDateTime(2015, 5, 1), new ZDateTime(2015, 5, 1));
				AssertHasNoValidationErrors(validatorRevenue);
				TestObjectCreator.USD.ExchangeRates.Cast<RefExchangeRate>().FirstOrDefault(x => x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate).Delete();
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}

			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV"))
			{
				var expectedError = string.Format(@"This job cannot be posted. The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
				AssertHasValidationError(expectedError, validatorRevenue);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.71m, new ZDateTime(2015, 5, 1), new ZDateTime(2015, 5, 1));
				AssertHasNoValidationErrors(validatorRevenue);
				TestObjectCreator.USD.ExchangeRates.Cast<RefExchangeRate>().FirstOrDefault(x => x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate).Delete();
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}

			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST"))
			{
				var expectedError = string.Format(@"This job cannot be posted. The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", charge.Branch.GB_Code, charge.Department.GE_Code);
				AssertHasValidationError(expectedError, validatorRevenue);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.71m, new ZDateTime(2015, 5, 1), new ZDateTime(2015, 5, 1));
				AssertHasNoValidationErrors(validatorRevenue);
				TestObjectCreator.USD.ExchangeRates.Cast<RefExchangeRate>().FirstOrDefault(x => x.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.SellRate).Delete();
				Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2021, 1, 20)]
		public void TestValidationErrorIfConsolCostInvoiceDateIsInconsistentWithApportionedCharge()
		{
			SetupTest();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C002001");
			TestObjectCreator.CreateShipment("S002001", consol);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100M, Creditor);

			Job job = cost.ApportionmentCharges[0].InvoicingJob;
			cost.ApportionmentCharges[0].JR_GE = TestObjectCreator.FIADepartment.PK;
			cost.ApportionmentCharges[0].JR_OH_SellAccount = Debtor.PK;
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			job.JH_GE = TestObjectCreator.FIADepartment.PK;
			job.JH_OA_AgentCollectAddr = Agent.MainAddress.PK;
			job.JH_OA_LocalChargesAddr = Creditor1.MainAddress.PK;

			Factory.Save();

			AssertNoErrors(job);
			var newfactory = Factory.CreateNewFactory();
			var charge = newfactory.Load<Charge>(cost.ApportionmentCharges[0].PK);
			using (new DisposableAction(() => SuspendCriticalValidationAttribute.IsActive = true, () => SuspendCriticalValidationAttribute.IsActive = false))
			using (charge.GetValidationSuspender())
			{
				charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(10);
				newfactory.Save();
			}

			AssertNotEquals("Pre-condition: Invoice Date should be different between charge and cost.", charge.JR_APInvoiceDate, cost.E6_InvoiceDate);

			var jobs = new[] { job };
			var validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			AssertHasValidationError("Charge invoice date and payment date must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.", validator);
		}

		[TestDate(2021, 1, 20)]
		public void TestValidationErrorIfConsolCostPaymentDateIsInconsistentWithApportionedCharge()
		{
			SetupTest();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C003001");
			TestObjectCreator.CreateShipment("S003001", consol);

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100M, Creditor);

			Job job = cost.ApportionmentCharges[0].InvoicingJob;
			cost.ApportionmentCharges[0].JR_GE = TestObjectCreator.FIADepartment.PK;
			cost.ApportionmentCharges[0].JR_OH_SellAccount = Debtor.PK;
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;
			job.JH_GE = TestObjectCreator.FIADepartment.PK;
			job.JH_OA_AgentCollectAddr = Agent.MainAddress.PK;
			job.JH_OA_LocalChargesAddr = Creditor1.MainAddress.PK;

			Factory.Save();

			AssertNoErrors(job);

			var newfactory = Factory.CreateNewFactory();
			var charge = newfactory.Load<Charge>(cost.ApportionmentCharges[0].PK);
			using (new DisposableAction(() => SuspendCriticalValidationAttribute.IsActive = true, () => SuspendCriticalValidationAttribute.IsActive = false))
			using (charge.GetValidationSuspender())
			{
				charge.JR_PaymentDate = ZDateTime.Now.AddDays(10);
				newfactory.Save();
			}

			AssertNotEquals("Pre-condition: Payment Date should be different between charge and cost.", charge.JR_PaymentDate, cost.E6_PaymentDate);

			var jobs = new[] { job };
			var validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.Costs);
			AssertHasValidationError("Charge invoice date and payment date must be equal to value on parent Consol Cost. Use Job Invoicing > Synchronize Cost Invoice Details menu on Consolidation > Consol Costing to fix data.", validator);
		}

		protected void testJob_ShouldUseImmediateRevenueRecognisedDate(object sender, UserQueryEventArgs e)
		{
			e.Response = true;
		}

		public void TestIsCostEligibleToPost()
		{
			var job = CreateJob("T00001000", LocalClient, 5M, Agent, 10M);
			var org = Factory.New<OrgHeader>();
			var charge = job.Charges.AddNew();
			charge.JR_OH_CostAccount = org.PK;
			charge.JR_APInvoiceDate = ZDateTime.Now;
			charge.JR_APInvoiceNum = "1";

			AssertIsCostEligibleToPost(job, charge, JobInvoicingPostingOption.All);

			AssertIsCostEligibleToPost(job, charge, JobInvoicingPostingOption.Costs);

			charge.JR_InvoiceType = AccTransactionHeader.DisbursementInvoiceTypes[0];
			charge.PostAPWhenInvokedByCustomInvoiceCreator = true;
			AssertIsCostEligibleToPost(job, charge, JobInvoicingPostingOption.CustomsDSBChargeAPOnly);

			charge.JR_E6 = ZGuid.NewZGuid();
			AssertIsCostEligibleToPost(job, charge, JobInvoicingPostingOption.ConsolCosts);
		}

		public void TestRunJobValidation_WhenJobOneFieldHasChanges_ReturnsChangedField()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var job = CreateJobForSavingValidation("A00001000");
			Factory.Save();

			PostManagerValidation validator = NewPostManagerValidation(new[] { job }, JobInvoicingPostingOption.All);

			Assert("Precondition : job has no changes.", !job.HasChanges);

			job.JH_Description = "ChangedDescription";

			var expectedError = $"Please save job {job.JH_JobNum} before posting costs and/or charges.\n\n----\n\n" +
				$"1 Job ({job.JH_JobNum}) have changed.\r\nThe following fields have changed: {nameof(JobHeader.Schema.JH_Description)}\r\n\n";

			AssertEquals(expectedError, validator.ValidateJobAndParentSavedOnly());

			Factory.Save();
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;

			expectedError = $"Please save job {job.JH_JobNum} before posting costs and/or charges.\n\n----\n\n" +
				$"1 Job ({job.JH_JobNum}) have changed.\r\nThe following fields have changed: {nameof(JobHeader.Schema.JH_OA_AgentCollectAddr)}\r\n\n";

			AssertEquals(expectedError, validator.ValidateJobAndParentSavedOnly());
		}

		public void TestRunJobValidation_WhenJobFieldsHasFewChanges_ReturnsChangedFields()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var job = CreateJobForSavingValidation("A00001000");
			Factory.Save();

			PostManagerValidation validator = NewPostManagerValidation(new[] { job }, JobInvoicingPostingOption.All);

			Assert("Precondition : job has no changes.", !job.HasChanges);
			
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;

			var expectedError = $"Please save job {job.JH_JobNum} before posting costs and/or charges.\n\n----\n\n" +
				$"1 Job ({job.JH_JobNum}) have changed.\r\nThe following fields have changed: {nameof(JobHeader.Schema.JH_GE)}, {nameof(JobHeader.Schema.JH_OA_AgentCollectAddr)}\r\n\n";

			AssertEquals(expectedError, validator.ValidateJobAndParentSavedOnly());
		}

		public void TestEDIMessageChanges_ReturnsChangedFields()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var job = CreateJobForSavingValidation("A00001000");
			var ediMessage = job.Factory.NewWithValidTestData<EDIMessage>();
			Factory.Save();

			var validator = NewPostManagerValidation(new[] { job }, JobInvoicingPostingOption.All);

			Assert("Precondition : job has no changes.", !job.HasChanges);

			job.JH_HoldReason = "UUU";
			ediMessage.EM_MessageText = "UUU";

			var expectedError = @$"1 Job (A00001000) have changed.
The following fields have changed: JH_HoldReason
1 EDI Message have changed.
The following fields have changed: EM_MessageText
";
			var actualError = "";
			AssertNoExceptionThrown("No exception if EDIMessage is changed", () => actualError = validator.ValidateJobAndParentSavedOnly());
			Assert("Changes of EDIMessage is contained", actualError.Contains(expectedError));
		}

		public void TestRunJobValidation_WhenDifferentFieldsOfDifferentRelateChildrenHasChanges_ReturnsChangedFields()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var job = CreateJobForSavingValidation("A00001000");
			Factory.Save();

			PostManagerValidation validator = NewPostManagerValidation(new[] { job }, JobInvoicingPostingOption.All);

			Assert("Precondition : job has no changes.", !job.HasChanges);
			Assert("Precondition : job charges have no changes.", !job.Charges.Any(c => c.HasChanges));
			Assert("Precondition : job exchanges rates have no changes.", !job.ExchangeRates.Any(c => c.HasChanges));

			job.Charges[0].JR_GE = ZGuid.Empty;
			job.Charges[0].JR_Desc = "Description1";
			job.Charges[1].JR_Desc = "Description1";
			job.Charges[1].JR_SellSupplyType = AccountingMasterFilesConstants.SupplyTypeClassificationCodes.LOC;
			job.Charges[1].JR_OSSellExRate = 0.9m;
			job.ExchangeRates[0].JF_BaseRate = 1.1234m;

			var expectedError = $"Please save job {job.JH_JobNum} before posting costs and/or charges.\n\n----\n\n2 JobCharge have changed.\r\n" +
				$"The following fields have changed: {nameof(JobCharge.Schema.JR_Desc)}, {nameof(JobCharge.Schema.JR_GE)}, {nameof(JobCharge.Schema.JR_LocalSellAmt)}, " +
				$"{nameof(JobCharge.Schema.JR_OSSellExRate)}, {nameof(JobCharge.Schema.JR_SellSupplyType)}\r\n" +
				$"1 JobExRate have changed.\r\nThe following fields have changed: {nameof(ExchangeRate.Schema.JF_BaseRate)}\r\n\n";

			AssertEquals(expectedError, validator.ValidateJobAndParentSavedOnly());
		}

		public void TestRunJobValidation_WhenTwoJobFieldsHaveChanges_ReturnsChangedField()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var job1 = CreateJobForSavingValidation("A00001000");
			var job2 = CreateJobForSavingValidation("A00001001");
			Factory.Save();

			PostManagerValidation validator = NewPostManagerValidation(new[] { job1, job2 }, JobInvoicingPostingOption.All);

			Assert("Precondition : job has no changes.", !job1.HasChanges);
			Assert("Precondition : job has no changes.", !job2.HasChanges);

			job1.JH_Description = "ChangedDescription";
			job2.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			var expectedError = $"Please save job {job1.JH_JobNum} before posting costs and/or charges.\n\n----\n\n2 Job ({job1.JH_JobNum}, {job2.JH_JobNum}) have changed.\r\n" +
				$"The following fields have changed: {nameof(JobHeader.Schema.JH_Description)}, {nameof(JobHeader.Schema.JH_OA_AgentCollectAddr)}\r\n\n" +
				$"Please save job {job2.JH_JobNum} before posting costs and/or charges.\n\n----\n\n2 Job ({job1.JH_JobNum}, {job2.JH_JobNum}) have changed.\r\n" +
				$"The following fields have changed: {nameof(JobHeader.Schema.JH_Description)}, {nameof(JobHeader.Schema.JH_OA_AgentCollectAddr)}\r\n\n";

			AssertEquals(expectedError, validator.ValidateJobAndParentSavedOnly());
		}

		public void TestNoRunJobsAreSaved_WhenBulkPosting()
		{
			SetupTest();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GE = TestObjectCreator.FISDepartment.PK;
			job.JH_OA_AgentCollectAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = Debtor.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var jobs = new[] { job };

			job.HasChanges = true;

			PostManagerValidation validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs, true);
			AssertHasNoValidationErrors(validation);
		}

		public void TestPostManagerValidationConstructor_IsBulkPostingValue()
		{
			using (Job job = CreateJobForSavingValidation("A00001000"))
			{
				var jobs = new[] { job };
				var validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs);
				Assert("IsBulkPosting is false", !validation.IsBulkPosting_ForTestOnly);

				validation = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All, jobs, true);
				Assert("IsBulkPosting is true", validation.IsBulkPosting_ForTestOnly);
			}
		}

		#region Setup

		OrgHeader Debtor;
		OrgHeader Debtor1;
		OrgHeader Creditor;
		OrgAddress LocalChargesAddress;
		OrgAddress AgentCollectAddress;
		GlbStaff TestUser;

		void SetupTest(bool shouldPrepareAccountPeriod = true)
		{
			if (shouldPrepareAccountPeriod)
			{
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
				testHelper.SetupPeriods();
			}

			Debtor = TestObjectCreator.ABIGAS;
			Debtor.CompanyData.OB_IsDebtor = true;
			Debtor.CompanyData.SetARTaxApplicable(true);
			Debtor.CompanyData.OB_ARWHTApplicable = false;
			Debtor.OH_FullName = "Test Debtor";

			Debtor1 = TestObjectCreator.Debtor;
			Debtor1.CompanyData.OB_IsDebtor = true;
			Debtor1.CompanyData.SetARTaxApplicable(true);
			Debtor1.CompanyData.OB_ARWHTApplicable = false;
			Debtor1.OH_FullName = "Test Debtor 1";

			Creditor = TestObjectCreator.AALSHI;
			Creditor.CompanyData.OB_IsCreditor = true;
			Creditor.CompanyData.SetAPTaxApplicable(true);
			Creditor.CompanyData.OB_ARWHTApplicable = false;
			Creditor.OH_FullName = "Test Creditor";

			SetupInvoiceStyles(Debtor, InvoicePostingOptionsList.Codes.DisbursementAndFinal);
			SetupInvoiceStyles(Creditor, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			LocalChargesAddress = Factory.NewWithValidTestData<OrgAddress>();
			AgentCollectAddress = Factory.NewWithValidTestData<OrgAddress>();

			Factory.Save();
		}

		void SetupSecurity()
		{
			TestUser = Factory.NewWithValidTestData<GlbStaff>();
			TestUser.GS_IsController = false;
			Factory.Save();

			var securityFactory = new BusinessObjectFactory();

			SecurityCore security = new UserLoginController().GetSecurityForUser(TestUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			var loginSecurity = securityFactory.New<GlbSecurity>();
			loginSecurity.GU_GB = TestObjectCreator.NonCurrentBranch.PK;
			loginSecurity.GU_GE = TestObjectCreator.NonCurrentDepartment.PK;
			loginSecurity.GU_GS = TestUser.PK;
			loginSecurity.GU_SecurityRight = security.Login.Code;
			loginSecurity.GU_SecurityItemIsAllowed = false;

			var invoicingSecurity = securityFactory.New<GlbSecurity>();
			invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
			invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
			invoicingSecurity.GU_GS = TestUser.PK;
			invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;
			invoicingSecurity.GU_SecurityItemIsAllowed = false;
			security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).IsAllowed = false;

			securityFactory.Save();
		}

		Job CreateJobForSavingValidation(string jubNumber)
		{
			var job = CreateJob(jubNumber, LocalClient, 5M, Agent, 10M);
			var charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			var charge2 = CreateCharge(job, CC1, "Charge Code 2", AUD, 100M, Debtor1, AUD, 150M, LocalClient);
			charge1.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			charge2.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
			charge2.JR_GE = TestObjectCreator.NonCurrentDepartment.PK;
			CreateExchangeRate(job, AUD, .7M);
			LocalChargesAddress = Factory.NewWithValidTestData<OrgAddress>();

			return job;
		}
		protected virtual void SetupInvoiceStyles(OrgHeader debtor, string invoicePostingOption)
		{
			OrgInvoiceRollupOrGroup group = debtor.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_InvoicePostingStyle = invoicePostingOption;
		}

		#endregion

		#region Assert

		protected virtual void AssertHasValidationError(string expectedError, PostManagerValidation validation)
		{
			PostManagerNotification validationResult = validation.Validate();
			AssertNotNull("Validation result should be not null", validationResult);
			AssertEquals("Should be an Error", CargoWise.ComponentModel.NotificationType.Error, validationResult.Type);
			AssertEquals(expectedError, validationResult.Message);
		}

		protected virtual void AssertHasValidationWarning(string expectedWarning, PostManagerValidation validation, PostManagerValidationType expectedType)
		{
			PostManagerNotification validationResult = validation.Validate();
			AssertNotNull("Validation result should be not null", validationResult);
			AssertEquals("Should be an Warning", CargoWise.ComponentModel.NotificationType.Warning, validationResult.Type);
			AssertEquals("Warning Type Returned Correctly", expectedType, validationResult.ValidationType);
			AssertEquals("Warning Message", expectedWarning, validationResult.Message);
		}

		protected virtual void AssertHasNoValidationErrors(PostManagerValidation validation)
		{
			INotification validationResult = validation.Validate();
			Assert("Should be no notification or no Error. Warning is allowed", validationResult == null || validationResult.Type != CargoWise.ComponentModel.NotificationType.Error);
		}

		protected virtual void AssertHasNoValidationWarnings(PostManagerValidation validation)
		{
			INotification validationResult = validation.Validate();
			Assert("Should be no notification or no warning. Error is allowed", validationResult == null || validationResult.Type != CargoWise.ComponentModel.NotificationType.Warning);
		}

		protected virtual void AssertIsCostEligibleToPost(Job job, Charge charge, JobInvoicingPostingOption postingOption)
		{
			var validator = NewPostManagerValidation(new Job[] { job }, postingOption);

			using (charge.Calculations.SuspendCalculations())
			{
				charge.JR_OSCostAmt = 100m;
				charge.JR_LocalCostAmt = 0m;
				AssertNotEquals("Pre-condition", 0m, charge.JR_OSCostAmt);
				AssertEquals("Pre-condition", 0m, charge.JR_LocalCostAmt);
				AssertEquals(false, validator.IsCostEligibleToPost_ForTestOnly(charge));

				charge.JR_OSCostAmt = 0m;
				charge.JR_LocalCostAmt = 100m;
				AssertEquals("Pre-condition", 0m, charge.JR_OSCostAmt);
				AssertNotEquals("Pre-condition", 0m, charge.JR_LocalCostAmt);
				AssertEquals(false, validator.IsCostEligibleToPost_ForTestOnly(charge));

				charge.JR_OSCostAmt = 0m;
				charge.JR_LocalCostAmt = 0m;
				AssertEquals("Pre-condition", 0m, charge.JR_OSCostAmt);
				AssertEquals("Pre-condition", 0m, charge.JR_LocalCostAmt);
				AssertEquals(false, validator.IsCostEligibleToPost_ForTestOnly(charge));

				charge.JR_OSCostAmt = 100m;
				charge.JR_LocalCostAmt = 100m;
				AssertNotEquals("Pre-condition", 0m, charge.JR_OSCostAmt);
				AssertNotEquals("Pre-condition", 0m, charge.JR_LocalCostAmt);
				AssertEquals(true, validator.IsCostEligibleToPost_ForTestOnly(charge));
			}
		}

		protected virtual void AssertNoChargesValidation(PostManagerValidation validator)
		{
			INotification validationResult = validator.Validate();
			AssertNotNull("Validation result should be not null", validationResult);
			AssertEquals("Should be an Error", CargoWise.ComponentModel.NotificationType.Error, validationResult.Type);
			AssertEquals("Should be invalid if no charges.", "Please enter charges before posting.", validationResult.Message);
		}

		void AssertCustomsDisbursementChargesDuplicateCheckValidation(Job job, string warningMessage)
		{
			SetupInvoiceStyles(LocalClient, InvoicePostingOptionsList.Codes.FinalInvoiceOnly);
			LocalClient.Factory.Save();

			TestCaseHelper.ClearTable("AccPeriodManagement");
			Create2MonthPeriod();
			var jobs = new[] { job };
			Factory.Save();
			var charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			var charge2 = CreateCharge(job, CC1, "Charge Code 1", AUD, 100M, Creditor1, AUD, 150M, LocalClient);
			job.LocalChargesPK = ZGuid.Empty;
			Factory.Save();

			var validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			AssertHasNoValidationErrors(validator);

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CC1.PK.ToGuid());

			validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
			AssertHasValidationWarning(string.Format(warningMessage, "<empty>"), validator, PostManagerValidationType.CustomsDisbursementChargesDuplicateCheckValidation);

			if (job.Parent?.IsGatewayBillingEnabled() ?? false)
			{
				charge1.JR_Calc_RelatedJobNumber = "S12345";
				Factory.Save();
				validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
				AssertHasNoValidationWarnings(validator);

				charge2.JR_Calc_RelatedJobNumber = "S12345";
				Factory.Save();
				validator = NewPostManagerValidation(jobs, JobInvoicingPostingOption.All);
				AssertHasValidationWarning(string.Format(warningMessage, "S12345"), validator, PostManagerValidationType.CustomsDisbursementChargesDuplicateCheckValidation);
			}
		}

		#endregion

		#region Implementation

		protected virtual bool ShouldErrorWhenInvoiceTypeIsEmpty => true;

		protected virtual string expectedErrorWhenRunJobChargeSupplyTypeValidation_ForApportionmentCharges => string.Empty;

		protected PostManagerValidation NewPostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption)
		{
			return NewPostManagerValidation(jobs, postingOption, jobs);
		}

		protected virtual PostManagerValidation NewPostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
		{
			return new PostManagerValidation(jobs, postingOption, originalJobs);
		}

		protected PostManagerValidation NewPostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs, bool isBulkPosting)
		{
			return new PostManagerValidation(jobs, postingOption, originalJobs, isBulkPosting);
		}

		protected virtual PostManagerValidation NewPostManagerValidation(IEnumerable<Job> jobs, IJobCostingPlugIn consol, JobInvoicingPostingOption postingOption)
		{
			return NewPostManagerValidation(jobs, postingOption);
		}

		protected virtual string DisbursementInvoiceType
		{
			get { return InvoiceTypesList.Codes.DisbursementInvoice; }
		}

		protected virtual string FinalInvoiceType
		{
			get { return InvoiceTypesList.Codes.FinalInvoice; }
		}

		//this method emulates Enterprise.Accounting.GUI.JobInvoicing.PostManagerGUIWrapper
		JobCollection LoadJobsInNewFactory(JobCollection jobs, JobCollection originalJobs)
		{
			JobCollection result = new JobCollection(new BusinessObjectFactory());
			ZQuery jobsQuery = new ZQuery();
			jobsQuery.DefaultJoinCondition = JoinCondition.Or;
			foreach (Job job in jobs)
			{
				if (!originalJobs.Contains(job))
				{
					job.Charges.AddNew(); //hack added to emulate the scenario that the GUI creates in memory charges when Is Used check box is played around with!!!
					originalJobs.Add(job);
				}
			}
			jobsQuery.AddToFilter(JobHeaderSchema.PK, jobs.GetPKs());

			if (jobs.Count == 0)
			{
				jobsQuery.IsNoResultQuery = true;
			}

			result.Load(jobsQuery);

			return result;
		}

		#endregion

		#region Create Business Objects

		Job CreateJobForTest()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.Parent = TestObjectCreator.GetTestShipmentPlugIn();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery(GlbDepartmentSchema.GE_Code, "FIS")).PK;
			job.JH_OA_LocalChargesAddr = LocalChargesAddress.PK;
			job.JH_OA_AgentCollectAddr = AgentCollectAddress.PK;
			return job;
		}

		#endregion
	}
}
