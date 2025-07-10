using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

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

		builder.AddColumn();
		builder.Add(commonBag.MrnTextBox, ControlWidthClass.Long);
		builder.Add(commonBag.DepartureStatusDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.PhaseStatusDropEdit, ControlWidthClass.Long);
		builder.Add(commonBag.MessageStatusDropEdit, ControlWidthClass.Long);

		builder.AddColumn();
		builder.Add(commonBag.ReleaseDateEdit, ControlWidthClass.Auto, commonBag.MrnTextBox);
		builder.SetCaption(commonBag.ReleaseDateEdit, _ => Res.GetData("91C36F7E-8972-4BBA-97E7-C931C2699693", "Release Date"));

		builder.Add(commonBag.AcceptanceDateEdit, ControlWidthClass.Auto, commonBag.DepartureStatusDropEdit);
		builder.SetCaption(commonBag.AcceptanceDateEdit, _ => Res.GetData("D3EDCD9B-5317-4033-8108-37C8794EC406", "Acceptance Date"));

		builder.Add(commonBag.ActivationDeadlineDateEdit, ControlWidthClass.Auto, commonBag.DepartureStatusDropEdit);
		builder.SetCaption(commonBag.ActivationDeadlineDateEdit, _ => Res.GetData("DF60D2CB-5AE3-4285-902F-C26639178ED1", "Activation Deadline"));

		return builder.Build();
	}
}
