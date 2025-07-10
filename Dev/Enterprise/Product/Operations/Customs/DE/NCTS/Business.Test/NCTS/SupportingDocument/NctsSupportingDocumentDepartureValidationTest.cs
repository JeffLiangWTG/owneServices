using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsSupportingDocumentDepartureValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber_Mandatory()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(supportingDocument.CSI_ReferenceNumberInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.Reference);
		}

		public void TestCheckCSI_ItemNumber()
		{
			const string messageError = "You have not entered an Item Number (1-99999).";
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(supportingDocument.CSI_ItemNumberInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber, messageError);
		}

		public void TestCheckCSI_ReferenceNumber2_Mandatory()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(supportingDocument.CSI_ReferenceNumber2Info, DEReferenceConstants.RefCusCodeListAttributes.Name.Complement);
		}

		public void TestCSI_DescriptionMaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLength(supportingDocument.CSI_DescriptionInfo, 26);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			supportingDocument = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
		}
		NctsSupportingDocument supportingDocument;

		void AssertPropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName, string notificationText = MandatoryValidation.YouHaveNotEntered)
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(Core.Constants.CountryCodes.Germany, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header, Factory, attributeName);
			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(propertyInfo, notificationText);

				supportingDocument.CSI_Code = refCusCodeList2.ZZD_Code;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				supportingDocument.CSI_Code = refCusCodeList3.ZZD_Code;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				supportingDocument.CSI_Code = ZString.Empty;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);
			});
		}
	}
}
