using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobComInvoiceHeaderDeepCloneStrategy))]
sealed class JobComInvoiceHeaderDeepCloneStrategyTest : TestCaseWithFactory
{
	public void TestSpecialMentuionsCloned()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoice.SpecialMentions = "abc";
		Factory.Save();

		var clonedInvoice = (JobComInvoiceHeader)new JobComInvoiceHeaderDeepCloneStrategy(invoice, CloneType.TemplateCopy, declaration, null).Clone();
		CombineAssertions(() =>
		{
			AssertEquals("abc", clonedInvoice.SpecialMentions);
		});
	}
}
