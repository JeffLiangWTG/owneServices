using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusClassPartPivotAddInfo))]
	sealed class CusClassPartPivotAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestResetOtherQuarantineDetailFieldsWhenClearProduceType()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			var addInfo = pivot.AddInfo;
			addInfo.ZA_AQISProduceType_Hidden = "DAI";
			addInfo.ZA_AQISProduct_Hidden = "MIL";
			addInfo.ZA_AQISSupplementaryCode_Hidden = "DM";
			addInfo.ZA_AQISPackType_Hidden = "BB";
			addInfo.ZA_AQISPreservation_Hidden = "F";
			addInfo.ZA_AQISCutCode_Hidden = "1803";
			addInfo.ZA_AQISCategoryCode_Hidden = "CHS";

			addInfo.ZA_AQISProduceType_Hidden = "";
			AssertEquals("ZA_AQISProduct_Hidden is cleared", "", addInfo.ZA_AQISProduct_Hidden);
			AssertEquals("ZA_AQISSupplementaryCode_Hidden is cleared", "", addInfo.ZA_AQISSupplementaryCode_Hidden);
			AssertEquals("ZA_AQISPackType_Hidden is cleared", "", addInfo.ZA_AQISPackType_Hidden);
			AssertEquals("ZA_AQISPreservation_Hidden is cleared", "", addInfo.ZA_AQISPreservation_Hidden);
			AssertEquals("ZA_AQISCutCode_Hidden is cleared", "", addInfo.ZA_AQISCutCode_Hidden);
			AssertEquals("ZA_AQISCategoryCode_Hidden is cleared", "", addInfo.ZA_AQISCategoryCode_Hidden);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CusClassPartPivot>().AddInfo;
	}
}
