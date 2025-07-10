using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class AFRBillDataContextManager : ShipmentDataContextManager<JPAFRBills>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.AFRBill; }
		}

		public override ZString DataContextKey
		{
			get { return ZString.Empty; }
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			throw new System.NotImplementedException();
		}

		protected override bool TryGetMatchingDataTarget(UniversalShipment universalShipment, out IDataTargetDataObject dataTarget)
		{
			dataTarget = null;
			return false;
		}

		public override bool ManagesShipments
		{
			get { return false; }
		}

		public override bool ManagesEvents
		{
			get { return false; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			throw new System.NotImplementedException();
		}

		public override string DefaultOutputDirectory
		{
			get { throw new System.NotImplementedException(); }
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();
			var bill = ParentBO;
			if (bill != null)
			{
				var header = bill.Header;
				if (header != null)
				{
					new AFRHeaderDataEventContextReader(header).AddEventContextValues(result, false);
					var helper = new EventContextValuesHelper(false, result);
					helper.AddHouseBillNumberAndPortCodes(bill.JPB_BillNumber, bill.Origin, bill.FinalDestination);
				}
			}

			return result.Count == 0 ? null : result;
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			throw new System.NotImplementedException();
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}
	}
}
