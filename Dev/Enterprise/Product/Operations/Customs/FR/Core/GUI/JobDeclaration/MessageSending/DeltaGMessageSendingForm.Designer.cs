using System.Windows.Forms;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class DeltaGMessageSendingForm : MessageSendingFormWithValidationDetails
	{
		new void InitializeComponent()
		{
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// SendButton
			//
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 419, true);
			this.SendButton.TabIndex = 3;
			//
			// CancelButton2
			//
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 419, true);
			this.CancelButton2.TabIndex = 4;
			//
			// messageSendingObjectsGroupBox
			//
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 154, true);
			//
			// MessageSendingObjectsGrid
			//
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 135, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 448, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(DeltaGJobDeclarationMessageSendingObjectParent);
			//
			// MessageSendingForm
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 471, true);
			this.DataSourceType = typeof(DeltaGJobDeclarationMessageSendingObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 510, true);
			this.Name = "MessageSendingForm";
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
