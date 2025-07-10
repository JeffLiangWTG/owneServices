using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromDtbConsignment : FreightWrapper, IDocTypeCode
	{
		public FreightWrapperFromDtbConsignment(DtbConsignment consignment, BusinessObjectFactory factory)
			: base(consignment, factory)
		{
			dtbConsignment = consignment ?? Factory.GetNull<DtbConsignment>();
		}
		readonly DtbConsignment dtbConsignment;

		#region Consignment

		protected DtbConsignment Consignment
		{
			get { return (DtbConsignment)WrappedBO; }
		}

		#endregion

		#region GetJobNumberHeading

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("c223e6d0-b9fe-4bca-af42-0dcf62bab34c", "Consignment ID");
		}

		#endregion

		#region GetJobNumber

		protected override ZString GetJobNumber()
		{
			return string.IsNullOrWhiteSpace(Consignment.LTC_JobID) ? Consignment.LTC_ConnoteNumber : Consignment.LTC_JobID;
		}

		#endregion

		#region GetTransportReferenceHeading

		protected override ZString GetTransportReferenceHeading()
		{
			return Res.GetString("788a921d-ef65-40b2-9813-6189d9803380", "Connote Number");
		}

		#endregion

		#region ParentWrapper

		protected FreightWrapper ParentWrapper
		{
			get
			{
				if (parentWrapper == null)
				{
					if (Consignment != null)
					{
						var wrappers = FreightWrapper.New(Consignment, Factory);
						parentWrapper = wrappers.Length > 0 ? wrappers[0] : null;
					}
				}

				return parentWrapper;
			}
		}

		FreightWrapper parentWrapper;

		#endregion

		#region GetCarrier

		protected override OrganisationWrapper GetCarrier()
		{
			return new OrganisationWrapper(OrganisationUsageType.TransportCompany, GlbCompany.CurrentCompany.OrgProxy, ContactType.LocalTransport, Factory);
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, Carrier.Organisation, ContactType.LocalTransport, Factory);
		}

		#endregion

		#region SecondaryHeading

		protected override ZString GetSecondaryHeading()
		{
			return ParentWrapper != null ? Res.GetString("41053594-c0f2-4f6b-b0de-39597ba82ee3", "Job ID") : "";
		}

		#endregion

		#region Pickup / Delivery From /To

		#region Delivery

		protected override ZDateTime GetDeliveryFrom()
		{
			return DeliveryAddressBO != null ? DeliveryAddressBO.ReqFrom.ToLocalZDateTime() : ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			return DeliveryAddressBO != null ? DeliveryAddressBO.ReqTo.ToLocalZDateTime() : ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryActual()
		{
			var action = DeliveryAddressBO != null ? DeliveryAddressBO.DeliveryAction : null;
			var rsi = action != null ? action.RunSheetInstruction : null;
			return rsi != null ? rsi.K1_TimeOut.ToLocalZDateTime() : ZDateTime.Empty;
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return DeliveryAddressBO != null && !DeliveryAddressBO.Address.IsEmpty ? new AddressWrapper(DeliveryAddressBO.Address, Factory) : null;
		}

		protected override ZString GetCustomerReference()
		{
			return DeliveryAddressBO != null && DeliveryAddressBO.Actions.Count > 0 ? DeliveryAddressBO.Actions[0].LTA_ReferenceNumber : ZString.Empty;
		}

		#endregion

		#region Pickup

		protected override ZDateTime GetPickupFrom()
		{
			return PickupAddressBO != null ? PickupAddressBO.ReqFrom.ToLocalZDateTime() : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupRequiredBy()
		{
			return PickupAddressBO != null ? PickupAddressBO.ReqTo.ToLocalZDateTime() : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupActual()
		{
			var action = PickupAddressBO != null ? PickupAddressBO.PickupAction : null;
			var rsi = action != null ? action.RunSheetInstruction : null;
			return rsi != null ? rsi.K1_TimeOut.ToLocalZDateTime() : ZDateTime.Empty;
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return PickupAddressBO != null && !PickupAddressBO.Address.IsEmpty ? new AddressWrapper(PickupAddressBO.Address, Factory) : null;
		}

		protected override ZString GetOwnerReference()
		{
			return PickupAddressBO != null && PickupAddressBO.Actions.Count > 0 ? PickupAddressBO.Actions[0].LTA_ReferenceNumber : ZString.Empty;
		}

		#endregion

		#region GetConsignor

		protected override OrganisationWrapper GetConsignor()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignor,
				PickupAddressBO != null ? PickupAddressBO.Address : Factory.GetNull<JobDocAddress>(), Factory);
		}

		#endregion

		#region GetConsignee

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee,
				DeliveryAddressBO != null ? DeliveryAddressBO.Address : Factory.GetNull<JobDocAddress>(), Factory);
		}

		#endregion

		#endregion

		#region ServiceLevel

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return Consignment != null
				? new CodeAndDescriptionWrapper(Consignment.LTC_RS_NKServiceLevel, Consignment.Lookups.ServiceLevels, Factory)
				: CodeAndDescriptionWrapper.Empty;
		}

		#endregion

		#region PickupAddressBO

		DtbConsignmentAddress PickupAddressBO
		{
			get { return pickupAddressBO ?? (pickupAddressBO = GetConsignmentPickUpAddress()); }
		}

		DtbConsignmentAddress GetConsignmentPickUpAddress()
		{
			return (Consignment == null || !Consignment.Addresses.Any(a => a.LTS_InstructionType == ConsignmentAddressTypes.Codes.PickUp))
				? Factory.GetNull<DtbConsignmentAddress>()
				: Consignment.PickupAddress;
		}

		DtbConsignmentAddress pickupAddressBO;

		#endregion

		#region DeliveryAddressBO

		DtbConsignmentAddress DeliveryAddressBO
		{
			get { return deliveryAddressBO ?? (deliveryAddressBO = GetConsignmentDeliveryAddress()); }
		}

		DtbConsignmentAddress GetConsignmentDeliveryAddress()
		{
			return (Consignment == null || !Consignment.Addresses.Any(a => a.LTS_InstructionType == ConsignmentAddressTypes.Codes.Delivery))
				? Factory.GetNull<DtbConsignmentAddress>()
				: Consignment.DeliveryAddress;
		}

		DtbConsignmentAddress deliveryAddressBO;

		#endregion

		#region TextForBarcode

		protected override ZString TextForBarcode
		{
			get { return Consignment.LTC_JobID; }
		}

		#endregion

		#region PackagesDetails

		protected override ZString GetPackagesDetails()
		{
			var detailBuilder = new ZStringBuilder();

			var packages = dtbConsignment.PackageJob.Packages.Where(p => p.KP_KP_ParentPackage == ZGuid.Empty)
				.GroupBy(p => new { PkgCode = p.KP_F3_NKPackType, WeightUnit = p.KP_WeightUQ, VolumeUnit = p.KP_VolumeUQ })
				.Select(p => new
				{
					Key = p.Key,
					Count = p.Sum(package => package.KP_PackageQty),
					Weight = p.Sum(package => package.KP_Weight),
					Volume = p.Sum(package => package.KP_Volume)
				});

			foreach (var item in packages)
			{
				var detail = new ZStringBuilder();
				detail.Append(Res.GetString("5fc2b261-a7c5-482d-842b-2807c237d2ff", "{0}x {1}", item.Count, item.Key.PkgCode));
				detail.Append(Res.GetString("9a8a5f5c-3f1d-45ef-8d3b-a2a0d8c6522b", "Wgt: {0} {1}", item.Weight.ToString("N3", ObjectCache.CultureProvider.Culture.NumberFormat), item.Key.WeightUnit));
				detail.Append(Res.GetString("73de224d-fb68-46bb-8294-52d7d7255e2f", "Vol: {0} {1}", item.Volume.ToString("N3", ObjectCache.CultureProvider.Culture.NumberFormat), item.Key.VolumeUnit));
				detailBuilder.Append(detail.ToStringWithDelimiterBetweenAppends(", "));
			}

			return detailBuilder.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(Consignment, Factory);
		}

		protected override ZString GetTransportReference()
		{
			return Consignment.LTC_ConnoteNumber;
		}

		protected override ZString GetConNote()
		{
			return Consignment.LTC_ConnoteNumber;
		}

		protected override ZBool GetHazardous()
		{
			return dtbConsignment.LTC_IsHazardous;
		}

		protected override ZBool GetRefrigerated()
		{
			return dtbConsignment.LTC_RequiresRefrigeration;
		}

		protected override RequiredDocumentsWrapperCollection GetRequiredDocuments()
		{
			return new RequiredDocumentsWrapperCollection(Factory);
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(dtbConsignment.Services, Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			var total = ZWeight.Empty;
			if (Consignment != null)
			{
				total = new ZWeight(Consignment.PackageJob.Weight, Consignment.PackageJob.WeightUQ);
			}

			return new WeightWrapper(total.Amount, total.Unit, WeightWrapper.StandardDecimalPlaces, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			var total = ZVolume.Empty;
			if (Consignment != null)
			{
				total = new ZVolume(Consignment.PackageJob.Volume, Consignment.PackageJob.VolumeUQ);
			}

			return new VolumeWrapper(total.Amount, total.Unit, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		protected override ZString GetSecondaryNumber()
		{
			return ParentWrapper != null ? ParentWrapper.JobNumber : ZString.Empty;
		}

		protected override InstructionWrapperCollection GetBookingInstructions()
		{
			var addresses = new InstructionWrapperCollection(Factory);
			foreach (var address in Consignment.Addresses)
			{
				AddWrappersFromInstructionConfirmations(addresses, address);
			}
			return addresses;
		}

		void AddWrappersFromInstructionConfirmations(InstructionWrapperCollection addresses, DtbConsignmentAddress address)
		{
			foreach (var action in address.Actions)
			{
				var instructionWrapper = new ConsignmentAddressWrapper(address, action, Factory);
				addresses.Add(instructionWrapper);
			}
		}

		protected override AddressWrapperCollection GetTransportAddresses()
		{
			var addresses = new AddressWrapperCollection(Factory);

			foreach (var consignmentAddress in Consignment.Addresses)
			{
				AddAddressIfNotTheSameAsLast(addresses, consignmentAddress.Address);
			}

			return addresses;
		}

		void AddAddressIfNotTheSameAsLast(AddressWrapperCollection addresses, JobDocAddress docAddress)
		{
			if (docAddress != null)
			{
				var lastAddress = addresses.Cast<AddressWrapper>().LastOrDefault();
				if (lastAddress == null || !docAddress.IsTheSameAddressAs((IDocAddress)lastAddress.WrappedObject))
				{
					addresses.Add(new AddressWrapper(docAddress, Factory));
				}
			}
		}

		protected override PackageWrapperCollection GetPackages()
		{
			return new PackageWrapperCollection(Consignment, Factory);
		}

		protected override PackQTYWrapper GetShipmentOuterPacksQty()
		{
			var total = Consignment?.LoosePackages?.Sum(p => p.KP_PackageQty);
			var unit = GetShipmentOuterPacksUnit();

			return new PackQTYWrapper(total ?? 0, unit, BindToLists.GetCachedLists(Factory).OuterPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		ZString GetShipmentOuterPacksUnit()
		{
			var unit = ZString.Empty;

			var firstUnit = Consignment?.LoosePackages?.FirstOrDefault()?.KP_F3_NKPackType;
			if (firstUnit.HasValue && Consignment.LoosePackages.All(p => p.KP_F3_NKPackType == firstUnit.Value))
			{
				unit = firstUnit.Value;
			}
			else
			{
				unit = Constants.PkgUnit.Package;
			}

			return unit;
		}

		protected override Job GetJob()
		{
			return Consignment == null ? null : (Job)new JobHeader.Loader(Consignment).Load();
		}

		protected override OrganisationWrapper NewJobHeaderLocalClient()
		{
			OrganisationWrapper orgWrapper;

			if (Job == null && Consignment != null)
			{
				orgWrapper = new OrganisationWrapper(OrganisationUsageType.LocalClient, Consignment.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty), Factory);
			}
			else
			{
				orgWrapper = base.NewJobHeaderLocalClient();
			}

			return orgWrapper;
		}
	}
}
