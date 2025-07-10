using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.FR.DataTransfer.Universal
{
	public class FRNctsEventParentFinder : EU.NCTS.DataTransfer.NctsEventParentFinder
	{
		public FRNctsEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger) : base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetChildrenIfSpecifiedInContext(BusinessObject[] logParents, UniversalEvent eventData)
		{
			var nctsHeader = logParents?.Cast<NctsHeader>().FirstOrDefault();

			if (nctsHeader != null)
			{
				UpdateNctsHeaderDetails(nctsHeader, eventData);
			}

			return base.GetChildrenIfSpecifiedInContext(logParents, eventData);
		}

		protected void UpdateNctsHeaderDetails(NctsHeader nctsHeader, UniversalEvent topLevelDataObject)
		{
			var outgoingInterchangeNumber = ((IXmlEventValueObject)topLevelDataObject).Context.InterchangeNumber.GetValueOrDefault();
			var eventReference = topLevelDataObject.EventReference.GetValueOrDefault();
			var messageStatus = ParentFinderHelper.GetMessageStatus(eventReference);
			if (nctsHeader is NctsHeader frNctsHeader && messageStatus == EDIMessageStatusList.Codes.Rejected)
			{
				ParentFinderHelper.UpdateMatchingOutgoingMessageStatus(frNctsHeader, outgoingInterchangeNumber, messageStatus);

				frNctsHeader.EffectiveMessageStatus = messageStatus;
			}
		}
	}
}
