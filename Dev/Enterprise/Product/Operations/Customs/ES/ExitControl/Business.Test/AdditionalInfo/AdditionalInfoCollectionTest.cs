using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(AdditionalInfoCollection))]
	sealed class AdditionalInfoCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<AdditionalInfo>
	{
		protected override Customs.Business.CusSupportingInfoCollection<AdditionalInfo> GetCusSupportingInfoCollection()
		{
			(var report, _, _, _, _) = CusExitReportItemTest.GetNewBusinessObject(Factory);
			return new AdditionalInfoCollection(report);
		}

		public void TestItemNumberAutoNum()
		{
			var exitReportItem = Factory.New<CusExitReportItem>();
			var additionalInfo1 = exitReportItem.AdditionalInfos.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("additionalInfo1.CSI_ItemNumber", (ZShort)1, additionalInfo1.CSI_ItemNumber);
				var additionalInfo2 = exitReportItem.AdditionalInfos.AddNew();
				AssertEquals("additionalInfo2.CSI_ItemNumber", (ZShort)2, additionalInfo2.CSI_ItemNumber);
				additionalInfo2.CSI_ItemNumber = 10;
				var additionalInfo3 = exitReportItem.AdditionalInfos.AddNew();
				AssertEquals("additionalInfo3.CSI_ItemNumber", (ZShort)11, additionalInfo3.CSI_ItemNumber);
			});
		}
	}
}
