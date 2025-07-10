using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZCheckBoxForSelectColumn : ZTemplateColumn
	{
		public ZCheckBoxForSelectColumn(string headerText, string uniqueKey, ZDataGrid grid, ISupportEDocsBulkDownload module)
			: base(headerText, "")
		{
			Argument.NotNullOrEmpty(uniqueKey, uniqueKey);
			Argument.NotNull(grid, "grid");
			Argument.NotNull(module, "module");
			this.grid = grid;
			this.module = module;
			this.Key = uniqueKey;
			this.CheckBoxID = checkBoxIDPrefix + uniqueKey.Replace(" ", "");
			this.HeaderTemplate = GetHeaderTemplate();
			this.ItemTemplate = GetItemTemplate();
			this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
		}

		const string checkBoxIDPrefix = "ZCheckBoxForSelectColumn-";
		internal readonly string CheckBoxID;
		internal readonly string Key;
		readonly ZDataGrid grid;
		readonly ISupportEDocsBulkDownload module;
		internal Action<object, EventArgs> OnCellPreRender
		{
			get;
			set;
		}

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZCheckBoxForSelectColumnItemTemplate(this);
		}

		protected internal virtual ITemplate GetHeaderTemplate()
		{
			return new ZCheckBoxForSelectColumnHeaderTemplate(this);
		}

		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell(cell, columnIndex, itemType);
			ZCheckBoxForSelect checkBox = null;
			if (cell.Controls.Count > 0)
			{
				checkBox = cell.Controls[0] as ZCheckBoxForSelect;
			}
			if (checkBox != null)
			{
				checkBox.CheckedChanged += new EventHandler(SelectionChanged);
				if (OnCellPreRender != null)
				{
					cell.PreRender += new EventHandler(OnCellPreRender);
				}
			}
		}

		protected void SelectionChanged(object sender, EventArgs e)
		{
			var checkBox = sender as ZCheckBoxForSelect;
			ZDataGridItem item = checkBox != null ? checkBox.Parent.Parent as ZDataGridItem : null;

			if (item != null)
			{
				var gridPk = grid.GetPKByRowIndex(item.ItemIndex);
				var bizoPKs = module.GetEDocsBulkDownloadRelevantAndRelatedPKs(grid, item.ItemIndex);
				var humanReadableName = module.GetPersistantBizoHumanReadableName(grid, gridPk);
				grid.SelectCellValue(gridPk, bizoPKs, humanReadableName, Key, checkBox.Checked);
			}
		}
	}
}
