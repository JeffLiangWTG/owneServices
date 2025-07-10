using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Test
{
	[TestedType(typeof(CommunicationModeMigratorForm))]
	public class CommunicationModeMigratorFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CommunicationModeMigratorForm(Factory, CommunicationModeMigrator);
		}

		public CommunicationModeMigratorDataModel CommunicationModeMigrator
		{
			get
			{
				if (_communicationModeMigrator == null)
				{
					_communicationModeMigrator = new CommunicationModeMigratorDataModel(Factory);
				}
				return _communicationModeMigrator;
			}
		}
		public CommunicationModeMigratorDataModel _communicationModeMigrator;
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

		public void TestSaveAndCloseFunctionality()
		{
			var migrator = CommunicationModeMigrator;
			var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.BasicAuthentication);
			var edpMode = CreateNewEDICommunicationsMode(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface);
			var config = CreateNewEDICommunicationPartyConfig("OUT");
			migrator.ECP_PK = party.PK;
			migrator.CloseOnCompletion = true;
			party.Configs.Add(config);
			Factory.Save();
			using (var form = new CommunicationModeMigratorForm(Factory, CommunicationModeMigrator))
			{
				form.Show();
				AssertEquals(true, form.Visible);
				form.SendAndCloseButton_Click(null, EventArgs.Empty);
				AssertEquals(false, form.Visible);
			}
			edpMode.Reload();
			AssertEquals(config.PK, edpMode.EK_ECC_CommunicationPartyConfig);
		}

		public void TestSaveFailConfirmation()
		{
			var migrator = CommunicationModeMigrator;
			var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.BasicAuthentication);
			var edpMode = CreateNewEDICommunicationsMode(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface);
			var config = CreateNewEDICommunicationPartyConfig("OUT");
			migrator.ECP_PK = party.PK;
			party.Configs.Add(config);
			Factory.Save();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			using (var form = new CommunicationModeMigratorForm(Factory, CommunicationModeMigrator))
			{
				form.Show();
				form.SendAndCloseButton_Click(null, EventArgs.Empty);
			}
			edpMode.Reload();
			AssertEquals(ZGuid.Empty, edpMode.EK_ECC_CommunicationPartyConfig);
		}

		public void TestNoOutboundConfig()
		{
			var migrator = CommunicationModeMigrator;
			var party = CreateNewEDICommunicationParty(EDICommunicationPartyConfigDirectionsList.Codes.Outbound, EDICommunicationAuthModesList.Codes.BasicAuthentication);
			var edpMode = CreateNewEDICommunicationsMode(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface);
			migrator.ECP_PK = party.PK;
			Factory.Save();
			using (var form = new CommunicationModeMigratorForm(Factory, CommunicationModeMigrator))
			{
				form.Show();
				form.SendAndCloseButton_Click(null, EventArgs.Empty);
			}
			edpMode.Reload();
			AssertEquals(ZGuid.Empty, edpMode.EK_ECC_CommunicationPartyConfig);
		}

		public void TestOnlyOneEDIClientSelected()
		{
			var edpMode = CreateNewEDICommunicationsMode(EDICommunicationsModeCommsDirectionList.Codes.Transmit, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface);
			using (var form = new CommunicationModeMigratorForm(Factory, new CommunicationModeMigratorDataModel(Factory, edpMode.PK)))
			{
				form.Show();
				AssertEquals(edpMode.PK, form.userDataInput.ECP_PK);
			}
		}

		public void TestNoEDIClientSelected()
		{
			using (var form = new CommunicationModeMigratorForm(Factory, CommunicationModeMigrator))
			{
				form.Show();
				AssertEquals(ZGuid.Empty, form.userDataInput.ECP_PK);
			}
		}
	}
}
