namespace GlowIndexQueryService.Business;

public class RangeQuery : IGlowQuery
{
	public RangeQuery(Term begin, Term end, bool includeBegin = true, bool includeEnd = true)
	{
		this.begin = begin;
		this.end = end;
		this.beginOp = includeBegin ? $"ge" : $"gt";
		this.endOp = includeEnd ? $"le" : $"lt";
	}

	public string ToUrlComponent()
	{
		var left = (begin == null || string.IsNullOrEmpty(begin.Keyword)) ? string.Empty : $"({begin.SearchField} {beginOp} {begin.Keyword})";
		var right = (end == null || string.IsNullOrEmpty(end.Keyword)) ? string.Empty : $"({end.SearchField} {endOp} {end.Keyword})";

		if (!string.IsNullOrEmpty(left) && !string.IsNullOrEmpty(right))
		{
			return $"({left} and {right})";
		}
		else
		{
			return left + right;
		}
	}

	readonly Term begin;
	readonly Term end;
	readonly string beginOp;
	readonly string endOp;
}
