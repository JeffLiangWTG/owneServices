using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	class ZDummyButton : ZButton, IPostBackEventHandler
	{
		#region IPostBackEventHandler Members

		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			RaisedPostBackEvent = true;
		}
		public bool RaisedPostBackEvent;
		#endregion
	}
}
