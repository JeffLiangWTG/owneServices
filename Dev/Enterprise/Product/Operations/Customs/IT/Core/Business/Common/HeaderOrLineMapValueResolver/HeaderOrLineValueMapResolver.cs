using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.IT.Business;

sealed class HeaderOrLineValueMapResolver<THeader, TLine, TOut> : IHeaderOrLineValueMapResolver<THeader, TLine, TOut>
{
	public HeaderOrLineValueMapResolver(Func<THeader, IEnumerable<TLine>> linesGetter,
		Func<TLine, THeader> headerGetter,
		Func<THeader, TOut> headerValueGetter,
		Func<TLine, TOut> lineValueGetter)
	{
		_linesGetter = Argument.NotNull(linesGetter, nameof(linesGetter));
		_headerGetter = Argument.NotNull(headerGetter, nameof(headerGetter));
		_headerValueGetter = Argument.NotNull(headerValueGetter, nameof(headerValueGetter));
		_lineValueGetter = Argument.NotNull(lineValueGetter, nameof(lineValueGetter));
	}

	public Func<TOut, bool> IsEmptyFunc { get; set; }
	public bool UseFallBack { get; set; }
	public IEqualityComparer<TOut> EqualityComparer { get; set; }

	public TOut GetValueForHeader(THeader header)
	{
		Argument.NotNull(header, nameof(header));

		var lineCollection = _linesGetter(header);
		if (!lineCollection.Any())
		{
			return _headerValueGetter(header);
		}

		return LinesHaveTheSameValue(header)
			? GetSingleValue(header)
			: default;
	}

	public TOut GetValueForLine(TLine line)
	{
		Argument.NotNull(line, nameof(line));

		var header = _headerGetter(line);
		return !LinesHaveTheSameValue(header)
			? LineValueSelectionOrFallback(line)
			: default;
	}

	#region Implementation

	bool LinesHaveTheSameValue(THeader header)
	{
		var lineCollection = _linesGetter(header);
		var values = lineCollection.Select(x => LineValueSelectionOrFallback(x));

		values = EqualityComparer is null
			? values.Distinct()
			: values.Distinct(EqualityComparer);

		return !values.Skip(1).Any();
	}

	TOut GetSingleValue(THeader header)
	{
		var lineCollection = _linesGetter(header);
		return lineCollection
			.Select(l => LineValueSelectionOrFallback(l))
			.FirstOrDefault();
	}

	TOut LineValueSelectionOrFallback(TLine line)
	{
		var lineValue = _lineValueGetter(line);

		return IsEmpty(lineValue) && UseFallBack
			? GetValueFromHeader()
			: lineValue;

		TOut GetValueFromHeader()
		{
			var header = _headerGetter(line);
			return _headerValueGetter(header);
		}
	}

	bool IsEmpty(TOut value)
	{
		if (IsEmptyFunc is null)
		{
			return value == null;
		}

		return IsEmptyFunc(value);
	}

	readonly Func<THeader, IEnumerable<TLine>> _linesGetter;
	readonly Func<TLine, THeader> _headerGetter;
	readonly Func<THeader, TOut> _headerValueGetter;
	readonly Func<TLine, TOut> _lineValueGetter;

	#endregion
}
