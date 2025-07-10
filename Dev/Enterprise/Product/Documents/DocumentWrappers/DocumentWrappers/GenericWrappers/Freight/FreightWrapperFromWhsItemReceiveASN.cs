using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Transit.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromWhsItemReceiveASN : FreightWrapper
	{
		public FreightWrapperFromWhsItemReceiveASN(WhsItemReceiveASN transitReceiveASN, BusinessObjectFactory factory)
			: base(transitReceiveASN, factory)
		{
			TransitReceiveASN = transitReceiveASN ?? Factory.GetNull<WhsItemReceiveASN>();
		}

		readonly WhsItemReceiveASN TransitReceiveASN;

		#region JobNumbers

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("1be3083d-9ee3-4fc8-8224-933c054322e9", "Warehouse Reference");
		}

		protected override ZString GetJobNumber()
		{
			return TransitReceiveASN.WRP_ReferenceNumber;
		}

		#endregion

		#region WarehouseJob

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			return new WhsItemReceiveASNWrapper(TransitReceiveASN, Factory);
		}

		#endregion

		#region GetPackages

		protected override PackageWrapperCollection GetPackages()
		{
			return WarehouseJob.Packages;
		}

		#endregion

		#region GetShippersReference

		protected override ZString GetShippersReference()
		{
			return OrderNumbersWithOwnersReference;
		}

		#endregion

		#region HouseBill

		protected override ZString GetHouseBill()
		{
			if(!TransitReceiveASN.ReceiveConsignments.Any())
			{
				return string.Empty;
			}
			var houseBillReference = TransitReceiveASN.ReceiveConsignments.FirstOrDefault(r => r.WRC_HouseBillNumber != "")?.WRC_HouseBillNumber;
			return houseBillReference ?? string.Empty;
		}

		protected override ZString GetHouseBillHeading() => HeadingHelper.GetHouseBillHeading(TransitReceiveASN.WRP_TransportMode);

		#endregion

		#region MasterBill

		protected override ZString GetMasterBill()
		{
			var masterBillReference = TransitReceiveASN.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.MasterBill);
			return masterBillReference?.CE_EntryNum ?? ZString.Empty;
		}

		protected override ZString GetMasterBillHeading() => HeadingHelper.GetMasterBillHeading(TransitReceiveASN.WRP_TransportMode);

		#endregion

		#region Additional References

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(TransitReceiveASN, Factory);
		}

		#endregion

		#region Child Collections

		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return new RouteWrapperCollection(TransitReceiveASN, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(TransitReceiveASN, Factory);
		}

		#endregion

		#region TransportReference

		protected override ZString GetTransportReference() => TransitReceiveASN.WRP_VehicleReference;

		#endregion

		#region TransitJobTransportMode

		protected override CodeAndDescriptionWrapper GetTransitJobTransportMode()
		{
			return new CodeAndDescriptionWrapper(TransitReceiveASN.WRP_TransportMode, new TransitWarehouseTransportModeList(), Factory);
		}

		#endregion

		#region ArrivalCFSTransport

		protected override OrganisationWrapper GetArrivalCFSTransport()
		{
			var transportCoAddress = ((IDocAddresses)TransitReceiveASN).DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			return new OrganisationWrapper(OrganisationUsageType.TransportCompany, transportCoAddress, Factory);
		}

		#endregion

		#region BookingReference

		protected override ZString GetBookingReference()
		{
			return TransitReceiveASN.CarrierBookingReference;
		}

		#endregion

		#region BookingParty

		protected override OrganisationWrapper GetBookingParty() => new OrganisationWrapper(OrganisationUsageType.BookingParty, TransitReceiveASN.BookingPartyDocAddress, Factory);

		#endregion
	}
}
