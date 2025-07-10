using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCMRMessageChargeItem : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocCMRMessageChargeItem(ZString chargeType, ZDecimal amount)
		{
			fChargeType = chargeType;
			fAmount = amount;
		}

		public ZString ChargeType
		{
			get { return fChargeType; }
		}

		public ZString Description
		{
			get
			{
				ZString result = ChargeType;
				if (ShouldAddTotalPayableText)
				{
					result = "Total Payable " + ChargeType;
				}

				return result.ToUpper();
			}
		}

		public ZDecimal Amount
		{
			get { return fAmount; }
		}

		public ZString EmptyStringIfAmountIsZero
		{
			get { return Amount.IsEmpty ? "" : Amount.ToString(2); }
		}

		#region Implementation
		readonly ZString fChargeType;
		readonly ZDecimal fAmount;
		ZBool ShouldAddTotalPayableText
		{
			get
			{
				return (ChargeType == CusEntryChargeTypeList.Descriptions.DutyAmount ||
					ChargeType == CusEntryChargeTypeList.Descriptions.EntryFee ||
					ChargeType == CusEntryChargeTypeList.Descriptions.GSTAmount ||
					ChargeType == CusEntryChargeTypeList.Descriptions.GSTDeferred ||
					ChargeType == CusEntryChargeTypeList.Descriptions.OtherCharges);
			}
		}
		#endregion
	}

	public class DocCMRMessageChargeItemCollection : NonPersistentBusinessObjectCollection<DocCMRMessageChargeItem>
	{
		public DocCMRMessageChargeItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
