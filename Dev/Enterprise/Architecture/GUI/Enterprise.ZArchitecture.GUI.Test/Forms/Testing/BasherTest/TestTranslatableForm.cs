using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestTranslatableForm : ZForm
	{
		public TestTranslatableForm(object dataSource) : base(dataSource) { }

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return "Form Caption"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1093:DoNotUseSystemWindowsFormsToolStripControls", Justification = "Testing")]
		new void InitializeComponent()
		{
			Name = "TestForm";
			Size = new Size(300, 300);

			CaptionRenderingEnabled = true;
			BindingSource.DataSourceType = typeof(DummyBusinessObject);

			var groupBox1 = new ZGroupBox { Name = "groupBox1", Location = new Point(10, 10), Size = new Size(80, 40), Text = "Blaha" };
			Controls.Add(groupBox1);

			var groupBox2 = new ZGroupBox { Name = "groupBox2", Location = new Point(110, 10), Size = new Size(80, 40) };
			Controls.Add(groupBox2);

			var groupBox3 = new GroupBox { Name = "groupBox3", Location = new Point(10, 10), Size = new Size(80, 40), Text = "Blaha" };
			Controls.Add(groupBox3);

			var button1 = new ZButton { Name = "button1", Location = new Point(10, 10), Size = new Size(50, 20), Text = "Blaha" };
			groupBox1.Controls.Add(button1);

			var grid = new ZGrid { Name = "grid", Location = new Point(10, 100), Size = new Size(200, 100) };
			BindingSource.SetBindingMember(grid, "Collection");
			Controls.Add(grid);

			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Description" });
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "Z0_Int", Caption = "Blaha" });

			var contextMenu = new ContextMenu();
			button1.ContextMenu = contextMenu;

			var menuItem1 = new MenuItem { Name = "menuItem1", Text = "Menu One" };
			contextMenu.MenuItems.Add(menuItem1);

			Menu.MenuItems.Clear();
			var menuItem2 = new MenuItem { Name = "menuItem2", Text = Res.GetString("2", "Menu Two") };
			menuItem2.Click += new EventHandler(menuItem2_Click);
			Menu.MenuItems.Add(menuItem2);

			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripButton("Tool Strip Button") { Name = "ToolStripButton" });
			this.Controls.Add(toolStrip);

			var dropEdit = new ZDropEdit();
			dropEdit.CaptionResourceString = Res.GetData("z", "ZZZ");
			var list = new CodeDescriptionPairList();
			list.AddPair("zzz", "Zee Not Translatable");
			dropEdit.List = list;
			this.Controls.Add(dropEdit);
		}

		void menuItem2_Click(object sender, EventArgs e)
		{ }
	}
}
