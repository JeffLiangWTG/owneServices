using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public class UnloadingDetailsLayout : IPanelLayoutProvider
{
	public UnloadingDetailsLayout()
	{
		layout = CreateUnloadingDetailsLayout();
	}

	readonly PanelLayout layout;

	public PanelLayout Layout => layout;

	PanelLayout CreateUnloadingDetailsLayout()
	{
		var builder = new UnloadingDetailsLayoutBuilder<Business.NctsArrivalMovementHeader>();
		var commonBag = builder.CommonBag;

		builder.AddColumn();
		builder.Add(commonBag.UnloadingDateDateEdit, ControlWidthClass.Medium);
		builder.Add(commonBag.UnloadingConformCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.UnloadingRemarksTextBox, ControlWidthClass.Long);
		return builder.Build();
	}
}
