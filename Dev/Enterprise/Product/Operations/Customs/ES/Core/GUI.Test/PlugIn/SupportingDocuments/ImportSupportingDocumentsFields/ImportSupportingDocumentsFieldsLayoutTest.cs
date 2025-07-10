using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(ImportSupportingDocumentsFieldsLayout))]
public class ImportSupportingDocumentsFieldsLayoutTest : LayoutsAbstractTest
{
	public void TestH1FieldsVisibility() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(true))
		{
			AssertEquals("AdditionalDescriptionTextBox Visible for IMP and H1 activate", true, Layout.IsVisible(ImportSupportingDocumentsFieldsControlBag.Instance.AdditionalDescriptionTextBox, declaration));
			AssertEquals("ItemNumberCalcEdit Visible for IMP and H1 activate", true, Layout.IsVisible(ImportSupportingDocumentsFieldsControlBag.Instance.ItemNumberCalcEdit, declaration));
			AssertEquals("NKCountryCodeFindBox Visible for IMP and H1 activate", true, Layout.IsVisible(ImportSupportingDocumentsFieldsControlBag.Instance.NKCountryCodeFindBox, declaration));
		}

		using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(false))
		{
			AssertEquals("NKCountryCodeFindBox not Visible for IMP and H1 not activate", false, Layout.IsVisible(ImportSupportingDocumentsFieldsControlBag.Instance.NKCountryCodeFindBox, declaration));
		}
	});

	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (ImportSupportingDocumentsFieldsControlBag.Instance.CodeCodeFindBox, ControlWidthClass.Auto);
			yield return (ImportSupportingDocumentsFieldsControlBag.Instance.ImportSupportingDocumentReferenceNumberUserControl, ControlWidthClass.Auto);
			yield return (ImportSupportingDocumentsFieldsControlBag.Instance.StatusAndProcedureUserControl, ControlWidthClass.Auto);
			yield return (ImportSupportingDocumentsFieldsControlBag.Instance.QuantityAndUnitUserControl, ControlWidthClass.Auto);
			yield return (ImportSupportingDocumentsFieldsControlBag.Instance.SecondQuantityAndUnitUserControl, ControlWidthClass.Auto);
			yield return (ImportSupportingDocumentsFieldsControlBag.Instance.ValueAndCurrencyUserControl, ControlWidthClass.Auto);
			yield return (ImportSupportingDocumentsFieldsControlBag.Instance.DateOfIssueAndExpiryUserControl, ControlWidthClass.Auto);
			yield return (ImportSupportingDocumentsFieldsControlBag.Instance.AdditionalDescriptionTextBox, ControlWidthClass.Auto);
			yield return (ImportSupportingDocumentsFieldsControlBag.Instance.ItemNumberCalcEdit, ControlWidthClass.Auto);
			yield return (ImportSupportingDocumentsFieldsControlBag.Instance.NKCountryCodeFindBox, ControlWidthClass.Auto);
		}
	}

	PanelLayout Layout => layout ?? (layout = new ImportSupportingDocumentsFieldsLayout().Layout);
	PanelLayout layout;

	protected override int ControlBagCount => 1;
	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ImportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>();
}
