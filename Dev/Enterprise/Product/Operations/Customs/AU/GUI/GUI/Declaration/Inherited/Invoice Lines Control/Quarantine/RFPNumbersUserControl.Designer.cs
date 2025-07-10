namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPNumbersUserControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (declaration != null)
				{
					declaration.AddInfo.ZA_IsAQISCertificateRequest_HiddenInfo.ValueChanged -= ZA_IsAQISCertificateRequest_HiddenInfo_ValueChanged;
				}
			}

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.RFPNumberDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PackCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RFPLineCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RFPNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFPNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RFPNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RFPNumbersPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RFPNumbersUnavailableLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NetQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RFPNumberDetailsGroupBox.SuspendLayout();
			this.RFPNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RFPNumbersGrid)).BeginInit();
			this.RFPNumbersPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			// 
			// RFPNumberDetailsGroupBox
			// 
			this.RFPNumberDetailsGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|b64e09b8-6bca-4e5b-96fb-c0951b30f70f", "RFP Number Details");
			this.RFPNumberDetailsGroupBox.Controls.Add(this.NetQuantityCalcDropEdit);
			this.RFPNumberDetailsGroupBox.Controls.Add(this.PackTypeDropEdit);
			this.RFPNumberDetailsGroupBox.Controls.Add(this.PackCountCalcEdit);
			this.RFPNumberDetailsGroupBox.Controls.Add(this.RFPLineCalcEdit);
			this.RFPNumberDetailsGroupBox.Controls.Add(this.RFPNumberTextBox);
			this.RFPNumberDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.RFPNumberDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 143, true);
			this.RFPNumberDetailsGroupBox.Name = "RFPNumberDetailsGroupBox";
			this.RFPNumberDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 72, true);
			this.RFPNumberDetailsGroupBox.TabIndex = 3;
			this.RFPNumberDetailsGroupBox.TabStop = false;
			// 
			// PackTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PackTypeDropEdit, "FilteredInvoiceLines.RFPNumbers.ZA_RFPPackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPPackType)));
			this.PackTypeDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|47ffd816-9fa0-40fa-a31c-89a45dcfb00b", "Pack Type");
			this.PackTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 45, true);
			this.PackTypeDropEdit.Name = "PackTypeDropEdit";
			this.PackTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.PackTypeDropEdit.TabIndex = 3;
			// 
			// PackCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PackCountCalcEdit, "FilteredInvoiceLines.RFPNumbers.ZA_RFPPackCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPPackCount)));
			this.PackCountCalcEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|4c742e18-79ce-429c-a299-b88a499d8f31", "Pack Count");
			this.PackCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 19, true);
			this.PackCountCalcEdit.Name = "PackCountCalcEdit";
			this.PackCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 20, true);
			this.PackCountCalcEdit.TabIndex = 2;
			this.PackCountCalcEdit.Text = "0";
			this.PackCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RFPLineCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RFPLineCalcEdit, "FilteredInvoiceLines.RFPNumbers.ZA_RFPLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPLine)));
			this.RFPLineCalcEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|3f0318cb-7753-487d-b4c6-d2aaa134d19b", "RFP Line");
			this.RFPLineCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 45, true);
			this.RFPLineCalcEdit.Name = "RFPLineCalcEdit";
			this.RFPLineCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.RFPLineCalcEdit.TabIndex = 1;
			this.RFPLineCalcEdit.Text = "0";
			this.RFPLineCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RFPNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RFPNumberTextBox, "FilteredInvoiceLines.RFPNumbers.ZA_RFPNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPNumber)));
			this.RFPNumberTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|159bf4e5-8001-41d3-96a4-6bb28bf3b716", "RFP Number");
			this.RFPNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 19, true);
			this.RFPNumberTextBox.Name = "RFPNumberTextBox";
			this.RFPNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 20, true);
			this.RFPNumberTextBox.TabIndex = 0;
			// 
			// RFPNumbersGroupBox
			// 
			this.RFPNumbersGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|faa47bd9-c569-4ebe-acef-c62e9bf229a7", "RFP Numbers");
			this.RFPNumbersGroupBox.Controls.Add(this.RFPNumbersGrid);
			this.RFPNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.RFPNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RFPNumbersGroupBox.Name = "RFPNumbersGroupBox";
			this.RFPNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 137, true);
			this.RFPNumbersGroupBox.TabIndex = 2;
			this.RFPNumbersGroupBox.TabStop = false;
			// 
			// RFPNumbersGrid
			// 
			this.RFPNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RFPNumbersGrid, "FilteredInvoiceLines.RFPNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPNetQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPQtyUM)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPPackCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPPackType)));
			this.RFPNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|43009b46-7acc-4b3b-8195-59b77dc88b27", "RFP Number");
			zTextBoxColumnStyleInfo2.ColumnName = "ZA_RFPNumber";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|c7e13668-aa95-4769-9e66-7920bb99de6f", "RFP Line");
			zCalcEditColumnStyleInfo4.ColumnName = "ZA_RFPLine";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|438023ab-8c99-4dde-94e2-9c5928842257", "Net Quantity");
			zCalcEditColumnStyleInfo5.ColumnName = "ZA_RFPNetQuantity";
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|eff978c5-61d2-4549-90e3-25efe78c7c05", "UQ");
			zDropEditColumnStyleInfo3.ColumnName = "ZA_RFPQtyUM";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|73a38d78-ef2b-4ecc-825a-9d966703a03e", "Pack Count");
			zCalcEditColumnStyleInfo6.ColumnName = "ZA_RFPPackCount";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|13cc7a5a-e7ef-4746-911d-c023de9c8c18", "Pack Type");
			zDropEditColumnStyleInfo4.ColumnName = "ZA_RFPPackType";
			this.RFPNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RFPNumbersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.RFPNumbersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.RFPNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.RFPNumbersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.RFPNumbersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.RFPNumbersGrid.GridId = "f2bc7b11-bd7f-49a7-98a8-653e09ce9b79";
			this.RFPNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RFPNumbersGrid.LayoutKey = "RFPNumbersGrid";
			this.RFPNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.RFPNumbersGrid.Name = "RFPNumbersGrid";
			this.RFPNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 118, true);
			this.RFPNumbersGrid.TabIndex = 0;
			// 
			// RFPNumbersPanel
			// 
			this.RFPNumbersPanel.Controls.Add(this.RFPNumberDetailsGroupBox);
			this.RFPNumbersPanel.Controls.Add(this.RFPNumbersGroupBox);
			this.RFPNumbersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RFPNumbersPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RFPNumbersPanel.Name = "RFPNumbersPanel";
			this.RFPNumbersPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 215, true);
			this.RFPNumbersPanel.TabIndex = 4;
			// 
			// RFPNumbersUnavailableLabel
			// 
			this.RFPNumbersUnavailableLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.RFPNumbersUnavailableLabel.AutoEllipsis = true;
			this.RFPNumbersUnavailableLabel.AutoSize = true;
			this.RFPNumbersUnavailableLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|75bf7c94-bc1b-416d-9fe2-e9b655b16cea", "", "RFP Numbers are required for Certificate Request message only.\r\nPlease tick the Certificate Request box on the Misc tab to allow entry of data on this tab.");
			this.RFPNumbersUnavailableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 100, true);
			this.RFPNumbersUnavailableLabel.Name = "RFPNumbersUnavailableLabel";
			this.RFPNumbersUnavailableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.RFPNumbersUnavailableLabel.TabIndex = 5;
			this.RFPNumbersUnavailableLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// NetQuantityCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.NetQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPNetQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.RFPNumber)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RFPNumbers)).SyncRoot)).ZA_RFPQtyUM)));
			this.NetQuantityCalcDropEdit.BindToAmount = "FilteredInvoiceLines.RFPNumbers.ZA_RFPNetQuantity";
			this.NetQuantityCalcDropEdit.BindToUnit = "FilteredInvoiceLines.RFPNumbers.ZA_RFPQtyUM";
			this.NetQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("RFPNumbersUserControl|578aa33b-3271-48ba-9b5b-a1f5ec9b88b1", "Net Quantity");
			this.NetQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 19, true);
			this.NetQuantityCalcDropEdit.Name = "NetQuantityCalcDropEdit";
			this.NetQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.NetQuantityCalcDropEdit.TabIndex = 4;
			// 
			// RFPNumbersUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.RFPNumbersUnavailableLabel);
			this.Controls.Add(this.RFPNumbersPanel);
			this.Name = "RFPNumbersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 215, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RFPNumberDetailsGroupBox.ResumeLayout(false);
			this.RFPNumberDetailsGroupBox.PerformLayout();
			this.RFPNumbersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.RFPNumbersGrid)).EndInit();
			this.RFPNumbersPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox RFPNumberDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit NetQuantityCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PackTypeDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PackCountCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit RFPLineCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox RFPNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RFPNumbersGroupBox;
		private Enterprise.ZArchitecture.ZGrid RFPNumbersGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel RFPNumbersPanel;
		private Enterprise.ZArchitecture.ZLabel RFPNumbersUnavailableLabel;
	}
}
