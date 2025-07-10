using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	[TestedType(typeof(ExitSummaryMainPanelLayout))]
	public sealed class ExitSummaryMainPanelLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExitSummaryMainPanelLayoutBuilder<Business.CusExitControlHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (ExitSummaryMainPanelControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
				yield return (ExitSummaryMainPanelControlBag.Instance.HeaderCustomsOfficeCodeFindBox, ControlWidthClass.Auto);
				yield return (ExitSummaryMainPanelControlBag.Instance.HeaderArrivalNotificationDateDateEdit, ControlWidthClass.Auto);
				yield return (ExitSummaryMainPanelControlBag.Instance.HeaderArrivalNotificationPlaceTextBox, ControlWidthClass.Auto);
				yield return (ExitSummaryMainPanelControlBag.Instance.HeaderExitDateDateEdit, ControlWidthClass.Auto);
				yield return (ExitSummaryMainPanelControlBag.Instance.HeaderTransportIdTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ExitSummaryMainPanelControlBag.Instance.AgentOrgAddressControl, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (ExitSummaryMainPanelControlBag.Instance.HeaderCarrierOrgAddressControl, ControlWidthClass.Auto);
			}
		}
	}
}
