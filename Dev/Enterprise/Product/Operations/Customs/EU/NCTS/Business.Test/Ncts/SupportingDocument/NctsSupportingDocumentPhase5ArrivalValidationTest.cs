using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsSupportingDocumentPhase5ArrivalValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCSI_ItemNumber()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(supportingDocument.CSI_ItemNumberInfo, RefCusCodeListAttributeTypes.ItemNumber);
		}

		public void TestCSI_ItemNumber_NotAnyAttribute()
		{
			AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(supportingDocument.CSI_ItemNumberInfo, RefCusCodeListAttributeTypes.ItemNumber);
		}

		public void TestCSI_ReferenceNumber()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(supportingDocument.CSI_ReferenceNumberInfo, RefCusCodeListAttributeTypes.Reference);
		}

		public void TestCSI_ReferenceNumber_NotAnyAttribute()
		{
			AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(supportingDocument.CSI_ReferenceNumberInfo, RefCusCodeListAttributeTypes.Reference);
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthForPhase5AndTransitionPeriod(supportingDocument.CSI_ReferenceNumberInfo, 70, 35);
			});
		}

		public void TestCSI_ReferenceNumber2()
		{
			AssertPropertyIsMandatoryWhenHasAttributeWithValueY(supportingDocument.CSI_ReferenceNumber2Info, RefCusCodeListAttributeTypes.Complement);
		}

		public void TestCSI_ReferenceNumber2_NotAnyAttribute()
		{
			AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(supportingDocument.CSI_ReferenceNumber2Info, RefCusCodeListAttributeTypes.Complement);
		}

		public void TestCheckCSI_ReferenceNumber2_MaxLength()
		{
			CombineAssertions(() =>
			{
				UniversalValidationHelperTest.AssertMaxLengthForPhase5AndTransitionPeriod(supportingDocument.CSI_ReferenceNumber2Info, 35, 26);
			});
		}

		public void TestCheckCSI_Code()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC44N, "CusCodeTypeDC44N");
			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_DC44N, "REF01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, Business.UniversalReferenceConstants.RefCusCodeListLevelTypes.House);
			Factory.Save();

			var targetInfo = supportingDocument.CSI_CodeInfo;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertErrorIfNotEntered(targetInfo, MandatoryValidation.MustBeEntered);
				ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "BAD", "REF01");
			});
		}

		public void TestCheckCSI_Satus()
		{
			var headerSupportingDocument = nctsHeader.ArrivalMovementHeader.SupportingDocuments.AddNew();
			headerSupportingDocument.CSI_Status = "INV";

			var goodsItemSupportingDocument = bill.ArrivalGoodsItems.AddNew().SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertErrorIfInvalidCode(headerSupportingDocument.CSI_StatusInfo, "INV", SupportingDocumentStatusList.Codes.DEC);
				ValidationTestHelper.AssertErrorIfInvalidCode(supportingDocument.CSI_StatusInfo, "INV", SupportingDocumentStatusList.Codes.DEC);
				ValidationTestHelper.AssertErrorIfInvalidCode(goodsItemSupportingDocument.CSI_StatusInfo, "INV", SupportingDocumentStatusList.Codes.DEC);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			bill = nctsHeader.Bills.AddNew();
			supportingDocument = bill.SupportingDocuments.AddNew();
		}
		NctsHeader nctsHeader;
		NctsBill bill;
		NctsSupportingDocument supportingDocument;

		void AssertPropertyIsMandatoryWhenHasAttributeWithValueY(ZPropertyInfo propertyInfo, string attributeName)
		{
			CusSupportingInfoTestHelper.AssertPropertyIsMandatoryWhenHasAttributeWithValueY(propertyInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.House, attributeName);
		}

		void AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(ZPropertyInfo propertyInfo, string attributeName)
		{
			CusSupportingInfoTestHelper.AssertPropertyIsNotMandatoryWhenHasNotAnyAttribute(propertyInfo, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, RefCusCodeListLevelTypes.House, attributeName);
		}
	}
}
