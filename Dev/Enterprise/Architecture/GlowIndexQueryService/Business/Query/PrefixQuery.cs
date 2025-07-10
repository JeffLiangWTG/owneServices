using System;

namespace GlowIndexQueryService.Business;

public class PrefixQuery : IGlowQuery
{
	public PrefixQuery(Term term)
	{
		this.term = term ?? throw new ArgumentNullException(nameof(term));
	}

	public string ToUrlComponent()
	{
		if (string.IsNullOrEmpty(term.Keyword))
		{
			return string.Empty;
		}
		return $"(startswith({term.SearchField},'{GlowIndexQueryParam.SanitiseInput(term.Keyword)}'))";
	}

	readonly Term term;
}
