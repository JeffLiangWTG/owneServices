namespace Enterprise.Registry.GUI
{
	partial class IndexShardRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RetentionTimeGrid = new Enterprise.Registry.GUI.IndexShardRegistryGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RetentionTimeGrid)).BeginInit();
			this.RetentionTimeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.IndexShardList);
			// 
			// RetentionTimeGrid
			// 
			this.RetentionTimeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RetentionTimeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IndexShard)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.IndexShard)(null)).IndexTableName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.IndexShard)(null)).IndexTableNames)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.IndexShard)(null)).IsOverridden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Registry.Business.IndexShard)(null)).ShardIndex)));
			this.RetentionTimeGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "IndexTableNames";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("936252c9-0f4b-4b0c-8956-ec0d0a1d5972", "Index Table Name", "Table Name");
			zDropEditColumnStyleInfo1.ColumnName = "IndexTableName";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bc792474-f217-4555-8663-7b09b532b7f5", "Is Overridden", "If the shard is overridden");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsOverridden";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f50b01e8-40e8-42d3-8af7-d6b5b93efca6", "Shard Index", "Table Shard Index");
			zTextBoxColumnStyleInfo1.ColumnName = "ShardIndex";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.RetentionTimeGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RetentionTimeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RetentionTimeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RetentionTimeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RetentionTimeGrid.GridId = "90430895-2074-470f-b245-cd0081b8a15d";
			this.RetentionTimeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RetentionTimeGrid.LayoutKey = "Grid";
			this.RetentionTimeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RetentionTimeGrid.Name = "RetentionTimeGrid";
			this.RetentionTimeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 285, true);
			this.RetentionTimeGrid.TabIndex = 0;
			// 
			// IndexShardRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RetentionTimeGrid);
			this.Name = "IndexShardRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 285, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RetentionTimeGrid)).EndInit();
			this.RetentionTimeGrid.ResumeLayout(false);
			this.RetentionTimeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal IndexShardRegistryGrid RetentionTimeGrid;
	}
}
