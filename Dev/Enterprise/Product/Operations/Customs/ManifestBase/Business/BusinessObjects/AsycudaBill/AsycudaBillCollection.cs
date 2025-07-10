using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaBillCollection<TBill, TManifestHeader> : DependentBusinessObjectCollection<TBill, TManifestHeader>, IAsycudaBillCollection<TBill, TManifestHeader>
		where TBill : AsycudaBill
		where TManifestHeader : AsycudaManifestHeader
	{
		public AsycudaBillCollection(TManifestHeader master)
			: base(master)
		{
		}

		public AsycudaBillCollection(TManifestHeader master, ZQuery additionalFilter)
			: base(master, additionalFilter)
		{
		}

		public IEnumerator<TBill> GetEnumerator() => Elements.Cast<TBill>().GetEnumerator();

		public IEnumerable<TBill> AsEnumerable() => this;

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => Master.GetBillType();
	}

	public interface IAsycudaBillCollection<out TBill, out TManifestHeader> : IDependentBusinessObjectCollection, IBusinessObjectCollection<TBill>
		where TBill : AsycudaBill
		where TManifestHeader : AsycudaManifestHeader
	{
		new TBill this[int i] { get; }
		new TManifestHeader Master { get; }
		void RefreshBinding();
		new void Remove(BusinessObject bill);
		new TBill AddNew();
		TBill AddNew(Type bizOType);
		void Reload(bool reLoadExistingRows, bool assumeRowsMissingFromQueryResultsAreDeleted = false);
	}
}
