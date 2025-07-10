using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class DeclarationDetailsControlBag : ControlBag
	{
		public DeclarationDetailsControlBag()
		{
			AcceptanceDateDateEdit = RegisterControl(nameof(DeclarationDetailsUserControl.AcceptanceDateDateEdit));
			CircuitTextBox = RegisterControl(nameof(DeclarationDetailsUserControl.CircuitTextBox));
			ClearanceNumberTextBox = RegisterControl(nameof(DeclarationDetailsUserControl.ClearanceNumberTextBox));
			ClearanceDateDateEdit = RegisterControl(nameof(DeclarationDetailsUserControl.ClearanceDateDateEdit));
			ArrivalLimitDateEdit = RegisterControl(nameof(DeclarationDetailsUserControl.ArrivalLimitDateEdit));
		}

		public static DeclarationDetailsControlBag Instance => instance ?? (instance = new DeclarationDetailsControlBag());

		[ThreadStatic]
		static DeclarationDetailsControlBag instance;

		public ControlReference AcceptanceDateDateEdit { get; }
		public ControlReference CircuitTextBox { get; }
		public ControlReference ClearanceNumberTextBox { get; }
		public ControlReference ClearanceDateDateEdit { get; }
		public ControlReference ArrivalLimitDateEdit { get; }

		protected override Control CreateTemplate() => new DeclarationDetailsUserControl();
	}
}
