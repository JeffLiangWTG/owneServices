using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.ApplicationLogging
{
	public partial class ApplicationLoggerForm : ZTemplateForm
	{
		public ApplicationLoggerForm(ApplicationLogger logger)
		: base(logger)
		{
			InitializeComponent();
			MainTabControl.Controls.Remove(LogsTabPage);
			LogsTabPage.Dispose();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;

		public override string FormCaption => "Application Logger - " + ((ApplicationLogger)BusinessEntity)?.ALG_Name;
	}
}
