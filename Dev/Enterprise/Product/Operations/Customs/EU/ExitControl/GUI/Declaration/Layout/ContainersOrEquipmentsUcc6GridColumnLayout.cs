using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public class ContainersOrEquipmentsUcc6GridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var builder = GridColumnLayoutBuilder.Create();
		var commonColumnBag = ContainersOrEquipmentsGridColumnsBag.Instance;

		builder.AddColumn(commonColumnBag.SequenceCalcEditColumn);
		builder.AddColumn(commonColumnBag.ContainerNumberTextBoxColumn);
		builder.AddColumn(commonColumnBag.IsEquipmentCheckBoxColumn);
		builder.AddColumn(commonColumnBag.StatusDropEditColumn);
		builder.AddColumn(commonColumnBag.SealCountCalcEditColumn);

		return builder.Build();
	}
}
