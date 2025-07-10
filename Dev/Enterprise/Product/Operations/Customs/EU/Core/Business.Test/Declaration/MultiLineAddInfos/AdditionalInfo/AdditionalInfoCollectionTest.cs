using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(AdditionalInfoCollection))]
	[TestedType(typeof(AdditionalInfoCollection<AdditionalInfo>))]
	public class AdditionalInfoCollectionTest : AdditionalInfoCollectionGenericTest<AdditionalInfo>
	{
		public void TestMaxCount_ExitDetail()
		{
			var exitDetail = Factory.New<CusExitDetail>();
			AssertEquals("MaxCount set to 99 for CusExitDetail", 99, exitDetail.AdditionalInfos.MaxCount);
		}

		public void TestSetDefaultsForNewChild_ExitDetail()
		{
			var exitDetail = Factory.New<CusExitDetail>();
			AssertEquals("CSI_SubType set to 'INF'", "INF", exitDetail.AdditionalInfos.AddNew().CSI_SubType);
		}

		public void TestAddNewWithCodeAndReferenceNumber()
		{
			var additionalInfoCollection = GetAdditionalInfoCollection();
			AssertEquals("[PRE-CONDITION] count before AddNew", 0, additionalInfoCollection.Count);
			var additionalInfo = additionalInfoCollection.AddNew("XYZ", "1234");
			AssertEquals("[PRE-CONDITION] count after AddNew", 1, additionalInfoCollection.Count);
			CombineAssertions("CSI_Code and CSI_ReferenceNumber", () =>
			{
				AssertSame("returned bizo", additionalInfo, additionalInfoCollection[0]);
				AssertEquals(nameof(additionalInfo.CSI_Code), "XYZ", additionalInfo.CSI_Code);
				AssertEquals(nameof(additionalInfo.CSI_ReferenceNumber), "1234", additionalInfo.CSI_ReferenceNumber);
			});
		}
	}
}
