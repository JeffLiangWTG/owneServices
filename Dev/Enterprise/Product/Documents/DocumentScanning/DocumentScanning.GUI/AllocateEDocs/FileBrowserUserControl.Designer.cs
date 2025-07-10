namespace Enterprise.DocumentScanning.GUI.AllocateEDocs
{
	partial class FileBrowserUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.directoryTreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.filesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.pathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.filesGrid)).BeginInit();
			this.filesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.Types.ZString);
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.directoryTreeView);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.filesGrid);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 429, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(203);
			this.kSplitContainer1.TabIndex = 0;
			// 
			// directoryTreeView
			// 
			this.directoryTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.directoryTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.directoryTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.directoryTreeView.Name = "directoryTreeView";
			this.directoryTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 429, true);
			this.directoryTreeView.TabIndex = 1;
			// 
			// filesGrid
			// 
			this.filesGrid.AllowNavigation = false;
			this.filesGrid.AllowReadOnlyRowsToBeDeleted = true;
			this.filesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("eb46346f-a71d-4033-af10-c1f9d1ea9e94", "Name");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "FileName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("03e715e6-9e16-48a7-bad2-65f8429d1ae4", "Ext.", "Extension", "");
			zTextBoxColumnStyleInfo2.ColumnName = "Extension";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.filesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.filesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.filesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filesGrid.GridId = "b76847df-c787-4977-ac82-ee5d343f0d3f";
			this.filesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.filesGrid.LayoutKey = "filesGrid";
			this.filesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.filesGrid.Name = "filesGrid";
			this.filesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 429, true);
			this.filesGrid.TabIndex = 2;
			// 
			// pathTextBox
			// 
			this.pathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.pathTextBox, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Types.ZString)(null)))));
			this.pathTextBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("e1cf41dd-c88b-40ce-b2b5-2afcb394e5b1", "Path");
			this.pathTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.pathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 3, true);
			this.pathTextBox.Name = "pathTextBox";
			this.pathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 20, true);
			this.pathTextBox.TabIndex = 0;
			// 
			// FileBrowserUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.pathTextBox);
			this.Controls.Add(this.kSplitContainer1);
			this.Name = "FileBrowserUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 458, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.filesGrid)).EndInit();
			this.filesGrid.ResumeLayout(false);
			this.filesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
		private ZArchitecture.GUI.ZTreeView directoryTreeView;
		private ZArchitecture.ZGrid filesGrid;
		private ZArchitecture.ZTextBox pathTextBox;
	}
}
