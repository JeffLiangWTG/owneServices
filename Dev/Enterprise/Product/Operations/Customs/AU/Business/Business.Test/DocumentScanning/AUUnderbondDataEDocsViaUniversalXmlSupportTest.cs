using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUUnderbondDataEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadBusinessObjectFromCode()
		{
			var mAWB = Factory.New<CusMAWB>();
			var underbond1 = mAWB.Underbonds.AddNew();
			underbond1.C4_SendersMessageReference = "U00003023";
			var underbond2 = mAWB.Underbonds.AddNew();
			underbond2.C4_SendersMessageReference = "U00003024";
			Factory.Save();

			var loader = new AUUnderbondDataEDocsViaUniversalXmlSupport();
			CombineAssertions(() =>
			{
				AssertEquals("Load from code 'U00003023'", underbond1.PK, loader.LoadBusinessObjectFromCode(Factory, "U00003023")?.PK);
				AssertEquals("Load from code 'U00003024'", underbond2.PK, loader.LoadBusinessObjectFromCode(Factory, "U00003024")?.PK);
			});
		}
	}
}
