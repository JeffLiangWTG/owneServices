using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class LinkedCusAuthorisationRuleLookups : Customs.Business.LinkedCusAuthorisationRuleLookups
	{
		public LinkedCusAuthorisationRuleLookups(LinkedCusAuthorisationRule parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList RuleCodeList
		{
			get
			{
				var result = new CodeDescriptionPairList(base.RuleCodeList);
				result.AddRangeOverwriteIfExists(new LinkedCusAuthorisationRuleTypeList());
				result.SortByDescription();
				return result;
			}
		}

		protected override Dictionary<ZString, Func<ICollection>> GetValueListFromRuleCodeCore()
		{
			var result = base.GetValueListFromRuleCodeCore();

			result[LinkedCusAuthorisationRuleTypeList.Codes.Active] = () => Factory.GetCachedValue<YesNoList>();
			result[LinkedCusAuthorisationRuleTypeList.Codes.IEB] = () => Factory.GetCachedValue<ImportExportList>();
			result[LinkedCusAuthorisationRuleTypeList.Codes.OfficeType] = () => Factory.GetCachedValue<LocationQualifierList>();

			return result;
		}
	}
}
