using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZGroupColumn : ZTemplateColumn
	{
		public ZGroupColumn(string headerText, params DataGridColumn[] columns)
			: base(headerText, string.Empty)
		{
			groupMembers = new DataGridColumn[columns.Length];
			columns.CopyTo(groupMembers, 0);
		}

		public DataGridColumn[] GroupMembers
		{
			get { return groupMembers; }
		}
		readonly DataGridColumn[] groupMembers;

		protected internal override ITemplate GetItemTemplate()
		{
			return new ZGroupColumnItemTemplate(this);
		}

		protected internal override ITemplate GetEditItemTemplate()
		{
			return new ZGroupColumnEditItemTemplate(this);
		}
	}
}
