using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	abstract class CNAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			var parent = GetNewAddInfo();
			AssertEquals(parent.Validation.Parent, parent);
		}

		protected abstract AddInfo GetNewAddInfo();
	}
}
