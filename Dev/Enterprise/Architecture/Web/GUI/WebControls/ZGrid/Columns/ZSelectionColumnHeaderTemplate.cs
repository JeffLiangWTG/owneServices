using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZSelectionColumnHeaderTemplate : ZSelectionColumnItemTemplate
	{
		public ZSelectionColumnHeaderTemplate(ZTemplateColumn column) : base(column)
		{
		}

		protected override void InstantiateInCore(Control container)
		{
			ZSelectionCheckBox checkBox = new ZSelectionCheckBox(true);
			container.Controls.Add(checkBox);
		}
	}
}
