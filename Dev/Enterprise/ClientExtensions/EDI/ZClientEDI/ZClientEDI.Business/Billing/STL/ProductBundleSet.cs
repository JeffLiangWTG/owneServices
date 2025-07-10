using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ProductBundleSet
	{
		public void Init(IEnumerable<StlMonthlyUsage> stlMonthlyUsages)
		{
			OrgProductMap = stlMonthlyUsages
						.Where(x => x.Database != null
						&& !x.Database.LD_OH_WebAccessOrg.IsEmpty
						&& x.Database.LD_LicenceType == DatabaseTypes.Codes.Production
						&& x.Database.IsBillable)
						.Select(x => new
						{
							x.Database.LD_OH_WebAccessOrg,
							Databases = new[] { x.Database }
										.Concat(x.Usages.Select(u => u.Database)
														.Where(d => d != null
														&& d.LD_Product == ProductTypes.Codes.BorderWise
														&& d.LD_LicenceType == DatabaseTypes.Codes.Production
														&& d.IsBillable))
										.DistinctBy(d => d.PK)
						})
						.GroupBy(x => x.LD_OH_WebAccessOrg)
						.ToDictionary(k => k.Key, v => v.SelectMany(x => x.Databases)
														.DistinctBy(x => x.PK)
														.Select(x => x.LD_Product)
														.ToArray()
														.AsEnumerable());
		}

		public IEnumerable<ZString> GetProductBundleByOrg(ZGuid orgPk)
		{
			return OrgProductMap.TryGetValue(orgPk, out var productBundle) ? productBundle : Enumerable.Empty<ZString>();
		}

		Dictionary<ZGuid, IEnumerable<ZString>> OrgProductMap = new Dictionary<ZGuid, IEnumerable<ZString>>();
	}
}
