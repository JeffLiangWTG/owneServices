using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.Business.Accounting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class CASSBillingClaimCreator
	{
		public CASSBillingClaimCreator(CASSBillingLine line)
		{
			Argument.NotNull(line, "CASSBillingLine");
			billingLine = line;
		}
		readonly CASSBillingLine billingLine;

		public ZString GetClaimWarningMessage()
		{
			var msg = ZString.Empty;

			if (ClaimExistsAndNeedToNotifyUser)
			{
				msg = Res.GetString("f28c4cef-56f1-406a-b674-6442fe2b9e53", "Claim already exists for this MAWB and will be linked to this new AP invoice.");
			}
			else if (ClaimExistsAndNeedsToBeClosed)
			{
				msg = Res.GetString("fdc42197-78f4-4653-8c8d-c65fc682df61", "Claim already exists for this MAWB and will be closed as accepted, unless overridden using the option below.");
			}

			return msg;
		}

		public (bool result, string[] errorMessages) CreateOrUpdateClaim(InvoicingBase invoice, bool isOverrideAutoClose)
		{
			var result = true;
			APAccQueryClaim claim = null;
			if (NeedToCreateNewClaim)
			{
				claim = CreateClaim(invoice);
				result = (claim != null);
			}
			else if (ClaimExistsAndNeedToNotifyUser)
			{
				claim = GetTheOnlyOpenClaim(invoice.Factory);
				result = UpdateClaim(claim, invoice.PK);
			}
			else if (ClaimExistsAndNeedsToBeClosed)
			{
				claim = GetTheOnlyOpenClaim(invoice.Factory);
				result = UpdateClaimThatCanBeClosed(claim, invoice.PK, isOverrideAutoClose);
			}

			claim?.RunPreSaveValidation();
			return (result && (claim == null || !claim.HasErrors), GetClaimErrorMessages());

			string[] GetClaimErrorMessages() => claim?.Notifications.Where(n => n.Type == NotificationType.Error).Select(en => en.Message).ToArray() ?? Array.Empty<string>();
		}

		#region Functions

		APAccQueryClaim CreateClaim(InvoicingBase invoice)
		{
			var claimType = GetClaimType();
			if (!claimType.HasValue)
			{
				return null;
			}

			var claim = invoice.Factory.New<APAccQueryClaim>();
			claim.AY_QueryClaimType = claimType.Value;
			claim.AY_MasterBillNumber = billingLine.MAWBNumber;
			if (billingLine.Creditor != null)
			{
				claim.AY_OH_Debtor = billingLine.Creditor.PK;
				if (claim.Contact == null)
				{
					var firstActiveContract = billingLine.Creditor.ContactsActive.FirstOrDefault();
					if (firstActiveContract != null)
					{
						claim.AY_OC = firstActiveContract.PK;
					}
				}
			}

			claim.AY_AH = invoice.PK;
			claim.AY_QueryClaimAmount = AmountDifference;
			claim.AY_ShortDescriptionOfClaim = Res.GetString("eea5c9ac-4a29-4faf-b05e-074c919acf48", "CASS Import Billing Discrepancy");
			claim.AY_HoldOption = HoldOptionType.Codes.ALM;
			claim.AddToLog(GetClaimStatusText() +
				System.Environment.NewLine +
				GetCASSBillingLineInfoAsText());

			return claim;
		}

		bool UpdateClaimThatCanBeClosed(APAccQueryClaim claim, ZGuid invoicePK, bool isOverrideAutoClose)
		{
			var result = false;
			if (claim != null)
			{
				claim.AY_AH = invoicePK;

				if (isOverrideAutoClose)
				{
					claim.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus8AcceptedNotClosed;
					claim.AddToLog(Res.GetString("c385dc0c-7c4d-489c-9737-efd706b7ffd6", "MAWB billing amended by DCR/CCR records in the CASS file and billing discrepancy is addressed. Related claim can now be closed.") +
						System.Environment.NewLine +
						GetCASSBillingLineInfoAsText());
				}
				else
				{
					claim.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus9AcceptedClosed;
					claim.AddToLog(Res.GetString("93f3bae6-ac29-4817-a7e0-c6db52cf9dc6", "MAWB billing amended by DCR/CCR records in the CASS file and billing discrepancy is addressed. Related claim is now closed.") +
						System.Environment.NewLine +
						GetCASSBillingLineInfoAsText());
				}

				result = true;
			}

			return result;
		}

		bool UpdateClaim(APAccQueryClaim claim, ZGuid invoicePK)
		{
			var result = false;
			if (claim != null)
			{
				claim.AY_AH = invoicePK;
				claim.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus7RejectedWithDCRCCRNotClosed;
				claim.AY_QueryClaimAmount -= AmountDifference;
				claim.AddToLog(Res.GetString("ba3adbae-0817-46e2-8990-03ed9ec22973", "MAWB billing amended by DCR/CCR records in the CASS file. However, billing discrepancy still exists and claim record is now linked to the latest billing.") +
						System.Environment.NewLine +
						GetCASSBillingLineInfoAsText());

				result = true;
			}

			return result;
		}

		ZString GetClaimStatusText()
		{
			var createClaimAppendLog = ZString.Empty;

			if (billingLine.Status == CASSBillingLine.StatusType.NotInSystem)
			{
				createClaimAppendLog = Res.GetString("c8e44092-03c6-4457-a20c-94090e19765c", "Claim Created for MAWB {0} appearing in the CASS billing file, but is missing in {1}.", billingLine.MAWBNumber, Core.Constants.ProductName);
			}
			else
			{
				var status = ZString.Empty;
				if (billingLine.Status == CASSBillingLine.StatusType.Underbilled)
				{
					status = Res.GetString("04bf7c36-2582-44e2-b8d3-1f4274df8ce2", "under-billing");
				}
				else if (billingLine.Status == CASSBillingLine.StatusType.Overbilled)
				{
					status = Res.GetString("9825d376-d801-49c2-9844-b0f6de4dfd00", "over-billing");
				}

				createClaimAppendLog = Res.GetString("b3549dd3-b916-416c-9afc-9e47de9aca03", "Claim Created for CASS {0} of charges on {1}. The variance amount is {2} {3}", status, billingLine.MAWBNumber, billingLine.CASSCostCurrencyCode, AmountDifference);
			}

			return createClaimAppendLog;
		}

		ZString GetCASSBillingLineInfoAsText()
		{
			var logDetails =
				Res.GetString("3787608e-1d72-4d0e-ab44-f2256033a8e6", @"MAWB : {0}
Issue Date : {1}
Airline : {2}
Load : {3}
Discharge : {4}
Consol Weight : {5}
CASS Weight : {6} {7}
Weight Difference : {8} {7}
Accruals : {9} {13}
Net CASS Charges : {10} {13}
Is CASS Amendment : {11}
Status : {12}",
					billingLine.MAWBNumber, billingLine.IssueDate.ToShortDateString(),
					billingLine.Airline2LetterCode, billingLine.LoadPort, billingLine.DischargePort,
					billingLine.SystemWeight, billingLine.CASSWeight, billingLine.CASSWeightUnit,
					billingLine.WeightDifference, billingLine.SystemCostAccrualValue, billingLine.NetCASSCost,
					billingLine.IsCASSAmendment, billingLine.Status, Env.Instance.CurrentCompany.LocalCurrency.Code);

			return logDetails;
		}

		ZString? GetClaimType()
		{
			ZString? type = null;
			var status = billingLine.Status;
			switch (status)
			{
				case CASSBillingLine.StatusType.Underbilled:
					type = QueryClaimTypeCodeList.Codes.QCType6;
					break;
				case CASSBillingLine.StatusType.Overbilled:
					type = QueryClaimTypeCodeList.Codes.QCType5;
					break;
				case CASSBillingLine.StatusType.NotInSystem:
					type = QueryClaimTypeCodeList.Codes.QCType7;
					break;
				default:
					ErrorReporter.ReportOnce(Invariant($"CASSBillingClaimCreator.GetClaimType is called for invalid billingLine.Status {status}."));
					break;
			}

			return type;
		}

		APAccQueryClaim GetTheOnlyOpenClaim(BusinessObjectFactory factory = null)
		{
			var claim = GetClaim(billingLine.Factory);

			if (claim != null && factory != null && factory != billingLine.Factory)
			{
				var claimInAnotherFactory = GetClaim(factory);

				claim = claimInAnotherFactory != null && claimInAnotherFactory.AY_QueryClaimAmount == claim.AY_QueryClaimAmount ? claimInAnotherFactory : null;
			}

			return claim;

			APAccQueryClaim GetClaim(BusinessObjectFactory claimFactory)
			{
				var claims = APAccQueryClaim.LoadOpenClaimsWithMAWB(claimFactory, billingLine.MAWBNumber);

				return claims.Length > 1 ? null : claims.FirstOrDefault();
			}
		}

		#endregion

		#region Properties

		public bool ClaimExistsAndNeedToNotifyUser => ClaimExists &&
													(AmountDifference < ClaimAmountInLocalCurrency ||
													(AmountDifference > ClaimAmountInLocalCurrency && AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.Value == CASSAutoCreateClaim.BothOverUnderBilled));

		public bool ClaimExistsAndNeedsToBeClosed => ClaimExists &&
													(AmountDifference == ClaimAmountInLocalCurrency ||
													(AmountDifference > ClaimAmountInLocalCurrency && AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.Value == CASSAutoCreateClaim.OverBilled));

		public bool NeedToCreateNewClaim => !ClaimExists &&
			(
				billingLine.Status == CASSBillingLine.StatusType.Overbilled && new[] { CASSAutoCreateClaim.OverBilled, CASSAutoCreateClaim.BothOverUnderBilled }.Contains(AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.Value)
				|| billingLine.Status == CASSBillingLine.StatusType.Underbilled && AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.Value == CASSAutoCreateClaim.BothOverUnderBilled
				|| billingLine.Status == CASSBillingLine.StatusType.NotInSystem && AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.Value != CASSAutoCreateClaim.NotCreated
			)
			&&
			(
				AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.Value == AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.DefaultValue
				|| Math.Abs(AmountDifference) >= AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.Value
			);

		ZDecimal AmountDifference => -1M * (billingLine.IsCASSAmendment ? billingLine.NetCASSCost : billingLine.CostDifference);

		bool ClaimExists => billingLine.IsCASSAmendment && ClaimAmountInLocalCurrency != 0;

		ZDecimal ClaimAmountInLocalCurrency => GetTheOnlyOpenClaim()?.AY_QueryClaimAmount ?? 0;

		#endregion
	}
}
