using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class ConsignmentItemPackingDetailsGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var columnBag = ConsignmentItemPackingDetailsGridColumnsBag.Instance;
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(columnBag.PackageSequenceTextBoxColumn);
			builder.AddColumn(columnBag.PackageQuantityCalEditColumn);
			builder.AddColumn(columnBag.PackageTypeDropEditColumn);
			builder.AddColumn(columnBag.PackageMarksAndNumbersTextBoxColumn);
			builder.AddColumn(columnBag.ContainerGuidDropEditColumn);
			return builder.Build();
		}
	}
}
