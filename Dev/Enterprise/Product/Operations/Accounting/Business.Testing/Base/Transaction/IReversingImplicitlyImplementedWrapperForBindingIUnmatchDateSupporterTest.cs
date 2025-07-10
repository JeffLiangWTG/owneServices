using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(IReversingImplicitlyImplementedWrapperForBinding))]
	public class IReversingImplicitlyImplementedWrapperForBindingIUnmatchDateSupporterTest : BaseITransactionTestCase
	{
		protected override ITransaction GetNewObject()
		{
			return new IReversingImplicitlyImplementedWrapperForBinding(Factory.NewWithValidTestData<ARInvoice>());
		}
	}
}
