using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	class BaseWIPAccrualCriticalValidation : AccTransactionLinesCriticalValidation
	{
		public BaseWIPAccrualCriticalValidation(BaseWIPAccrual parent)
			: base(parent)
		{
		}

		new BaseWIPAccrual Parent
		{
			get { return base.Parent as BaseWIPAccrual; }
		}

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			if (!CheckWIPAccrualLinkedToJobCharge())
			{
				var reversedErrorMessage = CriticalValidationMessageTemplate.GetReversedTransactionLineShouldBeOrNotLinkedToJobCharge_IfReversedErrorMessage(Parent.AL_LineType);
				var notReversedErrorMessage = CriticalValidationMessageTemplate.GetReversedTransactionLineShouldBeOrNotLinkedToJobCharge_IfNotReversedErrorMessage(Parent.AL_LineType);
				yield return new CriticalValidationResult(CriticalValidationErrorType.ReversedTransactionLineShouldBeOrNotLinkedToJobCharge_3,
													Parent.IsReversed ? reversedErrorMessage : notReversedErrorMessage,
													((Parent.IsReversed || Parent.RelatedJobCharge != null) ? Parent.RelatedJobCharge.GetJobChargeInfo() : string.Empty),
													Parent.GetTransactionLineInfo());
			}
			if (!CheckWIPAccrualRelatedToSameJobAsLinkedJobCharge())
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.LineShouldBeRelatedToSameJobAsLinkedJobCharge_3,
													CriticalValidationMessageTemplate.GetLineShouldBeRelatedToSameJobAsLinkedJobChargeErrorMessage(Parent.AL_LineType),
													Parent.RelatedJobCharge.GetJobChargeInfo(),
													Parent.GetTransactionLineInfo());
			}

			yield return CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount();

			if (!CheckWIPAccrualCurrencyIsLocalOne())
			{
				var info = CriticalValidationInfoCollectorService.GetService(Parent.Factory)?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.ACRWIPCurrencyNotSameWithLocalCurrency);
				var userContextCompany = Env.CurrentUserContext.Company;
				var jobCompany = Parent.Job.Company as ICompany;
				var currentUserInfo = FormattableString.Invariant($"""
					Current user context: company PK: {userContextCompany.PK}, code: {userContextCompany.Code}, currency: {userContextCompany.LocalCurrency.Code}.
					Parent Job Company info: company PK: {jobCompany.PK}, code: {jobCompany.Code}, currency: {jobCompany.LocalCurrency.Code}.
					""");

				yield return new CriticalValidationResult(CriticalValidationErrorType.LineCurrencyShouldHaveSameCurrencyAsLocalCurrency_3,
													CriticalValidationMessageTemplate.GetLineCurrencyShouldHaveSameCurrencyAsLocalCurrencyErrorMessage(Parent.AL_LineType),
													Parent.GetTransactionLineInfo(), info, currentUserInfo);
			}
		}

		bool CheckWIPAccrualLinkedToJobCharge()
		{
			var result = false;
			if (!Parent.IsReversed && Parent.RelatedJobCharge != null)
			{
				var chargeLineHasChanges = (Parent.AL_LineType == TransactionLineTypes.WIP) ? Parent.RelatedJobCharge.JR_AL_ARLineInfo.HasChanges : Parent.RelatedJobCharge.JR_AL_APLineInfo.HasChanges;
				result = !chargeLineHasChanges || Parent.RelatedJobCharge.IsSavedByFactory;
			}
			else if (Parent.IsReversed && Parent.RelatedJobCharge == null)
			{
				result = true;
			}
			return result;
		}

		bool CheckWIPAccrualRelatedToSameJobAsLinkedJobCharge()
		{
			var result = true;
			if (Parent.RelatedJobCharge == null || Parent.RelatedJobCharge.IsDeleted)
			{
				result = true;
			}
			else
			{
				result = Parent.AL_JH.IsValid && Parent.RelatedJobCharge.JR_JH == Parent.AL_JH;
				if (result && Parent.RelatedJobCharge.JR_JHInfo.HasChanges)
				{
					result = Parent.RelatedJobCharge.IsSavedByFactory;
				}
			}
			return result;
		}

		CriticalValidationResult CheckNewRelatedWIPAccrualLineAmountIsTheSameAsChargeAmount()
		{
			if (!Parent.AL_JH.IsEmpty && !Parent.IsReversed && (!Parent.IsInDatabase || Parent.AL_LineAmountInfo.HasChanges) &&
				Parent.RelatedJobCharge != null)
			{
				if (Parent.AL_LineType == TransactionLineTypes.WIP)
				{
					var chargeAmount = Parent.RelatedJobCharge.JR_LocalSellInvoiceAmt;
					var chargeCFX = Parent.RelatedJobCharge.JR_CFXAmt;
					var chargeTotal = chargeAmount - chargeCFX;
					var lineAmount = -Parent.AL_LineAmount;
					if (chargeTotal != lineAmount)
					{
						var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
						var extraMsg = collectorService?.GetInfo(Parent.RelatedJobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedWIPAmount);
						var extraMsg2 = collectorService?.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.LineLocalAmountNotEqualRelatedJobChargeLocalSellAmount);
						var extraMsg3 = collectorService?.GetInfo(Parent.RelatedJobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeChangedInDifferentCompanies);
						var extraMsg4 = collectorService?.GetInfo(Parent.RelatedJobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.WIPLocalSellInvoiceAmtInfo);
						var extraMsg5 = @$"Company Currency Change Log (Refer to issue with Exception Key JobChargeLocalAmountDecimalsDoNotMatchCurrencySubUnitRatio for more information):
{CompanyCurrencyChangeLogHelper.GetCurrentCompanyCurrencyChangeLog(Parent.Factory)}";
						var extraMsg6 = collectorService?.GetInfo(Parent.RelatedJobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_LineCFXHasChangesAfterSaving);
						if (!string.IsNullOrEmpty(extraMsg4))
						{
							extraMsg4 = extraMsg4 + (NoResString)"\r\n"
								+ (NoResString)"SellInvoiceExchangeRate In Critical Validation:\r\n"
								+ Parent.RelatedJobCharge.SellInvoiceExchangeRate.PropertiesToString(indentCharDepth: 2);
						}

						string previousLineDetails = string.Empty;
						if (Parent.RelatedJobCharge.JR_AL_ARLineInfo.HasChanges && !Parent.RelatedJobCharge.JR_AL_ARLineInfo.OriginalValue.IsEmpty)
						{
							var previousLine = Parent.Factory.Load<TransactionLine>((ZGuid)Parent.RelatedJobCharge.JR_AL_ARLineInfo.OriginalValue);
							if (previousLine == null)
							{
								previousLineDetails = (NoResString)"Could not load the Transaction Line.";
							}
							else
							{
								previousLineDetails = previousLine.GetAllPropertyValues();
							}
						}
						return new CriticalValidationResult(CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11,
															CriticalValidationMessageTemplate.GetLineShouldHaveSameAmountAsJobChargeErrorMessage(Parent.AL_LineType),
															Invariant($"{chargeAmount} - {chargeCFX} = {chargeTotal} != {lineAmount}"),
															Invariant($"JR_LocalSellInvoiceAmt = {Parent.RelatedJobCharge.JR_LocalSellInvoiceAmt}"),
															Invariant($"Charge.CompanyLocalCurrencyDecimals = {Parent.RelatedJobCharge.CompanyLocalCurrencyDecimals}"),
															Invariant($"Charge.LocalCurrencyDecimals = {Parent.RelatedJobCharge.LocalCurrencyDecimals}"),
															"----",
															Invariant($"Parent.RelatedJobCharge Properties:"),
															Parent.RelatedJobCharge.GetAllPropertyValues(),
															Invariant($"Parent Properties:"),
															Parent.GetAllPropertyValues(),
															Invariant($"Parent.Job Properties:"),
															Parent.Job.GetAllPropertyValues(),
															!string.IsNullOrEmpty(previousLineDetails) ? Invariant($@"Previous Line:
{previousLineDetails}") : string.Empty,
															extraMsg,
															extraMsg2,
															extraMsg3,
															extraMsg4,
															extraMsg5,
															extraMsg6
															);
					}
					else if ((Parent.RelatedJobCharge.JR_LocalSellAmtInfo.HasChanges || CheckCFXAmtRelatedChargeFieldsHasChanges()) && !Parent.RelatedJobCharge.IsSavedByFactory)
					{
						return new CriticalValidationResult(CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11,
															CriticalValidationMessageTemplate.GetLineShouldHaveSameAmountAsJobChargeErrorMessage(Parent.AL_LineType),
															Invariant($"Amounts are equal, but changed charge will not be saved in db."),
															Parent.RelatedJobCharge.GetJobChargeInfo(),
															Parent.GetTransactionLineInfo());
					}
				}
				else
				{
					var chargeAmount = Parent.RelatedJobCharge.JR_LocalCostAmt;
					var lineAmount = Parent.AL_LineAmount;
					if (chargeAmount != lineAmount)
					{
						var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
						var extraMsg = collectorService?.GetInfo(Parent.RelatedJobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalCostAmtNotEqualRelatedAccrualAmount);
						return new CriticalValidationResult(CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11,
															CriticalValidationMessageTemplate.GetLineShouldHaveSameAmountAsJobChargeErrorMessage(Parent.AL_LineType),
															Invariant($"{chargeAmount} != {lineAmount}"),
															Parent.RelatedJobCharge.GetJobChargeInfo(),
															Parent.GetTransactionLineInfo(),
															extraMsg);
					}
					else if (Parent.RelatedJobCharge.JR_LocalCostAmtInfo.HasChanges && !Parent.RelatedJobCharge.IsSavedByFactory)
					{
						return new CriticalValidationResult(CriticalValidationErrorType.LineShouldHaveSameAmountAsJobCharge_11,
															CriticalValidationMessageTemplate.GetLineShouldHaveSameAmountAsJobChargeErrorMessage(Parent.AL_LineType),
															Invariant($"Amounts are equal, but changed charge will not be saved in db."),
															Parent.RelatedJobCharge.GetJobChargeInfo(),
															Parent.GetTransactionLineInfo());
					}
				}
			}

			return null;
		}

		bool CheckWIPAccrualCurrencyIsLocalOne()
		{
			bool result = true;

			if (!Parent.IsReversed && (!Parent.IsInDatabase || Parent.AL_RX_NKTransactionCurrencyInfo.HasChanges) &&
				Parent.Job != null && Parent.Job.Company != null)
			{
				result = Parent.AL_RX_NKTransactionCurrency == Parent.Job.Company.GC_RX_NKLocalCurrency;
			}

			return result;
		}

		bool CheckCFXAmtRelatedChargeFieldsHasChanges()
		{
			var result = false;
			if (Parent.RelatedJobCharge.IsCFXPosted)
			{
				result = Parent.RelatedJobCharge.JR_AL_CFXLineInfo.HasChanges;
			}
			else if (Parent.RelatedJobCharge.IsApplyCFX && Parent.RelatedJobCharge.JR_LineCFX != 0m)
			{
				result = Parent.RelatedJobCharge.JR_RX_NKSellCurrencyInfo.HasChanges || Parent.RelatedJobCharge.JR_OSSellAmtInfo.HasChanges;
			}
			return result;
		}
	}
}

