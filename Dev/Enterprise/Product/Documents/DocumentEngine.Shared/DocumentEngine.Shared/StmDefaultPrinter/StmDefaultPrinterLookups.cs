//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmDefaultPrinterLookups
//
//    This class should be used for overriding collections in AutoStmDefaultPrinterLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.Shared
{
	public class StmDefaultPrinterLookups : AutoStmDefaultPrinterLookups
	{
		public StmDefaultPrinterLookups(AutoStmDefaultPrinter parent)
			: base(parent)
		{
		}
	}
}
