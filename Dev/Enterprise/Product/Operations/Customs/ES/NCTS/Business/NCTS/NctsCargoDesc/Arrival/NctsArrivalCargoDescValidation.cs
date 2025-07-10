using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsArrivalCargoDescValidation : EU.NCTS.Business.NctsArrivalCargoDescValidation
{
	public NctsArrivalCargoDescValidation(NctsArrivalCargoDesc parent) : base(parent)
	{
	}

	protected new NctsArrivalCargoDesc Parent => (NctsArrivalCargoDesc)base.Parent;

	protected override INotificationType TR0084NotificationType => NotificationType.Warning;

	protected override bool LiabilityListNotificationTypeIsMessageError => false;

	protected override void CheckLiabilityTariff()
	{
		base.CheckLiabilityTariff();

		if (Parent.IsLiabilityCalculationForArrivalSupported)
		{
			CheckLiabilityTariffFirstEightNumbers();
		}

		void CheckLiabilityTariffFirstEightNumbers()
		{
			var harmonisedTariff = Parent.BY_HarmonisedTariff;
			var liabilityTariff = Parent.LiabilityTariff;
			var unloadesGoodsItemsHarmonisedTariff = Parent.UnloadedGoodsItem?.BY_HarmonisedTariff ?? ZString.Empty;

			if (liabilityTariff.Length == 10)
			{
				var tariffToCompare = Parent.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF ? unloadesGoodsItemsHarmonisedTariff : harmonisedTariff;

				var liabilityTariffFirstEight = liabilityTariff.SubstringSafe(0, 8);
				if (liabilityTariffFirstEight != tariffToCompare.Substring(0, 8))
				{
					Parent.LiabilityTariffInfo.AddWarning(Res.GetString("C94E7D97-0D26-4206-B613-5A5A310FC10E", "Commodity Code entered for liability calculation does not match the value used in the Transit declaration. Please note this value will be used to enter goods into Temporary Storage"));
				}
			}
		}
	}
}
