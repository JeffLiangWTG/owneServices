using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Constants = Enterprise.Core.Constants;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromWhsItemDispatchConsignment : FreightWrapperFromTransitPackageJob, IPackageOverrider
	{
		public FreightWrapperFromWhsItemDispatchConsignment(WhsItemDispatchConsignment dispatchConsignment, BusinessObjectFactory factory)
			: base(dispatchConsignment, factory)
		{
			DispatchConsignment = dispatchConsignment ?? Factory.GetNull<WhsItemDispatchConsignment>();
		}

		readonly WhsItemDispatchConsignment DispatchConsignment;

		#region FirstAndOnlyReceiveConsignment

		FreightWrapper FirstAndOnlyReceiveConsignment
		{
			get
			{
				FreightWrapper result = null;

				var firstReceiveConsignment = DispatchConsignment.PackageStates.FirstOrDefault()?.ReceiveConsignment;
				if (firstReceiveConsignment != null && DispatchConsignment.PackageStates.All(p => p.WPS_WRC_TransitReceiveConsignment == firstReceiveConsignment.PK))
				{
					result = new FreightWrapperFromWhsItemReceiveConsignment(firstReceiveConsignment, Factory);
				}

				return result;
			}
		}

		#endregion

		#region JobNumbers

		protected override ZString GetJobNumberHeading() => Res.GetString("0cfed55a-191d-44a7-80c1-d0179dfe15a8", "Dispatch Consignment");

		protected override ZString GetJobNumber() => DispatchConsignment.WDC_JobID;

		protected override ZString GetSecondaryHeading() => Res.GetString("45012adf-93a0-4217-bb63-13465008a91b", "DCN Reference");

		protected override ZString GetSecondaryNumber() => DispatchConsignment.WDC_ConsignmentID;

		#endregion

		#region Destination

		protected override PlaceAndDateWrapper GetDestination()
		{
			if (!DispatchConsignment.WDC_RL_NKDestination.IsEmpty)
			{
				return new PlaceAndDateWrapper(DispatchConsignment.WDC_RL_NKDestination, ZDateTime.Empty, ZDateTime.Empty, Factory);
			}
			return DispatchConsignment.PackageStates.Where(p => p.DispatchLoadList is not null).Select(p => p.WPS_WDL_LoadList).Distinct().Count() == 1
				? new PlaceAndDateWrapper(DispatchConsignment.PackageStates[0].DispatchLoadList.WDL_RL_NKLastDischargePort, ZDateTime.Empty, ZDateTime.Empty, Factory)
				: base.GetDestination();
		}

		#endregion

		#region Addresses

		protected override OrganisationWrapper GetBookingParty()
		{
			var bookingPartyDocAddress = ((IDocAddresses)DispatchConsignment).DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
			return new OrganisationWrapper(OrganisationUsageType.BookingParty, bookingPartyDocAddress, Factory);
		}

		protected override OrganisationWrapper GetClient()
		{
			return BookingParty;
		}

		protected override Job GetJob()
		{
			return DispatchConsignment == null ? null : (Job)new JobHeader.Loader(DispatchConsignment).Load();
		}

		#region JobHeaderLocalClient

		protected override OrganisationWrapper NewJobHeaderLocalClient()
		{
			if (Job != null && Job.LocalCharges != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.LocalClient, Job.LocalCharges, ContactType.Receivables, Factory);
			}
			else if (Job == null && DispatchConsignment != null)
			{
				return new OrganisationWrapper(OrganisationUsageType.LocalClient, DispatchConsignment.ClientRequestedBillToPartyDocAddress, Factory);
			}
			else
			{
				return base.NewJobHeaderLocalClient();
			}
		}

		#endregion

		protected override OrganisationWrapper GetConsignor()
		{
			return PickupAddress.Organization;
		}

		protected override OrganisationWrapper GetConsignee()
		{
			var consigneeDocAddress = ((IDocAddresses)DispatchConsignment).DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
			return new OrganisationWrapper(OrganisationUsageType.Consignee, consigneeDocAddress, Factory);
		}

		#region ExportReceivingCTOAddress

		protected override AddressWrapper GetExportReceivingCTOAddress()
		{
			return new AddressWrapper(OrganisationUsageType.CTODeparture, DispatchConsignment.CTODocAddress, Factory);
		}

		#endregion

		protected override AddressWrapper GetPickupAddress()
		{
			AddressWrapper result = FirstAndOnlyReceiveConsignment?.PickupAddress;

			if (result == null || result.CompanyName.IsEmpty)
			{
				var consignorDocAddress = ((IDocAddresses)DispatchConsignment).DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
				result = new AddressWrapper(OrganisationUsageType.Consignor, consignorDocAddress, Factory);
			}

			return result;
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return new AddressWrapper(OrganisationUsageType.Consignee, DispatchConsignment.DeliveryDocAddress, Factory);
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

		#region GetDocumentNumber

		protected override ZInt GetDocumentNumber()
		{
			return (Packages.FirstOrDefault() as PackageWrapperFromPkgPackageHeader)?.OutterPackageSequence ?? ZInt.Zero;
		}

		#endregion

		#region GetShipmentOuterPacksQty

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			var total = 0;
			var unit = ZString.Empty;

			foreach (PackageWrapper package in Packages)
			{
				total += package.Packages.Value.ToZInt();

				if (unit.IsEmpty)
				{
					unit = package.Packages.Unit.Code;
				}
				else if (unit != package.Packages.Unit.Code)
				{
					unit = Constants.PkgUnit.Package;
				}
			}

			return new PackQTYWrapper(total, unit, new RefPackTypeCollection(Factory).GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#endregion

		#region Service Levels

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(DispatchConsignment.WDC_RS_NKServiceLevel, DispatchConsignment.Lookups.ServiceLevels, Factory);
		}

		#endregion

		#region ShipmentType

		protected override CodeAndDescriptionWrapper GetShipmentType() => new CodeAndDescriptionWrapper(DispatchConsignment.WDC_Direction, new ConsignmentDirections(), Factory);

		#endregion

		#region JobService

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(DispatchConsignment.Services, Factory);
		}

		#endregion

		#region Additional References

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(DispatchConsignment, Factory);
		}

		#endregion

		#region WarehouseJob

		protected override WarehouseJobGenericWrapper GetWarehouseJob()
		{
			return new TransitWarehouseDispatchConsignmentWrapper(DispatchConsignment, Factory);
		}

		#endregion

		#region HouseBill

		protected override ZString GetHouseBill()
		{
			return DispatchConsignment.WDC_HouseBillNumber;
		}

		protected override ZString GetHouseBillHeading() => HeadingHelper.GetHouseBillHeading(DispatchConsignment.WDC_TransportMode);

		#endregion

		#region MasterBill

		protected override ZString GetMasterBill()
		{
			var masterBillReference = DispatchConsignment.AdditionalReferenceNumbers.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.MasterBill);
			return masterBillReference?.CE_EntryNum ?? ZString.Empty;
		}

		protected override ZString GetMasterBillHeading() => HeadingHelper.GetMasterBillHeading(DispatchConsignment.WDC_TransportMode);

		#endregion

		#region TransitJobTransportMode

		protected override CodeAndDescriptionWrapper GetTransitJobTransportMode()
		{
			return new CodeAndDescriptionWrapper(DispatchConsignment.WDC_TransportMode, new TransitWarehouseTransportModeList(), Factory);
		}

		#endregion

		#region DepartureCFSTransport

		protected override OrganisationWrapper GetDepartureCFSTransport()
		{
			return new OrganisationWrapper(OrganisationUsageType.TransportCompany, DispatchConsignment.TransportCompanyDocAddress, Factory);
		}

		#endregion

		#region IPackageOverrider

		void IPackageOverrider.SetPackageOverride(PkgPackage package, PkgPackageItemDivotsWrapper[] packedItems, int documentNumber, int documentTotal, int uomTypeNumber, int uomTypeTotal)
		{
			overriddenPackages = new PackageWrapperCollection(Factory);
			var packageWrapper = new PackageWrapperFromTransitPackage(package, Factory);
			overriddenPackages.Add(packageWrapper);
		}

		void IPackageOverrider.SetPackageCollectionOverride(PkgPackage[] packages, PkgPackageHeader[] packageHeaders)
		{
			var dcnLabelsCount = DispatchConsignment.OuterPackages.Count(p => !p.KP_KPH_PackageHeader.IsEmpty) + DispatchConsignment.PackageJob.LoosePackageIDs.Count;

			Dictionary<PkgPackageHeader, ZGuid?> requireLinkPackageToHeaders = null;
			if (packageHeaders != null && packageHeaders.Length > 0)
			{
				requireLinkPackageToHeaders = new Dictionary<PkgPackageHeader, ZGuid?>();
				var loosePackages = DispatchConsignment.OuterPackages
					.Where(p => p.KP_KPH_PackageHeader.IsEmpty)
					.OrderBy(p => p.KP_Sequence);

				var loosePackagesCount = loosePackages.Sum(p => p.KP_PackageQty);
				var existingLooseIDsCount = DispatchConsignment.PackageJob.LoosePackageIDs.Count;
				var existingLinkedLooseIDsCount = existingLooseIDsCount - packageHeaders.Length;
				var looseIDsNeedToLinkCount = 0;
				var hasExtraHeader = true;
				if (existingLinkedLooseIDsCount < loosePackagesCount)
				{
					looseIDsNeedToLinkCount = loosePackagesCount - existingLinkedLooseIDsCount;

					if (looseIDsNeedToLinkCount >= packageHeaders.Length)
					{
						looseIDsNeedToLinkCount = packageHeaders.Length;
						hasExtraHeader = false;
					}

					var loosePackagePks = new List<ZGuid>();
					foreach (var loosePackage in loosePackages)
					{
						for (var i = 0; i < loosePackage.KP_PackageQty; i++)
						{
							loosePackagePks.Add(loosePackage.PK);
						}
					}
					loosePackagePks = loosePackagePks.Skip(existingLinkedLooseIDsCount).ToList();

					for (var i = 0; i < looseIDsNeedToLinkCount; i++)
					{
						requireLinkPackageToHeaders.Add(packageHeaders[i], loosePackagePks[i]);
					}
				}

				if (hasExtraHeader)
				{
					foreach (var packageHeader in packageHeaders.Skip(looseIDsNeedToLinkCount))
					{
						requireLinkPackageToHeaders.Add(packageHeader, null);
					}
				}
			}
			overriddenPackages = CreatePackageWrapperCollection(Factory, packages, requireLinkPackageToHeaders, Convert.ToInt16(dcnLabelsCount));
		}

		#endregion
	}
}
