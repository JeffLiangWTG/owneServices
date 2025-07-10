using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	public abstract class CAAddInfoValidationTest<T> : BusinessObjectValidationTestCase
			where T : AddInfo
	{
		public void TestParent()
		{
			T parent = GetNewAddInfo();
			AssertEquals(parent.Validation.Parent, parent);
		}

		protected abstract T GetNewAddInfo();
	}
}
