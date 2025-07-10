using System;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public class DataImportIndicatorService : IService
	{
		public bool IsDataImportInProgress { get; private set; }

		public static DataImportIndicatorService GetInstance(BusinessObjectFactory factory)
		{
			var instance = factory.ServiceContainer.GetService<DataImportIndicatorService>();

			if (instance == null)
			{
				instance = new DataImportIndicatorService();
				factory.ServiceContainer.AddService(instance);
			}

			return instance;
		}

		public static IDisposable StartDataImport(BusinessObjectFactory factory)
		{
			var service = GetInstance(factory);
			if (service.IsDataImportInProgress)
			{
				throw new InvalidOperationException("Data Import already started.");
			}

			return new DisposableAction(() => { service.IsDataImportInProgress = true; }, () => { service.IsDataImportInProgress = false; });
		}
	}
}
