using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	sealed class CusAuthorisationHelperTest : TestCaseWithFactory
	{
		public void TestFindAuthorisation()
		{
			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "HLD001";

			var authHeader1 = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authHeader1.CPH_Type = "ABC";
			authHeader1.CPH_OH_PermitHolder = holder.PK;
			authHeader1.CPH_Number = "Auth12345";

			var header = CusAuthorisationHelper.FindAuthorisationHeader(Factory, "ABC", holder.PK);

			AssertNotNull("Header found", header);
			AssertEquals("Correct Number", "Auth12345", header.CPH_Number);

			header = CusAuthorisationHelper.FindAuthorisationHeader(Factory, "QWE", holder.PK);
			AssertNull("Incorrect Type should not be found", header);

			header = CusAuthorisationHelper.FindAuthorisationHeader(Factory, "ABC", ZGuid.NewZGuid());
			AssertNull("Incorrect Holder should not be found", header);

			var authHeader2 = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authHeader2.CPH_Type = "ABC";
			authHeader2.CPH_OH_PermitHolder = holder.PK;
			authHeader2.CPH_Number = "Auth23456";

			header = CusAuthorisationHelper.FindAuthorisationHeader(Factory, "ABC", holder.PK);
			AssertNull("More than one match should return null", header);
		}
	}
}
