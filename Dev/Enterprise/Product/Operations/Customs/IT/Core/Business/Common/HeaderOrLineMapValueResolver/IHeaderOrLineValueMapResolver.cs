using System;
using System.Collections.Generic;

namespace Enterprise.Customs.IT.Business;

public interface IHeaderOrLineValueMapResolver<THeader, TLine, TOut>
{
	TOut GetValueForHeader(THeader header);

	TOut GetValueForLine(TLine line);

	Func<TOut, bool> IsEmptyFunc { get; set; }

	bool UseFallBack { get; set; }

	IEqualityComparer<TOut> EqualityComparer { get; set; }
}
