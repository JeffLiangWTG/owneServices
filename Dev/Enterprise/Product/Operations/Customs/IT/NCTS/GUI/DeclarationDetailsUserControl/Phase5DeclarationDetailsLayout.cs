using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

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

		var itControlBag = DeclarationDetailsControlBag.Instance;

		builder.AddControlBag(itControlBag);
		builder.AddColumn();

		builder.Add(commonBag.MrnTextBox, ControlWidthClass.Long);
		builder.Add(itControlBag.ReleaseCodeTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DepartureStatusDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PhaseStatusDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.MessageStatusDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.AcceptanceDateEdit, ControlWidthClass.Auto, commonBag.MrnTextBox);
		builder.Add(itControlBag.ReleaseDateEdit, ControlWidthClass.Auto, itControlBag.ReleaseCodeTextBox);
		builder.Add(itControlBag.WriteOffDateEdit, ControlWidthClass.Auto, commonBag.DepartureStatusDropEdit);
		builder.Add(itControlBag.ControlChannelDropEdit, ControlWidthClass.Auto, commonBag.PhaseStatusDropEdit);

		return builder.Build();
	}
}
