using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IEUCusEntryLineFeeCollection<out TFee, out TLine> : ICusEntryLineFeeCollection<TFee, TLine>
		where TFee : CusEntryLineFee
		where TLine : CusEntryLine
	{
		new TFee this[int index] { get; }
		IDisposable SuspendSystemAddedVatFeeRecalculation();
		bool HasOverrideFeeOfGivenCode(ZString feeType);
		IVatRefresher VatRefresher { get; }
		IEnumerable<TFee> AllLineFees { get; }
	}

	public class CusEntryLineFeeCollection<TFee, TLine> : Customs.Business.CusEntryLineFeeCollection<TFee, TLine>, IEUCusEntryLineFeeCollection<TFee, TLine>
		where TFee : CusEntryLineFee
		where TLine : CusEntryLine
	{
		public CusEntryLineFeeCollection(TLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
			VatRefresher = VatFeeRefresher<TFee, TLine>.New(this);
		}

		public bool HasOverrideFeeOfGivenCode(ZString feeType)
		{
			return Elements.Cast<CusEntryLineFee>().Any(f => f.CF_ChargeType == feeType && f.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Override);
		}

		public IEnumerable<TFee> AllLineFees => this.Cast<TFee>();
		public IVatRefresher VatRefresher { get; }
		public IDisposable SuspendSystemAddedVatFeeRecalculation() => VatRefresher.SuspendSystemAddedVatFeeRecalculation();
	}
}
