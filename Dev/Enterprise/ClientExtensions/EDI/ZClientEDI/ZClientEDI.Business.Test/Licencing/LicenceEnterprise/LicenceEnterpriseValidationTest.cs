using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class LicenceEnterpriseValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckLE_EnterpriseCode()
		{
			var ent = Factory.NewWithValidTestData<LicenceEnterprise>();

			ent.LE_EnterpriseCode = "A";
			AssertHasError(ent.LE_EnterpriseCodeInfo, "Enterprise Code must be empty or 3 characters in length");

			ent.LE_EnterpriseCode = "4";
			AssertHasError(ent.LE_EnterpriseCodeInfo, "Enterprise Code must be empty or 3 characters in length");

			ent.LE_EnterpriseCode = "!!!";
			AssertHasError(ent.LE_EnterpriseCodeInfo, "Enterprise Code must consist of numbers and letters only");

			ent.LE_EnterpriseCode = "AAA";
			AssertNoErrors(ent.LE_EnterpriseCodeInfo);

			Factory.Save();

			var ent2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			ent2.LE_EnterpriseCode = "AAA";
			AssertHasError(ent2.LE_EnterpriseCodeInfo, "Code is in use");
		}
	}
}