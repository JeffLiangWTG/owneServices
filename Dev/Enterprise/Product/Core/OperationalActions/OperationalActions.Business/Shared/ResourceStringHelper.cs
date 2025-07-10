using System.Reflection;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Services.OperationalActions.Business
{
	public static class ResourceStringHelper
	{
		public static ResourceStringData GetData(PropertyInfo info)
		{
			return DataBoundResourceStrings.GetDataForProperty(info);
		}
	}
}
