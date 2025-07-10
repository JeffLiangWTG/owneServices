using System.Linq;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business
{
	public sealed class UCC5ImportInvoiceHeaderValidationDecider : EU.Business.Declaration.IInvoiceHeaderValidationDecider, IRuleCD8051ForJZ_ValuationCodeDecider
	{
		public bool IsRuleC0002Active => false;

		public bool IsRuleC0624Active => false;

		public bool IsRuleC0627Active => false;

		public bool IsRuleC0729Active => false;

		public bool IsRuleC0728Active => false;

		public bool IsRuleC0738Active => false;

		public bool IsRuleR0012Active => false;

		bool IRuleCD8051ForJZ_ValuationCodeDecider.IsActive(JobComInvoiceHeader header) => header.CusEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.IsH1 || x.IsH5);
	}
}
