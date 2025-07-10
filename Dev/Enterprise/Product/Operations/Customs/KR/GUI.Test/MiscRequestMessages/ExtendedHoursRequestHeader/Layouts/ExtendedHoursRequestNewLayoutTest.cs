using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI
{
	[TestedType(typeof(ExtendedHoursRequestNewLayout))]
	sealed class ExtendedHoursRequestNewLayoutTest : LayoutsAbstractTest
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExtendedHoursRequestNewLayoutsBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (ExtendedHoursRequestNewControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (ExtendedHoursRequestNewControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (ExtendedHoursRequestNewControlBag.Instance.CustomsDivisionCodeFindBox, ControlWidthClass.Long);
				yield return (ExtendedHoursRequestNewControlBag.Instance.RequestPeriodStartDateEdit, ControlWidthClass.Long);
				yield return (ExtendedHoursRequestNewControlBag.Instance.RequestPeriodEndDateEdit, ControlWidthClass.Long);
				yield return (ExtendedHoursRequestNewControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ExtendedHoursRequestNewControlBag.Instance.RequestReasonTextBox, ControlWidthClass.Long);
			}
		}
	}
}
