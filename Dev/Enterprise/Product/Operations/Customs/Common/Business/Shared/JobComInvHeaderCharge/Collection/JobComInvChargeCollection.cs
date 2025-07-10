using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared.JobComInvHeaderCharge;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	public interface IJobComInvChargeCollection<out T> : IJobComInvChargeBaseCollection<T>
		where T : JobComInvCharge
	{
		ApportionChargeKey[] AllChargeKeys { get; }

		new T this[int index] { get; }
		T First();

		void SetCurrency(ZString currencyCode);
		void SetIncludedInInvoiceFlagsForNonIncotermNeutralCharges();
		ZDecimal AmountToAddForITOT(RefCurrency currency);
		void CopyToList(IList charges);
		void AddCharge(ApportionChargeKey chargeKey, ZDecimal amount, ZString currency);
		void RemoveAll(string chargeName);
		void AssignIsIncludedInITOT(bool value, ChargeCodeChargeKey chargeKey);

		bool HasChargeWithCurrency(ApportionChargeKey apportionChargeKey);
		bool HasChargesIncludedInITOT(ChargeCodeChargeKey chargeKey);
		bool HasChargeWithPercentage(ApportionChargeKey chargeKey);
		bool HasChargeWithPercentage(ChargeCodeChargeKey chargeKey);
		bool HasOnlyOneChargeCurrency();
		bool HasChargesDistributedByOtherThanValue(params ChargeCodeChargeKey[] chargeKeys);
		bool HasNonDutiableGSTApplicableCharges();

		string GetDistributeBy(ApportionChargeKey chargeKey);
		Money GetCharge(CurrencyConverter currencyConverter, params ChargeCodeChargeKey[] chargeKeys);
		Money GetChargeFilteredByGSTApplicabilityAndITOTInclusion(bool isGSTApplicable, bool isIncludedInLines);

		ZDecimal AmountDutiable(RefCurrency currency);
		ZDecimal AmountGSTApplicable(RefCurrency currency);
		IEnumerable<Money> MoneyToAddToITOTWithThisFlag(Func<T, bool> isApplicable);
	}
	public class JobComInvChargeCollection<T> : JobComInvChargeCollectionBase<T>, IJobComInvChargeCollection<T>
		where T : JobComInvCharge
	{
		public JobComInvChargeCollection(ICommonInvoice invoice)
			: base(invoice)
		{
		}

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();
		public T First() => this.Cast<T>().First();

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeOfElements ?? (typeOfElements = GetElementTypeFromCollectionType(GetType()));
		Type typeOfElements;

		public bool HasChargesDistributedByOtherThanValue() =>
			this.Cast<T>().Any(charge => charge.J7_DistributeBy == ChargeDistributeByList.Codes.Weight) ||
			this.Cast<T>().Any(charge => charge.J7_DistributeBy == ChargeDistributeByList.Codes.Volume);

		public string GetDistributeBy(ChargeCodeChargeKey chargeKey)
		{
			var charges = Find(chargeKey);
			string result = "";
			if (charges.Length > 0)
			{
				result = charges[0].J7_DistributeBy;
			}
			return result;
		}

		public string GetDistributeBy(ApportionChargeKey chargeKey)
		{
			var charges = this.Cast<T>().Where(c => c.WithKey(chargeKey)).ToArray();
			var result = "";
			if (charges.Length > 0)
			{
				result = charges[0].J7_DistributeBy;
			}
			return result;
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			var chargeToRemove = (JobComInvCharge)bizO;
			isChargeToBeRemovedValid = ((chargeToRemove.J7_Amount > 0m && chargeToRemove.Currency != null) || chargeToRemove.J7_Percentage > 0m);
		}

		bool isChargeToBeRemovedValid;
		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (isChargeToBeRemovedValid && Parent.InvoicesHolder != null)
			{
				Parent.InvoicesHolder.MarkApportionmentDirty();
			}
			isChargeToBeRemovedValid = false;
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var charge = (T)element;
			return charge != null && !charge.J7_IsApportionedCharge;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var charge = (T)child;
			charge.J7_IsApportionedCharge = false;
			using (charge.SuspendMarkApportionmentDirty())
			{
				charge.J7_DistributeBy = DefaultDistributeBy();
			}
		}

		string DefaultDistributeBy()
		{
			var defaultValue = ChargeDistributeByList.Codes.Value;
			if (Parent is IChargeHolder chargeHolder)
			{
				var valuFromChargeHolder = chargeHolder.GetDefaultDistributeBy();
				if (!valuFromChargeHolder.IsEmpty)
				{
					defaultValue = valuFromChargeHolder;
				}
			}
			return defaultValue;
		}

		public void AddCharge(ApportionChargeKey chargeKey, ZDecimal amount, ZString currency)
		{
			T result = null;

			foreach (T charge in this)
			{
				if (charge.WithKey(chargeKey) && charge.J7_RX_NKCurrency == currency)
				{
					result = charge;
					break;
				}
			}

			if (result == null)
			{
				result = AddNew();

				result.J7_ChargeType = chargeKey.ChargeKey.ChargeCode;
				result.J7_IsDutiable = chargeKey.ChargeKey.IsDutiable;
				result.J7_IsGSTApplicable = chargeKey.ChargeKey.IsVATible;
				result.J7_IsNotIncludedInInvoice = chargeKey.IsIncludedInInvoice == GroupIsIncludedInLinesOptionList.Codes.No;
				result.J7_IsIncludedInITOT = chargeKey.IsIncludedInITOT == GroupIsIncludedInLinesOptionList.Codes.Yes;
				result.J7_DistributeBy = chargeKey.DistributeBy;
				result.J7_FullOrPartialApportionment = chargeKey.ApportionType;

				result.J7_Percentage = chargeKey.Percentage;
				result.J7_RX_NKCurrency = currency;
				result.J7_ChargeDescription = chargeKey.ChargeDesc;
			}

			result.J7_Amount += amount;
		}

		public void AssignIsIncludedInITOT(bool value, ChargeCodeChargeKey chargeKey)
		{
			var charges = Find(chargeKey);
			foreach (var charge in charges)
			{
				charge.J7_IsIncludedInITOT = value;
			}
		}

		public void SetCurrency(ZString currencyCode)
		{
			foreach (var charge in this)
			{
				if (charge.J7_Amount > 0 && charge.J7_RX_NKCurrency.IsEmpty)
				{
					charge.J7_RX_NKCurrency = currencyCode;
				}
			}
		}

		public void SetIncludedInInvoiceFlagsForNonIncotermNeutralCharges()
		{
			foreach (var charge in this)
			{
				charge.ResetDefaultIsIncludedInAmountAndIsIncludedInITOTIfDetermined();
			}
		}

		public ZDecimal AmountToAddForITOT(RefCurrency currency)
		{
			var result = new Money(0, currency);

			if (CurrencyConverter != null)
			{
				foreach (var charge in this)
				{
					if (!charge.J7_IsIncludedInITOT
						&& !charge.J7_IsNotIncludedInInvoice
						&& !charge.J7_AdjustedCharge)
					{
						if (charge.IsDiscount)
						{
							result = CurrencyConverter.Subtract(result, charge.Money);
						}
						else
						{
							result = CurrencyConverter.Add(result, charge.Money);
						}
					}
				}

				result = CurrencyConverter.ConvertExact(result, currency);
			}
			return result.Amount;
		}
	}
}

