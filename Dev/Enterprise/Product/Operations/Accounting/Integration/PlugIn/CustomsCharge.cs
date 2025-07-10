using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public class CustomsCharge
	{
		public struct ChargeInfo
		{
			public ZString Description;
			public ZDecimal Amount;

			public ChargeInfo(ZString description, ZDecimal amount)
			{
				Description = description;
				Amount = amount;
			}

			public override string ToString()
			{
				return $"  {Description} {Amount.ToString(2)}";
			}
		}

		public CustomsCharge(AccChargeCode chargeCode, ZString description, ZDecimal amount, ZDecimal gST, ZBool isPaidByBroker, ZGuid creditorPK)
			: this(chargeCode != null ? chargeCode.PK : ZGuid.Empty, description, amount, gST, isPaidByBroker, creditorPK)
		{
		}

		public CustomsCharge(AccChargeCode chargeCode, ZString description, ZDecimal amount, ZDecimal gST, ZBool isPaidByBroker, ZGuid creditorPK, ZString overrideCurrency)
			: this(chargeCode != null ? chargeCode.PK : ZGuid.Empty, description, amount, gST, isPaidByBroker, creditorPK, overrideCurrency, ZString.Empty, ZGuid.Empty)
		{
		}

		public CustomsCharge(ZGuid chargeCodePK, ZString description, ZDecimal amount, ZDecimal gST, ZBool isPaidByBroker, ZGuid creditorPK)
			: this(chargeCodePK, description, amount, gST, isPaidByBroker, creditorPK, ZString.Empty, ZString.Empty, ZGuid.Empty)
		{
		}

		public CustomsCharge(ZGuid chargeCodePK, ZString description, ZDecimal amount, ZDecimal gST, ZBool isPaidByBroker, ZGuid creditorPK, ZString overrideCurrency, ZString entryReference, ZGuid debtorPK)
		{
			this.ChargeCodePK = chargeCodePK;
			this.Description = description;
			this.Amount = amount;
			this.GST = gST;
			this.IsPaidByBroker = isPaidByBroker;
			this.CreditorPK = creditorPK;
			this.OverrideCurrency = overrideCurrency;
			this.EntryReference = entryReference;
			this.DebtorPK = debtorPK;
			this.AdditionalChargeInfos = new List<ChargeInfo>();
		}

		public AccChargeCode GetChargeCode(BusinessObjectFactory factory)
		{
			return factory != null ? factory.Load<AccChargeCode>(ChargeCodePK) : null;
		}

		public ZGuid ChargeCodePK { get; set; }
		public ZString Description { get; private set; }
		public ZString EntryReference { get; set; }
		public ZDecimal Amount { get; private set; }
		public ZString OverrideCurrency { get; private set; }
		public ZDecimal GST { get; private set; }
		public ZBool IsPaidByBroker { get; private set; }
		public ZBool IsInformationOnly { get { return !IsPaidByBroker; } }
		public ZGuid CreditorPK { get; private set; }
		public ZGuid DebtorPK { get; set; }
		public List<ChargeInfo> AdditionalChargeInfos { get; private set; }

		public void AddAmount(ZDecimal amountToAdd, ZDecimal gstToAdd)
		{
			Amount += amountToAdd;
			GST += gstToAdd;
		}

		public void AddAmount(bool amountToAddIsGST, ZDecimal amountToAdd)
		{
			if (amountToAddIsGST)
			{
				GST += amountToAdd;
			}
			else
			{
				Amount += amountToAdd;
			}
		}

		public override bool Equals(object obj)
		{
			CustomsCharge other = obj as CustomsCharge;
			if (other == null)
			{
				return false;
			}

			return ChargeCodePK == other.ChargeCodePK &&
				Description == other.Description &&
				Amount == other.Amount &&
				OverrideCurrency == other.OverrideCurrency &&
				GST == other.GST &&
				IsPaidByBroker == other.IsPaidByBroker &&
				EntryReference == other.EntryReference &&
				CreditorPK == other.CreditorPK &&
				DebtorPK == other.DebtorPK;
		}

		public override int GetHashCode()
		{
			return ChargeCodePK.GetHashCode() ^
				Description.GetHashCode() ^
				Amount.GetHashCode() ^
				OverrideCurrency.GetHashCode() ^
				GST.GetHashCode() ^
				IsPaidByBroker.GetHashCode() ^
				EntryReference.GetHashCode() ^
				CreditorPK.GetHashCode() ^
				DebtorPK.GetHashCode();
		}
	}
}
