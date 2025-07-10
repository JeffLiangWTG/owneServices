using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace GlowIndexQueryService.Business
{
	public interface IGlowIndexQueryEngine
	{
		GlowIndexQueryResultCollection Query(GlowIndexQueryParam queryParam);
		IEnumerable<ZGuid> QueryPk(GlowIndexQueryParam queryParam);
		SearchFieldCollection GetSearchFields(string entityType);
		ISet<string> GetGlowEntityTypes();
		ISet<string> GetGlowEntityCategories();
		CodeDescriptionPairList GetListByRuleId(Guid lookupRuleId);
	}
}
