using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public class TransactionDetailsControlBag : ControlBag
	{
		TransactionDetailsControlBag()
		{
			CountryOfSupplyDropEdit = RegisterControl(nameof(TransactionDetailsUserControl.CountryOfSupplyDropEdit));
			CountryOfReceiptDropEdit = RegisterControl(nameof(TransactionDetailsUserControl.CountryOfReceiptDropEdit));
			TransactionDateEdit = RegisterControl(nameof(TransactionDetailsUserControl.TransactionDateEdit));
			ConsigneeVATTextBox = RegisterControl(nameof(TransactionDetailsUserControl.ConsigneeVATTextBox));
			SupplierVATTextBox = RegisterControl(nameof(TransactionDetailsUserControl.SupplierVATTextBox));
			TradersReferenceTextBox = RegisterControl(nameof(TransactionDetailsUserControl.TradersReferenceTextBox));
			NatureOfTransactionDropEdit = RegisterControl(nameof(TransactionDetailsUserControl.NatureOfTransactionDropEdit));
			ModeOfTransportDropEdit = RegisterControl(nameof(TransactionDetailsUserControl.ModeOfTransportDropEdit));
			IncoTermDropEdit = RegisterControl(nameof(TransactionDetailsUserControl.IncoTermDropEdit));
			SupplierNameTextBox = RegisterControl(nameof(TransactionDetailsUserControl.SupplierNameTextBox));
			ConsigneeNameTextBox = RegisterControl(nameof(TransactionDetailsUserControl.ConsigneeNameTextBox));
		}

		public ControlReference SupplierNameTextBox { get; set; }

		public ControlReference ConsigneeNameTextBox { get; set; }

		public ControlReference TradersReferenceTextBox { get; set; }

		public ControlReference ConsigneeVATTextBox { get; set; }

		public ControlReference SupplierVATTextBox { get; set; }

		public ControlReference NatureOfTransactionDropEdit { get; set; }

		public ControlReference ModeOfTransportDropEdit { get; set; }

		public ControlReference IncoTermDropEdit { get; set; }

		public ControlReference CountryOfSupplyDropEdit { get; }

		public ControlReference CountryOfReceiptDropEdit { get; }

		public ControlReference TransactionDateEdit { get; }

		public static TransactionDetailsControlBag Instance => instance ??= new TransactionDetailsControlBag();

		[ThreadStatic]
		static TransactionDetailsControlBag instance;

		protected override Control CreateTemplate() => new TransactionDetailsUserControl();
	}
}
