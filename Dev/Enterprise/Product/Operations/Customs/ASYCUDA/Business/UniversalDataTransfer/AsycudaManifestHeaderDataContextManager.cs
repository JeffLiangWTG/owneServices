using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestHeaderDataContextManager : ShipmentDataContextManager<AsycudaManifestHeader>, IDataContextManagerFromEDIMessage
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.AsycudaManifest; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.AMA_JobReference; }
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new AsycudaManifestHeaderDataObjectReader(universalShipment, logger, factory);
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		public override bool ManagesEvents
		{
			get { return true; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			var header = ParentBO ?? (writeManager.Action?.ParentBO as AsycudaManifestHeader);
			return header?.ApplicationBusinessProvider?.GetAsycudaManifestHeaderDataObjectWriter(writeManager);
		}

		public override string DefaultOutputDirectory
		{
			get { return ""; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return null;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new AsycudaManifestHeaderDataEventParentFinder(factory, this, logger);
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZQuery result = null;
			if (!matchingValues.Key.IsEmpty)
			{
				result = new ZQuery(AsycudaManifestHeaderSchema.AMA_JobReference, matchingValues.Key);
			}
			return result;
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var manifestHeader = businessObject as AsycudaManifestHeader;
			var provider = manifestHeader?.ApplicationBusinessProvider;
			var messagingProvider = manifestHeader?.MessagingProvider;
			if (provider != null && messagingProvider != null)
			{
				var messageType = messagingProvider.GetAsycudaEDIMessageType();
				var ediMessage = message.Factory.Load(messageType, message.PK) as AsycudaEDIMessage;
				var universalEvent = eventDataObject as UniversalEvent;
				_ = ediMessage?.EM_MessageInterpretation; // cause the system to create the note so that has changes doesn't happen when clicking on the form
				var processor = provider.GetNewAsycudaUniversalEventMessageProcessor(logger, universalEvent, ediMessage, manifestHeader);
				processor?.Process();
			}
		}
	}
}
