using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.RemotePrinting.Server.Controls
{
	public class RequestLogsColumnItemTemplate : ZItemTemplate
	{
		public RequestLogsColumnItemTemplate(RequestLogsColumn column) : base(column) { }

		protected override ISelfBindingWebControl GetControl()
		{
			var control = new RequestLogsPopupButton();
			return control;
		}
	}
}
