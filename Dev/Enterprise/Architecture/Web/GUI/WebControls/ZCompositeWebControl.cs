using System.Web.UI;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZCompositeWebControl : CompositeControl, ISelfBindingWebControl
	{
		#region Constructors

		public ZCompositeWebControl()
			: base()
		{
		}

		#endregion

		public bool IsBindable(object dataSource)
		{
			bool result = false;
			foreach (WebControl control in Controls)
			{
				if (control is ISelfBindingWebControl)
				{
					result = result || ((ISelfBindingWebControl)control).IsBindable(dataSource);
				}
			}
			return result;
		}

		public void Bind(object dataSource)
		{
			foreach (WebControl control in Controls)
			{
				if (control is ISelfBindingWebControl)
				{
					((ISelfBindingWebControl)control).Bind(dataSource);
				}
			}
		}

		public void UnBind()
		{
			foreach (WebControl control in Controls)
			{
				if (control is ISelfBindingWebControl)
				{
					((ISelfBindingWebControl)control).UnBind();
				}
			}
		}

		public string BindTo
		{
			get
			{
				return string.Empty;
			}
			set
			{
			}
		}

		protected override void RenderChildren(HtmlTextWriter writer)
		{
			foreach (Control childControl in Controls)
			{
				childControl.RenderControl(writer);
				writer.Write("&nbsp;");
			}
		}
	}
}
