using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class DeclarationDetailsControlBag : ControlBag
	{
		public DeclarationDetailsControlBag()
		{
			MrnTextBox = RegisterControl(nameof(DeclarationDetailsUserControl.MrnTextBox));
			DepartureStatusDropEdit = RegisterControl(nameof(DeclarationDetailsUserControl.DepartureStatusDropEdit));
			MessageStatusDropEdit = RegisterControl(nameof(DeclarationDetailsUserControl.MessageStatusDropEdit));
			PhaseStatusDropEdit = RegisterControl(nameof(DeclarationDetailsUserControl.PhaseStatusDropEdit));
			ReleaseDateEdit = RegisterControl(nameof(DeclarationDetailsUserControl.ReleaseDateEdit));
			AcceptanceDateEdit = RegisterControl(nameof(DeclarationDetailsUserControl.AcceptanceDateEdit));
			ActivationDeadlineDateEdit = RegisterControl(nameof(DeclarationDetailsUserControl.ActivationDeadlineDateEdit));
		}

		public static DeclarationDetailsControlBag Instance => instance ?? (instance = new DeclarationDetailsControlBag());

		[ThreadStatic]
		static DeclarationDetailsControlBag instance;

		public ControlReference MrnTextBox { get; }

		public ControlReference DepartureStatusDropEdit { get; }

		public ControlReference MessageStatusDropEdit { get; }

		public ControlReference PhaseStatusDropEdit { get; }

		public ControlReference ReleaseDateEdit { get; }

		public ControlReference AcceptanceDateEdit { get; }

		public ControlReference ActivationDeadlineDateEdit { get; }

		protected override Control CreateTemplate() => new DeclarationDetailsUserControl();
	}
}
