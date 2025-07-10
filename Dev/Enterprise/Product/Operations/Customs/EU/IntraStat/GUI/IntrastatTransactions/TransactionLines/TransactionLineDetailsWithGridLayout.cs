using System;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public class TransactionLineDetailsWithGridLayout : IPanelLayoutWithGridProvider
	{
		public TransactionLineDetailsWithGridLayout()
		{
			Layout = CreateDetailsLayout();
		}

		public PanelLayout Layout { get; }

		static PanelLayout CreateDetailsLayout()
		{
			var builder = new TransactionLineDetailsLayoutBuilder<CusIntrastatLine>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.DescriptionOfGoodsTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.TariffFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.RegionDropEdit, ControlWidthClass.Long);
			builder.AddColumn();
			builder.Add(commonBag.InvoiceValueDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.StatisticalValueDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.MassDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.SupplementaryUnitsCalcDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}

		Type IPanelLayoutWithGridProvider.GridUserControlType => typeof(TransactionLinesGridUserControl);
		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
