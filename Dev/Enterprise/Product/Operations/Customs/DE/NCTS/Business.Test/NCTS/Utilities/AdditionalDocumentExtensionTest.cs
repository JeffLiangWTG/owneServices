using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	class AdditionalDocumentExtensionTest : TestCaseWithFactory
	{
		public void TestIsNotificationToCustomsOffice()
		{
			CombineAssertions(() =>
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalDocument.CSI_Code = DEReferenceConstants.AdditionalInfoCodes.T0000;
				AssertEquals("CSI_SubType <> INF, CSI_Code = T0000", false, AdditionalDocumentExtension.IsNotificationToCustomsOffice(additionalDocument));

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("CSI_SubType = INF, CSI_Code = T0000", true, AdditionalDocumentExtension.IsNotificationToCustomsOffice(additionalDocument));

				additionalDocument.CSI_Code = DEReferenceConstants.AdditionalInfoCodes.X0000;
				AssertEquals("CSI_SubType = INF, CSI_Code <> T0000", false, AdditionalDocumentExtension.IsNotificationToCustomsOffice(additionalDocument));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			additionalDocument = bill.AdditionalDocuments.AddNew();
		}
		NctsBillAdditionalDocument additionalDocument;
	}
}
