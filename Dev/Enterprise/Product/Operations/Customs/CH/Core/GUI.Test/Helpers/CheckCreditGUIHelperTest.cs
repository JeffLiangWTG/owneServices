using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CheckCreditGUIHelper))]
sealed class CheckCreditGUIHelperTest : TestCaseWithFactory
{
	public void TestCheckCredit() => CombineAssertions(() =>
	{
		using var temporaryCreditControllerOverrideThresholdValue = CreditCheckTestHelper.DisposableCreditControllerOverride();
		using var temporaryCreditCheckOnMessageSendValue = CreditCheckTestHelper.DisposableCreditCheckOnSendOverride(true);

		var declaration1 = Factory.New<JobDeclaration>();
		declaration1.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
		declaration1.Invoices.AddNew().InvoiceLines.AddNew();
		declaration1.Importer.MiscServ.OM_ARCreditLimit = -1m;

		var declaration2 = Factory.New<JobDeclaration>();
		declaration2.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
		declaration2.Invoices.AddNew().InvoiceLines.AddNew();
		declaration2.Importer.MiscServ.OM_ARCreditLimit = 9999999m;
		Factory.Save();

		UnitTestUserNotification.Instance.ClearMessages();
		AssertEquals("Check not passed", false, declaration1.CheckCredit());
		AssertNotNull("Check not passed - reason", UnitTestUserNotification.Instance.LastMessage.Text);

		UnitTestUserNotification.Instance.ClearMessages();
		AssertEquals("Check passed", true, declaration2.CheckCredit());
		AssertNull("Check passed - reason", UnitTestUserNotification.Instance.LastMessage.Text);
	});
}
