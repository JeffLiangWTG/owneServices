using System;
using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class MailboxSettings : IMailboxSettings
	{
		public delegate IRegistryItem CreateMailBoxItemDelegate();

		public MailboxSettings(string namePrefix, MultilingualString mailBoxCategory, Func<string, CreateMailBoxItemDelegate, IRegistryItem> getMailBoxItem, RegistryOptions registryOptions = RegistryOptions.Default)
		{
			this.getMailBoxItem = getMailBoxItem ?? throw new ArgumentNullException(nameof(getMailBoxItem));
			this.mailBoxCategory = mailBoxCategory ?? throw new ArgumentNullException(nameof(mailBoxCategory));
			this.registryOptions = registryOptions | RegistryOptions.PreserveTestValue;

			NamePrefix = namePrefix;
		}

		protected readonly MultilingualString mailBoxCategory;
		protected readonly Func<string, CreateMailBoxItemDelegate, IRegistryItem> getMailBoxItem;
		protected readonly RegistryOptions registryOptions;

		public string NamePrefix { get; }

		#region RegistryItem Properties

		protected IRegistryItem MailRetrievalProtocolRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "MailRetrievalProtocol",
					delegate
					{
						var mailRetrievalProtocolsListProvider = new CodeDescriptionPairListProvider(() => new MailRetrievalProtocols());

						return new CodePairRegistryItem(
							NamePrefix + "MailRetrievalProtocol",
							mailBoxCategory,
							ResString.GetMultilingualString("D2AF11AC-9530-4354-8606-613E0C2ED2FD", "Mail Retrieval Protocol"),
							ResString.GetMultilingualString("08C7ADDC-C7E3-4E26-9805-711003AFBB2F", "Protocol used to retrieve incoming mail"),
							mailRetrievalProtocolsListProvider,
							false, true, new ComboBoxRegistryEditorInfo(mailRetrievalProtocolsListProvider),
							RegistryStorageFlags.System,
							registryOptions,
							MailRetrievalProtocols.POP3,
							false);
					});
			}
		}

		protected IRegistryItem MailServerRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "MailServer",
					delegate
					{
						return new RawDataRegistry.PhysicalServerRegistryItem(
							NamePrefix + "MailServer",
							mailBoxCategory,
							ResString.GetMultilingualString("FE43F8CF-1DC1-480A-9EC1-E57FE4D2E754", "Mail Server"),
							ResString.GetMultilingualString("FE43F8CF-1DC1-480A-9EC1-E57FE4D2E754", "Mail Server"),
							RegistryDataTypes.StringType,
							registryOptions,
							"");
					});
			}
		}

		protected IRegistryItem MailServerPortRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "MailServerPort",
					delegate
					{
						return new RawDataRegistry.PhysicalServerRegistryItem(
							NamePrefix + "MailServerPort",
							mailBoxCategory,
							ResString.GetMultilingualString("F057E3CC-67EE-4181-87D5-F72D2AFE4593", "Mail Server Port"),
							ResString.GetMultilingualString("F057E3CC-67EE-4181-87D5-F72D2AFE4593", "Mail Server Port"),
							RegistryDataTypes.IntType,
							registryOptions,
							110);
					});
			}
		}

		protected IRegistryItem MailboxUserNameRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "MailboxUserName",
					delegate
					{
						return new RawDataRegistry.PhysicalServerRegistryItem(
							NamePrefix + "MailboxUserName",
							mailBoxCategory,
							ResString.GetMultilingualString("9006E689-A417-438E-8518-562F53544578", "Mailbox User Name"),
							ResString.GetMultilingualString("AAE66246-9788-4207-B34D-516A6673FC25", "e.g. user@example.com."),
							RegistryDataTypes.StringType,
							registryOptions,
							"");
					});
			}
		}

		protected IRegistryItem MailboxPasswordRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "MailboxPassword",
					delegate
					{
						return new RawDataRegistry.PhysicalServerRegistryItem(
							NamePrefix + "MailboxPassword",
							mailBoxCategory,
							ResString.GetMultilingualString("155AEC1C-7BB3-424B-8A82-4652DEB676E7", "Mailbox Password"),
							ResString.GetMultilingualString("155AEC1C-7BB3-424B-8A82-4652DEB676E7", "Mailbox Password"),
							RegistryDataTypes.StringType,
							registryOptions,
							"")
						{
							EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password)
						};
					});
			}
		}

		protected IRegistryItem IMAPSecureConnectionTypeRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "IMAPSecureConnection",
					delegate
					{
						var secureConnectionTypesListProvider = new CodeDescriptionPairListProvider(() => new SecureConnectionTypes());

						return new CodePairRegistryItem(
							NamePrefix + "IMAPSecureConnection",
							RegistryItemSet.CombineCategories(mailBoxCategory, ResString.GetMultilingualString("7AC11B4C-90C2-4295-AEF7-D49B795B6E2C", "IMAP")),
							ResString.GetMultilingualString("637B3938-63EA-48CF-AEAC-D9457AD9A813", "IMAP Server Secure Connection"),
							ResString.GetMultilingualString("E3C6AD07-7414-4FD3-9228-F26191104F7C", "The type of secure connection to use to connect to the IMAP mail server"),
							secureConnectionTypesListProvider,
							false,
							true,
							new ComboBoxRegistryEditorInfo(secureConnectionTypesListProvider),
							RegistryStorageFlags.System,
							registryOptions,
							SecureConnectionTypes.TLS,
							false);
					});
			}
		}

		protected IRegistryItem POP3SecureConnectionTypeRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "POP3SecureConnection",
					delegate
					{
						var secureConnectionTypesListProvider = new CodeDescriptionPairListProvider(() => new SecureConnectionTypes());

						return new CodePairRegistryItem(
							NamePrefix + "POP3SecureConnection",
							RegistryItemSet.CombineCategories(mailBoxCategory, ResString.GetMultilingualString("6F18883B-871D-4726-AE76-3BB4FAE7BA49", "POP3")),
							ResString.GetMultilingualString("F67DC1D0-8A8E-4D62-AE37-A64B71B84BDA", "POP3 Server Secure Connection"),
							ResString.GetMultilingualString("B8323CCF-CD50-417E-ABD1-217C44E8F0CF", "The type of secure connection to use to connect to the POP3 mail server"),
							secureConnectionTypesListProvider,
							false,
							true,
							new ComboBoxRegistryEditorInfo(secureConnectionTypesListProvider),
							RegistryStorageFlags.System,
							registryOptions,
							SecureConnectionTypes.None,
							false);
					});
			}
		}

		#endregion

		#region Properties

		protected bool UseFallback => string.IsNullOrWhiteSpace(MailServerRegistry.Value.ToString());

		public string MailRetrievalProtocol
		{
			get { return UseFallback ? Env.Registry.MailRetrievalProtocol : MailRetrievalProtocolRegistry.Value.ToString(); }
#if DEBUG
			set { MailRetrievalProtocolRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string Server
		{
			get { return UseFallback ? Env.Registry.MailServer : MailServerRegistry.Value.ToString(); }
#if DEBUG
			set { MailServerRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int Port
		{
			get { return UseFallback ? Env.Registry.MailServerPort : (int)MailServerPortRegistry.Value; }
		}

		public string UserName
		{
			get { return MailboxUserNameRegistry.Value.ToString(); }
#if DEBUG
			set { MailboxUserNameRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string Password
		{
			get
			{
				return MailboxPasswordRegistry.Value.ToString();
			}
#if DEBUG
			set { MailboxPasswordRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string SecureConnectionType
		{
			get
			{
				if (MailRetrievalProtocol == MailRetrievalProtocols.POP3)
				{
					return UseFallback ? Env.Registry.POP3SecureConnection : POP3SecureConnectionTypeRegistry.Value.ToString();
				}

				return UseFallback ? Env.Registry.IMAPSecureConnection : IMAPSecureConnectionTypeRegistry.Value.ToString();
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		public virtual bool IsValid(out ICollection<string> errorMessages)
		{
			errorMessages = new List<string>();

			if (string.IsNullOrWhiteSpace(Server))
			{
				errorMessages.Add("Mail server is not configured");
			}

			if (string.IsNullOrWhiteSpace(UserName))
			{
				errorMessages.Add("Mail account username is not configured");
			}

			return errorMessages.Count == 0;
		}

		public virtual IEnumerable<IRegistryItem> GetAllItems()
		{
			return new[]
			{
				IMAPSecureConnectionTypeRegistry,
				MailboxPasswordRegistry,
				MailboxUserNameRegistry,
				MailServerRegistry,
				MailServerPortRegistry,
				POP3SecureConnectionTypeRegistry,
				MailRetrievalProtocolRegistry,
			};
		}
	}
}
