using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class CusAuthorisationRuleLookups : Customs.Business.CusAuthorisationRuleLookups
	{
		public CusAuthorisationRuleLookups(CusAuthorisationRule parent) : base(parent)
		{
		}

		#region ValueList
		protected override Dictionary<ZString, Func<ICollection>> GetValueListFromRuleCodeCore()
		{
			var result = new Dictionary<ZString, Func<ICollection>>();
			var type = Parent?.AuthorisationHeader?.CPH_Type ?? ZString.Empty;
			if (CusAuthorisationHeaderExtensions.TypeThatHaveAGuidLOCRuleValueFrom.Contains(type))
			{
				result.Add(CusAuthorisationRuleTypeList.Codes.Location, () => GetValueListOrganization);
			}
			else
			{
				result.Add(CusAuthorisationRuleTypeList.Codes.Location, () => GetValueListLocation);
			}
			return result;
		}

		OrgHeaderCollection GetValueListOrganization => new OrgHeaderCollection(Factory);

		ZZRefCusCodeListCombinedCollection GetValueListLocation => ZZRefCusCodeListCombinedCollection.GetCachedCollection(
			Factory,
			Parent.AuthorisationHeader.CPH_RN_NKCountryCode,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LocationsInAuthorisations,
			ZDateTime.Today);

		#endregion

		#region DescriptionList
		protected override Dictionary<ZString, Func<ICollection>> GetDescriptionListFromRuleCodeCore()
		{
			var result = new Dictionary<ZString, Func<ICollection>>();
			var type = Parent?.AuthorisationHeader?.CPH_Type ?? ZString.Empty;
			if (CusAuthorisationHeaderExtensions.TypeThatHaveAGuidLOCRuleValueFrom.Contains(type))
			{
				result.Add(CusAuthorisationRuleTypeList.Codes.Location, () => GetDescriptionListLocation);
			}
			return result;
		}

		OrgAddressCollection GetDescriptionListLocation
		{
			get
			{
				var collection = new OrgAddressCollection(Factory, new ZQuery(OrgAddressSchema.OA_OH, new ZGuid(Parent.CPR_ValueFrom)));
				collection.Load();
				return collection;
			}
		}
		#endregion
	}
}
