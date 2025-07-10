using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZCheckBoxForSelectColumnHeaderTemplate : ZCheckBoxForSelectColumnItemTemplate
	{
		public ZCheckBoxForSelectColumnHeaderTemplate(ZCheckBoxForSelectColumn column)
			: base(column)
		{
		}

		protected override void InstantiateInCore(Control container)
		{
			var headerLabel = new ZTextLabel(Column.HeaderText + "<br />");
			headerLabel.EnableHtmlEncoding = false;
			var checkBox = new ZCheckBoxForSelect(true);
			checkBox.ID = SelectColumn.CheckBoxID;
			container.Controls.Add(headerLabel);
			container.Controls.Add(checkBox);
		}
	}
}
