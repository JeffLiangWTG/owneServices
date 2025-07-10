using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;

sealed class ExportSupportingDocumentsFieldsControlTest : TestCaseWithFactory
{
	public void TestBindingSourceType()
	{
		AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
	}

	public void TestFields()
	{
		CombineAssertions(() =>
		{
			foreach (var (controlName, tabIndex, controlType) in FieldsDetails)
			{
				var actualField = control.FindSingleOrDefault<Control>(x => x.Name == controlName);
				if (actualField == null)
				{
					Assert($"Control: {controlName} does not exist", false);
				}
				else
				{
					AssertEquals($"{controlName}: tab index", tabIndex, actualField.TabIndex);
					AssertEquals($"{controlName}: type", controlType, actualField.GetType());
				}
			}
		});
	}

	public void TestFieldsProperties()
	{
		CombineAssertions(() =>
		{
			AssertEquals("CodeCodeFindBox: CharacterCasing", CharacterCasing.Upper, control.CodeCodeFindBox.CodeBox.CharacterCasing);
			AssertEquals("CSI_Status: CharacterCasing", CharacterCasing.Upper, control.StatusDropEdit.CharacterCasing);
			AssertEquals("ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, control.ReferenceNumberTextBox.CharacterCasing);
			AssertEquals("ReferenceNumberCodeFindBox: CharacterCasing", CharacterCasing.Upper, control.ReferenceNumberCodeFindBox.CodeBox.CharacterCasing);
			AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, control.AdditionalDescriptionTextBox.CharacterCasing);
			AssertEquals("CSI_ItemNumber: Decimals", 0, control.ItemNumberCalcEdit.Decimals);
		});
	}

	IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
	{
		("CodeCodeFindBox", 0, typeof(ZCodeFindBox)),
		("ReferenceNumberTextBox", 1, typeof(ZTextBox)),
		("ReferenceNumberCodeFindBox", 1, typeof(ZCodeFindBox)),
		("StatusDropEdit", 2, typeof(ZDropEdit)),
		("QuantityAndUnitUserControl", 3, typeof(QuantityAndUnitUserControl)),
		("SecondQuantityAndUnitUserControl", 4, typeof(SecondQuantityAndUnitUserControl)),
		("ValueAndCurrencyUserControl", 5, typeof(ValueAndCurrencyUserControl)),
		("DateOfIssueAndExpiryUserControl", 6, typeof(DateOfIssueAndExpiryUserControl)),
		("AdditionalDescriptionTextBox", 7, typeof(ZTextBox)),
		("ItemNumberCalcEdit", 8, typeof(ZCalcEdit))
	};

	protected override void SetUp()
	{
		base.SetUp();
		control = new ExportSupportingDocumentsFieldsControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	ExportSupportingDocumentsFieldsControl control;
}
