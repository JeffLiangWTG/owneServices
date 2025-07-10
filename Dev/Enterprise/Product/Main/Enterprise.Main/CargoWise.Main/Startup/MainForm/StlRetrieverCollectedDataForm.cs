using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public partial class StlRetrieverCollectedDataForm(StlTransactionForDisplayCollection billingTransactionWrappers) : ZChildForm(billingTransactionWrappers)
	{
		public override string FormCaption
		{
			get { return Res.GetString("Enterprise.Billing.StlCollector.Retriever.StlRetrieverCollectedDataForm", "Billing Collected Data"); }
		}
	}
}
