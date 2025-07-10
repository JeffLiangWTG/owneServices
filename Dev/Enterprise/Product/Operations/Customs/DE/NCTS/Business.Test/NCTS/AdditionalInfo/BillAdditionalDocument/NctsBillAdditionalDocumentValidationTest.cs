using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsBillAdditionalDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Description()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(additionalDocument.CSI_DescriptionInfo);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				ValidationTestHelper.AssertFieldIsNotMandatory(additionalDocument.CSI_DescriptionInfo);

				additionalDocument.CSI_Code = DEReferenceConstants.AdditionalInfoCodes.T0000;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(additionalDocument.CSI_DescriptionInfo);

				additionalDocument.CSI_SubType = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(additionalDocument.CSI_DescriptionInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			additionalDocument = bill.AdditionalDocuments.AddNew();
		}
		NctsBillAdditionalDocument additionalDocument;
	}
}
