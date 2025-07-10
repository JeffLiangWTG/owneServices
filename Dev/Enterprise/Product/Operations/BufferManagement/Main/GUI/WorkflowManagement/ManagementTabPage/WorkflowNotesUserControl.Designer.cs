namespace Enterprise.BufferManagement.GUI
{
	partial class WorkflowNotesUserControl
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
			this.WorkflowNotesRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.ProcessHeader);
			// 
			// WorkflowNotesRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.WorkflowNotesRichTextBox, "WorkflowNote.ST_NoteData");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).WorkflowNote.ST_NoteData)));
			this.WorkflowNotesRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WorkflowNotesRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WorkflowNotesRichTextBox.MaxLength = 10000000;
			this.WorkflowNotesRichTextBox.Name = "WorkflowNotesRichTextBox";
			this.WorkflowNotesRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 270, true);
			this.WorkflowNotesRichTextBox.TabIndex = 0;
			// 
			// WorkflowNotesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.WorkflowNotesRichTextBox);
			this.Name = "WorkflowNotesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 270, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZRichTextBox WorkflowNotesRichTextBox;
	}
}
