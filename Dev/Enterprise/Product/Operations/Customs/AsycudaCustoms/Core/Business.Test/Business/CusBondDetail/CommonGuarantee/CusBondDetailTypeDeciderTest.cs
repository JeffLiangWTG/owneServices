using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusBondDetailTypeDeciderTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var guarantee = instruction.Guarantee;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("Default", typeof(CusBondDetail), new BusinessObjectFactory().Load(typeof(CommonCusBondDetail), guarantee.PK).GetType());

				guarantee.PW_ApplicationCode = SecondCusBondDetail.ApplicationCode;
				Factory.Save();
				AssertEquals("SecondCusBondDetail", typeof(SecondCusBondDetail), new BusinessObjectFactory().Load(typeof(CommonCusBondDetail), guarantee.PK).GetType());
			});
		}
	}
}
