using NUnit.Framework;

namespace Enterprise.Client.AWH.Testing
{
	public class AWHExceptionTest : TestCase
	{
		public void TestConstructor()
		{
			AWHException ex = new AWHException("ABC", AWHException.AWHExceptionType.BranchCodeMappingNotSet);
			AssertEquals("ABC", ex.BizObjName);
			AssertEquals(AWHException.AWHExceptionType.BranchCodeMappingNotSet, ex.Type);
			ex = new AWHException("EFG", AWHException.AWHExceptionType.DeptCodeMappingNotSet);
			AssertEquals("EFG", ex.BizObjName);
			AssertEquals(AWHException.AWHExceptionType.DeptCodeMappingNotSet, ex.Type);
			ex = new AWHException("IJK", AWHException.AWHExceptionType.LegacySystemCodeNotSet);
			AssertEquals("IJK", ex.BizObjName);
			AssertEquals(AWHException.AWHExceptionType.LegacySystemCodeNotSet, ex.Type);
		}
	}
}
