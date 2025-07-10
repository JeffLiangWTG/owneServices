using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	class AsycudaPackRowFetchStrategy : IRowFetchStrategy
	{
		#region IRowFetchStrategy members

		void IRowFetchStrategy.FetchForLoad(BusinessObjectFactory factory, DataRow[] rows)
		{
			foreach (var row in rows)
			{
				factory.AddFetchHint(AsycudaBillSchema.PK, new ZGuid(row[AsycudaPack.Schema.APA_ABL_Bill]));
			}
		}

		#endregion
	}
}
