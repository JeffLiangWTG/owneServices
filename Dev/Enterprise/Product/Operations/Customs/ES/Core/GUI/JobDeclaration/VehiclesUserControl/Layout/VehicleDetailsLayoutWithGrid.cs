using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

class VehicleDetailsLayoutWithGrid : IPanelLayoutWithGridProvider
{
	public VehicleDetailsLayoutWithGrid()
	{
		Layout = CreateVehicleDetailsLayout();
	}
	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(VehiclesGridUserControl);

	PanelLayout CreateVehicleDetailsLayout()
	{
		var builder = new VehicleDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.VinTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.BrandTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.ModelTextBox, ControlWidthClass.Long);

		return builder.Build();
	}
}
