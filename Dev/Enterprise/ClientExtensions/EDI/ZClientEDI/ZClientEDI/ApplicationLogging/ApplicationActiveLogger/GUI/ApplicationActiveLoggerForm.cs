using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.ApplicationLogging
{
	public partial class ApplicationActiveLoggerForm : ZTemplateForm
	{
		public ApplicationActiveLoggerForm(ApplicationActiveLogger logger)
		: base(logger)
		{
			InitializeComponent();
			MainTabControl.Controls.Remove(LogsTabPage);
			LogsTabPage.Dispose();
		}

		protected override bool AllowNew => true;

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;

		public override string FormCaption => $"Active Application Logger - {((ApplicationActiveLogger)BusinessEntity)?.ApplicationLogger?.ALG_Name} - {((ApplicationActiveLogger)BusinessEntity)?.AAL_Environment}";
	}
}
