using System;
using System.Collections.Generic;
using System.Linq;

namespace GlowIndexQueryService.Business;

public class BooleanQuery : IGlowQuery
{
	public BooleanQuery(BooleanOperator booleanOperator, params IGlowQuery[] queries)
	{
		this.booleanOperator = booleanOperator;
		this.innerGlowQueries = queries ?? throw new ArgumentNullException(nameof(queries));
	}

	public string ToUrlComponent()
	{
		var op = $" {booleanOperator.ToString().ToLower()} ";
		var portions = innerGlowQueries.Select(q => q?.ToUrlComponent()).Where(urlComponent => !string.IsNullOrEmpty(urlComponent)).ToList();

		if (portions.Count == 1)
		{
			return portions.First();
		}

		if (portions.Count > 1)
		{
			return $"({string.Join(op, portions)})";
		}
		return string.Empty;
	}

	public BooleanOperator BooleanOperator => booleanOperator;
	public IEnumerable<IGlowQuery> GlowQueries => innerGlowQueries;

	readonly BooleanOperator booleanOperator;
	readonly IEnumerable<IGlowQuery> innerGlowQueries;
}
