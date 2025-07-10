using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.Testing
{
	public abstract class TestFormatStringFunction : TestCaseWithFactory
	{
		public abstract void TestMatch();
		public abstract void TestExecute();
	}
}
