using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromWhsItemReceiveTransportationUnit : FreightWrapperFromTransitPackageJob, IPackageOverrider
	{
		public FreightWrapperFromWhsItemReceiveTransportationUnit(WhsItemReceiveTransportationUnit receiveTransportationUnit, BusinessObjectFactory factory)
			: base(receiveTransportationUnit, factory)
		{
			ReceiveTransportationUnit = receiveTransportationUnit ?? Factory.GetNull<WhsItemReceiveTransportationUnit>();
		}

		readonly WhsItemReceiveTransportationUnit ReceiveTransportationUnit;

		#region Job

		protected override Job GetJob()
		{
			return ReceiveTransportationUnit == null ? null : (Job)ReceiveTransportationUnit.JobHeader;
		}

		#endregion

		#region JobHeaderLocalClient 

		protected override OrganisationWrapper NewJobHeaderLocalClient()
		{
			if (Job != null && Job.LocalCharges != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.LocalClient, Job.LocalCharges, ContactType.Receivables, Factory);
			}
			else if (Job == null && ReceiveTransportationUnit != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.LocalClient, ReceiveTransportationUnit.ClientRequestedBillToPartyDocAddress, Factory);
			}
			else
			{
				return base.NewJobHeaderLocalClient();
			}
		}

		#endregion

		#region JobNumbers

		protected override ZString GetJobNumberHeading() => Res.GetString("00737d87-28ad-4fea-905a-0eae943f15ad", "Transit Receive Transportation Unit");

		protected override ZString GetJobNumber() => ReceiveTransportationUnit.WRH_ReferenceNumber;

		protected override ZString GetSecondaryHeading() => ReceiveTransportationUnit.IsContainerUnitType
			? Res.GetString("FA4C1F2A-299A-4800-B2F4-629E865C2AB3", "Container Reference")
			: Res.GetString("F6E144F3-6786-449B-95EF-347B7B9CD51E", "Vehicle Reference");
		protected override ZString GetSecondaryNumber() => ReceiveTransportationUnit.WRH_VehicleReference;

		#endregion

		#region WarehouseJob

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			return new WhsItemReceiveTransportationUnitWrapper(ReceiveTransportationUnit, Factory);
		}

		#endregion

		#region GetPackages

		protected override PackageWrapperCollection GetPackages()
		{
			return overriddenPackages ?? WarehouseJob.Packages;
		}
		PackageWrapperCollection overriddenPackages;

		#endregion

		#region GetShippersReference

		protected override ZString GetShippersReference()
		{
			return OrderNumbersWithOwnersReference;
		}

		#endregion

		#region IPackageOverrider

		void IPackageOverrider.SetPackageOverride(PkgPackage package, PkgPackageItemDivotsWrapper[] packedItems, int documentNumber, int documentTotal, int uomTypeNumber, int uomTypeTotal)
		{
			overriddenPackages = new PackageWrapperCollection(Factory);
			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			overriddenPackages.Add(packageWrapper);
		}

		void IPackageOverrider.SetPackageCollectionOverride(PkgPackage[] packages, PkgPackageHeader[] packageHeaders)
		{
			overriddenPackages = CreatePackageWrapperCollection(Factory, packages, packageHeaders);
		}

		#endregion

		#region HouseBill

		protected override ZString GetHouseBill()
		{
			if (ReceiveTransportationUnit.ReceiveConsignments.Count == 0)
			{
				return string.Empty;
			}
			var houseBillReference = ReceiveTransportationUnit.ReceiveConsignments.FirstOrDefault(r => r.WRC_HouseBillNumber != "")?.WRC_HouseBillNumber;
			return houseBillReference ?? string.Empty;
		}

		protected override ZString GetHouseBillHeading() => HeadingHelper.GetHouseBillHeading(ReceiveTransportationUnit.TransportMode);

		#endregion

		#region MasterBill

		protected override ZString GetMasterBill()
		{
			var masterBillReference = ReceiveTransportationUnit.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.MasterBill);
			return masterBillReference?.CE_EntryNum ?? ZString.Empty;
		}

		protected override ZString GetMasterBillHeading() => HeadingHelper.GetMasterBillHeading(ReceiveTransportationUnit.TransportMode);

		#endregion

		#region Additional References

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(ReceiveTransportationUnit, Factory);
		}

		#endregion

		#region GateInTime

		protected override ZDateTimeOffset GetGateInTime() => ReceiveTransportationUnit.WRH_GateInTime;

		#endregion

		#region ArrivalCFSTransport

		protected override OrganisationWrapper GetArrivalCFSTransport()
		{
			return new OrganisationWrapper(OrganisationUsageType.TransportCompany, ReceiveTransportationUnit.TransportCompany, Factory);
		}

		#endregion

		#region GateOutTime

		protected override ZDateTimeOffset GetGateOutTime() => ReceiveTransportationUnit.WRH_GateOutTime;

		#endregion

		#region TransportReference

		protected override ZString GetTransportReference() => ReceiveTransportationUnit.TransportReference;

		#endregion

		#region BookingReference

		protected override ZString GetBookingReference() => ReceiveTransportationUnit.CarrierBookingReference;

		#endregion
	}
}
