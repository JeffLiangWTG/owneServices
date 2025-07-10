using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	public abstract class KRAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			var parent = GetNewAddInfo();
			AssertEquals(parent.Validation.Parent, parent);
		}

		protected abstract KRAddInfo GetNewAddInfo();
	}
}
