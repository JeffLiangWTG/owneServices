using System;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.AccQueryClaims.Testing
{
	public class APAccQueryClaimValidationTest : AccQueryClaimBaseValidationTest
	{
		public void TestCheckAY_OH_Debtor_WarningIsShownExplainingWhyDebtorIsReadOnly()
		{
			APInvoice header = Factory.NewWithValidTestData<APInvoice>();
			OrgHeaderCollection creditors = GetOrganisationsCollection();
			creditors.Load();
			QueryClaim.AY_OH_Debtor = creditors[0].PK;
			QueryClaim.AY_AH = header.PK;
			QueryClaim.Validation.ValidateAY_OH_Debtor();
			AssertNoWarnings(QueryClaim.AY_OH_DebtorInfo);
			UACreditNote creditNote = QueryClaim.CreateAndAttachRelatedCreditNote();
			QueryClaim.Validation.ValidateAY_OH_Debtor();
			AssertHasWarning(QueryClaim.AY_OH_DebtorInfo, "Creditor cannot be changed after claim charges are allocated.");
		}

		public void TestCheckAY_QueryClaimAmountNotExceedsOriginalInvoiceAmount()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_OSTotal = 100m;
			QueryClaim.AY_AH = invoice.PK;
			AssertNoErrors(QueryClaim.AY_QueryClaimAmountInfo);
			QueryClaim.AY_QueryClaimAmount = 101m;
			AssertHasError(QueryClaim.AY_QueryClaimAmountInfo, "Amount Claimed cannot exceed the original invoice’s amount.");
		}

		public void TestCheckAY_HoldOptionValidation()
		{
			var claim = PrepareValidClaim();
			var validation = new APAccQueryClaimValidation(claim);
			var group = claim.Debtor.CompanyData.APCreditorGroup;

			var tmpSecurityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				claim.AY_HoldOption = HoldOptionType.Codes.ALM;

				Env.Security.PayablesClaimsAndQueriesHoldOption.IsAllowed = true;
				group.OG_IsAllowALM = true;
				validation.ValidateAY_HoldOption();

				AssertNoNotifications("Can change hold option when all granted", claim.AY_HoldOptionInfo);

				Env.Security.PayablesClaimsAndQueriesHoldOption.IsAllowed = true;
				group.OG_IsAllowALM = false;
				validation.ValidateAY_HoldOption();

				AssertNoNotifications("Can change hold option when security right is granted, despite of the group settings", claim.AY_HoldOptionInfo);

				Env.Security.PayablesClaimsAndQueriesHoldOption.IsAllowed = false;
				group.OG_IsAllowALM = true;
				validation.ValidateAY_HoldOption();

				AssertNoNotifications("Can change hold option when group settings is granted, despite of the security right", claim.AY_HoldOptionInfo);

				Env.Security.PayablesClaimsAndQueriesHoldOption.IsAllowed = false;
				group.OG_IsAllowALM = false;
				validation.ValidateAY_HoldOption();

				AssertEquals("Can NOT change hold option when both security right and group setting are denied", 1, claim.AY_HoldOptionInfo.GetErrors().Count());
				AssertHasError(claim.AY_HoldOptionInfo, "Unable to change Invoice Hold Option. Please review the Allowed Invoice Hold Options for the Creditor Group. Otherwise, ask your system administrator to adjust your security right: Manage > Payables > Claims and Queries > Modify Invoice Hold Option.");

				Factory.Save();
				claim.AY_HoldOption = HoldOptionType.Codes.DNM;

				Env.Security.PayablesClaimsAndQueriesHoldOption.IsAllowed = true;
				group.OG_IsAllowALM = true;
				validation.ValidateAY_HoldOption();
				AssertNoNotifications(claim.AY_HoldOptionInfo);

				Env.Security.PayablesClaimsAndQueriesHoldOption.IsAllowed = true;
				group.OG_IsAllowALM = false;
				validation.ValidateAY_HoldOption();
				AssertNoNotifications(claim.AY_HoldOptionInfo);

				Env.Security.PayablesClaimsAndQueriesHoldOption.IsAllowed = false;
				group.OG_IsAllowALM = true;
				validation.ValidateAY_HoldOption();
				AssertNoNotifications(claim.AY_HoldOptionInfo);

				Env.Security.PayablesClaimsAndQueriesHoldOption.IsAllowed = false;
				group.OG_IsAllowALM = false;
				validation.ValidateAY_HoldOption();
				AssertNoNotifications(claim.AY_HoldOptionInfo);
			}
		}

		// Precondition check of Hold Option system.
		public void TestOnlyAllowOneClaimPerAPTransaction()
		{
			var transaction = TestObjectCreator.CreateAPInvoice<APInvoice>("TST01", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.Creditor1);
			Factory.Save();

			var claim1 = PrepareValidClaim();
			claim1.AY_QueryClaimAmount = 10;
			claim1.AY_ShortDescriptionOfClaim = "TST";

			var claim2 = PrepareValidClaim(TestObjectCreator.Creditor2);
			claim2.AY_QueryClaimAmount = 10;
			claim2.AY_ShortDescriptionOfClaim = "TST";

			var validation1 = new APAccQueryClaimValidation(claim1);
			var validation2 = new APAccQueryClaimValidation(claim2);

			claim1.AY_AH = transaction.PK;
			validation1.ValidateAll();
			AssertNoNotifications(claim1);

			claim2.AY_AH = transaction.PK;
			validation2.ValidateAll();
			AssertEquals(1, claim2.GetErrors().Count());
			AssertHasError(claim2.AY_AHInfo, "Each transaction can only be associated with one claim.  This transaction cannot be chosen because it is already linked to a Claim.");
		}

		APAccQueryClaim fQueryClaim;
		APAccQueryClaim QueryClaim
		{
			get
			{
				if (fQueryClaim == null)
				{
					fQueryClaim = Factory.New<APAccQueryClaim>();
					fQueryClaim.AY_OH_Debtor = TestObjectCreator.ABIGAS.PK;
				}

				return fQueryClaim;
			}
		}

		protected override Type GetExpectedParentType()
		{
			return typeof(APAccQueryClaim);
		}

		protected override OrgHeaderCollection GetOrganisationsCollection()
		{
			return QueryClaim.Lookups.Creditors;
		}

		#region Implementation

		APAccQueryClaim PrepareValidClaim()
		{
			return PrepareValidClaim(TestObjectCreator.Creditor1);
		}

		APAccQueryClaim PrepareValidClaim(OrgHeader org)
		{
			TestObjectCreator.Creditor1.CompanyData.OB_OG_APCreditorGroup = TestObjectCreator.CreateCreditorGroup().PK;

			var claim = Factory.NewWithValidTestData<APAccQueryClaim>();
			claim.AY_OH_Debtor = org.PK;

			var contact = claim.Debtor.Contacts.AddNew();
			contact.OC_ContactName = "TST";
			claim.AY_OC = contact.PK;

			return claim;
		}

		#endregion
	}
}
