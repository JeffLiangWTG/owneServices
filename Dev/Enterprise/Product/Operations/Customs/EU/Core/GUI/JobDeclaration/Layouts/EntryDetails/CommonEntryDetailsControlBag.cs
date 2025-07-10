using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class CommonEntryDetailsControlBag : ControlBag
	{
		public static CommonEntryDetailsControlBag Instance => instance ?? (instance = new CommonEntryDetailsControlBag());

		[ThreadStatic]
		static CommonEntryDetailsControlBag instance;

		CommonEntryDetailsControlBag()
		{
			TotalsLabel = RegisterControl(nameof(CommonEntryDetailsUserControl.TotalsLabel));
			CustomsLabel = RegisterControl(nameof(CommonEntryDetailsUserControl.CustomsLabel));
			NoPacksCalcEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.NoPacksCalcEdit));
			GrossWeightCalcDropEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.GrossWeightCalcDropEdit));
			NetWeightCalcDropEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.NetWeightCalcDropEdit));
			CustomsQuantityCalcDropEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.CustomsQuantityCalcDropEdit));
			DutyCalcEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.DutyCalcEdit));
			VatCalcEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.VatCalcEdit));
			EntryLinesCountCalcEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.EntryLinesCountCalcEdit));
			ReferenceNumberTextBox = RegisterControl(nameof(CommonEntryDetailsUserControl.ReferenceNumberTextBox));
			SubmittedDateDateEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.SubmittedDateDateEdit));
			MRNTextBox = RegisterControl(nameof(CommonEntryDetailsUserControl.MRNTextBox));
			ReleaseDateDateEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.ReleaseDateDateEdit));
			EntryStatusDropEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.EntryStatusDropEdit));
			AcceptanceDateDateEdit = RegisterControl(nameof(CommonEntryDetailsUserControl.AcceptanceDateDateEdit));
		}

		protected override Control CreateTemplate() => new CommonEntryDetailsUserControl();

		public ControlReference TotalsLabel { get; }
		public ControlReference CustomsLabel { get; }
		public ControlReference NoPacksCalcEdit { get; }
		public ControlReference GrossWeightCalcDropEdit { get; }
		public ControlReference NetWeightCalcDropEdit { get; }
		public ControlReference CustomsQuantityCalcDropEdit { get; }
		public ControlReference DutyCalcEdit { get; }
		public ControlReference VatCalcEdit { get; }
		public ControlReference EntryLinesCountCalcEdit { get; }
		public ControlReference ReferenceNumberTextBox { get; }
		public ControlReference SubmittedDateDateEdit { get; }
		public ControlReference MRNTextBox { get; }
		public ControlReference ReleaseDateDateEdit { get; }
		public ControlReference EntryStatusDropEdit { get; }
		public ControlReference AcceptanceDateDateEdit { get; }
	}
}
