namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class GuaranteeUserControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.LinkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BondTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GuaranteeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReleaseGuaranteeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ReleaseGuaranteeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RemainingCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BondTypeDropEdit.SuspendLayout();
			this.GuaranteeGuidFindBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseGuaranteeGrid)).BeginInit();
			this.ReleaseGuaranteeGrid.SuspendLayout();
			this.ReleaseGuaranteeGroupBox.SuspendLayout();
			this.IssueDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction);
			// 
			// LinkButton
			// 
			this.BindingSource.SetBindingMember(this.LinkButton, "Guarantee.LinkButtonLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).Guarantee.LinkButtonLabel)));
			this.LinkButton.IsCaptionOverridden = false;
			this.LinkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 28, true);
			this.LinkButton.Name = "LinkButton";
			this.LinkButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.LinkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.LinkButton.TabIndex = 2;
			this.LinkButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.LinkButton.ToolTipCaption = null;
			this.LinkButton.UseVisualStyleBackColor = true;
			this.LinkButton.Click += new System.EventHandler(this.LinkButton_Click);
			// 
			// BondTypeDropEdit
			// 
			this.BondTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondTypeDropEdit, "Guarantee.PW_BondType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).Guarantee.PW_BondType)));
			this.BondTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 5, true);
			this.BondTypeDropEdit.Name = "BondTypeDropEdit";
			this.BondTypeDropEdit.ShouldResizeByMaxLength = true;
			this.BondTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.BondTypeDropEdit.TabIndex = 0;
			// 
			// GuaranteeGuidFindBox
			// 
			this.GuaranteeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeGuidFindBox, "Guarantee.PW_CPH_Guarantee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).Guarantee.PW_CPH_Guarantee)));
			this.GuaranteeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 29, true);
			this.GuaranteeGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Guarantees;
			this.GuaranteeGuidFindBox.Name = "GuaranteeGuidFindBox";
			this.GuaranteeGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GuaranteeGuidFindBox.ParentType = null;
			this.GuaranteeGuidFindBox.ShowDescriptionBox = false;
			this.GuaranteeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.GuaranteeGuidFindBox.TabIndex = 1;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "Guarantee.PW_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).Guarantee.PW_Status)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 132, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.StatusDropEdit.TabIndex = 8;
			// 
			// AmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountCalcEdit, "Guarantee.PW_BondAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).Guarantee.PW_BondAmount)));
			this.AmountCalcEdit.CaptionResourceString = null;
			this.AmountCalcEdit.DecimalPlaces = 2;
			this.AmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 106, true);
			this.AmountCalcEdit.Name = "AmountCalcEdit";
			this.AmountCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.AmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.AmountCalcEdit.TabIndex = 6;
			this.AmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReleaseGuaranteeGrid
			// 
			this.ReleaseGuaranteeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReleaseGuaranteeGrid, "ReleaseGuarantees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).ReleaseGuarantees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.SecondCusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).ReleaseGuarantees)).SyncRoot)).PW_BondNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AsycudaCustoms.Business.SecondCusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).ReleaseGuarantees)).SyncRoot)).PW_BondEffectiveDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.SecondCusBondDetail)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).ReleaseGuarantees)).SyncRoot)).PW_BondAmount)));
			this.ReleaseGuaranteeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "PW_BondNumber2";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(118);
			zDateEditColumnStyleInfo1.ColumnName = "PW_BondEffectiveDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PW_BondAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ReleaseGuaranteeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReleaseGuaranteeGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ReleaseGuaranteeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ReleaseGuaranteeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReleaseGuaranteeGrid.GridId = "8f763846-69b8-457e-a3ca-7adec5b86e2a";
			this.ReleaseGuaranteeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseGuaranteeGrid.LayoutKey = "ReleaseGuaranteesGrid";
			this.ReleaseGuaranteeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ReleaseGuaranteeGrid.Name = "ReleaseGuaranteeGrid";
			this.ReleaseGuaranteeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 179, true);
			this.ReleaseGuaranteeGrid.TabIndex = 0;
			// 
			// ReleaseGuaranteeGroupBox
			// 
			this.ReleaseGuaranteeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ReleaseGuaranteeGroupBox.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("4f5733da-e11f-4170-a637-e27081109b09", "Guarantee Release");
			this.ReleaseGuaranteeGroupBox.Controls.Add(this.ReleaseGuaranteeGrid);
			this.ReleaseGuaranteeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 158, true);
			this.ReleaseGuaranteeGroupBox.Name = "ReleaseGuaranteeGroupBox";
			this.ReleaseGuaranteeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 198, true);
			this.ReleaseGuaranteeGroupBox.TabIndex = 9;
			this.ReleaseGuaranteeGroupBox.TabStop = false;
			// 
			// RemainingCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RemainingCalcEdit, "Guarantee.Remaining");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).Guarantee.Remaining)));
			this.RemainingCalcEdit.CaptionResourceString = null;
			this.RemainingCalcEdit.DecimalPlaces = 2;
			this.RemainingCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 106, true);
			this.RemainingCalcEdit.Name = "RemainingCalcEdit";
			this.RemainingCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.RemainingCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.RemainingCalcEdit.TabIndex = 7;
			this.RemainingCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IssueDateEdit
			// 
			this.IssueDateEdit.AllowDrop = true;
			this.IssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.IssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.IssueDateEdit, "Guarantee.PW_BondEffectiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).Guarantee.PW_BondEffectiveDate)));
			this.IssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 80, true);
			this.IssueDateEdit.Name = "IssueDateEdit";
			this.IssueDateEdit.TabIndex = 4;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "Guarantee.PW_BondNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).Guarantee.PW_BondNumber2)));
			this.ReferenceNumberTextBox.CaptionResourceString = null;
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 54, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 3;
			// 
			// CustomsCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsCurrencyTextBox, "Guarantee.CustomsCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).Guarantee.CustomsCurrency)));
			this.CustomsCurrencyTextBox.CaptionResourceString = null;
			this.CustomsCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 80, true);
			this.CustomsCurrencyTextBox.Name = "CustomsCurrencyTextBox";
			this.CustomsCurrencyTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.CustomsCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.CustomsCurrencyTextBox.TabIndex = 5;
			// 
			// GuaranteeUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsCurrencyTextBox);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.IssueDateEdit);
			this.Controls.Add(this.RemainingCalcEdit);
			this.Controls.Add(this.ReleaseGuaranteeGroupBox);
			this.Controls.Add(this.AmountCalcEdit);
			this.Controls.Add(this.StatusDropEdit);
			this.Controls.Add(this.GuaranteeGuidFindBox);
			this.Controls.Add(this.BondTypeDropEdit);
			this.Controls.Add(this.LinkButton);
			this.Name = "GuaranteeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 359, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BondTypeDropEdit.ResumeLayout(true);
			this.BondTypeDropEdit.PerformLayout();
			this.GuaranteeGuidFindBox.ResumeLayout(true);
			this.GuaranteeGuidFindBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseGuaranteeGrid)).EndInit();
			this.ReleaseGuaranteeGrid.ResumeLayout(false);
			this.ReleaseGuaranteeGrid.PerformLayout();
			this.ReleaseGuaranteeGroupBox.ResumeLayout(false);
			this.ReleaseGuaranteeGroupBox.PerformLayout();
			this.IssueDateEdit.ResumeLayout(true);
			this.IssueDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZButton LinkButton;
		private ZArchitecture.GUI.ZDropEdit BondTypeDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox GuaranteeGuidFindBox;
		private ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		private ZArchitecture.ZCalcEdit AmountCalcEdit;
		private ZArchitecture.ZGrid ReleaseGuaranteeGrid;
		private ZArchitecture.GUI.ZGroupBox ReleaseGuaranteeGroupBox;
		private ZArchitecture.ZCalcEdit RemainingCalcEdit;
		private ZArchitecture.GUI.ZDateEdit IssueDateEdit;
		private ZArchitecture.ZTextBox ReferenceNumberTextBox;
		private ZArchitecture.ZTextBox CustomsCurrencyTextBox;
	}
}
