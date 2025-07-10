using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class Phase5ArrivalSummaryDeclarationControlBag : ControlBag
	{
		Phase5ArrivalSummaryDeclarationControlBag()
		{
			SummaryTypeDropEdit = RegisterControl(nameof(Phase5ArrivalSummaryDeclarationUserControl.SummaryTypeDropEdit));
			PreviousSummaryDeclarationTextBox = RegisterControl(nameof(Phase5ArrivalSummaryDeclarationUserControl.PreviousSummaryDeclarationTextBox));
			G4PreviousDocumentGroupBox = RegisterControl(nameof(Phase5ArrivalSummaryDeclarationUserControl.G4PreviousDocumentGroupBox));
		}

		public static Phase5ArrivalSummaryDeclarationControlBag Instance => instance ?? (instance = new Phase5ArrivalSummaryDeclarationControlBag());

		[ThreadStatic]
		static Phase5ArrivalSummaryDeclarationControlBag instance;

		protected override Control CreateTemplate() => new Phase5ArrivalSummaryDeclarationUserControl();

		public ControlReference SummaryTypeDropEdit { get; }

		public ControlReference PreviousSummaryDeclarationTextBox { get; }

		public ControlReference G4PreviousDocumentGroupBox { get; }
	}
}
