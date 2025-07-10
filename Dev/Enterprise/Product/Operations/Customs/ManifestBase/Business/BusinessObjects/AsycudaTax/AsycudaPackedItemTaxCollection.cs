using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public interface IAsycudaPackedItemTaxCollection<out B, out MasterB> : IDependentBusinessObjectCollection, IBusinessObjectCollection<B>
		where B : AsycudaTax
		where MasterB : AsycudaPackedItem
	{
		new MasterB Master { get; }
		new B this[int i] { get; }
		B AddNew(Type bizOType);
		void RefreshBinding();
		void Reload(bool reLoadExistingRows, bool assumeRowsMissingFromQueryResultsAreDeleted = false);
	}

	public class AsycudaPackedItemTaxCollection<B, MasterB> : DependentBusinessObjectCollection<B, MasterB>, IAsycudaPackedItemTaxCollection<B, MasterB>
		where B : AsycudaTax
		where MasterB : AsycudaPackedItem, IAsycudaTaxTypeSupporter
	{
		public AsycudaPackedItemTaxCollection(MasterB master)
			: base(master, GetClusterKeyFilter(master))
		{
		}

		public IEnumerator<B> GetEnumerator() => Elements.Cast<B>().GetEnumerator();
		public override Type GetTypeOfElementsFromPK(ZGuid pk) => Master.GetAsycudaTaxType();

		protected override string FkColumnName => AsycudaTaxSchema.AET_API_AsycudaPackedItem.Name;
		static ZQuery GetClusterKeyFilter(MasterB master) => new ZQuery(AsycudaTaxSchema.AET_ClusterKey, master.API_ClusterKey);
	}
}


