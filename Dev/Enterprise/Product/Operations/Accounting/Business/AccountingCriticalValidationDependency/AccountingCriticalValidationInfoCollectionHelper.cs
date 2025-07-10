using System;
using System.Diagnostics;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;

namespace Enterprise.Accounting.Business.AccountingCriticalValidationDependency
{
	public interface IAccountingCriticalValidationInfoCollectionHelper
	{
		void AddInfo_APInvoiceLinePostingCarryForwardAmountWithDifferentSigns(ZGuid carryForwardAmountID, InvoicingLineBase aPInvoiceLine, Charge[] associatedCharges, ZDecimal localAmount, ZDecimal oSAmount, Func<ZString> getMoreInfo);

		void AddInfo_CarryForwardChargeAmountWithDifferentSigns(Charge charge, InvoicingLineBase aPInvoiceLine, TransactionLineJobChargeTransformer.AmountWrapper amountWrapper, ZDecimal localAmount, ZDecimal oSAmount, Func<ZString> getMoreInfo);
	}

	public class AccountingCriticalValidationInfoCollectionHelper : IAccountingCriticalValidationInfoCollectionHelper
	{
		void IAccountingCriticalValidationInfoCollectionHelper.AddInfo_APInvoiceLinePostingCarryForwardAmountWithDifferentSigns(ZGuid carryForwardAmountID, InvoicingLineBase aPInvoiceLine, Charge[] associatedCharges, ZDecimal localAmount, ZDecimal oSAmount, Func<ZString> getMoreInfo)
		{
			if (oSAmount != 0 && localAmount != 0 && Math.Sign(localAmount) != Math.Sign(oSAmount))
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(aPInvoiceLine.Factory).AddInfoWhenAllowed(carryForwardAmountID, CriticalValidationInfoCollectorServiceKeyType.APInvoiceLinePostingCarryForwardAmountWithDifferentSigns,
				() =>
				{
					var associatedChargesInfo = new ZStringBuilder(associatedCharges.Select(x => x.GetAllPropertyValues()));

					return FormattableString.Invariant(
$@"Current Line PK: {aPInvoiceLine.PK}
{nameof(localAmount)}: {localAmount}, {nameof(oSAmount)}: {oSAmount}
{nameof(aPInvoiceLine.IsPopulatedFromImportedJobCharge)}: {aPInvoiceLine.IsPopulatedFromImportedJobCharge}
{getMoreInfo()}

Charges for carry forward calculation:
{associatedChargesInfo.ToStringWithNewLineBetweenAppends()}

StackTrace: {GetFirstStackTraceFramesOnly()}");
				});
			}
		}

		void IAccountingCriticalValidationInfoCollectionHelper.AddInfo_CarryForwardChargeAmountWithDifferentSigns(Charge charge, InvoicingLineBase aPInvoiceLine, TransactionLineJobChargeTransformer.AmountWrapper amountWrapper, ZDecimal localAmount, ZDecimal oSAmount, Func<ZString> getMoreInfo)
		{
			if (oSAmount != 0 && localAmount != 0 && Math.Sign(localAmount) != Math.Sign(oSAmount))
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(charge.Factory).AddInfoWhenAllowed(charge.PK, CriticalValidationInfoCollectorServiceKeyType.CarryForwardChargeAmountWithDifferentSigns,
				() =>
				{
					var collectedInfoOfAPInvoiceLine = CriticalValidationInfoCollectorService.GetService(aPInvoiceLine.Factory).GetInfoSafe(amountWrapper.InfoCollectionID, CriticalValidationInfoCollectorServiceKeyType.APInvoiceLinePostingCarryForwardAmountWithDifferentSigns);

					var aPInvoiceLinesInfo = new ZStringBuilder(aPInvoiceLine.InvoiceBase.Lines.Select(x => x.GetAllPropertyValues()));

					return FormattableString.Invariant(
$@"{nameof(amountWrapper.OSAmount)}: {amountWrapper.OSAmount}, {nameof(amountWrapper.LocalAmount)}: {amountWrapper.LocalAmount}
{nameof(amountWrapper.IsOSCurrencyApplicable)}: {amountWrapper.IsOSCurrencyApplicable}
{getMoreInfo()}
{collectedInfoOfAPInvoiceLine}

Charge with opposite amounts:
{charge.GetAllPropertyValues()}

All Invoice Lines:
{aPInvoiceLinesInfo.ToStringWithNewLineBetweenAppends()}

StackTrace: {GetFirstStackTraceFramesOnly()}");
				});
			}
		}

		static string GetFirstStackTraceFramesOnly()
		{
			var callstack = new StackTrace(5, false);
			var stringBuilder = new ZStringBuilder();
			for (int i = 0; i <= 3; i++)
			{
				stringBuilder.Append(callstack.GetFrame(i).ToString());
			}
			return stringBuilder.ToString();
		}
	}
}
