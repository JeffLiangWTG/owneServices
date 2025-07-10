namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	internal partial class MultipleChoiceUserControl : RuntimeOptionUserControl
	{
		Enterprise.ZArchitecture.GUI.ZDropEdit DropEdit;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
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
			this.DropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 4, true);
			this.DropEdit.Name = "DropEdit";
			this.DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DropEdit.TabIndex = 10;
			// 
			// MultipleChoiceUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DropEdit);
			this.Name = "MultipleChoiceUserControl";
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
