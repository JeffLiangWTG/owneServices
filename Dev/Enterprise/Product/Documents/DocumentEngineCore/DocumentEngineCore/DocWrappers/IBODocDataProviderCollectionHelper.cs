using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public interface IBODocDataProviderCollectionHelper : IBODocDataProviderCollection
	{
		BusinessObject[] GetFilteredBusinessObjects(ZString filter);
		PropertyInfo GetPropertyInfo(ZString fieldName);
	}
}
