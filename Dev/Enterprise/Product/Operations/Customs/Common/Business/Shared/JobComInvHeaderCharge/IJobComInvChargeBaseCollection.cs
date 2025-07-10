using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common.Shared.JobComInvHeaderCharge
{
	public interface IJobComInvChargeBaseCollection<out T> : IBusinessObjectCollection<T>
	{
		new T this[int index] { get; }
		new T AddNew();

		CurrencyConverter CurrencyConverter { get; }

		T[] Find(ChargeCodeChargeKey chargeCode);
		T[] Find(ApportionChargeKey chargeCode);

		T AddNew(string chargeCode);
		T AddNew(string chargeCode, ZDecimal amount);
		T AddNew(string chargeCode, ZDecimal amount, ZString currency);

		T[] GetCharge(string chargeCode);
		Money GetCharge(ApportionChargeKey chargeKey);
		Money GetCharge(ChargeCodeChargeKey chargeKey);
		Money GetCharge(MessageChargeKey chargeKey);
		ZDecimal GetCharge(ChargeCodeChargeKey chargeCode, RefCurrency currency);
		Money GetCharge(bool isDutiable, bool isIncludedInLines);
		Money GetCharge(bool isDutiable, bool isIncludedInLines, bool isGSTApplicable);
		ZDecimal GetCharge(string chargeCode, RefCurrency currency, Func<T, bool> funcToFurtherFilterChargesWhichShouldReturnTrueToInclude = null);
		T GetChargeByChargeName(string chargeCode);
		RefCurrency GetCommonApportionedChargesCurrencyOrInvoiceHeaderCurrency(RefCurrency currency);

		bool HasAnElementWithValidCharges();
		bool HasChargesIncludedInITOT(string chargeCode);
		bool HasChargesExcludedInITOT(string chargeCode);
		bool HasChargeDistributedBy(string chargeCode);
		bool HasChargeWithThisKey(ApportionChargeKey chargeKey);
		bool HasChargeWithThisKey(ChargeCodeChargeKey chargeKey);
		bool HasChargeWithCurrency(ChargeCodeChargeKey chargeKey);
		bool HasChargeOfThisTypeAndAmountAndCurrency(params ChargeCodeChargeKey[] chargeKeys);

		ZDecimal AmountToAddToITOTForDutiableCharges(RefCurrency currency);
		ZDecimal AmountToAddToITOTForVatableGstableCharges(RefCurrency currency);
		ZDecimal AmountToAddToITOTForStatisticalCharges(RefCurrency currency);

		IEnumerable<Money> MoneyToAddToITOTForDutiableCharges();
		IEnumerable<Money> MoneyToAddToITOTForVatableGstableCharges();

		IEnumerable<IChargesToAddToITOTWithThisFlagReturnValue<T>> ChargesToAddToITOTWithThisFlag(Func<T, bool> isApplicable);

		ZDecimal GetTotal(CurrencyConverter currencyConverter, RefCurrency currency, Func<T, bool> predict);
		bool ShouldAddAmountToITOT(JobComInvCharge charge, bool isApplicable);
		bool ShouldSubtractAmountFromITOT(JobComInvCharge charge, bool isApplicable);
	}

	public interface IChargesToAddToITOTWithThisFlagReturnValue<out T>
	{
		T Charge { get; }
		bool Substract { get; }
	}
}
