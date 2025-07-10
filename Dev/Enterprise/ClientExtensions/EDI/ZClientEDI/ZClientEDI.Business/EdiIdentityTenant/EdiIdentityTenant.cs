using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.IdentityTenant.Business
{
	[CodeProperty(EdiIdentityTenantSchema.Constants.IDT_Name), DescriptionProperty(EdiIdentityTenantSchema.Constants.IDT_Name)]
	public class EdiIdentityTenant : AutoEdiIdentityTenant
	{
		public EdiIdentityTenant(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("DE9C68D0-09F8-4A9E-9806-7426A39F41D0", "Tenant - {0}", IDT_Name);

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public EdiIdentityTenant OnboardingEdiIdentityTenant
		{
			get
			{
				if (onboardingTenant == null)
				{
					var query = new ZQuery(EdiIdentityTenantSchema.IDT_Onboarding, true);
					query.AddToFilter(EdiIdentityTenantSchema.IDT_TenantId, SQLComparisonOperator.NotEqual,
						IDT_TenantId);
					onboardingTenant = Factory.LoadTop1<EdiIdentityTenant>(query);
				}
				return onboardingTenant;
			}
		}

		EdiIdentityTenant onboardingTenant;

		public override void OnSaving()
		{
			if (IDT_Onboarding)
			{
				SetOtherTenantOnboardingToFalse();
			}
			base.OnSaving();
		}

		void SetOtherTenantOnboardingToFalse()
		{
			if (OnboardingEdiIdentityTenant != null)
			{
				OnboardingEdiIdentityTenant.IDT_Onboarding = false;
			}
		}
	}
}
