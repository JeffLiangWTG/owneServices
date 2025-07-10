using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ISelfBindingWebControl.
	/// </summary>
	public interface ISelfBindingWebControl : IBindTo
	{
		bool IsBindable(object dataSource);
		void Bind(object dataSource);
		void UnBind();
	}
}
