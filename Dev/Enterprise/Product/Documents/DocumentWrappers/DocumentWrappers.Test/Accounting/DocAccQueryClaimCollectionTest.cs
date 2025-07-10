using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.AccQueryClaims;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers
{
	[TestedType(typeof(DocAccQueryClaimCollection))]
	sealed class DocAccQueryClaimCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocAccQueryClaimCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			ARAccQueryClaim claim = Factory.New<ARAccQueryClaim>();
			return DocAccQueryClaim.New(claim, Factory);
		}

		protected override DocAccQueryClaimCollection GetCollectionToTest()
		{
			return new DocAccQueryClaimCollection(Factory);
		}
	}
}
