using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZSelectionColumn : ZTemplateColumn
	{
		public ZSelectionColumn(ZDataGrid grid)
			: base("", "")
		{
			this.grid = grid;
			this.HeaderTemplate = GetHeaderTemplate();
			this.ItemTemplate = GetItemTemplate();
		}

		public ZDataGrid Grid
		{
			get { return grid; }
		}
		readonly ZDataGrid grid;

		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell(cell, columnIndex, itemType);

			TableItemStyle style = null;
			switch (itemType)
			{
				case ListItemType.Item:
					style = Grid.ItemStyle;
					break;

				case ListItemType.AlternatingItem:
					style = Grid.AlternatingItemStyle;
					break;

				default:
					return;
			}

			ZSelectionCheckBox checkBox = cell.Controls[0] as ZSelectionCheckBox;
			if (checkBox != null)
			{
				#region SuppressResourceStringsCheckRegion

				checkBox.ID = "chkbx";
				checkBox.Attributes["onclick"] = String.Format(@"javascript:HighlightRow(this, '{0}', '{1}');", Grid.SelectedItemStyle.CssClass, style.CssClass);
				checkBox.CheckedChanged += new EventHandler(SelectionChanged);

				#endregion
			}
		}

		protected void SelectionChanged(object sender, EventArgs e)
		{
			ZSelectionCheckBox checkBox = sender as ZSelectionCheckBox;
			ZDataGridItem item = checkBox != null ? checkBox.Parent.Parent as ZDataGridItem : null;

			if (item != null && Grid != null)
			{
				Grid.SelectRow(Grid.DataKeys[item.ItemIndex], checkBox.Checked);

				TableItemStyle style = checkBox.Checked ? Grid.SelectedItemStyle :
					item.ItemType == ListItemType.AlternatingItem ? Grid.AlternatingItemStyle : Grid.ItemStyle;
				item.ApplyStyle(style);
			}
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZSelectionColumnItemTemplate(this);
		}

		protected internal virtual ITemplate GetHeaderTemplate()
		{
			return new ZSelectionColumnHeaderTemplate(this);
		}
	}
}
