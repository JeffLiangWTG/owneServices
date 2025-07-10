using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class AWBRoundingRegistryControl : ChargeableWeightRoundingRegistryControl
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
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeableWeightRoundingGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.AWBRounding);
			// 
			// ChargeableWeightRoundingGrid
			// 
			this.ChargeableWeightRoundingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeableWeightRoundingGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.AWBRounding)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AWBRounding)(null)).AWBType)));

			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AWBRoundingRegistryControl|b4e522fb-355b-402f-95b4-779baeac8790", "AWB Type");
			zTextBoxColumnStyleInfo1.ColumnName = "AWBType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			this.ChargeableWeightRoundingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
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
			// AWBRoundingRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChargeableWeightRoundingGrid);
			this.Name = "AWBRoundingRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeableWeightRoundingGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

	}
}
