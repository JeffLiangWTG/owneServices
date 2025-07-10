using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class DrawbackImportLicenseLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestModalityList()
		{
			var parent = Factory.New<DrawbackImportLicense>();
			var drawbackModalityList = parent.Lookups.DrawbackModalityList;
			CombineAssertions(() =>
			{
				AssertEquals(5, drawbackModalityList.Count);
				AssertEquals("1, 2, 3, 4, 5", drawbackModalityList.CodesAsString);
			});
		}
	}
}

