using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class Phase5DeclarationDetailsLayout : IPanelLayoutProvider
	{
		public Phase5DeclarationDetailsLayout()
		{
			Layout = CreateDeclarationDetailsLayout();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		PanelLayout CreateDeclarationDetailsLayout()
		{
			var builder = new DeclarationDetailsLayoutBuilder<Business.NctsDepartureMovementHeader>();
			var commonBag = builder.CommonBag;
			var esBag = DeclarationDetailsControlBag.Instance;
			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(commonBag.MrnTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.DepartureStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.PhaseStatusDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.MessageStatusDropEdit, ControlWidthClass.Long);
			builder.Add(esBag.CircuitTextBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(esBag.AcceptanceDateDateEdit, ControlWidthClass.Long);
			builder.Add(esBag.ClearanceNumberTextBox, ControlWidthClass.Long);
			builder.Add(esBag.ClearanceDateDateEdit, ControlWidthClass.Long);
			builder.Add(esBag.ArrivalLimitDateEdit, ControlWidthClass.Long);

			return builder.Build();
		}
	}
}
