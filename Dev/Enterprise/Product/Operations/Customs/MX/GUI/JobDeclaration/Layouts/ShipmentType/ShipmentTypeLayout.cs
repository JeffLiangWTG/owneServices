using Enterprise.Customs.GUI;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI
{
	public sealed class ShipmentTypeLayout : IPanelLayoutProvider
	{
		PanelLayout ShipmentType { get; }

		PanelLayout IPanelLayoutProvider.Layout => ShipmentType;

		public ShipmentTypeLayout()
		{
			ShipmentType = CreateShipmentTypeLayout();
		}

		PanelLayout CreateShipmentTypeLayout()
		{
			var builder = new ShipmentTypeLayoutBuilder<JobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.MessageSubTypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsProfileDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
