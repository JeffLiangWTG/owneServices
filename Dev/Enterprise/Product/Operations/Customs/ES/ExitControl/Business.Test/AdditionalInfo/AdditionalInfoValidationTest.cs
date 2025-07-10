using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	sealed class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCSI_ItemNumberNo0NoDuplicatedInExistReportItems()
		{
			var reportItem = Factory.New<CusExitReportItem>();
			var additionalInfo1 = reportItem.AdditionalInfos.AddNew();
			var additionalInfo2 = reportItem.AdditionalInfos.AddNew();
			CSI_ItemNumberNo0NoDuplicatedCommon((AdditionalInfo)additionalInfo1, (AdditionalInfo)additionalInfo2);
		}

		public void TestCSI_ItemNumberNo0NoDuplicatedInExistReport()
		{
			var report = Factory.New<CusExitReport>();
			var additionalInfo1 = report.AdditionalInfos.AddNew();
			var additionalInfo2 = report.AdditionalInfos.AddNew();
			CSI_ItemNumberNo0NoDuplicatedCommon((AdditionalInfo)additionalInfo1, (AdditionalInfo)additionalInfo2);
		}

		void CSI_ItemNumberNo0NoDuplicatedCommon(AdditionalInfo additionalInfo1, AdditionalInfo additionalInfo2)
		{
			var targetInfo1 = additionalInfo1.CSI_ItemNumberInfo;
			var targetInfo2 = additionalInfo2.CSI_ItemNumberInfo;
			var valueCannotBeZeroMessage = "Sequence Number cannot be zero.";
			var messageErrorNotBeDuplicated = "Sequence Number (1) should not be duplicated";
			CombineAssertions(() =>
			{
				additionalInfo1.CSI_ItemNumber = 0;
				AssertHasError("CSI_ItemNumber = 0", targetInfo1, valueCannotBeZeroMessage);
				additionalInfo1.CSI_ItemNumber = 1;
				AssertNoError("CSI_ItemNumber = 1", targetInfo1, valueCannotBeZeroMessage);

				additionalInfo2.CSI_ItemNumber = 1;
				AssertHasError("The test is for control of sequence duplicated", targetInfo2, messageErrorNotBeDuplicated);
				additionalInfo2.CSI_ItemNumber = 2;
				AssertNoError("No test sequence duplicated", targetInfo2, messageErrorNotBeDuplicated);
			});
		}
	}
}
