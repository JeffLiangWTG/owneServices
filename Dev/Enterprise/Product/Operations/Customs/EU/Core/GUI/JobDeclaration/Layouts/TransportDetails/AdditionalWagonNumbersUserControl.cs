using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class AdditionalWagonNumbersUserControl : ZUserControl
	{
		public AdditionalWagonNumbersUserControl()
		{
			InitializeComponent();
			AdditionalWagonNumbersButton.ToolTipCaption = ResString.GetMultilingualString("4F3748DE-8688-4D1E-8676-E59CD025C86A", "Additional Wagon Numbers");
		}

		protected new JobDeclaration DataSource => (JobDeclaration)base.DataSource;

		void AdditionalWagonNumbersButton_Click(object sender, System.EventArgs e)
		{
			var parentControl = (ZUserControl)((ZButton)sender).Parent;

			ZFormModaliser.Show(new AdditionalWagonNumbersForm(DataSource.InlandTransports), parentControl.ParentForm);
		}
	}
}
