using NUnit.Framework;
namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ExportInvoiceLineValidationDecider))]
	sealed class UCC6ExportInvoiceLineValidationDeciderTest : ExportInvoiceLineValidationDeciderTest<UCC6ExportInvoiceLineValidationDecider>
	{
		protected override bool ExpectedIsRuleR0222Active => false;

		protected override bool ExpectedIsRuleR0223Active => true;

		protected override bool ExpectedIsRuleR0224Active => true;
	}
}
