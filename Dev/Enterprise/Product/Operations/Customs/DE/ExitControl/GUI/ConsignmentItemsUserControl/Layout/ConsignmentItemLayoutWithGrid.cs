using System;
using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.ExitControl.GUI
{
	public sealed class ConsignmentItemLayoutWithGrid : IPanelLayoutWithGridProvider
	{
		public ConsignmentItemLayoutWithGrid()
		{
			Layout = CreateConsignmentItemsLayoutWithGrid();
		}

		PanelLayout Layout { get; }

		PanelLayout CreateConsignmentItemsLayoutWithGrid()
		{
			var builder = new EU.ExitControl.GUI.ConsignmentItemLayoutBuilder<CusExitConsignmentItem>();
			var euBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(euBag.ConsignmentItemPackingDetailsUserControl, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(ConsignmentItemsGridUserControl);
	}
}
