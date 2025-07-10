using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{ 
	partial class DeleteExpiredRatesUserControl
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
            this.ExpiredPeriodInYearsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.BatchSizeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.DeleteExpiredRatesWrapper);
            // 
            // ExpiredPeriodInYearsCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ExpiredPeriodInYearsCalcEdit, "ExpiredRatesPeriodInYears");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.GUI.DeleteExpiredRatesWrapper)(null)).ExpiredRatesPeriodInYears)));
            this.ExpiredPeriodInYearsCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c234dbf8-7ff8-4b95-af3f-63fb4486dd2a", "Expired Period in Years");
            this.ExpiredPeriodInYearsCalcEdit.DecimalPlaces = 0;
            this.ExpiredPeriodInYearsCalcEdit.Decimals = 0;
            this.ExpiredPeriodInYearsCalcEdit.IsCalculatorEnabled = false;
            this.ExpiredPeriodInYearsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 10, true);
            this.ExpiredPeriodInYearsCalcEdit.Name = "ExpiredPeriodInYearsCalcEdit";
            this.ExpiredPeriodInYearsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 16, true);
            this.ExpiredPeriodInYearsCalcEdit.TabIndex = 1;
            this.ExpiredPeriodInYearsCalcEdit.TabStop = false;
            this.ExpiredPeriodInYearsCalcEdit.Text = "0";
            this.ExpiredPeriodInYearsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ExpiredPeriodInYearsCalcEdit.TrackDisposedAccess = true;
			this.ExpiredPeriodInYearsCalcEdit.AllowNegative = false;
            // 
            // BatchSizeCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.BatchSizeCalcEdit, "BatchSize");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.GUI.DeleteExpiredRatesWrapper)(null)).BatchSize)));
            this.BatchSizeCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("e6f9eedb-c1cf-4709-b410-2a629282a3c5", "Batch Size");
            this.BatchSizeCalcEdit.DecimalPlaces = 0;
            this.BatchSizeCalcEdit.Decimals = 0;
            this.BatchSizeCalcEdit.IsCalculatorEnabled = false;
            this.BatchSizeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 36, true);
            this.BatchSizeCalcEdit.Name = "BatchSizeCalcEdit";
            this.BatchSizeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 16, true);
            this.BatchSizeCalcEdit.TabIndex = 2;
            this.BatchSizeCalcEdit.TabStop = false;
            this.BatchSizeCalcEdit.Text = "0";
            this.BatchSizeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.BatchSizeCalcEdit.TrackDisposedAccess = true;
			this.BatchSizeCalcEdit.AllowNegative = false;
            // 
            // DeleteExpiredRatesUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ExpiredPeriodInYearsCalcEdit);
            this.Controls.Add(this.BatchSizeCalcEdit);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 170, true);
            this.Name = "DeleteExpiredRatesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 170, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit ExpiredPeriodInYearsCalcEdit;
		private ZArchitecture.ZCalcEdit BatchSizeCalcEdit;
	}
}
