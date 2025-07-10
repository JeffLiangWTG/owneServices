using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(OrganizationSubBalanceCollection))]
	public class OrganizationSubBalanceCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<OrganizationSubBalanceCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrganizationSubBalance();
		}

		protected override OrganizationSubBalanceCollection GetCollectionToTest()
		{
			return new OrganizationSubBalanceCollection(Factory);
		}
	}
}
