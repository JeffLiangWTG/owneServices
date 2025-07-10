using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class ChargeableWeightRoundingRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ChargeableWeightRoundingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeableWeightRoundingGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ChargeableWeightRounding);
			// 
			// ChargeableWeightRoundingGrid
			// 
			this.ChargeableWeightRoundingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeableWeightRoundingGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeableWeightRounding)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeableWeightRounding)(null)).RoundingMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeableWeightRounding)(null)).RoundingModes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeableWeightRounding)(null)).RoundingScale)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ChargeableWeightRounding)(null)).RoundingScales)));
			this.ChargeableWeightRoundingGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "RoundingModes";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ChargeableWeightRoundingRegistryControl|902ca793-5f89-4060-b423-af1e972e69a1", "Rounding Mode");
			zDropEditColumnStyleInfo1.ColumnName = "RoundingMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "RoundingScales";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ChargeableWeightRoundingRegistryControl|ed8b2331-fddb-47d0-9347-dd849e774a0a", "Rounding Scale");
			zDropEditColumnStyleInfo2.ColumnName = "RoundingScale";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ChargeableWeightRoundingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ChargeableWeightRoundingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ChargeableWeightRoundingGrid.GridId = "1705533f-d811-4012-a72c-ff7d03a1b1a3";
			this.ChargeableWeightRoundingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeableWeightRoundingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeableWeightRoundingGrid.LayoutKey = "ChargeableWeightRoundingGrid";
			this.ChargeableWeightRoundingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeableWeightRoundingGrid.Name = "ChargeableWeightRoundingGrid";
			this.ChargeableWeightRoundingGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ChargeableWeightRoundingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			this.ChargeableWeightRoundingGrid.TabIndex = 0;
			// 
			// ChargeableWeightRoundingRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChargeableWeightRoundingGrid);
			this.Name = "ChargeableWeightRoundingRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeableWeightRoundingGrid)).EndInit();
			this.ResumeLayout(false);

		}

        #endregion

        protected internal Enterprise.ZArchitecture.ZGrid ChargeableWeightRoundingGrid;
	}
}
