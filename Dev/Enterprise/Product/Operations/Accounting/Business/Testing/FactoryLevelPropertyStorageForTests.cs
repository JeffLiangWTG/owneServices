#if DEBUG

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public class FactoryLevelPropertyStorageForTests : IService
	{
		FactoryLevelPropertyStorageForTests()
		{
			Values = new Dictionary<ZGuid, Dictionary<ZString, object>>();
		}

		readonly Dictionary<ZGuid, Dictionary<ZString, object>> Values;

		public void AddNewElement(ZGuid pk, string propertyName, object propertyValue)
		{
			if (Values.ContainsKey(pk))
			{
				var dictValues = Values[pk];
				dictValues[propertyName] = propertyValue;
			}
			else
			{
				Values[pk] = new Dictionary<ZString, object>() { { propertyName, propertyValue } };
			}
		}

		public object GetValueFromElement(ZGuid pk, string propertyName)
		{
			object result = null;

			Dictionary<ZString, object> propertyValue;
			Values.TryGetValue(pk, out propertyValue);

			propertyValue?.TryGetValue(propertyName, out result);

			return result;
		}

		public static FactoryLevelPropertyStorageForTests GetOrCreateNewInstance(BusinessObjectFactory factory)
		{
			var factoryLevelPropertyStorage = factory.ServiceContainer.GetService<FactoryLevelPropertyStorageForTests>();
			if (factoryLevelPropertyStorage == null)
			{
				factoryLevelPropertyStorage = new FactoryLevelPropertyStorageForTests();
				factory.ServiceContainer.AddService(factoryLevelPropertyStorage);
			}
			return factoryLevelPropertyStorage;
		}

		public static FactoryLevelPropertyStorageForTests GetInstance(BusinessObjectFactory factory)
		{
			return factory.ServiceContainer.GetService<FactoryLevelPropertyStorageForTests>();
		}
	}
}

#endif
