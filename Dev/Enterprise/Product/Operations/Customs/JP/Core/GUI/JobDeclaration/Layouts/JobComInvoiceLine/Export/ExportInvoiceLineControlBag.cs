using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class ExportInvoiceLineControlBag : ControlBag
	{
		ExportInvoiceLineControlBag()
		{
			TariffFindBox = RegisterControl(nameof(ExportInvoiceLineTemplate.TariffFindBox));
		}

		public ControlReference TariffFindBox { get; }

		public static ExportInvoiceLineControlBag Instance => instance ??= new ExportInvoiceLineControlBag();

		[ThreadStatic]
		static ExportInvoiceLineControlBag instance;

		protected override Control CreateTemplate() => new ExportInvoiceLineTemplate();
	}
}
