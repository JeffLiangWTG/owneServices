using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	internal class CollapsibleTableLayoutPanel : TableLayoutPanel
	{
		readonly TableLayoutPanelRowCollapser collapser;

		public CollapsibleTableLayoutPanel()
			: base()
		{
			collapser = new TableLayoutPanelRowCollapser(this);
		}

		public void Collapse()
		{
			collapser.Collapse();
		}

		/// <summary>
		/// This recursively searches children of this control to set DoubleBuffered, and probes
		/// up through parent controls to the top parent control. Will only have desired effect
		/// if the parent heirarchy goes all the way up to the form. So don't call on a freshly
		/// newed up control for example.
		/// </summary>
		public void StopFlicker()
		{
			Control ctl = this;

			do
			{
				ctl.SetDoubleBuffered(true);
				ctl = ctl.Parent;
			}
			while (ctl != null);

			Stack<Control> childControls = new Stack<Control>();
			childControls.Push(this);

			do
			{
				ctl = childControls.Pop();
				ctl.SetDoubleBuffered(true);
				foreach (Control childControl in ctl.Controls)
				{
					childControls.Push(childControl);
				}
			}
			while (childControls.Count > 0);
		}
	}

	internal class TableLayoutPanelRowCollapser
	{
		readonly Dictionary<int, RowStyle> originalRowStyles;
		readonly TableLayoutPanel panel;
		public TableLayoutPanelRowCollapser(TableLayoutPanel p)
		{
			panel = p;
			originalRowStyles = new Dictionary<int, RowStyle>();
		}

		public void Collapse()
		{
			var rowsToCollapse = new HashSet<int>(Enumerable.Range(0, panel.RowCount));

			foreach (Control childControl in panel.Controls)
			{
				bool ignoreControl = false;

				if (!childControl.Visible)
				{
					ignoreControl = true;
				}
				else
				{
					if (childControl is Panel)
					{
						var allChildrenHidden = childControl.Controls.Cast<Control>().All(c => !c.Visible);
						ignoreControl = ignoreControl || allChildrenHidden;
					}
				}

				if (!ignoreControl)
				{
					var position = panel.GetCellPosition(childControl);
					rowsToCollapse.Remove(position.Row);
				}
			}

			for (int row = 0; row < panel.RowCount; row++)
			{
				var rowStyle = panel.RowStyles[row];
				if (rowsToCollapse.Contains(row))
				{
					if (rowStyle.Height != 0 || rowStyle.SizeType != SizeType.Absolute)
					{
						originalRowStyles[row] = new RowStyle(rowStyle.SizeType, rowStyle.Height);
						ControlDpiScalingHelper.SetHeight(ref rowStyle, 0, true);
						rowStyle.SizeType = SizeType.Absolute;
					}
				}
				else
				{
					if (rowStyle.Height == 0 && rowStyle.SizeType == SizeType.Absolute && originalRowStyles.ContainsKey(row))
					{
						panel.RowStyles[row] = originalRowStyles[row];
						originalRowStyles.Remove(row);
					}
				}
			}
		}
	}
}
