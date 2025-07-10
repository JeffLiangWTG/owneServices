using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class JobWorkGridColumnsLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var gridColumnBag = JobWorksGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(gridColumnBag.LineNoCalcEditColumn);
		builder.AddColumn(gridColumnBag.ReferenceNumberTextBoxColumn);
		builder.AddColumn(gridColumnBag.DateOfIssueDateEditColumn);
		builder.AddColumn(gridColumnBag.CustomsOfficeTextBoxColumn);
		builder.AddColumn(gridColumnBag.ReferenceNumber2TextBoxColumn);
		builder.AddColumn(gridColumnBag.ItemNumberCalcEditColumn);
		builder.AddColumn(gridColumnBag.QuantityCalcEditColumn);
		builder.AddColumn(gridColumnBag.UnitOfQuantityTextBoxColumn);
		return builder.Build();
	}

	IGridColumnLayout layout;

	#endregion
}

