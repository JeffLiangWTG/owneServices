using CargoWise.Common;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class WeightWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.IWeight
	{
		WeightWrapper(AsycudaPackedItem item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		readonly AsycudaPackedItem item;

		public decimal GrossMass => grossMass == 0 ? (grossMass = item.API_GrossWeight) : grossMass;
		decimal grossMass;

		public static WeightWrapper New(AsycudaPackedItem item) => item == null ? null : new WeightWrapper(item);
	}
}
