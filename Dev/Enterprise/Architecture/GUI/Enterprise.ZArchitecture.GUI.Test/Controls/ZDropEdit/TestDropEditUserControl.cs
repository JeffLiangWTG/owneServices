using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class TestDropEditUserControl : ZUserControl
	{
		public TextBox zCalcEdit1;
		public ZDropEdit DropEdit;

		public TestDropEditUserControl(BusinessObject bizObj)
		{
			InitializeComponent();
		}

		protected virtual string BindToPrefix
		{
			get { return ""; }
		}

		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			this.zCalcEdit1 = new TextBox();
			this.DropEdit = new ZDropEdit();
			this.SuspendLayout();
			// 
			// zCalcEdit1
			// 
			this.zCalcEdit1.Location = new System.Drawing.Point(16, 72);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.TabIndex = 1;
			this.zCalcEdit1.Text = "ZCALCEDIT1";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DropEdit
			// 
			this.DropEdit.BindTo = BindToPrefix + DummyBusinessObject.Schema.Z0_FK_Code;
			this.DropEdit.BindToList = BindToPrefix + "DummyList";
			this.DropEdit.Location = new System.Drawing.Point(24, 8);
			this.DropEdit.Name = "DropEdit";
			this.DropEdit.Size = new System.Drawing.Size(232, 20);
			this.DropEdit.TabIndex = 2;
			this.DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			// 
			// UserControl1
			// 
			this.ClientSize = new System.Drawing.Size(296, 129);
			this.Controls.Add(this.DropEdit);
			this.Controls.Add(this.zCalcEdit1);
			this.Name = "UserControl1";
			this.Text = "UserControl1";
			this.ResumeLayout(false);
		}
		#endregion
	}
}
