using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.DataTransfer.CusTempStorage
{
	public class CusTempStorageJobHeaderDataContextManager : ShipmentDataContextManager<CusTempStorageJobHeader>
	{
		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZQuery result = null;
			if (!matchingValues.Key.IsEmpty)
			{
				result = new ZQuery(CusTempStorageJobHeaderSchema.SJH_JobReference, matchingValues.Key);
			}
			return result;
		}

		public override DataContextType DataContextType => DataContextType.TemporaryStorage;

		public override ZString DataContextKey => ParentBO.SJH_JobReference;

		public override string DefaultOutputDirectory => "";

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues() => null;

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return GetTempStorageJobHeaderDataObjectWriter(writeManager, ParentBO ?? writeManager.Action?.ParentBO as CusTempStorageJobHeader);
		}

		static ITopLevelDataObjectWriter GetTempStorageJobHeaderDataObjectWriter(IDataWritingManager writeManager, CusTempStorageJobHeader header)
		{
			ITopLevelDataObjectWriter result = null;
			if (header != null)
			{
				var countryCode = header.CountryCode;
				if (!countryCode.IsEmpty)
				{
					var provider = header.Factory.GetCusTempStorageJobHeaderDataObjectProvider(countryCode);
					result = provider?.GetTempStorageJobHeaderDataObjectWriter(writeManager);
				}
			}
			return result;
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalDataBuss.DataObjects.Core.UniversalObjectFactory factory) => null;

		public override bool ManagesShipments => true;
	}
}
