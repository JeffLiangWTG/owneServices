using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	class AsycudaContainerBillOrPackageLinkRowFetchStrategy : IRowFetchStrategy
	{
		#region IRowFetchStrategy members

		void IRowFetchStrategy.FetchForLoad(BusinessObjectFactory factory, DataRow[] rows)
		{
			foreach (var row in rows)
			{
				factory.AddFetchHint(AsycudaPackSchema.PK, new ZGuid(row[AsycudaContainerBillOrPackageLink.Schema.APC_APA_Pack]));
				factory.AddFetchHint(AsycudaBillSchema.PK, new ZGuid(row[AsycudaContainerBillOrPackageLink.Schema.APC_ABL_Bill]));
			}
		}

		#endregion
	}
}
