using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	partial class DelayFactorControl
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
			this.HVSDelayIntervalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.HVSDelayIntervalTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CONDelayIntervalTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CONDelayIntervalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HVSDelayIntervalTypeDropEdit.SuspendLayout();
			this.CONDelayIntervalTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Registry.DelayFactorRegistryBusinessObject);
			// 
			// HVSDelayIntervalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.HVSDelayIntervalCalcEdit, "HVSDelayInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Registry.DelayFactorRegistryBusinessObject)(null)).HVSDelayInterval)));
			this.HVSDelayIntervalCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2fd2b168-3834-4fd1-bf40-f0085884723f", "HVS Factor");
			this.HVSDelayIntervalCalcEdit.DecimalPlaces = 0;
			this.HVSDelayIntervalCalcEdit.Decimals = 0;
			this.HVSDelayIntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 3, true);
			this.HVSDelayIntervalCalcEdit.Name = "HVSDelayIntervalCalcEdit";
			this.HVSDelayIntervalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.HVSDelayIntervalCalcEdit.TabIndex = 1;
			this.HVSDelayIntervalCalcEdit.Text = "0";
			this.HVSDelayIntervalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// HVSDelayIntervalTypeDropEdit
			// 
			this.HVSDelayIntervalTypeDropEdit.AllowDrop = true;
			this.HVSDelayIntervalTypeDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HVSDelayIntervalTypeDropEdit, "HVSDelayIntervalType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Registry.DelayFactorRegistryBusinessObject)(null)).HVSDelayIntervalType)));
			this.HVSDelayIntervalTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 3, true);
			this.HVSDelayIntervalTypeDropEdit.Name = "HVSDelayIntervalTypeDropEdit";
			this.HVSDelayIntervalTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.HVSDelayIntervalTypeDropEdit.TabIndex = 2;
			// 
			// CONDelayIntervalTypeDropEdit
			// 
			this.CONDelayIntervalTypeDropEdit.AllowDrop = true;
			this.CONDelayIntervalTypeDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CONDelayIntervalTypeDropEdit, "CONDelayIntervalType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Registry.DelayFactorRegistryBusinessObject)(null)).CONDelayIntervalType)));
			this.CONDelayIntervalTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 29, true);
			this.CONDelayIntervalTypeDropEdit.Name = "CONDelayIntervalTypeDropEdit";
			this.CONDelayIntervalTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.CONDelayIntervalTypeDropEdit.TabIndex = 4;
			// 
			// CONDelayIntervalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CONDelayIntervalCalcEdit, "CONDelayInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Registry.DelayFactorRegistryBusinessObject)(null)).CONDelayInterval)));
			this.CONDelayIntervalCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cbe6ea70-73c0-4cab-bb89-fdba57b02554", "CourierLVS-F Factor");
			this.CONDelayIntervalCalcEdit.DecimalPlaces = 0;
			this.CONDelayIntervalCalcEdit.Decimals = 0;
			this.CONDelayIntervalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 29, true);
			this.CONDelayIntervalCalcEdit.Name = "CONDelayIntervalCalcEdit";
			this.CONDelayIntervalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.CONDelayIntervalCalcEdit.TabIndex = 3;
			this.CONDelayIntervalCalcEdit.Text = "0";
			this.CONDelayIntervalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DelayFactorControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CONDelayIntervalTypeDropEdit);
			this.Controls.Add(this.CONDelayIntervalCalcEdit);
			this.Controls.Add(this.HVSDelayIntervalTypeDropEdit);
			this.Controls.Add(this.HVSDelayIntervalCalcEdit);
			this.Name = "DelayFactorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 95, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HVSDelayIntervalTypeDropEdit.ResumeLayout(true);
			this.HVSDelayIntervalTypeDropEdit.PerformLayout();
			this.CONDelayIntervalTypeDropEdit.ResumeLayout(true);
			this.CONDelayIntervalTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.ZCalcEdit HVSDelayIntervalCalcEdit;
		private ZArchitecture.GUI.ZDropEdit HVSDelayIntervalTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit CONDelayIntervalTypeDropEdit;
		public ZArchitecture.ZCalcEdit CONDelayIntervalCalcEdit;
	}
}
