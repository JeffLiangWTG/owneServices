using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public class SealsUcc6GridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var commonColumnBag = SealsGridColumnsBag.Instance;
		var layoutBuilder = GridColumnLayoutBuilder.Create();

		layoutBuilder.AddColumn(commonColumnBag.SequenceNumberCalcEditColumn);
		layoutBuilder.AddColumn(commonColumnBag.SealNumberTextBoxColumn);
		layoutBuilder.AddColumn(commonColumnBag.UnloadingStatusDropEditColumn);

		return layoutBuilder.Build();
	}
}
