namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class GuaranteeAccessCodesUserSelectionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ConfirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.GuaranteesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.GuaranteesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ButtonsPanel.SuspendLayout();
            this.GuaranteesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GuaranteesGrid)).BeginInit();
            this.GuaranteesGrid.SuspendLayout();
            this.MainGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObjectCollection);
            // 
            // ButtonsPanel
            // 
            this.ButtonsPanel.Controls.Add(this.ConfirmButton);
            this.ButtonsPanel.Controls.Add(this.CloseButton);
            this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 196, true);
            this.ButtonsPanel.Name = "ButtonsPanel";
            this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 29, true);
            this.ButtonsPanel.TabIndex = 1;
            // 
            // ConfirmButton
            // 
            this.ConfirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ConfirmButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b50e9dd0-70c9-4e43-9f2e-445b441f073c", "Update Access Code");
            this.ConfirmButton.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ConfirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 3, true);
            this.ConfirmButton.Name = "ConfirmButton";
            this.ConfirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 21, true);
            this.ConfirmButton.TabIndex = 3;
            this.ConfirmButton.ToolTipCaption = null;
            this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b241e2bb-2d86-4086-bf5f-ea47bd722096", "Cancel");
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 3, true);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 21, true);
            this.CloseButton.TabIndex = 4;
            this.CloseButton.ToolTipCaption = null;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // GuaranteesGroupBox
            // 
            this.GuaranteesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GuaranteesGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("C0AE1A6A-3A9D-4288-9051-AF9B90A92C32", "Guarantees");
            this.GuaranteesGroupBox.Controls.Add(this.GuaranteesGrid);
            this.GuaranteesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 17, true);
            this.GuaranteesGroupBox.Name = "GuaranteesGroupBox";
            this.GuaranteesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 175, true);
            this.GuaranteesGroupBox.TabIndex = 5;
            this.GuaranteesGroupBox.TabStop = false;
            // 
            // GuaranteesGrid
            // 
            this.GuaranteesGrid.AccessibleName = "";
            this.GuaranteesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.GuaranteesGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObject)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObject)(null)).GuaranteeType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObject)(null)).GuaranteeReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObject)(null)).AccessCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObject)(null)).IsSelected)));
            this.GuaranteesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "GuaranteeType";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
            zTextBoxColumnStyleInfo2.ColumnName = "GuaranteeReferenceNumber";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(147);
            zTextBoxColumnStyleInfo3.ColumnName = "AccessCode";
            zTextBoxColumnStyleInfo3.IsSortable = false;
            zTextBoxColumnStyleInfo3.PasswordChar = '*';
            zTextBoxColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zCheckBoxColumnStyleInfo1.ColumnName = "IsSelected";
            zCheckBoxColumnStyleInfo1.IsSortable = false;
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
            this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.GuaranteesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.GuaranteesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GuaranteesGrid.GridId = "533984d3-13c8-4990-bd20-1bef1f932482";
            this.GuaranteesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.GuaranteesGrid.LayoutKey = "GuaranteesGrid";
            this.GuaranteesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.GuaranteesGrid.Name = "GuaranteesGrid";
            this.GuaranteesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 158, true);
            this.GuaranteesGrid.TabIndex = 6;
            // 
            // MainGroupBox
            // 
            this.MainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainGroupBox.Controls.Add(this.GuaranteesGroupBox);
            this.MainGroupBox.Controls.Add(this.ButtonsPanel);
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MainGroupBox, false);
            this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
            this.MainGroupBox.Name = "MainGroupBox";
            this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 227, true);
            this.MainGroupBox.TabIndex = 11;
            this.MainGroupBox.TabStop = false;
            // 
            // GuaranteeAccessCodesUserSelectionForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 256, true);
            this.Controls.Add(this.MainGroupBox);
            this.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesUserSelectionObjectCollection);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 293, true);
            this.Name = "GuaranteeAccessCodesUserSelectionForm";
            this.Text = "GuaranteeAccessCodesUserSelectionForm";
            this.Controls.SetChildIndex(this.MainGroupBox, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ButtonsPanel.ResumeLayout(false);
            this.ButtonsPanel.PerformLayout();
            this.GuaranteesGroupBox.ResumeLayout(false);
            this.GuaranteesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GuaranteesGrid)).EndInit();
            this.GuaranteesGrid.ResumeLayout(false);
            this.GuaranteesGrid.PerformLayout();
            this.MainGroupBox.ResumeLayout(false);
            this.MainGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		internal ZArchitecture.ZGrid GuaranteesGrid;
		internal ZArchitecture.GUI.ZGroupBox GuaranteesGroupBox;
		internal ZArchitecture.GUI.ZButton ConfirmButton;
		internal ZArchitecture.GUI.ZButton CloseButton;
		private ZArchitecture.GUI.ZPanel ButtonsPanel;
	}
}
