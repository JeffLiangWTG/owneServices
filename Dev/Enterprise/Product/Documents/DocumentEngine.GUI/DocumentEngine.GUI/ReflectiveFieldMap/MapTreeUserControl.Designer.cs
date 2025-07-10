namespace Enterprise.DocumentEngine.GUI.ReflectiveFieldMap
{
	partial class MapTreeUserControl
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
			this.mapFilterTreeView = new Enterprise.ZArchitecture.GUI.ZFilteredTreeView();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// mapTreeView
			// 
			this.mapFilterTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mapFilterTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mapFilterTreeView.Name = "mapTreeView";
			this.mapFilterTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 365, true);
			this.mapFilterTreeView.TabIndex = 0;
			this.mapFilterTreeView.Text = "mapTreeView";
			// 
			// MapTreeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mapFilterTreeView);
			this.Name = "MapTreeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 365, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZFilteredTreeView mapFilterTreeView;
	}
}
