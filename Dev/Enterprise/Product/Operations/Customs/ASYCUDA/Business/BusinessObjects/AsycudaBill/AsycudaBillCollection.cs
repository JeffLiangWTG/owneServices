using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public interface IAsycudaBillCollection<out TBill, out TMasterB> : ManifestBase.IAsycudaBillCollection<TBill, TMasterB>, ISequenceNumberHeader
		where TBill : AsycudaBill
		where TMasterB : AsycudaManifestHeader
	{
		new TBill this[int i] { get; }
		ShortSequenceNumberGenerator SequenceGenerator { get; }
	}

	public class AsycudaBillCollection<B, MasterB> : ManifestBase.AsycudaBillCollection<B, MasterB>, IAsycudaBillCollection<B, MasterB>
		where B : AsycudaBill
		where MasterB : AsycudaManifestHeader
	{
		public AsycudaBillCollection(MasterB master)
			: base(master, new ZQuery(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode))
		{
		}

		public AsycudaBillCollection(MasterB master, ZQuery additionalFilter)
			: base(master, additionalFilter)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			using (child.SuspendSettingHasChanges())
			{
				if (Master != null)
				{
					var bill = (AsycudaBill)child;
					if (!Master.AMA_RL_NKPortOfLoading.IsEmpty)
					{
						bill.ABL_RL_NKOrigin = Master.AMA_RL_NKPortOfLoading;
					}
					if (!Master.AMA_RL_NKPortOfDischarge.IsEmpty)
					{
						bill.ABL_RL_NKFinalDestination = Master.AMA_RL_NKPortOfDischarge;
					}
				}
			}
		}

		public ShortSequenceNumberGenerator SequenceGenerator => fSequenceGenerator ?? (fSequenceGenerator = new ShortSequenceNumberGenerator(this));
		ShortSequenceNumberGenerator fSequenceGenerator;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this.Cast<ISequenceNumberLine>();
	}

	sealed class AsycudaBillSynchronisationTargetCollection : ISailingSynchronisationTargetCollection<BillOfLading, AsycudaBill>
	{
		public AsycudaBillSynchronisationTargetCollection(IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> targetCollection)
		{
			this.targetCollection = targetCollection;
		}

		readonly IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> targetCollection;

		#region ISailingSynchronisationTargetCollection

		AsycudaBill ISailingSynchronisationTargetCollection<BillOfLading, AsycudaBill>.AddNew()
		{
			return targetCollection.AddNew();
		}

		void ISailingSynchronisationTargetCollection<BillOfLading, AsycudaBill>.Delete(AsycudaBill target)
		{
			targetCollection.RemoveAndDelete(target);
		}

		public IEnumerator<AsycudaBill> GetEnumerator()
		{
			return targetCollection.Cast<AsycudaBill>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
