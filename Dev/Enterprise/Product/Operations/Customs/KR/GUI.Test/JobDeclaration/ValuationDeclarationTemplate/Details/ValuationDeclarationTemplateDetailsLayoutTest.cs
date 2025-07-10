using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ValuationDeclarationTemplateDetailsLayout))]
	sealed class ValuationDeclarationTemplateDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ValuationDeclarationTemplateLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (ValuationDeclarationTemplateDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Long);
				yield return (ValuationDeclarationTemplateDetailsControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (ValuationDeclarationTemplateDetailsControlBag.Instance.DepartmentCodeFindBox, ControlWidthClass.Long);
				yield return (ValuationDeclarationTemplateDetailsControlBag.Instance.PONoTextBox, ControlWidthClass.Long);
				yield return (ValuationDeclarationTemplateDetailsControlBag.Instance.PODateEdit, ControlWidthClass.Auto);
				yield return (ValuationDeclarationTemplateDetailsControlBag.Instance.ServiceCodeFindBox, ControlWidthClass.Long);
			}
		}
	}
}
