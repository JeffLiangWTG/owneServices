using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class AdditionalSealsGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = AdditionalSealsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.SealNumberTextBoxColumn);

			return builder.Build();
		}

		#endregion
	}
}
