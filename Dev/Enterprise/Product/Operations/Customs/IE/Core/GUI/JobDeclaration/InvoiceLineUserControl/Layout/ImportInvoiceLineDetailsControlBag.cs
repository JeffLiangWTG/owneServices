using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class ImportInvoiceLineDetailsControlBag : ControlBag
	{
		public static ImportInvoiceLineDetailsControlBag Instance => importInvoiceLineDetailsControlBag.Value;

		ImportInvoiceLineDetailsControlBag()
		{
			CountryOfSupplyCodeFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.CountryOfSupplyCodeFindBox));
			CountryOfOriginCodeFindBox = RegisterControl(nameof(ImportInvoiceLineDetailsUserControl.CountryOfOriginCodeFindBox));
		}

		public ControlReference CountryOfSupplyCodeFindBox { get; }
		public ControlReference CountryOfOriginCodeFindBox { get; }

		protected override Control CreateTemplate() => new ImportInvoiceLineDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<ImportInvoiceLineDetailsControlBag> importInvoiceLineDetailsControlBag = new Lazy<ImportInvoiceLineDetailsControlBag>(() => new ImportInvoiceLineDetailsControlBag());
	}
}
