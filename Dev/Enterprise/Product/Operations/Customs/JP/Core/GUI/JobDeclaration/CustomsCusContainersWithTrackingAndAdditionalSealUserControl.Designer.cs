using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.JP.GUI
{
	partial class CustomsCusContainersWithTrackingAndAdditionalSealUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AdditionalSealsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalSealsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.containersUserControl1.SuspendLayout();
			this.BaseAllPanel.SuspendLayout();
			this.BaseContainerPanel.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).BeginInit();
			this.CusContainersBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalSealsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).BeginInit();
			this.AdditionalSealsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BaseContainerPanel
			// 
			this.BaseContainerPanel.Controls.Add(this.AdditionalSealsGroupBox);
			this.BaseContainerPanel.Controls.SetChildIndex(this.AdditionalSealsGroupBox, 0);
			this.BaseContainerPanel.Controls.SetChildIndex(this.ContainersGroupBox, 0);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 140, true);
			// 
			// CusContainersBoundGrid
			// 
			// 
			// 
			// 
			this.CusContainersBoundGrid.InnerGrid.AllowNavigation = false;
			this.CusContainersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CusContainersBoundGrid.InnerGrid.CaptionVisible = false;
			this.CusContainersBoundGrid.InnerGrid.GridId = "GridLayoutV6/wYdjdj5Cz4Nc7hGpbKA==";
			this.CusContainersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusContainersBoundGrid.InnerGrid.LayoutKey = "CusContainersBoundGrid";
			this.CusContainersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.CusContainersBoundGrid.InnerGrid.Name = "Grid";
			this.CusContainersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 79, true);
			this.CusContainersBoundGrid.InnerGrid.TabIndex = 0;
			this.CusContainersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 115, true);
			// 
			// RequiresMergeLabel
			// 
			this.RequiresMergeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(-1, 2, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusContainer);
			// 
			// AdditionalSealsGroupBox
			// 
			this.AdditionalSealsGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("6f585003-5e71-4c1d-b067-22a84ea645e4", "Additional Seals");
			this.AdditionalSealsGroupBox.Controls.Add(this.AdditionalSealsGrid);
			this.AdditionalSealsGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.AdditionalSealsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(672, 0, true);
			this.AdditionalSealsGroupBox.Name = "AdditionalSealsGroupBox";
			this.AdditionalSealsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 140, true);
			this.AdditionalSealsGroupBox.TabIndex = 1;
			this.AdditionalSealsGroupBox.TabStop = false;
			// 
			// AdditionalSealsGrid
			// 
			this.AdditionalSealsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalSealsGrid, "AdditionalSeals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusContainer)(null)).AdditionalSeals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.CusContainer)(null)).AdditionalSeals)).SyncRoot)).BK_SealNumber)));
			this.AdditionalSealsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BK_SealNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AdditionalSealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalSealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalSealsGrid.GridId = "556e73c2-132f-4e8e-9a72-34d9347850f9";
			this.AdditionalSealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalSealsGrid.LayoutKey = "AdditionalSealsGrid";
			this.AdditionalSealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.AdditionalSealsGrid.Name = "AdditionalSealsGrid";
			this.AdditionalSealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 115, true);
			this.AdditionalSealsGrid.TabIndex = 0;
			// 
			// CustomsCusContainersWithTrackingAndAdditionalSealUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "CustomsCusContainersWithTrackingAndAdditionalSealUserControl";
			this.containersUserControl1.ResumeLayout(true);
			this.containersUserControl1.PerformLayout();
			this.BaseAllPanel.ResumeLayout(false);
			this.BaseAllPanel.PerformLayout();
			this.BaseContainerPanel.ResumeLayout(false);
			this.BaseContainerPanel.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).EndInit();
			this.CusContainersBoundGrid.ResumeLayout(true);
			this.CusContainersBoundGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalSealsGroupBox.ResumeLayout(false);
			this.AdditionalSealsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).EndInit();
			this.AdditionalSealsGrid.ResumeLayout(false);
			this.AdditionalSealsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZGroupBox AdditionalSealsGroupBox;
		Enterprise.ZArchitecture.ZGrid AdditionalSealsGrid;
	}
}
