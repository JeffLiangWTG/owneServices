using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Manifest.Business
{
	internal class PackWrapper : IPack
	{
		internal PackWrapper(AsycudaPack pack)
		{
			this.pack = Argument.NotNull(pack, "AsycudaPack cannot be null");
			container = pack.Container;
			loadType = MercanteHelper.GetLoadType(pack.Bill.Header.AMA_ContainerMode);
			isContainerised = loadType == MercanteConstants.LoadType.Containerised;
			isBulk = loadType == MercanteConstants.LoadType.Bulk;
			isBreakBulk = loadType == MercanteConstants.LoadType.BreakBulk;
			dangerousGoodsCode = pack.UNDGs?.FirstOrDefault()?.DI_DG_NKSubs ?? ZString.Empty;
		}
		readonly AsycudaPack pack;
		readonly AsycudaContainer container;
		readonly ZInt loadType;
		readonly ZBool isContainerised;
		readonly ZBool isBulk;
		readonly ZBool isBreakBulk;
		readonly ZString dangerousGoodsCode;

		int IPack.CargoItemType => loadType;

		int IPack.ItemSequentialNumber => pack.APA_LineNo;

		decimal IPack.GrossWeight => ((ZDecimal)Core.Constants.Weight.ConvertSafe(pack.APA_Weight, pack.APA_WeightUQ, Core.Constants.Weight.Kilograms)).Truncate(3);

		string IPack.ContainerType => isContainerised ? container?.ContainerType?.RC_Code ?? ZString.Empty : ZString.Empty;

		string IPack.ContainerNumber => isContainerised ? container?.ACN_ContainerNumber ?? ZString.Empty : ZString.Empty;

		decimal IPack.ContainerTare => isContainerised ? container?.ContainerType?.RC_TareWeight ?? 0 : 0;

		bool IPack.PartialUseContainerIndicator => isContainerised && (container?.ACN_EmptyFullIndicator ?? ZString.Empty) == EmptyFullIndicatorList.Codes.LessThanFullContainerLoad;

		string IPack.LooseCargoPackageType
		{
			get
			{
				if (isBreakBulk)
				{
					var query = new ZQuery(RefPacksSchema.RP_CommercialPack, pack.APA_PackUQ);
					query.AddToFilter(RefPacksSchema.RP_CustomsCountry, Core.Constants.CountryCodes.Brazil);
					return pack.Factory.LoadTop1<CusRefPacks>(query)?.RP_CustomsPack ?? ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		string IPack.LooseCargoItemQuantity => isBreakBulk ? pack.APA_PackQty.ToString() : string.Empty;

		string IPack.PackageTypeBulk => isBulk ? pack.BulkType : ZString.Empty;

		string IPack.DescriptionBulk => isBulk ? pack.APA_GoodsDescription : ZString.Empty;

		string IPack.VehicleChassisNumber => ZString.Empty; // Will be filled in a future WI

		string IPack.BrandName => ZString.Empty; // Will be filled in a future WI

		string IPack.BrandNameCounterMarkPending => ZString.Empty; // Will be filled in a future WI

		string IPack.DangerousGoodsCode => loadType != 0 ? dangerousGoodsCode : ZString.Empty;

		string IPack.DangerousGoodsClassCode => !dangerousGoodsCode.IsEmpty ? pack.UNDGs.FirstOrDefault().DI_IMOClass : ZString.Empty;

		decimal IPack.Volume => ((ZDecimal)Core.Constants.Volume.ConvertSafe(pack.APA_Volume, pack.APA_VolumeUQ, Core.Constants.Volume.CubicMetres)).Truncate(3);

		IReadOnlyCollection<string> IPack.ContainerSealNumber => isContainerised && container != null ? new List<string>() { container.ACN_Seal1, container.ACN_Seal2, container.ACN_Seal3 } : new List<string>();

		IReadOnlyCollection<string> IPack.NCMGoodsCode => new List<string>() { pack.APA_CommodityCode };
	}
}
