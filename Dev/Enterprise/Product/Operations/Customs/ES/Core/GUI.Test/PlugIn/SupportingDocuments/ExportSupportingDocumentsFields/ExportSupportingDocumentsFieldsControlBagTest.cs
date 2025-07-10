using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

[TestedType(typeof(ExportSupportingDocumentsFieldsControlBag))]
sealed class ExportSupportingDocumentsFieldsControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => new ExportSupportingDocumentsFieldsControlBag();

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ExportSupportingDocumentsFieldsControl.ReferenceNumberTextBox);
			yield return nameof(ExportSupportingDocumentsFieldsControl.ReferenceNumberCodeFindBox);
			yield return nameof(ExportSupportingDocumentsFieldsControl.CodeCodeFindBox);
			yield return nameof(ExportSupportingDocumentsFieldsControl.StatusDropEdit);
			yield return nameof(ExportSupportingDocumentsFieldsControl.QuantityAndUnitUserControl);
			yield return nameof(ExportSupportingDocumentsFieldsControl.SecondQuantityAndUnitUserControl);
			yield return nameof(ExportSupportingDocumentsFieldsControl.ValueAndCurrencyUserControl);
			yield return nameof(ExportSupportingDocumentsFieldsControl.DateOfIssueAndExpiryUserControl);
			yield return nameof(ExportSupportingDocumentsFieldsControl.AdditionalDescriptionTextBox);
			yield return nameof(ExportSupportingDocumentsFieldsControl.ItemNumberCalcEdit);
		}
	}
}
