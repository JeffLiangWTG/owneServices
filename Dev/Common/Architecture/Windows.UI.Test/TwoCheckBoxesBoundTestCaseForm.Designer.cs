namespace CargoWise.Windows.UI.Testing
{
	sealed partial class TwoCheckBoxesBoundTestCaseForm
	{
		private System.ComponentModel.IContainer components = null;

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
			this.cbBoolean1 = new KCheckBox();
			this.cbBoolean2 = new KCheckBox();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(TwoCheckBoxesBoundTestCase.TestRootEntity);
			// 
			// cbBoolean1
			// 
			this.BindingSource.SetBindingMember(this.cbBoolean1, "Boolean");
			this.cbBoolean1.Location = new System.Drawing.Point(24, 24);
			this.cbBoolean1.Name = "cbBoolean1";
			this.cbBoolean1.Size = new System.Drawing.Size(80, 24);
			this.cbBoolean1.TabIndex = 1;
			this.cbBoolean1.Text = "Boolean";
			// 
			// cbBoolean2
			// 
			this.BindingSource.SetBindingMember(this.cbBoolean2, "Boolean");
			this.cbBoolean2.Location = new System.Drawing.Point(24, 56);
			this.cbBoolean2.Name = "cbBoolean2";
			this.cbBoolean2.Size = new System.Drawing.Size(80, 24);
			this.cbBoolean2.TabIndex = 2;
			this.cbBoolean2.Text = "Boolean";
			// 
			// D2CheckBoxesBoundTestCaseForm
			// 
			this.ClientSize = new System.Drawing.Size(232, 118);
			this.Controls.Add(this.cbBoolean2);
			this.Controls.Add(this.cbBoolean1);
			this.Name = "D2CheckBoxesBoundTestCaseForm";
			this.Text = "D2CheckBoxesBoundTestCaseForm";
			this.ResumeLayout(false);
		}

		#endregion

		internal KCheckBox cbBoolean1;
		internal KCheckBox cbBoolean2;
	}
}
