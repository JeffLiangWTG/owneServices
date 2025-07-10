using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromWhsItemReceiveConsignment : FreightWrapperFromTransitPackageJob, IPackageOverrider
	{
		public FreightWrapperFromWhsItemReceiveConsignment(WhsItemReceiveConsignment receiveConsignment, BusinessObjectFactory factory)
			: base(receiveConsignment, factory)
		{
			ReceiveConsignment = receiveConsignment ?? Factory.GetNull<WhsItemReceiveConsignment>();
		}

		readonly WhsItemReceiveConsignment ReceiveConsignment;

		#region FirstAndOnlyDispatchConsignment

		FreightWrapper FirstAndOnlyDispatchConsignment
		{
			get
			{
				FreightWrapper result = null;

				var packageWrappers = Packages.Cast<PackageWrapper>();
				var firstDispatchConsignment = packageWrappers.FirstOrDefault()?.PackageState?.DispatchConsignment;
				if (firstDispatchConsignment != null && packageWrappers.All(p => p.PackageState?.DispatchConsignment?.WrappedObjectPK == firstDispatchConsignment.WrappedObjectPK))
				{
					result = firstDispatchConsignment;
				}

				return result;
			}
		}

		#endregion

		#region JobNumbers

		protected override ZString GetJobNumberHeading() => Res.GetString("54d07ee6-ad5c-4d2a-99c6-be7962613e64", "Receive Consignment");

		protected override ZString GetJobNumber() => ReceiveConsignment.WRC_JobID;

		protected override ZString GetSecondaryHeading() => Res.GetString("1e85254d-6e39-41de-8ebd-a7a0ae9ba28e", "RCN Reference");

		protected override ZString GetSecondaryNumber() => ReceiveConsignment.WRC_ConsignmentID;

		#endregion

		#region Addresses

		protected override OrganisationWrapper GetBookingParty()
		{
			var bookingPartyDocAddress = ((IDocAddresses)ReceiveConsignment).DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
			return new OrganisationWrapper(OrganisationUsageType.BookingParty, bookingPartyDocAddress, Factory);
		}

		protected override OrganisationWrapper GetClient()
		{
			return BookingParty;
		}

		protected override Job GetJob()
		{
			return ReceiveConsignment == null ? null : (Job)new JobHeader.Loader(ReceiveConsignment).Load();
		}

		protected override OrganisationWrapper NewJobHeaderLocalClient()
		{
			if (Job != null && Job.LocalCharges != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.LocalClient, Job.LocalCharges, ContactType.Receivables, Factory);
			}
			else if (Job == null && ReceiveConsignment != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.LocalClient, ReceiveConsignment.ClientRequestedBillToPartyDocAddress, Factory);
			}
			else
			{
				return base.NewJobHeaderLocalClient();
			}
		}

		protected override OrganisationWrapper GetConsignor()
		{
			var consignorDocAddress = ((IDocAddresses)ReceiveConsignment).DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			return new OrganisationWrapper(OrganisationUsageType.Consignor, consignorDocAddress, Factory);
		}

		protected override OrganisationWrapper GetConsignee()
		{
			return DeliveryAddress.Organization;
		}

		protected override AddressWrapper GetImportArrivalCTOAddress()
		{
			return new AddressWrapper(OrganisationUsageType.CTOArrival, ReceiveConsignment.CTODocAddress, Factory);
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return new AddressWrapper(OrganisationUsageType.Consignor, ReceiveConsignment.ConsignorPickupDeliveryAddress, Factory);
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			AddressWrapper result = FirstAndOnlyDispatchConsignment?.DeliveryAddress;

			if (result == null || result.CompanyName.IsEmpty)
			{
				var consigneeDocAddress = ((IDocAddresses)ReceiveConsignment).DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
				result = new AddressWrapper(OrganisationUsageType.Consignee, consigneeDocAddress, Factory);
			}

			return result;
		}

		#endregion

		#region Packages / Containers

		#region GetPackages

		protected override PackageWrapperCollection GetPackages()
		{
			return overriddenPackages ?? WarehouseJob.Packages;
		}
		PackageWrapperCollection overriddenPackages;

		#endregion

		#region GetShipmentOuterPacksQty

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			var total = 0;
			var unit = ZString.Empty;

			var packages = PackageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers();
			foreach (var package in packages)
			{
				total += package.KP_PackageQty;

				if (unit.IsEmpty)
				{
					unit = package.KP_F3_NKPackType;
				}
				else if (unit != package.KP_F3_NKPackType)
				{
					unit = Constants.PkgUnit.Package;
				}
			}

			return new PackQTYWrapper(total, unit, new RefPackTypeCollection(Factory).GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetShipmentInnerPacksQty

		protected override PackQTYWrapper GetShipmentInnerPacksQty()
		{
			var total = 0;
			var unit = ZString.Empty;

			var packages = PackageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers();
			var innerPackages = packages.SelectMany(p => p.Packages);

			foreach (var package in innerPackages)
			{
				total += package.KP_PackageQty;

				if (unit.IsEmpty)
				{
					unit = package.KP_F3_NKPackType;
				}
				else if (unit != package.KP_F3_NKPackType)
				{
					unit = Constants.PkgUnit.Package;
				}
			}

			return new PackQTYWrapper(total, unit, new RefPackTypeCollection(Factory).GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetWeight

		protected override WeightWrapper GetWeight()
		{
			return new WeightWrapper(PackageJob.Weight, PackageJob.WeightUQ, WeightWrapper.StandardDecimalPlaces, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		#endregion

		#region GetVolume

		protected override VolumeWrapper GetVolume()
		{
			return new VolumeWrapper(PackageJob.Volume, PackageJob.VolumeUQ, WeightWrapper.StandardDecimalPlaces, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		#endregion

		#region GetDocumentNumber

		protected override ZInt GetDocumentNumber()
		{
			return (Packages.FirstOrDefault() as PackageWrapperFromPkgPackageHeader)?.OutterPackageSequence ?? ZInt.Zero;
		}

		#endregion

		#region PackageJob

		PkgPackageJob PackageJob
		{
			get { return packageJob ?? (packageJob = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(ReceiveConsignment)); }
		}
		PkgPackageJob packageJob;

		#endregion

		#region short/over

		protected override ZInt GetShorts()
		{
			ZInt res = 0;
			if (Packages != null)
			{
				res = (ZInt)Packages
						.Select(p => p as PackageWrapperFromPkgPackage)
						.Where(p => p != null)
						.Where(p => p.PackageState.Status.Code == TransitWarehouseStatuses.Codes.Booked && p.GetPackageBookedDetail().KPB_PackageQty > 0)
						.Sum(p => p.Packages.Value);
			}

			return res;
		}

		protected override ZInt GetOvers()
		{
			ZInt res = 0;
			if (Packages != null)
			{
				res = (ZInt)Packages
						.Select(p => p as PackageWrapperFromPkgPackage)
						.Where(p => p != null)
						.Where(p => p.PackageState.Status.Code != TransitWarehouseStatuses.Codes.Booked && p.GetPackageBookedDetail().KPB_PackageQty == 0)
						.Sum(p => p.Packages.Value);
			}

			return res;
		}

		#endregion

		#endregion

		#region HouseBill

		protected override ZString GetHouseBill()
		{
			var result = FirstAndOnlyDispatchConsignment != null ? FirstAndOnlyDispatchConsignment.HouseBill : ZString.Empty;
			if (result.IsEmpty)
			{
				result = ReceiveConsignment.WRC_HouseBillNumber;
			}

			return result;
		}

		protected override ZString GetHouseBillHeading() => HeadingHelper.GetHouseBillHeading(ReceiveConsignment.WRC_TransportMode);

		#endregion

		#region MasterBill

		protected override ZString GetMasterBill()
		{
			var result = ZString.Empty;
			var packageWrapper = Packages.FirstOrDefault();
			if (packageWrapper is PackageWrapperFromPkgPackage)
			{
				var packageStateWrapper = (packageWrapper as PackageWrapperFromPkgPackage)?.PackageState;
				result = packageStateWrapper?.MasterBill ?? ZString.Empty;
			}
			else if (packageWrapper is PackageWrapperFromPkgPackageHeader)
			{
				var masterBillReference = ReceiveConsignment.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.MasterBill);
				result = masterBillReference?.CE_EntryNum ?? ZString.Empty;
			}
			return result;
		}

		protected override ZString GetMasterBillHeading() => HeadingHelper.GetMasterBillHeading(ReceiveConsignment.WRC_TransportMode);

		#endregion

		#region Destination

		protected override PlaceAndDateWrapper GetDestination() => new PlaceAndDateWrapper(ReceiveConsignment.WRC_RL_NKDestination, ZDateTime.Empty, ZDateTime.Empty, Factory);

		#endregion

		#region ParentJob

		protected override FreightWrapper GetParentJob()
		{
			if (parentJob == null)
			{
				var freightWrappers = FreightWrapper.New((BusinessObject)PackageJob.ParentJob, Factory);
				parentJob = freightWrappers.Length > 0 ? freightWrappers[0] : null;
			}

			return parentJob;
		}

		FreightWrapper parentJob;

		#endregion

		#region Service Levels

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(ReceiveConsignment.WRC_RS_NKServiceLevel, ReceiveConsignment.Lookups.ServiceLevels, Factory);
		}

		#endregion

		#region Shipment

		protected override CodeAndDescriptionWrapper GetShipmentType() => new CodeAndDescriptionWrapper(ReceiveConsignment.WRC_Direction, new ConsignmentDirections(), Factory);

		#endregion

		#region JobService

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(ReceiveConsignment.Services, Factory);
		}

		#endregion

		#region Additional References

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(ReceiveConsignment, Factory);
		}

		#endregion

		#region WarehouseJob

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			return new TransitWarehouseReceiveConsignmentWrapper(ReceiveConsignment, Factory);
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

		#region Child Collections

		protected override RouteWrapperCollection GetConsolRoutes()
		{
			return new RouteWrapperCollection(ReceiveConsignment, Factory);
		}

		protected override RouteWrapperCollection GetShipmentRoutes()
		{
			return new RouteWrapperCollection(ReceiveConsignment, Factory);
		}

		#endregion

		#region TransitJobTransportMode

		protected override CodeAndDescriptionWrapper GetTransitJobTransportMode()
		{
			return new CodeAndDescriptionWrapper(ReceiveConsignment.WRC_TransportMode, new TransitWarehouseTransportModeList(), Factory);
		}

		#endregion

		#region WarehouseNextDischargePort

		protected override ZString GetWarehouseNextDischargePort() => ReceiveConsignment.WRC_RL_NKNextDischargePort;

		#endregion
	}
}
