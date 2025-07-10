using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class ContainersOrEquipmentsGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var builder = GridColumnLayoutBuilder.Create();
			var columnBag = ContainersOrEquipmentsGridColumnsBag.Instance;
			builder.AddColumn(columnBag.ContainerNumberTextBoxColumn);
			builder.AddColumn(columnBag.IsEquipmentCheckBoxColumn);
			return builder.Build();
		}
	}
}
