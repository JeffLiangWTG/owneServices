using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public static class CloneExtensions
{
	public static void CloneFrom<T>(this BusinessObjectCollection<T> clonedCollection, BusinessObjectCollection<T> sourceCollection, BusinessObjectCloneArgs args) where T : BusinessObject
	{
		clonedCollection.RemoveAndDeleteAll();
		foreach (var businessObject in sourceCollection)
		{
			var clonedObject = new CusLineTariffDetailDeepCloneStrategy(businessObject, CloneType.DeepTemplateCopy).Clone();

			using (clonedObject.SuspendSettingHasChanges())
			using (clonedObject.GetValidationSuspender())
			{
				clonedCollection.Add(clonedObject);
			}
		}
	}

	public static void CloneFrom(this BusinessObject clonedObject, BusinessObject sourceObject)
	{
		using (clonedObject.GetValidationSuspender())
		using (clonedObject.SuspendSettingHasChanges())
		{
			clonedObject.CopyPersistentValuesFrom(sourceObject);
		}
	}
}
