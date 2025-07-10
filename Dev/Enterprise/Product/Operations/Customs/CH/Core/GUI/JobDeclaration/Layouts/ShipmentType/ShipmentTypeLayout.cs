using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class ShipmentTypeLayout : IPanelLayoutProvider
{
	PanelLayout ShipmentType { get; }

	public PanelLayout Layout => ShipmentType;

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
		builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ContainerModeDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.Long);
		builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.Long);

		builder.SetVisibility(commonBag.MessageSubTypeDropEdit, h => h.IsExportDeclarationActivation, h => h.JE_MessageTypeInfo);

		return builder.Build();
	}
}
