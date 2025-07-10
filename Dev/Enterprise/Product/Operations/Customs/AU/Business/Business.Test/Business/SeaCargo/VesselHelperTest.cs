using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class VesselHelperTest : TestCaseWithFactory
	{
		public void TestIsExist()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "test1";
			vessel1.RV_LloydsNumber = "number1";
			Factory.Save();

			Assert(VesselHelper.IsExist(Factory, "test1", "number1"));
			Assert(!VesselHelper.IsExist(Factory, "test1", "number2"));
			Assert(!VesselHelper.IsExist(Factory, "test1", ""));
			Assert(!VesselHelper.IsExist(Factory, "test2", "number1"));
			Assert(!VesselHelper.IsExist(Factory, "", "number1"));
		}
	}
}
