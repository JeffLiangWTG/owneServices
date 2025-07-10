using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class TaxIdAndTaxMessageMappingLimitTaxMessagesValidator : TaxIdAndTaxMessageMappingValidator
	{
		public TaxIdAndTaxMessageMappingLimitTaxMessagesValidator(TaxIdAndTaxMessageCombinationRulesCollection rules) : base(rules)
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

			var mappingList = Rules
				.Where(x => x.TaxRate == taxId.PK && validLineTypes.Contains<string>(x.LineType));

			if (!mappingList.Any() || mappingList.GroupBy(x => x.LineType).All(x => x.Any(y => y.TaxMessage == taxMessage?.PK)))
			{
				return true;
			}

			return false;
		}
	}
}

