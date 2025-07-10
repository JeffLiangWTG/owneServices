using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IL.Manifest.Business.Constants;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class DeclarationConsignmentItemCommodityCommodityRelatedPackagingWrapper : IDeclarationConsignmentConsignmentItemCommodityCommodityRelatedPackaging
	{
		public DeclarationConsignmentItemCommodityCommodityRelatedPackagingWrapper(UNDGSubstance substanceUNDG)
		{
			this.substanceUNDG = substanceUNDG;
		}

		internal static IDeclarationConsignmentConsignmentItemCommodityCommodityRelatedPackaging NewOrNull(UNDGDataItem dataItemUNDG)
			=> dataItemUNDG?.UNDGSubstance == null
			? null
			: new DeclarationConsignmentItemCommodityCommodityRelatedPackagingWrapper(dataItemUNDG.UNDGSubstance);

		ICodeType IDeclarationConsignmentConsignmentItemCommodityCommodityRelatedPackaging.DangerousGoodsPackingRequirementGroupCode
			=> CodeTypeWrapper.NewOrNull(GetDangerousGoodsPackingRequirementGroupCode());

		ZString GetDangerousGoodsPackingRequirementGroupCode()
			=> substanceUNDG.DG_PG.ToString() switch
			{
				UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode => IsraeliCustoms.DangerousGoodsPackingHighDangerCode,
				UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode => IsraeliCustoms.DangerousGoodsPackingMediumDangerCode,
				UNDGSubstanceLookups.PackingGroupTypes.LowDangerCode => IsraeliCustoms.DangerousGoodsPackingLowDangerCode,
				_ => ZString.Empty
			};

		readonly UNDGSubstance substanceUNDG;
	}
}
