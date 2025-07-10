using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.GUI.Testing
{
	public class ARInvoiceReversingTest : TestCaseWithFactory
	{
		public void TestReversingPreventedWhenTwoApproversRequired()
		{
			var registry = new AuthorizationModeAndSettings();
			registry.AuthorizationMode = AuthorizationMode.Codes.TwoApprovers;
			var settings = registry.AuthorisationSettings;
			var settings1 = settings.AddNew();
			settings1.Range = RangeCodes.UpTo;
			settings1.Amount = 1;
			settings1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			var settings2 = settings.AddNew();
			settings2.Range = RangeCodes.Above;
			settings2.Amount = 1;
			settings2.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;

			AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);

			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var testObjectCreator = new TestObjectCreator(Factory);

			var org = testObjectCreator.AALSHI;
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.FillWithValidTestData();

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_OSExTaxAmount = 10m;
			arInvoice.AH_OH = org.PK;

			var line = (ARInvoiceLine)arInvoice.Lines.AddNew();
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_OSExTaxAmount = 10m;

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.ARCreditNote);
			using (var form = controller.ShowDeleteForm(arInvoice))
			{
				AssertEquals(typeof(LoginFormWithRequestAndNoCredentials), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(@"Credit notes of this value require approval by two authorized users.
Please queue a request for approval by pressing the 'Approval Request' button below.
If you are an authorized approver, your request will be queued for 2nd approval.", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);
			}
		}

		public void TestReversingPreventedWhenSequentialModeRequired()
		{
			var registry = new AuthorizationModeAndSettings();
			registry.AuthorizationMode = AuthorizationMode.Codes.SequentialApprovers;
			var settings = registry.AuthorisationSettings;
			var settings1 = settings.AddNew();
			settings1.Range = RangeCodes.UpTo;
			settings1.Amount = 1;
			settings1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			var settings2 = settings.AddNew();
			settings2.Range = RangeCodes.Above;
			settings2.Amount = 1;
			settings2.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;

			AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);

			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var testObjectCreator = new TestObjectCreator(Factory);

			var org = testObjectCreator.AALSHI;
			org.CompanyData.OB_IsDebtor = true;
			org.CompanyData.FillWithValidTestData();

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_OSExTaxAmount = 10m;
			arInvoice.AH_OH = org.PK;

			var line = (ARInvoiceLine)arInvoice.Lines.AddNew();
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_OSExTaxAmount = 10m;

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.ARCreditNote);
			using (var form = controller.ShowDeleteForm(arInvoice))
			{
				AssertEquals(typeof(LoginFormWithRequestAndNoCredentials), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(@"Credit notes of this value require approval by multiple authorized users.
Please queue a request for approval by pressing the 'Approval Request' button below.
If you are an authorized approver, your request will be queued for additional approvals.", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);
			}
		}

		public void TestReverseInSEQModeWithMultipleLines()
		{
			var registryForHeader = new AuthorizationModeAndSettings();
			registryForHeader.AuthorizationMode = AuthorizationMode.Codes.Default;
			var settings = registryForHeader.AuthorisationSettings;
			var settings1 = settings.AddNew();
			settings1.Range = RangeCodes.UpTo;
			settings1.Amount = 1000;
			settings1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			var settings2 = settings.AddNew();
			settings2.Range = RangeCodes.Above;
			settings2.Amount = 1000;
			settings2.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetValue(Guid.Empty, BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), registryForHeader);

			var registryForLine1 = new AuthorizationModeAndSettings();
			registryForLine1.AuthorizationMode = AuthorizationMode.Codes.TwoApprovers;
			settings = registryForLine1.AuthorisationSettings;
			settings1 = settings.AddNew();
			settings1.Range = RangeCodes.UpTo;
			settings1.Amount = 10;
			settings1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			settings2 = settings.AddNew();
			settings2.Range = RangeCodes.Above;
			settings2.Amount = 10;
			settings2.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetValue(Guid.Empty, BranchBNE.PK.ToGuid(), TestObjectCreator.FEADepartment.PK.ToGuid(), registryForLine1);

			var registryForLine2 = new AuthorizationModeAndSettings();
			registryForLine2.AuthorizationMode = AuthorizationMode.Codes.SequentialApprovers;
			settings = registryForLine2.AuthorisationSettings;
			settings1 = settings.AddNew();
			settings1.Range = RangeCodes.UpTo;
			settings1.Amount = 10;
			settings1.AuthorisationRequirement = AuthorisationCodes.NoApprovalRequired;
			settings2 = settings.AddNew();
			settings2.Range = RangeCodes.Above;
			settings2.Amount = 10;
			settings2.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetValue(Guid.Empty, BranchBNE.PK.ToGuid(), TestObjectCreator.FESDepartment.PK.ToGuid(), registryForLine2);

			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);

			using (Env.SetTemporaryUserContext("newuser", BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
			{
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), BranchBNE.PK.ToGuid(), staff.PK, false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FEADepartment.PK.ToGuid(), BranchBNE.PK.ToGuid(), staff.PK, false);
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FESDepartment.PK.ToGuid(), BranchBNE.PK.ToGuid(), staff.PK, false);

				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00001001", TestObjectCreator.LocalCurrency, 1, TestObjectCreator.Debtor);
				var arInvoiceLine1 = TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.FRT, TestObjectCreator.LocalCurrency, 1, "desc", 100);
				arInvoiceLine1.AL_GB = BranchBNE.PK;
				arInvoiceLine1.AL_GE = TestObjectCreator.FEADepartment.PK;
				var arInvoiceLine2 = TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.FRT, TestObjectCreator.LocalCurrency, 1, "desc", 100);
				arInvoiceLine1.AL_GB = BranchBNE.PK;
				arInvoiceLine2.AL_GE = TestObjectCreator.FESDepartment.PK;
				Factory.Save();

				var controller = ZControllerFactory.Create(ControllerIDs.ARCreditNote);
				using (var form = controller.ShowDeleteForm(arInvoice))
				{
					AssertEquals(typeof(LoginFormWithRequestAndNoCredentials), ZFormModaliser.LastFormShownDialogForTest.GetType());
					AssertEquals(@"Credit notes of this value require approval by multiple authorized users.
Please queue a request for approval by pressing the 'Approval Request' button below.
If you are an authorized approver, your request will be queued for additional approvals.", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);
				}
			}
		}

		GlbBranch BranchBNE => Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));

		protected TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;
	}
}
