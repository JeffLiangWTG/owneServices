using CargoWise.EntityFramework;

namespace Enterprise.Customs.KR.Business
{
	public static class CloneExtensions
	{
		public static void CloneFrom<T>(this BusinessObjectCollection<T> clonedCollection, BusinessObjectCollection<T> sourceCollection, BusinessObjectCloneArgs args) where T : BusinessObject
		{
			clonedCollection.RemoveAndDeleteAll();
			foreach (var businessObject in sourceCollection)
			{
				var clonedData = businessObject.Clone(args);

				using (clonedData.SuspendSettingHasChanges())
				using (clonedData.GetValidationSuspender())
				{
					clonedCollection.Add(clonedData);
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
}
