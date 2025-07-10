using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5TraderDetailsLayout))]
	sealed class Phase5TraderDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TraderDetailsLayoutBuilder<Business.NctsHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (TraderDetailsControlBag.Instance.PrincipalDocAddressControl, ControlWidthClass.LongControl);
				yield return (TraderDetailsControlBag.Instance.ConsignorDocAddressControl, ControlWidthClass.LongControl);
				yield return (TraderDetailsControlBag.Instance.ConsigneeDocAddressControl, ControlWidthClass.LongControl);
				yield return (TraderDetailsControlBag.Instance.RepresentativeDocAddressControl, ControlWidthClass.LongControl);
				yield return (TraderDetailsControlBag.Instance.FromWarehouseGroupBox, ControlWidthClass.LongControl);
			}
		}
	}
}
