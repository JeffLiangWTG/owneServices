using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	[TestedType(typeof(AccTaxReturnLine))]
	public class AccTaxReturnLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanApplyDataRefreshIsImplemented()
		{
			var taxReturnLine = Factory.New<AccTaxReturnLine>();
			var dataRefreshSupporter = taxReturnLine as ICanApplyDataRefresh;

			AssertNotNull(dataRefreshSupporter);
			AssertEquals(false, dataRefreshSupporter.CanApplyDataRefresh(DataRefreshAction.None, null));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var taxReturn = new TestObjectCreator(Factory).CreateAccTaxReturn(addTaxReturnLine: true);
			return taxReturn.Lines[0];
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var taxReturn = new TestObjectCreator(factory).CreateAccTaxReturn(addTaxReturnLine: true);
			return taxReturn.Lines[0];
		}
	}
}
