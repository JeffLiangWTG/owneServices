using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromWhsItemDispatchTransportationUnit : FreightWrapper
	{
		public FreightWrapperFromWhsItemDispatchTransportationUnit(WhsItemDispatchTransportationUnit dispatchUnit, BusinessObjectFactory factory)
			: base(dispatchUnit, factory)
		{
			TransitDispatchTransportationUnit = dispatchUnit ?? Factory.GetNull<WhsItemDispatchTransportationUnit>();
		}

		readonly WhsItemDispatchTransportationUnit TransitDispatchTransportationUnit;

		protected override Job GetJob()
		{
			return TransitDispatchTransportationUnit == null ? null : (Job)new JobHeader.Loader(TransitDispatchTransportationUnit).Load();
		}

		#region JobHeaderLocalClient

		protected override OrganisationWrapper NewJobHeaderLocalClient()
		{
			if (Job != null && Job.LocalCharges != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.LocalClient, Job.LocalCharges, ContactType.Receivables, Factory);
			}
			else if (Job == null && TransitDispatchTransportationUnit != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.LocalClient, TransitDispatchTransportationUnit.ClientRequestedBillToPartyDocAddress, Factory);
			}
			else
			{
				return base.NewJobHeaderLocalClient();
			}
		}

		#endregion

		#region JobNumbers

		protected override ZString GetJobNumberHeading() => Res.GetString("a0d3e42d-2089-4602-ad16-7ff09899601a", "Transit Dispatch Transportation Unit");

		protected override ZString GetJobNumber() => TransitDispatchTransportationUnit.WDH_ReferenceNumber;

		protected override ZString GetSecondaryHeading() => TransitDispatchTransportationUnit.IsContainerUnitType
			? Res.GetString("bd5ada0c-43d2-41d3-9d05-6c7e16169de4", "Container Reference")
			: Res.GetString("e764132e-ccc8-4063-9f7d-608776f0a575", "Vehicle Reference");

		protected override ZString GetSecondaryNumber() => TransitDispatchTransportationUnit.WDH_VehicleReference;

		#endregion

		#region WarehouseJob

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			return new WhsItemDispatchTransportationUnitWrapper(TransitDispatchTransportationUnit, Factory);
		}

		#endregion

		#region Empty

		#region Packages / Containers

		#region GetPackages

		protected override PackageWrapperCollection GetPackages()
		{
			return WarehouseJob.Packages;
		}

		#endregion

		#endregion

		protected override ZString GetShippersReference()
		{
			return OrderNumbersWithOwnersReference;
		}

		#endregion

		#region HouseBill

		protected override ZString GetHouseBill()
		{
			if (TransitDispatchTransportationUnit.PackageStates.Count == 0)
			{
				return string.Empty;
			}
			var houseBillReference = TransitDispatchTransportationUnit.PackageStates.FirstOrDefault(p => p.DispatchConsignment != null && p.DispatchConsignment.WDC_HouseBillNumber != "")?.DispatchConsignment.WDC_HouseBillNumber;
			return houseBillReference ?? string.Empty;
		}

		protected override ZString GetHouseBillHeading() => HeadingHelper.GetHouseBillHeading(TransitDispatchTransportationUnit.TransportMode);

		#endregion

		#region MasterBill

		protected override ZString GetMasterBill()
		{
			var masterBillReference = TransitDispatchTransportationUnit.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.MasterBill);
			return masterBillReference?.CE_EntryNum ?? ZString.Empty;
		}

		protected override ZString GetMasterBillHeading() => HeadingHelper.GetMasterBillHeading(TransitDispatchTransportationUnit.TransportMode);

		#endregion

		#region BookingReference

		protected override ZString GetBookingReference() => TransitDispatchTransportationUnit.CarrierBookingReference;

		#endregion

		#region Additional References

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(TransitDispatchTransportationUnit, Factory);
		}

		#endregion

		#region GateInTime

		protected override ZDateTimeOffset GetGateInTime() => TransitDispatchTransportationUnit.WDH_GateInTime;

		#endregion

		#region GateOutTime

		protected override ZDateTimeOffset GetGateOutTime() => TransitDispatchTransportationUnit.WDH_GateOutTime;

		#endregion

		#region DepartureCFSTransport

		protected override OrganisationWrapper GetDepartureCFSTransport()
		{
			return new OrganisationWrapper(OrganisationUsageType.TransportCompany, TransitDispatchTransportationUnit.TransportCompany, Factory);
		}

		#endregion
	}
}
