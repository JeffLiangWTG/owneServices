using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using NctsRefCusCodeListLevelTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListLevelTypes;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsBillAdditionalDocumentReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(()
				=> new NctsBillAdditionalDocumentReadOnlyProvider(additionalDocument: null, baseReadOnlyProvider: null));

			AssertExceptionThrown<ArgumentNullException>(()
				=> new NctsBillAdditionalDocumentReadOnlyProvider(additionalDocument: additionalDocument, baseReadOnlyProvider: null));
		}

		public void TestCSI_Description_ReadOnly_INF()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("When CSI_SubType: INF, CSI_DescriptionInfo.ReadOnly", false, additionalDocument.CSI_DescriptionInfo.ReadOnly);
		}

		public void TestCSI_Description_ReadOnly_NotINF()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertPropertyIsReadOnlyWhenAttributeIsMissing(RefCusCodeListTypes.Codes.Code_TD44N, additionalDocument.CSI_DescriptionInfo, RefCusCodeListAttributeTypes.Complement);
		}

		public void TestCSI_ReferenceNumberReadOnly()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertPropertyIsReadOnlyWhenAttributeIsMissing(RefCusCodeListTypes.Codes.Code_AR44N, additionalDocument.CSI_ReferenceNumberInfo, RefCusCodeListAttributeTypes.Reference);
		}

		#region Implementation

		void AssertPropertyIsReadOnlyWhenAttributeIsMissing(ZString codeType, ZPropertyInfo propertyInfo, string attributeName)
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(Constants.CountryCodes.Germany, codeType, NctsRefCusCodeListLevelTypes.House, Factory, attributeName);

			CombineAssertions(() =>
			{
				additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals($"TypeCode has attribute '{attributeName}' = 'Y'", false, propertyInfo.ReadOnly);

				additionalDocument.CSI_Code = refCusCodeList2.ZZD_Code;
				AssertEquals($"TypeCode has attribute '{attributeName}' = 'N'", false, propertyInfo.ReadOnly);

				additionalDocument.CSI_Code = refCusCodeList3.ZZD_Code;
				AssertEquals($"TypeCode doesn't have attribute '{attributeName}'", true, propertyInfo.ReadOnly);

				additionalDocument.CSI_Code = ZString.Empty;
				AssertEquals("TypeCode is empty", true, propertyInfo.ReadOnly);

				additionalDocument.CSI_Code = "AAAA";
				AssertEquals("TypeCode is not in the list", false, propertyInfo.ReadOnly);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			additionalDocument = bill.AdditionalDocuments.AddNew();
		}

		NctsHeader nctsHeader;
		NctsBillAdditionalDocument additionalDocument;

		#endregion
	}
}
