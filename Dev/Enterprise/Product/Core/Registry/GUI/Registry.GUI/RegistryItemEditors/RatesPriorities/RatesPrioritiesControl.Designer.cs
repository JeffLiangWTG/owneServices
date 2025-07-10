using System;

using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.Registry.Business;

namespace Enterprise.Registry.GUI
{
	partial class RatesPrioritiesControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.RatesPriorityGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RatesPriorityGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.RatesPrioritiesCollection);
			// 
			// RatesPriorityGrid
			// 
			this.RatesPriorityGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RatesPriorityGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RatesPriorities)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RatesPriorities)(null)).OrganizationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RatesPriorities)(null)).OrganizationTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RatesPriorities)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RatesPriorities)(null)).JobTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.RatesPriorities)(null)).UseCompanyTariff)));
			this.RatesPriorityGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "OrganizationTypeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f38e5797-99b8-49bd-849f-4057688fefc5", "Organization Type");
			zDropEditColumnStyleInfo1.ColumnName = "OrganizationType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "JobTypeList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("027FE65A-8840-4AE2-BFAA-9DC1D18E3C6C", "Job Type");
			zDropEditColumnStyleInfo2.ColumnName = "JobType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("83a3076a-2ec7-45f7-8bb2-9377977e08f9", "Use Comp Tariff", "Use Company Tariff", "");
			zCheckBoxColumnStyleInfo1.ColumnName = "UseCompanyTariff";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.RatesPriorityGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RatesPriorityGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RatesPriorityGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);

			this.RatesPriorityGrid.CopySelectedRowsAllowed = false;
			this.RatesPriorityGrid.Dock = System.Windows.Forms.DockStyle.Left;
			this.RatesPriorityGrid.GridId = "de55eeeb-a9bf-4de5-8a2f-3de7de245a4f";
			this.RatesPriorityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RatesPriorityGrid.LayoutKey = "RatesPriorityGrid";
			this.RatesPriorityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RatesPriorityGrid.Name = "RatesPriorityGrid";
			this.RatesPriorityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 389, true);
			this.RatesPriorityGrid.TabIndex = 0;
			// 
			// UpButton
			// 
			this.UpButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d0000d4c-cdf1-4c52-954f-9a833f7a79a8", "Move Up");
			this.UpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 3, true);
			this.UpButton.Name = "UpButton";
			this.UpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.UpButton.TabIndex = 1;
			this.UpButton.Click += new System.EventHandler(this.UpButton_Click);
			// 
			// DownButton
			// 
			this.DownButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b636d68b-ec33-4e4d-8439-5107cf7eec9e", "Move Down");
			this.DownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 32, true);
			this.DownButton.Name = "DownButton";
			this.DownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DownButton.TabIndex = 2;
			this.DownButton.Click += new System.EventHandler(this.DownButton_Click);
			// 
			// RatesPrioritiesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RatesPriorityGrid);
			this.Controls.Add(this.DownButton);
			this.Controls.Add(this.UpButton);
			this.Name = "RatesPrioritiesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 389, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RatesPriorityGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid RatesPriorityGrid;
		private Enterprise.ZArchitecture.GUI.ZButton UpButton;
		private Enterprise.ZArchitecture.GUI.ZButton DownButton;
	}
}
