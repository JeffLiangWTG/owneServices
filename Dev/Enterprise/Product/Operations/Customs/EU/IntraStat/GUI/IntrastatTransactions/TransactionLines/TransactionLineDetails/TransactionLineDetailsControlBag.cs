using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	public class TransactionLineDetailsControlBag : ControlBag
	{
		TransactionLineDetailsControlBag()
		{
			CountryOfOriginDropEdit = RegisterControl(nameof(TransactionLineDetailsUserControl.CountryOfOriginDropEdit));
			MassDropEdit = RegisterControl(nameof(TransactionLineDetailsUserControl.MassDropEdit));
			StatisticalValueDropEdit = RegisterControl(nameof(TransactionLineDetailsUserControl.StatisticalValueDropEdit));
			InvoiceValueDropEdit = RegisterControl(nameof(TransactionLineDetailsUserControl.InvoiceValueDropEdit));
			SupplementaryUnitsCalcDropEdit = RegisterControl(nameof(TransactionLineDetailsUserControl.SupplementaryUnitsCalcDropEdit));
			RegionDropEdit = RegisterControl(nameof(TransactionLineDetailsUserControl.RegionDropEdit));
			DescriptionOfGoodsTextBox = RegisterControl(nameof(TransactionLineDetailsUserControl.DescriptionOfGoodsTextBox));
			TariffFindBox = RegisterControl(nameof(TransactionLineDetailsUserControl.TariffFindBox));
		}

		public ControlReference RegionDropEdit { get; set; }

		public ControlReference SupplementaryUnitsCalcDropEdit { get; set; }

		public ControlReference InvoiceValueDropEdit { get; set; }

		public ControlReference DescriptionOfGoodsTextBox { get; set; }

		public ControlReference TariffFindBox { get; set; }

		public ControlReference CountryOfOriginDropEdit { get; }

		public ControlReference MassDropEdit { get; }

		public ControlReference StatisticalValueDropEdit { get; }

		public static TransactionLineDetailsControlBag Instance => instance ??= new TransactionLineDetailsControlBag();

		[ThreadStatic]
		static TransactionLineDetailsControlBag instance;

		protected override Control CreateTemplate() => new TransactionLineDetailsUserControl();
	}
}
