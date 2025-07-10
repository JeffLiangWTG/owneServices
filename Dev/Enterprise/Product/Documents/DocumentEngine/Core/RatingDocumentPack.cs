using Enterprise.DocumentEngineCore.Registry;

namespace Enterprise.DocumentEngine
{
	class RatingDocumentPack : DocumentPack
	{
		public RatingDocumentPack(DocumentCommand command, RatingDocPackBuilder builder)
			: base(command)
		{
			this.builder = builder;
			this.SupportsLanguageSelection = DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.Value;
		}

		protected override void RebuildIfLanguageChangedCore()
		{
			if (Language != LastTemplateGeneratorLanguage)
			{
				builder.RebuildAll();
				DeliveryInstructions.AddOtherEDocsToAttachIfNeeded(true);
			}
		}

		readonly RatingDocPackBuilder builder;
	}
}
