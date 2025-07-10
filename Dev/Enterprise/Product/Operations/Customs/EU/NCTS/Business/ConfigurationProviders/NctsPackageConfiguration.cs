namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPackageConfiguration
	{
		public NctsPackageConfiguration()
		{
		}

		public INctsPackageValidationDecider GetValidationDecider(NctsCommonCargoDesc goodsItem) => GetValidationDeciderCore(goodsItem);

		protected virtual INctsPackageValidationDecider GetValidationDeciderCore(NctsCommonCargoDesc goodsItem)
		{
			INctsPackageValidationDecider result = null;
			if (goodsItem != null)
			{
				if (goodsItem.IsPhase5Departure)
				{
					result = GetDeparturePhase5ValidationDecider();
				}
				else if (goodsItem.IsPhase5Arrival)
				{
					result = GetArrivalPhase5ValidationDecider();
				}
			}

			return result;
		}

		protected virtual INctsPackagePhase5ValidationDecider GetDeparturePhase5ValidationDecider() => new NctsPackageDeparturePhase5ValidationDecider();

		protected virtual INctsPackagePhase5ValidationDecider GetArrivalPhase5ValidationDecider() => new NctsPackageArrivalPhase5ValidationDecider();
	}
}
