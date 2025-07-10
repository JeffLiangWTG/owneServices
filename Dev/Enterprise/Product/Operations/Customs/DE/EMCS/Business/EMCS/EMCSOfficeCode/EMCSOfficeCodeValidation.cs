using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSOfficeCodeValidation : OfficeCodeValidation
	{
		public EMCSOfficeCodeValidation(EMCSOfficeCode parent)
			: base(parent)
		{
		}

		protected override Dictionary<string, ValidationRule> GetCountryValidationRulesCore()
		{
			var result = base.GetCountryValidationRulesCore();
			result.Add(nameof(DispatchOfficeMustBeGermanIfDeclarationIsConsolidatedDocument), new DispatchOfficeMustBeGermanIfDeclarationIsConsolidatedDocument(Parent));
			return result;
		}

		protected new EMCSOfficeCode Parent => (EMCSOfficeCode)base.Parent;
	}
}
