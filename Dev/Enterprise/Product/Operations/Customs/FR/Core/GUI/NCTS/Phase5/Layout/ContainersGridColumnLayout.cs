using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public class ContainersGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var gridColumnBag = ContainersGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(gridColumnBag.TypeContainerFindBoxColumn);
			builder.AddColumn(gridColumnBag.ModeDropEditColumn);
			builder.AddColumn(gridColumnBag.ContainerNumTextBoxColumn);
			builder.AddColumn(gridColumnBag.Seal1TextBoxColumn);
			builder.AddColumn(gridColumnBag.Seal2TextBoxColumn);
			builder.AddColumn(gridColumnBag.TotalSealCountCalcEditColumn);

			return builder.Build();
		}

		#endregion
	}
}
