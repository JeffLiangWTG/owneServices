namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	partial class RiskUserControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.RiskGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CustomsValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomsQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.RemainingNetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.RemainingCustomsValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RemainingCustomsQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RemainingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RiskGrid)).BeginInit();
			this.RiskGrid.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.RemainingNetWeightCalcDropEdit.SuspendLayout();
			this.RemainingGroupBox.SuspendLayout();
			this.EntryGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction);
			// 
			// RiskGrid
			// 
			this.RiskGrid.AllowNavigation = false;
			this.RiskGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RiskGrid, "RiskManagements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RiskManagements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.RiskManagement)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RiskManagements)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.RiskManagement)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RiskManagements)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.RiskManagement)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RiskManagements)).SyncRoot)).CSI_ReferenceNumberFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AsycudaCustoms.Business.RiskManagement)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RiskManagements)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.RiskManagement)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RiskManagements)).SyncRoot)).CSI_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.RiskManagement)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RiskManagements)).SyncRoot)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.RiskManagement)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RiskManagements)).SyncRoot)).CSI_Quantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.RiskManagement)(((System.Collections.IList)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RiskManagements)).SyncRoot)).CSI_AdditionalDescription)));
			this.RiskGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CSI_ReferenceNumberFieldType";
			zMultiControlColumnStyleInfo1.IsCustomColumn = false;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsCustomColumn = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Value";
			zCalcEditColumnStyleInfo1.IsCustomColumn = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity";
			zCalcEditColumnStyleInfo2.IsCustomColumn = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CSI_Quantity2";
			zCalcEditColumnStyleInfo3.IsCustomColumn = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zMultiLineTextBoxColumnInfo1.ColumnName = "CSI_AdditionalDescription";
			zMultiLineTextBoxColumnInfo1.IsCustomColumn = false;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.RiskGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RiskGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.RiskGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RiskGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RiskGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RiskGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RiskGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.RiskGrid.GridId = "59e66afd-d54f-4c2d-a108-0fa931e67ca8";
			this.RiskGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RiskGrid.LayoutKey = "RiskGrid";
			this.RiskGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 56, true);
			this.RiskGrid.Name = "RiskGrid";
			this.RiskGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(726, 110, true);
			this.RiskGrid.TabIndex = 4;
			// 
			// CustomsValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CustomsValueCalcEdit, "CustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).CustomsValue)));
			this.CustomsValueCalcEdit.CaptionResourceString = null;
			this.CustomsValueCalcEdit.DecimalPlaces = 2;
			this.CustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 21, true);
			this.CustomsValueCalcEdit.Name = "CustomsValueCalcEdit";
			this.CustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CustomsValueCalcEdit.TabIndex = 1;
			this.CustomsValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CustomsQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CustomsQuantityCalcEdit, "CustomsQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).CustomsQuantity)));
			this.CustomsQuantityCalcEdit.CaptionResourceString = null;
			this.CustomsQuantityCalcEdit.DecimalPlaces = 2;
			this.CustomsQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 22, true);
			this.CustomsQuantityCalcEdit.Name = "CustomsQuantityCalcEdit";
			this.CustomsQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CustomsQuantityCalcEdit.TabIndex = 3;
			this.CustomsQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).NetWeightKilograms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).NetWeightUQ)));
			this.NetWeightCalcDropEdit.BindToAmount = "NetWeightKilograms";
			this.NetWeightCalcDropEdit.BindToUnit = "NetWeightUQ";
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 21, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 2;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// RemainingNetWeightCalcDropEdit
			// 
			this.RemainingNetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RemainingNetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RemainingNetWeightKilograms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).NetWeightUQ)));
			this.RemainingNetWeightCalcDropEdit.BindToAmount = "RemainingNetWeightKilograms";
			this.RemainingNetWeightCalcDropEdit.BindToUnit = "NetWeightUQ";
			this.RemainingNetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 20, true);
			this.RemainingNetWeightCalcDropEdit.Name = "RemainingNetWeightCalcDropEdit";
			this.RemainingNetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.RemainingNetWeightCalcDropEdit.TabIndex = 6;
			this.RemainingNetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// RemainingCustomsValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RemainingCustomsValueCalcEdit, "RemainingCustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RemainingCustomsValue)));
			this.RemainingCustomsValueCalcEdit.CaptionResourceString = null;
			this.RemainingCustomsValueCalcEdit.DecimalPlaces = 2;
			this.RemainingCustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 20, true);
			this.RemainingCustomsValueCalcEdit.Name = "RemainingCustomsValueCalcEdit";
			this.RemainingCustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RemainingCustomsValueCalcEdit.TabIndex = 5;
			this.RemainingCustomsValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RemainingCustomsQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RemainingCustomsQuantityCalcEdit, "RemainingCustomsQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).RemainingCustomsQuantity)));
			this.RemainingCustomsQuantityCalcEdit.CaptionResourceString = null;
			this.RemainingCustomsQuantityCalcEdit.DecimalPlaces = 2;
			this.RemainingCustomsQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 20, true);
			this.RemainingCustomsQuantityCalcEdit.Name = "RemainingCustomsQuantityCalcEdit";
			this.RemainingCustomsQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RemainingCustomsQuantityCalcEdit.TabIndex = 7;
			this.RemainingCustomsQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RemainingGroupBox
			// 
			this.RemainingGroupBox.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("cd8d3dbf-83f5-4621-b51c-53aafd04b227", "Total Remaining");
			this.RemainingGroupBox.Controls.Add(this.RemainingCustomsValueCalcEdit);
			this.RemainingGroupBox.Controls.Add(this.RemainingCustomsQuantityCalcEdit);
			this.RemainingGroupBox.Controls.Add(this.RemainingNetWeightCalcDropEdit);
			this.RemainingGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.RemainingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 172, true);
			this.RemainingGroupBox.Name = "RemainingGroupBox";
			this.RemainingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 50, true);
			this.RemainingGroupBox.TabIndex = 8;
			this.RemainingGroupBox.TabStop = false;
			// 
			// EntryGroupBox
			// 
			this.EntryGroupBox.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("dc61ebbe-fe47-4e76-9cd3-7bc076a55d08", "Current Entry Values");
			this.EntryGroupBox.Controls.Add(this.CustomsValueCalcEdit);
			this.EntryGroupBox.Controls.Add(this.CustomsQuantityCalcEdit);
			this.EntryGroupBox.Controls.Add(this.NetWeightCalcDropEdit);
			this.EntryGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.EntryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryGroupBox.Name = "EntryGroupBox";
			this.EntryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 50, true);
			this.EntryGroupBox.TabIndex = 9;
			this.EntryGroupBox.TabStop = false;
			// 
			// RiskUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryGroupBox);
			this.Controls.Add(this.RiskGrid);
			this.Controls.Add(this.RemainingGroupBox);
			this.Name = "RiskUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(733, 222, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RiskGrid)).EndInit();
			this.RiskGrid.ResumeLayout(false);
			this.RiskGrid.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.RemainingNetWeightCalcDropEdit.ResumeLayout(true);
			this.RemainingNetWeightCalcDropEdit.PerformLayout();
			this.RemainingGroupBox.ResumeLayout(false);
			this.RemainingGroupBox.PerformLayout();
			this.EntryGroupBox.ResumeLayout(false);
			this.EntryGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid RiskGrid;
		private ZArchitecture.ZCalcEdit CustomsValueCalcEdit;
		private ZArchitecture.ZCalcEdit CustomsQuantityCalcEdit;
		private ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit RemainingNetWeightCalcDropEdit;
		private ZArchitecture.ZCalcEdit RemainingCustomsValueCalcEdit;
		private ZArchitecture.ZCalcEdit RemainingCustomsQuantityCalcEdit;
		private ZArchitecture.GUI.ZGroupBox RemainingGroupBox;
		private ZArchitecture.GUI.ZGroupBox EntryGroupBox;
	}
}
