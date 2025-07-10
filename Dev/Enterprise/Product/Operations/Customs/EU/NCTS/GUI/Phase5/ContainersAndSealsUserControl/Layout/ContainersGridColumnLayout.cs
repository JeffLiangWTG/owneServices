using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class ContainersGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = ContainersGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.ModeDropEditColumn);
			builder.AddColumn(euGridColumnBag.ContainerNumTextBoxColumn);
			builder.AddColumn(euGridColumnBag.Seal1TextBoxColumn);
			builder.AddColumn(euGridColumnBag.Seal2TextBoxColumn);
			builder.AddColumn(euGridColumnBag.TotalSealCountCalcEditColumn);

			return builder.Build();
		}

		#endregion
	}
}
