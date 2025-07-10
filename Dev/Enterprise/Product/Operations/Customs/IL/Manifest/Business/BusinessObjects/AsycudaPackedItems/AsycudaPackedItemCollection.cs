using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaPackedItemCollection<TBillItem, TBill> : AsycudaBillPackedItemCollection<TBillItem, TBill>, IAsycudaPackedItemCollection<TBillItem, TBill>, ISequenceNumberHeader
		where TBillItem : AsycudaPackedItem
		where TBill : AsycudaBill
	{
		public AsycudaPackedItemCollection(TBill bill) : base(bill)
		{
		}

		public new HugeSequenceNumberGenerator SequenceNumberCalculator
		{
			get { return sequenceNumberCalculator ?? (sequenceNumberCalculator = new HugeSequenceNumberGenerator(this)); }
		}

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this.Cast<ISequenceNumberLine>();

		HugeSequenceNumberGenerator sequenceNumberCalculator;
	}

	public interface IAsycudaPackedItemCollection<out TBillItem, out TBill> : IAsycudaBillPackedItemCollection<TBillItem, TBill>, IBusinessObjectCollection<TBillItem>
	where TBillItem : AsycudaPackedItem
	where TBill : AsycudaBill
	{
		new TBill Master { get; }
		new TBillItem AddNew(Type bizOType);
		new void Reload(bool reLoadExistingRows, bool assumeRowsMissingFromQueryResultsAreDeleted = false);
		new HugeSequenceNumberGenerator SequenceNumberCalculator { get; }
	}
}
