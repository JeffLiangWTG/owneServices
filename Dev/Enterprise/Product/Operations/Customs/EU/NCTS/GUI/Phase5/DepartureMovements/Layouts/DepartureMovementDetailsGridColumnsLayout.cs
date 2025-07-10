using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class DepartureMovementDetailsGridColumnsLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = DepartureMovementDetailsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.InlandTransportModeDropEdit);
			builder.AddColumn(euGridColumnBag.TransportAtDepartureDropEdit);
			builder.AddColumn(euGridColumnBag.FromWarehouseOrganisationFindBox);
			builder.AddColumn(euGridColumnBag.FromWarehouseAddressDropEdit);
			builder.AddColumn(euGridColumnBag.DepartureStatusDropEdit);
			builder.AddColumn(euGridColumnBag.DepartureStatusDescriptionTextEdit);
			builder.AddColumn(euGridColumnBag.MessageStatusDropEdit);
			builder.AddColumn(euGridColumnBag.MessageStatusDescriptionTextEdit);
			builder.AddColumn(euGridColumnBag.PhaseStatusDropEdit);
			builder.AddColumn(euGridColumnBag.PhaseStatusDescriptionTextEdit);
			return builder.Build();
		}

		IGridColumnLayout layout;

		#endregion
	}
}
