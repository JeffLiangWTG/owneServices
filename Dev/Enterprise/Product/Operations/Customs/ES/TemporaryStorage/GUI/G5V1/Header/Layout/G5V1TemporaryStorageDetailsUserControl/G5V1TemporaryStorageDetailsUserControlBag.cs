using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class G5V1TemporaryStorageDetailsUserControlBag : ControlBag
	{
		public static G5V1TemporaryStorageDetailsUserControlBag Instance => g5V1TemporaryStorageDetailsControlBag.Value;

		G5V1TemporaryStorageDetailsUserControlBag()
		{
			LRNTextBox = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.LRNTextBox));
			MRNTextBox = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.MRNTextBox));
			CustomsStatusDropEdit = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.CustomsStatusDropEdit));
			MessageStatusDropEdit = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.MessageStatusDropEdit));
			CircuitTextBox = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.CircuitTextBox));
			AcceptanceDateDateEdit = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.AcceptanceDateDateEdit));
			ClearanceNumberTextBox = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.ClearanceNumberTextBox));
			DsdtSdFormatHasUrlUserControl = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.DsdtSdFormatHasUrlUserControl));
			DsdtSdFormatNoUrlUserControl = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.DsdtSdFormatNoUrlUserControl));
			DsdtMrnBindingMemberUserControl = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.DsdtMrnBindingMemberUserControl));
			DsdtMrnNumberTextBox = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.DsdtMrnNumberTextBox));
			LAMEEntryNumberTextBox = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.LAMEEntryNumberTextBox));
			LAMEEntryDateDateEdit = RegisterControl(nameof(G5V1TemporaryStorageDetailsUserControl.LAMEEntryDateDateEdit));
		}

		public ControlReference LRNTextBox { get; }

		public ControlReference MRNTextBox { get; }

		public ControlReference CustomsStatusDropEdit { get; }

		public ControlReference MessageStatusDropEdit { get; }

		public ControlReference CircuitTextBox { get; }

		public ControlReference AcceptanceDateDateEdit { get; }

		public ControlReference ClearanceNumberTextBox { get; }

		public ControlReference DsdtSdFormatHasUrlUserControl { get; }

		public ControlReference DsdtSdFormatNoUrlUserControl { get; }

		public ControlReference DsdtMrnBindingMemberUserControl { get; }

		public ControlReference DsdtMrnNumberTextBox { get; }

		public ControlReference LAMEEntryNumberTextBox { get; }

		public ControlReference LAMEEntryDateDateEdit { get; }

		protected override Control CreateTemplate() => new G5V1TemporaryStorageDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<G5V1TemporaryStorageDetailsUserControlBag> g5V1TemporaryStorageDetailsControlBag = new Lazy<G5V1TemporaryStorageDetailsUserControlBag>(() => new G5V1TemporaryStorageDetailsUserControlBag());
	}
}
