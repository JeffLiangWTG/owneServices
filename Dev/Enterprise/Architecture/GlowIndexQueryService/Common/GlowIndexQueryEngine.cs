using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace GlowIndexQueryService.Common
{
	public class GlowIndexQueryEngine : IGlowIndexQueryEngine
	{
		public GlowIndexQueryResultCollection Query(GlowIndexQueryParam queryParam)
			=> GlowIndexQueryImpl.Query(queryParam);

		public IEnumerable<ZGuid> QueryPk(GlowIndexQueryParam queryParam)
			=> GlowIndexQueryImpl.QueryPKs(queryParam);

		public SearchFieldCollection GetSearchFields(string entityType)
			=> GlowIndexQueryImpl.GetSearchFields(entityType);

		public ISet<string> GetGlowEntityTypes()
			=> GlowIndexQueryImpl.GetGlowEntityTypes();

		public ISet<string> GetGlowEntityCategories()
			=> GlowIndexQueryImpl.GetGlowEntityCategories();

		public CodeDescriptionPairList GetListByRuleId(Guid lookupRuleId)
			=> GlowIndexQueryImpl.GetListByRuleId(lookupRuleId);
	}
}
