using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(EntryLineAdditionalDataUserControl))]
sealed class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
{
	public void TestTabPagesVisibilityAndOrder_Import() => AssertTabPagesVisibilityAndOrder(CHJobMessageTypeList.Codes.Import);

	public void TestTabPagesVisibilityAndOrder_Export() => AssertTabPagesVisibilityAndOrder(CHJobMessageTypeList.Codes.Export);

	public void TestTabPagesVisibilityAndOrder_EDA() => AssertTabPagesVisibilityAndOrder(CHJobMessageTypeList.Codes.ExportDeclarationActivation);

	void AssertTabPagesVisibilityAndOrder(string messageType)
	{
		Declaration.JE_MessageType = messageType;

		using (var form = new ZForm(Declaration))
		using (var userControl = new EntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			CombineAssertions(() =>
			{
				var expectedTabPagesInOrder = GetExpectedTabPagesInOrder(userControl, messageType);

				foreach (var expectedTabPage in expectedTabPagesInOrder)
				{
					Assert($"{expectedTabPage.Name} is visible", expectedTabPage.TabVisible);
				}
				AssertContainsExactElementsInExactOrder(expectedTabPagesInOrder, userControl.TabControl.TabPages);
			});
		}
	}

	IEnumerable<ZTabPage> GetExpectedTabPagesInOrder(EntryLineAdditionalDataUserControl control, string messageType)
	{
		switch (messageType)
		{
			case CHJobMessageTypeList.Codes.Import:
				yield return control.ExtendedInformationTabPage;
				yield return control.TaxOrFeeTabPage;
				break;
			case CHJobMessageTypeList.Codes.Export:
				yield return control.ExtendedInformationTabPage;
				break;
			case CHJobMessageTypeList.Codes.ExportDeclarationActivation:
				yield return control.ExtendedInformationTabPage;
				break;
		}
	}

	public void TestCalculatedDutyAndTaxGrid()
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		using (var form = new ZForm(Declaration))
		using (var userControl = new EntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			CombineAssertions(() =>
			{
				AssertNotNull("Calculated Duty And Tax Group Box exists", userControl.CalculatedDutyAndTaxGroupBox);
				AssertNotNull("Calculated Duty And Tax Grid exists", userControl.CalculatedDutyAndTaxGrid);
				AssertType<EntryLineAdditionalDataTaxGridUserControl>("CalculatedDutyAndTaxGrid reused control", userControl.CalculatedDutyAndTaxGrid);
				AssertEquals("Calculated Duty And Tax Grid binding member", "CustomsEntryHeaders.AllEntryLines.Fees", userControl.CalculatedDutyAndTaxGrid.GetBindingMember());
			});
		}
	}

	public void TestConfirmedDutyAndTaxGrid()
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;

		using (var form = new ZForm(Declaration))
		using (var userControl = new EntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			CombineAssertions(() =>
			{
				AssertNotNull("Confirmed Duty And Tax GroupBox exists", userControl.CalculatedDutyAndTaxGroupBox);
				AssertNotNull("Confirmed Duty And Tax Grid exists", userControl.ConfirmedDutyAndTaxGrid);
				AssertType<EntryLineAdditionalDataTaxGridUserControl>("ConfirmedDutyAndTaxGrid reused control", userControl.ConfirmedDutyAndTaxGrid);
				AssertEquals("Confirmed Duty And Tax Grid binding member", "CustomsEntryHeaders.AllEntryLines.ConfirmedFees", userControl.ConfirmedDutyAndTaxGrid.GetBindingMember());
			});
		}
	}

	JobDeclaration Declaration => declaration ??= GetNewJobDeclaration();
	JobDeclaration declaration;

	JobDeclaration GetNewJobDeclaration()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.ActiveEntryHeaders.AddNew();
		return declaration;
	}
}
