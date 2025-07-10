using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using AsycudaManifestHeader = Enterprise.Customs.GB.GVMS.AsycudaManifestHeader;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class GBGVMSAsycudaManifestHeaderDataContextManager : ShipmentDataContextManager<AsycudaManifestHeader>
	{
		public override bool ManagesShipments => true;

		public override bool ManagesEvents => true;

		public override ZString DataContextKey => ParentBO.AMA_JobReference;

		public override string DefaultOutputDirectory => string.Empty;

		public override DataContextType DataContextType => DataContextType.GvmsAsycudaManifestHeader;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZQuery result = null;
			if (!matchingValues.Key.IsEmpty)
			{
				result = new ZQuery(AsycudaManifestHeaderSchema.AMA_JobReference, matchingValues.Key);
			}
			return result;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => new AsycudaManifestHeaderDataEventParentFinder(factory, this, logger);

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new GBGVMSAsycudaManifestHeaderDataObjectReader(universalShipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			var header = ParentBO ?? (writeManager.Action?.ParentBO as AsycudaManifestHeader);
			return header?.ApplicationBusinessProvider?.GetAsycudaManifestHeaderDataObjectWriter(writeManager);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;
	}
}

