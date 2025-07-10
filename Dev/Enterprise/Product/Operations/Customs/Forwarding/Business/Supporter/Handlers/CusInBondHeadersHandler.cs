using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Business
{
	public class CusInBondHeadersHandler
	{
		public ICancellable[] Load(IBusiness parent, bool activeOnly)
		{
			var obj = (BusinessObject)parent;

			var filter = new ZQuery(CusInBondHeaderSchema.BH_ParentID, obj.PK)
			{
				FetchOnlyFromLocalCache = !obj.IsInDatabase,
				IgnoreActiveFilter = !activeOnly,
			};

			filter.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, obj.TablePrefix);

			return parent.Factory
				.Load<Integration.Customs.ICusInBondHeader>(filter)
				.ToArray<ICancellable>();
		}
	}
}
