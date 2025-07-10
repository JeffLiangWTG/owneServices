using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7BillPartiesControlBag : ControlBag
	{
		EUH7BillPartiesControlBag()
		{
			ImporterSeparatorUserControl = RegisterControl(nameof(EUH7BillPartiesUserControl.ImporterSeparatorUserControl));
			ExporterSeparatorUserControl = RegisterControl(nameof(EUH7BillPartiesUserControl.ExporterSeparatorUserControl));
			SellerSeparatorUserControl = RegisterControl(nameof(EUH7BillPartiesUserControl.SellerSeparatorUserControl));
			IdentificationNoTextBox = RegisterControl(nameof(EUH7BillPartiesUserControl.IdentificationNoTextBox));
			ImporterIdentificationTypeDropEdit =
				RegisterControl(nameof(EUH7BillPartiesUserControl.ImporterIdentificationTypeDropEdit));
			CountryOfImportDropEdit = RegisterControl(nameof(EUH7BillPartiesUserControl.CountryOfImportDropEdit));
			CountryOfExportDropEdit = RegisterControl(nameof(EUH7BillPartiesUserControl.CountryOfExportDropEdit));
			CountryOfSellerDropEdit = RegisterControl(nameof(EUH7BillPartiesUserControl.CountryOfSellerDropEdit));
		}

		public ControlReference ImporterSeparatorUserControl { get; }
		public ControlReference ExporterSeparatorUserControl { get; }
		public ControlReference SellerSeparatorUserControl { get; }
		public ControlReference IdentificationNoTextBox { get; }
		public ControlReference ImporterIdentificationTypeDropEdit { get; }
		public ControlReference CountryOfImportDropEdit { get; }
		public ControlReference CountryOfExportDropEdit { get; }
		public ControlReference CountryOfSellerDropEdit { get; }

		public static EUH7BillPartiesControlBag Instance => billPartiesControlBag.Value;

		protected override Control CreateTemplate() => new EUH7BillPartiesUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<EUH7BillPartiesControlBag> billPartiesControlBag = new Lazy<EUH7BillPartiesControlBag>(() => new EUH7BillPartiesControlBag());
	}
}
