using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaTransferDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.transferDetailsGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.transferBillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.dynamicDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.CountrySpecificPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.transferDetailsGroupbox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.transferBillsGrid)).BeginInit();
			this.transferBillsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader);
			// 
			// transferDetailsGroupbox
			// 
			this.transferDetailsGroupbox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("EFDAE1F1-BAE0-4AF8-88A7-31F49197FD76", "Transfer Details");
			this.transferDetailsGroupbox.Controls.Add(this.transferBillsGrid);
			this.transferDetailsGroupbox.Controls.Add(this.dynamicDetailsPanel);
			this.transferDetailsGroupbox.Controls.Add(this.CountrySpecificPanel);
			this.transferDetailsGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.transferDetailsGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.transferDetailsGroupbox.Name = "transferDetailsGroupbox";
			this.transferDetailsGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 498, true);
			this.transferDetailsGroupbox.TabIndex = 0;
			this.transferDetailsGroupbox.TabStop = false;
			// 
			// transferBillsGrid
			// 
			this.transferBillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.transferBillsGrid, "TransferBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).TransferBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferBill)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).TransferBills)).SyncRoot)).ATB_BillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferBill)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).TransferBills)).SyncRoot)).ATB_MessageStatus)));
			this.transferBillsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "ATB_BillNumber";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ATB_MessageStatus";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.transferBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.transferBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.transferBillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.transferBillsGrid.GridId = "C9305F3E-54C7-4717-BDF2-587D962B5807";
			this.transferBillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.transferBillsGrid.LayoutKey = "transferBillsGrid";
			this.transferBillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 106, true);
			this.transferBillsGrid.Name = "transferBillsGrid";
			this.transferBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1157, 355, true);
			this.transferBillsGrid.TabIndex = 1;
			// 
			// dynamicDetailsPanel
			// 
			this.dynamicDetailsPanel.AllowDrop = true;
			this.dynamicDetailsPanel.AutoScroll = false;
			this.dynamicDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.dynamicDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.dynamicDetailsPanel.Name = "dynamicDetailsPanel";
			this.dynamicDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.dynamicDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1157, 80, true);
			this.dynamicDetailsPanel.TabIndex = 0;
			// 
			// CountrySpecificPanel
			// 
			this.CountrySpecificPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.CountrySpecificPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 461, true);
			this.CountrySpecificPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CountrySpecificPanel.Name = "CountrySpecificPanel";
			this.CountrySpecificPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1157, 34, true);
			this.CountrySpecificPanel.TabIndex = 2;
			// 
			// AsycudaTransferDetailsUserControl
			// 
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 0, true);
			this.Controls.Add(this.transferDetailsGroupbox);
			this.Name = "AsycudaTransferDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 498, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.transferDetailsGroupbox.ResumeLayout(false);
			this.transferDetailsGroupbox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.transferBillsGrid)).EndInit();
			this.transferBillsGrid.ResumeLayout(false);
			this.transferBillsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGroupBox transferDetailsGroupbox;
		DynamicLayoutPanel dynamicDetailsPanel;
		private ZArchitecture.ZGrid transferBillsGrid;
		ZPanel CountrySpecificPanel;
	}
}
