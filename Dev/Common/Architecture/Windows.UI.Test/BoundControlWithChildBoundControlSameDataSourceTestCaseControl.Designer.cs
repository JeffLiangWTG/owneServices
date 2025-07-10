namespace CargoWise.Windows.UI.Testing
{
	sealed partial class BoundControlWithChildBoundControlSameDataSourceTestCaseControl
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
			this.txtText1 = new KTextBox();
			this.txtText2 = new KTextBox();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(BoundControlWithChildBoundControlSameDataSourceTestCase.TestDataSource);
			// 
			// txtText1
			// 
			this.BindingSource.SetBindingMember(this.txtText1, "Property1");
			this.txtText1.Location = new System.Drawing.Point(16, 16);
			this.txtText1.Name = "txtText1";
			this.txtText1.TabIndex = 0;
			this.txtText1.Text = "";
			// 
			// txtText2
			// 
			this.BindingSource.SetBindingMember(this.txtText2, "Property2");
			this.txtText2.Location = new System.Drawing.Point(16, 48);
			this.txtText2.Name = "txtText2";
			this.txtText2.TabIndex = 1;
			this.txtText2.Text = "";
			// 
			// DBoundControlWithChildBoundControlSameDataSourceTestCaseControl
			// 
			this.BindingSource.SetBindingMember(this, ".");
			this.Controls.Add(this.txtText2);
			this.Controls.Add(this.txtText1);
			this.Name = "DBoundControlWithChildBoundControlSameDataSourceTestCaseControl";
			this.ResumeLayout(false);
		}

		#endregion

		public KTextBox txtText1;
		public KTextBox txtText2;
	}
}
