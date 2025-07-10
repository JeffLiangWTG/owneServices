using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	public interface ITabPage
	{
		ResourceStringData Caption { get; }

		ZUserControl CreateUserControl();

		ZString UserControlBindingMember { get; }
	}
}
