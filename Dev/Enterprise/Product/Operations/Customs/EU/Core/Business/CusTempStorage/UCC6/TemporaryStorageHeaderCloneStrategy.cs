using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageHeaderCloneStrategy : CustomsBusinessObjectCloneStrategy
	{
		public TemporaryStorageHeaderCloneStrategy(BusinessObject bizObjToClone) : base(bizObjToClone, CloneType.DeepTemplateCopy)
		{
		}

		public override BusinessObject Clone()
		{
			var args = GetCloneArgs(bizObjToClone, alternativeFactoryToInstantiateCloneIn);
			var result = Clone(args);
			return result;
		}

		BusinessObjectCloneArgs GetCloneArgs(BusinessObject businessObjectToClone, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		{
			var result = GetCloneArgs(businessObjectToClone);
			if (result != null && alternativeFactoryToInstantiateCloneIn != null)
			{
				result = new BusinessObjectCloneArgs(alternativeFactoryToInstantiateCloneIn, result.GetExcludedColumns(), result.TypeToCloneAs, result.PerformRowCopyWithoutTriggeringValidationAndSetter);
			}

			return result;
		}

		protected virtual BusinessObjectCloneArgs GetCloneArgs(BusinessObject businessObjectToClone)
		{
			switch (businessObjectToClone)
			{
				case TemporaryStorageHeader:
					return TemporaryStorageHeaderCloneArgs;
				case TemporaryStorageBill:
					return TemporaryStorageBillCloneArgs;
				case TemporaryStoragePack:
					return TemporaryStoragePackCloneArgs;
				case TemporaryStoragePackedItem:
					return TemporaryStoragePackedItemCloneArgs;
				case TemporaryStorageSupportingDocument:
					return TemporaryStorageSupportingDocumentCloneArgs;
				case TemporaryStoragePreviousDocument:
					return TemporaryStoragePreviousDocumentCloneArgs;
				case TemporaryStorageAdditionalInfo:
					return TemporaryStorageAdditionalInfoCloneArgs;
				case CusSupplyChainActorReference:
					return CusSupplyChainActorReferenceCloneArgs;
				default:
					return null;
			}
		}

		#region TemporaryStorageHeader

		BusinessObjectCloneArgs TemporaryStorageHeaderCloneArgs => TemporaryStorageHeaderCloneArgsCore;

		protected virtual BusinessObjectCloneArgs TemporaryStorageHeaderCloneArgsCore => new BusinessObjectCloneArgs(AsycudaManifestHeaderSchema.All.Except(new SchemaColumn[]
				{
					AsycudaManifestHeaderSchema.AMA_OA_Carrier,
					AsycudaManifestHeaderSchema.AMA_TransportMode,
					AsycudaManifestHeaderSchema.AMA_ApplicationCode,
					AsycudaManifestHeaderSchema.AMA_ContainerMode,
					AsycudaManifestHeaderSchema.AMA_AgentType,
					AsycudaManifestHeaderSchema.AMA_OA_DischargeTerminalAddress,
					AsycudaManifestHeaderSchema.AMA_OA_ShippingAgent,
					AsycudaManifestHeaderSchema.AMA_Nature,
					AsycudaManifestHeaderSchema.AMA_ManifestType,
					AsycudaManifestHeaderSchema.AMA_RL_NKPortOfFirstArrival,
					AsycudaManifestHeaderSchema.AMA_CustomsOffice,
					AsycudaManifestHeaderSchema.AMA_CarrierCode,
					AsycudaManifestHeaderSchema.AMA_RL_NKPortOfFinalDeparture,
					AsycudaManifestHeaderSchema.AMA_OA_Declarant,
					AsycudaManifestHeaderSchema.AMA_OA_Presenter,
					AsycudaManifestHeaderSchema.AMA_OA_Representative,
				}.Cast<SchemaColumn>()).Select(x => x.Name), true);

		#endregion

		#region TemporaryStorageBill

		BusinessObjectCloneArgs TemporaryStorageBillCloneArgs => TemporaryStorageBillCloneArgsCore;

		protected virtual BusinessObjectCloneArgs TemporaryStorageBillCloneArgsCore => new BusinessObjectCloneArgs(AsycudaBillSchema.All.Except(new SchemaColumn[]
				{
					AsycudaBillSchema.ABL_BolType,
					AsycudaBillSchema.ABL_RL_NKOrigin,
					AsycudaBillSchema.ABL_RL_NKFinalDestination,
					AsycudaBillSchema.ABL_ConsigneePhone,
					AsycudaBillSchema.ABL_ConsigneePostcode,
					AsycudaBillSchema.ABL_NotifyPartyPhone,
					AsycudaBillSchema.ABL_NotifyPartyPostcode,
					AsycudaBillSchema.ABL_OA_Consignee,
					AsycudaBillSchema.ABL_OA_NotifyParty,
					AsycudaBillSchema.ABL_OA_Shipper,
					AsycudaBillSchema.ABL_RN_NKConsigneeCountry,
					AsycudaBillSchema.ABL_RN_NKNotifyPartyCountry,
					AsycudaBillSchema.ABL_RN_NKShipperCountry,
					AsycudaBillSchema.ABL_ShipperPostcode,
					AsycudaBillSchema.ABL_RL_NKPortOfDischarge,
					AsycudaBillSchema.ABL_RL_NKPortOfLoading,
					AsycudaBillSchema.ABL_ConsigneeName,
					AsycudaBillSchema.ABL_ShipperName,
					AsycudaBillSchema.ABL_NotifyPartyName,
					AsycudaBillSchema.ABL_ShipmentType,
					AsycudaBillSchema.ABL_ShipperRegNo,
					AsycudaBillSchema.ABL_ConsigneeRegNo,
					AsycudaBillSchema.ABL_NotifyPartyRegNo,
					AsycudaBillSchema.ABL_ConsigneeRegNoType,
					AsycudaBillSchema.ABL_NotifyPartyRegNoType,
					AsycudaBillSchema.ABL_ShipperRegNoType,
					AsycudaBillSchema.ABL_ShipperPhone,
					AsycudaBillSchema.ABL_ConsigneeStreet1,
					AsycudaBillSchema.ABL_ConsigneeStreet2,
					AsycudaBillSchema.ABL_ConsigneeCity,
					AsycudaBillSchema.ABL_ConsigneeState,
					AsycudaBillSchema.ABL_ShipperStreet1,
					AsycudaBillSchema.ABL_ShipperStreet2,
					AsycudaBillSchema.ABL_ShipperCity,
					AsycudaBillSchema.ABL_ShipperState,
					AsycudaBillSchema.ABL_NotifyPartyStreet1,
					AsycudaBillSchema.ABL_NotifyPartyStreet2,
					AsycudaBillSchema.ABL_NotifyPartyCity,
					AsycudaBillSchema.ABL_NotifyPartyState,
					AsycudaBillSchema.ABL_ConsigneeLocalCity,
					AsycudaBillSchema.ABL_ConsigneeLocalName,
					AsycudaBillSchema.ABL_ConsigneeLocalState,
					AsycudaBillSchema.ABL_ConsigneeLocalStreet1,
					AsycudaBillSchema.ABL_ConsigneeLocalStreet2,
					AsycudaBillSchema.ABL_NotifyPartyLocalCity,
					AsycudaBillSchema.ABL_NotifyPartyLocalName,
					AsycudaBillSchema.ABL_NotifyPartyLocalState,
					AsycudaBillSchema.ABL_NotifyPartyLocalStreet1,
					AsycudaBillSchema.ABL_NotifyPartyLocalStreet2,
					AsycudaBillSchema.ABL_ShipperLocalCity,
					AsycudaBillSchema.ABL_ShipperLocalName,
					AsycudaBillSchema.ABL_ShipperLocalState,
					AsycudaBillSchema.ABL_ShipperLocalStreet1,
					AsycudaBillSchema.ABL_ShipperLocalStreet2,
				}.Cast<SchemaColumn>()).Select(x => x.Name), true);

		#endregion

		#region TemporaryStoragePack

		BusinessObjectCloneArgs TemporaryStoragePackCloneArgs => TemporaryStoragePackCloneArgsCore;

		protected virtual BusinessObjectCloneArgs TemporaryStoragePackCloneArgsCore => new BusinessObjectCloneArgs(AsycudaPackSchema.All.Except(new SchemaColumn[]
				{
					AsycudaPackSchema.APA_PackQty,
					AsycudaPackSchema.APA_PackUQ,
					AsycudaPackSchema.APA_GoodsDescription,
					AsycudaPackSchema.APA_CommodityCode,
				}.Cast<SchemaColumn>()).Select(x => x.Name), true);

		#endregion

		#region TemporaryStoragePackedItem

		BusinessObjectCloneArgs TemporaryStoragePackedItemCloneArgs => TemporaryStoragePackedItemCloneArgsCore;

		protected virtual BusinessObjectCloneArgs TemporaryStoragePackedItemCloneArgsCore => new BusinessObjectCloneArgs(AsycudaPackedItemSchema.All.Except(new SchemaColumn[]
				{
					AsycudaPackedItemSchema.API_Tariff,
					AsycudaPackedItemSchema.API_GoodsDescription,
					AsycudaPackedItemSchema.API_ChemicalSubstanceCode,
				}.Cast<SchemaColumn>()).Select(x => x.Name), true);

		#endregion

		#region TemporaryStorageSupportingDocument

		BusinessObjectCloneArgs TemporaryStorageSupportingDocumentCloneArgs => TemporaryStorageSupportingDocumentCloneArgsCore;

		protected virtual BusinessObjectCloneArgs TemporaryStorageSupportingDocumentCloneArgsCore => new BusinessObjectCloneArgs(CusSupportingInfoSchema.All.Except(new SchemaColumn[]
				{
					CusSupportingInfoSchema.CSI_Code,
					CusSupportingInfoSchema.CSI_ReferenceNumber
				}.Cast<SchemaColumn>()).Select(x => x.Name), true);

		#endregion

		#region TemporaryStoragePreviousDocument

		BusinessObjectCloneArgs TemporaryStoragePreviousDocumentCloneArgs => TemporaryStoragePreviousDocumentCloneArgsCore;

		protected virtual BusinessObjectCloneArgs TemporaryStoragePreviousDocumentCloneArgsCore => new BusinessObjectCloneArgs(CusSupportingInfoSchema.All.Except(new SchemaColumn[]
				{
					CusSupportingInfoSchema.CSI_Code,
					CusSupportingInfoSchema.CSI_LineNo
				}.Cast<SchemaColumn>()).Select(x => x.Name), true);

		#endregion

		#region TemporaryStorageAdditionalInfo

		BusinessObjectCloneArgs TemporaryStorageAdditionalInfoCloneArgs => TemporaryStorageAdditionalInfoCloneArgsCore;

		protected virtual BusinessObjectCloneArgs TemporaryStorageAdditionalInfoCloneArgsCore => new BusinessObjectCloneArgs(CusSupportingInfoSchema.All.Except(new SchemaColumn[]
				{
					CusSupportingInfoSchema.CSI_Code,
					CusSupportingInfoSchema.CSI_ReferenceNumber,
					CusSupportingInfoSchema.CSI_SubType,
					CusSupportingInfoSchema.CSI_Description
				}.Cast<SchemaColumn>()).Select(x => x.Name), true);

		#endregion

		#region CusSupplyChainActorReference

		BusinessObjectCloneArgs CusSupplyChainActorReferenceCloneArgs => CusSupplyChainActorReferenceCloneArgsCore;

		protected virtual BusinessObjectCloneArgs CusSupplyChainActorReferenceCloneArgsCore => new BusinessObjectCloneArgs(CusReferenceSchema.All.Except(new SchemaColumn[]
				{
					CusReferenceSchema.CFR_Code,
					CusReferenceSchema.CFR_Reference,
					CusReferenceSchema.CFR_OA_Owner,
				}.Cast<SchemaColumn>()).Select(x => x.Name), true);

		#endregion

	}
}
