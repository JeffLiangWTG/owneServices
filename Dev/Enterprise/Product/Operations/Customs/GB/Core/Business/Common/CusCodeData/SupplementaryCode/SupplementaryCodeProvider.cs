using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business
{
	public sealed class SupplementaryCodeProvider : EU.Business.SupplementaryCodeProvider
	{
		public SupplementaryCodeProvider(ZString countryCode) : base(countryCode)
		{
		}

		public SupplementaryCodeProvider(ZString countryCode, ISupplementaryCodeSupporter master) : base(countryCode, master)
		{
		}

		public override ZShort NumberOfCodes => ZShort.Zero;

		public static BaseSupplementaryCodeProvider GetByJobDeclaration(JobDeclaration jobDeclaration) => GetByCountryCode(jobDeclaration.GetCountryCodeForSupplementaryCodeProvider());
	}
}
