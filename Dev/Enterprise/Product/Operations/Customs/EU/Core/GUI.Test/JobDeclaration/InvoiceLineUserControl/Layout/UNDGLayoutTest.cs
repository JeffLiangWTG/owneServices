using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(UNDGLayout))]
	sealed class UNDGLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new UNDGLayoutBuilder<JobDeclaration>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (UNDGUserControlBag.Instance.DGGuidFindBox, ControlWidthClass.Long);
				yield return (UNDGUserControlBag.Instance.FlashpointUserControl, ControlWidthClass.Long);
				yield return (UNDGUserControlBag.Instance.DGLinkLabel, ControlWidthClass.Long);
			}
		}
	}
}
