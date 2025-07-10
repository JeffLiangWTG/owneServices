using System.Collections;
using System.Text;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	/// <summary>
	/// Summary description for ConfigurationItemChecker.
	/// </summary>
	public class ConfigurationItemChecker : IPostLoginTask
	{
		public string TaskDescription
		{
			get { return Res.GetString("7851405a-da25-4354-a8e6-defe7e90f8e4", "Configuration Item Checker"); }
		}

		public ConfigurationItemChecker()
		{
			fMissingConfigurationItemList = new ArrayList();
		}

		public static void CheckConfigurationItems()
		{
			if (!Globals.IsTest)
			{
				var configurationItemChecker = new ConfigurationItemChecker();
				configurationItemChecker.CheckMissingConfigurationItems();

				foreach (MissingConfigurationItem missingItem in configurationItemChecker.fMissingConfigurationItemList)
				{
					string configurationMessage = configurationItemChecker.GetConfigurationMessage(missingItem);
					if (Globals.Message.Show(configurationMessage, missingItem.ItemName, missingItem.Buttons, ZMessageBoxIcon.Warning) == ZDialogResult.Cancel)
					{
						break;
					}
				}
			}
		}

		class MissingConfigurationItem
		{
			public string ItemName = "";
			public string Description = "";
			public string HowToRectify = "";
			public string Problem = "";
			public ZMessageBoxButtons Buttons = ZMessageBoxButtons.OKCancel;
		}

		void AddMissingConfigurationItem(string itemName, string description, string problem, string howToRectify, ZMessageBoxButtons buttons)
		{
			var configItem = new MissingConfigurationItem();
			configItem.ItemName = itemName;
			configItem.Description = description;
			configItem.HowToRectify = howToRectify;
			configItem.Problem = problem;
			configItem.Buttons = buttons;

			fMissingConfigurationItemList.Add(configItem);
		}

		IProductRegistration ProductRegInstance
		{
			get
			{
				if (productRegInstance == null)
				{
					productRegInstance = CargoWise.Application.ObjectFactory.Get<IProductRegistration>();
				}
				return productRegInstance;
			}
		}
		IProductRegistration productRegInstance;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		static string NotRegisteredProblemMessage
		{
			get { return Res.GetString("57823559-13E4-4D91-AE47-1122A5A6DB0F", "Service tasks will be disabled."); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		static string NotRegisteredResolutionMessage
		{
			get { return Res.GetString("C66DDA88-9703-4C58-BEEC-B0582111245E", "Register from the main screen menu > Help > Register Product."); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		string InvalidRegistrationCaption
		{
			get { return Res.GetString("BD6D9BDD-45B3-4546-84BF-C03673DC7E1C", "Invalid Registration"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		string ProductKeyLockedToAnotherServerAndDbMessage
		{
			get
			{
				return Res.GetString("76F50A2E-C812-4357-86A3-B7FD3351BCB6", "This installation is no longer registered.\r\nThe product key provided is locked to another server and database:\r\n\t{0} {1}.", ProductRegInstance.Key.ServerName, ProductRegInstance.Key.DatabaseName);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		string ProductKeyDeactivatedOrMissingMessage
		{
			get { return Res.GetString("2CFBC081-642B-4CD4-AD2F-B091D087EA6F", "This installation is no longer registered.\r\nThe product key provided has been deactivated or could not be found."); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		string UnregisteredCaption
		{
			get { return Res.GetString("3D7AC9FB-DE55-477E-B295-C727FA263012", "Unregistered"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		string InstallationNotRegisteredMessage
		{
			get { return Res.GetString("89EDCCA3-BAAA-4A94-92F1-2CEE8A635B07", "This installation is not registered."); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		string MissingEmailDestinationItemNameForReleaseMessage
		{
			get { return Res.GetString("391b62e7-6c57-4b23-8de4-8cfbdf32eeda", "Missing Email Destination Override"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		string MissingEmailDestinationDescriptionForReleaseMessage
		{
			get { return Res.GetString("1627a607-2383-439d-bd3b-23c7c7bfd304", "The Email Destination Override hasn't been set. It is required on non-production systems."); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		string MissingEmailDestinationProblemMsgForReleaseMessage
		{
			get { return Res.GetString("6ec407b6-a4f5-424d-bd2d-ca0b4b1d6192", "No emails will be sent until this setting has been set."); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used in release mode")]
		string MissingEmailDestinationHowToRectifyMsgForReleaseMessage
		{
			get { return Res.GetString("4295147c-1efb-4862-98e4-ad0273a6f72f", "Please set this configuration in Registry > System > Testing > Email Destination Override."); }
		}

		void CheckMissingConfigurationItems()
		{
			IEnvironment env = EnvProxy.Instance;

			// let's not nag developers
#if !DEBUG
			{
				var regoResult = ProductRegInstance.LocalVerify();
				if (regoResult == Enterprise.Integration.Licensing.ProductRegistrationVerifyResult.Fail
					|| regoResult == Enterprise.Integration.Licensing.ProductRegistrationVerifyResult.NotFound)
				{
					AddMissingConfigurationItem(
						InvalidRegistrationCaption,
						regoResult == Enterprise.Integration.Licensing.ProductRegistrationVerifyResult.Fail
							? ProductKeyLockedToAnotherServerAndDbMessage
							: ProductKeyDeactivatedOrMissingMessage,
						NotRegisteredProblemMessage,
						NotRegisteredResolutionMessage,
						ZMessageBoxButtons.OK);
				}
				else if (regoResult == Enterprise.Integration.Licensing.ProductRegistrationVerifyResult.Unregistered)
				{
					AddMissingConfigurationItem(
						UnregisteredCaption,
						InstallationNotRegisteredMessage,
						NotRegisteredProblemMessage,
						NotRegisteredResolutionMessage,
						ZMessageBoxButtons.OK);
				}
			}
#endif

			if (!env.CurrentUser.IsDeveloperLogin)
			{
				var mailboxEmailAddress = env.Registry.MailboxEmailAddress;

				if (mailboxEmailAddress.Length == 0)
				{
					AddMissingConfigurationItem(Res.GetString("14a9fe8f-13e0-472e-bc7b-45ddf4877a8f", "Missing Email Address"),
												Res.GetString("bf70aa9d-23f4-4284-9284-1736cdef51dd", "The Email address hasn't been set."),
												Res.GetString("e1242268-c46e-4d06-b7d7-4bbb01d53e57", "{0} may not be able to send & receive emails.", Constants.ProductName),
												Res.GetString("856207dd-5c2d-4ea8-a2fc-ffa8b99eb60e", "Change the configuration in the registry."), ZMessageBoxButtons.OKCancel);
				}

				if (mailboxEmailAddress == "Default@edi.com.au")
				{
					AddMissingConfigurationItem(Res.GetString("14a9fe8f-13e0-472e-bc7b-45ddf4877a8f", "Missing Email Address"),
												Res.GetString("022c6dc6-f318-419c-b91d-b68851568fd9", "The Email address is set to the default address."),
												Res.GetString("e1242268-c46e-4d06-b7d7-4bbb01d53e57", "{0} may not be able to send & receive emails.", Constants.ProductName),
												Res.GetString("856207dd-5c2d-4ea8-a2fc-ffa8b99eb60e", "Change the configuration in the registry."), ZMessageBoxButtons.OKCancel);
				}

				if (env.Registry.MailServer.Length == 0 && !env.Registry.UseGraphApiForIncoming)
				{
					AddMissingConfigurationItem(Res.GetString("5082abab-bc31-440b-b56a-52ea1b4809ac", "Missing Mail Server"),
												Res.GetString("3f79d84c-b199-4490-93a4-828274cd0349", "The Incoming Server name has not been set."),
												Res.GetString("36167832-82ea-4af9-b625-01659c506d7f", "{0} may not be able to receive emails and will reschedule the Inbound Mail Service Task.", Constants.ProductName),
												Res.GetString("20570643-7ee2-481d-bdbb-8db3272786a6", "Change the configuration in the registry at Registry -> {0} and the Service Task will automatically resume running after {1} seconds.", RawDataRegistry.Instance.MailServer.GetLocation(),
													RegistryRefresh.FrequencyInSeconds), ZMessageBoxButtons.OKCancel);
				}

				if (env.Registry.SMTPServer.Length == 0 && !env.Registry.UseGraphApiForOutgoing && !ObjectFactory.Get<ISystemDataRegistry>().RunOMSInSimulationMode)
				{
					AddMissingConfigurationItem(Res.GetString("0369c7f9-d74d-40f4-ada7-8f36f8c5fa5e", "Missing SMTP Server"),
												Res.GetString("d38611db-c633-45e8-b1f1-58137200517e", "The Outgoing Server name has not been set."),
												Res.GetString("4cd16320-74fb-4145-bccb-3d95f69383fc", "{0} may not be able to send emails and will reschedule the Outbound Mail Service Task.", Constants.ProductName),
												Res.GetString("b592eff4-7dec-495f-82ee-52bca7eab28e", "Change the configuration in the registry at Registry -> {0} and the Service Task will automatically resume running after {1} seconds.", RawDataRegistry.Instance.SMTPServer.GetLocation(),
													RegistryRefresh.FrequencyInSeconds), ZMessageBoxButtons.OKCancel);
				}

				if (!env.CurrentUser.IsOperational)
				{
					AddMissingConfigurationItem(Res.GetString("8aa14ae5-5f31-4cab-b5f6-1c81b62e8d6b", "Non-Operational Login"),
												Res.GetString("8446d761-9cb4-41d3-9284-d2d9a44e9871", "Non-Operational logins should be used only to grant or deny system administration rights to other users."),
												"",
												Res.GetString("6f07b27e-55fd-4d9d-ac9f-2f396905c466", "Please log out once you have completed this function."), ZMessageBoxButtons.OK);
				}
			}

			if (!Globals.IsTest && env.Registry.EmailDestinationOverride.Length == 0)
			{
#if DEBUG
				if (!env.CurrentUser.IsDeveloperLogin)
				{
					AddMissingConfigurationItem(Res.GetString("14bfc067-e282-45ae-b8b4-6f2f264c7558", "Missing Email Destination Override"),
												Res.GetString("1ea0ce24-57b4-492a-ae7d-724a9a1bb9d0", "The Email Destination Override hasn't been set. It is required on DEBUG builds."),
												Res.GetString("6ec407b6-a4f5-424d-bd2d-ca0b4b1d6192", "No emails will be sent until this setting has been set."),
												Res.GetString("4295147c-1efb-4862-98e4-ad0273a6f72f", "Please set this configuration in Registry > System > Testing > Email Destination Override."), ZMessageBoxButtons.OK);
				}
#else
				if (!env.IsProductionSystem)
				{
					AddMissingConfigurationItem(MissingEmailDestinationItemNameForReleaseMessage, MissingEmailDestinationDescriptionForReleaseMessage, MissingEmailDestinationProblemMsgForReleaseMessage, MissingEmailDestinationHowToRectifyMsgForReleaseMessage, ZMessageBoxButtons.OK);
				}
#endif
			}
		}

		string GetConfigurationMessage(MissingConfigurationItem configItem)
		{
			var stringBuilder = new StringBuilder();

			stringBuilder.Append(configItem.Description + System.Environment.NewLine);
			if (configItem.Problem.Length > 0)
			{
				stringBuilder.Append(configItem.Problem + System.Environment.NewLine);
			}

			stringBuilder.Append(System.Environment.NewLine);
			stringBuilder.Append(configItem.HowToRectify + System.Environment.NewLine);

			return stringBuilder.ToString();
		}

		readonly ArrayList fMissingConfigurationItemList;

		public bool ShouldExecute()
		{
			return true;
		}

		public void Execute()
		{
			ConfigurationItemChecker.CheckConfigurationItems();
		}
	}
}
