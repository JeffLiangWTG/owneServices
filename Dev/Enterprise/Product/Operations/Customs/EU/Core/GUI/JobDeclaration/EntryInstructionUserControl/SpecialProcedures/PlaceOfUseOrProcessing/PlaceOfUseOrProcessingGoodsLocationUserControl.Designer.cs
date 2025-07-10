namespace Enterprise.Customs.EU.GUI
{
	partial class PlaceOfUseOrProcessingGoodsLocationUserControl
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
		private void InitializeComponent()
		{
			Enterprise.Customs.EU.GUI.PlaceOfUseOrProcessingColumnStyleInfo placeOfUseOrProcessingColumnStyleInfo1 = new Enterprise.Customs.EU.GUI.PlaceOfUseOrProcessingColumnStyleInfo();
			this.PlacesOfUseOrProcessingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PlacesOfUseOrProcessingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PlacesOfUseOrProcessingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PlacesOfUseOrProcessingGrid)).BeginInit();
			this.PlacesOfUseOrProcessingGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// PlacesOfUseOrProcessingGroupBox
			//
			this.PlacesOfUseOrProcessingGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("54B097BA-B8B7-454F-A814-68C87A82F72C", "Places of Use or Processing");
			this.PlacesOfUseOrProcessingGroupBox.Controls.Add(this.PlacesOfUseOrProcessingGrid);
			this.PlacesOfUseOrProcessingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PlacesOfUseOrProcessingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PlacesOfUseOrProcessingGroupBox.Name = "PlacesOfUseOrProcessingGroupBox";
			this.PlacesOfUseOrProcessingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 144, true);
			this.PlacesOfUseOrProcessingGroupBox.TabIndex = 1;
			this.PlacesOfUseOrProcessingGroupBox.TabStop = false;
			// 
			// PlacesOfUseOrProcessingGrid
			// 
			this.PlacesOfUseOrProcessingGrid.AllowNavigation = false;
			this.PlacesOfUseOrProcessingGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.PlacesOfUseOrProcessingGrid, "CustomsEntryInstructions.PlaceOfUseOrProcessingCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PlaceOfUseOrProcessingCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.PlaceOfUseOrProcessing)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PlaceOfUseOrProcessingCollection)).SyncRoot)).DisplayText)));
			this.PlacesOfUseOrProcessingGrid.CaptionVisible = false;
			placeOfUseOrProcessingColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("7F0CA8DE-8E38-4907-91E7-438BCE5BF76F", "Goods Location", "[Annex A 4/9] Dates, Times, Periods and Places > Place(s) of Use or Processing");
			placeOfUseOrProcessingColumnStyleInfo1.ColumnName = "DisplayText";
			placeOfUseOrProcessingColumnStyleInfo1.DefaultCollectionIndex = 0;
			placeOfUseOrProcessingColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PlacesOfUseOrProcessingGrid.ColumnStyles.Add(placeOfUseOrProcessingColumnStyleInfo1);
			this.PlacesOfUseOrProcessingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PlacesOfUseOrProcessingGrid.GridId = "956946b4-cde4-4dd2-b849-e2b8447e19f4";
			this.PlacesOfUseOrProcessingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PlacesOfUseOrProcessingGrid.LayoutKey = "PlacesOfUseOrProcessingGrid";
			this.PlacesOfUseOrProcessingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PlacesOfUseOrProcessingGrid.Name = "PlacesOfUseOrProcessingGrid";
			this.PlacesOfUseOrProcessingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 125, true);
			this.PlacesOfUseOrProcessingGrid.TabIndex = 0;
			// 
			// PlaceOfUseOrProcessingGoodsLocationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PlacesOfUseOrProcessingGroupBox);
			this.Name = "PlaceOfUseOrProcessingGoodsLocationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PlacesOfUseOrProcessingGroupBox.ResumeLayout(false);
			this.PlacesOfUseOrProcessingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PlacesOfUseOrProcessingGrid)).EndInit();
			this.PlacesOfUseOrProcessingGrid.ResumeLayout(false);
			this.PlacesOfUseOrProcessingGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox PlacesOfUseOrProcessingGroupBox;
		internal ZArchitecture.ZGrid PlacesOfUseOrProcessingGrid;
	}
}
