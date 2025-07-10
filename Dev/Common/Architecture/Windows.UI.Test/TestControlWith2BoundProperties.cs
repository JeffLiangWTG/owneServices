using System;
using System.ComponentModel;

namespace CargoWise.Windows.UI.Testing
{
	[SuppressFormDesignerAnalysis]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	sealed class TestControlWith2BoundProperties : KUserControl
	{
		internal KTextBox textBox1;
		internal KTextBox textBox2;
		IContainer components;

		public TestControlWith2BoundProperties()
		{
			InitializeComponent();

			SetDataSourceBinding(this, "Text1", "Property1");
			SetDataSourceBinding(this, "Text2", "Property2");

			this.BindingSource.DataSourceType = typeof(object);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.components = new Container();
			this.textBox1 = new KTextBox();
			this.textBox2 = new KTextBox();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ControlWith2BoundPropsTestCase.TestDataSource);
			// 
			// textBox1
			// 
			this.BindingSource.SetBindingMember(this.textBox1, "");
			this.textBox1.Location = new System.Drawing.Point(8, 8);
			this.textBox1.Name = "textBox1";
			this.textBox1.TabIndex = 0;
			this.textBox1.Text = "";
			// 
			// textBox2
			// 
			this.BindingSource.SetBindingMember(this.textBox2, "");
			this.textBox2.Location = new System.Drawing.Point(8, 32);
			this.textBox2.Name = "textBox2";
			this.textBox2.TabIndex = 1;
			this.textBox2.Text = "";
			// 
			// TestControlWith2BoundProps
			// 
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.textBox1);
			this.Name = "TestControlWith2BoundProps";
			this.Size = new System.Drawing.Size(240, 240);
			this.ResumeLayout(false);
		}
		#endregion

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			this.currentDataItem = (ControlWith2BoundPropsTestCase.TestDataSource)dataSource;
		}

		public new ControlWith2BoundPropsTestCase.TestDataSource CurrentDataItem
		{
			get { return this.currentDataItem; }
		}
		ControlWith2BoundPropsTestCase.TestDataSource currentDataItem;

		public string Text1
		{
			get { return textBox1.Text; }
			set { textBox1.Text = value; }
		}
		public event EventHandler Text1Changed
		{
			add { textBox1.TextChanged += value; }
			remove { textBox1.TextChanged -= value; }
		}

		public string Text2
		{
			get { return textBox2.Text; }
			set { textBox2.Text = value; }
		}
		public event EventHandler Text2Changed
		{
			add { textBox2.TextChanged += value; }
			remove { textBox2.TextChanged -= value; }
		}

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
