using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.Integration
{
	public static class IImportResultTestExtensions
	{
		public static BusinessObject GetBizOForTesting(this IImportResult jobEntityID, ITopLevelDataObject topLevelDataObject, BusinessObjectFactory factory)
		{
			BusinessObject result = null;

			if (jobEntityID != null)
			{
				var dataContextManager = jobEntityID.DataContextType.GetUniversalDataContextManager();
				if (dataContextManager != null)
				{
					var dataTarget = DataContextFactory.NewDataTarget();
					dataTarget.Type = jobEntityID.DataContextType.ToString();
					dataTarget.Key = jobEntityID.DataContextKey;
					result = dataContextManager.LoadBusinessObjectFromDataTarget(topLevelDataObject, dataTarget, factory, new DummyLogger());
				}
			}

			return result;
		}
	}
}
