using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class JobConsolCostCriticalValidation : CriticalValidation<JobConsolCost>
	{
		public JobConsolCostCriticalValidation(JobConsolCost parent) : base(parent)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Reporting for Critical validation, Part of Developer Message")]
		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			bool postingWasReversed = !Parent.E6_AH_APInvoiceInfo.OriginalValue.IsEmpty && Parent.E6_AH_APInvoice.IsEmpty;
			if (!postingWasReversed)
			{
				var chargeCollection = Parent.Factory.Load<ApportionSplitCharge>(new ApportionmentSplitChargeCollection(Parent).CompleteFilter);
				var chargeOSCostAmtSum = chargeCollection.Sum(x => x.JR_OSCostAmt);
				if (Parent.E6_OSCostAmount != chargeOSCostAmtSum)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmount_9,
														CriticalValidationMessageTemplate.OverseasCostAmountNotEqualToSumOfApportionmentsOverseasCostAmountErrorMessage,
														Invariant($"{Parent.E6_OSCostAmount} != {chargeOSCostAmtSum}"),
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.JobConsolCostOSCostAmountChanged),
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost),
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero),
														Parent.GetJobConsolCostInfo(chargeCollection, true));
				}

				var chargeLocalCostAmtSum = chargeCollection.Sum(x => x.JR_LocalCostAmt);
				if (Parent.E6_LocalCostAmount != chargeLocalCostAmtSum)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.LocalCostAmountNotEqualToSumOfApportionmentsLocalCostAmount_6,
														CriticalValidationMessageTemplate.LocalCostAmountNotEqualToSumOfApportionmentsLocalCostAmountErrorMessage,
														Invariant($"{Parent.E6_LocalCostAmount} != {chargeLocalCostAmtSum}"),
														Parent.GetJobConsolCostInfo(chargeCollection),
														Parent.JobConsolCostInvalidLocalCurrencyDecimalsStackTrace,
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.PushToGreatestChargeExchangeRateDifferences_BeforeMethodCall),
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.PushToGreatestChargeExchangeRateDifferences_AfterMethodCall),
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.LocalCostAmountChangedThatCausedUnApportionedAmount),
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.ConsolCostAndApportionmentCharges_BeforeSynchroniseChargesWithInvoice),
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.ConsolCostAndApportionmentCharges_AfterSynchroniseChargesWithInvoice));
				}

				var chargeOSCostGSTAmtSum = chargeCollection.Sum(x => x.JR_OSCostGSTAmt);
				if (Parent.E6_IsTaxAmountOverridden && Parent.E6_OSGSTAmount != chargeCollection.Sum(x => x.JR_OSCostGSTAmt))
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.OverseasCostTaxAmountNotEqualToSumOfApportionmentsOverseasCostTaxAmount_11,
														CriticalValidationMessageTemplate.OverseasCostTaxAmountNotEqualToSumOfApportionmentsOverseasCostTaxAmountErrorMessage,
														Invariant($"{Parent.E6_OSGSTAmount} != {chargeOSCostGSTAmtSum}"),
														Parent.GetJobConsolCostInfo(chargeCollection),
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.PushUnApportionedGSTAmountBasedOnRepresentation_BeforeMethodCall),
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.PushUnApportionedGSTAmountBasedOnRepresentation_AfterMethodCall));
				}

				if (Parent.E6_ParentID.IsEmpty)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.CostNotLinkedToJobDueToEmptyParentID_2,
														CriticalValidationMessageTemplate.CostNotLinkedToJobDueToEmptyParentIDErrorMessage,
														Parent.GetJobConsolCostInfo(),
														RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType.ConsolCostParentChangedFromNonEmptyToEmpty));
				}

				if (Parent.E6_ParentTableCode.IsEmpty)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.CostNotLinkedToJobDueToEmptyParentTableCode_1,
														CriticalValidationMessageTemplate.CostNotLinkedToJobDueToEmptyParentTableCodeErrorMessage,
														Parent.GetJobConsolCostInfo());
				}

				if (Parent.E6_AH_APInvoice.IsValid && !Parent.E6_IsTaxAmountOverridden)
				{
					yield return new CriticalValidationResult(CriticalValidationErrorType.PostedConsolCostWithNonOverriddenTaxAmount_2,
														CriticalValidationMessageTemplate.GetPostedConsolCostWithNonOverriddenTaxAmountErrorMessage,
														Parent.GetJobConsolCostInfo());
				}

				BaseCharge problemCharge;
				bool problemChargeIsSaved;
				if (!CheckChargeAndConsolCostInvoiceDetailsAreEqual(chargeCollection, out problemCharge, out problemChargeIsSaved))
				{
					if (!problemChargeIsSaved)
					{
						yield return new CriticalValidationResult(CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8,
															CriticalValidationMessageTemplate.JobChargeInvoiceDetailsNotEqualConsolCostOnesErrorMessage,
															Parent.GetJobConsolCostInfo(),
															(NoResString)"Charge is not saved:",
															problemCharge.GetJobChargeInfo());
					}
					else
					{
						bool isExistingDataWithoutChange;
						var extraMsg = Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationInfoExtensions.GetConsolCostAndChargeInvoiceDetailsDifference(problemCharge, Parent, out isExistingDataWithoutChange);
						if (isExistingDataWithoutChange)
						{
							yield return new CriticalValidationResult(CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsWithoutChanges,
															CriticalValidationMessageTemplate.GetJobChargeInvoiceDetailsNotEqualConsolCostOnes_NeedToBeSyncErrorMessage(extraMsg));
						}
						else
						{
							var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
							var developerMessage1 = collectorService.GetInfoSafe(problemCharge.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails);
							var developerMessage2 = collectorService.GetInfoSafe(problemCharge.PK, CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);
							var developerMessage3 = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeCostAmountIsSetWithZero);
							var developerMessage4 = collectorService.GetInfoSafe(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost);

							yield return new CriticalValidationResult(CriticalValidationErrorType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails_8,
															CriticalValidationMessageTemplate.GetJobChargeInvoiceDetailsNotEqualConsolCostOnesErrorMessage(extraMsg),
															Parent.GetJobConsolCostInfo(),
															(NoResString)"Charge with incorrect data:",
															problemCharge.GetJobChargeInfo(),
															developerMessage1,
															developerMessage2,
															developerMessage3,
															developerMessage4);
						}
					}
				}

				var errorType = CheckChargeAndConsolCostHaveSameCostPostedStatus(chargeCollection, out problemCharge);
				switch (errorType)
				{
					case CriticalValidationErrorType.PostedConsolCostWithCostUnpostedApportionmentCharge_4:
						yield return new CriticalValidationResult(errorType,
															CriticalValidationMessageTemplate.PostedConsolCostWithCostUnpostedApportionmentChargeErrorMessage,
															Parent.GetJobConsolCostInfo(),
															"Charge with incorrect data:",
															problemCharge.GetJobChargeInfo(),
															GetReloadedObjectsInfo(Parent, problemCharge));
						break;

					case CriticalValidationErrorType.PostedConsolCostWithApportionmentChargePostedToDifferentInvoice_4:
						yield return new CriticalValidationResult(errorType,
															CriticalValidationMessageTemplate.PostedConsolCostWithApportionmentChargePostedToDifferentInvoiceErrorMessage,
															Parent.GetJobConsolCostInfo(),
															"Charge with incorrect data:",
															problemCharge.GetJobChargeInfo());
						break;

					case CriticalValidationErrorType.UnpostedConsolCostWithCostPostedApportionmentCharge_3:
						yield return new CriticalValidationResult(errorType,
															CriticalValidationMessageTemplate.UnpostedConsolCostWithCostPostedApportionmentChargeErrorMessage,
															Parent.GetJobConsolCostInfo(),
															"Charge with incorrect data:",
															problemCharge.GetJobChargeInfo());
						break;
				}
			}
		}

		string RetrieveInfoFromCriticalValidationInfoCollector(CriticalValidationInfoCollectorServiceKeyType keyType)
		{
			var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
			var info = collectorService.GetInfoSafe(Parent.PK, keyType);
			return info;
		}

		bool CheckChargeAndConsolCostInvoiceDetailsAreEqual(ApportionSplitCharge[] chargeCollection, out BaseCharge problemCharge, out bool problemChargeIsSaved)
		{
			bool result = true;
			problemCharge = null;
			problemChargeIsSaved = true;

			if ((!Parent.IsInDatabase || Parent.E6_InvoiceNumInfo.HasChanges || Parent.E6_InvoiceDateInfo.HasChanges ||
				Parent.E6_PaymentDateInfo.HasChanges || Parent.E6_OH_CreditorInfo.HasChanges || Parent.E6_CostReferenceInfo.HasChanges ||
				Parent.E6_AT_TaxRateInfo.HasChanges || Parent.E6_TaxDateInfo.HasChanges || Parent.E6_A9_VATClassInfo.HasChanges ||
				Parent.E6_IsTaxAmountOverriddenInfo.HasChanges || Parent.E6_SupplyTypeInfo.HasChanges ||
				Parent.E6_GB_CostTaxBranchInfo.HasChanges))
			{
				foreach (ApportionSplitCharge charge in chargeCollection)
				{
					if (charge.JR_APInvoiceNum != Parent.E6_InvoiceNum || !AreDateTimesEqualSafe(charge.JR_APInvoiceDateInfo, Parent.E6_InvoiceDateInfo) ||
						!AreDateTimesEqualSafe(charge.JR_PaymentDateInfo, Parent.E6_PaymentDateInfo) || charge.JR_OH_CostAccount != Parent.E6_OH_Creditor ||
						charge.JR_CostReference != Parent.E6_CostReference ||
						charge.JR_AT_CostGSTRate != Parent.E6_AT_TaxRate || charge.JR_CostTaxDate != Parent.E6_TaxDate ||
						charge.JR_A9_CostVATClass != Parent.E6_A9_VATClass || charge.JR_IsCostTaxAmountOverridden != Parent.E6_IsTaxAmountOverridden ||
						charge.JR_CostSupplyType != Parent.E6_SupplyType ||
						charge.JR_GB_CostTaxBranch != Parent.E6_GB_CostTaxBranch)
					{
						result = false;
						problemCharge = charge;
						break;
					}

					if (!ZDataUtils.ShouldRowBeSaved(((IBusinessObjectInternals)charge).Row) &&
						charge.JR_IsUsedForApportionment &&
						(!charge.IsInDatabase ||
						charge.JR_APInvoiceNumInfo.HasChanges ||
						charge.JR_APInvoiceDateInfo.HasChanges ||
						charge.JR_PaymentDateInfo.HasChanges ||
						charge.JR_OH_CostAccountInfo.HasChanges ||
						charge.JR_CostReferenceInfo.HasChanges ||
						charge.JR_AT_CostGSTRateInfo.HasChanges ||
						charge.JR_CostTaxDateInfo.HasChanges ||
						charge.JR_A9_CostVATClassInfo.HasChanges ||
						charge.JR_IsCostTaxAmountOverriddenInfo.HasChanges ||
						charge.JR_CostSupplyTypeInfo.HasChanges ||
						charge.JR_GB_CostTaxBranchInfo.HasChanges))
					{
						result = false;
						problemCharge = charge;
						problemChargeIsSaved = false;
						break;
					}
				}
			}

			return result;
		}

		CriticalValidationErrorType CheckChargeAndConsolCostHaveSameCostPostedStatus(ApportionSplitCharge[] chargeCollection, out BaseCharge problemCharge)
		{
			var result = CriticalValidationErrorType.NoError;
			problemCharge = null;

			if (!Parent.IsInDatabase || Parent.E6_AH_APInvoiceInfo.HasChanges)
			{
				var isConsolCostPosted = Parent.E6_AH_APInvoice.IsValid;
				bool isAtLeastOneChargePostedJRJ = isConsolCostPosted && chargeCollection.Any(t => t.IsCostPostedWithJobRevenueJournal);
				foreach (ApportionSplitCharge charge in chargeCollection)
				{
					if (isConsolCostPosted && !isAtLeastOneChargePostedJRJ && !charge.IsCostPostedWithAPTransaction)
					{
						result = CriticalValidationErrorType.PostedConsolCostWithCostUnpostedApportionmentCharge_4;
						problemCharge = charge;
						break;
					}
					if (isConsolCostPosted && !isAtLeastOneChargePostedJRJ && charge.IsCostPostedWithAPTransaction && charge.APLine.AL_AH != Parent.E6_AH_APInvoice)
					{
						result = CriticalValidationErrorType.PostedConsolCostWithApportionmentChargePostedToDifferentInvoice_4;
						problemCharge = charge;
						break;
					}
					if (!isConsolCostPosted && charge.IsCostPostedWithAPTransaction)
					{
						result = CriticalValidationErrorType.UnpostedConsolCostWithCostPostedApportionmentCharge_3;
						problemCharge = charge;
						break;
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of Developer Message")]
		static string GetReloadedObjectsInfo(JobConsolCost jobConsolCost, BaseCharge problemCharge)
		{
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var jobConsolCostReloaded = newFactory.Load<JobConsolCost>(jobConsolCost != null ? jobConsolCost.PK : ZGuid.Empty);
			var problemChargeReloaded = newFactory.Load<BaseCharge>(problemCharge != null ? problemCharge.PK : ZGuid.Empty);

			var message = string.Format(CultureInfo.InvariantCulture, "\r\nJobConsolCost Reloaded:\r\n{0}Problem Charge Reloaded:\r\n{1}",
				jobConsolCostReloaded != null ? jobConsolCostReloaded.GetJobConsolCostInfo() : "NULL",
				problemChargeReloaded != null ? problemChargeReloaded.GetJobChargeInfo() : "NULL");

			return message;
		}
	}
}

