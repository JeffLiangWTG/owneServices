using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.GB.Registry
{
	public sealed class GBCustomsDataRegistry : RegistryItemSet, Integration.Customs.GB.IGBCustomsDataRegistry
	{
		#region Construction
		public static GBCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new GBCustomsDataRegistry()); }
		}
		[ThreadStatic]
		static GBCustomsDataRegistry instance;

		GBCustomsDataRegistry()
		{
		}
		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_UnitedKingdom { get { return CombineCategories(Customs_CountryOrRegion, (NoResString)"United Kingdom"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders { get { return CombineCategories(Customs_UnitedKingdom, (NoResString)"Service Providers"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_CNS { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders, (NoResString)"CNS"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_CNS_URLs { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders_CNS, (NoResString)"URLs"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Live { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders_CNS_URLs, (NoResString)"Live"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Test { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders_CNS_URLs, (NoResString)"Test"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_MCP { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders, (NoResString)"MCP"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_CCSUK { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders, (NoResString)"CCS-UK"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_CCSUK_CHIEFFallback { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders_CCSUK, (NoResString)"CHIEF Fallback"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_CCSUK_Network { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders_CCSUK, (NoResString)"Network"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders_CCSUK_Network, (NoResString)"Dashboard"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders_CCSUK, (NoResString)"HCI"); } }
			public static MultilingualString Customs_UnitedKingdom_ICS { get { return CombineCategories(Customs_UnitedKingdom, (NoResString)"ICS"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_NCTS { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders, (NoResString)"NCTS"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_NES { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders, (NoResString)"NES"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_NES_Addresses { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders_NES, (NoResString)"Addresses"); } }
			public static MultilingualString Customs_UnitedKingdom_Notifications { get { return CombineCategories(Customs_UnitedKingdom, (NoResString)"Notifications"); } }
			public static MultilingualString Customs_UnitedKingdom_PrinterseDocs { get { return CombineCategories(Customs_UnitedKingdom, (NoResString)"Printers & eDocs"); } }
			public static MultilingualString Customs_UnitedKingdom_PrinterseDocs_CHIEF { get { return CombineCategories(Customs_UnitedKingdom_PrinterseDocs, (NoResString)"CHIEF"); } }
			public static MultilingualString Customs_UnitedKingdom_CreditChecking { get { return CombineCategories(Customs_UnitedKingdom, (NoResString)"Credit Checking"); } }
			public static MultilingualString Customs_UnitedKingdom_VATAdjustmentbox68 { get { return CombineCategories(Customs_UnitedKingdom, (NoResString)"VAT Adjustment (box 68)"); } }
			public static MultilingualString Customs_UnitedKingdom_ExchangeRates { get { return CombineCategories(Customs_UnitedKingdom, (NoResString)"Exchange Rates"); } }
			public static MultilingualString Customs_UnitedKingdom_H7 { get { return CombineCategories(Customs_UnitedKingdom, (NoResString)"Low Value"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_Pentant { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders, (NoResString)"Pentant"); } }
			public static MultilingualString Customs_UnitedKingdom_PrinterseDocs_CCSUK { get { return CombineCategories(Customs_UnitedKingdom_PrinterseDocs, (NoResString)"CCSUK"); } }
			public static MultilingualString Customs_UnitedKingdom_ServiceProviders_EMCS { get { return CombineCategories(Customs_UnitedKingdom_ServiceProviders, (NoResString)"EMCS"); } }
			public static MultilingualString Customs_UnitedKingdom_GroupToWhichCustomsResponseNotificationsShouldBeSent { get { return CombineCategories(Customs_UnitedKingdom_Notifications, (NoResString)"Group to which customs response notifications should be sent"); } }
			public static MultilingualString Customs_UnitedKingdom_CDS_Notifications { get { return CombineCategories(Customs_UnitedKingdom_GroupToWhichCustomsResponseNotificationsShouldBeSent, (NoResString)"CDS Notifications"); } }
			public static MultilingualString Customs_UnitedKingdom_CDS_Unsolicited_Updates { get { return CombineCategories(Customs_UnitedKingdom_CDS_Notifications, (NoResString)"CDS Unsolicited Updates"); } }
		}

		#endregion

		public BooleanRegistryItem DigitalPromptsTrialParticipateInNudgeTrial
		{
			get
			{
				return GetItem("DigitalPromptsTrialParticipateInNudgeTrial", () =>
				{
					return new BooleanRegistryItem(
						"DigitalPromptsTrialParticipateInNudgeTrial",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Digital prompts trial - participate in nudge trial",
						(NoResString)"Please inform HMRC of the client's desire to be excluded",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		#region Misc CHIEF options

		public IntRegistryItem MinutesToWaitAfterRoute6AcceptanceToGetClearance
		{
			get
			{
				return GetItem("GBMinutesToWaitAfterRoute6AcceptanceToGetClearance", () =>
				{
					return new IntRegistryItem(
						"GBMinutesToWaitAfterRoute6AcceptanceToGetClearance",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Minutes to wait for a clearance after a Route 6 acceptance.",
						(NoResString)"Minutes to wait for a clearance after a Route 6 acceptance.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						10);
				});
			}
		}

		public IntRegistryItem SecondsToWaitAfterCusres29ToGetClearance
		{
			get
			{
				return GetItem("GBSecondsToWaitAfterCusres29ToGetClearance", () =>
				{
					return new IntRegistryItem(
						"GBSecondsToWaitAfterCusres29ToGetClearance",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Seconds to wait for a clearance after a CUSRES 29.",
						(NoResString)"Seconds to wait for a clearance after a CUSRES 29.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						10);
				});
			}
		}

		public BooleanRegistryItem SendDevQueryForNonInventoryImportsUponAcceptance
		{
			get
			{
				return GetItem("GBSendDevQueryForNonInventoryImportsUponAcceptance", () =>
				{
					return new BooleanRegistryItem(
						"GBSendDevQueryForNonInventoryImportsUponAcceptance",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Auto-DEV",
						(NoResString)"Send DEV query messages automatically when a non-inventory import declaration receives an E2 response?",
						RegistryStorageFlags.System,
						false);  // NB, this is just a get-out-of-jail-free card and should only be set to FALSE to disable the feature of it goes wrong.  It shouldn't go wrong, but this is defensive. 
				});
			}
		}

		public BooleanRegistryItem SendDesQueryForNonInventoryImportsUponAcceptance
		{
			get
			{
				return GetItem("GBSendDesQueryForNonInventoryImportsUponAcceptance", () =>
				{
					return new BooleanRegistryItem(
						"GBSendDesQueryForNonInventoryImportsUponAcceptance",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Auto-DES",
						(NoResString)"Send DES query messages automatically when a non-inventory import declaration receives an E2 response?",
						RegistryStorageFlags.System,
						true);  // NB, this is just a get-out-of-jail-free card and should only be set to FALSE to disable the feature of it goes wrong.  It shouldn't go wrong, but this is defensive. 
				});
			}
		}

		public BooleanRegistryItem IncludeY02xSupportingDocumentsForAeoOrganisations
		{
			get
			{
				return GetItem("GBIncludeY02xSupportingDocumentsForAeoOrganisations", () =>
				{
					return new BooleanRegistryItem(
						"GBIncludeY02xSupportingDocumentsForAeoOrganisations",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Include supporting documents for AEOs",
						(NoResString)"Set to true to automatically add a supporting document when a declaration's party has an AEO certificate, e.g. Y024 for the declarant.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}
		public CodePairRegistryItem ChiefC88EntryPrintPreference
		{
			get
			{
				return GetItem("GBChiefC88EntryPrintPreference", () =>
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"GBChiefC88EntryPrintPreference",
						Categories.Customs_UnitedKingdom_PrinterseDocs_CHIEF,
						(NoResString)"C88 auto print",
						(NoResString)"Choose which style of C88 is automatically printed when a positive response message is processed for a declaration request.",
						OLookUpEditType.ChiefC88,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.ChiefC88Options.Code.Rich);
					return result;
				});
			}
		}
		#endregion

		#region Test/live switch

		public BooleanRegistryItem IsInTestMode
		{
			get
			{
				return GetItem("InstanceIsATestInstance", () =>
				{
					return new BooleanRegistryItem(
						"InstanceIsATestInstance",
						Categories.Customs_UnitedKingdom,
						(NoResString)("TEST? This instance of " + Core.Constants.ProductName + " is in test mode."),
						(NoResString)"Indicates to all GB customs applications that they should use test values, not production values.  Examples of values include user-names, IP addresses, email addresses, flags, etc.  Set to YES to indicate that this is TEST server; set to NO for LIVE/PRODUCTION.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						GetDefaultLiveTestValue());
				});
			}
		}

		bool GetDefaultLiveTestValue()
		{
			bool isTest = false;  // production code is live by default, but can be set to test for the test server
#if DEBUG
			isTest = true;  // debug (developer/DAT) code is always a test environment by default. 
#endif
			return isTest;
		}

		#endregion

		#region CNS webservice

		public string CnsPrintsUrl
		{
			get { return !IsInTestMode.Value ? CnsPrintsUrl_Live.Value : CnsPrintsUrl_Test.Value; }
			set
			{
				if (!IsInTestMode.Value)
				{
					CnsPrintsUrl_Live.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					CnsPrintsUrl_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		public string CnsUploadUrl
		{
			get { return IsInTestMode.Value ? CnsUploadUrl_Test.Value : CnsUploadUrl_Live.Value; }
			set
			{
				if (IsInTestMode.Value)
				{
					CnsUploadUrl_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					CnsUploadUrl_Live.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		public string CnsCdsCheckCredentialsUrl
		{
			get { return IsInTestMode.Value ? CnsCdsCheckCredentialsUrl_Test.Value : CnsCdsCheckCredentialsUrl_Live.Value; }
			set
			{
				if (IsInTestMode.Value)
				{
					CnsCdsCheckCredentialsUrl_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					CnsCdsCheckCredentialsUrl_Live.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		internal StringRegistryItem CnsPrintsUrl_Live
		{
			get
			{
				return GetItem("CnsPrintsUrl_Live", delegate
			   {
				   var result = new StringRegistryItem(
					   "CnsPrintsUrl_Live",
					   Categories.Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Live,
					   (NoResString)"URL of CNS print mailbox service (live)",
					   (NoResString)string.Format(CultureInfo.CurrentCulture, "Enter the URL of CNS print mailbox service to which {0} should connect.\r\nYou are recommended to open an browser and navigate to the URL and then copy-and-paste it into here to avoid typos.  Don't forget the trailing slash in the URLs.\r\nDo not override this value without speaking to {1}.", BrandingFactory.Instance.ProductName, BrandingFactory.Instance.CompanyName),
					   RegistryStorageFlags.System,
					   RegistryOptions.PreserveTestValue,
					   "https://www.cnsonline.co.uk/ws/Mailbox/MailBoxPortImpl");
				   result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
				   return result;
			   });
			}
		}

		internal StringRegistryItem CnsUploadUrl_Live
		{
			get
			{
				return GetItem("CnsUploadUrl_Live", () =>
				{
					var result = new StringRegistryItem(
						"CnsUploadUrl_Live",
						Categories.Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Live,
						(NoResString)"URL of CNS upload CCMI service (live)",
						(NoResString)string.Format(CultureInfo.CurrentCulture, "Enter the URL of CNS upload CCMI service to which {0} should connect.\r\nYou are recommended to open an browser and navigate to the URL and then copy-and-paste it into here to avoid typos.  Don't forget the trailing slash in the URLs.\r\nDo not override this value without speaking to {1}.", BrandingFactory.Instance.ProductName, BrandingFactory.Instance.CompanyName),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"https://www.cnsonline.co.uk/ws/CCMI/ChiefEDI");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					return result;
				});
			}
		}

		StringRegistryItem CnsCdsCheckCredentialsUrl_Live
		{
			get
			{
				return GetItem("CnsCdsCheckCredentialsUrl_Live", () =>
				{
					var result = new StringRegistryItem(
						"CnsCdsCheckCredentialsUrl_Live",
						Categories.Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Live,
						(NoResString)"HIDDEN - URL of CNS print mailbox service for CDS (live)",
						(NoResString)"HIDDEN - URL of CNS print mailbox service for CDS (live)",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						"https://www.cnsonline.co.uk/api/");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					return result;
				});
			}
		}

		internal StringRegistryItem CnsPrintsUrl_Test
		{
			get
			{
				return GetItem("CnsPrintsUrl_Test", () =>
				{
					var result = new StringRegistryItem(
						"CnsPrintsUrl_Test",
						Categories.Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Test,
						(NoResString)"URL of CNS print mailbox service (test)",
						(NoResString)string.Format(CultureInfo.CurrentCulture, "Enter the URL of CNS print mailbox service to which {0} should connect.\r\nYou are recommended to open an browser and navigate to the URL and then copy-and-paste it into here to avoid typos.  Don't forget the trailing slash in the URLs.\r\nDo not override this value without speaking to {1}.", BrandingFactory.Instance.ProductName, BrandingFactory.Instance.CompanyName),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"https://www.uat.cnsonline.net/ws/Mailbox/MailBoxPortImpl");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					return result;
				});
			}
		}

		internal StringRegistryItem CnsUploadUrl_Test
		{
			get
			{
				return GetItem("CnsUploadUrl_Test", () =>
				{
					var result = new StringRegistryItem(
						"CnsUploadUrl_Test",
						Categories.Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Test,
						(NoResString)"URL of CNS upload CCMI service (test)",
						(NoResString)string.Format(CultureInfo.CurrentCulture, "Enter the URL of CNS upload CCMI service to which {0} should connect.\r\nYou are recommended to open an browser and navigate to the URL and then copy-and-paste it into here to avoid typos.  Don't forget the trailing slash in the URLs.\r\nDo not override this value without speaking to {1}.", BrandingFactory.Instance.ProductName, BrandingFactory.Instance.CompanyName),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"https://www.uat.cnsonline.net/ws/CCMI/ChiefEDI");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					return result;
				});
			}
		}

		StringRegistryItem CnsCdsCheckCredentialsUrl_Test
		{
			get
			{
				return GetItem("CnsCdsCheckCredentialsUrl_Test", () =>
				{
					var result = new StringRegistryItem(
						"CnsCdsCheckCredentialsUrl_Test",
						Categories.Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Live,
						(NoResString)"HIDDEN - URL of CNS print mailbox service for CDS (test)",
						(NoResString)"HIDDEN - URL of CNS print mailbox service for CDS (test)",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						"https://www.uat.cnsonline.co.uk/api/");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					return result;
				});
			}
		}

		public BooleanRegistryItem CnsUseCompassForPullingPrints
		{
			get
			{
				return GetItem("GBCnsUseCompassForPullingPrints", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCnsUseCompassForPullingPrints",
									Categories.Customs_UnitedKingdom_ServiceProviders_CNS,
									(NoResString)"Use Compass to retrieve DTI prints from CNS",
									(NoResString)"Specifies whether to poll Compass directly to retrieve DTI prints for CNS jobs.",
									RegistryStorageFlags.System,
									RegistryOptions.PreserveTestValue,
									true);
					return result;
				});
			}
		}

		public DateTimeRegistryItem CnsNudgedDateTime
		{
			get
			{
				return GetItem("GBCnsNudgedDateTime", () =>
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
									"GBCnsNudgedDateTime",
									Categories.Customs_UnitedKingdom_ServiceProviders_CNS,
									(NoResString)"HIDDEN - nudge CNS downloader service task at this time",
									(NoResString)"HIDDEN - nudge CNS downloader service task at this time",
									RegistryStorageFlags.System,
									RegistryOptions.IsHidden,
									DateTime.MinValue);
					return result;
				});
			}
		}

		public IntRegistryItem CnsWebServiceLockoutThreshold
		{
			get
			{
				return GetItem("GBCnsWebServiceLockoutThreshold", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
									"GBCnsWebServiceLockoutThreshold",
									Categories.Customs_UnitedKingdom_ServiceProviders_CNS,
									(NoResString)"Anti-lockout threshold",
									(NoResString)"Anti-lockout threshold. The number of failed login attempts before shutting down CNS webservice activity to prevent lockout. Set to -1 to never lock down.",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyEditableBySupportIfHosted,
									30);  // Should be more than 10
					return result;
				});
			}
		}

		public BooleanRegistryItem CnsSaveStatusNotificationsToEDocs
		{
			get
			{
				return GetItem("GBCnsSaveStatusNotificationsToEDocs", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCnsSaveStatusNotificationsToEDocs",
									Categories.Customs_UnitedKingdom_ServiceProviders_CNS,
									(NoResString)"Save CNS status texts to eDocs",
									(NoResString)"Specifies whether to additionally save a copy of CNS status text 'prints' to eDocs. This will reduce performance of the service task and increase database space usage.",
									RegistryStorageFlags.System,
									false);
					return result;
				});
			}
		}

		#endregion

		#region MCP/destin8 stuff

		public IntRegistryItem McpMinimumPollPeriodInSeconds
		{
			get
			{
				return GetItem("GBMcpMinimumPollPeriodInSeconds", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
									"GBMcpMinimumPollPeriodInSeconds",
									Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
									(NoResString)"HIDDEN - minimum wait between polling MCP as suggested by them",
									(NoResString)"HIDDEN - minimum wait between polling MCP as suggested by them",
									RegistryStorageFlags.System,
									RegistryOptions.IsHidden,
									61);  // One minute 
					return result;
				});
			}
		}

		public DateTimeRegistryItem McpNudgedDateTime
		{
			get
			{
				return GetItem("GBMcpNudgedDateTime", () =>
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
									"GBMcpNudgedDateTime",
									Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
									(NoResString)"HIDDEN - nudge MCP downloader service task at this time",
									(NoResString)"HIDDEN - nudge MCP downloader service task at this time",
									RegistryStorageFlags.System,
									RegistryOptions.IsHidden,
									DateTime.MinValue);
					return result;
				});
			}
		}

		public IntRegistryItem McpWebServiceLockoutThreshold
		{
			get
			{
				return GetItem("GBMcpWebServiceLockoutThreshold", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
									"GBMcpWebServiceLockoutThreshold",
									Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
									(NoResString)"Anti-lockout threshold",
									(NoResString)"Anti-lockout threshold. The number of failed login attempts before shutting down MCP webservice activity to prevent lockout. Set to -1 to never lock down.",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyEditableBySupportIfHosted,
									30);  // Should be more than 10
					return result;
				});
			}
		}

		public BooleanRegistryItem McpUseDestin8ForPullingPrints
		{
			get
			{
				return GetItem("GBMcpUseDestin8ForPrints", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBMcpUseDestin8ForPrints",
									Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
									(NoResString)"Use Destin8 to retrieve DTI prints from MCP",
									(NoResString)"Specifies whether to use Destin8 directly to retrieve DTI prints for MCP jobs.",
									RegistryStorageFlags.System | RegistryStorageFlags.Company,
									RegistryOptions.PreserveTestValue,
									true);
					return result;
				});
			}
		}

		public string McpDestin8Url
		{
			get { return IsInTestMode.Value ? McpDestin8Url_Test2023.Value : McpDestin8Url_Live2023.Value; }
			set
			{
				if (IsInTestMode.Value)
				{
					McpDestin8Url_Test2023.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					McpDestin8Url_Live2023.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		public string McpCdsCheckCredentialsUrl
		{
			get { return IsInTestMode.Value ? McpCdsCheckCredentialsUrl_Test.Value : McpCdsCheckCredentialsUrl_Live.Value; }
			set
			{
				if (IsInTestMode.Value)
				{
					McpCdsCheckCredentialsUrl_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					McpCdsCheckCredentialsUrl_Live.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		internal StringRegistryItem McpDestin8Url_Live2023
		{
			get
			{
				return GetItem("GBMcpDestin8Url2023", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
									"GBMcpDestin8Url2023",
									Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
									(NoResString)"URL of Destin8 web service (live)",
									(NoResString)string.Format(CultureInfo.CurrentCulture, "Enter the URL of the Destin8 'ChiefEDI' webservice to which {0} should connect.\r\nYou are recommended to open an browser and navigate to the URL and then copy-and-paste it into here to avoid typos.  Don't forget the trailing slash in the URLs.\r\nDo not override this value without speaking to {1}.", BrandingFactory.Instance.ProductName, BrandingFactory.Instance.CompanyName),
									RegistryStorageFlags.System,
									RegistryOptions.PreserveTestValue,
									"https://edi.destin8.co.uk");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					return result;
				});
			}
		}

		StringRegistryItem McpDestin8Url_Test2023
		{
			get
			{
				return GetItem("GBMcpDestin8UrlTest2023", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
									"GBMcpDestin8UrlTest2023",
									Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
									(NoResString)"URL of Destin8 web service (test)",
									(NoResString)string.Format(CultureInfo.CurrentCulture, "Enter the URL of the Destin8 'ChiefEDI' webservice to which {0} should connect.\r\nYou are recommended to open an browser and navigate to the URL and then copy-and-paste it into here to avoid typos.  Don't forget the trailing slash in the URLs.\r\nDo not override this value without speaking to {1}.", BrandingFactory.Instance.ProductName, BrandingFactory.Instance.CompanyName),
									RegistryStorageFlags.System,
									"https://ediuat.destin8.co.uk");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					return result;
				});
			}
		}

		StringRegistryItem McpCdsCheckCredentialsUrl_Live
		{
			get
			{
				return GetItem("McpCdsCheckCredentialsUrl_Live", () =>
				{
					var result = new StringRegistryItem(
						"McpCdsCheckCredentialsUrl_Live",
						Categories.Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Live,
						(NoResString)"HIDDEN - URL of Destin8 web service for CDS Check Credentials (live)",
						(NoResString)"HIDDEN - URL of Destin8 web service for CDS Check Credentials (live)",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						"https://www.destin8.co.uk/api/");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					return result;
				});
			}
		}

		StringRegistryItem McpCdsCheckCredentialsUrl_Test
		{
			get
			{
				return GetItem("McpCdsCheckCredentialsUrl_Test", () =>
				{
					var result = new StringRegistryItem(
						"McpCdsCheckCredentialsUrl_Test",
						Categories.Customs_UnitedKingdom_ServiceProviders_CNS_URLs_Live,
						(NoResString)"HIDDEN - URL of Destin8 web service for CDS Check Credentials (test)",
						(NoResString)"HIDDEN - URL of Destin8 web service for CDS Check Credentials (test)",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						"https://uat.destin8.co.uk/api/");
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Url);
					return result;
				});
			}
		}

		public string McpIslWebserviceUrl
		{
			get { return IsInTestMode.Value ? McpIslWebserviceUrl_Test.Value : McpIslWebserviceUrl_Live.Value; }
			set
			{
				if (IsInTestMode.Value)
				{
					McpIslWebserviceUrl_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					McpIslWebserviceUrl_Live.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		internal StringRegistryItem McpIslWebserviceUrl_Live
		{
			get
			{
				return GetItem("McpIslWebserviceUrl_Live", () =>
				{
					var result = new StringRegistryItem(
						"McpIslWebserviceUrl_Live",
						Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
						(NoResString)"ISL Webservice URL (Live)",
						(NoResString)"Live MCP ISL Webservice URL",
						new StringRegistryDataType(CharacterCase.Lower),
						new TextRegistryEditorInfo(TextEditorType.Url),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"https://www.destin8.co.uk/ISLInterfaceMCP/ISLInterfaceMCP");
					return result;
				});
			}
		}

		internal StringRegistryItem McpIslWebserviceUrl_Test
		{
			get
			{
				return GetItem("McpIslWebserviceUrl_Test", () =>
				{
					var result = new StringRegistryItem(
						"McpIslWebserviceUrl_Test",
						Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
						(NoResString)"ISL Webservice URL (Test)",
						(NoResString)"Test MCP ISL Webservice URL",
						new StringRegistryDataType(CharacterCase.Lower),
						new TextRegistryEditorInfo(TextEditorType.Url),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"https://uat.destin8.co.uk/ISLInterfaceMCP/ISLInterfaceMCP");
					return result;
				});
			}
		}

		public McpIslCredentialsSettingCollectionRegistryItem McpIslWebServiceCredentialsSet
		{
			get
			{
				return GetItem("McpIslWebServiceCredentialsSet", () =>
				{
					return new McpIslCredentialsSettingCollectionRegistryItem(
						"McpIslWebServiceCredentialsSet",
						Categories.Customs_UnitedKingdom_ServiceProviders_MCP,
						"ISL Webservice Credentials",
						@"Enter company code, username, device and password as provided",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public IntRegistryItem MaximumRetriesOfFailedCusdecs
		{
			get
			{
				return GetItem("GBMcpMaximumRetriesOfFailedCusdecs", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"GBMcpMaximumRetriesOfFailedCusdecs",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Retries of failed CUSDECs",
						(NoResString)string.Format(CultureInfo.CurrentCulture, "The number of times that {0} should retry sending a CUSDEC to a CSP's web service before sending a warning email.", Core.Constants.ProductName),
						RegistryStorageFlags.System, 10);
					return result;
				});
			}
		}

		#endregion

		#region Notifications and other reporting

		public ZString CustomsResponseNotifications
		{
			get { return (ZString)CustomsResponseNotificationsItem.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { CustomsResponseNotificationsItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		/// <summary>
		/// Mode of notification - user, user and group, group, none, etc
		/// </summary>
		public CodePairRegistryItem CustomsResponseNotificationsItem
		{
			get
			{
				return GetItem("CustomsResponseNotifications", () =>
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"CustomsResponseNotifications",
						Categories.Customs_UnitedKingdom_Notifications,
						(NoResString)"Customs response notifications mode",
						(NoResString)"Choose an option for how responses to customs messages should be sent. The 'nominated group' is set in the 'Group to which customs response notifications should be sent' registry item. ",
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company,
						Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
					return result;
				});
			}
		}

		public Guid CustomsResponseNotificationsToGroup
		{
			get { return CustomsResponseNotificationsToGroupItem.Value; }
#if DEBUG
			set { CustomsResponseNotificationsToGroupItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, value); }
#endif
		}

		/// <summary>
		/// PK of default group, e.g. postmasters groups
		/// </summary>
		public GuidRegistryItem CustomsResponseNotificationsToGroupItem
		{
			get
			{
				return GetItem("CustomsResponseNotificationsToGroup", () =>
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CustomsResponseNotificationsToGroup",
						Categories.Customs_UnitedKingdom_Notifications,
						(NoResString)"Group to which customs response notifications should be sent",
						(NoResString)"The staff group that will be receiving notifications about Customs Responses. To make use of this, the 'Customs response notifications mode' registry item should be set to a value that includes a group.",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						Core.Constants.Groups.PostMastersGroupPK);
					result.DataType = new NotificationGroupGuidRegistryDataType();
					return result;
				});
			}
		}

		public IRegistryItem GetNotificationItem(ZString subItem, ZString humanName, IRegistryItem parentNotificationsItem)
		{
			var identifyingKey = parentNotificationsItem.Name + subItem;
			var parentNotificationsItemMultilingual = (IMultilingualRegistryItem)parentNotificationsItem;
			return GetItem(identifyingKey, () =>
			{
				return new RegistryItemImplWithDynamicDefaultValue
					(
						identifyingKey,
						new[] { CombineCategories(parentNotificationsItemMultilingual.CategoryMultilingual, parentNotificationsItemMultilingual.CaptionMultilingual) },
						(NoResString)(humanName),
						(NoResString)("Group for " + humanName),
						RegistryDataTypes.GuidType,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(companyPK, branchPK, departmentPK) => parentNotificationsItem.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK)
					);
			});
		}

		public Guid GetRegistryItemGuid(IRegistryItem registryItem, ZString code, Guid company, Guid branch, Guid dept)
		{
			var value = GetNotificationItem(code, "", registryItem).GetFallBackValueAtAllLevels(company, branch, dept);
			return Guid.Parse(value.ToString());
		}

		public IRegistryItem NotificationChief
		{
			get { return GetNotificationItem("CHIEF", "CHIEF Notifications", CustomsResponseNotificationsToGroupItem); }
		}

		public IRegistryItem NotificationChiefErrors
		{
			get { return GetNotificationItem("Errors", "CHIEF Errors", NotificationChief); }
		}

		public IRegistryItem NotificationChiefNegativeResponses
		{
			get { return GetNotificationItem("NegativeResponses", "CHIEF Negative Responses", NotificationChief); }
		}
		public IRegistryItem NotificationChiefNegativeResponsesContrl
		{
			get { return GetNotificationItem("CONTRL", "CHIEF CONTRL Error Responses", NotificationChiefNegativeResponses); }
		}
		public IRegistryItem NotificationChiefNegativeResponsesCusres27
		{
			get { return GetNotificationItem("CUSRES27", "CHIEF CUSRES/27 Error Responses", NotificationChiefNegativeResponses); }
		}
		public IRegistryItem NotificationChiefNegativeResponsesUkctrlNak
		{
			get { return GetNotificationItem("UKCTRLNAK", "CHIEF UKCTRL NAK Error Responses", NotificationChiefNegativeResponses); }
		}
		public IRegistryItem NotificationChiefNegativeResponsesAcd
		{
			get { return GetNotificationItem("ACD", "CHIEF Negative Responses for ACD Requests", NotificationChiefNegativeResponses); }
		}
		public IRegistryItem NotificationChiefNegativeResponsesCp3
		{
			get { return GetNotificationItem("CP3", "CHIEF Negative Responses for CP3 Requests", NotificationChiefNegativeResponses); }
		}
		public IRegistryItem NotificationChiefNegativeResponsesReq
		{
			get { return GetNotificationItem("REQ", "CHIEF Negative Responses for REQ Requests", NotificationChiefNegativeResponses); }
		}
		public IRegistryItem NotificationChiefNegativeResponsesXtc
		{
			get { return GetNotificationItem("XTC", "CHIEF Negative Responses for XTC Requests", NotificationChiefNegativeResponses); }
		}

		public IRegistryItem NotificationChiefPositiveResponses
		{
			get { return GetNotificationItem("PositiveResponses", "CHIEF Positive Responses", NotificationChief); }
		}

		public IRegistryItem NotificationChiefPositiveResponsesCusres29
		{
			get { return GetNotificationItem("CUSRES29", "CHIEF CUSRES/29 Positive Responses", NotificationChiefPositiveResponses); }
		}

		public IRegistryItem NotificationChiefPositiveResponsesUkctrlAck
		{
			get { return GetNotificationItem("UKCTRLNAK", "CHIEF UKCTRL ACK Positive Responses", NotificationChiefPositiveResponses); }
		}

		#region CHIEF Entry Related Requests

		public IRegistryItem NotificationChiefPositiveResponsesAcd
		{
			get { return GetNotificationItem("ACD", "CHIEF Positive Responses for ACD Requests", NotificationChiefPositiveResponses); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesCp3
		{
			get { return GetNotificationItem("CP3", "CHIEF Positive Responses for CP3 Requests", NotificationChiefPositiveResponses); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesReq
		{
			get { return GetNotificationItem("REQ", "CHIEF Positive Responses for REQ Requests", NotificationChiefPositiveResponses); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesXtc
		{
			get { return GetNotificationItem("XTC", "CHIEF Positive Responses for XTC Requests", NotificationChiefPositiveResponses); }
		}

		#endregion

		#region CHIEF Enquiries And Responses

		public IRegistryItem NotificationChiefPositiveResponsesDEM
		{
			get { return GetNotificationItem("DEM", "DEM - Display movement handling agent view of Entry", NotificationChiefPositiveResponses); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesDEV
		{
			get { return GetNotificationItem("DEV", "DEV - Display a version of an Entry", NotificationChiefPositiveResponses); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesDEC
		{
			get { return GetNotificationItem("DEC", "DEC - Display an Export consignment (Master or Declaration UCR)", NotificationChiefPositiveResponses); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesLEM
		{
			get { return GetNotificationItem("LEM", "LEM - List Export Movements", NotificationChiefPositiveResponses); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesDLU
		{
			get { return GetNotificationItem("DLU", "DLU - Display Licence Usage", NotificationChiefPositiveResponses); }
		}

		#endregion

		public IRegistryItem NotificationChiefStatusUpdates
		{
			get { return GetNotificationItem("StatusUpdates", "CHIEF Status Updates", NotificationChief); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesEac
		{
			get { return GetNotificationItem("EAC", "CHIEF UKCINV EAC - Associate Consignment", NotificationChiefStatusUpdates); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesEaa
		{
			get { return GetNotificationItem("EAA", "CHIEF UKCINV EAA - Anticipated Arrival", NotificationChiefStatusUpdates); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesEal
		{
			get { return GetNotificationItem("EAL", "CHIEF UKCINV EAL - Arrival at Location", NotificationChiefStatusUpdates); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesEdl
		{
			get { return GetNotificationItem("EDL", "CHIEF UKCINV EDL - Departure from Location", NotificationChiefStatusUpdates); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesErs
		{
			get { return GetNotificationItem("ERS", "CHIEF UKCINV ERS - Route or Status Change Notification", NotificationChiefStatusUpdates); }
		}
		public IRegistryItem NotificationChiefPositiveResponsesEmr
		{
			get { return GetNotificationItem("EMR", "CHIEF UKCINV EMR - Asynchronous Master Arrival Response for MASTER-OPT “F", NotificationChiefStatusUpdates); }
		}

		#region Chief Print Notifications

		public IRegistryItem NotificationChiefPrints
		{
			get { return GetNotificationItem("Prints", "CHIEF Prints", NotificationChief); }
		}

		public IRegistryItem NotificationChiefPrintsP2
		{
			get { return GetNotificationItem("P2", "P2", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsP2AMD
		{
			get { return GetNotificationItem("P2-AMD", "P2-AMD", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE2
		{
			get { return GetNotificationItem("E2", "E2", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE2R
		{
			get { return GetNotificationItem("E2-R", "E2-R", NotificationChiefPrintsE2); }
		}

		public IRegistryItem NotificationChiefPrintsH2
		{
			get { return GetNotificationItem("H2", "H2", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsX2
		{
			get { return GetNotificationItem("X2", "X2", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE2AMD
		{
			get { return GetNotificationItem("E2-AMD", "E2-AMD", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE2AMDR
		{
			get { return GetNotificationItem("E2-AMD-R", "E2-AMD-R", NotificationChiefPrintsE2AMD); }
		}

		public IRegistryItem NotificationChiefPrintsE7
		{
			get { return GetNotificationItem("E7", "E7", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE9
		{
			get { return GetNotificationItem("E9", "E9", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsH7
		{
			get { return GetNotificationItem("H7", "H7", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsP7
		{
			get { return GetNotificationItem("P7", "P7", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsX7
		{
			get { return GetNotificationItem("X7", "X7", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE2XH
		{
			get { return GetNotificationItem("E2-XH", "E2-XH", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE2XHR
		{
			get { return GetNotificationItem("E2-XH-R", "E2-XH-R", NotificationChiefPrintsE2XH); }
		}

		public IRegistryItem NotificationChiefPrintsE2XHAMD
		{
			get { return GetNotificationItem("E2-XH-AMD", "E2-XH-AMD", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE2XHAMDR
		{
			get { return GetNotificationItem("E2-XH-AMD-R", "E2-XH-AMD-R", NotificationChiefPrintsE2XHAMD); }
		}

		public IRegistryItem NotificationChiefPrintsX2XH
		{
			get { return GetNotificationItem("X2-XH", "X2-XH", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE8
		{
			get { return GetNotificationItem("E8", "E8", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsX8
		{
			get { return GetNotificationItem("X8", "X8", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsX9
		{
			get { return GetNotificationItem("X9", "X9", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsP3
		{
			get { return GetNotificationItem("P3", "P3", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsX0
		{
			get { return GetNotificationItem("X0", "X0", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsE5
		{
			get { return GetNotificationItem("E5", "E5", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsX5
		{
			get { return GetNotificationItem("X5", "X5", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsX6
		{
			get { return GetNotificationItem("X6", "X6", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsX4
		{
			get { return GetNotificationItem("X4", "X4", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsX1
		{
			get { return GetNotificationItem("X1", "X1", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsN1
		{
			get { return GetNotificationItem("N1", "N1", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsN3
		{
			get { return GetNotificationItem("N3", "N3", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsN4
		{
			get { return GetNotificationItem("N4", "N4", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsS0
		{
			get { return GetNotificationItem("S0", "S0", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsS1
		{
			get { return GetNotificationItem("S1", "S1", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsS3
		{
			get { return GetNotificationItem("S3", "S3", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsS4
		{
			get { return GetNotificationItem("S4", "S4", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsS5
		{
			get { return GetNotificationItem("S5", "S5", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsS6
		{
			get { return GetNotificationItem("S6", "s6", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsS8
		{
			get { return GetNotificationItem("S8", "S8", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsS9
		{
			get { return GetNotificationItem("S9", "S9", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsH3
		{
			get { return GetNotificationItem("H3", "H3", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsH5
		{
			get { return GetNotificationItem("H5", "H5", NotificationChiefPrints); }
		}
		public IRegistryItem NotificationChiefPrintsP9
		{
			get { return GetNotificationItem("P9", "P9", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsP5
		{
			get { return GetNotificationItem("P5", "P5", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE0
		{
			get { return GetNotificationItem("E0", "E0", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsE1
		{
			get { return GetNotificationItem("E1", "E1", NotificationChiefPrints); }
		}

		public IRegistryItem NotificationChiefPrintsN6
		{
			get { return GetNotificationItem("N6", "N6", NotificationChiefPrints); }
		}

		#endregion

		#region CNS

		public IRegistryItem NotificationCns
		{
			get { return GetNotificationItem("CNS", "CNS Notifications", CustomsResponseNotificationsToGroupItem); }
		}

		public IRegistryItem NotificationCnsErrors
		{
			get { return GetNotificationItem("Errors", "CNS Error Notifications", NotificationCns); }
		}

		public IRegistryItem NotificationCnsXml
		{
			get { return GetNotificationItem("Xml", "CNS XML Updates", NotificationCns); }
		}

		public IRegistryItem NotificationCnsTextUpdates
		{
			get { return GetNotificationItem("Text", "CNS Text Updates", NotificationCns); }
		}

		#endregion

		#region MCP Notifications

		public IRegistryItem NotificationMcp
		{
			get { return GetNotificationItem("MCP", "MCP Notifications", CustomsResponseNotificationsToGroupItem); }
		}

		public IRegistryItem NotificationMcpErrors
		{
			get { return GetNotificationItem("Errors", "MCP Error Notifications", NotificationMcp); }
		}

		public IRegistryItem NotificationMcpPhs11
		{
			get { return GetNotificationItem("PHS11", "MCP PHS11 Notification", NotificationMcp); }
		}

		public IRegistryItem NotificationMcpRra
		{
			get { return GetNotificationItem("RRA", "MCP RRA Notifications", NotificationMcp); }
		}

		public IRegistryItem NotificationMcpRra01
		{
			get { return GetNotificationItem("01", "MCP RRA01 Notifications", NotificationMcpRra); }
		}

		public IRegistryItem NotificationMcpRra11
		{
			get { return GetNotificationItem("11", "MCP RRA11 Notifications", NotificationMcpRra); }
		}

		public IRegistryItem NotificationMcpRra12
		{
			get { return GetNotificationItem("12", "MCP RRA12 Notifications", NotificationMcpRra); }
		}

		public IRegistryItem NotificationMcpCsn
		{
			get { return GetNotificationItem("CSN", "MCP CSN Notifications", NotificationMcp); }
		}

		public IRegistryItem NotificationMcpLum
		{
			get { return GetNotificationItem("LUM", "MCP LUM Notifications", NotificationMcp); }
		}

		#endregion

		#region CCSUK Notifications

		public IRegistryItem NotificationCcsuk
		{
			get { return GetNotificationItem("CCSUK", "CCSUK Notifications", CustomsResponseNotificationsToGroupItem); }
		}

		public IRegistryItem NotificationCcsukErrors
		{
			get { return GetNotificationItem("Errors", "CCSUK Error Notifications", NotificationCcsuk); }
		}

		public StringRegistryItem HostedCcsukAlertsEmailAddress
		{
			get
			{
				return GetItem("HostedCcsukAlertsEmailAddress", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
									"HostedCcsukAlertsEmailAddress",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Email address to send notification of CCS-UK login failures",
									(NoResString)"Email address to send notification of CCS-UK login failures.",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForSupport,
									"DGCustomsNotificationsGB@wisetechglobal.com");
					return result;
				});
			}
		}

		public IRegistryItem NotificationCcsukCuscar
		{
			get { return GetNotificationItem("Cuscar", "CCSUK CUSCAR Notifications", NotificationCcsuk); }
		}

		public IRegistryItem NotificationCcsukCuscarFri
		{
			get { return GetNotificationItem("FRI", "CCSUK CUSCAR FRI Insert Notifications", NotificationCcsukCuscar); }
		}

		public IRegistryItem NotificationCcsukCuscarFrc
		{
			get { return GetNotificationItem("FRC", "CCSUK CUSCAR FRC Update Notifications", NotificationCcsukCuscar); }
		}

		public IRegistryItem NotificationCcsukCuscarFrx
		{
			get { return GetNotificationItem("FRX", "CCSUK CUSCAR FRX Delete Notifications", NotificationCcsukCuscar); }
		}

		public IRegistryItem NotificationCcsukCuscarFcs
		{
			get { return GetNotificationItem("FCS", "CCSUK CUSCAR FCS Split Notifications", NotificationCcsukCuscar); }
		}

		public IRegistryItem NotificationCcsukCargoFact
		{
			get { return GetNotificationItem("CargoFact", "CCSUK CargoFACT Notifications", NotificationCcsuk); }
		}

		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdates
		{
			get { return GetNotificationItem("FsnStatusUpdates", "CCSUK CargoFACT FSN Status Updates", NotificationCcsukCargoFact); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesCa
		{
			get { return GetNotificationItem("CA", "CCSUK CargoFACT FSN CA Status Updates - Entry or Request Accepted", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesCb
		{
			get { return GetNotificationItem("CB", "CCSUK CargoFACT FSN CB Status Updates - Released for Inter-Shed removal", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesCc
		{
			get { return GetNotificationItem("CC", "CCSUK CargoFACT FSN CC - Status Cleared Updates - Cleared by Customs", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesCq
		{
			get { return GetNotificationItem("CQ", "CCSUK CargoFACT FSN CQ Status Updates - HMC notification of activity/selection for examination", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesCr
		{
			get { return GetNotificationItem("CR", "CCSUK CargoFACT FSN CR Status Updates - Customs Queried/Detained", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesCs
		{
			get { return GetNotificationItem("CS", "CCSUK CargoFACT FSN CS Status Updates - Seized, Destroyed or Retained by Customs", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesCt
		{
			get { return GetNotificationItem("CT", "CCSUK CargoFACT FSN CT Status Updates - Released for Transhipment remova", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesCu
		{
			get { return GetNotificationItem("CU", "CCSUK CargoFACT FSN CU Status Updates - Through Air Waybill released", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesCw
		{
			get { return GetNotificationItem("CW", "CCSUK CargoFACT FSN CW Status Updates - Released for Inter-Airport removal", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesCx
		{
			get { return GetNotificationItem("CX", "CCSUK CargoFACT FSN CX Status Updates - Entry or Request Cancelled", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesXO
		{
			get { return GetNotificationItem("XO", "CCSUK CargoFACT FSN XO Status Updates - MASTER OPEN", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesXA
		{
			get { return GetNotificationItem("XA", "CCSUK CargoFACT FSN XA Status Updates - ROUTE ‘n’, CUSTOMS SEIZED or FALLBACK HOLD", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesXC
		{
			get { return GetNotificationItem("XC", "CCSUK CargoFACT FSN XC Status Updates - OK TO PROCEED or FALLBACK RELEASE", NotificationCcsukCargoFactFsnStatusUpdates); }
		}
		public IRegistryItem NotificationCcsukCargoFactFsnStatusUpdatesXS
		{
			get { return GetNotificationItem("XS", "CCSUK CargoFACT FSN XS Status Updates - CUSTOMS SEIZED", NotificationCcsukCargoFactFsnStatusUpdates); }
		}

		public IRegistryItem NotificationCcsukCargoFactCimFrd
		{
			get { return GetNotificationItem("CIMFRD", "CCSUK CargoFACT CIMFRD Split Request to Shed Notifications", NotificationCcsukCargoFact); }
		}

		public IRegistryItem NotificationCcsukCargoFactCimFrn
		{
			get { return GetNotificationItem("CIMFRN", "CCSUK CargoFACT CIMFRN Renomination Request to Shed Notifications", NotificationCcsukCargoFact); }
		}

		public IRegistryItem NotificationCcsukCargoFactCimFma
		{
			get { return GetNotificationItem("CIMFMA", "CCSUK CargoFACT CIMFMA Notifications", NotificationCcsukCargoFact); }
		}

		public IRegistryItem NotificationCcsukCargoFactCimFna
		{
			get { return GetNotificationItem("CIMFNA", "CCSUK CargoFACT CIMFNA Notifications", NotificationCcsukCargoFact); }
		}

		public IRegistryItem NotificationCcsukCusresResponsesToCusdec
		{
			get { return GetNotificationItem("CusresResponsesToCusdec", "CCSUK CUSRES Responses to CUSDEC", NotificationCcsuk); }
		}

		public IRegistryItem NotificationCcsukCusresResponsesToCusdecFbk
		{
			get { return GetNotificationItem("FBK", "CCSUK CUSRES FBK Notifications", NotificationCcsukCusresResponsesToCusdec); }
		}
		public IRegistryItem NotificationCcsukCusresResponsesToCusdecIar
		{
			get { return GetNotificationItem("IAR", "CCSUK CUSRES IAR Notifications", NotificationCcsukCusresResponsesToCusdec); }
		}
		public IRegistryItem NotificationCcsukCusresResponsesToCusdecTsr
		{
			get { return GetNotificationItem("TSR", "CCSUK CUSRES TSR Notifications", NotificationCcsukCusresResponsesToCusdec); }
		}
		public IRegistryItem NotificationCcsukCusresResponsesToCusdecISR
		{
			get { return GetNotificationItem("ISR", "CCSUK CUSRES ISR Notifications", NotificationCcsukCusresResponsesToCusdec); }
		}

		public IRegistryItem NotificationCcsukGenral
		{
			get { return GetNotificationItem("Genral", "CCSUK GENRAL Notifications", NotificationCcsuk); }
		}

		public IRegistryItem NotificationCcsukGenralText
		{
			get { return GetNotificationItem("Text", "CCSUK GENRAL Text Notifications", NotificationCcsukGenral); }
		}

		public IRegistryItem NotificationCcsukGenralBroadcast
		{
			get { return GetNotificationItem("Broadcast", "CCSUK GENRAL Broadcast Notifications", NotificationCcsukGenral); }
		}

		public IRegistryItem NotificationCcsukGenralFallback
		{
			get { return GetNotificationItem("Fallback", "CCSUK GENRAL Fallback Notifications", NotificationCcsukGenral); }
		}

		public IRegistryItem NotificationCcsukArchive
		{
			get { return GetNotificationItem("Archive", "CCSUK Archive Notifications", NotificationCcsuk); }
		}

		public IRegistryItem NotificationCcsukCukFsaReport
		{
			get { return GetNotificationItem("CUKFSAReport", "Unsolicited CUKFSA Report Notifications", NotificationCcsuk); }
		}
		public IRegistryItem NotificationCcsukCukFsaReportJA
		{
			get { return GetNotificationItem("CUKFSAReportJA", "Unsolicited CUKFSA JA Inventory Failure Report Notifications", NotificationCcsukCukFsaReport); }
		}
		public IRegistryItem NotificationCcsukCukFsaReportE0
		{
			get { return GetNotificationItem("CUKFSAReportE0", "Unsolicited CUKFSA E0 Inventory Failure Report Notifications", NotificationCcsukCukFsaReport); }
		}
		public IRegistryItem NotificationCcsukCukFsaReportG5
		{
			get { return GetNotificationItem("CUKFSAReportG5", "Unsolicited CUKFSA G5 Pre-arrival Agent Mismatch Report Notifications", NotificationCcsukCukFsaReport); }
		}
		public IRegistryItem NotificationCcsukCukFsaReportH3
		{
			get { return GetNotificationItem("CUKFSAReportH3", "Unsolicited CUKFSA H3 Goods Arrival Reprocessing Error Report Notifications", NotificationCcsukCukFsaReport); }
		}
		public IRegistryItem NotificationCcsukCukFsaReportP5
		{
			get { return GetNotificationItem("CUKFSAReportP5", "Unsolicited CUKFSA P5 Advice of Inter-Shed Removal Report Notifications", NotificationCcsukCukFsaReport); }
		}
		public IRegistryItem NotificationCcsukCukFsaReportU
		{
			get { return GetNotificationItem("CUKFSAReportU", "Unsolicited CUKFSA U Report Duplication Report Notifications", NotificationCcsukCukFsaReport); }
		}

		#endregion

		public BooleanRegistryItem ReportResponseMessageAbnormality
		{
			get
			{
				return GetItem("GBReportResponseMessageAbnormality", () =>
				{
					return new BooleanRegistryItem(
						"GBReportResponseMessageAbnormality",
						Categories.Customs_UnitedKingdom_Notifications,
						(NoResString)"Report Response Abnormality to CW",
						(NoResString)"Determines whether an abnormal response message should be reported to CargoWise (e.g. a message for a job with validation errors is accepted by CHIEF, or a message without validation errors is rejected by an upstream party). GeMS messaging only.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem ChiefFallbackImports
		{
			get
			{
				return GetItem("GBChiefFallbackImports", () =>
				{
					var item = new BooleanRegistryItem(
						"GBChiefFallbackImports",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_CHIEFFallback,
						(NoResString)"Imports fallback",
						(NoResString)"CCSUK has indicated that CHIEF is in imports fallback",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForController | RegistryOptions.NotCached | RegistryOptions.PreserveTestValue,
						false);
					item.IsExcludedFromCwOnlyNonCachedTest = true;
					return item;
				});
			}
		}

		public BooleanRegistryItem ChiefFallbackExports
		{
			get
			{
				return GetItem("GBChiefFallbackExports", () =>
				{
					var item = new BooleanRegistryItem(
						"GBChiefFallbackExports",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_CHIEFFallback,
						(NoResString)"Exports fallback",
						(NoResString)"CCSUK has indicated that CHIEF is in exports fallback",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForController | RegistryOptions.NotCached | RegistryOptions.PreserveTestValue,
						false);
					item.IsExcludedFromCwOnlyNonCachedTest = true;
					return item;
				});
			}
		}

		public BooleanRegistryItem ChiefExportConsolIntegrationShowForAirOnly
		{
			get
			{
				return GetItem("GBChiefExportConsolIntegrationShowForAirOnly", () =>
				{
					return new BooleanRegistryItem(
						"GBChiefExportConsolIntegrationShowForAirOnly",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"Show consol CHIEF plugin for air mode only",
						(NoResString)"Show the CHIEF plugin on consols only for air mode consols. Set to NO to make visible to all modes.",
						RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public CodePairRegistryItem ChiefBox1bEntrySubstyleDefault
		{
			get
			{
				return GetItem("GBChiefBox1bEntrySubstyleDefault", () =>
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"GBChiefBox1bEntrySubstyleDefault",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Entry substyle (box 1b) default",
						(NoResString)"The value below will be used to set the default value for box 1b.  It varies by department.",
						new CodeDescriptionPairListProvider(() => Substyles),
						RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment);
					return result;
				});
			}
		}

		CodeDescriptionPairList substyles;
		CodeDescriptionPairList Substyles
		{
			get
			{
				if (substyles == null)
				{
					substyles = new CodeDescriptionPairList();
					substyles.AddRange(ObjectFactory.Get<IEntrySubStyleListImport>().EntrySubStyleListImport());
					substyles.AddRange(ObjectFactory.Get<IEntrySubStyleListExport>().EntrySubStyleListExport());
				}
				return substyles;
			}
		}

		#endregion

		#region Printers

		public GuidRegistryItem PrinterShared
		{
			get
			{
				return GetItem("GBSharedGbPrinter", () =>
				{
					return new GuidRegistryItem
						(
							"GBSharedGbPrinter",
							Categories.Customs_UnitedKingdom_PrinterseDocs,
							(NoResString)"Generic Printer",
							(NoResString)"Main printer on which GB documents should be produced. Falls back to branch level. Can be over-ridden by other settings (e.g. ADS or CCSUK printers)",
							new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.StmPrintQueue),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							RegistryOptions.Default,
							Guid.Empty
						);
				});
			}
		}

		public IRegistryItem PrinterMcpNonChief
		{
			get
			{
				return GetItem("GBPrinterMcpNonChief", () =>
				{
					return new RegistryItemImplWithDynamicDefaultValue
						(
							"GBPrinterMcpNonChief",
							new[] { Categories.Customs_UnitedKingdom_PrinterseDocs },
							(NoResString)"Printer for MCP",
							(NoResString)"Printer on which MCP prints, excluding CHIEF prints, are printed, e.g. PHS11 messages. Falls back to branch level, the branch is decided using the home branch of the user whose name is on the job or message.",
							RegistryDataTypes.GuidType,
							new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.StmPrintQueue),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							RegistryOptions.Default,
							(companyPK, branchPK, departmentPK) => PrinterShared.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty)
						);
				});
			}
		}

		public IRegistryItem PrinterChief
		{
			get
			{
				return GetItem("GBChiefPrinter", () =>
				{
					return new RegistryItemImplWithDynamicDefaultValue
						(
							"GBChiefPrinter",
							new[] { Categories.Customs_UnitedKingdom_PrinterseDocs_CHIEF },
							(NoResString)"Default printer for CHIEF",
							(NoResString)"Printer on which CHIEF entry prints (E2 etc) should be printed. Falls back to branch level, the branch is decided using the home branch of the user whose name is on the job or message.",
							RegistryDataTypes.GuidType,
							new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.StmPrintQueue),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							RegistryOptions.Default,
							(companyPK, branchPK, departmentPK) => PrinterShared.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty)
						);
				});
			}
		}

		public IRegistryItem PrinterChief_E0
		{
			get { return GetPrinterForChief("E0", "Entry Exception Advice"); }
		}

		public IRegistryItem PrinterChief_E1
		{
			get { return GetPrinterForChief("E1", "Customs Examination Advice"); }
		}

		public IRegistryItem PrinterChief_E2
		{
			get { return GetPrinterForChief("E2", "Import Entry Acceptance Advice"); }
		}

		public IRegistryItem PrinterChief_E5
		{
			get { return GetPrinterForChief("E5", "Reselection Advice"); }
		}

		public IRegistryItem PrinterChief_E7
		{
			get { return GetPrinterForChief("E7", "Entry Amendment Advice"); }
		}

		public IRegistryItem PrinterChief_E8
		{
			get { return GetPrinterForChief("E8", "Supervising Office Report"); }
		}

		public IRegistryItem PrinterChief_E9
		{
			get { return GetPrinterForChief("E9", "Accounting Rejection Advice"); }
		}

		public IRegistryItem PrinterChief_H2
		{
			get { return GetPrinterForChief("H2", "Import Pre-lodgement Advice"); }
		}

		public IRegistryItem PrinterChief_H3
		{
			get { return GetPrinterForChief("H3", "Vessel Arrive Advice of Error"); }
		}

		public IRegistryItem PrinterChief_H5
		{
			get { return GetPrinterForChief("H5", "System Deleted Pre-lodged Entry Advice"); }
		}

		public IRegistryItem PrinterChief_H7
		{
			get { return GetPrinterForChief("H7", "Pre-lodgement Amendment Advice (import)"); }
		}

		public IRegistryItem PrinterChief_N1
		{
			get { return GetPrinterForChief("N1", "System Deleted Stored Entry Advice"); }
		}

		public IRegistryItem PrinterChief_N3
		{
			get { return GetPrinterForChief("N3", "Entry Cancellation Refusal Advice"); }
		}

		public IRegistryItem PrinterChief_N4
		{
			get { return GetPrinterForChief("N4", "Entry Cancellation Approval Advice"); }
		}

		public IRegistryItem PrinterChief_N6
		{
			get { return GetPrinterForChief("N6", "Queried Entry Advice"); }
		}

		public IRegistryItem PrinterChief_P2
		{
			get { return GetPrinterForChief("P2", "Export Pre-Lodgement Advice"); }
		}

		public IRegistryItem PrinterChief_P3
		{
			get { return GetPrinterForChief("P3", "Export Arrival Reprocessing Error Report"); }
		}

		public IRegistryItem PrinterChief_P5
		{
			get { return GetPrinterForChief("P5", "System Deleted Export Pre-lodgement Advice"); }
		}

		public IRegistryItem PrinterChief_P7
		{
			get { return GetPrinterForChief("P7", "Export Pre-lodgement Amendment Advice"); }
		}

		public IRegistryItem PrinterChief_P9
		{
			get { return GetPrinterForChief("P9", "Export Pre-lodgement Deletion Warning"); }
		}

		public IRegistryItem PrinterChief_S0
		{
			get { return GetPrinterForChief("S0", "Exit Follow-up Advice"); }
		}

		public IRegistryItem PrinterChief_S1
		{
			get { return GetPrinterForChief("S1", "System Deleted Stored Export Entry Advice"); }
		}

		public IRegistryItem PrinterChief_S3
		{
			get { return GetPrinterForChief("S3", "Export Cancellation Refusal Advice"); }
		}

		public IRegistryItem PrinterChief_S4
		{
			get { return GetPrinterForChief("S4", "Export Cancellation Approval Advice"); }
		}

		public IRegistryItem PrinterChief_S5
		{
			get { return GetPrinterForChief("S5", "Export Movement Arrival Advice"); }
		}
		public IRegistryItem PrinterChief_S6
		{
			get { return GetPrinterForChief("S6", "Queried Export Entry Advice"); }
		}

		public IRegistryItem PrinterChief_S8
		{
			get { return GetPrinterForChief("S8", "Export Movement Departure Advice"); }
		}

		public IRegistryItem PrinterChief_S9
		{
			get { return GetPrinterForChief("S9", "Export Goods Disposal Advice"); }
		}

		public IRegistryItem PrinterChief_X0
		{
			get { return GetPrinterForChief("X0", "Export Entry Exception Advice"); }
		}

		public IRegistryItem PrinterChief_X1
		{
			get { return GetPrinterForChief("X1", "Export Examination advice"); }
		}

		public IRegistryItem PrinterChief_X2
		{
			get { return GetPrinterForChief("X2", "Export Entry Acceptance Advice"); }
		}

		public IRegistryItem PrinterChief_X5
		{
			get { return GetPrinterForChief("X5", "Export Entry Reselection Advice"); }
		}

		public IRegistryItem PrinterChief_X6
		{
			get { return GetPrinterForChief("X6", "Export Entry Progress Advice"); }
		}

		public IRegistryItem PrinterChief_X7
		{
			get { return GetPrinterForChief("X7", "Export Entry Amendment Advice"); }
		}

		public IRegistryItem PrinterChief_X8
		{
			get { return GetPrinterForChief("X8", "Supervising Office Report"); }
		}

		public IRegistryItem PrinterChief_X9
		{
			get { return GetPrinterForChief("X9", "Export Accounting Rejection Advice"); }
		}

		public IRegistryItem GetPrinterForChief(ZString printCodeWithOrWithourPrefixAndSuffix, string printName = "unknown")
		{
			var printCode = printCodeWithOrWithourPrefixAndSuffix.Replace("DTI-", "").Left(2);  // e.g. P2, DTI-P2 or DTI-P2-XH-AMD becomes P2. 
			return GetItem("GBChiefPrinter" + printCode, () =>
			{
				return new RegistryItemImplWithDynamicDefaultValue
					(
						"GBChiefPrinter" + printCode,
						new[] { Categories.Customs_UnitedKingdom_PrinterseDocs_CHIEF },
						(NoResString)(string.Format(CultureInfo.CurrentCulture, "{0} {1}", printCode, printName)),
						(NoResString)(string.Format(CultureInfo.CurrentCulture, "Printer on which to print {0} ({1}) entry prints from CHIEF. Refer to the Tariff Volume 3 for an explanation of the codes.", printCode, printName)),
						RegistryDataTypes.GuidType,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.StmPrintQueue),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(companyPK, branchPK, departmentPK) => PrinterChief.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty)
					);
			});
		}

		public IRegistryItem PrinterCcsuk_C1
		{
			get { return GetPrinterForCcsuk("C1", "Agents Travelling Copy"); }
		}

		public IRegistryItem PrinterCcsuk_RRA
		{
			get { return GetPrinterForCcsuk("RRA", "Release/Removal Authority"); }
		}

		public IRegistryItem PrinterCcsuk_P5
		{
			get { return GetPrinterForCcsuk("P5", "Inter Airport Removals"); }
		}

		public IRegistryItem GetPrinterForCcsuk(ZString printCodeWithOrWithourPrefixAndSuffix, string printName = "unknown")
		{
			//var printCode = printCodeWithOrWithourPrefixAndSuffix.Replace("DTI-", "").Left(2);  // e.g. P2, DTI-P2 or DTI-P2-XH-AMD becomes P2. 
			return GetItem("GBCcsukPrinter" + printCodeWithOrWithourPrefixAndSuffix, () =>
			{
				return new RegistryItemImplWithDynamicDefaultValue
					(
						"GBCcsukPrinter" + printCodeWithOrWithourPrefixAndSuffix,
						new[] { Categories.Customs_UnitedKingdom_PrinterseDocs_CCSUK },
						(NoResString)(string.Format(CultureInfo.CurrentCulture, "{0} {1}", printCodeWithOrWithourPrefixAndSuffix, printName)),
						(NoResString)(string.Format(CultureInfo.CurrentCulture, "Printer on which to print {0} ({1}) entry prints from CCSUK.", printCodeWithOrWithourPrefixAndSuffix, printName)),
						RegistryDataTypes.GuidType,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.StmPrintQueue),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(companyPK, branchPK, departmentPK) => PrinterCcsuk.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty)
					);
			});
		}

		public IRegistryItem PrinterCcsuk
		{
			get
			{
				return GetItem("GBCcsukPrinter", () =>
				{
					return new RegistryItemImplWithDynamicDefaultValue
						(
							"GBCcsukPrinter",
							new[] { Categories.Customs_UnitedKingdom_PrinterseDocs_CCSUK },
							(NoResString)"Default printer for CCSUK",
							(NoResString)"Printer on which CCS-UK prints (e.g. G2, F2, GR) are printed.  Falls back to branch level, the branch is decided using the home branch of the user whose name is on the job or message.",
							RegistryDataTypes.GuidType,
							new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.StmPrintQueue),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							RegistryOptions.Default,
							(companyPK, branchPK, departmentPK) => PrinterShared.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty)
						);
				});
			}
		}

		#endregion

		#region Misc

		public BooleanRegistryItem ChiefEnableRRS01Automation
		{
			get
			{
				return GetItem("GBChiefEnableRRS01Automation", () =>
				{
					return new BooleanRegistryItem(
						"GBChiefEnableRRS01Automation",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Enable RRS01 automation for CHIEF",
						(NoResString)"Enable automatic adding and removing of RRS01 AI statement for inventory-linked or arrived jobs.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem AllowLocationOfGoodsCalculationForNotArrivedGoods
		{
			get
			{
				return GetItem("GBAllowLocationOfGoodsCalculationForNotArrivedGoods", () =>
				{
					return new BooleanRegistryItem(
						"GBAllowLocationOfGoodsCalculationForNotArrivedGoods",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Allow location of goods calculation for not-arrived goods.",
						(NoResString)"Enabled setting location of goods automatically for all entry substyles (default); set to NO to calculate only for arrived goods",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public ItemDefaulterSettingCollectionRegistryItem ItemDefaults
		{
			get
			{
				return GetItem("GBItemDefaults", () =>
				{
					return new ItemDefaulterSettingCollectionRegistryItem(
						"GBItemDefaults",
						Categories.Customs_UnitedKingdom,
						"Item Defaults",
						"",
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
				});
			}
		}

		public BooleanRegistryItem ChiefUpdateJobsToClearWhenIcsCodeMeansClear
		{
			get
			{
				return GetItem("GBChiefUpdateJobsToClearWhenIcsCodeMeansClear", () =>
				{
					return new BooleanRegistryItem(
						"GBChiefUpdateJobsToClearWhenIcsCodeMeansClear",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Update import jobs to clear based on ICS",
						(NoResString)"If set to YES, upon receipt of a CUSRES with an ICS value that indicates 'clear' (e.g. 01), the job's status is updated to CLR and workflow is fired.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public IntRegistryItem ChiefMaximumMessageSizeInDecimalBytes
		{
			get
			{
				return GetItem("GBChiefMaximumMessageSizeInDecimalBytes", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"GBChiefMaximumMessageSizeInDecimalBytes",
						Categories.Customs_UnitedKingdom,
						(NoResString)"CHIEF data size limit",
						(NoResString)"CHIEF data size limit. Maximum size in decimal bytes.  CargoWise only.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						56000);  // defined by EDCS
					return result;
				});
			}
		}

		public BooleanRegistryItem ChiefValidationCreditCheck_NotLodgedArrived
		{
			get
			{
				return GetItem("ChiefValidationCreditCheck_NotLodgedArrived", () =>
				{
					return new BooleanRegistryItem(
						"ChiefValidationCreditCheck_NotLodgedArrived",
						Categories.Customs_UnitedKingdom_CreditChecking,
						(NoResString)"Check for unlodged arrived jobs",
						(NoResString)"Perform the credit check on declarations that do not have an entry number when sending an 'arrived' declaration",
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem ChiefValidationCreditCheck_AlwaysCheck
		{
			get
			{
				return GetItem("ChiefValidationCreditCheck_AlwaysOrNeverCheck", () =>
				{
					return new BooleanRegistryItem(
						"ChiefValidationCreditCheck_AlwaysOrNeverCheck",
						Categories.Customs_UnitedKingdom_CreditChecking,
						(NoResString)"Always check",
						(NoResString)"Always check the credit standing before sending a message.  Overrides all other settings in this category.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public BooleanRegistryItem ChiefValidationCreditCheck_PreLodgedArriving
		{
			get
			{
				return GetItem("ChiefValidationCreditCheck_PreLodgedArriving", () =>
				{
					return new BooleanRegistryItem(
						"ChiefValidationCreditCheck_PreLodgedArriving",
						Categories.Customs_UnitedKingdom_CreditChecking,
						(NoResString)"Check for prelodged arriving jobs",
						(NoResString)"Perform the credit check on declarations that are on route H when sending an 'arrived' declaration",
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public DecimalRegistryItem ChiefValidationCreditCheck_EstimateDuty
		{
			get
			{
				return GetItem("ChiefValidationCreditCheck_EstimateDuty", () =>
				{
					return new DecimalRegistryItem(
						"ChiefValidationCreditCheck_EstimateDuty",
						Categories.Customs_UnitedKingdom_CreditChecking,
						(NoResString)"Duty/VAT estimation threshold",
						(NoResString)"Perform the credit check on declarations that have an entry number and whose estimated duty/VAT at the time of submission is greater than the already recorded fees by this percentage. Set to a negative number to suppress the estimation.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						10m);
				});
			}
		}

		public BooleanRegistryItem ShouldSetTaxesToDeferredIfDeclarationSaySo
		{
			get
			{
				return GetItem("GBShouldSetTaxesToDeferredIfDeclarationSaySo", () =>
				{
					return new BooleanRegistryItem(
						"GBShouldSetTaxesToDeferredIfDeclarationSaySo",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Box 47 tax MoP defaults",
						(NoResString)"If set to YES, taxes (box 47) will have their MoP code set to F (deferred) based on the declaration's deferment settings (box 48).",
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem ReportMessageMissingMandatoryField
		{
			get
			{
				return GetItem("GBReportMessageMissingMandatoryField", () =>
				{
					return new BooleanRegistryItem(
						"GBReportMessageMissingMandatoryField",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Report Message Missing Mandatory Field",
						(NoResString)"Determines whether a message being sent with a missing mandatory field will generate an error and disallow sending of the declaration to Customs.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem AllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating
		{
			get
			{
				return GetItem("GBAllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating", () =>
				{
					return new BooleanRegistryItem(
						"GBAllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating",
						Categories.Customs_UnitedKingdom,
						(NoResString)"UCN/MUCR matching for inbound updates",
						(NoResString)"Allow inbound UCNs to match MUCRs on jobs verbatim, without trimming the MUCR? When set to no (default), an inbound UCN will be properly trimmed before being used to match existing jobs by MUCR.  When set to yes, matching is done using the trimmed OR untrimmed UCN.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem EnforceCHIEFRulesNotAllowed
		{
			get
			{
				return GetItem("GBEnforceCHIEFRulesNotAllowed", () =>
				{
					return new BooleanRegistryItem(
						"GBEnforceCHIEFRulesNotAllowed",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Enforce CHIEF Rules Not Allowed",
						(NoResString)"Determines whether a message being sent with fields that violate CHIEF rules for 'Not Allowed' fields will still be sent with their provided values, or have their values overridden.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem Eori_SendAiStatementsForEoriSuffixes
		{
			get
			{
				return GetItem("GbCustoms_Eori_SendAiStatementsForEoriSuffixes", () =>
				{
					return new BooleanRegistryItem(
						"GbCustoms_Eori_SendAiStatementsForEoriSuffixes",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Send EORI suffixes as AI statements?",
						(NoResString)"Set this to false to disable the sending of EORI suffixes (e.g. BR001, AG001) as AI statements in customs declarations.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public BooleanRegistryItem ProcInst_ACC
		{
			get
			{
				return GetItem("GBProcessingInstruction_ACC", () =>
				{
					return new BooleanRegistryItem(
						"GBProcessingInstruction_ACC",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Request acceptance reports - ACC",
						(NoResString)"Set to true to request acceptance reports of CHIEF.  This sets processing instruction 'ACC'. No effect on amendment.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public BooleanRegistryItem ProcInst_PRG
		{
			get
			{
				return GetItem("GBProcessingInstruction_PRG", () =>
				{
					return new BooleanRegistryItem(
						"GBProcessingInstruction_PRG",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Request progress reports - PRG",
						(NoResString)"Set to true to request progress reports of CHIEF.  This sets processing instruction 'PRG'.  Exports only. No effect on amendment.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public DecimalRegistryItem VAT_AdjustmentRatePerKiloAir
		{
			get
			{
				return GetItem("GBVAT_AdjustmentRatePerKiloAir", () =>
				{
					var item = new DecimalRegistryItem(
						"GBVAT_AdjustmentRatePerKiloAir",
						Categories.Customs_UnitedKingdom_VATAdjustmentbox68,
						(NoResString)"Air Rate Per Kilo",
						(NoResString)"Rate per kilo for VAT adjustment as defined by BIFA (£/kg).",
						RegistryStorageFlags.System,
						0.4m);
					item.EditorInfo = new NumericRegistryEditorInfo(2);
					return item;
				});
			}
		}

		public DecimalRegistryItem VAT_AdjustmentDeminimusAir
		{
			get
			{
				return GetItem("GBVAT_AdjustmentDeminimusAir", () =>
				{
					var item = new DecimalRegistryItem(
						"GBVAT_AdjustmentDeminimusAir",
						Categories.Customs_UnitedKingdom_VATAdjustmentbox68,
						(NoResString)"Air Minimum Adjustment",
						(NoResString)"The minimum VAT adjustment amount for air shipments.",
						RegistryStorageFlags.System,
						100m);
					item.EditorInfo = new NumericRegistryEditorInfo(2);
					return item;
				});
			}
		}

		public DecimalRegistryItem VAT_AdjustmentFCLFeeSeaAndRoad
		{
			get
			{
				return GetItem("GBVAT_AdjustmentFCLFeeSea", () =>
				{
					var item = new DecimalRegistryItem(
						"GBVAT_AdjustmentFCLFeeSea",
						Categories.Customs_UnitedKingdom_VATAdjustmentbox68,
						(NoResString)"FCL Rate Per Container",
						(NoResString)"Amount per FCL container for VAT adjustment.",
						RegistryStorageFlags.System,
						550.0m);
					item.EditorInfo = new NumericRegistryEditorInfo(2);
					return item;
				});
			}
		}

		public DecimalRegistryItem VAT_AdjustmentLCLRateSeaAndRoad
		{
			get
			{
				return GetItem("GBVAT_AdjustmentLCLFeeSea", () =>
				{
					var item = new DecimalRegistryItem(
						"GBVAT_AdjustmentLCLFeeSea",
						Categories.Customs_UnitedKingdom_VATAdjustmentbox68,
						(NoResString)"LCL Rate Per Tonne",
						(NoResString)"Amount per Tonne for VAT adjustment for LCL containers",
						RegistryStorageFlags.System,
						90.0m);
					item.EditorInfo = new NumericRegistryEditorInfo(2);
					return item;
				});
			}
		}

		public DecimalRegistryItem VAT_AdjustmentLCLFlatFeeSeaAndRoad
		{
			get
			{
				return GetItem("GBVAT_AdjustmentLCLFlatFeeSea", () =>
				{
					var item = new DecimalRegistryItem(
						"GBVAT_AdjustmentLCLFlatFeeSea",
						Categories.Customs_UnitedKingdom_VATAdjustmentbox68,
						(NoResString)"LCL Flat Fee",
						(NoResString)"Flat Fee for LCL containers for VAT adjustment.",
						RegistryStorageFlags.System,
						80.0m);
					item.EditorInfo = new NumericRegistryEditorInfo(2);
					return item;
				});
			}
		}

		public DecimalRegistryItem VAT_AdjustmentLCLDeminimusSeaAndRoad
		{
			get
			{
				return GetItem("GBVAT_AdjustmentLCLDeminimusSeaAndRoad", () =>
				{
					var item = new DecimalRegistryItem(
						"GBVAT_AdjustmentLCLDeminimusSeaAndRoad",
						Categories.Customs_UnitedKingdom_VATAdjustmentbox68,
						(NoResString)"LCL Minimum",
						(NoResString)"Minimum amount for LCL containers for VAT adjustment.",
						RegistryStorageFlags.System,
						170.0m);
					item.EditorInfo = new NumericRegistryEditorInfo(2);
					return item;
				});
			}
		}

		public BooleanRegistryItem GB_CustomsModuleEnabledForShipmentsAndConsols
		{
			get
			{
				return GetItem("GBCustomsModuleEnabledForShipmentsAndConsols", () =>
				{
					return new BooleanRegistryItem(
						"GBCustomsModuleEnabledForShipmentsAndConsols",
						Categories.Customs_UnitedKingdom,
						(NoResString)"CW1 GB customs module is enabled for shipment/consols",
						(NoResString)"Set this to NO to tell CW1 that attachments or detachments of shipments and consols should not cause an associate/disassociate message to be sent to CHIEF, and moreover that failures to send such messages due to insufficient data should be suppressed. There is no other effect.",
						RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem GB_UseBranchEoriForMucrOnConsols => GetItem("GBUseBranchEoriForMucrOnConsols",
			() => new BooleanRegistryItem(
				"GBUseBranchEoriForMucrOnConsols",
				Categories.Customs_UnitedKingdom,
				(NoResString)"Use current branch for EORI when calculating MUCR references on Consols",
				(NoResString)"Set this to true to always use the current branch's OrgProxy to provide an EORI when calculating Master UCR values for consols for MUCR modes that require an EORI. The default value of false will use the Sending Agent's EORI instead, if the Sending Agent is set.",
				RegistryStorageFlags.Branch,
				false)
			);

		public BooleanRegistryItem DisableBlueValidationOnMUCRForInventoryLinkedPortsImport
		{
			get
			{
				return GetItem("GBDisableBlueValidationOnMUCRForInventoryLinkedPortsImport", () =>
				{
					return new BooleanRegistryItem(
						"GBDisableBlueValidationOnMUCRForInventoryLinkedPortsImport",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Disable blue validation on MUCR for inventory-linked ports (Import)",
						(NoResString)"For a port (box [30]) that is known to be inventory linked, a missing MUCR on an import will show, when set to no (default) a message warning (blue) validation when absent. When set to yes, will show a warning message (yellow).",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem CDSStatisticalValueManualOverride
		{
			get
			{
				return GetItem("GBCDSStatisticalValueManualOverride", () =>
				{
					return new BooleanRegistryItem(
						"GBCDSStatisticalValueManualOverride",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Set override for statistical value for CDS imports",
						(NoResString)"Set override for statistical value for CDS imports?",
						RegistryStorageFlags.Company,
						false
						);
				});
			}
		}

		public BooleanRegistryItem DisableBlueValidationOnMUCRForExport
		{
			get
			{
				return GetItem("GBDisableBlueValidationOnMUCRForExport", () =>
				{
					return new BooleanRegistryItem(
						"GBDisableBlueValidationOnMUCRForExport",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Disable blue validation on MUCR for export",
						(NoResString)"A missing MUCR on an export will show, when set to no (default) a message warning (blue) validation when absent. When set to yes, will show a warning message (yellow).",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem N935_AddSupportingDocToInvoiceHeader
		{
			get
			{
				return GetItem("N935_AddSupportingDocToInvoiceHeader", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem("N935_AddSupportingDocToInvoiceHeader",
									Categories.Customs_UnitedKingdom,
									(NoResString)"N935 - add supporting document to invoice header",
									(NoResString)"When set to YES, this option will allow the automatic creation of a supporting document type N935, with the invoice number as its reference, against the invoice header.",
									RegistryStorageFlags.Branch,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem N935_AddAllInvoiceHeaderNumbersToAllInvoiceHeaders
		{
			get
			{
				return GetItem("N935_AddAllInvoiceHeaderNumbersToAllInvoiceHeaders", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem("N935_AddAllInvoiceHeaderNumbersToAllInvoiceHeaders",
									Categories.Customs_UnitedKingdom,
									(NoResString)"N935 - add all invoice header numbers to all invoice headers",
									(NoResString)"When set to YES, this option will allow the automatic creation of multiple supporting documents (type N935), referencing each invoice header on all invoice headers. This allows merging of otherwise-similar invoice lines across multiple invoice headers.",
									RegistryStorageFlags.Branch,
									true);
					return result;
				});
			}
		}

		public CDSDUCRAutomationRegistryItem CDSDUCRAutomation
		{
			get
			{
				return GetItem("CDSDUCRAutomation", () =>
				{
					return new CDSDUCRAutomationRegistryItem(
						"CDSDUCRAutomation",
						Categories.Customs_UnitedKingdom,
						(NoResString)"CDS DUCR Automation",
						(NoResString)"CDS DUCR automation with three options based on selecting a) Split entry reference into DUCR and Part fields b) Combine.  Send only a single entry reference. c) Do not automatically add DCR/DCS for imports and combine for exports.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new CDSDUCRAutomationSettings { CDSDUCRAutomation = CDSUCRAutomationSettingsList.Codes.NotForImports }
						);
				});
			}
		}

		public BooleanRegistryItem SendCDS317
		{
			get
			{
				return GetItem("SendCDS317", () =>
				{
					return new BooleanRegistryItem(
						"SendCDS317",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Send CDS 3/17",
						(NoResString)"For supported message types, send the declarant's name and address (3/17) in addition to always sending the declarant ID (3/18).",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		#endregion

		#region Badges and credentials

		public BadgeCodeSettingCollectionRegistryItem BadgeCodes
		{
			get
			{
				return GetItem("GBCustomsBadgeCodes", () =>
				{
					return new BadgeCodeSettingCollectionRegistryItem(
						"GBCustomsBadgeCodes",
						Categories.Customs_UnitedKingdom,
						"Badge Codes",
						"Badge Codes registered against a branch along with the associated Port Code and CSP/Gateway details. N.B. To provide a default badge to be used for all imports, or all exports, regardless of the port through which the job moves, enter a row with a blank port code and an appropriate 'Direction'. You may mix-and-match, so that a given port may use one badge for imports and another for exports.",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public CredentialsSettingCollectionRegistryItem Credentials
		{
			get
			{
				return GetItem("GbCustomsWebServiceCredentials", () =>
				{
					return new CredentialsSettingCollectionRegistryItem("GbCustomsWebServiceCredentials",
						Categories.Customs_UnitedKingdom,
						"Badge code credentials",
						@"Explanation of columns...

* Badge code (mnemonic): the (friendly) badge code as entered in the Badge Codes section of the registry. After editing the Badge Codes section, ensure changes are saved before editing this column, and ensure you are logged in as the correct company.

* Badge/Company or NES Role: your 3-character badge code, or for NES your role (e.g. THS1ABC). For Pentant this is your full 6-character badge code, e.g. PNTABC. If you are not using friendly/mnemonic names, this column matches the first column.

* Output Device/NES Location/PIMA/CDS Topic:
---- For MCP and CNS under CHIEF this is the EDI 'mailbox' or 'printer' to which CHIEF prints are sent, e.g. ABC9 or ABCFXTMLBX. It is not an email address, mail output device (ABCM) or ink-and-paper printer name.
---- For MCP and CNS under CDS this is the 'topic' allocated to your badge, e.g. ABCX.
---- For NES this is your EDCS location, e.g. LOCEDC1ABC.
---- For CCSUK this is your PIMA, e.g. CUKFFW98000ABC.
---- For Pentant, this is the name of the FTP folder allocated to you.

* Username & password: only needed for Pentant, MCP & CNS, these are the webservice or FTP login credentials. They are not the credentials used to access the Pentant, Destin8 or Compass websites.

* Fallback for shed:  the shed for which this fallback agent is acting (CCSUK only).

* Receiver ID and Sender ID.  These are only needed for Pentant - supply the values allocated to you by Pentant.

* Loader.  Tick this box to give your CCSUK badge the ability to send export arrival and departure messages. Tick this only if Customs have authorised your badge for this role.

* Endpoint. For Pentant, select the code that indicates which endpoint address has been allocated to you, the inventory address or the declaration address.

* Failures and test state.  Values indicating whether these web credentials (for MCP and CNS only) have been proven good, or have had successive login failures. Too many login failures and the use of the badge is suspended. You can remove the suspension by right-clicking the gutter of a row and selecting 'Check Credentials'.",
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue | RegistryOptions.NotCached);
				});
			}
		}

		#endregion

		#region ExcessNPRExclusionEventCode
		public ExcessNPRExclusionEventCodeSettingCollectionRegistryItem NPRExclusionEventCodes
		{
			get
			{
				return GetItem("CCSUK_excess_NPR_exclusion_event_code", () =>
				{
					return new ExcessNPRExclusionEventCodeSettingCollectionRegistryItem(
						"CCSUK_excess_NPR_exclusion_event_code",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						"CCSUK excess NPR exclusion event code",
						"Select the custom event code which will mean a CCSUK AWB with NPR > NPX is excluded from the AWB Warning Report.",
						RegistryStorageFlags.System);
				});
			}
		}
		#endregion

		#region NES

		public string NesEmailAddress
		{
			get
			{
				return IsInTestMode.Value ? NesEmailAddress_Test.Value : NesEmailAddress_Live.Value;
			}
			set
			{
				if (IsInTestMode.Value)
				{ NesEmailAddress_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
				else
				{ NesEmailAddress_Live.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
			}
		}

		StringRegistryItem NesEmailAddress_Live
		{
			get
			{
				return GetItem("GBNesEmailAddress_Live", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
									"GBNesEmailAddress_Live",
									Categories.Customs_UnitedKingdom_ServiceProviders_NES_Addresses,
									(NoResString)"Live email address",
									(NoResString)"Email address of the CIES (live) EDCS email service.  You should not edit this away from its default of edcs@edcsdata.hmce.gov.uk value unless instructed to.",
									RegistryStorageFlags.System,
									"edcs@edcsdata.hmce.gov.uk");  //     If you change this, also change the nudge predicate in the NES/GNE task definition
					return result;
				});
			}
		}

		StringRegistryItem NesEmailAddress_Test
		{
			get
			{
				return GetItem("GBNesEmailAddress_Test", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
									"GBNesEmailAddress_Test",
									Categories.Customs_UnitedKingdom_ServiceProviders_NES_Addresses,
									(NoResString)"Test email address",
									(NoResString)"Email address of the HMUT (test) EDCS email service.  You should not edit this away from its default value of stest@smtptest.hmce.gov.uk unless instructed to.",
									RegistryStorageFlags.System,
									"stest@smtptest.hmce.gov.uk");  //   If you change this, also change the nudge predicate in the NES/GNE task definition
					return result;
				});
			}
		}
		public StringRegistryItem EdcsWtgAlertString
		{
			get
			{
				return GetItem("GBEdcsWtgAlertString", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
									"GBEdcsWtgAlertString",
									Categories.Customs_UnitedKingdom_ServiceProviders_NES,
									(NoResString)"WTG EDCS Alert Message",
									(NoResString)"This message was set by WTG to alert your users that EDCS (NES email) is having problems.  It will be unset again by WTG once HMRC have confirmed that the problem is over.  You can also clear it yourself to suppress alerts.",
									new StringRegistryDataType(),
									new TextRegistryEditorInfo(TextEditorType.Memo),
									RegistryStorageFlags.System,
									RegistryOptions.Default,
									"");
					return result;
				});
			}
		}
		public DateTimeRegistryItem EdcsWtgAlertTime
		{
			get
			{
				return GetItem("GBEdcsWtgAlertTime", () =>
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
									"GBEdcsWtgAlertTime",
									Categories.Customs_UnitedKingdom_ServiceProviders_NES,
									(NoResString)"WTG EDCS Alert Time",
									(NoResString)"For 6 hours from this time, which was set by WTG, your users will be alerted that EDCS (NES email) is having problems.  It will be unset again by WTG once HMRC have confirmed that the problem is over.  You can also clear it yourself to suppress alerts.",
									new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
									RegistryStorageFlags.System,
									RegistryOptions.Default,
									DateTime.MinValue,
									false);
					return result;
				});
			}
		}

		#endregion

		#region CCSUK

		public BooleanRegistryItem CcsukAllowCheckinAtMawbLevel
		{
			get
			{
				return GetItem("GBCcsukAllowCheckinAtMawbLevel", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAllowCheckinAtMawbLevel",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Allow check-in at MAWB level?",
									(NoResString)"Option to Bulk Release / Delivery of MAWB & HAWBs in ETSF.",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public DateTimeRegistryItem CcsukGoLiveDate
		{
			get
			{
				return GetItem("GBCcsukGoLiveDate", () =>
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
									"GBCcsukGoLiveDate",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"CCSUK go live date",
									(NoResString)"CCSUK go live date",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
									ZDateTime.MinSmallDateTimeValue.ToDateTime());
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAlwaysSendAwbReport
		{
			get
			{
				return GetItem("GBCcsukAlwaysSendAwbReport", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAlwaysSendAwbReport",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Always send AWB Warning Report?",
									(NoResString)"Send CCS-UK AWB Warning Report even when it is empty",
									RegistryStorageFlags.System,
									false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukShowDEPProfiles
		{
			get
			{
				return GetItem("GBCcsukShowDEPProfiles", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem("GBCcsukShowDEPProfiles",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Show DEP profiles?",
									(NoResString)"Show DEP profiles for branch",
									RegistryStorageFlags.Branch,
									RegistryOptions.IsOnlyForSupport,
									false);
					return result;
				});
			}
		}

		public IntRegistryItem CcsukMaximumTransmittablePayloadSize
		{
			get
			{
				return GetItem("GBCcsukMaximumTransmittablePayloadSize", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"GBCcsukMaximumTransmittablePayloadSize",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"CCS-UK data size limit",
						(NoResString)"CCS-UK data size limit. Maximum size in decimal bytes. CargoWise only.  Cannot be larger than 0xA00000 (10,485,760) bytes or smaller than 50 bytes or the CUK task won't start. Set this only if advised by CCS-UK.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						0xA00000);  // 10 meg - 10485760 bytes
					return result;
				});
			}
		}

		public StringRegistryItem LucasFakePimaForCwTesting
		{
			get
			{
				return GetItem("GBLucasFakePimaForCwTesting", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBLucasFakePimaForCwTesting",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"Fake LUCAS PIMA (for CW only)",
						(NoResString)string.Format(CultureInfo.CurrentCulture, "Set this to be your own PIMA so that you can send yourself Genral messages and {0} will treat the enquiry as if it came from LUCAS.", Core.Constants.ProductName),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						"");
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAutoPopulateHawbsOnConsol
		{
			get
			{
				return GetItem("GBCcsukAutoPopulateHawbsOnConsol", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAutoPopulateHawbsOnConsol",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Auto-populate HAWBs on Consol",
									(NoResString)"Determines whether HAWBs should be created for shipments when a user creates the MAWB by clicking in the CCSUK tab from within a Consol.",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAutoPrintC1WhenAllPiecesReceivedAndReleased
		{
			get
			{
				return GetItem("GBCcsukAutoPrintC1WhenAllPiecesReceivedAndReleased", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAutoPrintC1WhenAllPiecesReceivedAndReleased",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Automatically print C1 releases",
									(NoResString)"Controls whether C1 release documents will be printed automatically when all pieces are received and granted release via FSN or FRC message.",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public IntRegistryItem CcsukPingPeriod
		{
			get
			{
				return GetItem("GBCcsukPingPeriod", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
									"GBCcsukPingPeriod",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Ping period in seconds",
									(NoResString)"Ping period in seconds (WTG only)",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForSupport,
									600);// 10 mins
					return result;
				});
			}
		}

		public DateTimeRegistryItem CcsukLastPingDateTime => GetItem("GBCcsukLastPingDateTime",
			() => new DateTimeRegistryItem(
				"GBCcsukLastPingDateTime",
				Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
				(NoResString)"Last ping",
				(NoResString)"When did we last ping CCSUK?",
				new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.LongIncludingSeconds),
				RegistryStorageFlags.System,
				RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
				DateTime.MinValue,
				false)
			);

		public DateTimeRegistryItem CcsukLastLogonDateTime => GetItem("GBCcsukLastLogonDateTime",
			() => new DateTimeRegistryItem(
				"GBCcsukLastLogonDateTime",
				Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
				(NoResString)"Last logon",
				(NoResString)"When did we last logon to CCSUK?",
				new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.LongIncludingSeconds),
				RegistryStorageFlags.System,
				RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
				DateTime.MinValue,
				false)
			);

		public StringRegistryItem CcsukLastConnectedProfile => GetItem("GBCcsukLastConnectedProfile",
			() => new StringRegistryItem(
				"GBCcsukLastConnectedProfile",
				Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
				(NoResString)"Last connected profile",
				(NoResString)"Profile details last used to connect to CCSUK",
				new StringRegistryDataType(CharacterCase.Normal),
				new TextRegistryEditorInfo(TextEditorType.Memo),
				RegistryStorageFlags.System,
				RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
				string.Empty)
			);

		public DateTimeRegistryItem CcsukLastSentMessageDateTime => GetItem("GBCcsukLastSentMessageDateTime",
			() => new DateTimeRegistryItem(
				"GBCcsukLastSentMessageDateTime",
				Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
				(NoResString)"Last sent message",
				(NoResString)"When did we last send a message to CCSUK?",
				new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.LongIncludingSeconds),
				RegistryStorageFlags.System,
				RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
				DateTime.MinValue,
				false)
			);

		public DateTimeRegistryItem CcsukLastReceivedMessageDateTime => GetItem("GBCcsukLastReceivedMessageDateTime",
			() => new DateTimeRegistryItem(
				"GBCcsukLastReceivedMessageDateTime",
				Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
				(NoResString)"Last received message",
				(NoResString)"When did we last receive a message from CCSUK?",
				new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.LongIncludingSeconds),
				RegistryStorageFlags.System,
				RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
				DateTime.MinValue,
				false)
			);

		public BooleanRegistryItem CcsukIsConnected => GetItem("GBCcsukIsConnected",
			() => new BooleanRegistryItem(
				"GBCcsukIsConnected",
				Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
				(NoResString)"Is connected",
				(NoResString)"Are we connected to CCSUK?",
				RegistryStorageFlags.System,
				RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
				false)
			);

		public StringRegistryItem CcsukConnectedProcessController => GetItem("GBCcsukConnectedProcessController",
			() => new StringRegistryItem(
				"GBCcsukConnectedProcessController",
				Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
				(NoResString)"Connected process controller",
				(NoResString)"Name of the process controller server that is connected to CCSUK",
				RegistryStorageFlags.System,
				RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
				string.Empty)
			);

		public IntRegistryItem CcsukProcessID => GetItem("GBCcsukProcessID",
			() => new IntRegistryItem(
				"GBCcsukProcessID",
				Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network_Dashboard,
				(NoResString)"Process ID",
				(NoResString)"PID of the connected process on the process controller server",
				RegistryStorageFlags.System,
				RegistryOptions.IsReadOnly | RegistryOptions.PreserveTestValue,
				0)
			);

		public BooleanRegistryItem CcsukQueryChildObjectsWhenMentionedInFsa
		{
			get
			{
				return GetItem("GBCcsukQueryChildObjectsWhenMentionedInFsa", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukQueryChildObjectsWhenMentionedInFsa",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Automatically query child splits consignments from FSA?",
									(NoResString)string.Format(CultureInfo.CurrentCulture, "When a consignment is queried using an FSR, and the 'update' option is used, then this option will control whether {0} should automatically query (with update) the child consignments.  For example, when a basic with 3 splits is queried, setting this option will result in three new queries being created automatically, one for each split. This option has no effect if the 'update' type of FSR is not sent.", Core.Constants.ProductName),
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAlsoSendFsrAfterFrx
		{
			get
			{
				return GetItem("GBCcsukAlsoSendFsrAfterFrx", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAlsoSendFsrAfterFrx",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Automatically send FSR message after FRX to confirm deletion?",
									(NoResString)"When set to yes, an FSR query is automatically sent after an FRX delete request, which will act as a confirmation of the delete.",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAllowShedAutoSplitFromFrd
		{
			get
			{
				return GetItem("GBCcsukAllowShedAutoSplitFromFrd", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAllowShedAutoSplitFromFrd",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Automatically split consignments from FRD?",
									(NoResString)"Enables the automatic splitting of consignments using inbound FRD requests from agents. Set to YES to automatically create FCS messages based on the FRD, set to NO to force the splitting to be made manually",
									RegistryStorageFlags.System,
									false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAllowVersionThreeCuscarForSpecialHandling
		{
			get
			{
				return GetItem("GBCcsukAllowVersionThreeCuscarForSpecialHandling", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAllowVersionThreeCuscarForSpecialHandling",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Version 3 CUSCAR messages",
									(NoResString)@"Use version 3 of the CUSCAR message? This will allow sending of Special/Community Handling Codes in FRI and FRC messages.  This should only be enabled once CCS-UK can support version 3 messages.",
									RegistryStorageFlags.System,
									false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukMarkAsArchivedWhenNoLongerOurConsignment
		{
			get
			{
				return GetItem("GBCcsukMarkAsArchivedWhenNoLongerOurConsignment", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukMarkAsArchivedWhenNoLongerOurConsignment",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Archive when nominated to an external agent",
									(NoResString)@"When processing an FRC message which advises that the newly-nominated agent is not one of our badges, mark the record as 'archived'",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAllowStatus1DateInFrc
		{
			get
			{
				return GetItem("GBCcsukAllowStatus1DateInFrc", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAllowStatus1DateInFrc",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Status 1 in FRC",
									(NoResString)@"Send Status 1 date in FRC messages where relevant.",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public CcsukNonstandardPimaSettingCollectionRegistryItem CcsukNonStandardPimas
		{
			get
			{
				return GetItem("GBCcsukNonstandPimas", () =>
				{
					return new CcsukNonstandardPimaSettingCollectionRegistryItem(
						"GBCcsukNonstandPimas",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						"PIMA mappings",
						"Supply here details of any non-standard PIMA that should be used instead of a standard one. Edit this only if advised by CCS-UK or " + BrandingFactory.Instance.CompanyName + ".",
						RegistryStorageFlags.System,
						new CcsukNonstandardPimaSettingCollection().GetDefaultValues());
				});
			}
		}

		public BooleanRegistryItem CcsukTemporarilyAllowDeactivationOfCcsukAwbs
		{
			get
			{
				return GetItem("GBCcsukTemporarilyAllowDeactivationOfCcsukAwbs", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukTemporarilyAllowDeactivationOfCcsukAwbs",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Allow deactivation?",
									(NoResString)@"WTG only. Temporarily set this to YES to enable the menu option on a CCSUK AWB to allow WTG staff to deactivate it. You should revert this setting to NO after you have deactivated the records.  This registry option is to hide the menu item when it is not needed, so that it does not appear in eLearning videos, etc. ",
									RegistryStorageFlags.System,
									false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukPutChcsFromInboundP5ReportOntoNewJob
		{
			get
			{
				return GetItem("GBCcsukPutChcsFromInboundP5ReportOntoNewJob", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukPutChcsFromInboundP5ReportOntoNewJob",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Store CHCs from P5",
									(NoResString)@"Set this to store (YES, default) or ignore (NO) the Community Handling Codes (CHCs) contained in inbound P5 reports, which advise of an inbound inter-shed or inter-airport remove to your shed.",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAllowConsolPluginForNonAirModesWhenAtLeastOneGbAirShipment
		{
			get
			{
				return GetItem("GBCcsukAllowConsolPluginForNonAirModesWhenAtLeastOneGbAirShipment", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAllowConsolPluginForNonAirModesWhenAtLeastOneGbAirShipment",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Allow Consol Plugin for non-Air",
									(NoResString)string.Format(CultureInfo.CurrentCulture, @"Set this value to allow the CCS-UK plugin to be shown for Consols whose mode is other than AIR, so long as the Consol has at least one AIR Shipment into GB"),
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAllocationOfNPR
		{
			get
			{
				return GetItem("GBCcsukAllocationOfNprOption", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAllocationOfNprOption",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Allocation of NPR",
									(NoResString)string.Format(CultureInfo.CurrentCulture, @"Set this value to dictate how {0} will allocate NPR to splits when status 1 is obtained on a superior record via an FRC.  
NO = Do not allocate NPR to splits when setting status 1 on the superior record (not recommended)
YES = Set NPR to be NPX on all split records when setting status 1 on the superior record (default)", Core.Constants.ProductName),
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAllowSplittingOfEcStatusJobs
		{
			get
			{
				return GetItem("GBCcsukAllowSplittingOfEcStatusJobs", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAllowSplittingOfEcStatusJobs",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Allow splitting of SDC=C/E",
									(NoResString)string.Format(CultureInfo.CurrentCulture, @"Set this to allow the 'Request and manage splits' menu option to appear for jobs with SDC=C/E. Normally should be NO because FCS for EC jobs usually results in NAK. {0} Support only.", Core.Constants.ProductName),
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForSupport,
									false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukCascadeStatus1FromBasicToNewHouse
		{
			get
			{
				return GetItem("GBCcsukCascadeStatus1FromBasicToNewHouse", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukCascadeStatus1FromBasicToNewHouse",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Allow Status 1 Cascade",
									(NoResString)@"Controls the automatic cascade of Status 1 from a basic air waybill to its houses when first adding the houses.",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukAllowAutoFrc
		{
			get
			{
				return GetItem("GBCcsukAllowAutoFrc", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAllowAutoFrc",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Allow Auto-FRC",
									(NoResString)string.Format(CultureInfo.CurrentCulture, @"Disable this to prevent automatic amendments messages (FRCs) being sent upon saving. This is not an option for the running of the client's business (it's a requirement), but it's a get-out-of-jail-free card in case we get into a pickle. {0} Support only.", Core.Constants.ProductName),
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForSupport,
									true);
					return result;
				});
			}
		}
		public BooleanRegistryItem CcsukAllowAutoPrintOfRRA
		{
			get
			{
				return GetItem("GBCcsukAllowAutoPrintOfRRA", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukAllowAutoPrintOfRRA",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Allow auto-print RRA",
									(NoResString)@"Enable this option to allow the automatic production of an ETSF release note. The release note will be created when an FSN message sets a relevant customs action code (e.g. CC), and status 1 is *already* set, and the releasable pieces have the 'Release Now' flag set.",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukP5ProcessorShouldInsertNewRecords
		{
			get
			{
				return GetItem("GBCcsukP5ProcessorShouldInsertNewRecords", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukP5ProcessorShouldInsertNewRecords",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"P5 auto-insert",
									(NoResString)@"When processing P5 advice of inter-shed removal reports, insert a new record if none is found, to save re-keying data. For splits, see also 'P5 split behaviour'.",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}
		public BooleanRegistryItem CcsukP5ProcessorSplitHandlingShouldMakeNewSplit
		{
			get
			{
				return GetItem("GBCcsukP5ProcessorSplitHandlingNew", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukP5ProcessorSplitHandlingNew",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"P5 split behavior",
									(NoResString)@"When processing P5 advice of inter-shed removal reports for a single split, if the parent record is not already in the local database, should a split be added (YES) or should a dummy basic be created for the split (NO)",
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukP5ProcessorLinkToConsole
		{
			get
			{
				return GetItem("GBCcsukP5ProcessorLinkToConsole", () =>
				{
					return new BooleanRegistryItem(
						"GBCcsukP5ProcessorLinkToConsole",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"P5 Console Linkage behavior",
						(NoResString)@"When processing P5 advice of inter-shed removal reports, should the MAMB or HAWB be linked to the consolidation and corresponding shipment?",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public CodePairRegistryItem CcsukTruncateChiefBox7ToRight8Characters
		{
			get
			{
				return GetItem("GBCcsukTruncateChiefBox7ToRight8Characters", () =>
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
									"GBCcsukTruncateChiefBox7ToRight8Characters",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"CHIEF box 7 behaviour",
									(NoResString)@"CHIEF allows 21 characters for the trader's own reference to the entry (box 7) but CCS-UK will return only 8 characters in the agent's reference field of status updates. Set this to true to truncate box 7 before sending (turning SLHR000012345 into 00012345 rather than leaving CCS-UK to turn it into SLHR0000). Set to no to send to CHIEF without truncating. Applied only to inventory-controlled air imports sent via CCS-UK.",
									new CodeDescriptionPairListProvider(() => new CcsukChiefBox7BehaviourList()),
									RegistryStorageFlags.System,
									CcsukChiefBox7BehaviourList.Codes.DefaultSendFullReferenceWithoutManipulation);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukMakeAwbNumberAndNamedPartyFieldsReadOnly
		{
			get
			{
				return GetItem("GBCcsukMakeAwbNumberAndNamedPartyFieldsReadOnly", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukMakeAwbNumberAndNamedPartyFieldsReadOnly",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Make AWB# and party fields readonly",
									(NoResString)string.Format(CultureInfo.CurrentCulture, @"This controls whether {0} will make the MAWB or HAWB number and shed/airport of an agent-profile inventory record readonly if the record is known to be on, or completed on, the CCS-UK database. Set to NO to not set to readonly under these criteria.", Core.Constants.ProductName),
									RegistryStorageFlags.System,
									true);
					return result;
				});
			}
		}

		public string CcsukRemoteIpAddress_ADSL
		{
			get { return IsInTestMode.Value ? CcsukRemoteIpAddress_ADSL_Test.Value : CcsukRemoteIpAddress_ADSL_Live.Value; }
			set
			{
				if (IsInTestMode.Value)
				{
					CcsukRemoteIpAddress_ADSL_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					CcsukRemoteIpAddress_ADSL_Live.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		internal StringRegistryItem CcsukRemoteIpAddress_ADSL_Live
		{
			get
			{
				return GetItem("GBCcsukIpAddress_Live", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukIpAddress_Live",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"CCS-UK remote host IP (live)",
						(NoResString)"CCS-UK remote host IP. The IP of the remote CCSUK host. Check with CCS-UK before editing this.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"172.22.250.3");
					return result;
				});
			}
		}

		internal StringRegistryItem CcsukRemoteIpAddress_VPN_Live
		{
			get
			{
				return GetItem("GBCcsukIpAddress_VPN_Live", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukIpAddress_VPN_Live",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"CCS-UK remote host IP (VPN) (live)",
						(NoResString)"CCS-UK remote host IP. The IP of the remote CCSUK host for VPN access. Check with CCS-UK before editing this.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"195.218.40.1");
					return result;
				});
			}
		}

		internal StringRegistryItem CcsukRemoteIpAddress_ADSL_Test
		{
			get
			{
				return GetItem("GBCcsukIpAddress_Test", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukIpAddress_Test",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"CCS-UK remote host IP (test)",
						(NoResString)"CCS-UK remote host IP. The IP of the remote CCSUK host. Check with CCS-UK before editing this.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"172.22.240.241");
					return result;
				});
			}
		}

		internal StringRegistryItem CcsukRemoteIpAddress_VPN_Test
		{
			get
			{
				return GetItem("CcsukRemoteIpAddress_VPN_Test", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"CcsukRemoteIpAddress_VPN_Test",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"CCS-UK remote host IP (VPN) (test)",
						(NoResString)"CCS-UK remote host IP. The IP of the remote CCSUK host for VPN access. Check with CCS-UK before editing this.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"195.218.40.2"); // TODO - check with BT
					return result;
				});
			}
		}

		public string CcsukRemoteIpAddress_VPN
		{
			get { return IsInTestMode.Value ? CcsukRemoteIpAddress_VPN_Test.Value : CcsukRemoteIpAddress_VPN_Live.Value; }
			set
			{
				if (IsInTestMode.Value)
				{
					CcsukRemoteIpAddress_VPN_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					CcsukRemoteIpAddress_VPN_Live.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		public StringRegistryItem CcsukLocalHostMnemonic
		{
			get
			{
				return GetItem("GBCcsukHostName", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukHostName",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"Local host Mnemonic",
						(NoResString)"Local host Mnemonic. Required. The hostname assigned to you by CCSUK.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"");
					return result;
				});
			}
		}

		public StringRegistryItem CcsukNetworkIpToDeclareForCallBack_Legacy
		{
			get
			{
				return GetItem("GBCcsukLocalIpToDeclareForCallBack", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukLocalIpToDeclareForCallBack",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"CCS-UK network IP for callback.",
						(NoResString)string.Format(CultureInfo.CurrentCulture, "CCS-UK network IP for callback. Recommended (required if NAT in use or if multiple CCS-UK IPs will exist on the service task host sever). This is the address that will go into the outbound handshake request to tell CCSUK where to call back. It MUST be on the CCSUK network but does not need to represent the IP of a {0} service host. If not specified then {0} will request a callback on the first CCSUK address found on the host, and if no such CCSUK address (172.220.0) is found will throw an exception.", Core.Constants.ProductName),
						new StringRegistryDataType(0, 15),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"");
					return result;
				});
			}
		}

		public int CcsukRemotePort
		{
			get { return IsInTestMode.Value ? CcsukRemotePort_Test.Value : CcsukRemotePort_Live.Value; }
			set
			{
				if (IsInTestMode.Value)
				{
					CcsukRemotePort_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					CcsukRemotePort_Live.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		IntRegistryItem CcsukRemotePort_Live
		{
			get
			{
				return GetItem("GBCcsukRemotePort_Live", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"GBCcsukRemotePort_Live",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"CCS-UK remote port (live)",
						(NoResString)"CCS-UK remote port. Remote TCP port to which outbound sockets will connect. Check with CCS-UK before editing this.",
						RegistryStorageFlags.System,
						4000);  // defined by CCSUK 
					return result;
				});
			}
		}

		IntRegistryItem CcsukRemotePort_Test
		{
			get
			{
				return GetItem("GBCcsukRemotePortTest", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"GBCcsukRemotePortTest",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"CCS-UK remote port (test)",
						(NoResString)"CCS-UK remote port. Remote TCP port to which outbound sockets will connect. Check with CCS-UK before editing this.",
						RegistryStorageFlags.System,
						4014);  // defined by CCSUK 
					return result;
				});
			}
		}

		public CcsukIpaddressesSettingCollectionRegistryItem CcsukIpAddresses
		{
			get
			{
				return GetItem("GBCcsukIpAddresses", () =>
				{
					return new CcsukIpaddressesSettingCollectionRegistryItem(
						"GBCcsukIpAddresses",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						"IP Addresses",
						"Pairs of local and participant IP addresses.",
						RegistryStorageFlags.System);
				});
			}
		}

		public StringRegistryItem CcsukLocalIpForBindingListener_Legacy
		{
			get
			{
				return GetItem("GBCcsukLocalIpForBinding", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukLocalIpForBinding",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"Local IP for binding",
						(NoResString)"Local IP for binding listener. Optional. Bind listening socket to this local IP.  Does not need to be an address on the CCSUK network, but if not you must ensure your routing is correct and that the call back IP is one on the network. If blank the socket will bind to IpAddress.Any (i.e. to  0.0.0.0)",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"");
					return result;
				});
			}
		}

		public IntRegistryItem CcsukLocalPortForBinding
		{
			get
			{
				return GetItem("GBCcsukLocalPortForBinding", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"GBCcsukLocalPortForBinding",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"Start of local port range",
						(NoResString)"Start of local port range. Start looking for free TCP ports to which to bind the listening socket starting at this value. Only set to something other than 5000 if advised by CCSUK or CW.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						5000);
					return result;
				});
			}
		}

		public IntRegistryItem CcsukSpoolPeriodInSeconds
		{
			get
			{
				return GetItem("GBCcsukSpoolPeriodInSeconds", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"GBCcsukSpoolPeriodInSeconds",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"Spool Period",
						(NoResString)"Spool period in seconds.  How often the CCSUK sender should look in the database for waiting messages.",
						RegistryStorageFlags.System,
						5);
					return result;
				});
			}
		}

		public StringRegistryItem CcsukPassword
		{
			get
			{
				return GetItem("GBCcsukPassword", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukPassword",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"CCS-UK password",
						(NoResString)"CCS-UK password",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						"");
					return result;
				});
			}
		}

		public StringRegistryItem CcsukPassword_New
		{
			get
			{
				return GetItem("GBCcsukPasswordNew", () =>
				{
					// public StringRegistryItem(TextRegistryEditorInfo EditorInfo, RegistryStorageFlags Storage, RegistryOptions Options, string DefaultValue);
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukPasswordNew",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_Network,
						(NoResString)"CCS-UK password (to change)",
						(NoResString)"New CCS-UK password. Set this to change your CCSUK password without calling the helpdesk. Also changes HCI password.  Cycle the process controller after setting this.",
						new StringRegistryDataType(CharacterCase.Upper, 0, 14),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.Default | RegistryOptions.PreserveTestValue,
						"");
					return result;
				});
			}
		}

		public StringRegistryItem CcsukShedAutoRenominateTrustedAgents
		{
			get
			{
				return GetItem("GBCcsukTrustedAgents", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukTrustedAgents",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"ERTS - FRN trusted agent list.",
						(NoResString)"Always automatically renominate AWBs if requested by any of these agents.  Comma-separated list, asterisk to trust every agent, or blank to trust no-one. Also check the forbid list.  e.g. AAA,BBB,CCC",
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						"*");
					return result;
				});
			}
		}

		public StringRegistryItem CcsukShedAutoRenominateForbiddenAgents
		{
			get
			{
				return GetItem("GBCcsukForbiddenAgents", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukForbiddenAgents",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"ERTS - FRN forbidden agent list.",
						(NoResString)"Never automatically renominate an AWB when requested to do so by these agents. Forbid trumps trust. Comma-separated list or asterisk to forbid every agent (manually send FRCs). e.g. AAA,BBB,CCC",
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						"");
					return result;
				});
			}
		}

		public StringRegistryItem CcsukDropOffRotationNumberHighWatermark
		{
			get
			{
				return GetItem("GBCcsukDropOffRotationNumberHighWatermark", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukDropOffRotationNumberHighWatermark",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"DRP Highwatermark.",
						(NoResString)"DRP Highwatermark.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						"");
					return result;
				});
			}
		}

		public IntRegistryItem CcsukMaximumBatchSize
		{
			get
			{
				return GetItem("CcsukMaximumBatchSize", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"CcsukMaximumBatchSize",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"CCS-UK Batch Size for Sending",
						(NoResString)"CCS-UK Batch. Sets the maximum number of interchanges to send in a batch. Set to 0 for no maximum batch size",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						0); 
					return result;
				});
			}
		}

		public BooleanRegistryItem ForceRecalculationOfNPR
		{
			get
			{
				return GetItem("ForceRecalculationOfNPR", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"ForceRecalculationOfNPR",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
									(NoResString)"Force recalculation of NPR while sending interchanges",
									(NoResString)"This setting controls whether we allow the recalculation of NPR for Airway Bills while sending interchanges.  Do not edit this setting without consulting WTG",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
									false);
					return result;
				});
			}
		}

		public IntRegistryItem CcsukInterchangePackagerBatchSize
		{
			get
			{
				return GetItem("CcsukInterchangePackagerBatchSize", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"CcsukInterchangePackagerBatchSize",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"CCS-UK Batch Size for Packaging Interchanges",
						(NoResString)"CCS-UK Batch. Sets the maximum number of messages retrieved for packaging into an interchange.  Do not edit this setting without consulting WTG",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						50);
					return result;
				});
			}
		}

		#endregion

		#region  CCSUK HCI

		IntRegistryItem CcsukRemotePortHCI_Live
		{
			get
			{
				return GetItem("GBCcsukRemotePortHCI_Live", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"GBCcsukRemotePortHCI_Live",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
						(NoResString)"HCI Port (live)",
						(NoResString)"Only set to something other than 5000 if advised by CCSUK or CW.",
						RegistryStorageFlags.System,
						4500);
					return result;
				});
			}
		}
		IntRegistryItem CcsukRemotePortHCI_Test
		{
			get
			{
				return GetItem("GBCcsukRemotePortHCI_Test", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
						"GBCcsukRemotePortHCI_Test",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
						(NoResString)"HCI Port (test)",
						(NoResString)"Only set to something other than 5000 if advised by CCSUK or CW.",
						RegistryStorageFlags.System,
						4018);
					return result;
				});
			}
		}

		public int CcsukRemotePortHCI
		{
			get { return IsInTestMode.Value ? CcsukRemotePortHCI_Test.Value : CcsukRemotePortHCI_Live.Value; }
			set
			{
				if (IsInTestMode.Value)
				{
					CcsukRemotePortHCI_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
				else
				{
					CcsukRemotePortHCI_Live.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
				}
			}
		}

		public StringRegistryItem CcsukHciRegistryKey
		{
			get
			{
				return GetItem("GBCcsukHciRegistryKey", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukHciRegistryKey",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
						(NoResString)"HCI Registry Key",
						(NoResString)@"HCI registry key, e.g. SOFTWARE\Wow6432Node\Syntegra\HCI for x64 machines or SOFTWARE\Syntegra\HCI for x86",
						RegistryStorageFlags.System,
						@"");
					return result;
				});
			}
		}

		public StringRegistryItem CcsukHciExe
		{
			get
			{
				return GetItem("GBCcsukHciExe", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukHciExe",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
						(NoResString)"HCI Executable",
						(NoResString)@"Name of the HCI executable (without path)",
						RegistryStorageFlags.System,
						"hci32.exe");
					return result;
				});
			}
		}

		public StringRegistryItem CcsukHciMsgDll
		{
			get
			{
				return GetItem("GBCcsukHciMsgDll", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukHciMsgDll",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
						(NoResString)"HCI Message DLL",
						(NoResString)@"Name of the HCI messaging DLL (without path)",
						RegistryStorageFlags.System,
						"HCI32Msg.dll");
					return result;
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "This is just a hint/example")]
		public StringRegistryItem CcsukHciInstallationPath
		{
			get
			{
				return GetItem("GBCcsukHciInstallationPath", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukHciInstallationPath",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
						(NoResString)"HCI Installation Path",
						(NoResString)@"Path to the location of the (shared) HCI installation, e.g. C:\Program Files (x86)\BT\HCI32",
						RegistryStorageFlags.System,
						"");
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukHciWantDebug
		{
			get
			{
				return GetItem("GBCcsukHciWantDebug", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukHciWantDebug",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
									(NoResString)"Set Debug Flag",
									(NoResString)"Debug flag.  Set to true to aid diagnosis of HCI problems, in combination with the 'log file' setting.",
									RegistryStorageFlags.System,
									false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukHciWantTrace
		{
			get
			{
				return GetItem("GBCcsukHciWantTrace", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukHciWantTrace",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
									(NoResString)"Set Trace Flag",
									(NoResString)"Trace flag.  Set to true to aid diagnosis of HCI problems, in combination with the 'log file' setting.",
									RegistryStorageFlags.System,
									false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CcsukHciWantHexDump
		{
			get
			{
				return GetItem("GBCcsukHciWantHexDump", () =>
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
									"GBCcsukHciWantHexDump",
									Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
									(NoResString)"Set Hex Dump Flag",
									(NoResString)"Hex dump flag.  Set to true to aid diagnosis of HCI problems, in combination with the 'log file' setting.",
									RegistryStorageFlags.System,
									false);
					return result;
				});
			}
		}

		public StringRegistryItem CcsukHciDebugLogFile
		{
			get
			{
				return GetItem("GBCcsukHciDebugLogFile", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukHciDebugLogFile",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
						(NoResString)"HCI Log File",
						(NoResString)@"HCI log file name",
						RegistryStorageFlags.System,
						"");
					return result;
				});
			}
		}

		public StringRegistryItem CcsukHciRegistryHive
		{
			get
			{
				return GetItem("GBCcsukHciRegistryHive", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCcsukHciRegistryHive",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK_HCI,
						(NoResString)"HCI Registry Hive",
						(NoResString)@"HCI registry hive, e.g. HKLM, HKCU, HKU, HKCR, HKCC",
						RegistryStorageFlags.System,
						"HKLM");
					return result;
				});
			}
		}

		#endregion

		#region ICS stuff

		public StringRegistryItem ICSUsername
		{
			get
			{
				return GetItem("ICSUsername", () =>
				{
					return new StringRegistryItem(
						"ICSUsername",
						Categories.Customs_UnitedKingdom_ICS,
						(NoResString)"Username",
						(NoResString)"Username for ICS access",
						new StringRegistryDataType(0, 35),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers);
				});
			}
		}

		public StringRegistryItem ICSPasssword
		{
			get
			{
				return GetItem("ICSPasssword", () =>
				{
					return new StringRegistryItem(
						"ICSPasssword",
						Categories.Customs_UnitedKingdom_ICS,
						(NoResString)"Password",
						(NoResString)"Password for ICS access",
						new StringRegistryDataType(0, 35),
						new TextRegistryEditorInfo(TextEditorType.Password),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						"");
				});
			}
		}

		#endregion

		#region NCTS Government Gateway and system configuration

		public BooleanRegistryItem CTC_UseDepartureIdOrArrivalId
		{
			get
			{
				return GetItem("CTC_UseDepartureIdOrArrivalId", () =>
				{
					return new BooleanRegistryItem(
						"CTC_UseDepartureIdOrArrivalId",
						Categories.Customs_UnitedKingdom_ServiceProviders_NCTS,
						ResString.GetMultilingualString("29DA03C3-714C-46ED-B5EB-C40FD3215411", @"Use CTC departure ID or arrival ID for message correlation."),
						ResString.GetMultilingualString("92330E06-AC87-444F-ABA4-FE270D8798EE", @"Use the departure ID or arrival ID instead of the CAR or Box [7] reference for processing CTC messages,"),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public BooleanRegistryItem NCTS_Fallback_Business_Continuity_Is_Active
		{
			get
			{
				return GetItem("NCTS_Fallback_Business_Continuity_Is_Active", delegate
				{
					var result = new BooleanRegistryItem(
						"NCTS_Fallback_Business_Continuity_Is_Active",
						Categories.Customs_UnitedKingdom_ServiceProviders_NCTS,
						ResString.GetMultilingualString("5F71AAA7-F768-4199-9821-298AEF4042D7", "NCTS fallback (business continuity) is active"),
						ResString.GetMultilingualString("8C8E7566-57F1-48A8-9C96-9E2BD9F65645", "NCTS fallback (business continuity) is active"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
					result.DataType = new BooleanRegistryDataType();
					return result;
				});
			}
		}

		public BooleanRegistryItem SuppressForeignEORI
		{
			get
			{
				return GetItem("SuppressForeignEORI", () =>
				{
					return new BooleanRegistryItem(
						"SuppressForeignEORI",
						Categories.Customs_UnitedKingdom_ServiceProviders_NCTS,
						ResString.GetMultilingualString("A33F6477-8567-4003-B4E9-F7517ECB0EC8", @"Suppress Foreign EORI"),
						ResString.GetMultilingualString("50D71709-E10D-426D-A205-677F25C241C8", @"Suppress Foreign EORI"),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}
		#endregion

		#region Exchange rate

		public StringRegistryItem HmrcExchangeRateYearlyRelativeUrlWithoutYear
		{
			get
			{
				return GetItem("GBHmrcExchangeRateYearlyRelativeUrlWithoutYear", () =>
				{
					return new StringRegistryItem(
						"GBHmrcExchangeRateYearlyRelativeUrlWithoutYear",
						Categories.Customs_UnitedKingdom_ExchangeRates,
						(NoResString)"Exchange rates relative URL",
						(NoResString)"Relative URL of HMRC exchange rates yearly listing page, with the year suffix as a {0} param. CW.",
						RegistryStorageFlags.System,
						"/government/publications/hmrc-exchange-rates-for-{0}-monthly");
				});
			}
		}

		public StringRegistryItem HmrcExchangeRateBaseUrlHost
		{
			get
			{
				return GetItem("GBHmrcExchangeRateBaseUrlHost", () =>
				{
					return new StringRegistryItem(
						"GBHmrcExchangeRateBaseUrlHost",
						Categories.Customs_UnitedKingdom_ExchangeRates,
						(NoResString)"Exchange rates host of URL",
						(NoResString)"Host of URL of HMRC exchange rates pages.",
						RegistryStorageFlags.System,
						"https://www.gov.uk");
				});
			}
		}

		public StringRegistryItem HmrcExchangeRateRegularExpressionHmrc
		{
			get
			{
				return GetItem("GBHmrcExchangeRateRegularExpression", () =>
				{
					// ********** Don't touch the regex, man. *******			
					string hmrcExchangeRateRegularExpressionPattern = @"<tr>\s*<td valign='top'><p>(?<COUNTRY>.*?)</p><br >\s*</td>\s*<td valign='top'><p>(?<CURRENCY>.*?)</p><br >\s*</td>\s*<td valign='top'><p>(?<CURRENTRATE>.*?)</p><br >\s*</td>\s*<td valign='top'>(?<DATEOFCHANGE>.*?)\s*</td>\s*<td valign='top'>(?<NEWRATE>.*?)\s*</td></tr>".Replace("'", "\"");
					return new StringRegistryItem(
						"GBHmrcExchangeRateRegularExpression",
						Categories.Customs_UnitedKingdom_ExchangeRates,
						(NoResString)"Regular Expression (HMRC.gov.uk)",
						(NoResString)"Regular Expression pattern for extracting rate data from HMRC HTML web pages. Allows synchronisation of the pattern to accommodate small ad-hoc changes to the format of the HMRC web page. Do not edit without instruction from CargoWise.",
						RegistryStorageFlags.System,
						hmrcExchangeRateRegularExpressionPattern);
				});
			}
		}

		public StringRegistryItem HmrcExchangeRateRegularExpressionGov
		{
			get
			{
				return GetItem("GBHmrcExchangeRateRegularExpressionGov", () =>
				{
					// ********** Don't touch the regex, man. *******			
					string govExchangeRateRegularExpressionPattern = @"<a href='(?<URL>.*?/exrates(-monthly)?-(?<MONTH>\d\d)(?<YEAR>\d\d(\d\d)?).*?.csv)'>".Replace("'", "\"");
					return new StringRegistryItem(
						"GBHmrcExchangeRateRegularExpressionGov",
						Categories.Customs_UnitedKingdom_ExchangeRates,
						(NoResString)"Regular Expression (Gov.uk)",
						(NoResString)"Regular Expression pattern for extracting monthly page URLs from Gov.uk HTML web pages. Allows synchronisation of the pattern to accommodate small ad-hoc changes to the format of the HMRC web page. Do not edit without instruction from CargoWise.",
						RegistryStorageFlags.System,
						govExchangeRateRegularExpressionPattern);
				});
			}
		}

		#endregion

		public BooleanRegistryItem ExcludeSuspendedAndWaivedFeesFromCalculations =>
			GetItem("GBExcludeSuspendedAndWaivedFeesFromCalculations", () =>
			new BooleanRegistryItem("GBExcludeSuspendedAndWaivedFeesFromCalculations",
				Categories.Customs_UnitedKingdom,
				(NoResString)"Exclude Suspended and Waived Fees from Calculations",
				(NoResString)"Exclude suspended and waived fees from duty and VAT calculations",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				defaultValue: true));

		public StringRegistryItem EDocsParentNameMacro
		{
			get
			{
				return GetItem("GBEDocsParentNameMacro", () =>
				{
					return new StringRegistryItem(
						"GBEDocsParentNameMacro",
						Categories.Customs_UnitedKingdom_PrinterseDocs,
						(NoResString)"Macro for message description",
						(NoResString)"The search-and-replace macro to describe messages when used as eDoc parents. Available macros are <NUMBER> (message serial number), <REFERENCE> (application reference), <TYPE> (message type code), <CODE> (application code) and <SUBTYPE> (message sub-type code).",
						RegistryStorageFlags.System,
						"<TYPE><SUBTYPE> <REFERENCE> #<NUMBER>");
				});
			}
		}

		public BooleanRegistryItem ForceOldWebCredentialsToBeTestedBeforeUse
		{
			get
			{
				return GetItem("GBForceOldWebCredentialsToBeTestedBeforeUse", () =>
				{
					return new BooleanRegistryItem(
						"GBForceOldWebCredentialsToBeTestedBeforeUse",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Force testing of old credentials",
						(NoResString)"Set this to true to inhibit the use of old CSP web credentials that have not explicitly been tested. Enabling this will mean that no messages can be sent or polled using a set of credentials until those credentials have been manually tested. This setting has no effect on the requirement to validate new or newly-edited data, only on old pre-existing data.",
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem ShouldResetCDSCalculatedEntryLineFeesAtMerger
		{
			get
			{
				return GetItem("GBShouldResetCDSCalculatedEntryLineFeesAtMerger", () =>
				{
					return new BooleanRegistryItem(
						"GBShouldResetCDSCalculatedEntryLineFeesAtMerger",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Should reset CDS calculated entry line fees at merger",
						(NoResString)"Should reset CDS calculated entry line fees at merger.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public enum EDocsParentNameMacroNames
		{
			NUMBER,
			REFERENCE,
			TYPE,
			SUBTYPE,
			CODE
		}

		#region PENTANT

		public IntRegistryItem PentantSleepTimeInSeconds
		{
			get
			{
				return GetItem("GBPentantSleepTimeInSeconds", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
									"GBPentantSleepTimeInSeconds",
									Categories.Customs_UnitedKingdom_ServiceProviders_Pentant,
									(NoResString)"Sleep period",
									(NoResString)"How long should CW1 sleep between failing FTP operations. CW1 will retry 5 times before aborting Number of seconds.",
									RegistryStorageFlags.System,
									15);
					return result;
				});
			}
		}

		public DateTimeRegistryItem PentantNudgedDateTime
		{
			get
			{
				return GetItem("GBPentantNudgedDateTime", () =>
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
									"GBPentantNudgedDateTime",
									Categories.Customs_UnitedKingdom_ServiceProviders_Pentant,
									(NoResString)"HIDDEN - nudge PENTANT downloader service task at this time",
									(NoResString)"HIDDEN - nudge PENTANT downloader service task at this time",
									RegistryStorageFlags.System,
									RegistryOptions.IsHidden,
									DateTime.MinValue);
					return result;
				});
			}
		}

		public IntRegistryItem PentantWebServiceLockoutThreshold
		{
			get
			{
				return GetItem("GBPentantWebServiceLockoutThreshold", () =>
				{
					IntRegistryItem result = new IntRegistryItem(
									"GBPentantWebServiceLockoutThreshold",
									Categories.Customs_UnitedKingdom_ServiceProviders_Pentant,
									(NoResString)"Anti-lockout threshold",
									(NoResString)"Anti-lockout threshold. The number of failed login attempts before shutting down Pentant FTP service activity to prevent lockout. Set to -1 to never lock down.",
									RegistryStorageFlags.System,
									RegistryOptions.IsOnlyEditableBySupportIfHosted,
									30);  // Should be more than 10
					return result;
				});
			}
		}

		public string GetPentantFtpEndpoint(CredentialsSetting credential)
		{
			var endPoint = PentantFtpEndpoint_Secure_Test.Value;

			if (!IsInTestMode.Value)
			{
				if (credential == null)
				{
					endPoint = string.Empty;
				}
				else
				{
					endPoint = credential.Endpoint == EndpointList.Codes.INV ? PentantFtpEndpoint_Live_Secure_Inv.Value : PentantFtpEndpoint_Live_Secure_CusLink.Value;
				}
			}

			return endPoint;
		}

#if DEBUG
		public string GetPentantFtpEndpoint_Live_Inv() => PentantFtpEndpoint_Live_Inv.Value;

		public string GetPentantFtpEndpoint_Live_Secure_Inv() => PentantFtpEndpoint_Live_Secure_Inv.Value;
#endif

		internal StringRegistryItem PentantFtpEndpoint_Live_Inv
		{
			get
			{
				return GetItem("PentantFtpEndpoint_Live_Inv", () =>
				{
					return new StringRegistryItem(
						"PentantFtpEndpoint_Live_Inv",
						Categories.Customs_UnitedKingdom_ServiceProviders_Pentant,
						(NoResString)"URL of Pentant inventory FTP service (live)",
						(NoResString)"URL of Pentant inventory FTP service (live)",
						RegistryStorageFlags.System,
						"ftp://inventory.pentant.co.uk");
				});
			}
		}

		internal StringRegistryItem PentantFtpEndpoint_Live_Secure_Inv
		{
			get
			{
				return GetItem("PentantFtpEndpoint_Live_Secure_Inv", () =>
				{
					return new StringRegistryItem(
						"PentantFtpEndpoint_Live_Secure_Inv",
						Categories.Customs_UnitedKingdom_ServiceProviders_Pentant,
						(NoResString)"URL of Pentant inventory FTP secure service (live)",
						(NoResString)"URL of Pentant inventory FTP secure service (live)",
						RegistryStorageFlags.System,
						"ftpes://inventory.pentant.co.uk");
				});
			}
		}

		internal string GetPentantFtpEndpoint_Live_CusLink() => PentantFtpEndpoint_Live_CusLink.Value;

		internal string GetPentantFtpEndpoint_Live_Secure_CusLink() => PentantFtpEndpoint_Live_Secure_CusLink.Value;

		internal StringRegistryItem PentantFtpEndpoint_Live_CusLink
		{
			get
			{
				return GetItem("PentantFtpEndpoint_Live_CusLink", () =>
				{
					return new StringRegistryItem(
						"PentantFtpEndpoint_Live_CusLink",
						Categories.Customs_UnitedKingdom_ServiceProviders_Pentant,
						(NoResString)"URL of Pentant CusLink FTP service (live)",
						(NoResString)"URL of Pentant CusLink FTP service (live)",
						RegistryStorageFlags.System,
						"ftp://cuslink.pentant.co.uk");
				});
			}
		}

		internal StringRegistryItem PentantFtpEndpoint_Live_Secure_CusLink
		{
			get
			{
				return GetItem("PentantFtpEndpoint_Live_Secure_CusLink", () =>
				{
					return new StringRegistryItem(
						"PentantFtpEndpoint_Live_Secure_CusLink",
						Categories.Customs_UnitedKingdom_ServiceProviders_Pentant,
						(NoResString)"URL of Pentant CusLink FTP secure service (live)",
						(NoResString)"URL of Pentant CusLink FTP secure service (live)",
						RegistryStorageFlags.System,
						"ftpes://cuslink.pentant.co.uk");
				});
			}
		}

		internal void SetPentantTestEndPoint(string value)
		{
			PentantFtpEndpoint_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		internal void SetPentantSecureTestEndPoint(string value)
		{
			PentantFtpEndpoint_Secure_Test.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		internal StringRegistryItem PentantFtpEndpoint_Test
		{
			get
			{
				return GetItem("PentantFtpEndpoint_Test", () =>
				{
					return new StringRegistryItem(
						"PentantFtpEndpoint_Test",
						Categories.Customs_UnitedKingdom_ServiceProviders_Pentant,
						(NoResString)"URL of Pentant FTP services (test)",
						(NoResString)"URL of Pentant FTP service (test)",
						RegistryStorageFlags.System,
						"ftp://development.pentant.co.uk");
				});
			}
		}

		internal StringRegistryItem PentantFtpEndpoint_Secure_Test
		{
			get
			{
				return GetItem("PentantFtpEndpoint_Secure_Test", () =>
				{
					return new StringRegistryItem(
						"PentantFtpEndpoint_Secure_Test",
						Categories.Customs_UnitedKingdom_ServiceProviders_Pentant,
						(NoResString)"URL of Pentant FTP secure services (test)",
						(NoResString)"URL of Pentant FTP secure service (test)",
						RegistryStorageFlags.System,
						"ftpes://development.pentant.co.uk");
				});
			}
		}

		#endregion

		#region CDS

		public BillCustomisationRegistryItem CdsEntryLocalReferenceNumberCustomisationFromCW1
		{
			get
			{
				return GetItem("CdsEntryLocalReferenceNumberCustomisationFromCW1", () => new BillCustomisationRegistryItem(
					"CdsEntryLocalReferenceNumberCustomisationFromCW1",
					Categories.Customs_UnitedKingdom,
					ResString.GetMultilingualString("A04D52B8-FC06-4BAC-9C0B-83E2190596DB", "CDS Entry Local Reference Number Customization"),
					ResString.GetMultilingualString("77F73A96-7A62-460A-9F7A-F0F2101C5184", "Override this value to customize how CDS Entry Local Reference Numbers are formatted"),
					RegistryStorageFlags.All,
					new CDSEntryLocalReferenceNumberCustomisationRegistryDataType()
				));
			}
		}

		public const int CDSEntryLocalReferenceNumberMaxLength = 22;

		public StringRegistryItem CdsCcsukRecipientIdExport
		{
			get
			{
				return GetItem("GBCdsCcsukRecipientIdExport", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCdsCcsukRecipientIdExport",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"Recipient for CDS export",
						(NoResString)"Set this to be the Recipient for CDS export.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						CDSCcsukRecipient);
					return result;
				});
			}
		}

		public StringRegistryItem CdsCcsukRecipientIdImport
		{
			get
			{
				return GetItem("GBCdsCcsukRecipientIdImport", () =>
				{
					StringRegistryItem result = new StringRegistryItem(
						"GBCdsCcsukRecipientIdImport",
						Categories.Customs_UnitedKingdom_ServiceProviders_CCSUK,
						(NoResString)"Recipient for CDS import",
						(NoResString)"Set this to be the Recipient for CDS import.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.IsOnlyForDevelopers,
						CDSCcsukRecipient);
					return result;
				});
			}
		}

		public string GetCdsCcsukProcessingInstruction(bool isImport, ZString pimaForSenderId, ZString extCorrelationId)
		{
			return string.Format(CultureInfo.InvariantCulture, @"<?ccsuk senderid=""{0}"" recipientid=""{1}"" ext-correlation-id=""{2}""?>", pimaForSenderId, isImport ? CdsCcsukRecipientIdImport.Value : CdsCcsukRecipientIdExport.Value, extCorrelationId);
		}

		public IRegistryItem NotificationCDS
		{
			get
			{
				ZString subItem = "CDS";
				ZString humanName = "CDS Notifications";
				IRegistryItem parentNotificationsItem = CustomsResponseNotificationsToGroupItem;

				var identifyingKey = parentNotificationsItem.Name + subItem;
				var parentNotificationsItemMultilingual = (IMultilingualRegistryItem)parentNotificationsItem;

				return GetItem(identifyingKey, () =>
				{
					return new RegistryItemImplWithDynamicDefaultValue(
						identifyingKey,
						new[] { CombineCategories(parentNotificationsItemMultilingual.CategoryMultilingual, parentNotificationsItemMultilingual.CaptionMultilingual) },
						(NoResString)(humanName),
						(NoResString)("Group for " + humanName),
						RegistryDataTypes.GuidType,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(companyPK, branchPK, departmentPK) => parentNotificationsItem.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK));
				});
			}
		}

		public BooleanRegistryItem CDSPilotMode
		{
			get
			{
				return GetItem("CDSAuthorisationUsesTestEHub", () =>
				{
					var result = new BooleanRegistryItem(
						"CDSAuthorisationUsesTestEHub",
						Categories.Customs_UnitedKingdom,
						(NoResString)"CDS authorisation uses test eHub",
						(NoResString)"CDS authorisation uses test eHub?",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						false);
					return result;
				});
			}
		}

		public BooleanRegistryItem SendNetMassForH2Declarations
		{
			get
			{
				return GetItem("SendNetMassForH2Declarations", () =>
				{
					var result = new BooleanRegistryItem(
						"SendNetMassForH2Declarations",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Send mass (6/1) for H2 declarations?",
						(NoResString)"Send net mass data element 6/1 for H2 CDS declarations.",
						RegistryStorageFlags.System,
						true);
					return result;
				});
			}
		}

		public BooleanRegistryItem SendNetMassForH3Declarations
		{
			get
			{
				return GetItem("SendNetMassForH3Declarations", () =>
				{
					var result = new BooleanRegistryItem(
						"SendNetMassForH3Declarations",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Send mass (6/1) for H3 declarations?",
						(NoResString)"Send net mass data element 6/1 for H3 CDS declarations.",
						RegistryStorageFlags.System,
						true);
					return result;
				});
			}
		}

		public BooleanRegistryItem SendDTNTaxBaseToCDS
		{
			get
			{
				return GetItem("SendDTNTaxBaseToCDS", () =>
				{
					var result = new BooleanRegistryItem(
						"SendDTNTaxBaseToCDS",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Send convertible mass tax bases to CDS?",
						(NoResString)"Set to YES to send DTN, TNE and other convertible mass tax bases to CDS.",
						RegistryStorageFlags.System,
						false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CDSEnabledForExports
		{
			get
			{
				return GetItem("CDSEnabledForExports", () =>
				{
					var result = new BooleanRegistryItem(
						"CDSEnabledForExports",
						Categories.Customs_UnitedKingdom,
						(NoResString)"CDS is enabled for exports",
						(NoResString)"CDS is enabled for exports?",
						RegistryStorageFlags.Branch,
						false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CDSEnabledForImports
		{
			get
			{
				return GetItem("CDSEnabledForImports", () =>
				{
					var result = new BooleanRegistryItem(
						"CDSEnabledForImports",
						Categories.Customs_UnitedKingdom,
						(NoResString)"CDS is enabled for imports",
						(NoResString)"CDS is enabled for imports?",
						RegistryStorageFlags.Branch,
						false);
					return result;
				});
			}
		}

		public BooleanRegistryItem CDSEnableEXRRAutomationForArrivedROROExports
		{
			get
			{
				return GetItem("CDSEnableEXRRAutomationForArrivedROROExports", () =>
				{
					return new BooleanRegistryItem(
						"CDSEnableEXRRAutomationForArrivedROROExports",
						Categories.Customs_UnitedKingdom,
						(NoResString)"CDS Enable EXRR automation for arrived RORO exports",
						(NoResString)"Automate the creation of an EXRR authorization for arrived RORO exports?",
						RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem CDSEnableAutoConvertExciseUnits
		{
			get
			{
				return GetItem("CDSEnableAutoConvertExciseUnits", () =>
				{
					return new BooleanRegistryItem(
						"CDSEnableAutoConvertExciseUnits",
						Categories.Customs_UnitedKingdom,
						(NoResString)"CDS Enable Automatic Conversion of Excise Units",
						(NoResString)"Enable automatic conversion of excise units for CDS?",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region ICS

		public BooleanRegistryItem EnableIcsManifest
		{
			get
			{
				return GetItem("EnableIcsManifest", () =>
				{
					var result = new BooleanRegistryItem(
						"EnableIcsManifest",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Enable Northern Ireland ICS Manifest",
						(NoResString)"Enable Northern Ireland ICS Manifest?",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		IRegistryItem Integration.Customs.GB.IGBCustomsDataRegistry.EnableIcsManifest => EnableIcsManifest;

		public BooleanRegistryItem EnableSSGBManifest
		{
			get
			{
				return GetItem("EnableSSGBManifest", () =>
				{
					var result = new BooleanRegistryItem(
						"EnableSSGBManifest",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Enable S&S GB Manifest",
						(NoResString)"Enable S&S GB Manifest?",
						RegistryStorageFlags.System,
						true);
					return result;
				});
			}
		}

		IRegistryItem Integration.Customs.GB.IGBCustomsDataRegistry.EnableSSGBManifest => EnableSSGBManifest;

		#endregion

		public BooleanRegistryItem CDSSuppressSendingASVXFor3XXTaxTypes
		{
			get
			{
				return GetItem("CDSSuppressSendingASVXFor3XXTaxTypes", () =>
				{
					var result = new BooleanRegistryItem(
						"CDSSuppressSendingASVXFor3XXTaxTypes",
						Categories.Customs_UnitedKingdom,
						(NoResString)"Suppress transmission of ASVX unit to CDS for 3xx tax types",
						(NoResString)"Suppress transmission of ASVX unit to CDS while awaiting fix for incorrect CDS behaviour, for 3xx tax types?",
						RegistryStorageFlags.Branch,
						defaultValue: true);
					return result;
				});
			}
		}

		#region CDS Email Notification Groups

		public IRegistryItem NotificationCDSPositiveReplies
		{
			get { return GetNotificationItem("PositiveReplies", "CDS Positive Replies", NotificationCDS); }
		}

		public IRegistryItem NotificationCDSRejections
		{
			get { return GetNotificationItem("Rejections", "CDS Rejections (REJ)", NotificationCDS); }
		}

		public IRegistryItem NotificationCDSUnsolicitedUpdates
		{
			get { return GetNotificationItem("UnsolicitedUpdates", "CDS Unsolicited Updates", NotificationCDS); }
		}

		public IRegistryItem NotificationCDSACC
		{
			get { return GetNotificationItem("ACC", "ACC", NotificationCDSPositiveReplies); }
		}

		public IRegistryItem NotificationCDSRCV
		{
			get { return GetNotificationItem("RCV", "RCV", NotificationCDSPositiveReplies); }
		}

		public IRegistryItem NotificationCDSINC
		{
			get { return GetNotificationItem("INC", "INC", NotificationCDSUnsolicitedUpdates); }
		}

		public IRegistryItem NotificationCDSCTL
		{
			get { return GetNotificationItem("CTL", "CTL", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSDOC
		{
			get { return GetNotificationItem("DOC", "DOC", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSRES
		{
			get { return GetNotificationItem("RES", "RES", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSROG
		{
			get { return GetNotificationItem("ROG", "ROG", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSCLE
		{
			get { return GetNotificationItem("CLE", "CLE", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSINV
		{
			get { return GetNotificationItem("INV", "INV", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSREQ
		{
			get { return GetNotificationItem("REQ", "REQ", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSTAX
		{
			get { return GetNotificationItem("TAX", "TAX", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSCPI
		{
			get { return GetNotificationItem("CPI", "CPI", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSCPR
		{
			get { return GetNotificationItem("CPR", "CPR", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSEOG
		{
			get { return GetNotificationItem("EOG", "EOG", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSEXT
		{
			get { return GetNotificationItem("EXT", "EXT", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSGER
		{
			get { return GetNotificationItem("GER", "GER", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSALV
		{
			get { return GetNotificationItem("ALV", "ALV", NotificationCDSUnsolicitedUpdates); }
		}
		public IRegistryItem NotificationCDSQRY
		{
			get { return GetNotificationItem("QRY", "QRY", NotificationCDSUnsolicitedUpdates); }
		}

		#endregion CDS Email Notification Groups

		#region Implementation

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.UnitedKingdom;
		}
		#endregion

		const string CDSCcsukRecipient = "CUKCTM98CDSUSR";
	}
}
