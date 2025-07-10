using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI;

public sealed class UCC6TemporaryStorageBillGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = UCC6TemporaryStorageBillGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.IsMasterCheckBoxColumn);
		builder.AddColumn(euGridColumnBag.BillDocumentTypeDropEditColumn);
		builder.AddColumn(euGridColumnBag.BillNumberTextBoxColumn);
		builder.AddColumn(euGridColumnBag.UCRNumberTextBoxColumn);
		builder.AddColumn(euGridColumnBag.GrossWeightCalcEditColumn);
		builder.AddColumn(euGridColumnBag.GrossWeightUQDropEditColumn);
		builder.AddColumn(euGridColumnBag.ConsignorOrgFindBoxColumn);
		builder.AddColumn(euGridColumnBag.ShipperAddressDropEditColumn);
		builder.AddColumn(euGridColumnBag.ConsigneeOrgFindBoxColumn);
		builder.AddColumn(euGridColumnBag.ConsigneeAddressDropEditColumn);

		return builder.Build();
	}

	#endregion
}

