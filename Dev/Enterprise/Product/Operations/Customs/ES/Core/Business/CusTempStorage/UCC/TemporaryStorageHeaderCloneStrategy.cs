using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStorageHeaderCloneStrategy : EU.Business.CusTempStorage.TemporaryStorageHeaderCloneStrategy
{
	public TemporaryStorageHeaderCloneStrategy(BusinessObject bizObjToClone) : base(bizObjToClone)
	{
	}

	protected override BusinessObjectCloneArgs GetCloneArgs(BusinessObject businessObjectToClone)
	{
		switch (businessObjectToClone)
		{
			case EU.Business.CusTempStorage.TemporaryStorageHeaderGuarantee:
				return TemporaryStorageHeaderGuaranteeCloneArgs;
			default:
				return base.GetCloneArgs(businessObjectToClone);
		}
	}

	#region TemporaryStorageHeader

	protected override BusinessObjectCloneArgs TemporaryStorageHeaderCloneArgsCore => new BusinessObjectCloneArgs(AsycudaManifestHeaderSchema.All.Except(new SchemaColumn[]
				{
					AsycudaManifestHeaderSchema.AMA_MessageType,
					AsycudaManifestHeaderSchema.AMA_TransportMode,
					AsycudaManifestHeaderSchema.AMA_CustomsOffice,
					AsycudaManifestHeaderSchema.AMA_OA_Declarant,
					AsycudaManifestHeaderSchema.AMA_OA_Representative,
				}.Cast<SchemaColumn>()).Select(x => x.Name), true);
	#endregion

	#region TemporaryStorageHeaderGuarantee

	BusinessObjectCloneArgs TemporaryStorageHeaderGuaranteeCloneArgs => new BusinessObjectCloneArgs(CusBondDetailSchema.All.Except(new SchemaColumn[]
			{
					CusBondDetailSchema.PW_BondNumber,
					CusBondDetailSchema.PW_Override,
					CusBondDetailSchema.PW_BondAmount,
			}.Cast<SchemaColumn>()).Select(x => x.Name), true);
	#endregion

	#region TemporaryStorageBill

	protected override BusinessObjectCloneArgs TemporaryStorageBillCloneArgsCore => new BusinessObjectCloneArgs(AsycudaBillSchema.All.Except(new SchemaColumn[]
			{
				AsycudaBillSchema.ABL_BolType,
				AsycudaBillSchema.ABL_BillNumber,
				AsycudaBillSchema.ABL_OA_Consignee,
				AsycudaBillSchema.ABL_ConsigneePhone,
				AsycudaBillSchema.ABL_ConsigneePostcode,
				AsycudaBillSchema.ABL_RN_NKConsigneeCountry,
				AsycudaBillSchema.ABL_ConsigneeName,
				AsycudaBillSchema.ABL_ConsigneeRegNo,
				AsycudaBillSchema.ABL_ConsigneeRegNoType,
				AsycudaBillSchema.ABL_ConsigneeStreet1,
				AsycudaBillSchema.ABL_ConsigneeStreet2,
				AsycudaBillSchema.ABL_ConsigneeCity,
				AsycudaBillSchema.ABL_ConsigneeState,
				AsycudaBillSchema.ABL_OA_Shipper,
				AsycudaBillSchema.ABL_RN_NKShipperCountry,
				AsycudaBillSchema.ABL_ShipperPostcode,
				AsycudaBillSchema.ABL_ShipperName,
				AsycudaBillSchema.ABL_ShipperRegNo,
				AsycudaBillSchema.ABL_ShipperRegNoType,
				AsycudaBillSchema.ABL_ShipperPhone,
				AsycudaBillSchema.ABL_ShipperStreet1,
				AsycudaBillSchema.ABL_ShipperStreet2,
				AsycudaBillSchema.ABL_ShipperCity,
				AsycudaBillSchema.ABL_ShipperState,
			}.Cast<SchemaColumn>()).Select(x => x.Name), true);

	#endregion

	#region TemporaryStoragePackedItem

	protected override BusinessObjectCloneArgs TemporaryStoragePackedItemCloneArgsCore => new BusinessObjectCloneArgs(AsycudaPackedItemSchema.All.Except(new SchemaColumn[]
			{
					AsycudaPackedItemSchema.API_Tariff,
					AsycudaPackedItemSchema.API_GoodsDescription,
					AsycudaPackedItemSchema.API_ChemicalSubstanceCode,
					AsycudaPackedItemSchema.API_GrossWeight,
					AsycudaPackedItemSchema.API_GrossWeightUQ,
					AsycudaPackedItemSchema.API_PackStatus,
					AsycudaPackedItemSchema.API_RN_NKGoodsOrigin,
					AsycudaPackedItemSchema.API_GoodsValue,
					AsycudaPackedItemSchema.API_RX_NKGoodsValueCurrency,
					AsycudaPackedItemSchema.API_CustomsQty,
					AsycudaPackedItemSchema.API_CustomsUQ,
					AsycudaPackedItemSchema.API_CustomsQty2,
					AsycudaPackedItemSchema.API_CustomsUQ2,
					AsycudaPackedItemSchema.API_CustomsQty3,
					AsycudaPackedItemSchema.API_CustomsUQ3
			}.Cast<SchemaColumn>()).Select(x => x.Name), true);

	#endregion

	#region TemporaryStoragePreviousDocument

	protected override BusinessObjectCloneArgs TemporaryStoragePreviousDocumentCloneArgsCore => new BusinessObjectCloneArgs(CusSupportingInfoSchema.All.Except(new SchemaColumn[]
			{
				CusSupportingInfoSchema.CSI_Code,
				CusSupportingInfoSchema.CSI_LineNo,
				CusSupportingInfoSchema.CSI_ReferenceNumber,
				CusSupportingInfoSchema.CSI_ReferenceNumber2
			}.Cast<SchemaColumn>()).Select(x => x.Name), true);

	#endregion
}
