using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.H7.GUI.Bill
{
	public class H7BillPartiesControlBag : ControlBag
	{
		H7BillPartiesControlBag()
		{
			VatNumberTextBox = RegisterControl(nameof(H7BillPartiesUserControl.VatNumberTextBox));
			PostponedVatAccountingCheckBox = RegisterControl(nameof(H7BillPartiesUserControl.PostponedVatAccountingCheckBox));
		}

		public ControlReference VatNumberTextBox { get; }
		public ControlReference PostponedVatAccountingCheckBox { get; }

		public static H7BillPartiesControlBag Instance => billPartiesControlBag.Value;

		protected override Control CreateTemplate() => new H7BillPartiesUserControl();

		[ThreadSafe]
		readonly static Lazy<H7BillPartiesControlBag> billPartiesControlBag = new Lazy<H7BillPartiesControlBag>(() => new H7BillPartiesControlBag());
	}
}
