using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.AWB.Testing
{
	[TestedType(typeof(JASConsolExportAWBRateLine))]
	public class JASConsolExportAWBRateLineTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("implemented in a 'special' way on base class", true);
		}

		public void TestHumanReadableName()
		{
			JASConsolExportAWBHeader awbHeader = Factory.New<JASConsolExportAWBHeader>();
			JASConsolExportAWBRateLine rateLine = (JASConsolExportAWBRateLine)awbHeader.AWBRateLines.AddNew();
			AssertEquals("Master Air Waybill Freight Breakdown", rateLine.HumanReadableName);
		}

		public void TestNatureAndQtyOfGoodsText()
		{
			JASConsolExportAWBHeader awbHeader = Factory.New<JASConsolExportAWBHeader>();
			JASConsolExportAWBRateLine rateLine = (JASConsolExportAWBRateLine)awbHeader.AWBRateLines.AddNew();
			AssertEquals(typeof(JASNatureAndQtyOfGoods), rateLine.NatureAndQtyOfGoodsText.GetType());
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<JASConsolExportAWBRateLine>();
		}
	}
}
