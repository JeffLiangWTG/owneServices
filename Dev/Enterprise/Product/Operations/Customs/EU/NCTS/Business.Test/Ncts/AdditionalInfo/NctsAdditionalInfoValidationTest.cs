using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code()
		{
			var validation = new NctsAdditionalInfoValidationForTest(goodItemAdditionalInfo);
			CombineAssertions("When IsCodeEnabled and ShouldCodeBeInTheList", () =>
			{
				validation.IsCodeEnabledExposed = true;
				validation.ShouldCodeBeInTheListExposed = true;
				goodItemAdditionalInfo.CSI_Code = "";
				validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining(goodItemAdditionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

				goodItemAdditionalInfo.CSI_Code = "XYZ";
				validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining(goodItemAdditionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
			});

			CombineAssertions("When IsCodeEnabled and !ShouldCodeBeInTheList", () =>
			{
				validation.IsCodeEnabledExposed = true;
				validation.ShouldCodeBeInTheListExposed = false;
				goodItemAdditionalInfo.CSI_Code = "";
				validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining(goodItemAdditionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

				goodItemAdditionalInfo.CSI_Code = "XYZ";
				validation.ValidateCSI_Code();
				AssertNoMessageErrorContaining(goodItemAdditionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
			});

			CombineAssertions("When !IsCodeEnabled", () =>
			{
				validation.IsCodeEnabledExposed = false;
				goodItemAdditionalInfo.CSI_Code = "";
				validation.ValidateCSI_Code();
				AssertNoMessageErrors(goodItemAdditionalInfo.CSI_CodeInfo);

				goodItemAdditionalInfo.CSI_Code = "XYZ";
				validation.ValidateCSI_Code();
				AssertNoMessageErrors(goodItemAdditionalInfo.CSI_CodeInfo);
			});
		}

		public void TestCheckCSI_ReferenceNumber_Mandatory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "CusCodeTypeTD44N");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "N235", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList1.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);

			var refCusCodeList2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "N236", "02 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList2.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);

			Factory.Save();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

			CombineAssertions(() =>
			{
				nctsHeaderAdditionalInfo.CSI_Code = "N235";
				nctsHeaderAdditionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertNoMessageErrorContaining("Reference Attribute N", nctsHeaderAdditionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeaderAdditionalInfo.CSI_Code = "N236";
				nctsHeaderAdditionalInfo.CSI_ReferenceNumber = ZString.Empty;
				AssertHasMessageErrorContaining("Reference Attribute Y", nctsHeaderAdditionalInfo.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCSI_Description_Mandatory()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderAdditionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(nctsHeaderAdditionalInfo.CSI_DescriptionInfo);

				nctsHeaderAdditionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes.T0000;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(nctsHeaderAdditionalInfo.CSI_DescriptionInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodItemAdditionalInfo = nctsHeader.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew();
			nctsHeaderAdditionalInfo = nctsHeader.AdditionalDocuments.AddNew();
		}

		NctsHeader nctsHeader;
		NctsAdditionalInfo goodItemAdditionalInfo;
		NctsAdditionalInfo nctsHeaderAdditionalInfo;

		class NctsAdditionalInfoValidationForTest : NctsAdditionalInfoValidation
		{
			public NctsAdditionalInfoValidationForTest(NctsAdditionalInfo parent) : base(parent)
			{
			}

			public bool IsCodeEnabledExposed { get; set; }

			protected override bool IsCodeEnabled => IsCodeEnabledExposed;

			public bool ShouldCodeBeInTheListExposed { get; set; }
			protected override bool ShouldCodeBeInTheList => ShouldCodeBeInTheListExposed;
		}
	}
}
