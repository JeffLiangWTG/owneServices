using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(CertificateOfOriginDetailsLayout))]
	sealed class CertificateOfOriginDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SEDDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (SEDDetailsControlBag.Instance.GoodsOriginCodeFindBox, ControlWidthClass.Auto);
				yield return (SEDDetailsControlBag.Instance.COODeterminationRuleDropEdit, ControlWidthClass.Auto);
				yield return (SEDDetailsControlBag.Instance.COOIssueStatusDropEdit, ControlWidthClass.Auto);
				yield return (SEDDetailsControlBag.Instance.COOLabelLocationDropEdit, ControlWidthClass.Auto);
			}
		}
	}
}
