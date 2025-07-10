using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ExtendedHoursRequestViewLayout))]
	sealed class ExtendedHoursRequestViewLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CusMiscRequestHeaderViewLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CusMiscRequestHeaderViewControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (CusMiscRequestHeaderViewControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (CusMiscRequestHeaderViewControlBag.Instance.CustomsDivisionCodeFindBox, ControlWidthClass.Long);
				yield return (CusMiscRequestHeaderViewControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Long);
				yield return (CusMiscRequestHeaderViewControlBag.Instance.RequestDetailsTextBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CusMiscRequestHeaderViewControlBag.Instance.ApplicationNumberTextBox, ControlWidthClass.Auto);
				yield return (CusMiscRequestHeaderViewControlBag.Instance.StatusDropEdit, ControlWidthClass.Long);
				yield return (CusMiscRequestHeaderViewControlBag.Instance.RequestDateEdit, ControlWidthClass.Auto);
				yield return (CusMiscRequestHeaderViewControlBag.Instance.EntryCountCalcEdit, ControlWidthClass.Auto);
			}
		}
	}
}
