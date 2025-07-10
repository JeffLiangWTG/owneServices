using System.Reflection;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	public static class TableLayoutPanelRowCollapserTestHelper
	{
		public static bool GetDoubleBuffered(this Control control)
		{
			var property = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			return (bool)property.GetValue(control, null);
		}
	}

	public class TableLayoutPanelRowCollapserTest : TransactionedTestCase
	{
		public Label NewInvisibleLabel()
		{
			var label = new Label();
			label.Visible = false;
			return label;
		}

		public void TestStopFlicker()
		{
			using (var form = new Form())
			{
				var container = new Panel();
				form.Controls.Add(container);
				var panel = new CollapsibleTableLayoutPanel();
				panel.ColumnCount = 1;
				panel.RowCount = 1;
				panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
				panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
				container.Controls.Add(panel);
				var control_Column0_Row0 = new Panel();
				panel.Controls.Add(control_Column0_Row0, 0, 0);
				var label = new Label();
				control_Column0_Row0.Controls.Add(label);
				label.SetDoubleBuffered(false);
				AssertEquals(false, form.GetDoubleBuffered());
				AssertEquals(false, container.GetDoubleBuffered());
				AssertEquals(false, panel.GetDoubleBuffered());
				AssertEquals(false, control_Column0_Row0.GetDoubleBuffered());
				AssertEquals(false, label.GetDoubleBuffered());
				panel.StopFlicker();
				AssertEquals(true, form.GetDoubleBuffered());
				AssertEquals(true, container.GetDoubleBuffered());
				AssertEquals(true, panel.GetDoubleBuffered());
				AssertEquals(true, control_Column0_Row0.GetDoubleBuffered());
				AssertEquals(true, label.GetDoubleBuffered());
			}
		}

		public void TestCollapseSingleColumn()
		{
			using (var panel = new TableLayoutPanel())
			{
				var collapser = new TableLayoutPanelRowCollapser(panel);
				panel.ColumnCount = 1;
				panel.RowCount = 7;
				panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 10));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 20));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 30));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 40));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 50));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 60));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 70));

				var control_Column0_Row0 = new Label();
				panel.Controls.Add(control_Column0_Row0, 0, 0);
				var control_Column0_Row1 = new Panel();
				panel.Controls.Add(control_Column0_Row1, 0, 1);
				var control_Column0_Row2 = new ZPanel();
				control_Column0_Row2.Controls.Add(new Label());
				panel.Controls.Add(control_Column0_Row2, 0, 2);
				var control_Column0_Row3 = new ZPanel();
				panel.Controls.Add(control_Column0_Row3, 0, 3);
				// Row 4 has no controls
				var control_Column0_Row5 = new Label();
				control_Column0_Row5.Visible = false;
				panel.Controls.Add(control_Column0_Row5, 0, 5);
				var control_Column0_Row6 = new ZPanel();
				control_Column0_Row6.Controls.Add(NewInvisibleLabel());
				control_Column0_Row6.Controls.Add(NewInvisibleLabel());
				panel.Controls.Add(control_Column0_Row6, 0, 6);

				collapser.Collapse();

				AssertEquals("Row contains visible label, so don't collapse", 10f, panel.RowStyles[0].Height);
				AssertEquals("Row contains visible label, so don't collapse", SizeType.AutoSize, panel.RowStyles[0].SizeType);
				AssertEquals("Row contains panel with no children, so collapse", 0f, panel.RowStyles[1].Height);
				AssertEquals("Row contains panel with no children, so collapse", SizeType.Absolute, panel.RowStyles[1].SizeType);
				AssertEquals("Row contains zpanel with visible children, so don't collapse", 30f, panel.RowStyles[2].Height);
				AssertEquals("Row contains zpanel with visible children, so don't collapse", SizeType.AutoSize, panel.RowStyles[2].SizeType);
				AssertEquals("Row contains zpanel with no children so collapse", 0f, panel.RowStyles[3].Height);
				AssertEquals("Row contains zpanel with no children so collapse", SizeType.Absolute, panel.RowStyles[3].SizeType);
				AssertEquals("Row contains no controls so collapse", 0f, panel.RowStyles[4].Height);
				AssertEquals("Row contains no controls so collapse", SizeType.Absolute, panel.RowStyles[4].SizeType);
				AssertEquals("Row contains invisible label so collapse", 0f, panel.RowStyles[5].Height);
				AssertEquals("Row contains invisible label so collapse", SizeType.Absolute, panel.RowStyles[5].SizeType);
				AssertEquals("Row contains zpanel with only invisible children, so collapse", 0f, panel.RowStyles[6].Height);
				AssertEquals("Row contains zpanel with only invisible children, so collapse", SizeType.Absolute, panel.RowStyles[6].SizeType);

				control_Column0_Row6.Controls[0].Visible = true;
				collapser.Collapse();

				AssertEquals("As Before", 10f, panel.RowStyles[0].Height);
				AssertEquals("As Before", SizeType.AutoSize, panel.RowStyles[0].SizeType);
				AssertEquals("As Before", 0f, panel.RowStyles[1].Height);
				AssertEquals("As Before", SizeType.Absolute, panel.RowStyles[1].SizeType);
				AssertEquals("As Before", 30f, panel.RowStyles[2].Height);
				AssertEquals("As Before", SizeType.AutoSize, panel.RowStyles[2].SizeType);
				AssertEquals("As Before", 0f, panel.RowStyles[3].Height);
				AssertEquals("As Before", SizeType.Absolute, panel.RowStyles[3].SizeType);
				AssertEquals("As Before", 0f, panel.RowStyles[4].Height);
				AssertEquals("As Before", SizeType.Absolute, panel.RowStyles[4].SizeType);
				AssertEquals("As Before", 0f, panel.RowStyles[5].Height);
				AssertEquals("As Before", SizeType.Absolute, panel.RowStyles[5].SizeType);
				AssertEquals("Cached dimensions used now that this row should be visible", 70f, panel.RowStyles[6].Height);
				AssertEquals("Cached dimensions used now that this row should be visible", SizeType.AutoSize, panel.RowStyles[6].SizeType);

				control_Column0_Row5.Visible = true;
				panel.RowStyles[5].Height = 77;
				panel.RowStyles[5].SizeType = SizeType.Percent;
				collapser.Collapse();

				AssertEquals("As Before", 10f, panel.RowStyles[0].Height);
				AssertEquals("As Before", SizeType.AutoSize, panel.RowStyles[0].SizeType);
				AssertEquals("As Before", 0f, panel.RowStyles[1].Height);
				AssertEquals("As Before", SizeType.Absolute, panel.RowStyles[1].SizeType);
				AssertEquals("As Before", 30f, panel.RowStyles[2].Height);
				AssertEquals("As Before", SizeType.AutoSize, panel.RowStyles[2].SizeType);
				AssertEquals("As Before", 0f, panel.RowStyles[3].Height);
				AssertEquals("As Before", SizeType.Absolute, panel.RowStyles[3].SizeType);
				AssertEquals("As Before", 0f, panel.RowStyles[4].Height);
				AssertEquals("As Before", SizeType.Absolute, panel.RowStyles[4].SizeType);
				AssertEquals("Because this was done manually outside of the collapser, the new dimensions are honoured over the cached ones.", 77f, panel.RowStyles[5].Height);
				AssertEquals("Because this was done manually outside of the collapser, the new dimensions are honoured over the cached ones.", SizeType.Percent, panel.RowStyles[5].SizeType);
				AssertEquals("As Before", 70f, panel.RowStyles[6].Height);
				AssertEquals("As Before", SizeType.AutoSize, panel.RowStyles[6].SizeType);
			}
		}

		public void TestCollapseTwoColumn()
		{
			using (var panel = new TableLayoutPanel())
			{
				panel.ColumnCount = 2;
				panel.RowCount = 4;
				panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
				panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 30));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 30));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 30));
				panel.RowStyles.Add(new RowStyle(SizeType.AutoSize, 30));

				var control_Column0_Row0 = new Label();
				panel.Controls.Add(control_Column0_Row0, 0, 0);
				var control_Column1_Row1 = new Label();
				panel.Controls.Add(control_Column1_Row1, 1, 1);
				var control_Column0_Row2 = new ZPanel();
				var control_Column1_Row2 = new ZPanel();
				panel.Controls.Add(control_Column0_Row2, 0, 2);
				panel.Controls.Add(control_Column1_Row2, 1, 2);

				new TableLayoutPanelRowCollapser(panel).Collapse();

				AssertEquals(30f, panel.RowStyles[0].Height);
				AssertEquals(SizeType.AutoSize, panel.RowStyles[0].SizeType);
				AssertEquals(30f, panel.RowStyles[1].Height);
				AssertEquals(SizeType.AutoSize, panel.RowStyles[1].SizeType);
				AssertEquals(0f, panel.RowStyles[2].Height);
				AssertEquals(SizeType.Absolute, panel.RowStyles[2].SizeType);
				AssertEquals(0f, panel.RowStyles[3].Height);
				AssertEquals(SizeType.Absolute, panel.RowStyles[3].SizeType);
			}
		}
	}
}
