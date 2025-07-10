using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHContainerCollection))]
	sealed class CusCAeMHContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<CusCAeMHContainerCollection>
	{
		protected override CusCAeMHContainerCollection GetCollectionToTest()
		{
			return Master.Containers;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Master.Containers.AddNew();
		}

		CusCAeMHMaster Master
		{
			get { return fMaster ?? (fMaster = Factory.New<CusCAeMHMaster>()); }
		}
		CusCAeMHMaster fMaster;
	}
}
