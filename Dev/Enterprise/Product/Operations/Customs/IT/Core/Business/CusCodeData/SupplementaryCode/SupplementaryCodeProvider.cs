using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business;

sealed class SupplementaryCodeProvider : EU.Business.SupplementaryCodeProvider
{
	public SupplementaryCodeProvider(ZString countryCode) : base(countryCode)
	{
	}

	public SupplementaryCodeProvider(ZString countryCode, ISupplementaryCodeSupporter master) : base(countryCode, master)
	{
	}

	public override ZShort NumberOfCodes => 97;

	protected override BaseSupplementaryCodeValidation GetNewValidationCore(BaseSupplementaryCode supplementaryCode)
		=> new SupplementaryCodeValidation(supplementaryCode);

	protected override BaseSupplementaryCodePropertyChangedNotifier GetNewSupplementaryCodePropertyChangedNotifierCore(BaseSupplementaryCode supplementaryCode)
		=> new SupplementaryCodePropertyChangedNotifier(supplementaryCode);

	protected override BaseSupplementaryCodeLookups GetNewLookupsCore(BaseSupplementaryCode supplementaryCode)
		=> new SupplementaryCodeLookups(supplementaryCode);
}

