using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaBillPackedItemCollection<TPackedItem, TBill> : DependentBusinessObjectCollection<TPackedItem, TBill>, IAsycudaBillPackedItemCollection<TPackedItem, TBill>, ISequenceNumberHeader
		where TPackedItem : AsycudaPackedItem
		where TBill : AsycudaBill
	{
		public AsycudaBillPackedItemCollection(TBill bill) : base(bill)
		{
		}

		public IEnumerator<TPackedItem> GetEnumerator() => Elements.Cast<TPackedItem>().GetEnumerator();

		protected override SchemaGuidColumn FKSchemaColumnInDependent => AsycudaPackedItemSchema.API_ABL_Bill;

		public HugeSequenceNumberGenerator SequenceNumberCalculator => sequenceNumberCalculator ??= SequenceNumberCalculatorCore;
		HugeSequenceNumberGenerator sequenceNumberCalculator;

		protected virtual HugeSequenceNumberGenerator SequenceNumberCalculatorCore => new HugeSequenceNumberGenerator(this);

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			SequenceNumberCalculator.ReCalculateAll();
		}

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this.Cast<ISequenceNumberLine>();
	}

	public interface IAsycudaBillPackedItemCollection<out TPackedItem, out TBill> : IDependentBusinessObjectCollection, IBusinessObjectCollection<TPackedItem>
		where TPackedItem : AsycudaPackedItem
		where TBill : AsycudaBill
	{
		new TBill Master { get; }
		TPackedItem AddNew(Type bizOType);
		void Reload(bool reLoadExistingRows, bool assumeRowsMissingFromQueryResultsAreDeleted = false);
		HugeSequenceNumberGenerator SequenceNumberCalculator { get; }
	}
}
