
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Registry.GUI
{
	partial class ContainerPenaltyFreeDaysOptionsRegistryItemControl
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
			this.FreeDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnlimitedFreeDaysCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ContainerPenaltyFreeDaysOptions);
			// 
			// FreeDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.FreeDaysCalcEdit, "FreeDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ContainerPenaltyFreeDaysOptions)(null)).FreeDays)));
			this.FreeDaysCalcEdit.CaptionResourceString = Res.GetData("0DBC3E48-EBA1-433C-87B1-2B44A59B40EB", "Free Days");
			this.FreeDaysCalcEdit.DecimalPlaces = 0;
			this.FreeDaysCalcEdit.Decimals = 0;
			this.FreeDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 16, true);
			this.FreeDaysCalcEdit.Name = "FreeDaysCalcEdit";
			this.FreeDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.FreeDaysCalcEdit.TabIndex = 0;
			this.FreeDaysCalcEdit.Text = "0";
			this.FreeDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NoFreeDaysGivenCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UnlimitedFreeDaysCheckBox, "UnlimitedFreeDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.ContainerPenaltyFreeDaysOptions)(null)).UnlimitedFreeDays)));
			this.UnlimitedFreeDaysCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnlimitedFreeDaysCheckBox.CaptionResourceString = Res.GetData("7CE020EF-E9D8-4988-8F33-7231FFC27280", "Unlimited Free Days");
			this.UnlimitedFreeDaysCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 14, true);
			this.UnlimitedFreeDaysCheckBox.Name = "UnlimitedFreeDaysCheckBox";
			this.UnlimitedFreeDaysCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 24, true);
			this.UnlimitedFreeDaysCheckBox.TabIndex = 1;
			// 
			// ContainerPenaltyFreeDaysOptionsRegistryItemControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.FreeDaysCalcEdit);
			this.Controls.Add(this.UnlimitedFreeDaysCheckBox);
			this.Name = "ContainerPenaltyFreeDaysOptionsRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 57, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZCalcEdit FreeDaysCalcEdit;
		ZArchitecture.GUI.ZCheckBox UnlimitedFreeDaysCheckBox;
		#endregion
	}
}
