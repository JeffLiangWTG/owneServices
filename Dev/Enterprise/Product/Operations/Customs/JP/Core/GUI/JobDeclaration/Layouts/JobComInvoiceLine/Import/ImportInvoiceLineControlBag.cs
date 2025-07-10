using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public class ImportInvoiceLineControlBag : ControlBag
	{
		ImportInvoiceLineControlBag()
		{
			TariffFindBox = RegisterControl(nameof(ImportInvoiceLineTemplate.TariffFindBox));
			DutyRateTextBox = RegisterControl(nameof(ImportInvoiceLineTemplate.DutyRateTextBox));
			ProcedureTextBox = RegisterControl(nameof(ImportInvoiceLineTemplate.ProcedureTextBox));
			StorageTypeDropEdit = RegisterControl(nameof(ImportInvoiceLineTemplate.StorageTypeDropEdit));
			CertificateOfOriginPanel = RegisterControl(nameof(ImportInvoiceLineTemplate.CertificateOfOriginPanel));
		}

		public ControlReference TariffFindBox { get; }
		public ControlReference DutyRateTextBox { get; }
		public ControlReference ProcedureTextBox { get; }
		public ControlReference StorageTypeDropEdit { get; }
		public ControlReference CertificateOfOriginPanel { get; }

		public static ImportInvoiceLineControlBag Instance => instance ??= new ImportInvoiceLineControlBag();

		[ThreadStatic]
		static ImportInvoiceLineControlBag instance;

		protected override Control CreateTemplate() => new ImportInvoiceLineTemplate();
	}
}
