using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusGuaranteeLineTransactionCollection))]
	public class CusGuaranteeLineTransactionCollectionTest : ActiveBusinessObjectCollectionTestCase<CusGuaranteeLineTransactionCollection>
	{
		protected override CusGuaranteeLineTransactionCollection GetCollectionToTest()
		{
			return new CusGuaranteeLineTransactionCollection(Factory.New<CusGuaranteeHeader>());
		}
	}
}
