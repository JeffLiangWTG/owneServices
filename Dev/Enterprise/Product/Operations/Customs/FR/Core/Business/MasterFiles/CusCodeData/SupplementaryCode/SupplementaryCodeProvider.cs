using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	sealed class SupplementaryCodeProvider : EU.Business.SupplementaryCodeProvider
	{
		public SupplementaryCodeProvider(ZString countryCode) : base(countryCode)
		{
		}

		public SupplementaryCodeProvider(ZString countryCode, ISupplementaryCodeSupporter master) : base(countryCode, master)
		{
		}

		protected override BaseSupplementaryCodeValidation GetNewValidationCore(BaseSupplementaryCode supplementaryCode)
		{
			return new SupplementaryCodeValidation(supplementaryCode);
		}
	}
}
