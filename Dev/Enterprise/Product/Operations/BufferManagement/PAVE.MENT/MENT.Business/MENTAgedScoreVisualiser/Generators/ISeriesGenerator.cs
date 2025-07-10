using System.Collections.Generic;
using OxyPlot.Series;

namespace Enterprise.PAVE.MENT.Business
{
	interface ISeriesGenerator
	{
		IEnumerable<Series> GenerateSeries();
	}
}
