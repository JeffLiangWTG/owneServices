using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Business
{
	public class AsycudaManifestHeadersHandler
	{
		public ICancellable[] Load(IBusiness parent, bool activeOnly)
		{
			var obj = (BusinessObject)parent;

			var filter = new ZQuery(AsycudaManifestHeaderSchema.AMA_ParentId, obj.PK)
			{
				FetchOnlyFromLocalCache = !obj.IsInDatabase,
				IgnoreActiveFilter = !activeOnly,
			};

			filter.AddToFilter(AsycudaManifestHeaderSchema.AMA_ParentTableCode, obj.TablePrefix);

			return parent.Factory
				.Load<Integration.Customs.ManifestBase.IAsycudaManifestHeader>(filter)
				.ToArray<ICancellable>();
		}
	}
}
