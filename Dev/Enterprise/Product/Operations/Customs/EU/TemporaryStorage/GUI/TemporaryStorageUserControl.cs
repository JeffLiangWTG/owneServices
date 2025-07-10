using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.TemporaryStorage.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class TemporaryStorageUserControl : ZUserControl
	{
		public TemporaryStorageUserControl()
		{
			InitializeComponent();
			SetGuaranteeLayout();
			InitPreviousDocumentsUserControl();
		}

		void InitPreviousDocumentsUserControl()
		{
			var pervioudDocumentsUserControl = new UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid();
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(pervioudDocumentsUserControl, "", SupportingInfoColumnLayoutContext);
			pervioudDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			PreviousDocumentsLayoutPanel.Controls.Add(pervioudDocumentsUserControl);
		}

		void SetGuaranteeLayout()
		{
			DynamicGuaranteePanel.UpdateLayout(new GuaranteeGroupBoxWithOverrideLayout());
		}

		const string SupportingInfoColumnLayoutContext = "STO";
	}
}
