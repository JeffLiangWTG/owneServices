//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmUniversalCopyLookups
//
//    This class should be used for overriding collections in AutoStmUniversalCopyLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.UniversalCopy.Business
{
	public class StmUniversalCopyLookups : AutoStmUniversalCopyLookups
	{
		public StmUniversalCopyLookups(AutoStmUniversalCopy parent) : base(parent)
		{
		}

		protected new StmUniversalCopy Parent
		{
			get { return (StmUniversalCopy)base.Parent; }
		}

		public override StmModuleFilterCollection CopyTemplates
		{
			get { return new StmModuleFilterCollection(Factory, Parent.GridContext, new FilterStripLayoutsHelper(), Parent.CopyTemplate?.PK ?? ZGuid.Empty); }
		}
	}
}
