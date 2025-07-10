using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	public abstract class XMLMessageTestHelper<T> : TestCaseWithFactory
	{
		public abstract string TestFilesPath { get; }
	}
}
