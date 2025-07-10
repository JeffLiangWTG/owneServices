using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class DocumentScanningModuleInitialiserTest : TestCaseWithFactory
	{
		public void TestCanLoadThroughSpring()
		{
			DocumentScanningModuleInitialiser initialiser = ObjectFactory.Get<DocumentScanningModuleInitialiser>();
			AssertNoExceptionThrown(delegate { initialiser.Initialise(); });
		}
	}
}
