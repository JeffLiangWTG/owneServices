using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APContraRow))]
	public class APContraRowMatchingTest : ContraRowMatchingTest
	{
		protected override ContraRow GetNewContraRow()
		{
			return Factory.New<APContraRow>();
		}
	}
}
