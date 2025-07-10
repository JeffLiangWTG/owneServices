namespace CargoWise.Windows.UI.Testing
{
	sealed partial class BindingToParentRelationWhenParentChangesWhileControlDisposedForm
	{
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.dTextBox1 = new KTextBox();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(BindingToParentRelationWhenParentChangesWhileControlDisposedTestCase.TestEntity);
			// 
			// dTextBox1
			// 
			this.BindingSource.SetBindingMember(this.dTextBox1, "Parent.BoundValue");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(new ComponentModel.Design.CompileTimeCheckBindingMember(((BindingToParentRelationWhenParentChangesWhileControlDisposedTestCase.TestEntity)(null)).Parent.BoundValue));
			this.dTextBox1.Location = new System.Drawing.Point(56, 32);
			this.dTextBox1.Name = "dTextBox1";
			this.dTextBox1.TabIndex = 0;
			this.dTextBox1.Text = "dTextBox1";
			// 
			// DBindingToParentRelationWhenParentChangesWhileControlDisposed
			// 
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.dTextBox1);
			this.Name = "DBindingToParentRelationWhenParentChangesWhileControlDisposed";
			this.Text = "DBindingToParentRelationWhenParentChangesWhileControlDisposedTestCase";
			this.ResumeLayout(false);
		}

		#endregion

		KTextBox dTextBox1;
	}
}
