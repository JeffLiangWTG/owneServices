using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class REXNotificationDataContextManager : EventDataContextManager<QuarantineNexDocNotification>, IDataContextManagerFromEDIMessage
	{
		public REXNotificationDataContextManager() : base() { }

		public override bool ManagesEvents => true;

		public override DataContextType DataContextType => DataContextType.REXNotification;

		public override ZString DataContextKey => ParentBO.QN_RexNumber;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new REXNotificationEventParentFinder(factory, this, logger);
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			if (businessObject != null)
			{
				new NEXDOCEventProcessor().ProcessMessage(logger, eventDataObject, message, businessObject);
			}
		}

		public override string DefaultOutputDirectory => ZString.Empty;

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;
	}
}
