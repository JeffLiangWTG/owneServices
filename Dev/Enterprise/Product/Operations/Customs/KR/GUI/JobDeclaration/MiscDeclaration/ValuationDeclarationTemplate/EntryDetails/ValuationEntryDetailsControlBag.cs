using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class ValuationEntryDetailsControlBag : ControlBag
	{
		public static ValuationEntryDetailsControlBag Instance => instance ?? (instance = new ValuationEntryDetailsControlBag());

		[ThreadStatic]
		static ValuationEntryDetailsControlBag instance;

		public ValuationEntryDetailsControlBag()
		{
			EntryNumberTextBox = RegisterControl(nameof(ValuationEntryDetailsUserControl.EntryNumberTextBox));
			MessageStatusDropEdit = RegisterControl(nameof(ValuationEntryDetailsUserControl.MessageStatusDropEdit));
			EntryStatusDropEdit = RegisterControl(nameof(ValuationEntryDetailsUserControl.EntryStatusDropEdit));
			EntrySubmittedDateDateEdit = RegisterControl(nameof(ValuationEntryDetailsUserControl.EntrySubmittedDateDateEdit));
			AcceptedDateDateEdit = RegisterControl(nameof(ValuationEntryDetailsUserControl.AcceptedDateDateEdit));
			ApprovalDateDateEdit = RegisterControl(nameof(ValuationEntryDetailsUserControl.ApprovalDateDateEdit));
			EffectiveToDateDateEdit = RegisterControl(nameof(ValuationEntryDetailsUserControl.EffectiveToDateDateEdit));
			ApprovalNumberTextBox = RegisterControl(nameof(ValuationEntryDetailsUserControl.ApprovalNumberTextBox));
			ResultReasonTextBox = RegisterControl(nameof(ValuationEntryDetailsUserControl.ResultReasonTextBox));
		}

		protected override Control CreateTemplate() => new ValuationEntryDetailsUserControl();
		
		public ControlReference EntryNumberTextBox { get; }
		public ControlReference MessageStatusDropEdit { get; }
		public ControlReference EntryStatusDropEdit { get; }
		public ControlReference EntrySubmittedDateDateEdit { get; }
		public ControlReference AcceptedDateDateEdit { get; }
		public ControlReference ApprovalDateDateEdit { get; }
		public ControlReference EffectiveToDateDateEdit { get; }
		public ControlReference ApprovalNumberTextBox { get; }
		public ControlReference ResultReasonTextBox { get; }
	}
}
