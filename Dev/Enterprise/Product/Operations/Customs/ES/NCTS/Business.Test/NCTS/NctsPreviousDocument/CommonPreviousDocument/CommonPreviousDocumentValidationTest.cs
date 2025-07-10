using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class CommonPreviousDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCSI_ReferenceNumber2NotMandatory()
		{
			var referenceNumber2Info = previousDocument.CSI_ReferenceNumber2Info;
			var (refCusCodeList1, _, _) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest("ES", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, "House", referenceNumber2Info.BizObj.Factory, "Complement");
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(referenceNumber2Info.HumanReadableName);
			previousDocument.CSI_Code = refCusCodeList1.ZZD_Code.Left(CusSupportingInfo.Schema.CSI_CodeMaxLength);
			ValidationTestHelper.AssertFieldIsNotMandatory(referenceNumber2Info, messageError, "Complement should not have error if empty");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			previousDocument = nctsBill.PreviousDocuments.AddNew();
		}
		CommonPreviousDocument previousDocument;
	}
}
