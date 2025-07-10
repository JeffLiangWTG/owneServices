using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(Contra))]
	public class ContraIUnmatchDateSupporterTest : BaseITransactionTestCase
	{
		protected override ITransaction GetNewObject()
		{
			return Contra.New(Factory);
		}
	}
}
