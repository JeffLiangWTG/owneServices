using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	class ConsolidatedDeclarationControlBag : ControlBag
	{
		public static ConsolidatedDeclarationControlBag Instance => instance ?? (instance = new ConsolidatedDeclarationControlBag());

		[ThreadStatic]
		static ConsolidatedDeclarationControlBag instance;

		ConsolidatedDeclarationControlBag()
		{
			EntryStyleDropEdit = RegisterControl(nameof(ConsolidatedDeclarationControlBagTemplate.EntryStyleDropEdit));
			VesselCodeFindBox = RegisterControl(nameof(ConsolidatedDeclarationControlBagTemplate.VesselCodeFindBox));
			ConsolidatedDeclarationDetailsUserControl = RegisterControl(nameof(ConsolidatedDeclarationControlBagTemplate.ConsolidatedDeclarationDetailsUserControl));
			VoyageFlightNoTextBox = RegisterControl(nameof(ConsolidatedDeclarationControlBagTemplate.VoyageFlightNoTextBox));
			PaymentStatusTextBox = RegisterControl(nameof(ConsolidatedDeclarationControlBagTemplate.PaymentStatusTextBox));
		}

		public ControlReference EntryStyleDropEdit { get; }
		public ControlReference VesselCodeFindBox { get; }
		public ControlReference VoyageFlightNoTextBox { get; }
		public ControlReference ConsolidatedDeclarationDetailsUserControl { get; }
		public ControlReference PaymentStatusTextBox { get; }

		protected override Control CreateTemplate() => new ConsolidatedDeclarationControlBagTemplate();
	}
}
