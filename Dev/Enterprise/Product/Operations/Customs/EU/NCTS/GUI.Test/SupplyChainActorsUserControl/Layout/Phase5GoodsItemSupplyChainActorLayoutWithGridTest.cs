using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemSupplyChainActorLayoutWithGrid))]
	sealed class Phase5GoodsItemSupplyChainActorLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(GoodsItemSupplyChainActorsGridUserControl);

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupplyChainActorLayoutBuilder<CusSupplyChainActorReference>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (SupplyChainActorControlBag.Instance.RoleDropEdit, ControlWidthClass.Auto);
				yield return (SupplyChainActorControlBag.Instance.ReferenceTextBox, ControlWidthClass.Long);
				yield return (SupplyChainActorControlBag.Instance.OwnerOrganisationFindBox, ControlWidthClass.Auto);
			}
		}
	}
}
