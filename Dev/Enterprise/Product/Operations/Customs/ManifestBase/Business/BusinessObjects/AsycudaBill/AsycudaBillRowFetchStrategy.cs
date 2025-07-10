using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	class AsycudaBillRowFetchStrategy : IRowFetchStrategy
	{
		#region IRowFetchStrategy members

		void IRowFetchStrategy.FetchForLoad(BusinessObjectFactory factory, DataRow[] rows)
		{
			foreach (var row in rows)
			{
				factory.AddFetchHint(AsycudaManifestHeaderSchema.PK, new ZGuid(row[AsycudaBill.Schema.ABL_AMA]));
			}
		}

		#endregion
	}
}
