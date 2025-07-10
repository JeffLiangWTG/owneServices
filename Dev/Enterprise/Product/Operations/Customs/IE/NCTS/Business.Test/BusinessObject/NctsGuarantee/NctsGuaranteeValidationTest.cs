using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class NctsGuaranteeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPW_BondType()
		{
			var guarantee = Factory.New<NctsGuarantee>();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(guarantee.PW_BondTypeInfo);
		}
	}
}
