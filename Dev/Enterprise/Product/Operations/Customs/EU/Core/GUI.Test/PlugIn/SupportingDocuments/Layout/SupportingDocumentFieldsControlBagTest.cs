using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(SupportingDocumentFieldsControlBag))]
	sealed class SupportingDocumentFieldsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SupportingDocumentFieldsControlBag.CodeCodeFindBox);
				yield return nameof(SupportingDocumentFieldsControlBag.ValueCalcEdit);
				yield return nameof(SupportingDocumentFieldsControlBag.DateOfIssueDateEdit);
				yield return nameof(SupportingDocumentFieldsControlBag.UnitOfQuantity2TextBox);
				yield return nameof(SupportingDocumentFieldsControlBag.ReferenceNumberTextBox);
				yield return nameof(SupportingDocumentFieldsControlBag.CurrencyCodeFindBox);
				yield return nameof(SupportingDocumentFieldsControlBag.UnitOfQuantityTextBox);
				yield return nameof(SupportingDocumentFieldsControlBag.UnitOfQuantityDropEdit);
				yield return nameof(SupportingDocumentFieldsControlBag.Quantity2CalcEdit);
				yield return nameof(SupportingDocumentFieldsControlBag.QuantityCalcEdit);
				yield return nameof(SupportingDocumentFieldsControlBag.QuantityCalcDropEdit);
				yield return nameof(SupportingDocumentFieldsControlBag.DateOfExpiryDateEdit);
				yield return nameof(SupportingDocumentFieldsControlBag.ReferenceNumberCodeFindBox);
				yield return nameof(SupportingDocumentFieldsControlBag.StatusDropEdit);
				yield return nameof(SupportingDocumentFieldsControlBag.AdditionalDescriptionTextBox);
				yield return nameof(SupportingDocumentFieldsControlBag.DocumentLineNoCalcEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SupportingDocumentFieldsControlBag.Instance;
	}
}
