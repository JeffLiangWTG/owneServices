using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CusAuthorizationUsageCollectionExtensionTest : TestCaseWithFactory
{
	public void TestHasAuthorizationOftype()
	{
		var collection = new List<CusAuthorizationUsage>();
		var authorization1 = Factory.New<CusAuthorizationUsage>();

		authorization1.AGC_Code = "XXX";
		collection.Add(authorization1);
		AssertEquals("No Authorization of Type ARG expected", false, collection.HasAuthorizationOfType("ARG"));

		authorization1.AGC_Code = "ARG";
		collection.Add(authorization1);
		AssertEquals("Authorization of Type ARG expected", true, collection.HasAuthorizationOfType("ARG"));
	}
}
