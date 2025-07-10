namespace Enterprise.Registry.GUI
{
	partial class DefaultMinimumStayAndTravelTimeControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.DefaultMinimumStayAndTravelTimeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultMinimumStayAndTravelTimeGrid)).BeginInit();
			this.DefaultMinimumStayAndTravelTimeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DefaultMinimumStayAndTravelTimeCollection);
			// 
			// DefaultMinimumStayAndTravelTimeGrid
			// 
			this.DefaultMinimumStayAndTravelTimeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DefaultMinimumStayAndTravelTimeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DefaultMinimumStayAndTravelTime)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DefaultMinimumStayAndTravelTime)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Registry.Business.DefaultMinimumStayAndTravelTime)(null)).StayTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Registry.Business.DefaultMinimumStayAndTravelTime)(null)).TravelTime)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DefaultMinimumStayAndTravelTimeControl|a67353ab-0171-421d-b18d-a92325c71152", "Transport Mode (Code)");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "TransportMode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DefaultMinimumStayAndTravelTimeControl|95c78632-4e12-467e-a9b3-0e23d736ae0c", "Minimum Stay Time (Minutes)");
			zCalcEditColumnStyleInfo1.ColumnName = "StayTime";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DefaultMinimumStayAndTravelTimeControl|57c44795-83fd-4e0f-b3d2-98bc6acd2bd3", "Minimum Travel Time (Minutes)");
			zCalcEditColumnStyleInfo2.ColumnName = "TravelTime";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.DefaultMinimumStayAndTravelTimeGrid.CaptionVisible = false;
			this.DefaultMinimumStayAndTravelTimeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DefaultMinimumStayAndTravelTimeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DefaultMinimumStayAndTravelTimeGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.DefaultMinimumStayAndTravelTimeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DefaultMinimumStayAndTravelTimeGrid.GridId = "f8cc2b6c-5d49-43c1-b42d-3f0cfcba85ac";
			this.DefaultMinimumStayAndTravelTimeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultMinimumStayAndTravelTimeGrid.LayoutKey = "DefaultMinimumStayAndTravelTimeGrid";
			this.DefaultMinimumStayAndTravelTimeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DefaultMinimumStayAndTravelTimeGrid.Name = "DefaultMinimumStayAndTravelTimeGrid";
			this.DefaultMinimumStayAndTravelTimeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 400, true);
			this.DefaultMinimumStayAndTravelTimeGrid.TabIndex = 0;
			// 
			// DefaultMinimumStayAndTravelTimeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.AutoScroll = true;
			this.Controls.Add(this.DefaultMinimumStayAndTravelTimeGrid);
			this.Name = "DefaultMinimumStayAndTravelTimeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultMinimumStayAndTravelTimeGrid)).EndInit();
			this.DefaultMinimumStayAndTravelTimeGrid.ResumeLayout(false);
			this.DefaultMinimumStayAndTravelTimeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid DefaultMinimumStayAndTravelTimeGrid;
	}
}
