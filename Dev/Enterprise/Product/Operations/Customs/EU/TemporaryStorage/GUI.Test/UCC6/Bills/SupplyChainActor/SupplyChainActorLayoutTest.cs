using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(SupplyChainActorLayout))]
	sealed class SupplyChainActorLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 1;

		protected override Type ExpectedGridUserControlType => typeof(SupplyChainActorReferencesUserControl);

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupplyChainActorLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (SupplyChainActorControlBag.Instance.RoleDropEdit, ControlWidthClass.Auto);
				yield return (SupplyChainActorControlBag.Instance.OwnerOrganisationFindBox, ControlWidthClass.Auto);
				yield return (SupplyChainActorControlBag.Instance.ReferenceNumberTextBox, ControlWidthClass.Auto);
			}
		}
	}
}
