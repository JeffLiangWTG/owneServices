using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(ExitSummaryMainPanelLayout))]
	public sealed class ExitSummaryMainPanelLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
				yield return FourthColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.GUI.PlugIn.ExitSummaryMainPanelLayoutBuilder<Business.Declaration.CusExitControlHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EU.GUI.PlugIn.ExitSummaryMainPanelControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
				yield return (EU.GUI.PlugIn.ExitSummaryMainPanelControlBag.Instance.HeaderCustomsOfficeCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.GUI.PlugIn.ExitSummaryMainPanelControlBag.Instance.HeaderArrivalNotificationDateDateEdit, ControlWidthClass.Auto);
				yield return (EU.GUI.PlugIn.ExitSummaryMainPanelControlBag.Instance.HeaderArrivalNotificationPlaceTextBox, ControlWidthClass.Auto);
				yield return (EU.GUI.PlugIn.ExitSummaryMainPanelControlBag.Instance.HeaderExitDateDateEdit, ControlWidthClass.Auto);
				yield return (EU.GUI.PlugIn.ExitSummaryMainPanelControlBag.Instance.HeaderTransportIdTextBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ExitSummaryMainPanelControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Medium);
				yield return (ExitSummaryMainPanelControlBag.Instance.CertificateDropEdit, ControlWidthClass.Medium);
				yield return (ExitSummaryMainPanelControlBag.Instance.DeclEmailAddrTextBox, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (EU.GUI.PlugIn.ExitSummaryMainPanelControlBag.Instance.AgentOrgAddressControl, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FourthColumnControls
		{
			get
			{
				yield return (EU.GUI.PlugIn.ExitSummaryMainPanelControlBag.Instance.HeaderCarrierOrgAddressControl, ControlWidthClass.Medium);
			}
		}
	}
}
