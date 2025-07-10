#if DEBUG
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Testing.UtilityClasses
{
	[SuppressFormsLocalizedTest]
	partial class DocBuilderTemplateUpdaterForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UpdateTemplateCommandsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExecuteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveDocBuilderTemplatesToFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UpdateTemplateCommandsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 467, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.DocBuilderTemplateUpdater);
			// 
			// UpdateTemplateCommandsGrid
			// 
			this.UpdateTemplateCommandsGrid.AllowNavigation = false;
			this.UpdateTemplateCommandsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.UpdateTemplateCommandsGrid, "UpdateTemplateCommands");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.DocBuilderTemplateUpdater)(null)).UpdateTemplateCommands)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.UpdateTemplateCommand)(((System.Collections.IList)(((Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.DocBuilderTemplateUpdater)(null)).UpdateTemplateCommands)).SyncRoot)).Description)));
			this.UpdateTemplateCommandsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.UpdateTemplateCommandsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UpdateTemplateCommandsGrid.GridId = "a792d2a1-a976-4302-83fd-752e4825030a";
			this.UpdateTemplateCommandsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UpdateTemplateCommandsGrid.LayoutKey = "UpdateTemplateCommandsGrid";
			this.UpdateTemplateCommandsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.UpdateTemplateCommandsGrid.Name = "UpdateTemplateCommandsGrid";
			this.UpdateTemplateCommandsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 420, true);
			this.UpdateTemplateCommandsGrid.TabIndex = 0;
			// 
			// ExecuteButton
			// 
			this.ExecuteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExecuteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 438, true);
			this.ExecuteButton.Name = "ExecuteButton";
			this.ExecuteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ExecuteButton.TabIndex = 2;
			this.ExecuteButton.Text = "Execute";
			this.ExecuteButton.UseVisualStyleBackColor = true;
			this.ExecuteButton.Click += new System.EventHandler(this.ExecuteButton_Click);
			// 
			// SaveDocBuilderTemplatesToFileButton
			// 
			this.SaveDocBuilderTemplatesToFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveDocBuilderTemplatesToFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 438, true);
			this.SaveDocBuilderTemplatesToFileButton.Name = "SaveDocBuilderTemplatesToFileButton";
			this.SaveDocBuilderTemplatesToFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 23, true);
			this.SaveDocBuilderTemplatesToFileButton.TabIndex = 1;
			this.SaveDocBuilderTemplatesToFileButton.Text = "Save DocBuilder Templates To File";
			this.SaveDocBuilderTemplatesToFileButton.UseVisualStyleBackColor = true;
			this.SaveDocBuilderTemplatesToFileButton.Click += new System.EventHandler(this.SaveDocBuilderTemplatesToFileButton_Click);
			// 
			// DocBuilderTemplateUpdaterForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 491, true);
			this.Controls.Add(this.UpdateTemplateCommandsGrid);
			this.Controls.Add(this.SaveDocBuilderTemplatesToFileButton);
			this.Controls.Add(this.ExecuteButton);
			this.DataSourceType = typeof(Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.DocBuilderTemplateUpdater);
			this.Name = "DocBuilderTemplateUpdaterForm";
			this.Text = "DocBuilder Template Updater";
			this.Controls.SetChildIndex(this.ExecuteButton, 0);
			this.Controls.SetChildIndex(this.SaveDocBuilderTemplatesToFileButton, 0);
			this.Controls.SetChildIndex(this.UpdateTemplateCommandsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UpdateTemplateCommandsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid UpdateTemplateCommandsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton ExecuteButton;
		private Enterprise.ZArchitecture.GUI.ZButton SaveDocBuilderTemplatesToFileButton;
	}
}
#endif
