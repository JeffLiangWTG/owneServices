using System;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromHVLVConsignment : FreightWrapper
	{
		public FreightWrapperFromHVLVConsignment(HVLVConsignment consignment, BusinessObjectFactory factory)
			: this(consignment, null, factory)
		{
		}

		public FreightWrapperFromHVLVConsignment(HVLVConsignment consignment, HVLVItem item, BusinessObjectFactory factory)
			: base(item, factory)
		{
			Argument.NotNull(factory, "factory");

			if (consignment != null)
			{
				if (item != null && item.HVI_HVC_Consignment != consignment.PK)
				{
					throw new ArgumentException("The HVLV Item doesn't belong to the Consignment");
				}

				ConsignmentBO = consignment;
			}
			else
			{
				ConsignmentBO = factory.GetNull<HVLVConsignment>();
			}

			ItemBO = item ?? factory.GetNull<HVLVItem>();
		}

		readonly HVLVItem ItemBO;

		readonly HVLVConsignment ConsignmentBO;

		#region HVLVItemDocWrapperCopyInfo

		protected override DocWrapperCopyInfo AdditionalCopyInfo =>
			ItemBO.IsNull
			? base.AdditionalCopyInfo
			: new HVLVItemDocWrapperCopyInfo(ConsignmentBO, ItemBO);

		class HVLVItemDocWrapperCopyInfo : DocWrapperCopyInfo
		{
			public HVLVItemDocWrapperCopyInfo(HVLVConsignment consignment, HVLVItem item)
				: base()
			{
				Name = ResString.GetMultilingualString("21c19082-d668-431c-91be-02f02a409c79", "HVLV Item ({0}/{1})", consignment.HVC_ConsignmentId, item.HVI_ItemId);
			}
		}

		#endregion

		#region Overrides

		protected override ForwardingShipment GetShipment()
		{
			return ConsignmentBO.ConsignmentHeader?.Shipment;
		}

		protected override ZBool GetIsPODRequired()
		{
			return ConsignmentBO.HVC_IsSignatureRequired;
		}

		protected override ZString GetJobNumberHeading()
		{
			return Res.GetString("f06d17ca-df49-41cc-a991-c05b3d2ae4b2", "Consignment");
		}

		protected override ZString GetJobNumber()
		{
			return ConsignmentBO.HVC_ConsignmentId;
		}

		protected override ZDateTime GetShipmentDateCreated()
		{
			var forwardingShipment = GetShipment();
			return forwardingShipment != null ? forwardingShipment.Logs.CreatedDateUtc : ZDateTime.Empty;
		}

		protected override ZInt GetDocumentTotal()
		{
			return ConsignmentBO.Items?.Count ?? ZInt.Zero;
		}

		//TODO: this numbering logic will be replaced after HVI_DisplayOrder is added in schema, and this comment line will be gone too
		protected override ZInt GetDocumentNumber()
		{
			var result = 0;
			var index = 1;
			var items = ConsignmentBO.Items?.OfType<HVLVItem>()?.OrderBy(x => x.HVI_ItemId)?.Select(x => new { x.PK, Index = index++ })?.ToArray();

			if (items != null && items.Any(x => x.PK == ItemBO.PK))
			{
				result = items.Single(x => x.PK == ItemBO.PK).Index;
			}

			return result;
		}

		protected override HVLVConsignment GetHVLVConsignment()
		{
			return ConsignmentBO;
		}

		protected override HVLVItem GetHVLVItem()
		{
			return ItemBO;
		}

		protected override OrganisationWrapper GetCarrier()
		{
			return new LocalTransportOrganisationWrapper(OrganisationUsageType.Carrier, ConsignmentBO.LocalTransportCompanyLabel, Factory);
		}

		protected override OrganisationWrapper GetConsignor()
		{
			var consignorAddress = GetConsignorJobDocAddress();
			var organisationWrapper = new OrganisationWrapper(OrganisationUsageType.Consignor, consignorAddress, Factory);

			return organisationWrapper;
		}

		protected override OrganisationWrapper GetConsignee()
		{
			var consigneeAddress = GetConsigneeJobDocAddress();
			var organisationWrapper = new OrganisationWrapper(OrganisationUsageType.Consignee, consigneeAddress, Factory);

			return organisationWrapper;
		}

		protected override VolumeWrapper GetVolume()
		{
			var volume = ConsignmentBO.HVC_ActualVolume <= 0 ? ConsignmentBO.HVC_ManifestedVolume : ConsignmentBO.HVC_ActualVolume;
			return new VolumeWrapper(
				volume,
				ConsignmentBO.HVC_VolumeUQ,
				HVLVConsignmentSchema.HVC_ActualVolume.Scale,
				Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume),
				Factory);
		}

		protected override MoneyWrapper GetGoodsValue()
		{
			return new MoneyWrapper(new Money(ConsignmentBO.HVC_GoodsValue, ConsignmentBO.GoodsValueCurrency), Factory);
		}

		protected override CodeAndDescriptionWrapper GetServiceLevel()
		{
			return new CodeAndDescriptionWrapper(ConsignmentBO.BookingHeader?.HVH_RS_NKBookingServiceLevel ?? ZString.Empty, ConsignmentBO.BookingHeader?.Lookups?.BookingServiceLevels, Factory);
		}

		protected override CarrierServiceLevelWrapper GetCarrierServiceLevel()
		{
			return new CarrierServiceLevelWrapper(ConsignmentBO.HVC_PL_NKLastMileCarrierServiceLevel, ConsignmentBO.Lookups?.LastMileCarrierServiceLevels, Factory);
		}

		protected override OrganisationWrapper GetDeliveryAgent()
		{
			return new OrganisationWrapper(OrganisationUsageType.DeliveryAgent, ConsignmentBO.LastMileCarrier, ContactType.All, Factory);
		}

		protected override ZString GetGoodsDescription()
		{
			return ConsignmentBO.HVC_GoodsDescription;
		}

		protected override PlaceAndDateWrapper GetOrigin()
		{
			ZString unloco = GetShipment()?.JS_RL_NKOrigin ??
				ConsignmentBO.BookingHeader?.OriginDepot?.OA_RL_NKRelatedPortCode ??
				ZString.Empty;

			return new PlaceAndDateWrapper(unloco, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		protected override PlaceAndDateWrapper GetDestination()
		{
			ZString unloco = GetShipment()?.JS_RL_NKDestination ??
				ConsignmentBO.DestinationDepot?.OA_RL_NKRelatedPortCode ??
				ZString.Empty;

			return new PlaceAndDateWrapper(unloco, ZDateTime.Empty, ZDateTime.Empty, Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			var weight = ConsignmentBO.HVC_ActualWeight <= 0 ? ConsignmentBO.HVC_ManifestedWeight : ConsignmentBO.HVC_ActualWeight;
			return new WeightWrapper(
				weight,
				ConsignmentBO.HVC_WeightUQ,
				HVLVConsignmentSchema.HVC_ActualWeight.Scale,
				Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight),
				Factory);
		}

		protected override PackageWrapperCollection GetPackages()
		{
			var result = new PackageWrapperCollection(Factory);

			var items = !ItemBO.IsNull
				? new[] { ItemBO }
				: ConsignmentBO.Items.Cast<HVLVItem>();

			foreach (var item in items)
			{
				var itemWrapper = new PackageWrapperFromHVLVItem(item, Factory);
				result.Add(itemWrapper);
			}

			return result;
		}

		JobDocAddress GetConsigneeJobDocAddress()
		{
			JobDocAddress result = null;
			if (ConsignmentBO != null)
			{
				result = Factory.New<JobDocAddress>();
				result.MakeNonPersistent();
				result.E2_AddressOverride = true;
				result.E2_CompanyName = ConsignmentBO.HVC_ConsigneeName.SubstringSafe(0, result.E2_CompanyNameInfo.MaxLength);
				result.E2_Address1 = ConsignmentBO.HVC_ConsigneeAddress1.SubstringSafe(0, result.E2_Address1Info.MaxLength);
				result.E2_Address2 = ConsignmentBO.HVC_ConsigneeAddress2.SubstringSafe(0, result.E2_Address2Info.MaxLength);
				result.E2_City = ConsignmentBO.HVC_ConsigneeCity;
				result.E2_State = ConsignmentBO.HVC_ConsigneeState.SubstringSafe(0, result.E2_StateInfo.MaxLength);
				result.E2_Postcode = ConsignmentBO.HVC_ConsigneePostcode.SubstringSafe(0, result.E2_PostcodeInfo.MaxLength);
				result.E2_RN_NKCountryCode = ConsignmentBO.HVC_RN_NKConsigneeCountryCode;
				result.E2_Contact = ConsignmentBO.HVC_ConsigneeContact;
				result.E2_Email = ConsignmentBO.HVC_ConsigneeEmail;
				result.E2_Phone = ConsignmentBO.HVC_ConsigneePhone;
				result.E2_Mobile = ConsignmentBO.HVC_ConsigneeMobile;
				result.E2_Fax = ConsignmentBO.HVC_ConsigneeFax;
			}

			return result;
		}

		JobDocAddress GetConsignorJobDocAddress()
		{
			JobDocAddress result = null;
			if (ConsignmentBO != null)
			{
				result = Factory.New<JobDocAddress>();
				result.MakeNonPersistent();
				result.E2_CompanyName = ConsignmentBO.HVC_ShipperName.SubstringSafe(0, result.E2_CompanyNameInfo.MaxLength);
				result.E2_Address1 = ConsignmentBO.HVC_ShipperAddress1.SubstringSafe(0, result.E2_Address1Info.MaxLength);
				result.E2_Address2 = ConsignmentBO.HVC_ShipperAddress2.SubstringSafe(0, result.E2_Address2Info.MaxLength);
				result.E2_City = ConsignmentBO.HVC_ShipperCity;
				result.E2_State = ConsignmentBO.HVC_ShipperState.SubstringSafe(0, result.E2_StateInfo.MaxLength);
				result.E2_Postcode = ConsignmentBO.HVC_ShipperPostcode.SubstringSafe(0, result.E2_PostcodeInfo.MaxLength);
				result.E2_RN_NKCountryCode = ConsignmentBO.HVC_RN_NKShipperCountryCode;
				result.E2_Contact = ConsignmentBO.HVC_ShipperContact;
				result.E2_Email = ConsignmentBO.HVC_ShipperEmail;
				result.E2_Phone = ConsignmentBO.HVC_ShipperPhone;
				result.E2_Mobile = ConsignmentBO.HVC_ShipperMobile;
				result.E2_Fax = ConsignmentBO.HVC_ShipperFax;
			}

			return result;
		}

		protected override Image JobHeaderBranchLogo
		{
			get
			{
				Image result = null;
				var lmcCode = ConsignmentBO.LastMileCarrier?.OH_Code;
				if (!string.IsNullOrEmpty(lmcCode))
				{
					var branding = DocumentsDataRegistry.Instance.LocalTransportCompanyBrand.Value.FindByCode(lmcCode) as LocalTransportCompanyBranding;
					if (branding != null)
					{
						result = branding.Image;
					}
				}

				return result;
			}
		}

		protected override AddressWrapper GetDeliveryAddress()
		{
			return new AddressWrapper(GetConsigneeJobDocAddress(), Factory);
		}

		protected override AddressWrapper GetPickupAddress()
		{
			return new AddressWrapper(GetConsignorJobDocAddress(), Factory);
		}

		protected override ZString GetOrderTrackingNumber()
		{
			var result = ConsignmentBO.HVC_ShipperReference;
			if (result.IsEmpty)
			{
				result = ConsignmentBO.HVC_WaybillNumber;
			}

			if (result.IsEmpty)
			{
				result = ConsignmentBO.HVC_ConsignmentId;
			}

			return result;
		}

		protected override AddressWrapper GetUnpackCFSAddress()
		{
			var org = GetShipment()?.ConsignorDocumentaryAddress?.Organisation;

			var rtaOrg = org != null ? org.GetRelatedParty(RelatedPartyTypeList.Codes.ReturnAgent, ZString.Empty) : null;

			var address = rtaOrg != null && rtaOrg.MainAddress != null
				? rtaOrg.MainAddress
				: ConsignmentBO.DestinationDepot;

			return new AddressWrapper(OrganisationUsageType.UnpackingLocation, address, ContactType.Depot, Factory);
		}

		#endregion
	}
}
