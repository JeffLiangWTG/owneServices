using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class CMRCusSCAHouseDataObjectReader : CusSCAHouseDataObjectReader<CusSCAHouse, CusSCAPivot>
	{
		public CMRCusSCAHouseDataObjectReader(IColumnIndexer oceanBill, HVLVShipmentDataObjectWrapper hvlvConsolidatorShipmentWrapper, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(oceanBill, hvlvConsolidatorShipmentWrapper, dataObject, logger, factory)
		{
		}

		internal static bool IsMessagingActive(CusSCAHouse houseBill)
		{
			return
				houseBill != null &&
				houseBill.IsInDatabase &&
				!CMRStatusHelper.IsMessagingBeingNotLodged(houseBill.CA_MessageStatus);
		}

		#region Overrides

		protected override void PopulatePackingLineCollection(IColumnIndexer houseBill)
		{
			if (hvlvConsolidatorShipmentWrapper?.hvlvShipment != null)
			{
				var lineNo = 0;
				var packingLineGroups = dataObject.PackingLineCollection.GroupBy(p => p.ContainerNumber);

				foreach (var packingLineGroup in packingLineGroups)
				{
					lineNo++;
					var packageData = packingLineGroup.First();
					packageData.Weight = packingLineGroup.Sum(p => p.Weight);
					packageData.Volume = packingLineGroup.Sum(p => p.Volume);
					packageData.ManifestedWeight = packingLineGroup.Sum(p => p.ManifestedWeight);
					packageData.GoodsDescription = string.Join(System.Environment.NewLine, packingLineGroup.Select(p => p.GetCleanSingleLineGoodsDescription()));
					var scaHouse = GetNewCusSCAPivotDataObjectReader(lineNo, houseBill, packageData).ReadIntoBusinessObject();
					if (scaHouse != null)
					{
						scaHouse.CV_PackageCount = packingLineGroup.Count();
					}
				}
			}
			else
			{
				base.PopulatePackingLineCollection(houseBill);
			}
		}

		protected override CusSCAPivotDataObjectReader<CusSCAPivot> GetNewCusSCAPivotDataObjectReader(ZInt lineNo, IColumnIndexer houseBill, PackingLine packageDataObject)
		{
			return new CMRCusSCAPivotDataObjectReader(lineNo, houseBill, hvlvConsolidatorShipmentWrapper, dataObject, packageDataObject, logger, factory);
		}

		protected override void PopulateCountrySpecificDetails(IColumnIndexer houseBill)
		{
			if (dataObject.PaymentMethod.TryGetCodeAsUpperCase(out var paymentCode))
			{
				SetValue(houseBill, CusSCAHouseSchema.CA_PrepaidCollectOther, GetCMRPaymentCode(paymentCode));
			}
			else if (dataObject.ShipmentIncoTerm.TryGetCodeAsUpperCase(out var incoTermCode))
			{
				var prepaidCollect = IncoTermRegistry.GetPrepaidCollect(ChargeCodeGroupList.Codes.Freight, incoTermCode);
				switch (prepaidCollect)
				{
					case Enterprise.Core.Constants.PaymentType.Collect:
						SetValue(houseBill, CusSCAHouseSchema.CA_PrepaidCollectOther, CMRMethodsOfPayment.Codes.Collect);
						break;
					case Enterprise.Core.Constants.PaymentType.Prepaid:
						SetValue(houseBill, CusSCAHouseSchema.CA_PrepaidCollectOther, CMRMethodsOfPayment.Codes.PrepaidBySeller);
						break;
				}
			}

			if (houseBill.GetValue(CusSCAHouseSchema.CA_RN_NKGoodsOrigin).IsEmpty && !oceanBill.GetValue(CusSCAOceanBillSchema.CB_RL_NKPortOfLoading).IsEmpty)
			{
				SetValue(houseBill, CusSCAHouseSchema.CA_RN_NKGoodsOrigin, oceanBill.GetValue(CusSCAOceanBillSchema.CB_RL_NKPortOfLoading).SubstringSafe(0, 2));
			}

			PopulateMasterHouse(houseBill);

			SetValue(houseBill, CusSCAHouseSchema.CA_ResponsiblePartyID, dataObject.GetResponsiblePartyID(logger, factory));

			var targetBo = houseBill as CusSCAHouse;
			if (targetBo != null)
			{
				PopulateWorkflowCustomFields(targetBo, dataObject);
			}
		}

		ZString GetCMRPaymentCode(ZString paymentCode)
		{
			if (factory.GetCachedValue<CMRMethodsOfPayment>().ContainsCode(paymentCode))
			{
				return paymentCode;
			}
			else if (paymentCode == Enterprise.Core.Constants.PaymentType.Collect)
			{
				return CMRMethodsOfPayment.Codes.Collect;
			}
			else
			{
				return CMRMethodsOfPayment.Codes.PrepaidOnly;
			}
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusSCAHouse targetBO)
		{
			var builder = new ZStringBuilder(base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO));
			if (IsMessagingActive(targetBO))
			{
				builder.AppendLine("Messaging is active for this House Bill");
			}
			return builder.ToString();
		}

		protected override ZString[] GetConsigneeAddressTypesInPreferredOrder(IColumnIndexer houseBill)
		{
			return IsMasterHouse
				? new ZString[] { nameof(DocAddressType.ReceivingForwarderAddress) }
				: base.GetConsigneeAddressTypesInPreferredOrder(houseBill);
		}

		protected override ZString[] GetConsignorAddressTypesInPreferredOrder(IColumnIndexer houseBill)
		{
			return IsMasterHouse
				? new ZString[] { nameof(DocAddressType.SendingForwarderAddress) }
				: base.GetConsignorAddressTypesInPreferredOrder(houseBill);
		}

		protected override OrgAddress GetMatchedOrgAddress(OrganizationAddress orgAddressData, BusinessObjectFactory boFactory)
		{
			return orgAddressData.GetMatchingOrgAddressUsingCodeOrAddress1(boFactory);
		}

		protected override void UpdateConsignorWithMatchedOrgAddress(IColumnIndexer houseBill, OrgAddress address, ZString? contactName)
		{
			SetValue(houseBill, CusSCAHouseSchema.CA_OA_ConsignorAddress, address.PK);
		}

		protected override void UpdateConsigneeWithMatchedOrgAddress(IColumnIndexer houseBill, OrgAddress address, ZString? contactName)
		{
			SetValue(houseBill, CusSCAHouseSchema.CA_OA_ConsigneeAddress, address.PK);
		}

		protected override void AdditionalConsignorUpdate(IColumnIndexer houseBill, OrganizationAddress address, OrgAddress orgAddress)
		{
			var hvlvShipment = hvlvConsolidatorShipmentWrapper?.hvlvShipment;
			if (hvlvShipment != null)
			{
				var vendorId = dataObject.VendorIdentifier ?? ZString.Empty;
				if (vendorId.IsEmpty && address != null)
				{
					vendorId = CargoHelper.GetConsignorVendorId(address);
					if (vendorId.IsEmpty && orgAddress != null)
					{
						vendorId = CargoHelper.GetConsignorVendor(orgAddress.Header);
					}
				}

				if (vendorId.IsEmpty)
				{
					vendorId = hvlvShipment.VendorIdentifier ?? ZString.Empty;
					if (vendorId.IsEmpty)
					{
						var hvlvConsignorAddressData = hvlvShipment.OrganizationAddressCollection?.FirstOrDefault(GetConsignorAddressTypesInPreferredOrder(houseBill));
						if (hvlvConsignorAddressData != null)
						{
							vendorId = CargoHelper.GetConsignorVendorId(hvlvConsignorAddressData);
							if (vendorId.IsEmpty)
							{
								var hvlvConsignorOrgAddress = GetOrgAddressBO(hvlvConsignorAddressData, logger.TopLevelDataContext);
								if (hvlvConsignorOrgAddress != null)
								{
									vendorId = CargoHelper.GetConsignorVendor(hvlvConsignorOrgAddress.Header);
								}
							}
						}
					}
				}

				SetValue(houseBill, CusSCAHouseSchema.CA_VendorIdentifier, vendorId);
			}
		}

		#endregion // Overrides

		#region Implementation

		void PopulateMasterHouse(IColumnIndexer houseBill)
		{
			var additionalBill = dataObject.AdditionalBillCollection.GetAdditionalBill(
				dataObject.WayBillNumber.GetValueOrDefault(), dataObject.WayBillType.GetCodeAsUpperCase());
			SetValue(houseBill, CusSCAHouseSchema.CA_MasterHouseBill, additionalBill == null ? ZString.Empty : additionalBill.ParentBillNumber);
			SetValue(houseBill, CusSCAHouseSchema.CA_IsMasterHouse, IsMasterHouse);
		}

		bool IsMasterHouse => dataObject.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.MasterHouse;

		#endregion // Implementation
	}
}
