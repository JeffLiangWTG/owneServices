using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Item template for ZCalcEditColumn
	/// </summary>
	public class ZGroupColumnItemTemplate : ZItemTemplate
	{
		public ZGroupColumnItemTemplate(ZGroupColumn column)
			: base(column)
		{
		}

		new ZGroupColumn Column
		{
			get { return base.Column as ZGroupColumn; }
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			ZCompositeWebControl control = new ZCompositeWebControl();
			foreach (DataGridColumn groupMember in Column.GroupMembers)
			{
				if (groupMember is ZTemplateColumn)
				{
					GetItemTemplate((ZTemplateColumn)groupMember).InstantiateIn(control);
				}
			}
			return control;
		}

		protected virtual ITemplate GetItemTemplate(ZTemplateColumn column)
		{
			if (column.ItemTemplate != null)
			{
				return column.ItemTemplate;
			}
			return column.GetNewItemTemplate();
		}
	}
}
