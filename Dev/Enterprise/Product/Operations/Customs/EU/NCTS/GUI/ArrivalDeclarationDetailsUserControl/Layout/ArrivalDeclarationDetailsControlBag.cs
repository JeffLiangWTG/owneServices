using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class ArrivalDeclarationDetailsControlBag : ControlBag
	{
		ArrivalDeclarationDetailsControlBag()
		{
			StatusDropEdit = RegisterControl(nameof(ArrivalDeclarationDetailsUserControl.StatusDropEdit));
			MessageStatusDropEdit = RegisterControl(nameof(ArrivalDeclarationDetailsUserControl.MessageStatusDropEdit));
			PhaseDropEdit = RegisterControl(nameof(ArrivalDeclarationDetailsUserControl.PhaseDropEdit));
			SeparatorLabel = RegisterControl(nameof(ArrivalDeclarationDetailsUserControl.SeparatorLabel));
			ReleaseDateEdit = RegisterControl(nameof(ArrivalDeclarationDetailsUserControl.ReleaseDateEdit));
			AcceptanceDateEdit = RegisterControl(nameof(ArrivalDeclarationDetailsUserControl.AcceptanceDateEdit));
		}

		public static ArrivalDeclarationDetailsControlBag Instance => instance ?? (instance = new ArrivalDeclarationDetailsControlBag());

		[ThreadStatic]
		static ArrivalDeclarationDetailsControlBag instance;

		public ControlReference StatusDropEdit { get; }

		public ControlReference MessageStatusDropEdit { get; }

		public ControlReference PhaseDropEdit { get; }

		public ControlReference SeparatorLabel { get; }

		public ControlReference ReleaseDateEdit { get; }

		public ControlReference AcceptanceDateEdit { get; }

		protected override Control CreateTemplate() => new ArrivalDeclarationDetailsUserControl();
	}
}
