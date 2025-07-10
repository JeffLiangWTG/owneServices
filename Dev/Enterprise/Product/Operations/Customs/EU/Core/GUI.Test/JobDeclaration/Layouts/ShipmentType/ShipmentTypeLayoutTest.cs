using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayouts))]
	sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder<JobDeclaration>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.EntryStyleDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.InlandModeOfTransportDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.CTStatusIDDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.SpecificCircumstanceDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.IsHighValueOvrdCheckBox, ControlWidthClass.Auto);
			}
		}
	}
}
