using Enterprise.Customs.IL.Manifest.Business;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	partial class ManifestQuerySelectionDialog
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.ItemsGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
            this.ItemsGrid.SuspendLayout();
            this.ButtonPanel.SuspendLayout();
            this.ItemsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.None;
            this.MainStatusBar.Enabled = false;
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 364, true);
            this.MainStatusBar.ShowPanels = false;
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 21, true);
            this.MainStatusBar.Visible = false;
            // 
            // MessageStatusBarPanel
            // 
            this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            // 
            // ErrorStatusBarPanel
            // 
            this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObjectParent);
            // 
            // ItemsGrid
            // 
            this.ItemsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ItemsGrid, "SendingObjectsCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObjectParent)(null)).SendingObjectsCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ShouldSend)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).MessageType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).MessageSubType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).MessageSubTypeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ManifestNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ParentDealNumber)));
            this.ItemsGrid.CaptionVisible = false;
            zCheckBoxColumnStyleInfo2.ColumnName = "ShouldSend";
            zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.ColumnName = "MessageType";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.ColumnName = "MessageSubType";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo8.ColumnName = "MessageSubTypeDescription";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
            zTextBoxColumnStyleInfo9.ColumnName = "ManifestNumber";
            zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo10.ColumnName = "ParentDealNumber";
            zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.ItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
            this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.ItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ItemsGrid.GridId = "77B18FF6-1624-434A-B7D1-A63DB6E30D22";
            this.ItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ItemsGrid.LayoutKey = "ItemsGridGridKey";
            this.ItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.ItemsGrid.Name = "ItemsGrid";
            this.ItemsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
            this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 339, true);
            this.ItemsGrid.TabIndex = 0;
            // 
            // ButtonPanel
            // 
            this.ButtonPanel.Controls.Add(this.SendButton);
            this.ButtonPanel.Controls.Add(this.ButtonCancel);
            this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 358, true);
            this.ButtonPanel.Name = "ButtonPanel";
            this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 30, true);
            this.ButtonPanel.TabIndex = 2;
            // 
            // SendButton
            // 
            this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SendButton.CaptionResourceString = Enterprise.Customs.IL.Manifest.GUI.Res.GetData("C0381BAB-323F-420B-B3B0-D5FC85BA1B8E", "&Send");
            this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(607, 5, true);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
            this.SendButton.TabIndex = 4;
            this.SendButton.ToolTipCaption = null;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonCancel.CaptionResourceString = Enterprise.Customs.IL.Manifest.GUI.Res.GetData("5B2A15D7-1487-485D-81CC-02A1DD2EAFF3", "&Cancel");
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(686, 5, true);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
            this.ButtonCancel.TabIndex = 5;
            this.ButtonCancel.ToolTipCaption = null;
            this.ButtonCancel.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ItemsGroupBox
            // 
            this.ItemsGroupBox.CaptionResourceString = Enterprise.Customs.IL.Manifest.GUI.Res.GetData("E6DAA12F-8924-4391-8DC1-71CDD6959F31", "Manifest Query");
            this.ItemsGroupBox.Controls.Add(this.ItemsGrid);
            this.ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ItemsGroupBox.Name = "ItemsGroupBox";
            this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 358, true);
            this.ItemsGroupBox.TabIndex = 0;
            this.ItemsGroupBox.TabStop = false;
            // 
            // ManifestQuerySelectionDialog
            // 
            this.CancelButton = this.ButtonCancel;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 388, true);
            this.Controls.Add(this.ItemsGroupBox);
            this.Controls.Add(this.ButtonPanel);
            this.DataSourceAssemblyName = "Enterprise.Customs.IL.Manifest.Business";
            this.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryMessageSendingObjectParent);
            this.DataSourceTypeName = "Enterprise.Customs.IL.Manifest.Business.AsycudaManifestQueryHeaderWrapper";
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 427, true);
            this.Name = "ManifestQuerySelectionDialog";
            this.Controls.SetChildIndex(this.ButtonPanel, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.ItemsGroupBox, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
            this.ItemsGrid.ResumeLayout(false);
            this.ItemsGrid.PerformLayout();
            this.ButtonPanel.ResumeLayout(false);
            this.ButtonPanel.PerformLayout();
            this.ItemsGroupBox.ResumeLayout(false);
            this.ItemsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.ZGrid ItemsGrid;
		Enterprise.ZArchitecture.GUI.ZPanel ButtonPanel;
		Enterprise.ZArchitecture.GUI.ZButton ButtonCancel;
		Enterprise.ZArchitecture.GUI.ZButton SendButton;
		Enterprise.ZArchitecture.GUI.ZGroupBox ItemsGroupBox;
	}
}
