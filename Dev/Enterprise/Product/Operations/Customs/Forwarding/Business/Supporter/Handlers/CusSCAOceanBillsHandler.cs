using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Business
{
	public class CusSCAOceanBillsHandler
	{
		public ICancellable[] Load(IBusiness parent, bool activeOnly)
		{
			var obj = (BusinessObject)parent;

			var filter = new ZQuery(CusSCAOceanBillSchema.CB_ParentId, obj.PK)
			{
				FetchOnlyFromLocalCache = !obj.IsInDatabase,
				IgnoreActiveFilter = !activeOnly,
			};
			filter.AddToFilter(CusSCAOceanBillSchema.CB_ParentTableCode, JobConsolSchema.Constants.Prefix);

			return parent.Factory
				.Load<Integration.Customs.Shared.IBaseCusSCAOceanBill>(filter)
				.ToArray<ICancellable>();
		}
	}
}
