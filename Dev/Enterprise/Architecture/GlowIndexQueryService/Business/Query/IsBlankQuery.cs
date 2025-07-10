using System;

namespace GlowIndexQueryService.Business;

public class IsBlankQuery : IGlowQuery
{
	public IsBlankQuery(Term term, bool includeEmptyString = true)
	{
		this.term = term ?? throw new ArgumentNullException(nameof(term));
		this.includeEmptyString = includeEmptyString;
	}

	public Term Term => term;

	readonly Term term;
	readonly bool includeEmptyString;

	public string ToUrlComponent()
	{
		var searchField = term.SearchField;
		if (includeEmptyString)
		{
			return $"(({searchField} eq null) or ({searchField} eq ''))";
		}
		return $"({searchField} eq null)";
	}
}
