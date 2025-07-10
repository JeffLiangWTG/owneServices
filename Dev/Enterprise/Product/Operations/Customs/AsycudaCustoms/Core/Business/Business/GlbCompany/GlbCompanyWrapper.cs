using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.AsycudaCustoms;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class GlbCompanyWrapper : MasterFiles.Business.GlbCompanyWrapper, IAsycudaGlbCompanyWrapper
	{
		protected GlbCompanyWrapper(GlbCompany company)
			: base(company)
		{
		}

		public override bool IsValidWrapper
		{
			get
			{
				var asycudaCustomsCountryProvider = ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>();
				return asycudaCustomsCountryProvider.IsAsycudaCustomsCountry(Company.GC_RN_NKCountryCode) && Company.PK == GlbCompany.CurrentCompany.PK;
			}
		}

		[ChildEditable]
		public ZZRefCusConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					configuration = ZZRefCusConfiguration.Get(Company) ?? ZZRefCusConfiguration.New(Company);
					RegisterEditableChildObject(configuration);
				}
				return configuration;
			}
		}
		ZZRefCusConfiguration configuration;
	}
}
