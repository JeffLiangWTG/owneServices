using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class AccChargeCodeCompanyFilterProvider : ICompanyFilterProvider
	{
		public List<ZQuery> GetCompanyFilters(ICompanyFilterProviderContext context, Guid? companyPK)
		{
			var queries = new List<ZQuery>()
			{
				new ZQuery(AccChargeCodeSchema.AC_GC, companyPK),
				new ZQuery(AccChargeCodeSchema.AC_GC, null),
			};

			if (context != null && !context.IsLocalFirst)
			{
				queries.Reverse();
			}

			return queries;
		}
	}
}
