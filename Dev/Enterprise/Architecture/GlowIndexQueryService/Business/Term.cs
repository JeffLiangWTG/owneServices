using System;

namespace GlowIndexQueryService.Business;

public class Term : IEquatable<Term>
{
	public Term(string searchField, string keyword)
	{
		SearchField = string.IsNullOrEmpty(searchField) ? throw new ArgumentException("'searchField' must be provided", nameof(searchField)) : searchField;
		Keyword = keyword;
	}

	public string SearchField { get; }
	public string Keyword { get; }

	#region Equality members

	public bool Equals(Term other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return SearchField == other.SearchField && Keyword == other.Keyword;
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

		return Equals((Term)obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return ((SearchField != null ? SearchField.GetHashCode() : 0) * 397) ^ (Keyword != null ? Keyword.GetHashCode() : 0);
		}
	}

	#endregion
}
