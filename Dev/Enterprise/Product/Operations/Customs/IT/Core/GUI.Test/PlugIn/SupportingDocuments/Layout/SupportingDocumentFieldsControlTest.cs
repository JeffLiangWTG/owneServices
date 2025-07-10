using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class SupportingDocumentFieldsControlTest : TestCaseWithFactory
{
	public void TestYearOfIssueTextBox()
	{
		TestControl<ZTextBox>(nameof(SupportingDocumentFieldsControl.YearOfIssueTextBox), (c) =>
		{
			AssertEquals("BindingMember", nameof(SupportingDocument.Schema.CSI_YearOfIssue), c.GetBindingMember());
			AssertEquals("CharacterCasing", CharacterCasing.Upper, c.CharacterCasing);
		});
	}

	public void TestIssuingAuthorityTextBox()
	{
		TestControl<ZTextBox>(nameof(SupportingDocumentFieldsControl.IssuingAuthorityTextBox), (c) =>
		{
			AssertEquals("BindingMember", nameof(SupportingDocument.Schema.CSI_ReferenceNumber2), c.GetBindingMember());
			AssertEquals("CharacterCasing", CharacterCasing.Normal, c.CharacterCasing);
		});
	}

	public void TestCountryCodeCodeFindBox()
	{
		TestControl<ZCodeFindBox>(nameof(SupportingDocumentFieldsControl.CountryCodeCodeFindBox), (c) =>
		{
			AssertEquals("BindingMember", nameof(SupportingDocument.Schema.CSI_RN_NKCountryCode), c.GetBindingMember());
		});
	}

	public void TestAvailabilityDropEdit()
	{
		TestControl<ZDropEdit>(nameof(SupportingDocumentFieldsControl.AvailabilityDropEdit), (c) =>
		{
			AssertEquals("BindingMember", nameof(SupportingDocument.Schema.CSI_Status), c.GetBindingMember());
			AssertEquals("CharacterCasing", CharacterCasing.Upper, c.CharacterCasing);
		});
	}

	public void TestLineNoCalcEdit()
	{
		TestControl<ZCalcEdit>(nameof(SupportingDocumentFieldsControl.LineNoCalcEdit), (c) =>
		{
			AssertEquals("BindingMember", nameof(SupportingDocument.Schema.CSI_LineNo), c.GetBindingMember());
			AssertEquals("MaxLength", 4, c.MaxLength);
		});
	}

	public void TestValueCalcFindBox()
	{
		TestControl<ZCalcFindBox>(nameof(SupportingDocumentFieldsControl.ValueCalcFindBox), (c) =>
		{
			AssertEquals("BindToAmount", nameof(SupportingDocument.Schema.CSI_Value), c.BindToAmount);
			AssertEquals("BindToUnit", nameof(SupportingDocument.Schema.CSI_RX_NKCurrency), c.BindToUnit);
			AssertEquals("Decimals", 5, c.Decimals);
		});
	}

	void TestControl<T>(string controlName, Action<T> assertControl) where T : Control
	{
		using (var control = new SupportingDocumentFieldsControl())
		{
			CombineAssertions(() =>
			{
				T field = null;
				AssertNoExceptionThrown($"{controlName} should be found", () => { field = control.FindSingle<T>(controlName); });
				assertControl(field);
			});
		}
	}
}
