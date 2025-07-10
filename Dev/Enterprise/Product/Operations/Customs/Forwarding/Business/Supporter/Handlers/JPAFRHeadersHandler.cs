using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Business
{
	public class JPAFRHeadersHandler
	{
		public ICancellable[] Load(IBusiness parent, bool activeOnly)
		{
			var obj = (BusinessObject)parent;

			var filter = new ZQuery(JPAFRHeaderSchema.JPH_ParentId, obj.PK)
			{
				FetchOnlyFromLocalCache = !obj.IsInDatabase,
				IgnoreActiveFilter = !activeOnly,
			};

			filter.AddToFilter(JPAFRHeaderSchema.JPH_ParentTableCode, obj.TablePrefix);

			return parent.Factory
				.Load<Integration.Customs.JP.AFR.IJPAFRHeader>(filter)
				.ToArray<ICancellable>();
		}
	}
}
