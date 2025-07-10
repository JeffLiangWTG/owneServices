using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(InAndOutwardProcessingFieldsControlBag))]
sealed class InAndOutwardProcessingFieldsControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => InAndOutwardProcessingFieldsControlBag.Instance;

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(InAndOutwardProcessingFieldsControlBag.SubTypeDropEdit);
			yield return nameof(InAndOutwardProcessingFieldsControlBag.CodeDropEdit);
			yield return nameof(InAndOutwardProcessingFieldsControlBag.ProcedureDropEdit);
			yield return nameof(InAndOutwardProcessingFieldsControlBag.IssuerTypeDropEdit);
			yield return nameof(InAndOutwardProcessingFieldsControlBag.StatusCheckBox);
			yield return nameof(InAndOutwardProcessingFieldsControlBag.DescriptionTextBox);
			yield return nameof(InAndOutwardProcessingFieldsControlBag.CustomsOfficeCodeFindBox);
		}
	}
}
