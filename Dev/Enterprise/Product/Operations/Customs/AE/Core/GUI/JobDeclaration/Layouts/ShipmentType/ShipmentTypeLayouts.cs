using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI;

public sealed class ShipmentTypeLayouts : IPanelLayoutProvider
{
	public PanelLayout Layout { get; }

	public ShipmentTypeLayouts()
	{
		Layout = CreateShipmentTypeLayout();
	}

	PanelLayout CreateShipmentTypeLayout()
	{
		var builder = new ShipmentTypeLayoutBuilder<JobDeclaration>();
		var commonBag = builder.CommonBag;
		var aeBag = ShipmentTypeControlBag.Instance;
		builder.AddControlBag(aeBag);

		builder.AddColumn();
		builder.Add(commonBag.MessageTypeDropEdit, ControlWidthClass.CustomWidth);
		builder.Add(commonBag.TransportModeDropEdit, ControlWidthClass.CustomWidth);
		builder.Add(aeBag.ExitPointDropEdit, ControlWidthClass.CustomWidth);
		builder.Add(aeBag.ClearanceLocationDropEdit, ControlWidthClass.CustomWidth);
		builder.Add(commonBag.ServiceLevelCodeFindBox, ControlWidthClass.CustomWidth);
		builder.Add(commonBag.ApplicationCodeDropEdit, ControlWidthClass.CustomWidth);

		return builder.Build();
	}
}
