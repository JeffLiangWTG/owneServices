using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public static class CloneExtensions
	{
		public static void CloneFrom<T>(this ICusLineTariffDetailCollection<T> clonedCollection, ICusLineTariffDetailCollection<T> sourceCollection) where T : CusLineTariffDetail
		{
			clonedCollection.RemoveAndDeleteAll();
			foreach (var businessObject in sourceCollection)
			{
				var clonedData = businessObject.Clone();

				using (clonedData.SuspendSettingHasChanges())
				using (clonedData.GetValidationSuspender())
				{
					clonedCollection.Add(clonedData);
				}
			}
		}

		public static void CloneFrom<T>(this BusinessObjectCollection<T> clonedCollection, BusinessObjectCollection<T> sourceCollection) where T : BusinessObject
		{
			clonedCollection.RemoveAndDeleteAll();
			foreach (var businessObject in sourceCollection)
			{
				var clonedData = businessObject.Clone();

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

		public static void CloneFrom<T>(this ActiveBusinessObjectCollection<T> clonedCollection, ActiveBusinessObjectCollection<T> sourceCollection) where T : BusinessObject
		{
			clonedCollection.DeleteAll();
			foreach (var businessObject in sourceCollection)
			{
				var clonedData = businessObject.Clone();

				using (clonedData.SuspendSettingHasChanges())
				using (clonedData.GetValidationSuspender())
				{
					clonedCollection.Add(clonedData as T);
				}
			}
		}
	}
}
