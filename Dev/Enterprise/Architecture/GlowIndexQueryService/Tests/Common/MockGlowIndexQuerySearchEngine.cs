using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace GlowIndexQueryService.Tests.Common
{
	public class MockGlowIndexQuerySearchEngine : IGlowIndexQueryEngine
	{
		public GlowIndexQueryResultCollection Results { get; set; } = new GlowIndexQueryResultCollection();
		public SearchFieldCollection SearchField { get; set; } = new SearchFieldCollection("dummyEntity", Array.Empty<SearchField>());

		public IEnumerable<ZGuid> Pks { get; } = new List<ZGuid>();
		public ISet<string> GlowEntityTypes { get; set; } = new HashSet<string>();
		public ISet<string> GlowEntityCategories { get; set; } = new HashSet<string>();

		public SearchFieldCollection GetSearchFields(string entityType)
			=> SearchField;

		public ISet<string> GetGlowEntityTypes()
			=> GlowEntityTypes;

		public ISet<string> GetGlowEntityCategories()
			=> GlowEntityCategories;

		public GlowIndexQueryResultCollection Query(GlowIndexQueryParam queryParam)
			=> Results;

		IEnumerable<ZGuid> IGlowIndexQueryEngine.QueryPk(GlowIndexQueryParam queryParam)
			=> Pks;

		public CodeDescriptionPairList GetListByRuleId(Guid lookupRuleId) => new CodeDescriptionPairList();
	}
}
