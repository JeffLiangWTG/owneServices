namespace Enterprise.Services.OperationalActions.GUI.Testing
{
	public partial class DummySettingsControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.excuseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.defaultExcuseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.lockCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.marks = new CornerMarks();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.Testing.DummyOperationalActionMethodSettings);
			//
			// excuseTextBox
			//
			this.BindingSource.SetBindingMember(this.excuseTextBox, "DefaultExcuse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.Testing.DummyOperationalActionMethodSettings)(null)).DefaultExcuse)));
			this.excuseTextBox.Location = new System.Drawing.Point(8, 24);
			this.excuseTextBox.Name = "excuseTextBox";
			this.excuseTextBox.Size = new System.Drawing.Size(144, 20);
			this.excuseTextBox.TabIndex = 1;
			//
			// defaultExcuseLabel
			//
			this.defaultExcuseLabel.Location = new System.Drawing.Point(8, 0);
			this.defaultExcuseLabel.Name = "defaultExcuseLabel";
			this.defaultExcuseLabel.Size = new System.Drawing.Size(88, 20);
			this.defaultExcuseLabel.TabIndex = 2;
			this.defaultExcuseLabel.Text = "Default Excuse";
			//
			// lockCheckBox
			//
			this.lockCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.lockCheckBox, "LockExcuse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Services.OperationalActions.Business.Testing.DummyOperationalActionMethodSettings)(null)).LockExcuse)));
			this.lockCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lockCheckBox.Location = new System.Drawing.Point(8, 48);
			this.lockCheckBox.Name = "lockCheckBox";
			this.lockCheckBox.Size = new System.Drawing.Size(50, 17);
			this.lockCheckBox.TabIndex = 3;
			this.lockCheckBox.Text = "Lock";
			this.lockCheckBox.UseVisualStyleBackColor = true;
			//
			// marks
			//
			this.marks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.marks.Location = new System.Drawing.Point(0, 0);
			this.marks.Name = "marks";
			this.marks.Size = new System.Drawing.Size(176, 71);
			this.marks.TabIndex = 4;
			//
			// DummySettingsControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.lockCheckBox);
			this.Controls.Add(this.defaultExcuseLabel);
			this.Controls.Add(this.excuseTextBox);
			this.Controls.Add(this.marks);
			this.Name = "DummySettingsControl";
			this.Size = new System.Drawing.Size(176, 71);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.ZLabel defaultExcuseLabel;
		Enterprise.ZArchitecture.GUI.ZCheckBox lockCheckBox;
		Enterprise.ZArchitecture.ZTextBox excuseTextBox;
		CornerMarks marks;

		#endregion
	}
}
