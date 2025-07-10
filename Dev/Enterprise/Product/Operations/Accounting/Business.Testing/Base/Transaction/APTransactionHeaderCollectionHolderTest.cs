using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(APTransactionHeaderCollectionHolder))]
	public class APTransactionHeaderCollectionHolderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new APTransactionHeaderCollectionHolder(Factory, new APTransactionHeaderCollection(Factory));
		}
	}
}
