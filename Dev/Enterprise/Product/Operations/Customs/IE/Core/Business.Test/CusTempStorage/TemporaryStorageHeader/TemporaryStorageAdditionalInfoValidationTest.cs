using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageAdditionalInfoValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckCSI_ReferenceNumber_BR20319()
		{
			var referenceNumberFormatErrorMessage = "[BR20319] 1D23 must be in the format of yyyyMMddHHmm.";
			addInfo.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			addInfo.CSI_ReferenceNumber = "2022-10-13 15:53";
			AssertHasMessageError("Reference not in correct format", addInfo.CSI_ReferenceNumberInfo, referenceNumberFormatErrorMessage);
			addInfo.CSI_ReferenceNumber = "202210131553";
			AssertNoMessageError("Reference must be in correct format", addInfo.CSI_ReferenceNumberInfo, referenceNumberFormatErrorMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			addInfo = (TemporaryStorageAdditionalInfo)bill.AdditionalInfos.AddNew();
		}
		TemporaryStorageAdditionalInfo addInfo;
	}
}
