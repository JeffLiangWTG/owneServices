namespace Enterprise.Customs.CN.GUI
{
	partial class AdditionalInformationForm
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
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.AdditionalInformationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ElementValueGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TextBoxGoodsSpecModel = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBoxNameOfGoods = new Enterprise.ZArchitecture.ZTextBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInformationGrid)).BeginInit();
			this.AdditionalInformationGrid.SuspendLayout();
			this.ElementValueGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 359, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.AdditionalInformationWrapper);
			// 
			// AdditionalInformationGrid
			// 
			this.AdditionalInformationGrid.AllowDrop = true;
			this.AdditionalInformationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalInformationGrid, "AdditionalElementValues");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.AdditionalInformationWrapper)(null)).AdditionalElementValues)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.AdditionalElementWrapper)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.AdditionalInformationWrapper)(null)).AdditionalElementValues)).SyncRoot)).ElementName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.AdditionalElementWrapper)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.AdditionalInformationWrapper)(null)).AdditionalElementValues)).SyncRoot)).ElementValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.AdditionalElementWrapper)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.AdditionalInformationWrapper)(null)).AdditionalElementValues)).SyncRoot)).ElementValue_FieldType)));
			this.AdditionalInformationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ElementName";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.ColumnName = "ElementValue";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ElementValue_FieldType";
			zMultiControlColumnStyleInfo1.IsMandatory = true;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.AdditionalInformationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalInformationGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.AdditionalInformationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationGrid.GridId = "8563ddce-cbbf-4a11-b17e-3420abc98ae1";
			this.AdditionalInformationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalInformationGrid.LayoutKey = "AdditionalInformationGrid";
			this.AdditionalInformationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInformationGrid.Name = "AdditionalInformationGrid";
			this.AdditionalInformationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 233, true);
			this.AdditionalInformationGrid.TabIndex = 0;
			// 
			// ElementValueGroupBox
			// 
			this.ElementValueGroupBox.Controls.Add(this.TextBoxGoodsSpecModel);
			this.ElementValueGroupBox.Controls.Add(this.TextBoxNameOfGoods);
			this.ElementValueGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ElementValueGroupBox, false);
			this.ElementValueGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ElementValueGroupBox.Name = "ElementValueGroupBox";
			this.ElementValueGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 90, true);
			this.ElementValueGroupBox.TabIndex = 0;
			this.ElementValueGroupBox.TabStop = false;
			// 
			// TextBoxGoodsSpecModel
			// 
			this.TextBoxGoodsSpecModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBoxGoodsSpecModel, "GoodsSpecModel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.AdditionalInformationWrapper)(null)).GoodsSpecModel)));
			this.TextBoxGoodsSpecModel.CaptionResourceString = null;
			this.TextBoxGoodsSpecModel.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBoxGoodsSpecModel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 37, true);
			this.TextBoxGoodsSpecModel.Multiline = true;
			this.TextBoxGoodsSpecModel.Name = "TextBoxGoodsSpecModel";
			this.TextBoxGoodsSpecModel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 45, true);
			this.TextBoxGoodsSpecModel.TabIndex = 1;
			// 
			// TextBoxNameOfGoods
			// 
			this.TextBoxNameOfGoods.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TextBoxNameOfGoods, "NameOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.AdditionalInformationWrapper)(null)).NameOfGoods)));
			this.TextBoxNameOfGoods.CaptionResourceString = null;
			this.TextBoxNameOfGoods.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TextBoxNameOfGoods.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 14, true);
			this.TextBoxNameOfGoods.Name = "TextBoxNameOfGoods";
			this.TextBoxNameOfGoods.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(271, 17, true);
			this.TextBoxNameOfGoods.TabIndex = 0;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("73C866FC-93AF-4F79-BD8B-7971AD7820C6", "OK");
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 97, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("ED211E85-EB80-448D-BAA5-03EF951B9F93", "Cancel");
			this.CancelButton.IsCaptionOverridden = false;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 97, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ElementValueGroupBox);
			this.BottomPanel.Controls.Add(this.OKButton);
			this.BottomPanel.Controls.Add(this.CancelButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 233, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 127, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// AdditionalInformationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 383, true);
			this.Controls.Add(this.AdditionalInformationGrid);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.CN.Business.AdditionalInformationWrapper);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 300, true);
			this.Name = "AdditionalInformationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.AdditionalInformationGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInformationGrid)).EndInit();
			this.AdditionalInformationGrid.ResumeLayout(false);
			this.AdditionalInformationGrid.PerformLayout();
			this.ElementValueGroupBox.ResumeLayout(false);
			this.ElementValueGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid AdditionalInformationGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ElementValueGroupBox;
		private Enterprise.ZArchitecture.ZTextBox TextBoxNameOfGoods;
		private Enterprise.ZArchitecture.ZTextBox TextBoxGoodsSpecModel;
		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
		internal new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
	}
}
