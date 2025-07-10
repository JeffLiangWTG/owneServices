using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public class NewCashbookExchangeDiffBankAccountFilterBusinessObject : AccBankAccountFilterBusinessObject
	{
		public NewCashbookExchangeDiffBankAccountFilterBusinessObject() : base()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "CashbookExchangeDiffBankAccountFilterBusinessObject";
		}

		protected override bool ShouldAddCustomSqlFilter => false;
	}
}