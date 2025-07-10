using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	/// <summary>
	/// Currently standalone Bills not supported.
	/// This class is required as the Bill has workflow and customised columns.
	/// </summary>

	public class AsycudaBillDataContextManager : ShipmentDataContextManager<AsycudaBill>
	{
		public override DataContextType DataContextType => DataContextType.AsycudaBill;

		public override ZString DataContextKey => ParentBO.ABL_BillNumber;

		public override bool ManagesShipments => false;

		public override bool ManagesEvents => false;

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory) => null;

		protected override bool TryGetMatchingDataTarget(UniversalShipment universalShipment, out IDataTargetDataObject dataTarget)
		{
			dataTarget = null;
			return false;
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager) => null;

		public override string DefaultOutputDirectory => ZString.Empty;

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger) => false;

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var results = new List<KeyValuePair<TypeWithDescription, IZType>>();
			if (ParentBO?.Header != null)
			{
				var asycudaBillEventContextReader = ParentBO.Header.ApplicationBusinessProvider.GetAsycudaBillEventContextReader(ParentBO);
				asycudaBillEventContextReader.AddEventContextValues(results);
			}

			return results.Any() ? results : null;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger) => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger) => null;
	}
}
