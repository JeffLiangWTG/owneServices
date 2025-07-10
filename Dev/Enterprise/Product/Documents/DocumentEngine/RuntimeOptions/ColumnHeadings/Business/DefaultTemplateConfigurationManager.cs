using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class DefaultTemplateConfigurationManager : ColumnConfigurationManager
	{
		protected readonly ReportColumnSettings headings;

		public DefaultTemplateConfigurationManager(ColumnConfigurationsManager headingManager)
			: base(headingManager)
		{
			headings = new ReportColumnSettings();
		}

		protected override bool CanSaveAndDeleteCore
		{
			get { return false; }
		}

		public void AddHeading(string sheetName, ColumnHeading heading, string sheetNameOverride = "")
		{
			if (!headings.Worksheets.Contains(sheetName))
			{
				headings.Worksheets.AddNew(sheetName);
			}
			headings.Worksheets[sheetName].ColumnHeadings.Add(heading);
			headings.Worksheets[sheetName].NameOverride = sheetNameOverride;
		}

		public void SetTitle(string sheetName, string title, string sheetNameOverride = "")
		{
			if (!headings.Worksheets.Contains(sheetName))
			{
				headings.Worksheets.AddNew(sheetName);
			}
			headings.Worksheets[sheetName].Title = title;
			headings.Worksheets[sheetName].NameOverride = sheetNameOverride;
		}

		protected override void DeleteCore()
		{
		}

		public ReportColumnSettings GetCopyOfHeadings()
		{
			return headings.Clone();
		}

		protected override void LoadCore()
		{
			RefreshHeadingManager(GetCopyOfHeadings());
		}

		protected override void SaveCore(CollectionOfIFilter filters, string selectedGroupByName, string selectedSortOrderName, string selectedOrientation, string selectedLanguage)
		{
		}

		public override string ToString()
		{
			return Description;
		}

		public override ZGuid LinkPK
		{
			get { return ZGuid.Empty; }
		}

		public override ZString Description
		{
			get { return Res.GetString("011caeac-29ca-46d8-bd45-b35bd24a27e4", "Default Configuration: See Configuration tab to modify"); }
		}

		public override ZString UniqueDescription
		{
			get { return Description; }
		}

		public override ZString LinkCode
		{
			get { return ZString.Empty; }
		}

		protected override string GetXml()
		{
			return null;
		}
	}
}
