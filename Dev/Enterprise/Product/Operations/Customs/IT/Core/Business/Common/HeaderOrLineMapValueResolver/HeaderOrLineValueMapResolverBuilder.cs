using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Customs.IT.Business;

public sealed class HeaderOrLineValueMapResolverBuilder<THeader, TLine, TOut>
{
	internal HeaderOrLineValueMapResolverBuilder()
	{
	}

	public HeaderOrLineValueMapResolverBuilder<THeader, TLine, TOut> SetLineGetter(Func<THeader, IEnumerable<TLine>> lineGetter)
	{
		_linesGetter = Argument.NotNull(lineGetter, nameof(lineGetter));
		return this;
	}

	public HeaderOrLineValueMapResolverBuilder<THeader, TLine, TOut> SetHeaderGetter(Func<TLine, THeader> headerGetter)
	{
		_headerGetter = Argument.NotNull(headerGetter, nameof(headerGetter));
		return this;
	}

	public HeaderOrLineValueMapResolverBuilder<THeader, TLine, TOut> SetLineValueGetter(Func<TLine, TOut> lineValueGetter)
	{
		_lineValueGetter = Argument.NotNull(lineValueGetter, nameof(lineValueGetter));
		return this;
	}

	public HeaderOrLineValueMapResolverBuilder<THeader, TLine, TOut> SetHeaderValueGetter(Func<THeader, TOut> headerValueGetter)
	{
		_headerValueGetter = Argument.NotNull(headerValueGetter, nameof(headerValueGetter));
		return this;
	}

	public HeaderOrLineValueMapResolverBuilder<THeader, TLine, TOut> WithEqualityComparer(IEqualityComparer<TOut> equalityComparer)
	{
		_equalityComparer = equalityComparer;
		return this;
	}

	public HeaderOrLineValueMapResolverBuilder<THeader, TLine, TOut> IsEmptyWhen(Func<TOut, bool> isEmptyFunc)
	{
		_isEmptyFunc = isEmptyFunc;
		return this;
	}

	public HeaderOrLineValueMapResolverBuilder<THeader, TLine, TOut> UseFallBack()
	{
		_useFallBack = true;
		return this;
	}

	public IHeaderOrLineValueMapResolver<THeader, TLine, TOut> Build()
	{
		if (_linesGetter is null ||
			_headerGetter is null ||
			_headerValueGetter is null ||
			_lineValueGetter is null)
		{
			throw new InvalidOperationException("Builder configuration is incomplete. Please make sure to set values for LineValueGetter, HeaderGetter, HeaderValueGetter, and LineGetter before building.");
		}

		var result = new HeaderOrLineValueMapResolver<THeader, TLine, TOut>(_linesGetter, _headerGetter, _headerValueGetter, _lineValueGetter)
		{
			EqualityComparer = _equalityComparer,
			IsEmptyFunc = _isEmptyFunc,
			UseFallBack = _useFallBack
		};

		Clear();

		return result;
	}

	public void Clear()
	{
		_linesGetter = null;
		_headerGetter = null;
		_lineValueGetter = null;
		_headerValueGetter = null;
		_equalityComparer = null;
		_isEmptyFunc = null;
		_useFallBack = false;
	}

	Func<THeader, IEnumerable<TLine>> _linesGetter;
	Func<TLine, THeader> _headerGetter;
	Func<THeader, TOut> _headerValueGetter;
	Func<TLine, TOut> _lineValueGetter;
	IEqualityComparer<TOut> _equalityComparer;
	Func<TOut, bool> _isEmptyFunc;
	bool _useFallBack;
}
