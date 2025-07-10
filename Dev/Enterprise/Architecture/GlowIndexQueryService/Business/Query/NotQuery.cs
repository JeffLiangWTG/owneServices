using System;

namespace GlowIndexQueryService.Business;

public class NotQuery : IGlowQuery
{
	public NotQuery(IGlowQuery query)
	{
		innerQuery = query ?? throw new ArgumentNullException(nameof(query));
	}

	readonly IGlowQuery innerQuery;
	public string ToUrlComponent()
	{
		var innerQueryUrl = innerQuery.ToUrlComponent();
		if (string.IsNullOrEmpty(innerQueryUrl))
		{
			return string.Empty;
		}
		return $"(not{innerQueryUrl})";
	}
}
