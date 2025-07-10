using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class VisualizerDocumentDataContextManager : ShipmentDataContextManager<VisualizerDocumentData>
	{
		public override ZString DataContextKey
		{
			get { return ParentDataContextManager.DataContextKey; }
		}

		public override DataContextType DataContextType
		{
			get { return ParentDataContextManager.DataContextType; }
		}

		IDataContextManager ParentDataContextManager
		{
			get
			{
				var contextManager = ParentBO != null && ParentBO.Parent != null
					? ParentBO.Parent.GetUniversalDataContextManager()
					: null;

				if (contextManager != null)
				{
					return contextManager;
				}

				throw new InvalidOperationException(
					"Document Data needs to be referenced to a parent job which supports UniversalXML.");
			}
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalDataBuss.DataObjects.Universal.Shipment universalShipment, IXmlImportLogger logger, UniversalDataBuss.DataObjects.Core.UniversalObjectFactory factory)
		{
			return null;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return null;
		}

		public override bool ManagesShipments
		{
			get { return false; }
		}

		public override bool ManagesEvents
		{
			get { return false; }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}

		public override string DefaultOutputDirectory
		{
			get { return string.Empty; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;// reader and writer not implemented - no need to do anything here
		}
	}
}
