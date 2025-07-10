using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.Business.UniversalDataTransfer
{
	public class DIFDocumentDataContextManager : EventDataContextManager<DIFDocument>, IDataContextManagerFromEDIMessage
	{
		public override bool ManagesEvents => true;

		public override DataContextType DataContextType => DataContextType.CADIFDocument;

		public override ZString DataContextKey => ParentBO.URN;

		public override string DefaultOutputDirectory => string.Empty;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return null;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new DIFDocumentEventParentFinder(factory, this, logger);
		}

		void IDataContextManagerFromEDIMessage.OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var parent = businessObject as JobRequiredDocumentAddInfo;
			if (parent == null)
			{
				logger.Log(Integration.LogType.Error, Res.GetString("6BED28FC-1BF4-4441-82F5-944A2149FDE1", "This is not a DIF Document Message. Parent BO type is {1}", businessObject.GetType()));
			}
			else
			{
				var processor = new DIFDocumentEventMessageProcessor(logger, eventDataObject as UniversalEvent, message as Messaging.Business.EDIMessage, parent);
				processor.Process();
			}
		}
	}
}
