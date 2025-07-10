using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class JobDeclarationEventParentFinder : Customs.DataTransfer.Universal.JobDeclarationEventParentFinder
	{
		public JobDeclarationEventParentFinder(BusinessObjectFactory factory, JobDeclarationDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContextCore(Event eventDataObject)
		{
			return CSWResponseMessageEventProcessor.IsCSWResponseMessage(eventDataObject)
				? CSWResponseMessageEventProcessor.GetLogParentsForEventUsingContext(eventDataObject, factory)
				: base.GetLogParentsForEventUsingContextCore(eventDataObject);
		}
	}
}
