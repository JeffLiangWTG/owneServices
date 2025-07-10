using System.Windows.Forms;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TempStorageRegTransactionNewForm))]
sealed class TempStorageRegTransactionNewFormTest : ZFormBasherTest
{
	public void TestFormCaption()
	{
		using var form = new TempStorageRegTransactionNewForm(regLine, regTransaction, regTransactionEditable);
		AssertEquals("FormCaption", "Manual Transaction", form.FormCaption);
	}

	public void TestMainPanel() => CombineAssertions(() =>
	{
		SetUpRegHeader();
		using var form = new TempStorageRegTransactionNewForm(regLine, regTransaction, regTransactionEditable);

		var mainPanel = form.MainPanel;
		AssertNotNull("MainPanel is not null", mainPanel);
		Assert("Form Contains MainPanel", form.Controls.Contains(mainPanel));
		Assert("Contains DynamicPanel", mainPanel.Controls.Contains(form.MainDynamicLayoutPanel));
	});

	public void TestMainDynamicLayoutPanel() => CombineAssertions(() =>
	{
		SetUpRegHeader();
		using var form = new TempStorageRegTransactionNewForm(regLine, regTransaction, regTransactionEditable);

		AssertNotNull("MainDynamicLayoutPanel is not null", form.MainDynamicLayoutPanel);
	});

	public void TestCloseButton() => CombineAssertions(() =>
	{
		SetUpRegHeader();
		using var form = new TempStorageRegTransactionNewForm(regLine, regTransaction, regTransactionEditable);

		var closeButton = form.CloseButton;
		AssertNotNull("CloseButton is not null", closeButton);
		Assert("Form Contains Close Button", form.Controls.Contains(closeButton));
		AssertEquals("Caption", "Close", closeButton.CaptionResourceString.Caption);
	});

	[RequiresSTA]
	public void TestCloseButton_SetPackageQtyToZero() => CombineAssertions(() =>
	{
		const string remainingError = "Remaining Packages should not be negative.";
		SetUpRegHeader();
		using var form = new TempStorageRegTransactionNewForm(regLine, regTransaction, regTransactionEditable);
		form.Show();
		regTransactionEditable.SRT_PackageQty = -20;

		form.SaveButton.PerformClick();
		AssertEquals("Form has errors", "The form has errors. Please fix them before continuing.", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertHasErrorContaining("Remaining has errors", regTransactionEditable.SRT_PackageQtyInfo, remainingError);

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		form.CloseButton.PerformClick();
		AssertNull("Form has no errors", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("PackageQty is deleted", expected: true, regTransactionEditable.IsDeleted);
	});

	public void TestAddNewButton() => CombineAssertions(() =>
	{
		SetUpRegHeader();
		using var form = new TempStorageRegTransactionNewForm(regLine, regTransaction, regTransactionEditable);

		var saveButton = form.SaveButton;
		AssertNotNull("SaveButton is not null", saveButton);
		Assert("Form Contains Save Button", form.Controls.Contains(saveButton));
		AssertEquals("Caption", "Save", saveButton.CaptionResourceString.Caption);
	});

	protected override Form GetFormToBashCore()
	{
		SetUpRegHeader();
		Factory.Save();
		var result = new TempStorageRegTransactionNewForm(regLine, regTransaction, regTransactionEditable);
		return result;
	}

	void SetUpRegHeader()
	{
		regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_Reference = "UNITTEST";
		var premises = Factory.New<CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "DESC";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "TestAddress";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		regHeader.SRH_SRP_Premises = premises.PK;
		regLine = regHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		regTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
		regTransactionEditable = new CusTempStorageRegLineTransactionFormEditable(regLine);
	}

	CusTempStorageRegHeader regHeader;
	CusTempStorageRegLine regLine;
	CusTempStorageRegLineTransaction regTransaction;
	CusTempStorageRegLineTransactionFormEditable regTransactionEditable;
}
