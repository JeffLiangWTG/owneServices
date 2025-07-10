using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportConsignment.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromDtbBookingConsignment : FreightWrapperFromDtbTransport<DtbBookingConsignment>
	{
		public FreightWrapperFromDtbBookingConsignment(DtbBookingConsignment dtbBookingConsignment, BusinessObjectFactory factory)
			: base(dtbBookingConsignment, factory)
		{
			dtbConsignment = dtbBookingConsignment ?? Factory.GetNull<DtbBookingConsignment>();
		}
		readonly DtbBookingConsignment dtbConsignment;

		#region GetJobNumberHeading

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("3f6e82f9-68bb-40ac-9346-6927ca52e49f", "Consignment ID");
		}

		#endregion

		#region GetJobNumber

		protected override ZString GetJobNumber()
		{
			return string.IsNullOrWhiteSpace(Transport.KM_TransportReference) ? Transport.KM_JobID : Transport.KM_TransportReference;
		}

		#endregion

		#region GetTransportReferenceHeading

		protected override ZString GetTransportReferenceHeading()
		{
			return Res.GetString("9fd8f419-cc11-45cc-81a3-bdbbc9e71a6b", "Connote Number");
		}

		#endregion

		#region GetBookingReference

		protected override ZString GetBookingReference()
		{
			return ParentWrapper != null ? ParentWrapper.JobNumber : ZString.Empty;
		}

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

		#region GetCartageInfo

		protected override CartageInfoWrapper GetCartageInfo()
		{
			return CartageInfoWrapper.New(Factory);
		}

		#endregion

		#region Instructions

		protected override TransportInstructionWrapper GetInstructionWrapper(DtbTransportInstruction instruction, BusinessObjectFactory factory)
		{
			return new ConsignmentInstructionWrapper((DtbConsignmentInstruction)instruction, Factory);
		}

		protected override TransportInstructionWrapper GetInstructionWrapper(DtbTransportInstruction instruction, DtbTransportInstructionPkgDivot divot, DtbTransportConfirmation confirmation, BusinessObjectFactory factory)
		{
			return new ConsignmentInstructionWrapper((DtbConsignmentInstruction)instruction, (DtbConsignmentInstructionPkgDivot)divot, (DtbConsignmentConfirmation)confirmation, Factory);
		}

		#endregion

		#region SecondaryHeading

		protected override ZString GetSecondaryHeading()
		{
			return ParentWrapper != null ? Res.GetString("3220129b-e71c-4c38-908f-743371b6b66c", "Booking ID") : "";
		}

		#endregion

		#region Pickup / Delivery From /To

		#region Delivery

		protected override ZDateTime GetDeliveryFrom()
		{
			return DeliveryInstructionBO != null ? DeliveryInstructionBO.ReqFrom : ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryRequiredBy()
		{
			return DeliveryInstructionBO != null ? DeliveryInstructionBO.ReqTo : ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryActual()
		{
			var confirmation = DeliveryInstructionBO != null ? DeliveryInstructionBO.DeliveryConfirmation : null;
			var rsi = confirmation != null ? confirmation.RunSheetInstruction : null;
			return rsi != null ? rsi.K1_TimeOut.ToLocalZDateTime() : ZDateTime.Empty;
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return DeliveryInstructionBO != null && !DeliveryInstructionBO.Address.IsEmpty ? new AddressWrapper(DeliveryInstructionBO.Address, Factory) : null;
		}

		protected override ZString GetCustomerReference()
		{
			return DeliveryInstructionBO != null && DeliveryInstructionBO.Confirmations.Count > 0 ? DeliveryInstructionBO.Confirmations[0].KK_ReferenceNum : ZString.Empty;
		}

		#endregion

		#region Pickup

		protected override ZDateTime GetPickupFrom()
		{
			return PickupInstructionBO != null ? PickupInstructionBO.ReqFrom : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupRequiredBy()
		{
			return PickupInstructionBO != null ? PickupInstructionBO.ReqTo : ZDateTime.Empty;
		}

		protected override ZDateTime GetPickupActual()
		{
			var confirmation = PickupInstructionBO != null ? PickupInstructionBO.PickupConfirmation : null;
			var rsi = confirmation != null ? confirmation.RunSheetInstruction : null;
			return rsi != null ? rsi.K1_TimeOut.ToLocalZDateTime() : ZDateTime.Empty;
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return PickupInstructionBO != null && !PickupInstructionBO.Address.IsEmpty ? new AddressWrapper(PickupInstructionBO.Address, Factory) : null;
		}

		protected override ZString GetOwnerReference()
		{
			return PickupInstructionBO != null && PickupInstructionBO.Confirmations.Count > 0 ? PickupInstructionBO.Confirmations[0].KK_ReferenceNum : ZString.Empty;
		}

		#endregion

		#region GetConsignor

		protected override OrganisationWrapper GetConsignor()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignor,
				PickupInstructionBO != null ? PickupInstructionBO.Address : Factory.GetNull<JobDocAddress>(), Factory);
		}

		#endregion

		#region GetConsignee

		protected override OrganisationWrapper GetConsignee()
		{
			return new OrganisationWrapper(OrganisationUsageType.Consignee,
				DeliveryInstructionBO != null ? DeliveryInstructionBO.Address : Factory.GetNull<JobDocAddress>(), Factory);
		}

		#endregion

		#endregion

		#region ServiceLevel

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(Transport.KM_RS_NKServiceLevel, Transport.Lookups.ServiceLevels, Factory);
		}

		#endregion

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(Transport, Factory);
		}

		protected override ZString GetConNote()
		{
			return Transport.KM_TransportReference;
		}

		protected override ZBool GetHazardous()
		{
			return dtbConsignment.KM_IsHazardous;
		}

		protected override ZBool GetRefrigerated()
		{
			return dtbConsignment.KM_RequiresRefrigeration;
		}

		protected override ServiceWrapperCollection GetServices()
		{
			return new ServiceWrapperCollection(dtbConsignment.Services, Factory);
		}

		#region ParentWrapper

		protected override FreightWrapper GetParentWrapperCore()
		{
			FreightWrapper parentWrapper = null;

			if (Transport != null)
			{
				var consolidation = Transport.ConsolidationSingleJob;
				if (consolidation != null)
				{
					var parent = consolidation.Parent;
					if (parent != null)
					{
						var wrappers = FreightWrapper.New(parent, Factory);
						parentWrapper = wrappers.Length > 0 ? wrappers[0] : null;
					}
				}
			}
			return parentWrapper;
		}

		#endregion

		#region PickupInstructionBO

		DtbConsignmentInstruction PickupInstructionBO
		{
			get { return pickupInstructionBO ?? (pickupInstructionBO = GetPickUpInstruction()); }
		}

		DtbConsignmentInstruction GetPickUpInstruction()
		{
			return Transport.IsNull ? Factory.GetNull<DtbConsignmentInstruction>() : Transport.PickupInstruction;
		}

		DtbConsignmentInstruction pickupInstructionBO;

		#endregion

		#region DeliveryInstructionBO

		DtbConsignmentInstruction DeliveryInstructionBO
		{
			get { return deliveryInstructionBO ?? (deliveryInstructionBO = GetDeliveryInstruction()); }
		}

		DtbConsignmentInstruction GetDeliveryInstruction()
		{
			return Transport.IsNull ? Factory.GetNull<DtbConsignmentInstruction>() : Transport.DeliveryInstruction;
		}

		DtbConsignmentInstruction deliveryInstructionBO;

		#endregion

		#region TextForBarcode

		protected override ZString TextForBarcode
		{
			get { return Transport.KM_TransportReference; }
		}

		#endregion

		#region PackagesDetails

		protected override ZString GetPackagesDetails()
		{
			var detailBuilder = new ZStringBuilder();

			var outerPackages = dtbConsignment.PackageJob.Packages.Where(p => p.KP_KP_ParentPackage == ZGuid.Empty)
				.GroupBy(p => new { PkgCode = p.KP_F3_NKPackType, WeightUnit = p.KP_WeightUQ, VolumeUnit = p.KP_VolumeUQ })
				.Select(p => new
				{
					Key = p.Key,
					Count = p.Sum(package => package.KP_PackageQty),
					Weight = p.Sum(package => package.KP_Weight),
					Volume = p.Sum(package => package.KP_Volume)
				});

			foreach (var item in outerPackages)
			{
				var detail = new ZStringBuilder();
				detail.Append(Res.GetString("c6da7959-7e3b-4183-a985-118a4bd52e9d", "{0}x {1}", item.Count, item.Key.PkgCode));
				detail.Append(Res.GetString("fdb877d1-7ba1-445d-b5a3-c3d56a85bbe6", "Wgt: {0} {1}", item.Weight.ToString("N3", ObjectCache.CultureProvider.Culture.NumberFormat), item.Key.WeightUnit));
				detail.Append(Res.GetString("8f004756-ef73-4ed0-9966-c59b897d6085", "Vol: {0} {1}", item.Volume.ToString("N3", ObjectCache.CultureProvider.Culture.NumberFormat), item.Key.VolumeUnit));
				detailBuilder.Append(detail.ToStringWithDelimiterBetweenAppends(", "));
			}

			return detailBuilder.ToStringWithNewLineBetweenAppends();
		}

		#endregion
	}
}
