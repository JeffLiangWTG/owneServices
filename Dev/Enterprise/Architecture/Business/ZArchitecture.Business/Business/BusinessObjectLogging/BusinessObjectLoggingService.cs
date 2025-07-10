using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	class BusinessObjectLoggingService : IAfterOnSavingBOProcessingService
	{
		internal BusinessObjectLoggingService(Func<BusinessObject, IEnumerable<IBusinessObjectLogger>> getLoggers)
		{
			this.getLoggers = getLoggers;
		}

		public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			foreach (var businessObject in businessObjectsInOnSavingOrder.Where(b => !b.IsDeleted && b is IAutoAdminLogTarget))
			{
				var businessObjectLoggers = getLoggers(businessObject).Where(bl => bl.RunAfterOnSavingForAllBizos);

				foreach (var logger in businessObjectLoggers)
				{
					var log = logger.CreateSaveLog(businessObject);
					log?.OnSaving();
				}
			}
		}

		readonly Func<BusinessObject, IEnumerable<IBusinessObjectLogger>> getLoggers;
	}
}
