namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ItineraryUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ItineraryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItineraryGrid)).BeginInit();
			this.ItineraryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// ItineraryGrid
			// 
			this.ItineraryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItineraryGrid, "Itinerary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Itinerary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NonPersistentItineraryCountry)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Itinerary)).SyncRoot)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NonPersistentItineraryCountry)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Itinerary)).SyncRoot)).Sequence)));
			this.ItineraryGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("375210b5-5077-49d5-acd8-d38e10812465", "Ct.", "Country", "Country of Routing");
			zDropEditColumnStyleInfo1.ColumnName = "CountryCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("dfa80866-cd3a-491e-b337-7427bf2e1e48", "Seq.", "Sequence", "");
			zCalcEditColumnStyleInfo1.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.ItineraryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ItineraryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ItineraryGrid.CopySelectedRowsAllowed = true;
			this.ItineraryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItineraryGrid.GridId = "bea688a8-283e-434c-b792-291dd8f14de9";
			this.ItineraryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItineraryGrid.LayoutKey = "ItineraryGrid";
			this.ItineraryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItineraryGrid.Name = "ItineraryGrid";
			this.ItineraryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 79, true);
			this.ItineraryGrid.TabIndex = 1;
			// 
			// ItineraryUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ItineraryGrid);
			this.Name = "ItineraryUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 79, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItineraryGrid)).EndInit();
			this.ItineraryGrid.ResumeLayout(false);
			this.ItineraryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ItineraryGrid;

	}
}
