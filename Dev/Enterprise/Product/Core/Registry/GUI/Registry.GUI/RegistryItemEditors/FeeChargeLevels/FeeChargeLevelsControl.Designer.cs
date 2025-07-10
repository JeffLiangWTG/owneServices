using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
namespace Enterprise.Registry.GUI
{
	partial class FeeChargeLevelsControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.FeeChargeTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FeeChargeLevelsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.FeeChargeTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FeeChargeTypeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeeChargeLevelsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FeeChargeTypesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.FeeChargeLevelsSection);
			// 
			// FeeChargeTypeGroupBox
			// 
			this.FeeChargeTypeGroupBox.Controls.Add(this.FeeChargeLevelsGrid);
			this.FeeChargeTypeGroupBox.Controls.Add(this.FeeChargeTypesGrid);
			this.FeeChargeTypeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FeeChargeTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FeeChargeTypeGroupBox.Name = "FeeChargeTypeGroupBox";
			this.FeeChargeTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 407, true);
			this.FeeChargeTypeGroupBox.TabIndex = 0;
			this.FeeChargeTypeGroupBox.TabStop = false;
			this.FeeChargeTypeGroupBox.Text = Res.GetString("bd5d3ff1-291a-4e2f-a7c8-9f158b12b35e", "Fees And Charges-levels");
			// 
			// FeeChargeLevelsGrid
			// 
			this.FeeChargeLevelsGrid.AllowNavigation = false;
			this.FeeChargeLevelsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FeeChargeLevelsGrid, "FeeChargeTypes.FeeChargeLevels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).FeeChargeLevels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FeeChargeLevel)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).FeeChargeLevels)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FeeChargeLevel)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).FeeChargeLevels)).SyncRoot)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FeeChargeLevel)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).FeeChargeLevels)).SyncRoot)).Amount1Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.FeeChargeLevel)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).FeeChargeLevels)).SyncRoot)).Amount1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FeeChargeLevel)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).FeeChargeLevels)).SyncRoot)).Amount1Currency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FeeChargeLevel)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).FeeChargeLevels)).SyncRoot)).Amount2Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.FeeChargeLevel)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).FeeChargeLevels)).SyncRoot)).Amount2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FeeChargeLevel)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).FeeChargeLevels)).SyncRoot)).Amount2Currency)));
			this.FeeChargeLevelsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4564ba32-1092-43a8-a8ab-b0108905e09f", "Level");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("25117d90-e039-4a58-b000-3990ad213580", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("9ecf36c1-b80f-4ddb-8fa6-252c6b80da6f", "Type 1", "Min/Excess/None", "");
			zDropEditColumnStyleInfo1.ColumnName = "Amount1Type";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bb94d651-6058-43d0-9ea0-a9547e073e67", "Value");
			zCalcEditColumnStyleInfo1.ColumnName = "Amount1";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e5cf3c28-dd6b-4100-a1dc-55a26401154f", "Currency");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Amount1Currency";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bc75c7af-49d7-4ec3-a092-1e985eded1d1", "Type 2", "Max/None", "");
			zDropEditColumnStyleInfo2.ColumnName = "Amount2Type";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b2c9ea9b-7f59-49f2-bd31-14b4ec36cdf5", "Value");
			zCalcEditColumnStyleInfo2.ColumnName = "Amount2";
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("34fedf87-b941-4509-91b2-f76bd2c767b3", "Currency");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Amount2Currency";
			this.FeeChargeLevelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FeeChargeLevelsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FeeChargeLevelsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FeeChargeLevelsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FeeChargeLevelsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.FeeChargeLevelsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.FeeChargeLevelsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FeeChargeLevelsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.FeeChargeLevelsGrid.CopySelectedRowsAllowed = true;
			this.FeeChargeLevelsGrid.GridId = "ed58eedf-6f10-4178-ab80-46cc0060e0a0";
			this.FeeChargeLevelsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeeChargeLevelsGrid.LayoutKey = "FeeChargeLevelsGrid";
			this.FeeChargeLevelsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 197, true);
			this.FeeChargeLevelsGrid.Name = "FeeChargeLevelsGrid";
			this.FeeChargeLevelsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 204, true);
			this.FeeChargeLevelsGrid.TabIndex = 1;
			// 
			// FeeChargeTypesGrid
			// 
			this.FeeChargeTypesGrid.AllowNavigation = false;
			this.FeeChargeTypesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FeeChargeTypesGrid, "FeeChargeTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.FeeChargeType)(((System.Collections.IList)(((Enterprise.Registry.Business.FeeChargeLevelsSection)(null)).FeeChargeTypes)).SyncRoot)).EnglishDescription)));
			this.FeeChargeTypesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e62fb806-4bcb-4b12-95c3-94410bcdf038", "Code");
			zTextBoxColumnStyleInfo3.ColumnName = "Code";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("86762418-96b1-4f4b-bfd9-78ee694e9d02", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "EnglishDescription";
			this.FeeChargeTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FeeChargeTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FeeChargeTypesGrid.CopySelectedRowsAllowed = true;
			this.FeeChargeTypesGrid.GridId = "d6a984e9-a582-47cd-bef7-c5032a596140";
			this.FeeChargeTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeeChargeTypesGrid.LayoutKey = "zGrid1";
			this.FeeChargeTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FeeChargeTypesGrid.Name = "FeeChargeTypesGrid";
			this.FeeChargeTypesGrid.PreferredColumnWidth = 100;
			this.FeeChargeTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 178, true);
			this.FeeChargeTypesGrid.TabIndex = 0;
			// 
			// FeeChargeLevelsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FeeChargeTypeGroupBox);
			this.Name = "FeeChargeLevelsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 407, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FeeChargeTypeGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.FeeChargeLevelsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FeeChargeTypesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox FeeChargeTypeGroupBox;
		private ZArchitecture.ZGrid FeeChargeTypesGrid;
		private ZArchitecture.ZGrid FeeChargeLevelsGrid;

	}
}
