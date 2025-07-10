using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestsSubclassesOf(typeof(CusExitHeader))]
	public abstract class CusExitHeaderAbstractTest<T> : EnterpriseBusinessObjectTestCase
		where T : CusExitHeader
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		public void TestShouldHaveSeqNumInContainersOrEquipmentsAndSeals()
		{
			AssertEquals("The value returned by ShouldHaveSeqNumInContainersOrEquipmentsAndSeals", ExpectedShouldHaveSeqNumInContainersOrEquipmentsAndSeals, exitHeader.ShouldHaveSeqNumInContainersOrEquipmentsAndSeals);
		}

		protected virtual bool ExpectedShouldHaveSeqNumInContainersOrEquipmentsAndSeals => false;

		public static T GetNewBusinessObject(BusinessObjectFactory factory) => factory.NewWithValidTestData<T>();

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = (T)GetNewBusinessObject();
		}
		protected T exitHeader;
	}
}
