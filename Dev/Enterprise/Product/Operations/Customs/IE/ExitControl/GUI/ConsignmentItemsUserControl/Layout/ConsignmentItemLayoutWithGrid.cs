using System;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
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
			var builder = new ConsignmentItemLayoutBuilder<CusExitConsignmentItem>();
			var ieBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(ieBag.MainUserControl, ControlWidthClass.Auto);
			return builder.Build();
		}

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(ConsignmentItemsGridUserControl);
	}
}
