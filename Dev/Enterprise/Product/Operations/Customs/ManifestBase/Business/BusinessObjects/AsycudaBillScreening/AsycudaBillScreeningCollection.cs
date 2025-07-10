using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public interface IAsycudaBillScreeningCollection<out B, out MasterB> : IDependentBusinessObjectCollection, IBusinessObjectCollection<B>
		where B : AsycudaBillScreening
		where MasterB : AsycudaBill
	{
		new MasterB Master { get; }
		new B this[int i] { get; }
		B AddNew(Type bizOType);
		void RefreshBinding();
		void Reload(bool reLoadExistingRows, bool assumeRowsMissingFromQueryResultsAreDeleted = false);
	}

	public class AsycudaBillScreeningCollection<B, MasterB> : DependentBusinessObjectCollection<B, MasterB>, IAsycudaBillScreeningCollection<B, MasterB>
		where B : AsycudaBillScreening
		where MasterB : AsycudaBill, IAsycudaBillScreeningTypeSupporter
	{
		public AsycudaBillScreeningCollection(MasterB master)
			: base(master, GetClusterKeyFilter(master))
		{
		}

		public IEnumerator<B> GetEnumerator() => Elements.Cast<B>().GetEnumerator();

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => Master.GetAsycudaBillScreeningType();
		protected override string FkColumnName => AsycudaBillScreeningSchema.ASR_ABL.Name;
		static ZQuery GetClusterKeyFilter(MasterB master) => new ZQuery(AsycudaBillScreeningSchema.ASR_ClusterKey, master.ABL_ClusterKey);
	}
}
