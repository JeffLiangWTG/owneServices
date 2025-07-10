using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromWhsItemDispatchLoadList : FreightWrapper
	{
		public FreightWrapperFromWhsItemDispatchLoadList(WhsItemDispatchLoadList dispatchLoadList, BusinessObjectFactory factory)
			: base(dispatchLoadList, factory)
		{
			DispatchLoadList = dispatchLoadList ?? Factory.GetNull<WhsItemDispatchLoadList>();
		}

		readonly WhsItemDispatchLoadList DispatchLoadList;

		#region Priority

		protected override ZByte GetPriority() => DispatchLoadList.WDL_Priority;

		#endregion

		#region JobNumbers

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("4e5794f8-4719-4b7c-b8ab-e502b5c5d57f", "Dispatch Load List");
		}

		protected override ZString GetJobNumber()
		{
			return DispatchLoadList.WDL_JobID;
		}

		#endregion

		#region SecondaryNumbers

		protected override ZString GetSecondaryHeading() => Res.GetString("e147f056-c274-4503-a4b6-c2019fca20ca", "Dispatch Load List Reference Number");

		protected override ZString GetSecondaryNumber() => DispatchLoadList.WDL_ReferenceNumber;

		#endregion

		#region Destination

		protected override PlaceAndDateWrapper GetDestination() => new (DispatchLoadList.WDL_RL_NKLastDischargePort, ZDateTime.Empty, ZDateTime.Empty, Factory);

		#endregion

		#region Additional References

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(DispatchLoadList, Factory);
		}

		#endregion

		#region WarehouseJob

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			return new WhsItemDispatchLoadListWrapper(DispatchLoadList, Factory);
		}

		#endregion

		#region GetPackages

		protected override PackageWrapperCollection GetPackages() => WarehouseJob.Packages;

		#endregion

		#region HouseBill

		protected override ZString GetHouseBill()
		{
			if (DispatchLoadList.PackageStates.Count == 0)
			{
				return string.Empty;
			}

			var houseBillReference = DispatchLoadList.PackageStates.FirstOrDefault(p => p.DispatchConsignment != null && p.DispatchConsignment.WDC_HouseBillNumber != "")?.DispatchConsignment.WDC_HouseBillNumber;
			return houseBillReference ?? string.Empty;
		}

		protected override ZString GetHouseBillHeading() => HeadingHelper.GetHouseBillHeading(DispatchLoadList.WDL_TransportMode);

		#endregion

		#region MasterBill

		protected override ZString GetMasterBill()
		{
			var masterBillReference = DispatchLoadList.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.MasterBill);
			return masterBillReference?.CE_EntryNum ?? ZString.Empty;
		}

		protected override ZString GetMasterBillHeading() => HeadingHelper.GetMasterBillHeading(DispatchLoadList.WDL_TransportMode);

		#endregion

		#region BookingReference

		protected override ZString GetBookingReference() => DispatchLoadList.CarrierBookingReference;

		#endregion

		#region Child Collections

		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return new RouteWrapperCollection(DispatchLoadList, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(DispatchLoadList, Factory);
		}

		#endregion

		#region TransitJobTransportMode

		protected override CodeAndDescriptionWrapper GetTransitJobTransportMode()
		{
			return new CodeAndDescriptionWrapper(DispatchLoadList.WDL_TransportMode, new TransitWarehouseTransportModeList(), Factory);
		}

		#endregion
	}
}
