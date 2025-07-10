using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class SavedScheduleConfigurationManager : ColumnConfigurationManager
	{
		ReportColumnSettings headings;
		string xml;

		public SavedScheduleConfigurationManager(ColumnConfigurationsManager headingManager)
			: base(headingManager)
		{
			headings = new ReportColumnSettings();
		}

		protected override bool CanSaveAndDeleteCore
		{
			get { return false; }
		}

		public void AddHeadings(ReportColumnSettings headings)
		{
			this.headings = headings;
		}

		protected override void DeleteCore()
		{
		}

		protected override void LoadCore()
		{
			RefreshHeadingManager(headings);
		}

		public void SaveXml(Report report)
		{
			xml = CreateXml(report.ColumnHeadingManager.LinkedFilterFields, report.GroupByCollection.SelectedGroupBy.DisplayName, report.SortOrderCollection.SelectedOrder.DisplayName, report.OrientationManager.Value, report.Parent.Language);
		}

		protected override void SaveCore(CollectionOfIFilter filters, string selectedGroupByName, string selectedSortOrderName, string orientation, string selectedLanguage)
		{
			xml = CreateXml(filters, selectedGroupByName, selectedSortOrderName, orientation, selectedLanguage);
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
			get { return Res.GetString("3b2243f7-509d-4a5d-8947-c9242d053bbb", "Last Saved Setting"); }
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
			return xml;
		}
	}
}
