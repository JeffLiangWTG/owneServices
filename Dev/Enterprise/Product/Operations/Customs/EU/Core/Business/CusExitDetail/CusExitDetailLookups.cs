using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitDetailLookups : AutoCusExitDetailLookups
	{
		public CusExitDetailLookups(AutoCusExitDetail parent) : base(parent)
		{
		}

		public new CusExitDetail Parent => (CusExitDetail)base.Parent;

		public CustomsOfficeCodeCollection CustomsOffices => Parent.Header?.Lookups.CustomsOffices ?? EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory);

		public OrganisationsFindBoxCollection OrganizationsFindBoxList => Parent.Header?.Lookups.OrganizationsFindBoxList ?? new OrganisationsFindBoxCollection(Factory);

		public virtual CodeDescriptionPairList StatusList => RefCusCodeListTypes.GetCachedList(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExitCustomsStatus, ZDateTime.Today);
	}
}
