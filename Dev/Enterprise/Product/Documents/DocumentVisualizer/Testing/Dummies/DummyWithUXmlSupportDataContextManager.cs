using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyWithUXmlSupportDataContextManager : ShipmentDataContextManager<DummyWithUXmlSupport>
	{
		public override ZString DataContextKey
		{
			get { return ParentBO.Z0_Code; }
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.DummyBusinessObject; }
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalDataBuss.DataObjects.Universal.Shipment universalShipment, IXmlImportLogger logger, UniversalDataBuss.DataObjects.Core.UniversalObjectFactory factory)
		{
			return null;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new DummyWithUXmlSupportDataWriter();
		}

		public override bool ManagesShipments
		{
			get { return true; }
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
			return null;  // Dummy
		}
	}
}
