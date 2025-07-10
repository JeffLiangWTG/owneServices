using System.Collections.Generic;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ShipmentTypeLayout))]
	sealed class ShipmentTypeLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;
		protected override bool ShouldAssertThatIncludedControlsAreSorted => false;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ShipmentTypeLayoutBuilder<JobDeclaration>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageTypeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.TransportModeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ContainerModeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.TransactionDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.DeclarationTypeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.MessageSubTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.TransactionTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.ExporterTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.PaymentTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.PlanTypeDropEdit, ControlWidthClass.Long);
				yield return (ShipmentTypeControlBag.Instance.ImporterTypeDropEdit, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ServiceLevelCodeFindBox, ControlWidthClass.Long);
				yield return (Customs.GUI.ShipmentTypeControlBag.Instance.ApplicationCodeDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
