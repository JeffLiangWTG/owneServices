//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmMenuDocumentConfigLookups
//
//    This class should be used for overriding collections in AutoStmMenuDocumentConfigLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuDocumentConfigLookups : AutoStmMenuDocumentConfigLookups
	{
		public StmMenuDocumentConfigLookups(AutoStmMenuDocumentConfig parent)
			: base(parent)
		{
		}

		public override OrgHeaderCollection Headers
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public override StmMenuTemplatePivotCollection MenuTemplatePivots
		{
			get { return new StmMenuTemplatePivotBaseCollection(Factory); }
		}

		public DocumentConfigPageStyleList AvailablePageStyles
		{
			get { return Factory.GetCachedValue<DocumentConfigPageStyleList>(); }
		}
	}
}
