using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	internal interface IControlVisibility
	{
		bool IsVisible(BusinessObject bo);
		IEnumerable<ZPropertyInfo> GetDependencies(BusinessObject bo);
	}
}
