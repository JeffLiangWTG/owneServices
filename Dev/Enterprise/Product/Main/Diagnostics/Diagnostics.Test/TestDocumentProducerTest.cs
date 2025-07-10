using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Diagnostics.Testing
{
	public class TestDocumentProducerTest : TestCaseWithFactory
	{
		public void TestRunPrintTestDocument()
		{
			var uiProvider = new PrintTaskForcePreviewTestingUIProvider();
			using (Report.TemporarilyStopErrorsThrowingAnException())
			using (new PrintTaskUIProviderFactory.OverriderForTesting(uiProvider))
			{
				new TestDocumentProducer().RunPrintTestDocument();
			}

			AssertMultilineASCIIEquals("uiProvider.LastErrors", null, uiProvider.LastErrors);
			AssertContains("uiProvider.LastSheetRendered", "{E}-[165 mm]", uiProvider.LastSheetRendered);
		}

		public void TestRunHAWBTestDocument()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "nosecurity";

			StmMenuItem userMenuItem = Factory.New<StmMenuItem>();
			userMenuItem.SU_MenuName = "Laser HAWB";
			userMenuItem.SU_BusinessContext = "Shipment";
			userMenuItem.SU_IsSystemDefined = false;
			Factory.Save();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ADALV";

			var uiProvider = new PrintTaskUIProviderForTest();
			using (Report.TemporarilyStopErrorsThrowingAnException())
			using (new PrintTaskUIProviderFactory.OverriderForTesting(uiProvider))
			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				new TestDocumentProducer().RunHAWBTestDocument();
			}

			AssertMultilineASCIIEquals("uiProvider.LastErrors", null, uiProvider.testProvider.LastErrors);
			AssertContains("uiProvider.LastSheetRendered", "[Test HAWB]", uiProvider.testProvider.LastSheetRendered);
		}

		public void TestRunCoverSheetDocument()
		{
			var uiProvider = new PrintTaskForcePreviewTestingUIProvider();
			using (Report.TemporarilyStopErrorsThrowingAnException())
			using (new PrintTaskUIProviderFactory.OverriderForTesting(uiProvider))
			{
				new TestDocumentProducer().RunCoverSheetDocument();
			}

			AssertMultilineASCIIEquals("uiProvider.LastErrors", null, uiProvider.LastErrors);
			AssertContains("uiProvider.LastSheetRendered", "[Test Cover Sheet - Air LSE]", uiProvider.LastSheetRendered);
		}
	}
}
