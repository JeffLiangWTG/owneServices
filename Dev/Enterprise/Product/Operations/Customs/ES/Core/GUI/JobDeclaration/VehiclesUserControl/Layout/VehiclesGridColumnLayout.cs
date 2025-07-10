using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public sealed class VehiclesGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ??= CreateLayout();
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = VehiclesGridColumnBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.VinTextBoxColumn);
			builder.AddColumn(euGridColumnBag.BrandTextBoxColumn);
			builder.AddColumn(euGridColumnBag.ModelTextBoxColumn);

			return builder.Build();
		}

		#endregion
	}
}
