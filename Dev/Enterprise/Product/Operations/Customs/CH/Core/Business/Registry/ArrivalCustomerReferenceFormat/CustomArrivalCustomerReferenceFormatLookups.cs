using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public sealed class CustomArrivalCustomerReferenceFormatLookups : ZLookups
{
	public CustomArrivalCustomerReferenceFormatLookups(AutoCustomArrivalCustomerReferenceFormat parent) : base(parent)
	{
	}

	new CustomArrivalCustomerReferenceFormat Parent => (CustomArrivalCustomerReferenceFormat)base.Parent;

	protected override BusinessObjectFactory Factory => Parent.CurrentFactory;

	public CodeDescriptionPairList AuthorizationLocationCodeList
	{
		get
		{
			var companyPK = Parent.CurrentFallbackLevel?.CompanyPK(false) ?? ZGuid.Empty;
			var cacheKey = $"CH.CustomArrivalCustomerReferenceFormatLookups.AuthorizationLocationCodeList.{companyPK}";
			return Factory.GetCachedValue(cacheKey, () =>
			{
				var authorizationList = new CodeDescriptionPairList();
				var company = Factory.Load<IGlbCompany>(companyPK);
				if (company != null)
				{
					var authorizations = new CusAuthorisationHeaderCollection(Factory, CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, company.GC_OH_OrgProxy, ZDate.Today);
					foreach (var authorization in authorizations)
					{
						authorizationList.AddPair(authorization.CPH_Number);
					}
				}
				return authorizationList;
			});
		}
	}

	public CodeDescriptionPairList YearOptionList => Factory.GetCachedValue<CustomArrivalCustomerReferenceFormatYearOptionList>();
}
