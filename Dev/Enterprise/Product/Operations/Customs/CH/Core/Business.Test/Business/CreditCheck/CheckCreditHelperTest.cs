using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CheckCreditHelperTest))]
sealed class CheckCreditHelperTest : TestCaseWithFactory
{
	public void TestCheckCredit() => CombineAssertions(() =>
	{
		using var temporaryCreditControllerOverrideThresholdValue = CreditCheckTestHelper.DisposableCreditControllerOverride();
		using var temporaryCreditCheckOnMessageSendValue = CreditCheckTestHelper.DisposableCreditCheckOnSendOverride(true);

		var declaration1 = Factory.New<JobDeclaration>();
		declaration1.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
		declaration1.Invoices.AddNew().InvoiceLines.AddNew();
		declaration1.Importer.MiscServ.OM_ARCreditLimit = -1;

		var declaration2 = Factory.New<JobDeclaration>();
		declaration2.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
		declaration2.Invoices.AddNew().InvoiceLines.AddNew();
		declaration2.Importer.MiscServ.OM_ARCreditLimit = 9999;
		Factory.Save();

		AssertEquals("Check not passed", false, declaration1.CheckCredit(out var reasonForNotAllowed1));
		AssertNotEquals("Check not passed reason", string.Empty, reasonForNotAllowed1);

		AssertEquals("Check passed ok", true, declaration2.CheckCredit(out var reasonForNotAllowed2));
	});
}
