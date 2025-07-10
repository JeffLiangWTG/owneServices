using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ShipmentTypeControlBag : ControlBag
	{
		ShipmentTypeControlBag()
		{
			TransactionTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.TransactionTypeDropEdit));
			DeclarationTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.DeclarationTypeDropEdit));
			ExporterTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.ExporterTypeDropEdit));
			TransactionDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.TransactionDropEdit));
			PaymentTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.PaymentTypeDropEdit));
			PlanTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.PlanTypeDropEdit));
			ImporterTypeDropEdit = RegisterControl(nameof(ShipmentTypeUserControl.ImporterTypeDropEdit));
		}

		public static ShipmentTypeControlBag Instance => instance ?? (instance = new ShipmentTypeControlBag());

		[ThreadStatic]
		static ShipmentTypeControlBag instance;

		public ControlReference TransactionTypeDropEdit;
		public ControlReference DeclarationTypeDropEdit;
		public ControlReference ExporterTypeDropEdit;
		public ControlReference TransactionDropEdit;
		public ControlReference PaymentTypeDropEdit;
		public ControlReference PlanTypeDropEdit;
		public ControlReference ImporterTypeDropEdit;

		protected override Control CreateTemplate() => new ShipmentTypeUserControl();
	}
}
