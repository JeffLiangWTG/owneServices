using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(ImportSupportingDocumentsFieldsControlBag))]
public class ImportSupportingDocumentsFieldsControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => new ImportSupportingDocumentsFieldsControlBag();

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ImportSupportingDocumentsFieldsControl.ImportSupportingDocumentReferenceNumberUserControl);
			yield return nameof(ImportSupportingDocumentsFieldsControl.CodeCodeFindBox);
			yield return nameof(ImportSupportingDocumentsFieldsControl.QuantityAndUnitUserControl);
			yield return nameof(ImportSupportingDocumentsFieldsControl.SecondQuantityAndUnitUserControl);
			yield return nameof(ImportSupportingDocumentsFieldsControl.StatusAndProcedureUserControl);
			yield return nameof(ImportSupportingDocumentsFieldsControl.ValueAndCurrencyUserControl);
			yield return nameof(ImportSupportingDocumentsFieldsControl.DateOfIssueAndExpiryUserControl);
			yield return nameof(ImportSupportingDocumentsFieldsControl.AdditionalDescriptionTextBox);
			yield return nameof(ImportSupportingDocumentsFieldsControl.ItemNumberCalcEdit);
			yield return nameof(ImportSupportingDocumentsFieldsControl.NKCountryCodeFindBox);
		}
	}
}
