namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	public partial class DummyApplicatorControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.marks = new CornerMarks();
			this.excuseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.excuseLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.Testing.DummyOperationalActionMethodApplicator);
			//
			// marks
			//
			this.marks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.marks.Location = new System.Drawing.Point(0, 0);
			this.marks.Name = "marks";
			this.marks.Size = new System.Drawing.Size(287, 89);
			this.marks.TabIndex = 1;
			//
			// excuseTextBox
			//
			this.BindingSource.SetBindingMember(this.excuseTextBox, "Excuse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.Testing.DummyOperationalActionMethodApplicator)(null)).Excuse)));
			this.excuseTextBox.Location = new System.Drawing.Point(64, 8);
			this.excuseTextBox.Name = "excuseTextBox";
			this.excuseTextBox.Size = new System.Drawing.Size(208, 20);
			this.excuseTextBox.TabIndex = 0;
			//
			// excuseLabel
			//
			this.excuseLabel.Location = new System.Drawing.Point(8, 8);
			this.excuseLabel.Name = "excuseLabel";
			this.excuseLabel.Size = new System.Drawing.Size(56, 20);
			this.excuseLabel.TabIndex = 0;
			this.excuseLabel.Text = "Excuse";
			//
			// DummyApplicatorControl
			//
			this.Controls.Add(this.excuseLabel);
			this.Controls.Add(this.excuseTextBox);
			this.Controls.Add(this.marks);
			this.Name = "DummyApplicatorControl";
			this.Size = new System.Drawing.Size(287, 89);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.ZTextBox excuseTextBox;
		Enterprise.ZArchitecture.ZLabel excuseLabel;
		CornerMarks marks;
	}
}
