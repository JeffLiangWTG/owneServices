using Enterprise.Customs.DE.Intrastat.Business;
using Enterprise.Customs.EU.Intrastat.GUI.Testing;
using Enterprise.Customs.EU.Intrastat.GUI.Transactions;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Intrastat.GUI.Testing
{
	[TestedType(typeof(IntrastatTransactionForm))]
	sealed class IntrastatTransactionFormTest : IntrastatTransactionFormAbstractTest<CusIntrastatHeader>
	{
	}
}
