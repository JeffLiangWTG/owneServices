using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business
{
	public class CusAuthorisationRuleLookups : EU.Business.CusAuthorisationRuleLookups
	{
		public CusAuthorisationRuleLookups(CusAuthorisationRule parent) : base(parent)
		{
		}

		CusAuthorisationHeader AuthorisationHeader => Parent.AuthorisationHeader;

		protected CusAuthorisationHeaderProvider Provider => (CusAuthorisationHeaderProvider)AuthorisationHeader?.Provider;

		protected override Dictionary<ZString, Func<ICollection>> GetValueListFromRuleCodeCore()
		{
			var result = new Dictionary<ZString, Func<ICollection>>();
			result.Add(EU.Business.CusAuthorisationRuleTypeList.Codes.Location, () => new RefUNLOCOCollection(Factory));
			return result;
		}
	}
}
