using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Module
{
	public class EDIInterchangeEHubIdFilter : ModuleTextFilter
	{
		internal EDIInterchangeEHubIdFilter(ZString description)
			: base(description, GetQuery)
		{
		}

		static ZQuery GetQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery interchangeQuery = new ZDBOnlyQuery(typeof(EDIInterchange));
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, Guid.Parse(value));
			return interchangeQuery;
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new EDIInterchangeEHubIdFilterValidation(this);
		}
	}
}
