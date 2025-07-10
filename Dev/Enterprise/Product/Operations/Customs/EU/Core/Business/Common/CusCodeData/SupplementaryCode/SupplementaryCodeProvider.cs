using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class SupplementaryCodeProvider : BaseSupplementaryCodeProvider
	{
		public SupplementaryCodeProvider(ZString countryCode, ISupplementaryCodeSupporter master) : base(countryCode)
		{
		}

		public SupplementaryCodeProvider(ZString countryCode) : base(countryCode)
		{
		}

		protected override BaseSupplementaryCodeValidation GetNewValidationCore(BaseSupplementaryCode supplementaryCode) => new SupplementaryCodeValidation(supplementaryCode);

		public new static BaseSupplementaryCodeProvider GetByCountryCode(ZString countryCode)
			=> SupplementaryCodeProviderFactory.GetByCountryCodeOrDefault(countryCode, () => new SupplementaryCodeProvider(countryCode));
	}
}
