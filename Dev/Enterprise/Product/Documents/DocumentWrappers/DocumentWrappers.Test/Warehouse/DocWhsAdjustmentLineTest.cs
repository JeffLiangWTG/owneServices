using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsAdjustmentLine))]
	sealed class DocWhsAdjustmentLineTest : DocWhsDocketLineTest<WhsAdjustment, WhsAdjustmentLine, DocWhsAdjustmentLine>
	{
		public void TestArrivalDate()
		{
			var date = ZDateTime.Today.AddDays(1);
			DocketLine.WE_AdjustmentArrivalDate = date.ToOffset();
			AssertEquals(date, DocketLineWrapper.ArrivalDate);
		}

		public void TestDocket()
		{
			AssertEquals(typeof(DocWhsAdjustment), DocketLineWrapper.Docket.GetType());
		}

		#region Implementation

		protected override DocWhsAdjustmentLine CreateDocketLineWrapper(WhsAdjustmentLine docketLine)
		{
			return DocWhsAdjustmentLine.New(docketLine, Factory);
		}

		#endregion
	}
}
