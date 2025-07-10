using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	[TestedType(typeof(HeaderDetailsLayout))]
	sealed class HeaderDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new HeaderDetailsLayoutBuilder<Business.CusExitHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (HeaderDetailsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Long);
				yield return (HeaderDetailsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Long);
				yield return (HeaderDetailsControlBag.Instance.ExporterOrgAddressControl, ControlWidthClass.Long);
				yield return (HeaderDetailsControlBag.Instance.CarrierAddressWithContactControl, ControlWidthClass.Auto);
			}
		}
	}
}
