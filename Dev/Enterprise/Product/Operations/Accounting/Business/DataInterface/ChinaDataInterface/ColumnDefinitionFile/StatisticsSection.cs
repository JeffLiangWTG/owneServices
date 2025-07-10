using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Already in another language")]
	public class StatisticsSection : Section
	{
		public StatisticsSection(BusinessObjectFactory factory)
			: base(factory)
		{
			fSectionHeader = "[统计码]";
		}

		protected override void Define()
		{
			StatisticalCodeCountLine = new ColumnDefinitionLine("统计码数");
		}

		protected override void SetDefault()
		{
			StatisticalCodeCount = 0;
		}

		protected override void SetValue()
		{
			StatisticalCodeCountLine.Value = StatisticalCodeCount.ToString();
		}

		protected override void AddLines()
		{
			Lines.Add(StatisticalCodeCountLine);
		}

		protected ColumnDefinitionLine StatisticalCodeCountLine;
		protected int StatisticalCodeCount;
	}
}
