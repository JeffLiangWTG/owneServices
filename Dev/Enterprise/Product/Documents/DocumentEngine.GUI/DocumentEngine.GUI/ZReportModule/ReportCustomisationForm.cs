using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI
{
	partial class ReportCustomisationForm : MenuCustomisationForm
	{
		public ReportCustomisationForm(ReportMenuCustomisation businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
			ResetDocumentOptionsPanel();
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		protected void ResetDocumentOptionsPanel()
		{
			this.defaultAttachmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 46, true);
			this.autoDeliveryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 100, true);
			this.autoDeliveryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 13, true);
			this.documentOptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 175, true);
			this.autoDeliveryGroupBox.Controls.Clear();
			this.autoDeliveryGroupBox.Controls.Add(this.defaultAttachmentTypeDropEdit);
			this.documentOptionsPanel.Controls.Clear();
			this.documentOptionsPanel.Controls.Add(this.autoDeliveryGroupBox);
			this.documentOptionsPanel.Visible = true;
		}

		protected override string MenuGridCaption
		{
			get { return Res.GetString("ReportCustomisationForm|MenuGridCaption", "Reports"); }
		}
	}
}
