using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	public partial class BiReportConfigurationForm : ZChildForm
	{
		readonly BiReportConfiguration ReportConfiguration;
		public BIReportsColumnArrangementUserControl arrangementControl;

		public BiReportConfigurationForm()
		{
			CaptionRenderingEnabled = true;
		}

		public BiReportConfigurationForm(BiReportConfiguration reportConfiguration)
		{
			InitializeComponent();
			ReportConfiguration = reportConfiguration;
			InitializeReportConfigurationForm();
		}

		void InitializeReportConfigurationForm()
		{
			ReportConfiguration.Report.PrepareForRender();
			arrangementControl = new BIReportsColumnArrangementUserControl(ReportConfiguration.Report);
			Controls.Add(arrangementControl);
		}
	}
}
