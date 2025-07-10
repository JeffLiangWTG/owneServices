
namespace Enterprise.Customs.IT.GUI
{
	partial class GroupedPreviousDocumentsUserControl
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
			this.M2LinesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.M2LinesGrid)).BeginInit();
			this.M2LinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.GroupedPreviousDocumentCollection);
			// 
			// M2LinesGrid
			// 
			this.M2LinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.M2LinesGrid, ".");
			//// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.Business.GroupedPreviousDocument)(null)))));
			this.M2LinesGrid.CaptionVisible = false;
			this.M2LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.M2LinesGrid.GridId = "fc6920b1-e797-4743-a5f6-928c8138e1ac";
			this.M2LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.M2LinesGrid.LayoutKey = "M2LinesGrid";
			this.M2LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.M2LinesGrid.Name = "M2LinesGrid";
			this.M2LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 274, true);
			this.M2LinesGrid.TabIndex = 0;
			// 
			// GroupedPreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.M2LinesGrid);
			this.Name = "GroupedPreviousDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.M2LinesGrid)).EndInit();
			this.M2LinesGrid.ResumeLayout(false);
			this.M2LinesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid M2LinesGrid;
	}
}
