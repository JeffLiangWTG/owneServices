using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.ZClientCCP.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.ZClientCCP.GUI
{
	public partial class KawasakiDataTransferForm : Enterprise.ZArchitecture.GUI.DataTransferForm
	{
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox SupplierFindBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;

		new void InitializeComponent()
		{
			this.SupplierFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LogGroupBox.SuspendLayout();
			this.RowStatisticsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			//
			// BrowseButton
			//
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 70, true);
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			//
			// LogGroupBox
			//
			this.LogGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 134, true);
			this.LogGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 262, true);
			//
			// FileNameTextBox
			//
			this.BindingSource.SetBindingMember(this.FileNameTextBox, "FileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.ZClientCCP.Business.KawasakiDataTransferSupplySupplier)(null)).FileName)));
			this.FileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 71, true);
			//
			// RowsProcessedTitle
			//
			this.RowsProcessedTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.RowsProcessedTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			//
			// RowsProcessedLabel
			//
			this.RowsProcessedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 6, true);
			this.RowsProcessedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 21, true);
			//
			// RowsExcludedLabel
			//
			this.RowsExcludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 48, true);
			this.RowsExcludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 20, true);
			//
			// ProcessButton
			//
			this.ProcessButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(564, 404, true);
			//
			// ProgressBar
			//
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 116, true);
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 14, true);
			//
			// RowsIncludedLabel
			//
			this.RowsIncludedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 28, true);
			this.RowsIncludedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 19, true);
			//
			// RowsIncludedTitle
			//
			this.RowsIncludedTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 28, true);
			this.RowsIncludedTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 19, true);
			//
			// RowStatisticsPanel
			//
			this.RowStatisticsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(549, 16, true);
			this.RowStatisticsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 243, true);
			//
			// LogListBox
			//
			this.LogListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(546, 238, true);
			//
			// CloseButton
			//
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(644, 404, true);
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 22, true);
			//
			// RowsExcludedTitle
			//
			this.RowsExcludedTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 48, true);
			this.RowsExcludedTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 431, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 23, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.ZClientCCP.Business.KawasakiDataTransferSupplySupplier);
			//
			// SupplierFindBox
			//
			this.BindingSource.SetBindingMember(this.SupplierFindBox, "SupplierForDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.ZClientCCP.Business.KawasakiDataTransferSupplySupplier)(null)).SupplierForDeclaration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.ZClientCCP.Business.KawasakiDataTransferSupplySupplier)(null)).SupplierList)));
			this.SupplierFindBox.BindToList = "SupplierList";
			this.SupplierFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("KawasakiDataTransferForm|c792c35c-1fd2-4b01-a4f9-10bfeb79379b", "Supplier");
			this.SupplierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 18, true);
			this.SupplierFindBox.Name = "SupplierFindBox";
			this.SupplierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 20, true);
			this.SupplierFindBox.TabIndex = 8;
			//
			// zGroupBox1
			//
			this.zGroupBox1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("KawasakiDataTransferForm|24d1b57e-d89d-4b59-814c-9e78051819bb", "Supplier For Import");
			this.zGroupBox1.Controls.Add(this.SupplierFindBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 45, true);
			this.zGroupBox1.TabIndex = 9;
			this.zGroupBox1.TabStop = false;
			//
			// KawasakiDataTransferForm
			//

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 454, true);
			this.Controls.Add(this.zGroupBox1);
			this.DataSourceAssemblyName = "ZClientCCP";
			this.DataSourceType = typeof(Enterprise.Client.ZClientCCP.Business.KawasakiDataTransferSupplySupplier);
			this.DataSourceTypeName = "Enterprise.Client.ZClientCCP.Business.KawasakiDataTransferSupplySupplier";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 421, true);
			this.Name = "KawasakiDataTransferForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.ProgressLabel, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ProcessButton, 0);
			this.Controls.SetChildIndex(this.BrowseButton, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.LogGroupBox, 0);
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.LogGroupBox.ResumeLayout(false);
			this.RowStatisticsPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
