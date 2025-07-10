using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTDataRow
	{
		public MENTDataRow(RowSegmentDefinitionAndData[] xSeriesObjects, RowSegmentDefinitionAndData[] xCategoryObjects, ZDecimal yValue)
		{
			this.XSeries = xSeriesObjects;
			this.XCategory = xCategoryObjects;
			this.YValue = yValue;
		}

		[SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
		public readonly RowSegmentDefinitionAndData[] XSeries;

		[SuppressMessage("Microsoft.Security", "CA2105:ArrayFieldsShouldNotBeReadOnly")]
		public readonly RowSegmentDefinitionAndData[] XCategory;
		public readonly ZDecimal YValue;

		public string XSeriesString
		{
			get { return RowSegmentAggregator(XSeries); }
		}

		public string XCategoryString
		{
			get { return RowSegmentAggregator(XCategory); }
		}

		Func<RowSegmentDefinitionAndData[], string> RowSegmentAggregator
		{
			get { return new Func<RowSegmentDefinitionAndData[], string>(c => c.OrderBy(o => o.Order).Select(x => x.Value.ToStringWithFormatForMENT()).Aggregate((current, next) => current + "-" + next)); }
		}
	}
}
