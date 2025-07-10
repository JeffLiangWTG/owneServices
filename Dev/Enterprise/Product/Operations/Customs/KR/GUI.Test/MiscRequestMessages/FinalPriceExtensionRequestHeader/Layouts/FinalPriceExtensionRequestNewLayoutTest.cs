using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI
{
	[TestedType(typeof(FinalPriceExtensionRequestNewLayout))]
	sealed class FinalPriceExtensionRequestNewLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new FinalPriceExtensionRequestNewLayoutsBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (FinalPriceExtensionRequestNewControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (FinalPriceExtensionRequestNewControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (FinalPriceExtensionRequestNewControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Long);
			}
		}
	}
}
