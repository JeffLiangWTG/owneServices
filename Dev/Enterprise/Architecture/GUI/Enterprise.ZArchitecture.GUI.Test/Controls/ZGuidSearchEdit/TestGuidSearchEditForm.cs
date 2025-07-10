using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class TestGuidSearchEditForm : ZChildForm
	{
		[ThreadStatic]
		public static bool SuppressBindings;

		internal TestZGuidSearchEditForTest GuidSearchEditForTest;
		public TextBox zCalcEdit1;
		public TextBox zCalcEdit2;

		public TestGuidSearchEditForm(DummyWithLookups bizObj)
			: base(bizObj)
		{
			var column = (SchemaStringColumn)ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(DummyDependentWithCodeBusinessObject.Schema.ZD1_Code, DummyDependentWithCodeBusinessObject.Schema.TableName);
			GuidSearchEditForTest.Searcher = new TestSearchBoxFilter(bizObj);
		}

		protected virtual string BindToPrefix
		{
			get { return ""; }
		}

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.zCalcEdit1 = new TextBox();
			this.zCalcEdit2 = new TextBox();
			this.GuidSearchEditForTest = new TestZGuidSearchEditForTest();
			this.SuspendLayout();
			// 
			// zCalcEdit1
			// 
			this.zCalcEdit1.Location = new System.Drawing.Point(16, 72);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.TabIndex = 5;
			this.zCalcEdit1.Text = "ZCALCEDIT1";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit2
			// 
			this.zCalcEdit2.Location = new System.Drawing.Point(176, 72);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.TabIndex = 6;
			this.zCalcEdit2.Text = "ZCALCEDIT1";
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GuidSearchEditForTest
			// 
			if (!SuppressBindings)
			{
				this.BindingSource.SetBindingMember(this.GuidSearchEditForTest, "Z0_Guid");
			}
			this.GuidSearchEditForTest.Location = new System.Drawing.Point(24, 8);
			this.GuidSearchEditForTest.Name = "GuidSearchEditForTest";
			this.GuidSearchEditForTest.Size = new System.Drawing.Size(232, 20);
			this.GuidSearchEditForTest.TabIndex = 7;
			// 
			// Form1
			// 

			this.ClientSize = new System.Drawing.Size(296, 129);
			this.Controls.Add(this.GuidSearchEditForTest);
			this.Controls.Add(this.zCalcEdit2);
			this.Controls.Add(this.zCalcEdit1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
		}
		#endregion
	}
}
