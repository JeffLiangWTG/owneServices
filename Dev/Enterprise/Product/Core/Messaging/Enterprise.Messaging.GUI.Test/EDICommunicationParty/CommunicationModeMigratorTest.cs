using System;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Test
{
	[TestedType(typeof(CommunicationModeMigrator))]
	public class CommunicationModeMigratorTest : TestCaseWithFactory
	{
		public EDICommunicationsMode CreateNewEDICommunicationsMode(string direction, string transport)
		{
			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = direction;
			communicationsMode.EK_CommunicationsTransport = transport;
			return communicationsMode;
		}
		public EDICommunicationParty CreateNewEDICommunicationParty(string direction, string authMode)
		{
			var ediCommunicationParty = Factory.NewWithValidTestData<EDICommunicationParty>();
			ediCommunicationParty.ECP_Name = $"Test Party {Guid.NewGuid()}";

			var ediCommunicationPartyConfig = direction == EDICommunicationPartyConfigDirectionsList.Codes.Inbound ?
				ediCommunicationParty.InboundConfig : ediCommunicationParty.OutboundConfig;

			ediCommunicationPartyConfig.Auth.ECA_ClientID = "Test Client ID";
			ediCommunicationPartyConfig.Auth.ECA_AuthorizationEndpoint = "Test Endpoint";
			ediCommunicationPartyConfig.Auth.ECA_Username = "JarJarBinks";
			ediCommunicationPartyConfig.Auth.ECA_AuthorizationMode = authMode;

			return ediCommunicationParty;
		}

		public EDICommunicationPartyConfig CreateNewEDICommunicationPartyConfig(string direction)
		{
			var communicationPartyConfig = Factory.New<EDICommunicationPartyConfig>();
			communicationPartyConfig.ECC_Direction = direction;
			communicationPartyConfig.ECC_IsActive = true;
			return communicationPartyConfig;
		}

		public void TestOrganizationCount()
		{
			var migrator = new CommunicationModeMigratorDataModel(Factory);
			AssertEquals(0, CommunicationModeMigrator.OrganizationCount(migrator.Factory));
			var edpMode = CreateNewEDICommunicationsMode(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface);
			Factory.Save();
			AssertEquals(1, CommunicationModeMigrator.OrganizationCount(migrator.Factory));
			var hubMode = CreateNewEDICommunicationsMode(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);
			Factory.Save();
			AssertEquals(1, CommunicationModeMigrator.OrganizationCount(migrator.Factory));
		}

		public void TestSaveFunctionality()
		{
			var migrator = new CommunicationModeMigratorDataModel(Factory);
			var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.BasicAuthentication);
			var edpMode = CreateNewEDICommunicationsMode(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface);
			var config = CreateNewEDICommunicationPartyConfig("OUT");
			party.Configs.Add(config);
			Factory.Save();
			AssertEquals(ZGuid.Empty, edpMode.EK_ECC_CommunicationPartyConfig);
			migrator.ECP_PK = party.PK;
			var log = new ActionLog();
			CommunicationModeMigrator.Run(log, Factory.Load<EDICommunicationParty>(migrator.ECP_PK)?.CurrentOutboundConfig?.PK, migrator.Factory);
			edpMode.Reload();
			AssertEquals(config.PK, edpMode.EK_ECC_CommunicationPartyConfig);
			log.Dispose();
		}

		public void TestSaveEmptyFunctionality()
		{
			var migrator = new CommunicationModeMigratorDataModel(Factory);
			var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.BasicAuthentication);
			var edpMode = CreateNewEDICommunicationsMode(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface);
			edpMode.EK_ECC_CommunicationPartyConfig = party.OutboundConfig.PK;
			Factory.Save();
			AssertEquals(party.OutboundConfig.PK, edpMode.EK_ECC_CommunicationPartyConfig);
			var log = new ActionLog();
			CommunicationModeMigrator.Run(log, Factory.Load<EDICommunicationParty>(migrator.ECP_PK)?.CurrentOutboundConfig?.PK, migrator.Factory);
			edpMode.Reload();
			AssertEquals(ZGuid.Empty, edpMode.EK_ECC_CommunicationPartyConfig);
			log.Dispose();
		}

		public void TestProcessLog()
		{
			var migrator = new CommunicationModeMigratorDataModel(Factory);
			var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.BasicAuthentication);
			var edpMode = CreateNewEDICommunicationsMode(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface);
			var config = CreateNewEDICommunicationPartyConfig("OUT");
			party.Configs.Add(config);
			edpMode.EK_ECC_CommunicationPartyConfig = party.OutboundConfig.PK;
			Factory.Save();
			migrator.ECP_PK = party.PK;
			using (var log = new ActionLog())
			{
				RichTextBox textBox = (RichTextBox)log.Controls["logTextBox"];
				CommunicationModeMigrator.Run(log, Factory.Load<EDICommunicationParty>(migrator.ECP_PK)?.CurrentOutboundConfig?.PK, migrator.Factory);

				AssertEquals("Start updating\nFinish updating on 1 organizations"
#if !WINZOR
  +	"\n"
#endif
				, textBox.Text);
				textBox.Dispose();
			}
		}

		public void TestProcessLogWithException()
		{
			using (DbConnection connection = new ConnectionWithTinyTimeoutForTest(Db.ServerName, Db.DatabaseName))
			{
				var bizoFactory = new BusinessObjectFactory(connection);
				var migrator = new CommunicationModeMigratorDataModel(Factory);
				var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.BasicAuthentication);
				var edpMode = CreateNewEDICommunicationsMode(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface);
				var config = CreateNewEDICommunicationPartyConfig("OUT");
				party.Configs.Add(config);
				edpMode.EK_ECC_CommunicationPartyConfig = party.OutboundConfig.PK;
				Factory.Save();
				migrator.ECP_PK = party.PK;
				using (var log = new ActionLog())
				{
					RichTextBox textBox = (RichTextBox)log.Controls["logTextBox"];
					CommunicationModeMigrator.Run(log, Factory.Load<EDICommunicationParty>(migrator.ECP_PK)?.CurrentOutboundConfig?.PK, bizoFactory);

					AssertEquals("Start updating\nUpdate failed : Lock request time out period exceeded.\nThe statement has been terminated."
#if !WINZOR
  +	"\n"
#endif
					, textBox.Text);
					textBox.Dispose();
				}
			}
		}
	}
}
