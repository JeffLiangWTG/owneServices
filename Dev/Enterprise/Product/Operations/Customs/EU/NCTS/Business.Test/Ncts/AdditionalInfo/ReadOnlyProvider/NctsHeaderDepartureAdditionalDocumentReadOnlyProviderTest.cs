using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefCusCodeListAttributeTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListAttributeTypes;
using RefCusCodeListTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderDepartureAdditionalDocumentReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderDepartureAdditionalDocumentReadOnlyProvider(additionalDocument: null));
		}

		public void TestCSI_ReferenceNumberReadOnly_NoAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			var typeCode = helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");
			var refCusCodeList1 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, "REF01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.Header);
			Factory.Save();

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var propertyInfo = additionalDocument.CSI_ReferenceNumberInfo;

			CombineAssertions(() =>
			{
				additionalDocument.CSI_Code = ZString.Empty;
				AssertEquals("CSI_Reference Number not disabled (REF), is empty code", false, propertyInfo.ReadOnly);

				additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("CSI_Reference Number disabled, as no reference Attribute", true, propertyInfo.ReadOnly);

				additionalDocument.CSI_Code = "AAA";
				AssertEquals("CSI_Reference Number not disabled (REF), is not in the list", false, propertyInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumberReadOnly_WithAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			var typeCode = helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");
			var refCusCodeList1 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, "REF01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Reference, RefCusCodeListAttributeValues.No);
			Factory.Save();

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var propertyInfo = additionalDocument.CSI_ReferenceNumberInfo;

			CombineAssertions("CSI_ReferenceNumber ReadOnly", () =>
			{
				additionalDocument.CSI_Code = "AAA";
				AssertEquals("CWhen CSI_Code is not in the list", false, propertyInfo.ReadOnly);

				additionalDocument.CSI_Code = ZString.Empty;
				AssertEquals("When CSI_Code is empty code", true, propertyInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("When CSI_Code is empty code, but SubType is REF", false, propertyInfo.ReadOnly);

				additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("When CSI_Code is filled with valid code", false, propertyInfo.ReadOnly);
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

		public void TestCSI_DescriptionReadOnly()
		{
			CombineAssertions("CSI_Description ReadOnly", () =>
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("When CSI_SubType is INF", false, additionalDocument.CSI_DescriptionInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("When CSI_SubType is REF", true, additionalDocument.CSI_DescriptionInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_ClearReadOnlyProperties()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			var typeCode = helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");
			var refCusCodeList1 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, "REF01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, "REF02", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Reference, RefCusCodeListAttributeValues.No);
			Factory.Save();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalDocument.CSI_ReferenceNumber = "Reference";
			additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;

			CombineAssertions(() =>
			{
				AssertEquals("CSI_ReferenceNumber isn't read-only, When Doc.Type Has Attribute", "Reference", additionalDocument.CSI_ReferenceNumber);

				additionalDocument.CSI_Code = "XXX";
				AssertEquals("CSI_ReferenceNumber isn't read-only, When Doc.Type is not in the list", "Reference", additionalDocument.CSI_ReferenceNumber);

				additionalDocument.CSI_Code = refCusCodeList2.ZZD_Code;
				AssertEquals("CSI_ReferenceNumber isn't read-only, When Doc.Type Has NOT Attribute", ZString.Empty, additionalDocument.CSI_ReferenceNumber);
			});
		}

		public void TestCSI_SubType_ClearReadOnlyProperties()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalDocument.CSI_Description = "Description";

			CombineAssertions(() =>
			{
				AssertEquals("CSI_Description isn't read-only", "Description", additionalDocument.CSI_Description);

				additionalDocument.CSI_SubType = "XXX";
				AssertEquals("CSI_Description is read-only", ZString.Empty, additionalDocument.CSI_Description);
			});
		}

		public void TestCSI_Code_DescriptionDisabled_WhenNCTS5DepartureAndDocTypeInListWithReference()
		{
			const string documentType = "GOOD";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");
			var refCusCodeList1 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, documentType, "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.Header);
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList1.PK, RefCusCodeListAttributeTypes.Reference, RefCusCodeListAttributeValues.Yes);
			Factory.Save();

			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.AdditionalReference, documentType, expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: false);
		}

		public void TestCSI_Code_DescriptionDisabled_WhenNCTS5DepartureAndDocTypeNotInList()
		{
			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.AdditionalReference, documentType: "BAD", expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: false);
		}

		public void TestCSI_Code_ReferenceAndDescriptionDisabled_WhenNCTS5DepartureAndDocTypeInListWithOutReference()
		{
			const string documentTypeInListWithOutReference = "NREF";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");
			var refCusCodeList1 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, documentTypeInListWithOutReference, "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.Header);
			Factory.Save();

			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: AdditionalInfoSubTypeList.Codes.AdditionalReference, documentTypeInListWithOutReference, expectedReferenceNumberIsEnable: false, expectedDescriptionIsEnable: false);
		}

		public void TestCSI_Code_ReferenceAndDescriptionDisabled_WhenNCTS5DepartureAndDocTypeNotInListAndDocKindNotInList()
		{
			AssertReferenceNumberAndDescriptionBehavior(additionalDocument, documentKind: "BAD", documentType: "BAD", expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: false);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			additionalDocument = nctsHeader.AdditionalDocuments.AddNew();
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
				AssertEquals("CSI_ReferenceNumber is cleared?", expectedReferenceNumberIsEnable ? expectedReferenceNumber : string.Empty, nctsAdditionalInfo.CSI_ReferenceNumber);

				AssertEquals("CSICSI_Description is disabled?", !expectedDescriptionIsEnable, nctsAdditionalInfo.CSI_DescriptionInfo.ReadOnly);
				AssertEquals("CSI_Description is cleared?", expectedDescriptionIsEnable ? expectedDescription : string.Empty, nctsAdditionalInfo.CSI_Description);
			});
		}

		NctsHeader nctsHeader;
		NctsAdditionalInfo additionalDocument;

		#endregion
	}
}
