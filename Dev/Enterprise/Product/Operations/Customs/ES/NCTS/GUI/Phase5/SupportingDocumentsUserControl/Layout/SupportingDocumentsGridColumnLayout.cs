using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	class SupportingDocumentsGridColumnLayout : IGridColumnLayoutProvider
	{
		public IGridColumnLayout Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = SupportingDocumentsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.SequenceNumberCalcEditColumn);
			builder.AddColumn(euGridColumnBag.CodeCodeFindBoxColumn);
			builder.AddColumn(euGridColumnBag.ReferenceNumberTextBoxColumn);
			builder.AddColumn(euGridColumnBag.ItemNumberCalcEditColumn);
			builder.AddColumn(euGridColumnBag.ReferenceNumber2TextBoxColumn);

			return builder.Build();
		}

		#endregion
	}
}
