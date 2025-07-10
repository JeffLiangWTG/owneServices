using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using DEReferenceConstants = Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NctsAdditionalInfoReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(()
				=> new NctsAdditionalInfoReadOnlyProvider(additionalDocument: null, baseReadOnlyProvider: null));

			AssertExceptionThrown<ArgumentNullException>(()
				=> new NctsAdditionalInfoReadOnlyProvider(additionalDocument: additionalInfo, baseReadOnlyProvider: null));
		}

		public void TestCSI_ReferenceNumber_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertPropertyIsReadOnlyWhenAttributeIsMissing(additionalInfo.CSI_ReferenceNumberInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.Reference);

				var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(
					Core.Constants.CountryCodes.Germany,
					EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N,
					EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item,
					Factory,
					DEReferenceConstants.RefCusCodeListAttributes.Name.Complement);

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("When CSI_SubType=INF and Reference=Y", true, additionalInfo.CSI_ReferenceNumberInfo.ReadOnly);
				additionalInfo.CSI_Code = refCusCodeList2.ZZD_Code;
				AssertEquals("When CSI_SubType=INF and Reference=N", true, additionalInfo.CSI_ReferenceNumberInfo.ReadOnly);
				additionalInfo.CSI_Code = refCusCodeList3.ZZD_Code;
				AssertEquals("When CSI_SubType=INF and Reference not set", true, additionalInfo.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_Description_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertPropertyIsReadOnlyWhenAttributeIsMissing(additionalInfo.CSI_DescriptionInfo, DEReferenceConstants.RefCusCodeListAttributes.Name.Complement);

				var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(Core.Constants.CountryCodes.Germany, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AI44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, Factory, DEReferenceConstants.RefCusCodeListAttributes.Name.Complement);
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("When CSI_SubType=INF and Complement=Y", false, additionalInfo.CSI_DescriptionInfo.ReadOnly);
				additionalInfo.CSI_Code = refCusCodeList2.ZZD_Code;
				AssertEquals("When CSI_SubType=INF and Complement=N", false, additionalInfo.CSI_DescriptionInfo.ReadOnly);
				additionalInfo.CSI_Code = refCusCodeList3.ZZD_Code;
				AssertEquals("When CSI_SubType=INF and Complement not set", false, additionalInfo.CSI_DescriptionInfo.ReadOnly);
			});
		}

		#region Implementation

		void AssertPropertyIsReadOnlyWhenAttributeIsMissing(ZPropertyInfo propertyInfo, string attributeName)
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(Core.Constants.CountryCodes.Germany, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_AR44N, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item, Factory, attributeName);

			additionalInfo.CSI_Code = refCusCodeList1.ZZD_Code;
			AssertEquals($"TypeCode has attribute '{attributeName}' = 'Y'", false, propertyInfo.ReadOnly);

			additionalInfo.CSI_Code = refCusCodeList2.ZZD_Code;
			AssertEquals($"TypeCode has attribute '{attributeName}' = 'N'", false, propertyInfo.ReadOnly);

			additionalInfo.CSI_Code = refCusCodeList3.ZZD_Code;
			AssertEquals($"TypeCode doesn't have attribute '{attributeName}'", true, propertyInfo.ReadOnly);

			additionalInfo.CSI_Code = ZString.Empty;
			AssertEquals("TypeCode is empty", true, propertyInfo.ReadOnly);

			additionalInfo.CSI_Code = "AAAA";
			AssertEquals("TypeCode is not in the list", false, propertyInfo.ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			additionalInfo = goodsItem.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		}

		NctsHeader nctsHeader;
		NctsAdditionalInfo additionalInfo;

		#endregion
	}
}
