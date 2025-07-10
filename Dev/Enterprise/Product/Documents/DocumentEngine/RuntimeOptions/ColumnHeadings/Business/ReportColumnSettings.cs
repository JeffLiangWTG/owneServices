using System.Xml.Serialization;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[XmlRoot(Namespace = "", IsNullable = false)]
	[XmlSerializerAssembly("Enterprise.DocumentEngine.XmlSerializers")]
	[ValueObjectSubclass("Enterprise.DocumentEngine.RuntimeOptions.AutoReportColumnSettings")]
	public class ReportColumnSettings : AutoReportColumnSettings
	{
		public ReportColumnSettings CloneVisibleOnly()
		{
			ReportColumnSettings cloneVisibleOnly = this.Clone();

			WorksheetCollection worksheetsToRemove = new WorksheetCollection();
			foreach (Worksheet worksheet in cloneVisibleOnly.Worksheets)
			{
				ColumnHeadingCollection columnsToRemove = new ColumnHeadingCollection();
				foreach (ColumnHeading heading in worksheet.ColumnHeadings)
				{
					if (heading.Hidden)
					{
						columnsToRemove.Add(heading);
					}
				}
				foreach (ColumnHeading heading in columnsToRemove)
				{
					worksheet.ColumnHeadings.Remove(heading);
				}
				if (worksheet.ColumnHeadings.Count == 0)
				{
					worksheetsToRemove.Add(worksheet);
				}
			}
			foreach (Worksheet worksheet in worksheetsToRemove)
			{
				cloneVisibleOnly.Worksheets.Remove(worksheet);
			}
			return cloneVisibleOnly;
		}

		public ReportColumnSettings Clone()
		{
			ReportColumnSettings clone = new ReportColumnSettings();
			clone.Worksheets = this.Worksheets.Clone();
			clone.Version = this.Version;
			clone.SelectedGroupByName = this.SelectedGroupByName;
			clone.SelectedSortOrderName = this.SelectedSortOrderName;
			clone.SelectedOrientation = this.SelectedOrientation;
			clone.SelectedLanguage = this.SelectedLanguage;
			if (this.Filters != null)
			{
				clone.Filters = (byte[])this.Filters.Clone();
			}
			return clone;
		}

		public void Clear()
		{
			this.Worksheets = new WorksheetCollection();
		}
	}
}
