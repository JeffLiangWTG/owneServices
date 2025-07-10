using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class ShipmentTypeLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout { get; }

		public ShipmentTypeLayout()
		{
			Layout = CreateShipmentTypeLayout();
		}

		static PanelLayout CreateShipmentTypeLayout()
		{
			var builder = new ShipmentTypeLayoutBuilder<JobDeclaration>();

			var commonBag = builder.CommonBag;
			var krBag = ShipmentTypeControlBag.Instance;
			builder.AddControlBag(krBag);

			builder.AddColumn();
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(krBag.TransactionDropEdit, ControlWidthClass.Long);
			builder.Add(krBag.DeclarationTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.MessageSubTypeDropEdit, ControlWidthClass.Long);
			builder.Add(krBag.TransactionTypeDropEdit, ControlWidthClass.Long);
			builder.Add(krBag.ExporterTypeDropEdit, ControlWidthClass.Long);
			builder.Add(krBag.PaymentTypeDropEdit, ControlWidthClass.Long);
			builder.Add(krBag.PlanTypeDropEdit, ControlWidthClass.Long);
			builder.Add(krBag.ImporterTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
