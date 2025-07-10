using Enterprise.ZArchitecture.Web.GUI.FilterStrips;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public interface ISearchControl
	{
		WebModuleID ModuleID { get; }
		ZFilterGridModule Module { get; }
		ZFilterStripControl FilterStripControl { get; }
	}
}
