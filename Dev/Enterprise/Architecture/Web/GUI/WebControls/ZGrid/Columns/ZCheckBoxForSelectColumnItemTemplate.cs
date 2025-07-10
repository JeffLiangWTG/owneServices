using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZCheckBoxForSelectColumnItemTemplate : ZItemTemplate
	{
		public ZCheckBoxForSelectColumnItemTemplate(ZCheckBoxForSelectColumn column)
			: base(column)
		{
		}

		protected ZCheckBoxForSelectColumn SelectColumn
		{
			get { return (ZCheckBoxForSelectColumn)Column; }
		}

		protected override void InstantiateInCore(Control container)
		{
			var checkbox = new ZCheckBoxForSelect();
			checkbox.ID = SelectColumn.CheckBoxID;
			container.Controls.Add(checkbox);
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			return null;
		}
	}
}
