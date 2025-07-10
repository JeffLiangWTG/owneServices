using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public class UnloadingDifferencesSupportingDocumentsGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = UnloadingDifferencesSupportingDocumentsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.SequenceNumberCalcEditColumn);
			builder.AddColumn(euGridColumnBag.StatusDropEditColumn);
			builder.AddColumn(euGridColumnBag.CodeCodeFindBoxColumn);
			builder.AddColumn(euGridColumnBag.ReferenceNumberTextBoxColumn);
			builder.AddColumn(euGridColumnBag.ReferenceNumber2TextBoxColumn);

			return builder.Build();
		}

		#endregion
	}
}
