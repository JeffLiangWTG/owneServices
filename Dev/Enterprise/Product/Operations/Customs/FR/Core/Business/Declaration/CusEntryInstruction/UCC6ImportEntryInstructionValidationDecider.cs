using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public sealed class UCC6ImportEntryInstructionValidationDecider : IEntryInstructionValidationDecider
	{
		public UCC6ImportEntryInstructionValidationDecider(CusEntryInstruction cusEntryInstruction)
		{
			this.cusEntryInstruction = Argument.NotNull(cusEntryInstruction, nameof(cusEntryInstruction));
		}

		public bool IsRuleR0012Active => true;

		public bool IsRuleC0002Active => true;

		public bool IsRuleC0619ActiveForGoodsLocationDescription => true;

		public bool IsRuleC0626ActiveForCEI_OA_Warehouse2 => !cusEntryInstruction.IsSimplified;

		public bool IsRuleC0627Active => !cusEntryInstruction.IsSimplified;

		public bool IsRuleC0628ActiveForGoodsLocationDescription => cusEntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.IsTradingWithSpecialFiscalTerritoriesProcedure);

		public bool IsRuleC0829ActiveForCEI_OA_Warehouse2 => true;

		public bool IsRuleC0853ActiveForCEI_OA_Warehouse => true;

		public bool IsRuleC0382ActiveForGoodsLocationAddressHouseNumber => true;

		public bool IsMaximumEntryLinesAllowedRuleActive => true;

		public bool IsRuleC0834_N02Active => true;

		public bool IsRuleC0810_N01Active => !cusEntryInstruction.IsSimplified;

		public bool IsRuleNAT_004BisActive => true;

		public bool IsRuleNAT_130BisActive => true;

		public bool IsRuleR0933_N03Active => true;

		public bool IsRuleNAT_030Active => true;

		readonly CusEntryInstruction cusEntryInstruction;
	}
}
