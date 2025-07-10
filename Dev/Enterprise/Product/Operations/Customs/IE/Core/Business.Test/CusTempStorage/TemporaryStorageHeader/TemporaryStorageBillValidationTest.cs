using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageBillValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestCheckABL_BillNumber_BR20319()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var billNumberValidateErrorMessage = "[BR20319] You have not entered a 1D23 additional reference under the Additional Information grid.";
			var addInfo = (TemporaryStorageAdditionalInfo)bill.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var addInfo2 = (TemporaryStorageAdditionalInfo)bill.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture;
			addInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

			header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			bill.Validation.ValidateABL_BillNumber();
			AssertNoMessageError("Bill Number level valid", bill.ABL_BillNumberInfo, billNumberValidateErrorMessage);

			header.AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V2;
			bill.Validation.ValidateABL_BillNumber();
			AssertHasMessageError("Bill Number level invalid", bill.ABL_BillNumberInfo, billNumberValidateErrorMessage);
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			bill.Validation.ValidateABL_BillNumber();
			AssertNoMessageError("Bill Number level valid", bill.ABL_BillNumberInfo, billNumberValidateErrorMessage);
		}
	}
}
