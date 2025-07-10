using System.Diagnostics.CodeAnalysis;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public interface IZAjaxPage
	{
		AJAXManager AJAX { get; }
		bool IsAsyncPostBack { get; }
		string AsyncPostBackSourceElementID { get; }

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		void UpdatePanelRedirect(string key, string redirectUrl);
	}
}
