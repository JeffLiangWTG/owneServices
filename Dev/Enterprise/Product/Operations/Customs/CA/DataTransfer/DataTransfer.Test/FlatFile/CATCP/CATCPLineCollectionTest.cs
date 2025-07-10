using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DataTransfer.Testing
{
	[TestedType(typeof(CATCPLineCollection))]
	sealed class CATCPLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CATCPLineCollection>
	{
		protected override CATCPLineCollection GetCollectionToTest() => new CATCPLineCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CATCPLine();
	}
}
