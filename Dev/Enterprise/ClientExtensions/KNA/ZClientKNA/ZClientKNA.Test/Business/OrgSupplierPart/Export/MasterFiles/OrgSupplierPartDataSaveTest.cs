using CargoWise.EntityFramework.Testing;
using Enterprise.Client.KNA;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class OrgSupplierPartDataSaveTest : TestCaseWithFactory
	{
		public void TestImportThenExport()
		{
			TestHelper.ClearCustomsRecordsBeforeTesting();
			TestHelper.SetupEnvironment();
			TestImportThenExportCore();
		}

		protected abstract void TestImportThenExportCore();
		KNATestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new KNATestHelper(Factory));
			}
		}

		KNATestHelper testHelper;
	}
}
