using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.Aggregator
{
	partial class ReAggregateSinglePeriodForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.zPeriodEdit1 = new Enterprise.ZArchitecture.GUI.ZPeriodEdit();
			this.CompanyFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ExecuteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 124, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Aggregator.SinglePeriodReaggregator);
			// 
			// zPeriodEdit1
			// 
			this.BindingSource.SetBindingMember(this.zPeriodEdit1, "PeriodToReaggregate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Accounting.Business.Aggregator.SinglePeriodReaggregator)(null)).PeriodToReaggregate)));
			this.zPeriodEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReAggregateSinglePeriodForm|f8719678-7c62-40fe-a0ec-9e70206920a8", "Period");
			this.zPeriodEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 47, true);
			this.zPeriodEdit1.Name = "zPeriodEdit1";
			this.zPeriodEdit1.TabIndex = 3;
			// 
			// CompanyFindBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyFindBox, "Company");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Aggregator.SinglePeriodReaggregator)(null)).Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Aggregator.SinglePeriodReaggregator)(null)).Companies)));
			this.CompanyFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReAggregateSinglePeriodForm|7bb88f95-df39-459a-909e-2ad9b5695fa4", "Company");
			this.CompanyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 17, true);
			this.CompanyFindBox.Name = "CompanyFindBox";
			this.CompanyFindBox.PopupCaption = null;
			this.CompanyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CompanyFindBox.TabIndex = 1;
			// 
			// ExecuteButton
			// 
			this.ExecuteButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReAggregateSinglePeriodForm|bc15fa68-98fb-4946-87f3-e2fbfc0f8834", "Execute");
			this.ExecuteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 80, true);
			this.ExecuteButton.Name = "ExecuteButton";
			this.ExecuteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ExecuteButton.TabIndex = 4;
			this.ExecuteButton.UseVisualStyleBackColor = true;
			this.ExecuteButton.Click += new System.EventHandler(this.ExecuteButton_Click);
			// 
			// zButton2
			// 
			this.zButton2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReAggregateSinglePeriodForm|4557e540-3112-42ce-a622-9e371ee46301", "Cancel");
			this.zButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 80, true);
			this.zButton2.Name = "zButton2";
			this.zButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zButton2.TabIndex = 5;
			this.zButton2.UseVisualStyleBackColor = true;
			this.zButton2.Click += new System.EventHandler(this.zButton2_Click);
			// 
			// ReAggregateSinglePeriodForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 148, true);
			this.Controls.Add(this.ExecuteButton);
			this.Controls.Add(this.zButton2);
			this.Controls.Add(this.zPeriodEdit1);
			this.Controls.Add(this.CompanyFindBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Aggregator.SinglePeriodReaggregator);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Aggregator.SinglePeriodReaggregator";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 173, true);
			this.Name = "ReAggregateSinglePeriodForm";
			this.Controls.SetChildIndex(this.CompanyFindBox, 0);
			this.Controls.SetChildIndex(this.zPeriodEdit1, 0);
			this.Controls.SetChildIndex(this.zButton2, 0);
			this.Controls.SetChildIndex(this.ExecuteButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPeriodEdit zPeriodEdit1;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CompanyFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton ExecuteButton;
		private Enterprise.ZArchitecture.GUI.ZButton zButton2;
	}
}
