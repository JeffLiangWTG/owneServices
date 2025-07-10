using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public sealed partial class AsycudaBillUserControl : ManifestSpecificProviderUserControl, IResetMessageStatusSupporter
	{
		public AsycudaBillUserControl()
		{
			InitializeComponent();
		}

		protected override void OnProviderIdentifierChanged(ApplicationGUIProvider provider)
		{
			var layout = provider?.GetBillLayout();
			dynamicBilllDetailsPanel.UpdateLayout(layout);

			base.OnProviderIdentifierChanged(provider);

			CustomiseMenuItem();
		}

		void CustomiseMenuItem()
		{
			var messageStatusTextBox = dynamicBilllDetailsPanel.Controls.Find(nameof(CommonBillControlBag.MessageStatusTextBox), true).FirstOrDefault();
			resetMessageStatus = messageStatusTextBox == null ? null : ResetMessageStatusHelper.CreateResetMessageStatusMenuItem(messageStatusTextBox, ResetMessageStatus_Click, ResetMessageStatus_Popup);
		}

		void IResetMessageStatusSupporter.ResetMessageStatus_Popup() => ResetMessageStatus_Popup();
		void ResetMessageStatus_Popup() => ResetMessageStatusHelper.ResetMessageStatus_PopupBillLevel(CurrentDataItem, resetMessageStatus, false);

		void IResetMessageStatusSupporter.ResetMessageStatus_Click() => ResetMessageStatus_Click();
		void ResetMessageStatus_Click() => ResetMessageStatusHelper.ResetMessageStatus_ClickBillLevel(CurrentDataItem);

		public new AsycudaBill CurrentDataItem => (AsycudaBill)base.CurrentDataItem;

		protected override Control ControlToAddManifestSpecificUserControl => BillSpecificPanel;
		protected override string ManifestSpecificUserControlDataMember => "";

		ZMenuItem resetMessageStatus;
	}
}
