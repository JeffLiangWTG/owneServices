using NUnit.Framework;

namespace Enterprise.Client.DP2.Testing
{
	public class DP2ExceptionTest : TestCase
	{
		public void TestConstructor()
		{
			DP2Exception ex = new DP2Exception("ABC", DP2Exception.DP2ExceptionType.BranchCodeMappingNotSet);
			AssertEquals("ABC", ex.BizObjName);
			AssertEquals(DP2Exception.DP2ExceptionType.BranchCodeMappingNotSet, ex.Type);
			ex = new DP2Exception("EFG", DP2Exception.DP2ExceptionType.DeptCodeMappingNotSet);
			AssertEquals("EFG", ex.BizObjName);
			AssertEquals(DP2Exception.DP2ExceptionType.DeptCodeMappingNotSet, ex.Type);
			ex = new DP2Exception("IJK", DP2Exception.DP2ExceptionType.LegacySystemCodeNotSet);
			AssertEquals("IJK", ex.BizObjName);
			AssertEquals(DP2Exception.DP2ExceptionType.LegacySystemCodeNotSet, ex.Type);
		}
	}
}
