using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(IndependentChargeCollectionForBinding))]
	public class IndependentChargeCollectionForBinding_InnerTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new IndependentChargeCollectionForBinding(Factory);
		}
	}
}
