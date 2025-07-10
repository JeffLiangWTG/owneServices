using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class TaxIdAndTaxMessageMappingLimitTaxIDsValidator : TaxIdAndTaxMessageMappingValidator
	{
		public TaxIdAndTaxMessageMappingLimitTaxIDsValidator(TaxIdAndTaxMessageCombinationRulesCollection rules) : base(rules)
		{
		}

		public override bool IsValidateMapping(string[] lineTypes, AccTaxRate taxId, AccInvMsg taxMessage)
		{
			if (lineTypes == null || taxId == null)
			{
				return true;
			}

			var validLineTypes = lineTypes.Intersect(ValidLineTypeList);
			if (!validLineTypes.Any())
			{
				return true;
			}

			var ruleList = Rules
				.Where(x => x.TaxRate == taxId.PK && x.TaxMessage == taxMessage?.PK && validLineTypes.Contains<string>(x.LineType));

			return ruleList.Count() == validLineTypes.Count();
		}
	}
}