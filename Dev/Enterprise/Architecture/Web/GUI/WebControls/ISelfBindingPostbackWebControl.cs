using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public interface ISelfBindingPostbackWebControl : ISelfBindingWebControl, IPostBackDataHandler
	{
		bool HasChanges { get; set; }
	}
}
