using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ReportOfReceiptReasonCollection))]
	class ReportOfReceiptReasonCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			var outturn = Factory.New<EMCSJobComInvoiceLine>().Outturn;
			var collection = outturn.ReportOfReceiptReasons;
			AssertEquals(9, collection.MaxCount);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var outturn = Factory.New<EMCSJobComInvoiceLine>().Outturn;
			return outturn.ReportOfReceiptReasons;
		}
	}
}
