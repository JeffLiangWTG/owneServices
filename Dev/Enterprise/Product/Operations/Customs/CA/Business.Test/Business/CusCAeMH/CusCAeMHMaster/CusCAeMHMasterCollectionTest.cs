using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHMasterCollection))]
	sealed class CusCAeMHMasterCollectionTest : ActiveBusinessObjectCollectionTestCase<CusCAeMHMasterCollection>
	{
		protected override CusCAeMHMasterCollection GetCollectionToTest()
		{
			return new CusCAeMHMasterCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusCAeMHMaster>();
		}
	}
}
