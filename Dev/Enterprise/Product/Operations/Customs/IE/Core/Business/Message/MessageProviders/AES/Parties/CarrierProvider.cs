using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CarrierProvider : ICarrier
	{
		public static CarrierProvider New(OrgHeader org, bool useTCUIfEORIIsMissing = true) => org == null ? null : new CarrierProvider(org, useTCUIfEORIIsMissing);
		public static CarrierProvider New(JobDocAddress address, bool useTCUIfEORIIsMissing = true) => New(address?.Organisation, useTCUIfEORIIsMissing);

		CarrierProvider(OrgHeader org, bool useTCUIfEORIIsMissing)
		{
			this.org = org;
			this.useTCUIfEORIIsMissing = useTCUIfEORIIsMissing;
		}
		readonly OrgHeader org;
		readonly bool useTCUIfEORIIsMissing;

		public string CarrierId => CachedValueHelper.GetValue(ref carrierIdCached, () => useTCUIfEORIIsMissing ? org.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true) : org.GetEoriDetails());
		CachedValue<string> carrierIdCached;

		public IContact CarrierContact => org != null ? CachedValueHelper.GetValue(ref carrierContactCached, () => ContactProvider.New(org, false)) : null;
		CachedValue<IContact> carrierContactCached;
	}
}
