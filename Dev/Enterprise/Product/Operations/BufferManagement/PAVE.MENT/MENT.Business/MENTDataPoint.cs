
namespace Enterprise.PAVE.MENT.Business
{
	class MENTDataPoint
	{
		public MENTDataPoint(string xCategoryLabel, decimal yValue, string xSeriesLabel, string dataSetLabel)
		{
			this.xCategoryLabel = xCategoryLabel;
			this.yValue = yValue;
			this.xSeriesLabel = xSeriesLabel;
			this.dataSetLabel = dataSetLabel;
		}

		readonly string xCategoryLabel;
		readonly decimal yValue;
		readonly string xSeriesLabel;
		readonly string dataSetLabel;

		public string XCategoryLabel
		{
			get { return xCategoryLabel; }
		}

		public decimal YValue
		{
			get { return yValue; }
		}

		public string XSeriesLabel
		{
			get { return xSeriesLabel; }
		}

		public string VisibleLabel
		{
			get
			{
				return string.IsNullOrWhiteSpace(dataSetLabel) ? XSeriesLabel : string.Concat(dataSetLabel, " - ", XSeriesLabel);
			}
		}
	}
}
