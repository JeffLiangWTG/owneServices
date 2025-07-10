using System;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class AdditionalInfosUserControl : EU.GUI.PlugIn.AdditionalInfosUserControl
	{
		public AdditionalInfosUserControl()
		{
			InitializeComponent();
			RemoveAllControlsButDescriptionTextBox();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AdditionalInfosPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			AdditionalInfosPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			AddiInfoDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 255, true);
			AddiInfoDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 14, true);
			BindingSource.SetBindingMember(AddiInfoDescriptionTextBox, nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.AdditionalInfoDescription));
		}

		void RemoveAllControlsButDescriptionTextBox()
		{
			Controls.Remove(AdditionalInfosGrid);
			AdditionalInfosGroupBox.Controls.Remove(AddInfoTypeCodeDropEdit);
		}
	}
}
