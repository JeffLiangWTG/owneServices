using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class UnloadingDifferencesAdditionalDocumentsGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = UnloadingDifferencesAdditionalDocumentsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.SequenceNumberCalcEditColumn);
			builder.AddColumn(euGridColumnBag.StatusDropEditColumn);
			builder.AddColumn(euGridColumnBag.SubTypeDropEditColumn);
			builder.AddColumn(euGridColumnBag.CodeCodeFindBoxColumn);
			builder.AddColumn(euGridColumnBag.ReferenceNumberTextBoxColumn);
			builder.AddColumn(euGridColumnBag.DescriptionTextBoxColumn);

			return builder.Build();
		}

		#endregion
	}
}
