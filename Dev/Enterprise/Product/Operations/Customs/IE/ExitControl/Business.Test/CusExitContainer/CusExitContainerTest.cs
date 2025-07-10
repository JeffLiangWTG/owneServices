using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitContainer))]
	sealed class CusExitContainerTest : EnterpriseBusinessObjectTestCase
	{
		public static CusExitContainer GetNewBusinessObject(BusinessObjectFactory factory) => factory.New<CusExitHeader>().CusExitContainers.AddNew();

		public void TestCusExitConsignmentPivots()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(cusExitContainer.CusExitConsignmentPivots);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => cusExitContainer;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override void SetUp()
		{
			base.SetUp();
			cusExitContainer = (CusExitContainer)GetNewBusinessObject();
		}

		CusExitContainer cusExitContainer;
	}
}
