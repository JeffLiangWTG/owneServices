using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public sealed class SupplementaryCodeProviderCDS : SupplementaryCodeProvider
	{
		public SupplementaryCodeProviderCDS(ZString countryCode) : base(countryCode)
		{
		}

		public SupplementaryCodeProviderCDS(ZString countryCode, ISupplementaryCodeSupporter master) : base(countryCode, master)
		{
		}

		public override ZShort NumberOfCodes => new ZShort(32767);
	}
}
