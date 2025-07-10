using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationDeclarationTemplateDetailsControlBag))]
	sealed class ValuationDeclarationTemplateDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ValuationDeclarationTemplateDetailsControlBag.Instance.ValuationCodeDropEdit);
				yield return nameof(ValuationDeclarationTemplateDetailsControlBag.Instance.CustomsOfficeCodeFindBox);
				yield return nameof(ValuationDeclarationTemplateDetailsControlBag.Instance.DepartmentCodeFindBox);
				yield return nameof(ValuationDeclarationTemplateDetailsControlBag.Instance.PONoTextBox);
				yield return nameof(ValuationDeclarationTemplateDetailsControlBag.Instance.PODateEdit);
				yield return nameof(ValuationDeclarationTemplateDetailsControlBag.Instance.ServiceCodeFindBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ValuationDeclarationTemplateDetailsControlBag.Instance;
	}
}
