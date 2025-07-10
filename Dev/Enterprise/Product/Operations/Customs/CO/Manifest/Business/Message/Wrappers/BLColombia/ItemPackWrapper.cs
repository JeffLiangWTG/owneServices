using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal class ItemPackWrapper : IItemPack
	{
		internal ItemPackWrapper(AsycudaPack pack)
		{
			this.pack = Argument.NotNull(pack, "AsycudaPack cannot be null");
		}
		readonly AsycudaPack pack;

		string IItemPack.PackagingCode
		{
			get
			{
				var query = new ZQuery(RefPacksSchema.RP_CommercialPack, pack.APA_PackUQ);
				query.AddToFilter(RefPacksSchema.RP_CustomsCountry, Core.Constants.CountryCodes.Colombia);
				return pack.Factory.LoadTop1<CusRefPacks>(query)?.RP_CustomsPack ?? pack.APA_PackUQ;
			}
		}

		string IItemPack.GeneralID => pack.APA_GoodsDescription;

		bool IItemPack.DangerousGood => pack.IsHazardous;

		int IItemPack.ONUSerialNumber
		{
			get
			{
				var undgs = pack.UNDGs?.FirstOrDefault();
				if (undgs != null)
				{
					var result = undgs.DI_DG_NKSubs.KeepNumericCharacters();
					return result.IsEmpty ? ZInt.Zero : ZInt.ParseSafe(result, 0);
				}
				return 0;
			}
		}

		string IItemPack.Ipel => pack.UNDGs?.FirstOrDefault()?.DI_IMOClass ?? string.Empty;
	}
}
