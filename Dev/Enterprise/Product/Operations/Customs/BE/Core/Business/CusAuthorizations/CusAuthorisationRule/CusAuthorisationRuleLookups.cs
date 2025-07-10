using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BE.Business;

class CusAuthorisationRuleLookups : EU.Business.CusAuthorisationRuleLookups
{
	public CusAuthorisationRuleLookups(CusAuthorisationRule parent) : base(parent)
	{
	}

	protected CusAuthorisationHeaderProvider Provider => (CusAuthorisationHeaderProvider)AuthorisationHeader?.Provider;

	protected override Dictionary<ZString, Func<ICollection>> GetValueListFromRuleCodeCore() => new Dictionary<ZString, Func<ICollection>>
	{
		{
			CusAuthorisationRuleTypeList.Codes.Location, () => ValueListAuthorizationLocation
		}
	};

	ZZRefCusCodeListCombinedCollection ValueListAuthorizationLocation => ZZRefCusCodeListCombinedCollection.GetCachedCollection(
		Factory,
		AuthorisationHeader.CPH_RN_NKCountryCode,
		Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities,
		ZDateTime.Today);

	CusAuthorisationHeader AuthorisationHeader => Parent.AuthorisationHeader;
}
