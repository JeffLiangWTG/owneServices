using System;

namespace GlowIndexQueryService.Business;

public class IsNotBlankQuery : IGlowQuery
{
	public IsNotBlankQuery(Term term, bool includeEmptyString = true)
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
			return $"(({searchField} ne null) and ({searchField} ne ''))";
		}
		return $"({searchField} ne null)";
	}
}
