using System.Web.UI;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZAjaxClientScriptManager : ZClientScriptManager
	{
		public ZAjaxClientScriptManager(ZAjaxPage page)
			: base(page)
		{
		}

		protected virtual bool IsAsyncPostBack
		{
			get { return ((ZAjaxPage)Page).IsAsyncPostBack; }
		}

		public override bool IsClientScriptIncludeRegistered(ZString key)
		{
			if (IsAsyncPostBack)
			{
				return false;
			}
			else
			{
				return base.IsClientScriptIncludeRegistered(key);
			}
		}

		public override void RegisterClientScriptInclude(ZString key, ZString scriptUrl)
		{
			if (IsAsyncPostBack)
			{
				ScriptManager.RegisterClientScriptInclude(Page, typeof(Page), key, scriptUrl);
			}
			else
			{
				base.RegisterClientScriptInclude(key, scriptUrl);
			}
		}

		public override void RegisterHiddenField(ZString name, ZString value)
		{
			if (IsAsyncPostBack)
			{
				ScriptManager.RegisterHiddenField(Page, name, value);
			}
			else
			{
				base.RegisterHiddenField(name, value);
			}
		}
	}
}
