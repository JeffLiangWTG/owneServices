using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayout))]
	sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstAndOnlyColumnControls;
			}
		}

		protected override int ControlBagCount => 1;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder<JobDeclaration>();

		public PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new ShipmentTypeLayout()).Layout);
		PanelLayout layout;

		IEnumerable<(ControlReference, ControlWidthClass)> FirstAndOnlyColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.CustomsProfileDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
