using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class Phase5MiscellaneousOptionsLayout : IPanelLayoutProvider
{
	PanelLayout Layout { get; } = CreateLayout();

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	static PanelLayout CreateLayout()
	{
		var builder = new MiscellaneousOptionsLayoutBuilder<Business.NctsHeader>();
		var commonBag = builder.CommonBag;
		var itBag = MiscellaneousOptionsControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(commonBag.BranchCodeFindBox, ControlWidthClass.Long);
		builder.Add(itBag.CustomsProfileDropEdit, ControlWidthClass.Long);

		return builder.Build();
	}
}
