using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface IAsycudaPackCollection<out TPack, out TBill> : ManifestBase.IAsycudaPackCollection<TPack, TBill>, ISequenceNumberHeader
		where TPack : AsycudaPack
		where TBill : AsycudaBill
	{
		new TPack this[int index] { get; }
		ShortSequenceNumberGenerator SequenceGenerator { get; }
	}

	public class AsycudaPackCollection<TPack, TBill> : ManifestBase.AsycudaPackCollection<TPack, TBill>, IAsycudaPackCollection<TPack, TBill>
		where TPack : AsycudaPack
		where TBill : AsycudaBill
	{
		public AsycudaPackCollection(TBill master)
			: base(master)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var header = Master.Header;
			if (header != null
				&& header.Containers.Count == 1
				&& ShouldDefaultContainerPK(header))
			{
				var pack = (AsycudaPack)child;
				pack.ContainerPK = header.Containers[0].PK;
			}
		}

		protected virtual bool ShouldDefaultContainerPK(AsycudaManifestHeader header)
		{
			return header.IsContainerized;
		}

		ShortSequenceNumberGenerator IAsycudaPackCollection<TPack, TBill>.SequenceGenerator => fSequenceGenerator ?? (fSequenceGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator fSequenceGenerator;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this.Cast<ISequenceNumberLine>();

		#region Fetch Strategy
		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new CollectionFetchStrategy(this);
		}

		class CollectionFetchStrategy : AsycudaCollectionFetchStrategy<AsycudaPackCollection<TPack, TBill>>
		{
			public CollectionFetchStrategy(AsycudaPackCollection<TPack, TBill> collection)
				: base(collection)
			{
			}

			protected override void FetchForViewAsycuda(BusinessObject[] businessObjects, TableColumn[] columns)
			{
				businessObjects = businessObjects.Where(x => x.IsInDatabase).ToArray();
				if (businessObjects.Length > 0)
				{
					var asycudaPackageContainerLinkRequiredFetchForView = false;
					var genAddOnColumnRequiredFetchForView = false;
					foreach (TableColumn tableColumn in columns)
					{
						if (IsAsycudaPackageContainerLinkRelatedColumn(tableColumn.ColumnName))
						{
							asycudaPackageContainerLinkRequiredFetchForView = true;
						}
						if (IsGenAddOnColumnRelatedColumn(tableColumn.ColumnName))
						{
							genAddOnColumnRequiredFetchForView = true;
						}
					}

					var factory = Collection.Factory;
					if (asycudaPackageContainerLinkRequiredFetchForView || genAddOnColumnRequiredFetchForView)
					{
						foreach (AsycudaPack pack in businessObjects)
						{
							if (asycudaPackageContainerLinkRequiredFetchForView)
							{
								factory.AddFetchHint(AsycudaContainerBillOrPackageLinkSchema.APC_APA_Pack, pack.PK);
							}
							if (genAddOnColumnRequiredFetchForView)
							{
								factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, pack.PK);
							}
						}
					}
				}
			}

			bool IsGenAddOnColumnRelatedColumn(string columnName)
			{
				return columnName == AsycudaPack.Schema.ConsignmentReference;
			}

			bool IsAsycudaPackageContainerLinkRelatedColumn(string columnName)
			{
				return columnName == AsycudaPack.Schema.ContainerPK;
			}
		}
		#endregion
	}

	sealed class AsycudaPackSynchronisationTargetCollection : ISailingSynchronisationTargetCollection<BillOfLadingPackLine, AsycudaPack>
	{
		public AsycudaPackSynchronisationTargetCollection(IAsycudaPackCollection<AsycudaPack, AsycudaBill> targetCollection)
		{
			this.targetCollection = targetCollection;
		}

		readonly IAsycudaPackCollection<AsycudaPack, AsycudaBill> targetCollection;

		#region ISailingSynchronisationTargetCollection

		AsycudaPack ISailingSynchronisationTargetCollection<BillOfLadingPackLine, AsycudaPack>.AddNew()
		{
			return targetCollection.AddNew();
		}

		void ISailingSynchronisationTargetCollection<BillOfLadingPackLine, AsycudaPack>.Delete(AsycudaPack target)
		{
			targetCollection.RemoveAndDelete(target);
		}

		public IEnumerator<AsycudaPack> GetEnumerator()
		{
			return targetCollection.Cast<AsycudaPack>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
