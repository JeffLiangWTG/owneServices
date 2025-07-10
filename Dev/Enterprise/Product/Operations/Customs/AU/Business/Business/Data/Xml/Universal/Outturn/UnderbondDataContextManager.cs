using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UnderbondDataContextManager : ShipmentDataContextManager<CusUnderbond>
	{
		public override DataContextType DataContextType => DataContextType.UnderBond;

		public override ZString DataContextKey => ParentBO.C4_SendersMessageReference;

		public override string DefaultOutputDirectory => "";

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(o => o.Code == RecipientRoleType.COA);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) =>
			new CusUnderbondDataObjectWriter(writeManager);

		public override bool ManagesShipments => true;

		public override bool ManagesEvents => false;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment universalShipment, IXmlImportLogger logger,
			UniversalObjectFactory factory)
		{
			ITopLevelDataObjectReader result = null;
			var coaRecipientRole = universalShipment.DataContext?.RecipientRoleCollection?.FirstOrDefault(rr => rr.Code == RecipientRoleType.COA);
			bool createReader;

			if (coaRecipientRole == null)
			{
				createReader = true;
			}
			else
			{
				if (coaRecipientRole.ServiceCode.HasValue)
				{
					createReader = coaRecipientRole.ServiceCode.Value == ServiceCodeType.AIR;
				}
				else
				{
					var transportMode = universalShipment.TransportMode.GetNullableCodeAsUpperCase();
					createReader = !transportMode.HasValue || transportMode.Value == Core.Constants.TransportModes.Air;
				}
			}

			if (createReader)
			{
				if (universalShipment.IsHVLV())
				{
					result = new ETailCusUnderbondDataObjectReader(universalShipment, logger, factory);
				}
				else if (universalShipment.IsTWH())
				{
					result = new CusUnderbondDataObjectReaderForTransitWarehouse(universalShipment, logger, factory);
				}
				else
				{
					result = new CusUnderbondDataObjectReader(universalShipment, logger, factory);
				}
			}
			return result;
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory,
			IXmlImportLogger logger)
		{
			ZQuery result = null;
			if (!matchingValues.Key.IsEmpty)
			{
				result = new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, matchingValues.Key);
			}
			return result;
		}
	}
}
