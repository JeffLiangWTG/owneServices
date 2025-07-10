using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.DataTransfer.Universal
{
	public class eManifestStatusNoticeDataContextManager : EventDataContextManager<CusEntryHeader>, IDataContextManagerFromEDIMessage
	{
		public override ZString DataContextKey
		{
			get { return ParentBO.CH_BGMReference; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.CAeManifestStatusNotice; }
		}

		public override string DefaultOutputDirectory
		{
			get { return ZString.Empty; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		public override bool ManagesEvents
		{
			get { return true; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new eManifestStatusNoticeEventParentFinder(factory, this, logger);
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var universalEventMessage = message.Factory.Load<UniversalEventMessage>(message.PK);
			var processor = new CustomsManifestStatusMessageProcessor(logger, eventDataObject as UniversalEvent, universalEventMessage, businessObject);
			if (processor != null)
			{
				processor.Process();
			}
		}
	}
}
