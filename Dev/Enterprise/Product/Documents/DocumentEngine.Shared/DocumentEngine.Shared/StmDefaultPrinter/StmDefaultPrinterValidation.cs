//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmDefaultPrinterValidation
//
//    This class should be used for overriding validation in AutoStmDefaultPrinterValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.DocumentEngine.Shared
{
	public class StmDefaultPrinterValidation : AutoStmDefaultPrinterValidation
	{
		public StmDefaultPrinterValidation(AutoStmDefaultPrinter parent)
			: base(parent)
		{
		}
	}
}
