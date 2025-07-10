using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(PreviousDocumentsFieldsControlBag))]
	sealed class PreviousDocumentsFieldsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(PreviousDocumentsFieldsControlBag.CodeDropEdit);
				yield return nameof(PreviousDocumentsFieldsControlBag.ReferenceTextBox);
				yield return nameof(PreviousDocumentsFieldsControlBag.ProcedureDropEdit);
				yield return nameof(PreviousDocumentsFieldsControlBag.Reference2TextBox);
				yield return nameof(PreviousDocumentsFieldsControlBag.IssueDateEdit);
				yield return nameof(PreviousDocumentsFieldsControlBag.LineNoCalcEdit);
				yield return nameof(PreviousDocumentsFieldsControlBag.CustomsOfficeCodeFindBox);
				yield return nameof(PreviousDocumentsFieldsControlBag.QuantityCalcDropEdit);
				yield return nameof(PreviousDocumentsFieldsControlBag.Quantity2CalcDropEdit);
				yield return nameof(PreviousDocumentsFieldsControlBag.Quantity3CalcDropEdit);
				yield return nameof(PreviousDocumentsFieldsControlBag.PackageQuantityCalcDropEdit);
				yield return nameof(PreviousDocumentsFieldsControlBag.ItemNumberCalcEdit);
				yield return nameof(PreviousDocumentsFieldsControlBag.SubTypeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => PreviousDocumentsFieldsControlBag.Instance;
	}
}
