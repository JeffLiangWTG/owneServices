using System.Reflection;

namespace CargoWise.Windows.UI.Design
{
	interface IChooseHowMyPropertiesScale
	{
		DpiState GetDpiState(MemberInfo method);
	}
}
