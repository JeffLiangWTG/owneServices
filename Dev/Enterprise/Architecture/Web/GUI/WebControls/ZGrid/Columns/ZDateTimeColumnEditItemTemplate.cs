
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZDateTimeColumnEditItemTemplate.
	/// </summary>
	public class ZDateTimeColumnEditItemTemplate : ZDateTimeColumnItemTemplate, IEditItemTemplate
	{
		public ZDateTimeColumnEditItemTemplate(ZDateTimeColumn column) : base(column)
		{
		}

		protected internal override ISelfBindingWebControl GetControl() => GetControlCore();

		protected virtual ISelfBindingWebControl GetControlCore()
		{
			var edit = new ZDateEdit();
			var column = (ZDateTimeColumn)Column;
			edit.DateTimeFormat = column.DateTimeFormat;
			edit.CanBeEnabledByClient = column.CanBeEnabledByClient;

			return edit;
		}

		protected override void InstantiateInCore(Control container)
		{
			base.InstantiateInCore(container);
			var cell = container as TableCell;
			if (cell != null)
			{
				cell.Wrap = false;
			}
		}
	}
}
