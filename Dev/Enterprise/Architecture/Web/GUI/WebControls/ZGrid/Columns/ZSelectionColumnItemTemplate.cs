using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZSelectionColumnItemTemplate : ZItemTemplate
	{
		public ZSelectionColumnItemTemplate(ZTemplateColumn column) : base(column)
		{
		}

		protected override void InstantiateInCore(Control container)
		{
			container.Controls.Add(new ZSelectionCheckBox());
		}

		protected internal override ISelfBindingWebControl GetControl()
		{
			return null;
		}
	}
}
