using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Template for editing controls in the grid
	/// </summary>
	public class ZGroupColumnEditItemTemplate : ZGroupColumnItemTemplate, IEditItemTemplate
	{
		public ZGroupColumnEditItemTemplate(ZGroupColumn column)
			: base(column)
		{
		}

		protected override ITemplate GetItemTemplate(ZTemplateColumn column)
		{
			if (column.EditItemTemplate != null)
			{
				return column.EditItemTemplate;
			}
			return column.GetNewEditItemTemplate();
		}
	}
}
