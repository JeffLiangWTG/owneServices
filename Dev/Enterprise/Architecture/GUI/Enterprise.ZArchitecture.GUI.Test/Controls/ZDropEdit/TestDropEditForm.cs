using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class TestDropEditForm : ZChildForm
	{
		internal TestDropEdit DropEdit;
		public TextBox zCalcEdit1;
		public TextBox zCalcEdit2;

		public TestDropEditForm(BusinessObject bizObj)
			: base(bizObj)
		{
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
			this.DropEdit = new TestDropEdit();
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
			// DropEdit
			// 
			this.DropEdit.BindTo = BindToPrefix + DummyBusinessObject.Schema.Z0_FK_Code;
			this.DropEdit.BindToList = BindToPrefix + "DummyList";
			this.DropEdit.Location = new System.Drawing.Point(24, 8);
			this.DropEdit.Name = "DropEdit";
			this.DropEdit.Size = new System.Drawing.Size(232, 20);
			this.DropEdit.TabIndex = 7;
			this.DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			// 
			// Form1
			// 

			this.ClientSize = new System.Drawing.Size(296, 129);
			this.Controls.Add(this.DropEdit);
			this.Controls.Add(this.zCalcEdit2);
			this.Controls.Add(this.zCalcEdit1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
		}
		#endregion
	}
}
