using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZPropertyInfoGuidTest : TestCaseWithDummy
	{
		public void TestSetValueFromString()
		{
			Dummy.Z0_Guid = ZGuid.Empty;

			ZGuid guid = ZGuid.NewZGuid();
			AssertEquals(true, Dummy.Z0_GuidInfo.SetValueFromString(guid.ToString()));
			AssertEquals(guid, Dummy.Z0_Guid);

			AssertEquals(false, Dummy.Z0_GuidInfo.SetValueFromString("Invalid"));
			AssertEquals(guid, Dummy.Z0_Guid);

			AssertEquals(false, Dummy.Z0_GuidInfo.SetValueFromString("123"));
			AssertEquals(guid, Dummy.Z0_Guid);
		}
	}
}
