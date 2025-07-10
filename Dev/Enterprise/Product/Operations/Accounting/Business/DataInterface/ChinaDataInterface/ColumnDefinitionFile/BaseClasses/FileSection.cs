using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Already in another language")]
	public abstract class FileSection : Section
	{
		public FileSection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public string FileName
		{
			get
			{
				return fFileName;
			}
		}

		protected string fFileName;
		protected ColumnDefinitionLine FileNameLine;

		protected override void Define()
		{
			FileNameLine = new ColumnDefinitionLine("文件名");
		}

		protected override void SetDefault()
		{
		}

		protected override void SetValue()
		{
			FileNameLine.Value = fFileName;
		}

		protected override void AddLines()
		{
			Lines.Add(FileNameLine);
		}
	}
}
