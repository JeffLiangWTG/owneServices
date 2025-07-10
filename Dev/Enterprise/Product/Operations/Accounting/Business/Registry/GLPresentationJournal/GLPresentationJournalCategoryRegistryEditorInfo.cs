using Enterprise.Registry.Business;

namespace Enterprise.Accounting.Registry.Business
{
	public class GLPresentationJournalCategoryRegistryEditorInfo : CodeDescriptionBoolWithExtraBoolRegistryEditorInfo
	{
		public GLPresentationJournalCategoryRegistryEditorInfo(string boolColumnCaption, string bool2ColumnCaption, string bool3ColumnCaption, string bool4ColumnCaption)
			: base(boolColumnCaption,true, bool2ColumnCaption, true)
		{
			this.Bool3ColumnCaption = bool3ColumnCaption;
			this.Bool4ColumnCaption = bool4ColumnCaption;
		}

		public readonly string Bool3ColumnCaption;
		public readonly string Bool4ColumnCaption;
	}
}

