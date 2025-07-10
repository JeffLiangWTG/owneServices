using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ExportInvoiceLineValidationDecider))]
	sealed class UCC6ExportInvoiceLineValidationDeciderTest : ExportInvoiceLineValidationDeciderTest<UCC6ExportInvoiceLineValidationDecider>
	{
		protected override bool ExpectedIsRuleR0222Active => true;

		protected override bool ExpectedIsRuleR0223Active => true;

		protected override bool ExpectedIsRuleR0224Active => true;
	}
}
