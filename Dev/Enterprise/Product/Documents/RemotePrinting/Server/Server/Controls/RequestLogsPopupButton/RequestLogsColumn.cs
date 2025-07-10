using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.RemotePrinting.Server.Controls
{
	public class RequestLogsColumn : ZTemplateColumn
	{
		public RequestLogsColumn(string headerText, string bindTo) : base(headerText, bindTo)
		{
		}

		protected override ITemplate GetItemTemplate()
		{
			return new RequestLogsColumnItemTemplate(this);
		}
	}
}
