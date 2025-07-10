using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Already in another language")]
	public class ReportSection : Section
	{
		public ReportSection(BusinessObjectFactory factory)
			: base(factory)
		{
			fSectionHeader = "[报表]";
		}

		protected override void Define()
		{
			ReportCountLine = new ColumnDefinitionLine("报表数");
		}

		protected override void SetDefault()
		{
			ReportCount = 0;
		}

		protected override void SetValue()
		{
			ReportCountLine.Value = ReportCount.ToString();
		}

		protected override void AddLines()
		{
			Lines.Add(ReportCountLine);
		}

		protected ColumnDefinitionLine ReportCountLine;

		protected int ReportCount;
	}
}
