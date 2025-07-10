using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_SubType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(additionalInfo.CSI_SubTypeInfo);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(additionalInfo.CSI_ReferenceNumberInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.Reference);
		}

		public void TestCheckCSI_Description()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(additionalInfo.CSI_DescriptionInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.Complement);
		}

		public void TestCheckCSI_Description_NotificationToCustomsOffice()
		{
			var targetInfo = additionalInfo.CSI_DescriptionInfo;
			additionalInfo.CSI_Code = DEReferenceConstants.AdditionalInfoCodes.T0000;
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo, MandatoryValidation.YouHaveNotEntered, "When CSI_Code=T0000, CSI_SubType=INF");

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered, "When CSI_Code=T0000, CSI_SubType<>INF");

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = DEReferenceConstants.AdditionalInfoCodes.X0000;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetInfo, MandatoryValidation.YouHaveNotEntered, "When CSI_Code<>T0000, CSI_SubType=INF");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			additionalInfo = goodsItem.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		}
		NctsAdditionalInfo additionalInfo;

		void AssertPropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName)
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(Core.Constants.CountryCodes.Germany, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, Factory, attributeName);
			CombineAssertions(() =>
			{
				additionalInfo.CSI_Code = refCusCodeList1.ZZD_Code;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo);

				additionalInfo.CSI_Code = refCusCodeList2.ZZD_Code;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				additionalInfo.CSI_Code = refCusCodeList3.ZZD_Code;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				additionalInfo.CSI_Code = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);
			});
		}
	}
}
