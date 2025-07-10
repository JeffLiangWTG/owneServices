using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class JobComInvoiceHeaderControlBag : ControlBag
	{
		JobComInvoiceHeaderControlBag()
		{
			GrossWeightCalcDropEdit = RegisterControl(nameof(GrossWeightCalcDropEdit));
			NetWeightCalcDropEdit = RegisterControl(nameof(NetWeightCalcDropEdit));
			InvoiceAmountConvertToLocalCurrencyControl = RegisterControl(nameof(InvoiceAmountConvertToLocalCurrencyControl));
			IncoTermsUserControl = RegisterControl(nameof(IncoTermsUserControl));
		}

		public ControlReference GrossWeightCalcDropEdit { get; }

		public ControlReference NetWeightCalcDropEdit { get; }

		public ControlReference InvoiceAmountConvertToLocalCurrencyControl { get; }

		public ControlReference IncoTermsUserControl { get; }

		public static JobComInvoiceHeaderControlBag Instance => instance ??= new JobComInvoiceHeaderControlBag();

		[ThreadStatic]
		static JobComInvoiceHeaderControlBag instance;

		protected override Control CreateTemplate() => new JobComInvoiceHeaderTemplate();
	}
}
