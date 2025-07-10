using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared.JobComInvHeaderCharge;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common
{
	public abstract class JobComInvChargeCollectionBase<T> : SubsetBusinessObjectCollection<T>
		where T : JobComInvCharge
	{
		protected JobComInvChargeCollectionBase(ICommonInvoice parent)
			: base(parent.AllCharges)
		{
			Parent = parent;
			Rebuild();
		}

		protected override void RebuildOnConstruction()
		{
			// Parent needs to be set before the system can rebuild it.
		}

		protected override Rebuilder GetRebuilder() => new JobComInvChargeCollectionBaseRebuilder(this);

		protected class JobComInvChargeCollectionBaseRebuilder : Rebuilder
		{
			protected internal JobComInvChargeCollectionBaseRebuilder(SubsetBusinessObjectCollection<T> collection) : base(collection)
			{
			}

			protected override IEnumerable<BusinessObject> GetElementsForRebuild()
			{
				var baseResult = base.GetElementsForRebuild();
				return baseResult.OfType<T>();
			}
		}

		public bool HasChargesDistributedByOtherThanValue(params ChargeCodeChargeKey[] chargeKeys) => (from charge in this.Cast<T>() from chargeKey in chargeKeys where charge.WithKey(chargeKey) && charge.Currency != null && charge.J7_Amount != 0m && charge.J7_DistributeBy != ChargeDistributeByList.Codes.Value select charge).Any();

		public bool HasChargeOfThisTypeAndAmountAndCurrency(params ChargeCodeChargeKey[] chargeKeys) => (from charge in this.Cast<T>() from chargeKey in chargeKeys where charge.WithKey(chargeKey) && charge.Currency != null && charge.J7_Amount != 0m select charge).Any();

		public bool HasNonDutiableGSTApplicableCharges() => this.Cast<T>().Any(charge => !charge.J7_IsDutiable && charge.J7_IsGSTApplicable && charge.Currency != null);

		/// <summary>
		/// Return the first element from the top. Use this when you are sure there is only one. Otherwise use Find()
		/// </summary>
		public T this[string chargeName] => this.Cast<T>().FirstOrDefault(charge => charge.J7_ChargeType == chargeName);

		public void ClearCharge(ChargeCodeChargeKey chargeKey)
		{
			var charges = Find(chargeKey);
			foreach (var charge in charges)
			{
				RemoveAndDelete(charge);
			}
		}

		public void RemoveAll(string chargeName)
		{
			this.Cast<T>().Where(c => c.J7_ChargeType == chargeName).ToArray().ForEach(c => c.Delete());
		}

		public T[] Find(ChargeCodeChargeKey chargeKey)
		{
			return this.Cast<T>().Where(c => c.WithKey(chargeKey)).ToArray();
		}

		public T[] Find(ApportionChargeKey apportionChargeKey)
		{
			return this.Cast<T>().Where(c => c.WithKey(apportionChargeKey)).ToArray();
		}

		public IDisposable SuspendInvoiceHeaderChargeListChanged() => SuspendListChanged();

		public ApportionChargeKey[] AllChargeKeys
		{
			get
			{
				if (allChargeKeysCached == null)
				{
					allChargeKeysCached = new CachedProperty<ApportionChargeKey[]>(Factory, new GetValueDelegate<ApportionChargeKey[]>(GetAllChargeKeys));
				}
				return allChargeKeysCached.Value;
			}
		}
		CachedProperty<ApportionChargeKey[]> allChargeKeysCached;

		ApportionChargeKey[] GetAllChargeKeys()
		{
			var result = new ApportionChargeKeyUniqueList();
			foreach (var charge in this.Cast<T>())
			{
				if ((charge.J7_Amount > 0m && charge.Currency != null) || charge.J7_Percentage > 0m)
				{
					result.AddUniquely(charge.ApportionChargeKey);
				}
			}
			return result.GetUniqueItems();
		}

		#region AddNew

		public T AddNew(string chargeCode)
		{
			var result = AddNew();
			result.J7_ChargeType = chargeCode;
			return result;
		}

		public T AddNew(string chargeCode, ZDecimal amount)
		{
			var result = AddNew(chargeCode);
			result.J7_Amount = amount;
			return result;
		}

		public T AddNew(string chargeCode, ZDecimal amount, ZString currency)
		{
			var result = AddNew(chargeCode, amount);
			result.J7_RX_NKCurrency = currency;
			return result;
		}

		public T AddNewAMMVCharge()
		{
			var newCharge = AddNew();
			newCharge.J7_IsSystem = true;
			newCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			newCharge.J7_IsDutiable = true;
			newCharge.J7_IsNotIncludedInInvoice = true;
			return newCharge;
		}

		#endregion

		#region Calculate Amount for FOB & CIF

		public ZDecimal AmountToAddToITOTForDutiableCharges(RefCurrency currency) => AmountToAddToITOTWithThisFlag(currency, JobComInvHeaderChargeSchema.J7_IsDutiable.Name);

		public ZDecimal AmountToAddToITOTForStatisticalCharges(RefCurrency currency) => AmountToAddToITOTWithThisFlag(currency, JobComInvHeaderChargeSchema.J7_IsStatisticalValueApplicable.Name);

		public ZDecimal AmountToAddToITOTForVatableGstableCharges(RefCurrency currency) => AmountToAddToITOTWithThisFlag(currency, JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name);

		public ZDecimal AmountDutiable(RefCurrency currency) => AmountWithThisFlag(currency, JobComInvHeaderChargeSchema.J7_IsDutiable.Name);

		public ZDecimal AmountGSTApplicable(RefCurrency currency) => AmountWithThisFlag(currency, JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name);

		#endregion

		#region GetCharge overloaded methods

		public T[] GetCharge(string chargeCode) => this.Cast<T>().Where(c => c.J7_ChargeType == chargeCode).ToArray();

		public T GetChargeByChargeName(string chargeName) => this[chargeName];

		/// <summary>
		/// Find all charges having ChargeKey and return aggregated value in Currency. Optionally also takes a function to futher filters the found charges, which should return true if you want that charge to be included
		/// </summary>
		public ZDecimal GetCharge(string chargeCode, RefCurrency currency, Func<T, bool> funcToFurtherFilterChargesWhichShouldReturnTrueToInclude = null)
		{
			var result = new Money(0, currency);
			if (CurrencyConverter != null)
			{
				result = (from T charge in this where charge.J7_ChargeType == chargeCode where funcToFurtherFilterChargesWhichShouldReturnTrueToInclude == null || funcToFurtherFilterChargesWhichShouldReturnTrueToInclude(charge) select charge).Aggregate(result, (current, charge) => CurrencyConverter.Add(current, charge.Money));
				result = CurrencyConverter.ConvertExact(result, currency);
			}
			return result.Amount;
		}

		public Money GetCharge(CurrencyConverter currencyConverter, params ChargeCodeChargeKey[] chargeKeys)
		{
			var result = Money.Empty;

			if (currencyConverter != null)
			{
				result = chargeKeys.Aggregate(result, (current, chargeKey) => currencyConverter.Add(current, GetCharge(chargeKey)));
			}
			return result;
		}

		public ZDecimal GetCharge(ChargeCodeChargeKey chargeKey, RefCurrency currency)
		{
			var result = new Money(0, currency);
			if (CurrencyConverter != null)
			{
				result = this.Cast<T>().Where(charge => charge.WithKey(chargeKey)).Aggregate(result, (current, charge) => CurrencyConverter.Add(current, charge.Money));
				result = CurrencyConverter.ConvertExact(result, currency);
			}
			return result.Amount;
		}

		/// <summary>
		/// Find all charges having ChargeKey and return aggregated value
		/// </summary>
		public Money GetCharge(ChargeCodeChargeKey chargeKey)
		{
			var result = new Money(0, null);//null Currency is important. DOnt change this to Money.Empty
			if (CurrencyConverter != null)
			{
				result = this.Cast<T>().Where(charge => charge.WithKey(chargeKey)).Aggregate(result, (current, charge) => CurrencyConverter.Add(current, charge.Money));
			}
			return result;
		}

		public Money GetCharge(ApportionChargeKey apportionChargeKey)
		{
			var result = new Money(0, null);//null Currency is important. DOnt change this to Money.Empty
			if (CurrencyConverter != null)
			{
				result = this.Cast<T>().Where(charge => charge.WithKey(apportionChargeKey)).Aggregate(result, (current, charge) => CurrencyConverter.Add(current, charge.Money));
			}
			return result;
		}

		/// <summary>
		/// Find all charges having ChargeKey and return aggregated value
		/// </summary>
		public Money GetCharge(MessageChargeKey chargeKey)
		{
			var result = Money.Empty;
			if (CurrencyConverter != null)
			{
				result = this.Cast<T>().Where(charge => charge.WithKey(chargeKey)).Aggregate(result, (current, charge) => CurrencyConverter.Add(current, charge.Money));
			}
			return result;
		}

		public Money GetCharge(bool isDutiable, bool isIncludedInLines)
		{
			var result = Money.Empty;

			if (CurrencyConverter != null)
			{
				result = this.Cast<T>().Where(charge => charge.J7_IsDutiable == isDutiable && charge.J7_IsIncludedInITOT == isIncludedInLines).Aggregate(result, (current, charge) => CurrencyConverter.Add(current, charge.Money));
			}
			return result;
		}

		public Money GetCharge(bool isDutiable, bool isIncludedInLines, bool isGSTApplicable)
		{
			var result = Money.Empty;

			if (CurrencyConverter != null)
			{
				result = this.Cast<T>().Where(charge => charge.J7_IsDutiable == isDutiable && charge.J7_IsIncludedInITOT == isIncludedInLines && charge.J7_IsGSTApplicable == isGSTApplicable).Aggregate(result, (current, charge) => CurrencyConverter.Add(current, charge.Money));
			}
			return result;
		}

		public Money GetChargeFilteredByGSTApplicabilityAndITOTInclusion(bool isGSTApplicable, bool isIncludedInLines)
		{
			var result = Money.Empty;

			if (CurrencyConverter != null)
			{
				result = this.Cast<T>().Where(charge => charge.J7_IsGSTApplicable == isGSTApplicable && charge.J7_IsIncludedInITOT == isIncludedInLines).Aggregate(result, (current, charge) => CurrencyConverter.Add(current, charge.Money));
			}
			return result;
		}

		#endregion

		#region MoneyToAddToITOTForDutiableCharges

		public IEnumerable<Money> MoneyToAddToITOTForDutiableCharges() => MoneyToAddToITOTWithThisFlag(charge => charge.J7_IsDutiable);

		public IEnumerable<Money> MoneyToAddToITOTForVatableGstableCharges() => MoneyToAddToITOTWithThisFlag(charge => charge.J7_IsGSTApplicable);

		#endregion

		#region HasCharges() methods

		public bool HasChargeDistributedBy(string distributeBy) => this.Cast<T>().Any(charge => charge.J7_DistributeBy == distributeBy);

		public bool HasAnElementWithValidCharges() => this.Cast<T>().Any(charge => charge.Money.IsValid);

		public bool HasChargeWithThisKey(ChargeCodeChargeKey chargeKey) => this.Cast<T>().Any(charge => charge.WithKey(chargeKey) && (charge.Currency != null || charge.J7_Percentage > 0));

		public bool HasChargeWithThisKey(ApportionChargeKey apportionChargeKey) => this.Cast<T>().Any(charge => charge.WithKey(apportionChargeKey) && (charge.Currency != null || charge.J7_Percentage > 0));

		public bool HasChargeWithPercentage(ChargeCodeChargeKey chargeKey) => this.Cast<T>().Any(charge => charge.WithKey(chargeKey) && charge.J7_Percentage > 0);

		public bool HasChargeWithPercentage(ApportionChargeKey apportionChargeKey) => this.Cast<T>().Any(charge => charge.WithKey(apportionChargeKey) && charge.J7_Percentage > 0);

		public bool HasChargeWithCurrency(ChargeCodeChargeKey chargeKey) => this.Cast<T>().Any(charge => charge.WithKey(chargeKey) && charge.Currency != null && charge.J7_Percentage == 0);

		public bool HasChargeWithCurrency(ApportionChargeKey apportionChargeKey) => this.Cast<T>().Any(charge => charge.WithKey(apportionChargeKey) && charge.Currency != null && charge.J7_Percentage == 0);

		public bool HasThisCharge(ChargeCodeChargeKey chargeKey) => Find(chargeKey).Length > 0;

		public bool HasChargesIncludedInITOT(string chargeCode) => this.Cast<T>().Any(charge => charge.J7_ChargeType == chargeCode && charge.Currency != null && charge.J7_IsIncludedInITOT);

		public bool HasChargesIncludedInITOT(ChargeCodeChargeKey chargeKey) => this.Cast<T>().Any(charge => charge.WithKey(chargeKey) && charge.J7_IsIncludedInITOT && charge.Currency != null);

		public bool HasChargesExcludedInITOT(string chargeCode) => this.Cast<T>().Any(charge => charge.J7_ChargeType == chargeCode && charge.Currency != null && !charge.J7_IsIncludedInITOT);

		public bool HasOnlyOneChargeCurrency() => this.Select(charge => charge.Currency).Distinct().Count() == 1;

		public bool ShouldAddAmountToITOT(JobComInvCharge charge, bool isApplicable) => !charge.J7_IsIncludedInITOT && isApplicable && !charge.J7_AdjustedCharge && !charge.IsDiscount;

		public bool ShouldSubtractAmountFromITOT(JobComInvCharge charge, bool isApplicable)
		{
			var result = false;

			var sameChargeExist = charge.HasSameChargeWithDifferentAdjustedFlag();

			//adjusted amount deducts only if there is an original charge.
			if (charge.IsDiscount)
			{
				if (!charge.J7_AdjustedCharge && !sameChargeExist || charge.J7_AdjustedCharge && sameChargeExist)
				{
					result = !isApplicable && !charge.J7_IsIncludedInITOT;
				}
			}
			else
			{
				result = charge.J7_AdjustedCharge
					? !isApplicable && sameChargeExist
					: !sameChargeExist && !isApplicable && charge.J7_IsIncludedInITOT;
			}

			return result;
		}

		public IEnumerable<Money> MoneyToAddToITOTWithThisFlag(Func<T, bool> isApplicable)
		{
			foreach (var charge in this.Cast<T>())
			{
				if (charge.ChargeCode != null)
				{
					if (ShouldAddAmountToITOT(charge, isApplicable(charge)))
					{
						yield return charge.Money;
					}
					else if (ShouldSubtractAmountFromITOT(charge, isApplicable(charge)))
					{
						yield return new Money(-charge.Money.Amount, charge.Money.Currency);
					}
				}
			}
		}

		public IEnumerable<IChargesToAddToITOTWithThisFlagReturnValue<T>> ChargesToAddToITOTWithThisFlag(Func<T, bool> isApplicable)
		{
			foreach (var charge in this.Cast<T>())
			{
				if (charge.ChargeCode != null)
				{
					if (ShouldAddAmountToITOT(charge, isApplicable(charge)))
					{
						yield return new ChargesToAddToITOTWithThisFlagReturnValue { Charge = charge, Substract = false };
					}
					else if (ShouldSubtractAmountFromITOT(charge, isApplicable(charge)))
					{
						yield return new ChargesToAddToITOTWithThisFlagReturnValue { Charge = charge, Substract = true };
					}
				}
			}
		}

		public struct ChargesToAddToITOTWithThisFlagReturnValue : IChargesToAddToITOTWithThisFlagReturnValue<T>
		{
			public T Charge { get; set; }
			public bool Substract { get; set; }
		}

		public RefCurrency GetCommonApportionedChargesCurrencyOrInvoiceHeaderCurrency(RefCurrency invoiceHeaderCurrency)
			=> HasOnlyOneChargeCurrency() ? this.Select(charge => charge.Currency).FirstOrDefault() : invoiceHeaderCurrency;

		#endregion

		public ZDecimal GetTotal(CurrencyConverter currencyConverter, RefCurrency currency, Func<T, bool> predicate)
		{
			var result = ZDecimal.Zero;
			if (currencyConverter != null)
			{
				var amount = new Money(0, currency);
				amount = this.OfType<T>().Where(predicate).Aggregate(amount, (current, charge) => currencyConverter.Add(current, charge.Money));
				result = currencyConverter.ConvertExact(amount, currency).Amount;
			}
			return result;
		}

		ZDecimal AmountWithThisFlag(RefCurrency currency, string booleanColumnName)
		{
			var result = Money.Empty;

			if (CurrencyConverter != null)
			{
				foreach (T charge in this)
				{
					if (charge.ChargeCode != null)
					{
						var isApplicable = new ZBool(charge[booleanColumnName]);

						if (isApplicable)
						{
							result = CurrencyConverter.Add(result, charge.Money);
						}
					}
				}
				result = CurrencyConverter.ConvertExact(result, currency);
			}

			return result.Amount;
		}

		ZDecimal AmountToAddToITOTWithThisFlag(RefCurrency currency, string booleanColumnName)
		{
			var result = ZDecimal.Zero;

			if (CurrencyConverter != null)
			{
				var amountToAdd = new Money(0, currency);
				var amountToSubtract = new Money(0, currency);

				foreach (T charge in this)
				{
					if (charge.ChargeCode != null)
					{
						var isApplicable = new ZBool(charge[booleanColumnName]);

						if (ShouldAddAmountToITOT(charge, isApplicable))
						{
							amountToAdd = CurrencyConverter.Add(amountToAdd, charge.Money);
						}
						else if (ShouldSubtractAmountFromITOT(charge, isApplicable))
						{
							amountToSubtract = CurrencyConverter.Add(amountToSubtract, charge.Money);
						}
					}
				}
				result = CurrencyConverter.ConvertExact(CurrencyConverter.Subtract(amountToAdd, amountToSubtract), currency).Amount;
			}

			return result;
		}

		#region Implementation

		public readonly ICommonInvoice Parent;

		public CurrencyConverter CurrencyConverter => Parent.CurrencyConverter;

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			((T)child).Parent = Parent;
		}

		#endregion

	}
}

