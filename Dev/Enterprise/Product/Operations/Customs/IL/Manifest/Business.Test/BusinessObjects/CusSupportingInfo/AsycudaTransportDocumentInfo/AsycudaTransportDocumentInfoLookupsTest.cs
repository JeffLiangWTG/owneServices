using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaTransportDocumentInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			TestPerSubType(
				ILAdditionalInfoSubTypeList.Codes.TransportDocument,
				"704, 705, 714, IL1, IL2, IL3, ILF, ILH",
				"IL.Business.CodeDescriptionPairLists.TransportDocsTypeList",
				() => new TransportDocsTypeList());
		}

		public void TestConditionList()
		{
			AssertType<ILBillConditionList>(lookups.ConditionList);
			var conditionList = Factory.GetCachedValue<ILBillConditionList>();

			AssertEquals("Should contain 4 items", 4, conditionList.Count);

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("27", "Door to door");
			expectedList.AddPair("28", "Door to pier");
			expectedList.AddPair("29", "Pier to door");
			expectedList.AddPair("30", "Pier to pier");
			AssertEquals("Elements should match", expectedList.ElementsAsString, conditionList.ElementsAsString);

			var transportDocument = Factory.New<AsycudaTransportDocumentInfo>();
			AssertSame("Condition list is cached", conditionList, transportDocument.Lookups.ConditionList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var transportDocument = bill.TransportDocuments.AddNew();
			lookups = transportDocument.Lookups;
		}

		void TestPerSubType<T>(string subType, string expectedCodeAsString, string cacheKey, GetValueDelegate<T> codeDescriptionPairListFactory) where T : CodeDescriptionPairList
		{
			var additionalInfo = Factory.New<AsycudaTransportDocumentInfo>();
			additionalInfo.CSI_SubType = subType;
			var codeList = additionalInfo.Lookups.CodeList as CodeDescriptionPairList;
			CombineAssertions(() =>
			{
				AssertNotNull("CodeList must be CodeDescriptionPairList", codeList);
				AssertEquals("Codes", expectedCodeAsString, codeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue(cacheKey, codeDescriptionPairListFactory), codeList);
			});
		}

		AsycudaTransportDocumentInfoLookups lookups;
	}
}
