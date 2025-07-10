using System;

namespace GlowIndexQueryService.Business;

public class EqualQuery : IGlowQuery, IEquatable<EqualQuery>
{
	public EqualQuery(Term term, bool useQuotes = true, bool allExact = false)
	{
		this.term = term ?? throw new ArgumentNullException(nameof(term));
		this.useQuotes = useQuotes;
		this.allExact = allExact;
	}

	readonly Term term;
	readonly bool useQuotes;
	readonly bool allExact;

	public string ToUrlComponent()
	{
		if (string.IsNullOrEmpty(term.Keyword))
		{
			return string.Empty;
		}
		var searchField = term.SearchField;
		var keyword = GlowIndexQueryParam.SanitiseInput(term.Keyword);
		keyword = allExact ? $"\"{keyword}\"" : keyword;
		keyword = useQuotes ? $"'{keyword}'" : keyword;
		return $"({searchField} eq {keyword})";
	}

	#region Equality members

	public bool Equals(EqualQuery other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return Equals(term, other.term) && useQuotes == other.useQuotes && allExact == other.allExact;
	}

	public override bool Equals(object obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((EqualQuery)obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = (term != null ? term.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ useQuotes.GetHashCode();
			hashCode = (hashCode * 397) ^ allExact.GetHashCode();
			return hashCode;
		}
	}

	#endregion
}
