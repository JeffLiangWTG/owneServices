using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AR.Manifest.Business
{
	class BLArgentinaWrapperPack : IPack
	{
		internal BLArgentinaWrapperPack(AsycudaPack pack)
		{
			this.pack = CargoWise.Common.Argument.NotNull(pack, "AsycudaPack cannot be null");
		}
		readonly AsycudaPack pack;

		int IPack.LineNumber => pack.APA_LineNo;

		string IPack.PackUQ
		{
			get
			{
				var query = new ZQuery(RefPacksSchema.RP_CommercialPack, this.pack.APA_PackUQ);
				query.AddToFilter(RefPacksSchema.RP_CustomsCountry, Core.Constants.CountryCodes.Argentina);
				var pack = this.pack.Factory.LoadTop1<CusRefPacks>(query);

				return pack?.RP_CustomsPack ?? this.pack.APA_PackUQ;
			}
		}

		int IPack.PackQuantity => pack.APA_PackQty;

		decimal IPack.VolumeWeight => pack.APA_Volume > 0 ? pack.APA_Volume.Round(3) : pack.APA_Weight.Round(3);

		string IPack.Description => pack.APA_GoodsDescription;

		string IPack.MarksAndNumbers => pack.APA_MarksAndNumbers;

		string IPack.ContainerCondition
		{
			get
			{
				var container = pack.Container;
				if (container != null && container.ACN_EmptyFullIndicator != EmptyFullIndicatorList.Codes.EmptyContainer)
				{
					return ARMessageConstants.NotEmptyContainerCondition;
				}
				return ARMessageConstants.EmptyContainerCondition;
			}
		}
	}
}
