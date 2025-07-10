using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public class EUICS2BillControlBag : ControlBag
	{
		public static EUICS2BillControlBag Instance => billControlBag.Value;

		public EUICS2BillControlBag()
		{
			TransportDocumentTypeDropEdit = RegisterControl(nameof(EUICS2BillFieldsUserControl.TransportDocumentTypeDropEdit));
			FreightValueAndCurrencyCalcFindBox = RegisterControl(nameof(EUICS2BillFieldsUserControl.FreightValueAndCurrencyCalcFindBox));
			ReceptacleIdTextBox = RegisterControl(nameof(EUICS2BillFieldsUserControl.ReceptacleIdTextBox));
		}

		public ControlReference TransportDocumentTypeDropEdit { get; }

		public ControlReference FreightValueAndCurrencyCalcFindBox { get; }

		public ControlReference ReceptacleIdTextBox { get; }

		protected override Control CreateTemplate() => new EUICS2BillFieldsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<EUICS2BillControlBag> billControlBag = new Lazy<EUICS2BillControlBag>(() => new EUICS2BillControlBag());
	}
}
