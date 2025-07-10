using System;

namespace GlowIndexQueryService.Business;

public class NotEqualQuery : IGlowQuery
{
	public NotEqualQuery(Term term, bool useQuotes = true)
	{
		this.term = term ?? throw new ArgumentNullException(nameof(term));
		this.useQuotes = useQuotes;
	}

	readonly Term term;
	readonly bool useQuotes;

	public string ToUrlComponent()
	{
		if (string.IsNullOrEmpty(term.Keyword))
		{
			return string.Empty;
		}

		var searchField = term.SearchField;
		var keyword = GlowIndexQueryParam.SanitiseInput(term.Keyword);
		keyword = useQuotes ? $"'{keyword}'" : keyword;

		return $"({searchField} ne {keyword})";
	}
}
