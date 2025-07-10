using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZLinkButton : LinkButton, ISelfBindingWebControl
	{
		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		public void Bind(object dataSource)
		{
			Text = (ZString)ZPropertyAccessor.Get(dataSource, BindTo);
		}

		public void UnBind()
		{
			Text = "";
		}

		#endregion

		#region IBindTo Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo = "";

		#endregion
	}
}
