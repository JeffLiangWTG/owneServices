using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccTaxReturnColumn))]
	public class AccTaxReturnColumnTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanApplyDataRefreshIsImplemented()
		{
			var taxReturnColumn = Factory.New<AccTaxReturnColumn>();
			var dataRefreshSupporter = taxReturnColumn as ICanApplyDataRefresh;

			AssertNotNull(dataRefreshSupporter);
			AssertEquals(false, dataRefreshSupporter.CanApplyDataRefresh(DataRefreshAction.None, null));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var taxReturn = new TestObjectCreator(Factory).CreateAccTaxReturn(addTaxReturnColumn: true);
			return taxReturn.Columns[0];
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var taxReturn = new TestObjectCreator(factory).CreateAccTaxReturn(addTaxReturnColumn: true);
			return taxReturn.Columns[0];
		}
	}
}
