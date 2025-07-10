using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class BaseApplicationBusinessProviderForTest : BaseApplicationBusinessProvider
	{
		public bool IsReciprocalRatesCoreExposed { get; set; }

		protected override ZString GetUniversalTariffTypeCore() => ZString.Empty;

		public override ZString UniversalRefDataSource => ZString.Empty;

		protected override bool IsReciprocalRatesCore(GlbCompany company) => IsReciprocalRatesCoreExposed;
	}
}
