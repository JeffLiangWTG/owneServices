using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	public class ELGExceptionTest : TestCase
	{
		public static void TestConstructor()
		{
			ELGException ex = new ELGException(ELGExceptionType.NoBranchDepartmentMappingsSetOrFound);
			AssertEquals(ELGExceptionType.NoBranchDepartmentMappingsSetOrFound, ex.Type);
			ex = new ELGException(ELGExceptionType.NoTransportModeAndChargeCodeMappingsSetOrFound);
			AssertEquals(ELGExceptionType.NoTransportModeAndChargeCodeMappingsSetOrFound, ex.Type);
			ex = new ELGException(ELGExceptionType.NoSageAccountCodeMappingsSetOrFound);
			AssertEquals(ELGExceptionType.NoSageAccountCodeMappingsSetOrFound, ex.Type);
			ex = new ELGException(ELGExceptionType.Unknown);
			AssertEquals(ELGExceptionType.Unknown, ex.Type);
		}
	}
}
