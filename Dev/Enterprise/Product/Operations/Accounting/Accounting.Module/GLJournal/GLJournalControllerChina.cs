using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Reversing;

namespace Enterprise.Accounting.Module
{
	public class GLJournalControllerChina : GLJournalController
	{
		protected override ReversingBase GetAutoCurrencyAdjustmentReversing(IBusiness transaction)
		{
			return new AutoCurrencyAdjustmentGLJournalReversingChina(transaction as IGeneralLedger);
		}
	}
}
