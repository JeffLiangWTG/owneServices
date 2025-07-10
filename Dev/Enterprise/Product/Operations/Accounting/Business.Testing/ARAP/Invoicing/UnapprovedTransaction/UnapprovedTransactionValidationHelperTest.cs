using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using NUnit.Framework;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class UnapprovedTransactionValidationHelperTest : LevelAuthorizationSecurityHelperTest<PaymentTwelveLevelAuthorisationSettingsRegistryItem, PaymentTwelveLevelAuthorisationSettingsCollection, PaymentTwelveLevelAuthorisationSettings>
	{
		[GuiTest]
		public void TestCheckSecurityLevelsAndPromptForAuthorisation()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			APInvoice invoice = Factory.New<APInvoice>();
			APInvoiceLine invoiceLine = invoice.Lines.AddNew() as APInvoiceLine;
			invoiceLine.AL_OSExTaxAmount = 100m;

			UnapprovedTransactionValidationHelper validation = new UnapprovedTransactionValidationHelper();
			validation.CheckSecurityLevelsAndPromptForAuthorisation(invoice);
			AssertNoRowErrors("No approval required as invoice is below 1000", invoice);

			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 1001m, Env.Security.APUnapprovedInvoicesFirstApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 2001m, Env.Security.APUnapprovedInvoicesSecondApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 3001m, Env.Security.APUnapprovedInvoicesThirdApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 4001m, Env.Security.APUnapprovedInvoicesFourthApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 5001m, Env.Security.APUnapprovedInvoicesFifthApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 6001m, Env.Security.APUnapprovedInvoicesSixthApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 7001m, Env.Security.APUnapprovedInvoicesSeventhApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 8001m, Env.Security.APUnapprovedInvoicesEighthApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 9001m, Env.Security.APUnapprovedInvoicesNinthApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 10001m, Env.Security.APUnapprovedInvoicesTenthApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 11001m, Env.Security.APUnapprovedInvoicesEleventhApproval);
			AssertCheckSecurityLevelsAndPromptForAuthorisation(invoice, invoiceLine, validation, 12001m, Env.Security.APUnapprovedInvoicesTwelfthApproval);
		}

		void AssertCheckSecurityLevelsAndPromptForAuthorisation(APInvoice invoice, APInvoiceLine invoiceLine, UnapprovedTransactionValidationHelper validation,
																ZDecimal amount, SecurityCheckpoint levelSecurityCheckPoint)
		{
			levelSecurityCheckPoint.IsAllowed = false;
			invoiceLine.AL_OSExTaxAmount = amount;
			validation.CheckSecurityLevelsAndPromptForAuthorisation(invoice);
			AssertHasRowError(invoice, "This transaction requires a higher level of approval authority.");
			levelSecurityCheckPoint.IsAllowed = true;
			validation.CheckSecurityLevelsAndPromptForAuthorisation(invoice);
			AssertNoRowErrors("No approval required as invoice amount is within current apporval limits", invoice);
		}

		public override void TestGetSecurityCheckPoint()
		{
			AssertSecurity(false, false);
			AssertSecurity(false, true);
			AssertSecurity(true, false);
			AssertSecurity(true, true);
		}

		void AssertSecurity(bool enableAPInvoiceApproval, bool interCompanyInvoiceApproval)
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableAPInvoiceApproval);
			bool shouldUseAPInvoiceApprovalSecurityCheckPoint = enableAPInvoiceApproval && !interCompanyInvoiceApproval;
			var testAmount = upTo1000.Amount;
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, -1000, Env.Security.None);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, 0, Env.Security.None);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount - 1, Env.Security.None);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount, Env.Security.None);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 1, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_FirstApproval : Env.Security.APUnapprovedInvoicesFirstApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 1000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_FirstApproval : Env.Security.APUnapprovedInvoicesFirstApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 1001, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_SecondApproval : Env.Security.APUnapprovedInvoicesSecondApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 2000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_SecondApproval : Env.Security.APUnapprovedInvoicesSecondApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 2001, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_ThirdApproval : Env.Security.APUnapprovedInvoicesThirdApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 3000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_ThirdApproval : Env.Security.APUnapprovedInvoicesThirdApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 4000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_FourthApproval : Env.Security.APUnapprovedInvoicesFourthApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 5000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_FifthApproval : Env.Security.APUnapprovedInvoicesFifthApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 6000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_SixthApproval : Env.Security.APUnapprovedInvoicesSixthApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 7000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_SeventhApproval : Env.Security.APUnapprovedInvoicesSeventhApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 8000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_EighthApproval : Env.Security.APUnapprovedInvoicesEighthApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 9000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_NinthApproval : Env.Security.APUnapprovedInvoicesNinthApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 10000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_TenthApproval : Env.Security.APUnapprovedInvoicesTenthApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 11000, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_EleventhApproval : Env.Security.APUnapprovedInvoicesEleventhApproval);
			AssertSecurityCheckPointForAmount(interCompanyInvoiceApproval, testAmount + 11001, shouldUseAPInvoiceApprovalSecurityCheckPoint ? Env.Security.APInvoiceApproval_TwelfthApproval : Env.Security.APUnapprovedInvoicesTwelfthApproval);
		}

		void AssertSecurityCheckPointForAmount(bool interCompanyInvoiceApproval, ZDecimal amount, SecurityCheckpoint expectedCheckPoint)
		{
			var testCheckpoint = new UnapprovedTransactionValidationHelper(interCompanyInvoiceApproval).GetSecurityCheckPoint(amount);
			AssertEquals(expectedCheckPoint.Code, testCheckpoint.Code);
		}

		#region Implementation

		public override PaymentTwelveLevelAuthorisationSettingsRegistryItem GetRegistryItem()
		{
			return AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings;
		}

		public override PaymentTwelveLevelAuthorisationSettingsCollection PrepareAuthorisationSettingsCollection()
		{
			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			upTo1000 = GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 1000, AuthorisationCodes.NoApprovalRequired);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 2000, AuthorisationCodes.FirstApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 3000, AuthorisationCodes.SecondApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 4000, AuthorisationCodes.ThirdApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 5000, AuthorisationCodes.FourthApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 6000, AuthorisationCodes.FifthApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 7000, AuthorisationCodes.SixthApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 8000, AuthorisationCodes.SeventhApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 9000, AuthorisationCodes.EighthApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 10000, AuthorisationCodes.NinthApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 11000, AuthorisationCodes.TenthApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.UpTo, 12000, AuthorisationCodes.EleventhApprovalRequiredOnly);
			GetNewAuthorisationSetting(valuesForTest, RangeCodes.Above, 12000, AuthorisationCodes.TwelfthApprovalRequiredOnly);
			return valuesForTest;
		}

		AmountBasedMultiLevelAuthorisationRequirement upTo1000;

		#endregion
	}
}