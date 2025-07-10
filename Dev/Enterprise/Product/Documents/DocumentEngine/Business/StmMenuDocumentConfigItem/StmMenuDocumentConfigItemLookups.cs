//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmMenuDocumentConfigItemLookups
//
//    This class should be used for overriding collections in AutoStmMenuDocumentConfigItemLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuDocumentConfigItemLookups : AutoStmMenuDocumentConfigItemLookups
	{
		public StmMenuDocumentConfigItemLookups(AutoStmMenuDocumentConfigItem parent)
			: base(parent)
		{
			configItem = (StmMenuDocumentConfigItem)parent;
		}
		readonly StmMenuDocumentConfigItem configItem;

		public ConfigurableSectionTypeList LegacySectionTypes
		{
			get { return configItem.Factory.GetCachedValue<ConfigurableSectionTypeList>(); }
		}

		public GenericSectionUsageList GenericSectionTypes
		{
			get { return configItem.Factory.GetCachedValue<GenericSectionUsageList>(); }
		}
	}
}
