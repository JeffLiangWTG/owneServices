using System;
using System.Data;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
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
	sealed class NctsHeaderArrivalAdditionalDocumentReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsHeaderArrivalAdditionalDocumentReadOnlyProvider(additionalDocument: null));
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

			CombineAssertions("CSI_ReferenceNumber ReadOnly", () =>
			{
				additionalDocument.CSI_Code = ZString.Empty;
				AssertEquals("When CSI_Code is empty code", true, propertyInfo.ReadOnly);

				additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("When CSI_Code == ZZD_Code, with no reference Attribute", true, propertyInfo.ReadOnly);
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

			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var propertyInfo = additionalDocument.CSI_ReferenceNumberInfo;

			CombineAssertions("CSI_ReferenceNumber ReadOnly", () =>
			{
				additionalDocument.CSI_Code = ZString.Empty;
				AssertEquals("When CSI_Code is empty code", true, propertyInfo.ReadOnly);

				additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("When CSI_Code is filled with valid code", false, propertyInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber2ReadOnly()
		{
			AssertEquals("CSI_ReferenceNumber2 is not read-only", false, additionalDocument.CSI_ReferenceNumber2Info.ReadOnly);
		}

		public void TestCSI_StatusReadOnly()
		{
			CombineAssertions("CSI_Status ReadOnly", () =>
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("In NCTS5", true, additionalDocument.CSI_StatusInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("In NCTS4", false, additionalDocument.CSI_StatusInfo.ReadOnly);
			});
		}

		public void TestCSI_LineNoReadOnly()
		{
			CombineAssertions("CSI_LineNo ReadOnly", () =>
			{
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				AssertEquals("In NCTS5", true, additionalDocument.CSI_LineNoInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				AssertEquals("In NCTS4", false, additionalDocument.CSI_LineNoInfo.ReadOnly);
			});
		}

		public void TestCSI_SubTypeReadOnly()
		{
			CombineAssertions("CSI_SubType ReadOnly", () =>
			{
				additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("When NCTS5, CSI_Status = DEC", true, additionalDocument.CSI_SubTypeInfo.ReadOnly);

				additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("When NCTS5, CSI_Status = DIF", false, additionalDocument.CSI_SubTypeInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("When NCTS4, CSI_Status = DEC", false, additionalDocument.CSI_SubTypeInfo.ReadOnly);
			});
		}

		public void TestCSI_CodeReadOnly()
		{
			CombineAssertions("CSI_Code ReadOnly", () =>
			{
				additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("When NCTS5, CI_Status = DEC", true, additionalDocument.CSI_CodeInfo.ReadOnly);

				additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DIF;
				AssertEquals("When NCTS5, CI_Status = DIF", false, additionalDocument.CSI_CodeInfo.ReadOnly);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("When NCTS4, CSI_Status = DEC", false, additionalDocument.CSI_CodeInfo.ReadOnly);
			});
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

		public void TestCSI_AllPropertiesAreReadOnlyForEUNctsArrivalMovementHeader()
		{
			AssertEquals("Prerequisite: NctsArrivalMovementHeaderForTest is not readonly", false, additionalDocument.ReadOnly);

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			additionalDocument = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();

			AssertEquals("NctsArrivalMovementHeader is readonly", true, additionalDocument.ReadOnly);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			var arrivalMovementHeader = Factory.New<NctsArrivalMovementHeaderForTest>();

			nctsHeader.MovementHeaders.RemoveAll(match => true);
			nctsHeader.MovementHeaders.Add(arrivalMovementHeader);

			additionalDocument = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.AddNew();
		}

		NctsHeader nctsHeader;
		NctsAdditionalInfo additionalDocument;

		class NctsArrivalMovementHeaderForTest : NctsArrivalMovementHeader
		{
			public NctsArrivalMovementHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZBool ShouldSetAdditionalDocumentsReadOnly => false;
		}

		#endregion
	}
}
