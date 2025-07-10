using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class KeyDataPairValidationTest : BusinessObjectValidationTestCase
	{
		public void TestChineseCharactersAllowed()
		{
			var pair = new KeyDataPair();
			pair.Key = "蒙古牛肉"; // Mongolian Beef
			pair.Data = "蒙古牛肉"; // Kung Pao Chicken
			CombineAssertions(delegate
			{
				AssertNoErrors(pair.KeyInfo);
				AssertNoErrors(pair.DataInfo);
			});
		}
	}
}
