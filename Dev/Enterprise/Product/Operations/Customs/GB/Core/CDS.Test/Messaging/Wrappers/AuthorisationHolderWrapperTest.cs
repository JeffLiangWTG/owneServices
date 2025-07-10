using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Testing;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	public class AuthorisationHolderWrapperTest : TestCaseWithFactory
	{
		public void TestAuthorisationHolderWrapperFieldsAreTruncated()
		{
			var helper = new DeclarationTestHelper(Factory);
			IAuthorisationHolder wrapper = AuthorisationHolderWrapper.New(helper.GetStringOfMaxSizePlusOneToTrim(17), "");
			AssertEquals("ID should be truncated to 17 characters", 17, wrapper.ID.Length);
		}
	}
}
