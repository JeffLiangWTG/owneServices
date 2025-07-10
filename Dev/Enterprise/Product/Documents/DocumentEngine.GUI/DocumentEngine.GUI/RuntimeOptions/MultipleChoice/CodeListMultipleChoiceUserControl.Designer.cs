namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class CodeListMultipleChoiceUserControl : RuntimeOptionUserControl
	{
		Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit DropEdit;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// DropEdit
			// 
			this.DropEdit.AllowDrop = true;
			this.DropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DropEdit, "ZValue");
			this.DropEdit.BindToList = "List";
			this.DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 4, true);
			this.DropEdit.Name = "DropEdit";
			this.DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.DropEdit.TabIndex = 10;
			// 
			// CodeListMultipleChoiceUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DropEdit);
			this.Name = "CodeListMultipleChoiceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 27, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DropEdit.ResumeLayout(true);
			this.DropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
