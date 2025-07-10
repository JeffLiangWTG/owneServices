
namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class GoodsItemPreviousDocumentsGridUserControl
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
			this.PreviousDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocument>);
			// 
			// PreviousDocumentsGrid
			// 
			this.PreviousDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocument)(null)))));
			this.PreviousDocumentsGrid.CaptionVisible = false;
			this.PreviousDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsGrid.GridId = "e7f61047-75c2-4371-a606-3b7043ddaeac";
			this.PreviousDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreviousDocumentsGrid.LayoutKey = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsGrid.Name = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 135, true);
			this.PreviousDocumentsGrid.TabIndex = 0;
			// 
			// GoodsItemPreviousDocumentsGridUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PreviousDocumentsGrid);
			this.Name = "GoodsItemPreviousDocumentsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal ZArchitecture.ZGrid PreviousDocumentsGrid;
	}
}
