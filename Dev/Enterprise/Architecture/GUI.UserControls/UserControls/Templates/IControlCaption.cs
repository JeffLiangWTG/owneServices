using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.GUI
{
	internal interface IControlCaption
	{
		bool TryGetCaption(BusinessObject bo, out IDictionary<string, ResourceStringData> controlData);

		IEnumerable<ZPropertyInfo> GetDependencies(BusinessObject bo);
	}
}
