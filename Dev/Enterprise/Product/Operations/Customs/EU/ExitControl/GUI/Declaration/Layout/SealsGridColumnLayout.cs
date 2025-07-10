using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class SealsGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var columnBag = SealsGridColumnsBag.Instance;
			var layoutBuilder = GridColumnLayoutBuilder.Create();
			layoutBuilder.AddColumn(columnBag.SealNumberTextBoxColumn);
			return layoutBuilder.Build();
		}
	}
}
