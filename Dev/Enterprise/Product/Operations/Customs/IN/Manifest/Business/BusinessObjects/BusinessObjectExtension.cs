using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Manifest.Business;

static class BusinessObjectExtension
{
	public static bool AnyPropertyHasChanges(this BusinessObject bizObj, params string[] propertyNames)
	{
		if (bizObj == null)
		{
			return false;
		}
		if (bizObj.IsInDatabase)
		{
			return bizObj.IsDeleted || propertyNames.Any(x => bizObj.FindPropertyInfo(x).HasChanges);
		}
		else
		{
			return propertyNames.Any(x => !bizObj.FindPropertyInfo(x).Value.IsEmpty);
		}
	}
}
