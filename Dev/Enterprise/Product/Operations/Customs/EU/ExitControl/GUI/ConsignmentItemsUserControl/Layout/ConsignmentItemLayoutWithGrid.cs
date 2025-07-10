using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
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
			var builder = new ConsignmentItemLayoutBuilder<Business.CusExitConsignmentItem>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.ConsignmentItemPackingDetailsUserControl, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(ConsignmentItemsGridUserControl);
	}
}
