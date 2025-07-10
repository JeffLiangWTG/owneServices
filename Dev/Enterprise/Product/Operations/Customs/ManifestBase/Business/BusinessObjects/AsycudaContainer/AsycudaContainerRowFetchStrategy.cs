using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	class AsycudaContainerRowFetchStrategy : IRowFetchStrategy
	{
		#region IRowFetchStrategy members

		void IRowFetchStrategy.FetchForLoad(BusinessObjectFactory factory, DataRow[] rows)
		{
			foreach (var row in rows)
			{
				factory.AddFetchHint(AsycudaManifestHeaderSchema.PK, new ZGuid(row[AsycudaContainer.Schema.ACN_AMA_Manifest]));
			}
		}

		#endregion
	}
}
