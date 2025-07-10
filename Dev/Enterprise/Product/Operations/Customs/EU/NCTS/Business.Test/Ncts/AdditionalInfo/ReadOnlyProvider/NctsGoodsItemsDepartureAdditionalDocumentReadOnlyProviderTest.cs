using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsGoodsItemsDepartureAdditionalDocumentReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsGoodsItemsDepartureAdditionalDocumentReadOnlyProvider(additionalDocument: null));
		}

		public void TestCSI_ReferenceNumberReadOnly()
		{
			var referencePropertyInfo = additionalDocument.CSI_ReferenceNumberInfo;
			CombineAssertions("CSI_ReferenceNumber ReadOnly", () =>
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("When NCTS5, CSI_SubType = REF", false, referencePropertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("When NCTS5, CSI_SubType = INF", true, referencePropertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocument.CSI_Code = "N235";
				AssertEquals("When NCTS5, CSI_SubType = TRA, CSI_Code = N235", false, referencePropertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocument.CSI_Code = "N236";
				AssertEquals("When NCTS5, CSI_SubType = TRA, CSI_Code = N236", false, referencePropertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocument.CSI_Code = "N237";
				AssertEquals("When NCTS5, CSI_SubType = TRA, CSI_Code = N237", true, referencePropertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocument.CSI_Code = "AAA";
				AssertEquals("When NCTS5, CSI_SubType = TRA, CSI_Code = AAA (not in list)", false, referencePropertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				additionalDocument.CSI_Code = "";
				AssertEquals("When NCTS5, CSI_SubType = TRA, CSI_Code = empty", true, referencePropertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = "ZZZ";
				AssertEquals("When NCTS5, CSI_SubType = ZZZ", true, referencePropertyInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("When NCTS4, CSI_SubType = REF", false, referencePropertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("When NCTS4, CSI_SubType = INF", true, referencePropertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals("When NCTS4, CSI_SubType = TRA", true, referencePropertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = "ZZZ";
				AssertEquals("When NCTS4, CSI_SubType = ZZZ", true, referencePropertyInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber2ReadOnly()
		{
			AssertEquals("CSI_ReferenceNumber2 isn't read-only", false, additionalDocument.CSI_ReferenceNumber2Info.ReadOnly);
		}

		public void TestCSI_StatusReadOnly()
		{
			AssertEquals("CSI_Status isn't read-only", false, additionalDocument.CSI_StatusInfo.ReadOnly);
		}

		public void TestCSI_LineNoReadOnly()
		{
			AssertEquals("CSI_LineNo isn't read-only", false, additionalDocument.CSI_LineNoInfo.ReadOnly);
		}

		public void TestCSI_CodeReadOnly()
		{
			AssertEquals("CSI_Code isn't read-only", false, additionalDocument.CSI_CodeInfo.ReadOnly);
		}

		public void TestCSI_SubTypeReadOnly()
		{
			AssertEquals("CSI_SubType isn't read-only", false, additionalDocument.CSI_SubTypeInfo.ReadOnly);
		}

		public void TestCSI_DescriptionReadOnly_WhenDocumentKindIsNotREF()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("CSI_LineNo isn't read-only", false, additionalDocument.CSI_DescriptionInfo.ReadOnly);
		}

		public void TestCSI_SubType_ReferenceDescriptionEnable_WhenAGoodItemNCTS5DepartureDocumentTypeInListAndDocKindIsREF()
		{
			var refCusCodeList1 = CreateDocTypeGoodItem();
			Factory.Save();

			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.AdditionalReference, documentType: refCusCodeList1.ZZD_Code, expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: true);
		}

		public void TestCSI_SubType_WhenGoodItemNCTS5DepartureDocKindIsTRADocTypeIsN237()
		{
			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.TransportDocument, documentType: "N237", expectedReferenceNumberIsEnable: false, expectedDescriptionIsEnable: false);
		}

		public void TestCSI_SubType_WhenGoodItemNCTS5DepartureDocKindIsTRADocTypeIsN236()
		{
			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.TransportDocument, documentType: "N236", expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: false);
		}

		public void TestCSI_SubType_WhenGoodItemNCTS5DepartureDocKindIsTRADocTypeIsN235()
		{
			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.TransportDocument, documentType: "N235", expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: false);
		}

		public void TestCSI_SubType_WhenGoodItemNCTS5DepartureDocKindIsTRADocTypeIsNotInList()
		{
			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.TransportDocument, documentType: "AAA", expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: false);
		}

		public void TestCSI_SubType_ReferenceAndDescriptionDisabled_WhenGoodItemNCTS5DepartureDocKindNotINFOrREFOrTRA()
		{
			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: "ZZZ", documentType: "A", expectedReferenceNumberIsEnable: false, expectedDescriptionIsEnable: false);
		}

		public void TestCSI_Code_MessageError_WhenGoodItemDepartureNCTS5DocTypeNotInList()
		{
			const string expectedErrorMessage = "The code you have selected is not in the list.";

			var refCusCodeList1 = CreateDocTypeGoodItem();
			Factory.Save();

			var cSI_CodeInfo = additionalDocument.CSI_CodeInfo;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;

			AssertNoMessageErrorContaining("When NCTS5 and DocType In List", cSI_CodeInfo, expectedErrorMessage);

			additionalDocument.CSI_Code = "BAD";
			AssertHasMessageErrorContaining("When NCTS5 and DocType Not In List", cSI_CodeInfo, expectedErrorMessage);
		}

		public void TestCSI_Code_ReferenceDisabled_WhenGoodItemNCTS5DepartureDocTypeNotInListAndDocKindIsINF()
		{
			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.AdditionalInformation, documentType: "BAD", expectedReferenceNumberIsEnable: false, expectedDescriptionIsEnable: true);
		}

		public void TestCSI_Code_DescriptionDisabled_WhenGoodItemNCTS5DepartureDocTypeNotInListAndDocKindIsREF()
		{
			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.AdditionalReference, documentType: "BAD", expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: true);
		}

		public void TestCSI_Code_ReferenceEnableDescriptionDisabled_WhenNCTS5DepartureKindREFAndDocTypeIsEmpty()
		{
			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.AdditionalReference, documentType: ZString.Empty, expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: false);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill = nctsHeader.Bills.AddNew();
			additionalDocument = bill.GoodsItems.AddNew().AdditionalInfos.AddNew();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "CusCodeTypeTD44N");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "N235", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList1.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListAttributeValues.No);

			var refCusCodeList2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "N236", "02 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList2.PK, UniversalReferenceConstants.RefCusCodeListAttributeTypes.Reference, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes);

			var refCusCodeList3 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_TD44N, "N237", "03 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList3.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item);

			Factory.Save();
		}

		void AssertReferenceNumberAndDescriptionBehavior(NctsAdditionalInfo nctsAdditionalInfo, string documentKind, string documentType, bool expectedReferenceNumberIsEnable, bool expectedDescriptionIsEnable)
		{
			const string expectedReferenceNumber = "MyReferenceNumber";
			const string expectedDescription = "MyDescription";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsAdditionalInfo.CSI_SubType = documentKind;
			nctsAdditionalInfo.CSI_ReferenceNumber = expectedReferenceNumber;
			nctsAdditionalInfo.CSI_Description = expectedDescription;

			CombineAssertions($"When NCTS Departure Phase 5 And Doc.Kind is {documentKind} and Doc.Type '{documentType}'", () =>
			{
				nctsAdditionalInfo.CSI_Code = documentType;

				AssertEquals("CSI_ReferenceNumber is disabled?", !expectedReferenceNumberIsEnable, nctsAdditionalInfo.CSI_ReferenceNumberInfo.ReadOnly);

				AssertEquals("CSICSI_Description is disabled?", !expectedDescriptionIsEnable, nctsAdditionalInfo.CSI_DescriptionInfo.ReadOnly);
			});
		}

		Universal.RefCusCodeList CreateDocTypeGoodItem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");

			var refCusCodeList1 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, RefCusCodeListTypes.Codes.Code_AR44N + "01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, "Level", RefCusCodeListLevelTypes.Item);
			return refCusCodeList1;
		}

		NctsHeader nctsHeader;
		NctsAdditionalInfo additionalDocument;

		#endregion
	}
}
