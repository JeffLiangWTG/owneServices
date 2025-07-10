namespace Enterprise.DocumentEngine.DocBuilder
{
	class CoverSheetTemplateTranslationHelper : LegacyDocumentTemplateTranslationHelper
	{
		public override string KeyPrefix => DocBuilderResourceStrings.CoverSheetLabelKeyPrefix;

		protected override string SubRootDirectory => @"\";

		public override bool NeedTranslate(string excelTemplateName, byte[] template) => true;
	}
}
