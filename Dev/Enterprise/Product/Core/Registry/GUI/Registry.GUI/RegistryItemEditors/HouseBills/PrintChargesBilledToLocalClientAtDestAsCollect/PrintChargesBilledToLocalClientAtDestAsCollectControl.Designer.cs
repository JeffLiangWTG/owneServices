namespace Enterprise.Registry.GUI
{
	partial class PrintChargesBilledToLocalClientAtDestAsCollectControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintChargesBilledToLocalClientAtDestAsCollectGrid)).BeginInit();
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.PrintChargesBilledToLocalClientAtDestAsCollectCollection);
			// 
			// PrintChargesBilledToLocalClientAtDestAsCollectGrid
			// 
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.AllowNavigation = false;
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
						| System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PrintChargesBilledToLocalClientAtDestAsCollectGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.PrintChargesBilledToLocalClientAtDestAsCollect)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.PrintChargesBilledToLocalClientAtDestAsCollect)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.PrintChargesBilledToLocalClientAtDestAsCollect)(null)).ExportCountry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.PrintChargesBilledToLocalClientAtDestAsCollect)(null)).ImportCountry)));
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2406fd0d-6cc2-4fda-8184-585c91155d2a", "Transport Mode");
			zDropEditColumnStyleInfo1.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("498d243f-9572-4081-921d-8c135ba17b37", "Export Country");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ExportCountry";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4ee1c1d1-1f6f-4c08-a257-7e1dd5ed53ea", "Import Country");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ImportCountry";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82);
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.GridId = "0628b79a-aa94-493a-9668-276611257a9c";
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.LayoutKey = "PrintChargesBilledToLocalClientAtDestAsCollectGrid";
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.Name = "PrintChargesBilledToLocalClientAtDestAsCollectGrid";
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 218, true);
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.TabIndex = 0;
			// 
			// PrintChargesBilledToLocalClientAtDestAsCollectControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PrintChargesBilledToLocalClientAtDestAsCollectGrid);
			this.Name = "PrintChargesBilledToLocalClientAtDestAsCollectControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 221, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrintChargesBilledToLocalClientAtDestAsCollectGrid)).EndInit();
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.ResumeLayout(false);
			this.PrintChargesBilledToLocalClientAtDestAsCollectGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid PrintChargesBilledToLocalClientAtDestAsCollectGrid;
	}
}
