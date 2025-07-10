using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;

sealed class ImportSupportingDocumentsFieldsControlTest : TestCaseWithFactory
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
		AssertEquals("CodeCodeFindBox: CharacterCasing", CharacterCasing.Upper, control.FindSingleOrDefault<ZCodeFindBox>("CodeCodeFindBox").CodeBox.CharacterCasing);
		AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, control.AdditionalDescriptionTextBox.CharacterCasing);
		AssertEquals("CSI_ItemNumber: Decimals", 0, control.ItemNumberCalcEdit.Decimals);
		AssertEquals("NKCountryCodeFindBox: CharacterCasing", CharacterCasing.Upper, control.FindSingleOrDefault<ZCodeFindBox>("NKCountryCodeFindBox").CodeBox.CharacterCasing);
	}

	IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
	{
		("CodeCodeFindBox", 0, typeof(ZCodeFindBox)),
		("ImportSupportingDocumentReferenceNumberUserControl", 1, typeof(ImportSupportingDocumentReferenceNumberUserControl)),
		("StatusAndProcedureUserControl", 2, typeof(StatusAndProcedureUserControl)),
		("QuantityAndUnitUserControl", 3, typeof(QuantityAndUnitUserControl)),
		("SecondQuantityAndUnitUserControl", 4, typeof(SecondQuantityAndUnitUserControl)),
		("ValueAndCurrencyUserControl", 5, typeof(ValueAndCurrencyUserControl)),
		("DateOfIssueAndExpiryUserControl", 6, typeof(DateOfIssueAndExpiryUserControl)),
		("AdditionalDescriptionTextBox", 7, typeof(ZTextBox)),
		("ItemNumberCalcEdit", 8, typeof(ZCalcEdit)),
		("NKCountryCodeFindBox", 9, typeof(ZCodeFindBox))
	};

	protected override void SetUp()
	{
		base.SetUp();
		control = new ImportSupportingDocumentsFieldsControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	ImportSupportingDocumentsFieldsControl control;
}
