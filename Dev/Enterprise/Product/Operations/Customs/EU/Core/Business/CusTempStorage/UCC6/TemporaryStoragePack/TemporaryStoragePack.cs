using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class TemporaryStoragePack : AsycudaPack, IShortSequenceNumberLine
	{
		public TemporaryStoragePack(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return ReadOnly || (Bill?.Header?.IsNoEditAllowedCustomsStatus() ?? false) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("03410E4E-29A3-4332-A264-67328A07A355", "Storage Pack");

		public new TemporaryStorageBill Bill => (TemporaryStorageBill)base.Bill;

		public new TemporaryStorageContainer Container => (TemporaryStorageContainer)base.Container;

		#region APA_LineNo

		public override ZShort APA_LineNo
		{
			get { return base.APA_LineNo; }
			set
			{
				var oldValue = APA_LineNo;
				base.APA_LineNo = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					Bill?.Packs.SequenceGenerator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}

		#region IShortSequenceNumberLine

		ZGuid ISequenceNumberLine.FKToHeader => APA_ABL_Bill;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get { return APA_LineNo; }
			set { APA_LineNo = value; }
		}

		#endregion

		#endregion

		[RelatedBusinessObject("Bill")]
		public override ZGuid APA_ABL_Bill
		{
			get => base.APA_ABL_Bill;
			set
			{
				var oldValue = base.APA_ABL_Bill;
				base.APA_ABL_Bill = value;
				if (!IsCopying && oldValue != APA_ABL_Bill)
				{
					if (!APA_ABL_Bill.IsValid)
					{
						DetachedFromBill(oldValue);
					}
					else
					{
						AttachedToBill();
					}
				}
			}
		}

		void DetachedFromBill(ZGuid oldValue)
		{
			var bill = Factory.Load<TemporaryStorageBill>(oldValue);
			if (bill != null)
			{
				bill.Packs.SequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		void AttachedToBill()
		{
			var bill = Bill;
			if (bill != null)
			{
				bill.Packs.SequenceGenerator.RecalculateWhenAdded(this);
			}
		}

		public new TemporaryStoragePackLookups Lookups => (TemporaryStoragePackLookups)base.Lookups;

		protected override AsycudaPackLookups GetNewLookups() => new TemporaryStoragePackLookups(this);

		protected override AsycudaPackValidation GetNewValidation()
		{
			return new TemporaryStoragePackValidation(this);
		}

		protected override Type GetPackedItemTypeCore() => typeof(TemporaryStoragePackedItem);

		#region Cloning and copying

		protected override bool SupportsCloneCore() => true;

		#endregion

		internal bool IsBulk => Factory.GetValue(ref isBulkCached, () => Lookups.BulkOnlyPackingUnitTypesList.ContainsCode(APA_PackUQ));
		CachedProperty<bool> isBulkCached;

		internal bool IsBreakBulk => Factory.GetValue(ref isBreakBulkCached, () => Lookups.BreakBulkOnlyPackingUnitTypesList.ContainsCode(APA_PackUQ));
		CachedProperty<bool> isBreakBulkCached;
	}
}
