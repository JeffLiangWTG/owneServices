using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusPollingTransactionCollection))]
	public class CusPollingTransactionCollectionTest : ActiveBusinessObjectCollectionTestCase<CusPollingTransactionCollection>
	{
		protected override CusPollingTransactionCollection GetCollectionToTest() => new CusPollingTransactionCollection(Message);
		EDIMessage Message => message ?? (message = Factory.New<EDIMessage>());
		EDIMessage message;
	}
}
