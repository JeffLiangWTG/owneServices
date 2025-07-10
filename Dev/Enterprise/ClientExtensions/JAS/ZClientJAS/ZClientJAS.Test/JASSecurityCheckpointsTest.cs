using CargoWise.EntityFramework.Testing;
using Enterprise.Client.JAS.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.JAS.Testing
{
	class JASSecurityCheckpointsTest : TestCaseWithFactory
	{
		public void TestJASSpecific()
		{
			SecurityCheckpoint jasSpecific = SecurityInstance.FindCheckPoint(new CheckpointLookupKey(ClientModuleRegistration.IDs.JASSpecific));
			AssertEquals("Display Name", ClientModuleRegistration.Names.JASSpecific, jasSpecific.DisplayText);
			AssertEquals(Env.Security.Operations.Code, jasSpecific.Parent.Code);
		}

		public void TestExportPrematchingTransactions()
		{
			SecurityCheckpoint exportPrematchingTransactions = SecurityInstance.FindCheckPoint(new CheckpointLookupKey(ClientModuleRegistration.IDs.ExportPrematchingTransactions));
			AssertEquals("Display Name", ClientModuleRegistration.Names.ExportPrematchingTransactions, exportPrematchingTransactions.DisplayText);
			AssertEquals(JASSecurityCheckpoints.JASSpecific.Code, exportPrematchingTransactions.Parent.Code);
		}

		public void TestImportMatchedTransactions()
		{
			SecurityCheckpoint importMatchedTransactions = SecurityInstance.FindCheckPoint(new CheckpointLookupKey(ClientModuleRegistration.IDs.ImportMatchedTransactions));
			AssertEquals("Display Name", ClientModuleRegistration.Names.ImportMatchedTransactions, importMatchedTransactions.DisplayText);
			AssertEquals(JASSecurityCheckpoints.JASSpecific.Code, importMatchedTransactions.Parent.Code);
		}

		public void TestImportJXCFile()
		{
			SecurityCheckpoint importJXCFile = SecurityInstance.FindCheckPoint(new CheckpointLookupKey(ClientModuleRegistration.IDs.ImportJXCFile));
			AssertEquals("Display Name", ClientModuleRegistration.Names.ImportJXCFile, importJXCFile.DisplayText);
			AssertEquals(JASSecurityCheckpoints.JASSpecific.Code, importJXCFile.Parent.Code);
		}

		public void TestExportCognosFile()
		{
			SecurityCheckpoint exportCognosFile = SecurityInstance.FindCheckPoint(new CheckpointLookupKey(ClientModuleRegistration.IDs.ExportCognosFile));
			AssertEquals("Display Name", ClientModuleRegistration.Names.ExportCognosFile, exportCognosFile.DisplayText);
			AssertEquals(JASSecurityCheckpoints.JASSpecific.Code, exportCognosFile.Parent.Code);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestSecurityCollection = new GlbSecurityCollection(GlbStaff.CurrentUser, Factory);
			SecurityInstance = new SecurityCore(TestSecurityCollection, GlbStaff.CurrentUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
		}

		GlbSecurityCollection TestSecurityCollection;
		SecurityCore SecurityInstance;
	}
}
