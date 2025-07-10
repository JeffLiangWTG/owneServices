using CargoWise.Common;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal class PackWrapper : IPack
	{
		internal PackWrapper(AsycudaPack pack)
		{
			this.pack = Argument.NotNull(pack, "AsycudaPack cannot be null");
		}
		readonly AsycudaPack pack;

		string IPack.ContainerNumber => pack.Container?.ACN_ContainerNumber ?? ZString.Empty;

		int IPack.PackageQty => pack.APA_PackQty;

		decimal IPack.Weight => ((ZDecimal)Core.Constants.Weight.ConvertSafe(pack.APA_Weight, pack.APA_WeightUQ, Core.Constants.Weight.Kilograms)).Truncate(5);

		decimal IPack.Volume => ((ZDecimal)Core.Constants.Volume.ConvertSafe(pack.APA_Volume, pack.APA_VolumeUQ, Core.Constants.Volume.CubicMetres)).Truncate(2);

		IItemPack IPack.Item => item ?? (item = new ItemPackWrapper(pack));
		IItemPack item;
	}
}
