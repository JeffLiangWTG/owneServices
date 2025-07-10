using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;

namespace Enterprise.Messaging.Business
{
	class EDIMessageAfterOnSavingCorruptionCheck : IAfterSaveInTransactionService
	{
		internal static EDIMessageAfterOnSavingCorruptionCheck HookupFactory(BusinessObjectFactory factory)
		{
			var service = factory.ServiceContainer.GetAfterSaveInTransactionService<EDIMessageAfterOnSavingCorruptionCheck>();

			if (service == null)
			{
				service = new EDIMessageAfterOnSavingCorruptionCheck();
				factory.ServiceContainer.AddAfterSaveInTransactionService(service);
			}
			return service;
		}

		public void DoFinalCheckBeforeCommit(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
		{
			foreach (var message in businessObjectsInOnSavingOrder.OfType<EDIMessage>().Where(m => !m.IsDeleted))
			{
				message.ReportIfInvalidMessageData();
			}
		}
	}
}

