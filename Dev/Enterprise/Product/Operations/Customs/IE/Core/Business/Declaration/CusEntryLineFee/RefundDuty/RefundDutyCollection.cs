using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class RefundDutyCollection : NonPersistentBusinessObjectCollection<RefundDuty>
	{
		public RefundDutyCollection(CusEntryLine entryLine) : base(entryLine.Factory)
		{
			CusEntryLine = entryLine;
		}

		public readonly CusEntryLine CusEntryLine;
		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException("Refund Duty Collection should not support adding.");
		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		public override void Load()
		{
			foreach (var chargeType in RefundDutyFees.Select(fee => fee.CF_ChargeType).Distinct())
			{
				AddNew(chargeType);
			}
		}

		public RefundDuty AddNew(ZString chargeType)
		{
			var refundDuty = new RefundDuty(chargeType, RefundDutyFees);
			Add(refundDuty);
			return refundDuty;
		}

		public RefundDutyFeeCollection RefundDutyFees
		{
			get
			{
				if (refundDutyFeesCache == null)
				{
					refundDutyFeesCache = new RefundDutyFeeCollection(CusEntryLine);
					refundDutyFeesCache.Load();
				}
				return refundDutyFeesCache;
			}
		}
		RefundDutyFeeCollection refundDutyFeesCache;
	}
}
