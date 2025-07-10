using System;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class SWProductionDetailsLayout : IPanelLayoutWithGridProvider
{
	public SWProductionDetailsLayout()
	{
		swProductionLayout = CreateLayout();
	}

	public Type GridUserControlType => typeof(SWProductionDetailsUserControl);

	public PanelLayout Layout => swProductionLayout;

	readonly PanelLayout swProductionLayout;

	PanelLayout CreateLayout()
	{
		var builder = new SWProductionDetailsLayoutBuilder<SWProduction>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.BatchIDTextBox, widthClass: ControlWidthClass.Long);
		builder.Add(commonBag.ManufacturingDateEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(commonBag.ExpiryDateEdit, widthClass: ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.BatchQuantityDropEdit, widthClass: ControlWidthClass.Auto);
		builder.Add(commonBag.BestBeforeDateTimeOffsetEdit, widthClass: ControlWidthClass.Auto);

		return builder.Build();
	}
}
