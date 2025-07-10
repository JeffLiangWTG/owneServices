using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI;

public sealed class Phase5ArrivalDeclarationDetailsLayout : IPanelLayoutProvider
{
	public Phase5ArrivalDeclarationDetailsLayout()
	{
		Layout = CreateLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new EU.NCTS.GUI.ArrivalDeclarationDetailsLayoutBuilder<NctsHeader>();
		var commonBag = builder.CommonBag;
		var esBag = ArrivalDeclarationDetailsControlBag.Instance;
		builder.AddControlBag(esBag);

		builder.AddColumn();
		builder.Add(commonBag.StatusDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.PhaseDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.MessageStatusDropEdit, ControlWidthClass.Auto);
		builder.Add(esBag.CircuitTextBox, ControlWidthClass.Auto);
		builder.Add(esBag.ArrivalSummaryDeclarationUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.SeparatorLabel, ControlWidthClass.Auto);
		builder.Add(commonBag.AcceptanceDateEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.ReleaseDateEdit, ControlWidthClass.Auto);

		builder.AddControlBehaviour<ZDateEdit>(commonBag.AcceptanceDateEdit, (control, header) => control.BindTo = nameof(header.AcceptanceDate));
		builder.AddControlBehaviour<ZDateEdit>(commonBag.ReleaseDateEdit, (control, header) => control.BindTo = nameof(header.ReleaseDate));

		return builder.Build();
	}
}
