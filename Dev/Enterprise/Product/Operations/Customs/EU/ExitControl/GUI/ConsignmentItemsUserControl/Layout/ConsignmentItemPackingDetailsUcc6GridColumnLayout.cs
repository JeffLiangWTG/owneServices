using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public class ConsignmentItemPackingDetailsUcc6GridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var commonColumnBag = ConsignmentItemPackingDetailsGridColumnsBag.Instance;
		var builder = GridColumnLayoutBuilder.Create();

		builder.AddColumn(commonColumnBag.PackageSequenceTextBoxColumn);
		builder.AddColumn(commonColumnBag.PackageQuantityCalEditColumn);
		builder.AddColumn(commonColumnBag.PackageTypeDropEditColumn);
		builder.AddColumn(commonColumnBag.PackageMarksAndNumbersTextBoxColumn);
		builder.AddColumn(commonColumnBag.ContainerGuidDropEditColumn);
		builder.AddColumn(commonColumnBag.PackageMarksAndNumbersStatusDropEditColumn);

		return builder.Build();
	}
}
