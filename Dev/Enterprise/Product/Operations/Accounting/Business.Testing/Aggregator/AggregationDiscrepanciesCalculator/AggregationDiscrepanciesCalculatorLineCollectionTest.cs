using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(AggregationDiscrepanciesCalculatorLineCollection))]
	public class AggregationDiscrepanciesCalculatorLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AggregationDiscrepanciesCalculatorLineCollection>
	{
		public void TestAddRemove()
		{
			Assert(!GetCollectionToTest().AllowNew);
			Assert(GetCollectionToTest().AllowRemove);
		}

		#region Implementation

		protected override AggregationDiscrepanciesCalculatorLineCollection GetCollectionToTest()
		{
			return new AggregationDiscrepanciesCalculatorLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AggregationDiscrepanciesCalculatorLine();
		}

		#endregion
	}
}
