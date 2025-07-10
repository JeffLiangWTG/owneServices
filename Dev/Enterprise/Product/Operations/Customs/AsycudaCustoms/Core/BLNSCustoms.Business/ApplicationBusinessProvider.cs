using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BLNSCustoms.Business
{
	public class ApplicationBusinessProvider : BaseApplicationBusinessProvider
	{
		protected override ZString GetUniversalTariffTypeCore() => Constants.CusTariffCode.Schedule1Part1;

		public override ZString UniversalRefDataSource => Core.Constants.Customs.Universal.DataSetTypes.WTGData;

		protected override bool IsReciprocalRatesCore(GlbCompany company)
		{
			return !company.GC_RN_NKCountryCode.IsBLNSCountry(company.Factory) && base.IsReciprocalRatesCore(company);
		}
	}
}
