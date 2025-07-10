using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(SumARegisterForm))]
sealed class SumARegisterFormTest : ZFormBasherTest
{
	public void TestFormCaption() => CombineAssertions(() =>
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		using var form = new SumARegisterForm(header);
		form.Show();
		AssertEquals("No Reference", "SumA Register Form", form.FormCaption);
		header.SRH_Reference = "ATB150002110520195876";
		AssertEquals("Reference Entered", "SumA Register Form - ATB150002110520195876", form.FormCaption);
	});

	public void TestCheckOnShowPreSaveDialogs()
	{
		var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		using var form = new SumARegisterForm(header);
		form.Show();

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

		form.FireSaveButton();
		AssertEquals("At least one line should be entered.", UnitTestUserNotification.Instance.LastMessage.Text);

		var line = Factory.New<CusTempStorageRegLine>();
		header.CusTempStorageRegLines.Add(line);
		line.SRL_LocationOfGoods = "CN";
		line.SRL_LimitDate = ZDateTime.Now.Date;
		line.SRL_LineNumber = 1;

		var transaction = line.CusTempStorageRegLineTransactions.AddNew();
		transaction.SRT_InternalReferenceNumber = "T001";
		transaction.SRT_PackageQty = 10;
		transaction.SRT_GrossWeight = 10m;
		transaction.SRT_PackageQty = 100;

		form.FireSaveButton();
		AssertEquals("Transactions cannot be amended once saved. Do you want to continue saving the transactions?", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestSectionCode()
	{
		var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		using var form = new SumARegisterForm(header);
		var codeOverridable = (ICustomerServiceMenuSectionCodeOverridable)form;
		AssertNotNull("SumARegisterForm as ICustomerServiceMenuSectionCodeOverridable", codeOverridable);
		AssertEquals("SectionCode", "NONE", codeOverridable.SectionCode);
	}

	protected override Form GetFormToBashCore()
	{
		var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		Factory.Save();
		var result = new SumARegisterForm(header);
		result.ControllerID = ControllerIDs.Customs.SumARegister;
		return result;
	}
}
