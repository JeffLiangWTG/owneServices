using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillsDepartureAdditionalDocumentReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsBillsDepartureAdditionalDocumentReadOnlyProvider(additionalDocument: null));
		}

		public void TestCSI_ReferenceNumberReadOnly()
		{
			CombineAssertions(() =>
			{
				additionalDocument.CSI_SubType = ZString.Empty;
				AssertEquals("ReferenceNumber should be readonly when Kind is empty", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("ReferenceNumber should be readonly when Kind is INF", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("ReferenceNumber should not be readonly wWhen Kind is REF", false, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber2ReadOnly()
		{
			AssertEquals("CSI_ReferenceNumber2 isn't read-only", false, additionalDocument.CSI_ReferenceNumber2Info.ReadOnly);
		}

		public void TestCSI_DescriptionReadOnly()
		{
			AssertEquals("When Kind of Document is empty", true, additionalDocument.CSI_DescriptionInfo.ReadOnly);
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("When Kind of Document is not empty", false, additionalDocument.CSI_DescriptionInfo.ReadOnly);
		}

		public void TestCSI_StatusReadOnly()
		{
			AssertEquals("CSI_Status is read-only", true, additionalDocument.CSI_StatusInfo.ReadOnly);
		}

		public void TestCSI_LineNoReadOnly()
		{
			AssertEquals("CSI_LineNo is read-only", true, additionalDocument.CSI_LineNoInfo.ReadOnly);
		}

		public void TestCSI_SubTypeReadOnly()
		{
			CombineAssertions("CSI_SubType ReadOnly", () =>
			{
				additionalDocument.CSI_Status = ZString.Empty;
				AssertEquals("Kind CSI_Status is Empty", false, additionalDocument.CSI_SubTypeInfo.ReadOnly);

				additionalDocument.CSI_Status = "NEW";
				AssertEquals("Kind CSI_Status is NEW", false, additionalDocument.CSI_SubTypeInfo.ReadOnly);

				additionalDocument.CSI_Status = "DEC";
				AssertEquals("Kind CSI_Status is DEC", true, additionalDocument.CSI_SubTypeInfo.ReadOnly);
			});
		}

		public void TestCSI_CodeReadOnly()
		{
			CombineAssertions("CSI_Code ReadOnly", () =>
			{
				additionalDocument.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.DEC;
				AssertEquals("When CSI_Status = DEC", true, additionalDocument.CSI_CodeInfo.ReadOnly);

				additionalDocument.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.NEW;
				AssertEquals("When CSI_Status = New", false, additionalDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_ReadOnlyProperties_WhenIsStatusDeclaredAndCSI_SubTypeIsEmpty()
		{
			additionalDocument.CSI_SubType = ZString.Empty;
			additionalDocument.CSI_ReferenceNumber = "REF";
			additionalDocument.CSI_Description = "test";
			CombineAssertions(() =>
			{
				additionalDocument.CSI_Code = "XXX";
				AssertEquals("When CSI_Code: XXX, CSI_ReferenceNumber is read-only", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
				AssertEquals("When CSI_Code: XXX, CSI_Description is read-only", true, additionalDocument.CSI_DescriptionInfo.ReadOnly);

				additionalDocument.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.DEC;
				additionalDocument.CSI_Code = "ABC";
				AssertEquals("When CSI_Status: DEC, CSI_Code: ABC, CSI_ReferenceNumber is read-only", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
				AssertEquals("When CSI_Status: DEC, CSI_Code: ABC, CSI_Description is read-only", true, additionalDocument.CSI_DescriptionInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_ReadOnlyProperties_WhenIsStatusDeclaredAndCSI_SubTypeIsNotEmpty()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalDocument.CSI_ReferenceNumber = "REF";
			additionalDocument.CSI_Description = "test";
			CombineAssertions(() =>
			{
				additionalDocument.CSI_Code = "XXX";
				AssertEquals("When CSI_Code: XXX, CSI_ReferenceNumber isn't read-only", false, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
				AssertEquals("When CSI_Code: XXX, CSI_Description is read-only", false, additionalDocument.CSI_DescriptionInfo.ReadOnly);

				additionalDocument.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.DEC;
				additionalDocument.CSI_Code = "ABC";
				AssertEquals("When CSI_Status: DEC, CSI_Code: ABC, CSI_ReferenceNumber is read-only", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
				AssertEquals("When CSI_Status: DEC, CSI_Code: ABC, CSI_Description is read-only", true, additionalDocument.CSI_DescriptionInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_ReferenceAndDescriptionEnabled_WhenNCTS5DepartureAndDocTypeInListAndKindInList()
		{
			const string documentTypeInList = "AR44N01";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListTypes.Codes.Code_AR44N, documentTypeInList, "AR44N01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.House);

			Factory.Save();

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertReferenceNumberAndDescriptionBehavior(documentKind: AdditionalInfoSubTypeList.Codes.AdditionalReference, documentType: documentTypeInList, expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: true);
		}

		public void TestCSI_Code_ReferenceDisabled_WhenNCTS5DepartureAndDocTypeNotInListAndKindIsINF()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertReferenceNumberAndDescriptionBehavior(documentKind: AdditionalInfoSubTypeList.Codes.AdditionalInformation, documentType: CodeNotInList, expectedReferenceNumberIsEnable: false, expectedDescriptionIsEnable: true);
		}

		public void TestCSI_Code_DescriptionDisabled_WhenNCTS5DepartureAndDocTypeNotInListAndKindIsREF()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertReferenceNumberAndDescriptionBehavior(documentKind: AdditionalInfoSubTypeList.Codes.AdditionalReference, documentType: CodeNotInList, expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: true);
		}

		public void TestCSI_Code_DescriptionDisabled_WhenNCTS5DepartureAndDocTypeNotInListAndKindIsTRA()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertReferenceNumberAndDescriptionBehavior(documentKind: AdditionalInfoSubTypeList.Codes.TransportDocument, documentType: CodeNotInList, expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: true);
		}

		public void TestCSI_SubType_ReferenceAndDescriptionDisabled_WhenNCTS5DepartureAndKindNotInList()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertReferenceNumberAndDescriptionBehavior(documentKind: CodeNotInList, documentType: ZString.Empty, expectedReferenceNumberIsEnable: false, expectedDescriptionIsEnable: false);
		}

		public void TestCSI_Code_DescriptionAndReferenceDisabled_WhenNCTS5DepartureAndDocTypeKindIsEmpty()
		{
			AssertReferenceNumberAndDescriptionBehavior(documentKind: ZString.Empty, documentType: CodeNotInList, expectedReferenceNumberIsEnable: false, expectedDescriptionIsEnable: false);
			AssertReferenceNumberAndDescriptionBehavior(documentKind: ZString.Empty, documentType: ZString.Empty, expectedReferenceNumberIsEnable: false, expectedDescriptionIsEnable: false);
		}

		public void TestCSI_Code_DescriptionDisabled_WhenNCTS5DepartureKindREFOrTRAAndDocTypeIsEmpty()
		{
			AssertReferenceNumberAndDescriptionBehavior(documentKind: AdditionalInfoSubTypeList.Codes.AdditionalReference, documentType: ZString.Empty, expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: false);
			AssertReferenceNumberAndDescriptionBehavior(documentKind: AdditionalInfoSubTypeList.Codes.TransportDocument, documentType: ZString.Empty, expectedReferenceNumberIsEnable: true, expectedDescriptionIsEnable: false);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

			nctsBill = nctsHeader.Bills.AddNew();
			additionalDocument = nctsBill.AdditionalDocuments.AddNew();
		}

		void AssertReferenceNumberAndDescriptionBehavior(string documentKind, string documentType, bool expectedReferenceNumberIsEnable, bool expectedDescriptionIsEnable)
		{
			const string expectedReferenceNumber = "MyReferenceNumber";
			const string expectedDescription = "MyDescription";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			additionalDocument.CSI_ReferenceNumber = expectedReferenceNumber;
			additionalDocument.CSI_Description = expectedDescription;

			CombineAssertions($"When NCTS Departure Phase 5 And Doc.Kind is {documentKind} and Doc.Type '{documentType}'", () =>
			{
				additionalDocument.CSI_SubType = documentKind;
				additionalDocument.CSI_Code = documentType;

				AssertEquals("CSI_ReferenceNumber is disabled?", !expectedReferenceNumberIsEnable, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);

				AssertEquals("CSICSI_Description is disabled?", !expectedDescriptionIsEnable, additionalDocument.CSI_DescriptionInfo.ReadOnly);
			});
		}

		const string CodeNotInList = "BAD";

		NctsHeader nctsHeader;
		NctsBill nctsBill;
		NctsBillAdditionalDocument additionalDocument;

		#endregion
	}
}
