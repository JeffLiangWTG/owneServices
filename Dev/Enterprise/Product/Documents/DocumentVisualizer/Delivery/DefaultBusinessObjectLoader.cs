using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Delivery
{
	sealed class DefaultBusinessObjectLoader : IBusinessObjectLoader
	{
		public IBusiness LoadBusinessObject(BusinessObjectFactory factory, IBusiness bizObj)
		{
			if (factory == null
				|| bizObj == null)
			{
				return null;
			}

			if (factory._Instance == bizObj.Factory._Instance)
			{
				return bizObj;
			}

			if (!bizObj.IsInDatabase)
			{
				return factory.ImportFromAnotherFactory((BusinessObject)bizObj);
			}

			IBusiness LoadBizObj() => factory.Load(bizObj.GetType(), bizObj.Identifier);

			try
			{
				return LoadBizObj();
			}
			catch (SqlException)
			{
				return factory.ImportFromAnotherFactory((BusinessObject)bizObj);
			}
		}
	}
}
