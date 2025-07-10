namespace Enterprise.Customs.CA.GUI.PlugIns
{
	partial class HouseBillArrivalCertificationSelectionDialog
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SelectAll_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeselectAll_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageToBeSendGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessageToBeSendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageToBeSendGrid)).BeginInit();
			this.MessageToBeSendGrid.SuspendLayout();
			this.MessageToBeSendGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 261, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.RNSRequestBOCollection);
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.SelectAll_Button);
			this.ButtonsPanel.Controls.Add(this.DeselectAll_Button);
			this.ButtonsPanel.Controls.Add(this.Cancel_Button);
			this.ButtonsPanel.Controls.Add(this.OKButton);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 285, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 35, true);
			this.ButtonsPanel.TabIndex = 1;
			// 
			// SelectAll_Button
			// 
			this.SelectAll_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectAll_Button.CaptionResourceString = Res.GetData("6EFB816D-521A-465F-950E-8CE673B45A0F", "Select All");
			this.SelectAll_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 6, true);
			this.SelectAll_Button.Name = "SelectAll_Button";
			this.SelectAll_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SelectAll_Button.TabIndex = 0;
			this.SelectAll_Button.ToolTipCaption = null;
			this.SelectAll_Button.UseVisualStyleBackColor = true;
			this.SelectAll_Button.Click += new System.EventHandler(this.SelectAll_Button_Click);
			// 
			// DeselectAll_Button
			// 
			this.DeselectAll_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeselectAll_Button.CaptionResourceString = Res.GetData("65580EBD-FB1A-40D4-A59C-9058E388A259", "Deselect All");
			this.DeselectAll_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 6, true);
			this.DeselectAll_Button.Name = "DeselectAll_Button";
			this.DeselectAll_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeselectAll_Button.TabIndex = 1;
			this.DeselectAll_Button.ToolTipCaption = null;
			this.DeselectAll_Button.UseVisualStyleBackColor = true;
			this.DeselectAll_Button.Click += new System.EventHandler(this.DeselectAll_Button_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Res.GetData("2AF6D921-118F-4327-958E-107E20E2EAE1", "Cancel");
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 9, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.Cancel_Button.TabIndex = 3;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Res.GetData("B64C265E-1EAA-4CEF-815E-5656B458F39B", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(538, 9, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// MessageToBeSendGrid
			// 
			this.MessageToBeSendGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessageToBeSendGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).HouseBillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).CargoControlNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).DateOfArrival)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).OfficeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSRequestBO)(null)).SubLocationCode)));
			this.MessageToBeSendGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("4B3F8E3C-144C-4755-8846-549079CB7D45", "Send?");
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("62E95FCC-12FC-430B-891B-92B278D1826F", "House Bill");
			zTextBoxColumnStyleInfo1.ColumnName = "HouseBillNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("2E80D506-91B1-4213-BFD2-A2795737CE84", "CCN");
			zTextBoxColumnStyleInfo2.ColumnName = "CargoControlNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.CaptionResourceString = Res.GetData("CF9905A3-91CC-4445-8ACF-877C5E3E8856", "Arrival Date");
			zDateEditColumnStyleInfo1.ColumnName = "DateOfArrival";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("881E80AC-0D55-4FC7-B919-8C64C46428A8", "CBSA Office");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OfficeCode";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("C18964EB-A8E6-47BA-B435-6AD23AF5D808", "Sub-Location");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "SubLocationCode";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MessageToBeSendGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MessageToBeSendGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessageToBeSendGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessageToBeSendGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessageToBeSendGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MessageToBeSendGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.MessageToBeSendGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageToBeSendGrid.GridId = "7E945D04-BDD3-4B74-B53F-5714B0A3CCB8";
			this.MessageToBeSendGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageToBeSendGrid.LayoutKey = "MessageToBeSendGrid";
			this.MessageToBeSendGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageToBeSendGrid.Name = "MessageToBeSendGrid";
			this.MessageToBeSendGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 266, true);
			this.MessageToBeSendGrid.TabIndex = 0;
			// 
			// MessageToBeSendGroupBox
			// 
			this.MessageToBeSendGroupBox.CaptionResourceString = Res.GetData("9021DB12-E083-4271-BE4A-16DCD116EC6F", "Messages to be send");
			this.MessageToBeSendGroupBox.Controls.Add(this.MessageToBeSendGrid);
			this.MessageToBeSendGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageToBeSendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageToBeSendGroupBox.Name = "MessageToBeSendGroupBox";
			this.MessageToBeSendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 285, true);
			this.MessageToBeSendGroupBox.TabIndex = 0;
			this.MessageToBeSendGroupBox.TabStop = false;
			// 
			// HouseBillArrivalCertificationSelectionDialog
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 320, true);
			this.Controls.Add(this.MessageToBeSendGroupBox);
			this.Controls.Add(this.ButtonsPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.CA.Business";
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.RNSRequestBOCollection);
			this.DataSourceTypeName = "Enterprise.Customs.CA.Business.RNSRequestBOCollection";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "HouseBillArrivalCertificationSelectionDialog";
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.MessageToBeSendGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageToBeSendGrid)).EndInit();
			this.MessageToBeSendGrid.ResumeLayout(false);
			this.MessageToBeSendGrid.PerformLayout();
			this.MessageToBeSendGroupBox.ResumeLayout(false);
			this.MessageToBeSendGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZGrid MessageToBeSendGrid;
		ZArchitecture.GUI.ZGroupBox MessageToBeSendGroupBox;
		public Enterprise.ZArchitecture.GUI.ZButton OKButton;
		public Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		protected Enterprise.ZArchitecture.GUI.ZPanel ButtonsPanel;
		public Enterprise.ZArchitecture.GUI.ZButton SelectAll_Button;
		public Enterprise.ZArchitecture.GUI.ZButton DeselectAll_Button;
	}
}
