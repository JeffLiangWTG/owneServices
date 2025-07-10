using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public class TransactionDetailsLayout : IPanelLayoutProvider
	{
		public TransactionDetailsLayout()
		{
			Layout = CreateDetailsLayout();
		}

		public PanelLayout Layout { get; }

		PanelLayout CreateDetailsLayout()
		{
			var builder = new TransactionDetailsLayoutBuilder<CusIntrastatHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.SupplierNameTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.SupplierVATTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeNameTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ConsigneeVATTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfSupplyDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfReceiptDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TransactionDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NatureOfTransactionDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ModeOfTransportDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TradersReferenceTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.IncoTermDropEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
