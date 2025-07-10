using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IControlVisibilityBehaviour
	{
		bool IsVisible(BusinessObject dataItem);
	}
}
