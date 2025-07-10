using System;
using CargoWise.Definitions;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Client.OIA.Business;
using Enterprise.Client.OIA.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.OIA.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class OIAClientOverrideTest : ClientOverrideTest
	{
		public void TestClientOverride()
		{
			AssertEquals("Should Return OIA client", Clients.OIA, ClientOverride.Instance.Client);
			AssertEquals("Client Display Name", "OIA Global Logistics", ClientOverride.Instance.ClientDisplayName);
		}

		public void TestModuleOverrides()
		{
			ModuleOverrides moduleOverrides = ClientOverride.Instance.ModuleOverrides;
			AssertNotNull("ModuleOverrides should not be null", moduleOverrides);
			AssertNotNull("ModulesOverries should contain GLJournal", moduleOverrides[ModuleIDs.GLJournal, Core.Constants.CountryCodes.Australia]);
			AssertEquals("ModulesOverrite should be OIAGLJournalModule", typeof(OIAGLJournalModule).FullName, moduleOverrides[ModuleIDs.GLJournal, Core.Constants.CountryCodes.Australia].TypePath.Split(',')[0]);
		}

		public void TestInitialiseAndUnInitialise()
		{
			ClientOverride.Instance.Uninitialise();
			AssertEquals("CSVExportProcessor type", typeof(GLTransactionExportProcessor), GLTransactionExportProcessor.New().GetType());
			ClientOverride.Instance.Initialise();
			AssertEquals("CSVExportProcessor type", typeof(OIAGLExportProcessor), GLTransactionExportProcessor.New().GetType());
		}

		public void TestDbSchemaUpgradeInfo()
		{
			TestCaseHelper.RunClientDbCreateScripts();
			var dbSchemaUpgradeInfo = ClientOverride.Instance.DbSchemaExtensionObjects;
			AssertEquals(0, dbSchemaUpgradeInfo.TableCreationScripts.Length);
			AssertEquals(1, dbSchemaUpgradeInfo.ViewAndRoutineCreationScripts.Length);
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
