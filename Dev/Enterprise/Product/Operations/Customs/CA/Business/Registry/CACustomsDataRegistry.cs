using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

using ICACustomsDataRegistry = Enterprise.Integration.Customs.CA.ICACustomsDataRegistry;

namespace Enterprise.Customs.CA.Registry
{
	public sealed class CACustomsDataRegistry : RegistryItemSet, ICACustomsDataRegistry
	{
		#region Construction
		public static CACustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new CACustomsDataRegistry()); }
		}
		[ThreadStatic]
		static CACustomsDataRegistry instance;

		CACustomsDataRegistry()
		{
		}
		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Canada { get { return CombineCategories(Customs_CountryOrRegion, Business.ResString.GetMultilingualString("4208acf5-7d1f-446d-9e49-57569259e46f", "Canada")); } }
			public static MultilingualString Customs_Canada_AuditActions { get { return CombineCategories(Customs_Canada, Business.ResString.GetMultilingualString("9421244e-4423-42dc-8f86-a2fe55db4154", "Audit Actions")); } }
			public static MultilingualString Customs_Canada_Export { get { return CombineCategories(Customs_Canada, Business.ResString.GetMultilingualString("e6b0bb37-0c68-4f46-bd99-98c329d12d56", "Export")); } }
			public static MultilingualString Customs_Canada_Export_DataLoadingModule { get { return CombineCategories(Customs_Canada_Export, Business.ResString.GetMultilingualString("f26502e8-b97a-4536-989b-220e01bdde6c", "Data Loading Module")); } }
			public static MultilingualString Customs_Canada_Import { get { return CombineCategories(Customs_Canada, Business.ResString.GetMultilingualString("bed4db6c-943f-4e3e-b845-aef5b9aeb257", "Import")); } }
			public static MultilingualString Customs_Canada_Import_ACI { get { return CombineCategories(Customs_Canada_Import, Business.ResString.GetMultilingualString("962a570d-8151-47a6-9235-f7132cc4e14e", "ACI")); } }
			public static MultilingualString Customs_Canada_Import_CSA { get { return CombineCategories(Customs_Canada_Import, Business.ResString.GetMultilingualString("358C3922-7592-4E5B-B425-F18D8A4FD94E", "CSA")); } }
			public static MultilingualString Customs_Canada_Import_eManifest { get { return CombineCategories(Customs_Canada_Import, Business.ResString.GetMultilingualString("19b2e122-4986-4a49-a3a8-d0c35eba541d", "eManifest|RNS Notifications")); } }
			public static MultilingualString Customs_Canada_Import_Declaration { get { return CombineCategories(Customs_Canada_Import, Business.ResString.GetMultilingualString("4223d1a8-be44-44e7-8357-90fe7a06da0c", "Declaration")); } }
			public static MultilingualString Customs_Canada_Import_Declaration_Defaults { get { return CombineCategories(Customs_Canada_Import_Declaration, Business.ResString.GetMultilingualString("f2f746ac-30fc-4bc1-8daf-ebfd7438840f", "Defaults")); } }
			public static MultilingualString Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy { get { return CombineCategories(Customs_Canada_Import_Declaration, Business.ResString.GetMultilingualString("857b9313-d0fa-40b0-b532-8916f80099bd", "Default LVS Consolidation Strategy")); } }
			public static MultilingualString Customs_Canada_Import_Declaration_EntrySendingOptions { get { return CombineCategories(Customs_Canada_Import_Declaration, Business.ResString.GetMultilingualString("AE2DF44B-A22D-4C7B-8507-93540E1A5A63", "Entry Sending Options")); } }
			public static MultilingualString Customs_Canada_Import_Declaration_Notifications { get { return CombineCategories(Customs_Canada_Import_Declaration, Business.ResString.GetMultilingualString("CAED5E20-51F6-4D76-B12C-76B43B6372B7", "Notifications")); } }
			public static MultilingualString Customs_Canada_Import_Declaration_CFIA { get { return CombineCategories(Customs_Canada_Import_Declaration, Business.ResString.GetMultilingualString("aa991174-212d-4abc-9d68-f478123acd44", "CFIA")); } }
			public static MultilingualString Customs_Canada_Import_Declaration_QualityControl { get { return CombineCategories(Customs_Canada_Import_Declaration, Business.ResString.GetMultilingualString("263387f5-3f49-40ec-95f4-a6c0e27d6077", "Quality Control")); } }
			public static MultilingualString Customs_Canada_Import_Declaration_CARM { get { return CombineCategories(Customs_Canada_Import_Declaration, Business.ResString.GetMultilingualString("0A580908-8558-4955-83F9-9D698FEC01B6", "CARM")); } }
			public static MultilingualString Customs_Canada_Import_RNS { get { return CombineCategories(Customs_Canada_Import, Business.ResString.GetMultilingualString("d43d9c03-8df3-4f3e-930b-78ad9d2e7a29", "RNS")); } }
			public static MultilingualString Customs_Canada_ServiceTasks { get { return CombineCategories(Customs_Canada, Business.ResString.GetMultilingualString("881622f0-27b7-4d60-b8cd-ad624055bc7a", "Service Tasks")); } }
			public static MultilingualString AutoRating_ChargeCodes_Customs_Canada { get { return CombineCategories(RatingDataRegistry.Categories.AutoRating_ChargeCodes_Customs, Business.ResString.GetMultilingualString("374861B1-3C55-40A9-9958-CAEB9B85CDD3", "Canada")); } }
			public static MultilingualString Customs_Canada_TestingDevelopment { get { return CombineCategories(Customs_Canada, Business.ResString.GetMultilingualString("663345ac-4295-4d59-b255-0e80ff634af3", "Testing and Development")); } }
		}

		#endregion

		#region DataLoadingModuleCategory

		public StringRegistryItem DataLoadingModuleOutputDirectory
		{
			get
			{
				return GetItem("DataLoadingModuleOutputDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"DataLoadingModuleOutputDirectory",
						Categories.Customs_Canada_Export_DataLoadingModule,
						Business.ResString.GetMultilingualString("20ac8985-c149-4e93-b5cf-1fdbf5e8ce9c", "Output Directory"),
						Business.ResString.GetMultilingualString("dfed0a8a-e4c9-48f1-99af-93a4275dd08b", "Enter the storage location for export messages to be sent to Customs"),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public StringRegistryItem InBondTermsAndConditions
		{
			get
			{
				return GetItem("CAInBondDSVTermsAndConditions", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"CAInBondDSVTermsAndConditions",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("52d6503d-6f03-428a-9434-5a2e9c4a2344", "In Bond Terms & Conditions"),
						Business.ResString.GetMultilingualString("35afb094-f57e-48da-828d-6a49c25415ce", "Enter the Terms & Conditions that will appear on the In Bond Document"),
						RegistryStorageFlags.Company,
						InBondDefaultTermsAndConditions);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		internal const string InBondDefaultTermsAndConditions = "All business conducted is accepted and handled subject to the Standard Trading Conditions adopted by the Canadian International Freight Forwarders Association.  Copies available upon request.  /  Toutes les activities exercees sont assujetties aux conditions generales adoptees par l'Association des transitaires international canadiens.  Copies disponibles sur demandes.\r\n\r\nAcceptance of advice note for customs clearance and release of goods constitutes your acceptance of liability for charges and storage, if any.  This shipment is presently on hand and held at risk & expense of owner, or owner's agent.  Storage charges additional for account of owner or owners agent if not cleared customs and delivered within free time.  /  Votre acceptance de l'avis pour fins de douanement et relachement des mdses, constitues votre acceptance de responsable des freas et tout entreposage.  Cette expedition etes presentement retenue endouane proprietaire ou son agent.  Frais d'entreport additionels au comptent proprietaire ou son agent si les mdses ne sont pas dedouanees et livres dans la periode gratuite.";

		#endregion

		#region CACustomsCategory

		public StringRegistryItem WTGBusinessNumber
		{
			get
			{
				return GetItem("WTGBusinessNumber", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"WTGBusinessNumber",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("985582D4-EEBE-4BAB-BF4D-3E22F3D438F1", "WTG Business Number"),
						Business.ResString.GetMultilingualString("0EAF8C90-51FC-41BF-ACD9-2C73E754772B", "WTG Business Number"),
						new StringRegistryDataType(CharacterCase.Upper),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						"822066668RM0002");
					return result;
				});
			}
		}

		public StringRegistryItem MailBoxIDAppliesAllCountries
		{
			get
			{
				return GetItem("MailBoxIDAppliesAllCountries", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"MailBoxIDAppliesAllCountries",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("B5E7DC8A-E45E-463E-A1D7-87633C0428D6", "Network / Mailbox ID"),
						Business.ResString.GetMultilingualString("fb2216e8-3c08-4712-b5a6-c42700aa90f6", "Enter the Client Network ID allocated to you by CargoWise for production messages."),
						new StringRegistryDataType(CharacterCase.Upper),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue);
					return result;
				});
			}
		}

		public BooleanRegistryItem DoNotUseZipPasswordProtection
		{
			get
			{
				return GetItem("DoNotUseZipPasswordProtection", delegate
				{
					return new BooleanRegistryItem(
						"DoNotUseZipPasswordProtection",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("6ad0c1a1-7cf2-4667-87cd-16bd544ee81e", "Do Not Use Zip Password Protection"),
						Business.ResString.GetMultilingualString("2fbcd7be-e75c-4200-934c-2b1be2801bfd", "Turning this on will inhibit the use of password protection on files transferred between Clients and the Messaging Server. This is for test use only because PC Plus will not 'white list' our account."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public StringRegistryItem SecurityKeyAppliesAllCountries
		{
			get
			{
				return GetItem("SecurityKeyAppliesAllCountries", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"SecurityKeyAppliesAllCountries",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("e07c5418-e780-41d2-bfc4-8bb1c4882d6a", "Security Key"),
						Business.ResString.GetMultilingualString("a8d34b11-c288-422b-91c8-cd2defb02705", "Security Key."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					return result;
				});
			}
		}

		public StringRegistryItem CBSATestNetworkIDAppliesAllCountries
		{
			get
			{
				return GetItem("CBSATestNetworkIDAppliesAllCountries", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"CBSATestNetworkIDAppliesAllCountries",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("fb80d72d-3742-4fe6-8b3c-3ca25b70e797", "CBSA Test Network ID"),
						Business.ResString.GetMultilingualString("a6721cf6-aade-4538-b744-b35779f360e5", "Enter the CBSA's Network ID for test system access"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"RCCECECPW");
					return result;
				});
			}
		}

		public StringRegistryItem CBSAProdNetworkIDAppliesAllCountries
		{
			get
			{
				return GetItem("CBSAProdNetworkIDAppliesAllCountries", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"CBSAProdNetworkIDAppliesAllCountries",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("070123EC-AA9A-404E-946F-1EDC7073D0F1", "CBSA Prod Network ID"),
						Business.ResString.GetMultilingualString("1724AFE5-33BA-45EA-8F3D-4DF6E72868CD", "Enter the CBSA's Network ID for production system access"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"RCCECECPW");
					return result;
				});
			}
		}

		public StringRegistryItem ControlOfficeAppliesAllCountries
		{
			get
			{
				return GetItem("ControlOfficeAppliesAllCountries", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"ControlOfficeAppliesAllCountries",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("fde4dde5-a3fe-4dfe-8cc6-7940a7e8a178", "Control Office"),
						Business.ResString.GetMultilingualString("06fb364d-120e-4a4e-b0d2-57457675199c", "Enter the Control Office"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					return result;
				});
			}
		}

		public StringRegistryItem TransmissionSite
		{
			get
			{
				return GetItem("TransmissionSite", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"TransmissionSite",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("bbe497ac-e942-4113-8721-541fe413f489", "Transmission Site"),
						Business.ResString.GetMultilingualString("34838e93-77fc-44b3-b624-c51110b3eae5", "Enter the Transmission Site"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						"U10207V2");
					return result;
				});
			}
		}

		public IntRegistryItem CCNReuseTimeSetting
		{
			get
			{
				return GetItem("CCNReuseTimeSetting", delegate
				{
					return new IntRegistryItem(
						"CCNReuseTimeSetting",
						Categories.Customs_Canada_Import,
						Business.ResString.GetMultilingualString("3F189F62-6AE0-4263-B31D-485D02AD33B0", "CCN Reuse Time Frame (years)"),
						Business.ResString.GetMultilingualString("491DC9AE-49E0-427A-8316-7A84776A6BC8", "CCN Reuse Time Frame (years)"),
						RegistryStorageFlags.System,
						3);
				});
			}
		}

		public BooleanRegistryItem FrenchLanguageIndicator
		{
			get
			{
				return GetItem("FrenchLanguageIndicator", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"FrenchLanguageIndicator",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("554f4e73-7038-4661-9fbc-d6c7f68794ae", "Is French Language Preferred?"),
						Business.ResString.GetMultilingualString("a5185f66-05ad-48cb-80cc-a1a39bf2aaa6", "Override if French is to be specified as the preferred language of communication with PGAs."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
					return result;
				});
			}
		}

		public BooleanRegistryItem MakeSomeFieldsJobDocAddress
		{
			get
			{
				return GetItem("MakeSomeFieldsJobDocAddress", delegate
				{
					return new BooleanRegistryItem(
						"MakeSomeFieldsJobDocAddress",
						Categories.Customs_Canada,
						Business.ResString.GetMultilingualString("D22CF51D-01CE-4ED9-A6DD-A209B4BA6C39", "Make Some Fields Job Doc Address"),
						Business.ResString.GetMultilingualString("389ABFC4-C573-4FFB-BB6D-9202E060AF4C", "Setting this value to 'Yes' will make Consignee, Delivery Party, Manufacturer Job Doc Address."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region CACustomsCategoryAuditActions

		public CodePairRegistryItem ReleaseHighValueProductAudit
		{
			get
			{
				return GetItem("ReleaseHighValueProductAudit", delegate
				{
					return new CodePairRegistryItem(
						"ReleaseHighValueProductAudit",
						Categories.Customs_Canada_AuditActions,
						Business.ResString.GetMultilingualString("eecff0e1-1be0-4a2e-acc3-0cd85ec5eef8", "Release High Value Product Audit"),
						Business.ResString.GetMultilingualString("ef096be6-2d11-45db-98de-5f13c9f162db", "Set the Product Audit Action for Release High Value."),
						new CodeDescriptionPairListProvider(() => new Enterprise.Registry.Business.Customs.ProductAuditActions()),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						ProductAuditActions.Codes.NoAction)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public CodePairRegistryItem ReleaseLowValueProductAudit
		{
			get
			{
				return GetItem("ReleaseLowValueProductAudit", delegate
				{
					return new CodePairRegistryItem(
						"ReleaseLowValueProductAudit",
						Categories.Customs_Canada_AuditActions,
						Business.ResString.GetMultilingualString("0ce045cb-0d81-4069-8553-bafeebe82a61", "Release Low Value Product Audit"),
						Business.ResString.GetMultilingualString("51cb0593-d72e-489e-ac55-032391861ea8", "Set the Product Audit Action for Release Low Value."),
						new CodeDescriptionPairListProvider(() => new Enterprise.Registry.Business.Customs.ProductAuditActions()),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						ProductAuditActions.Codes.NoAction)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public CodePairRegistryItem EntryHighValueProductAudit
		{
			get
			{
				return GetItem("EntryHighValueProductAudit", delegate
				{
					return new CodePairRegistryItem(
						"EntryHighValueProductAudit",
						Categories.Customs_Canada_AuditActions,
						Business.ResString.GetMultilingualString("53a128f2-82c9-464a-96db-7cefb7b1cb11", "Entry High Value Product Audit"),
						Business.ResString.GetMultilingualString("33018a64-ccd6-49a0-b9d7-c62ce2c92a3c", "Set the Product Audit Action for Entry High Value."),
						new CodeDescriptionPairListProvider(() => new Enterprise.Registry.Business.Customs.ProductAuditActions()),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						ProductAuditActions.Codes.NoAction)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public CodePairRegistryItem EntryLowValueProductAudit
		{
			get
			{
				return GetItem("EntryLowValueProductAudit", delegate
				{
					return new CodePairRegistryItem(
						"EntryLowValueProductAudit",
						Categories.Customs_Canada_AuditActions,
						Business.ResString.GetMultilingualString("e2044d82-3fd2-42f8-aaf3-7b49b27ccfa2", "Entry Low Value (including Consolidated LVS) Product Audit"),
						Business.ResString.GetMultilingualString("67c16de0-9aac-47b0-9724-9d54bde265cb", "Set the Product Audit Action for Entry Low Value (including Consolidated LVS)."),
						new CodeDescriptionPairListProvider(() => new Enterprise.Registry.Business.Customs.ProductAuditActions()),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						ProductAuditActions.Codes.NoAction)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		#endregion

		#region CACustomsCategoryExport

		public GuidRegistryItem DefaultServiceProviderOrganization
		{
			get
			{
				return GetItem("DefaultServiceProviderOrganization", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"DefaultServiceProviderOrganization",
						Categories.Customs_Canada_Export,
						Business.ResString.GetMultilingualString("A84562CD-D832-4E7E-AC66-0AE80C9322A0", "Default Service Provider Organization"),
						Business.ResString.GetMultilingualString("229C4A00-0B47-4103-A148-FC72CE7C1A43", "This organization will be used to default the Service Provider on G7 export jobs."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public StringRegistryItem DefaultPlaceOfReport
		{
			get
			{
				return GetItem("DataLoadingModuleDefaultPlaceOfReport", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"DataLoadingModuleDefaultPlaceOfReport",
						Categories.Customs_Canada_Export,
						Business.ResString.GetMultilingualString("7a6af8eb-0bad-4f8f-9d98-d0073b20f892", "Place Of Report"),
						Business.ResString.GetMultilingualString("c75ba403-d25a-4fd4-8dd1-f0ef84e297b1", "Enter the default Place Of Report"),
						RegistryStorageFlags.Branch);
					result.EditorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.Customs.Universal.ZZRefCusCodeList, GetCBSAOfficeCollection);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public BooleanRegistryItem ExportDeclarationActive
		{
			get
			{
				return GetItem("ExportDeclarationActive", delegate
				{
					return new BooleanRegistryItem(
						"ExportDeclarationActive",
						Categories.Customs_Canada_Export,
						Business.ResString.GetMultilingualString("7069008B-ECF2-4699-8A52-8973A087B304", "Should Export Declaration Functionality be activated?"),
						Business.ResString.GetMultilingualString("7069008B-ECF2-4699-8A52-8973A087B304", "Should Export Declaration Functionality be activated?"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						true)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem SendG7ExportMessages
		{
			get
			{
				return GetItem("SendG7ExportMessages", delegate
				{
					return new BooleanRegistryItem(
						"SendG7ExportMessages",
						Categories.Customs_Canada_Export,
						Business.ResString.GetMultilingualString("2d79e2fa-709b-4399-a9f4-13e0c596e0ea", "Send G7 Export Message"),
						Business.ResString.GetMultilingualString("6378a593-b370-4743-a456-33e185bf1f4e", "Should G7 Export Message be sent (if not then the CAED export message is sent)?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public CodePairRegistryItem SendEXPAcknowledgements
		{
			get
			{
				return GetItem("SendEXPAcknowledgements", delegate
				{
					return new CodePairRegistryItem(
						"SendEXPAcknowledgements",
						Categories.Customs_Canada_Export,
						Business.ResString.GetMultilingualString("4aae0476-f526-4f9d-95c0-dd84130d6ec6", "Send G7 Export Message Acknowledgements"),
						Business.ResString.GetMultilingualString("63a53858-d6ca-4036-b6de-b0a4b4146faf", "Send G7 Export message acknowledgements to staff member, nominated group or combination of both"),
						new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.EmailTo)),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public GuidRegistryItem SendEXPAcknowledgementsToGroup
		{
			get
			{
				return GetItem("SendEXPAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendEXPAcknowledgementsToGroup",
						Categories.Customs_Canada_Export,
						Business.ResString.GetMultilingualString("0b784524-b986-429a-92aa-40f4dd1bb907", "Send G7 Export message Acknowledgements To Group"),
						Business.ResString.GetMultilingualString("80046ad9-32be-4eef-b678-5935512097fa", "Send G7 Export message acknowledgements to selected group"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public CodePairRegistryItem SendEXPErrors
		{
			get
			{
				return GetItem("SendEXPErrors", delegate
				{
					return new CodePairRegistryItem(
						"SendEXPErrors",
						Categories.Customs_Canada_Export,
						Business.ResString.GetMultilingualString("80eceb3d-816f-4d7b-b7aa-309be85fb988", "Send G7 Export Message Errors"),
						Business.ResString.GetMultilingualString("abe6ed00-3de4-4655-8bdb-77152d55dd59", "Send G7 Export message errors to staff member, nominated group or combination of both"),
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public GuidRegistryItem SendEXPErrorsToGroup
		{
			get
			{
				return GetItem("SendEXPErrorsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendEXPErrorsToGroup",
						Categories.Customs_Canada_Export,
						Business.ResString.GetMultilingualString("487d3caa-f8f2-4499-95a1-a002f593359a", "Send G7 Export Message Errors To Group"),
						Business.ResString.GetMultilingualString("dd9bfc00-bbff-4edf-ad2c-e7b8a42ddd7a", "Send G7 Export message errors to selected group"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		#endregion

		#region CACustomsCategoryImport

		public BooleanRegistryItem ShouldDefaultCustomsCodes
		{
			get
			{
				return GetItem("ShouldDefaultCustomsCodes", delegate
				{
					return new BooleanRegistryItem(
						"ShouldDefaultCustomsCodes",
						Categories.Customs_Canada_Import,
						Business.ResString.GetMultilingualString("B7895806-0AEF-4EB0-9F20-0124FF3A24DB", "Default Customs Codes"),
						Business.ResString.GetMultilingualString("9898CD72-22F1-4305-B652-41AF5BE1FCCD", "Should Sub-Location and Customs Port of Clearance be defaulted from the Port of Discharge UNLOCO?"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, true);
				});
			}
		}

		public BooleanRegistryItem ConsolidateSOA
		{
			get
			{
				return GetItem("ConsolidateSOA", delegate
				{
					return new BooleanRegistryItem(
						"ConsolidateSOA",
						Categories.Customs_Canada_Import,
						Business.ResString.GetMultilingualString("36E9C3F2-8942-48FF-BCB2-319584FA6AC1", "Consolidate SOA"),
						Business.ResString.GetMultilingualString("F92F7F1D-7EB9-4BA3-B2CF-FC14210E519E", "Setting this value to 'Yes' will consolidate multi-part SOA message sent by CBSA into one ARL statement of Account in CW1."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public static bool ShouldConsolidateSOA => Instance.ConsolidateSOA.Value;

		#region CASuppressShipmentRelatedFields

		public BooleanRegistryItem SuppressShipmentRelatedFields
		{
			get
			{
				return GetItem("SuppressShipmentRelatedFields", delegate
				{
					return new BooleanRegistryItem(
						"SuppressShipmentRelatedFields",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("ab03508e-a345-41bd-adcc-48530ef2907d", "Suppress Shipment Related Fields for non synchronized declarations?"),
						Business.ResString.GetMultilingualString("ed4ba5f0-f11a-458e-a363-f9f636bc0a94", "If this item is set to YES, then forwarding / shipment related fields used for data synchronization will be suppressed and not visible.\r\n\r\nIf set to NO, then these fields will be visible by default in each new declaration."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch, true)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		#endregion

		#region CACustomsCategoryImportACI

		public StringRegistryItem SupplementaryNumberSuffixAppliesAllCountries
		{
			get
			{
				return GetItem("SupplementaryNumberSuffixAppliesAllCountries", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"SupplementaryNumberSuffixAppliesAllCountries",
						Categories.Customs_Canada_Import_ACI,
						Business.ResString.GetMultilingualString("3ddedfb6-da64-46d6-8bd4-1ac90e1b4c2a", "Supplementary Reference Number Suffix"),
						Business.ResString.GetMultilingualString("813bdb78-431c-48ff-bb5b-a1e8218af681", "This value will be added to the end of the generated ACI Supplementary Reference Number"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					return result;
				});
			}
		}

		public CodePairRegistryItem SendACIAcknowledgementsAppliesAllCountries
		{
			get
			{
				return GetItem("SendACIAcknowledgementsAppliesAllCountries", delegate
				{
					return new CodePairRegistryItem(
						"SendACIAcknowledgementsAppliesAllCountries",
						Categories.Customs_Canada_Import_ACI,
						Business.ResString.GetMultilingualString("4b242ead-faae-4371-8997-54fb3779b2d2", "Send ACI Message Acknowledgements"),
						Business.ResString.GetMultilingualString("72b39ca2-1904-4738-a5c8-7e68a43571b7", "Send ACI message acknowledgements to staff member, nominated group or combination of both"),
						new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.EmailTo)),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem SendACIAcknowledgementsToGroupAppliesAllCountries
		{
			get
			{
				return GetItem("SendACIAcknowledgementsToGroupAppliesAllCountries", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendACIAcknowledgementsToGroupAppliesAllCountries",
						Categories.Customs_Canada_Import_ACI,
						Business.ResString.GetMultilingualString("40bcc3d1-c16e-4b91-83f8-1a3e6a0d4998", "Send ACI Message Acknowledgements To Group"),
						Business.ResString.GetMultilingualString("7f7a0b8d-ce59-4400-aa55-7b9735c5475a", "Send ACI message acknowledgements to selected group"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem SendACIErrorsAppliesAllCountries
		{
			get
			{
				return GetItem("SendACIErrorsAppliesAllCountries", delegate
				{
					return new CodePairRegistryItem(
						"SendACIErrorsAppliesAllCountries",
						Categories.Customs_Canada_Import_ACI,
						Business.ResString.GetMultilingualString("6312ef39-e478-45eb-a5f3-fa6c2543f7e8", "Send ACI Message Errors"),
						Business.ResString.GetMultilingualString("03cb92be-9328-4cab-99dc-848fd0155a2e", "Send ACI message errors to staff member, nominated group or combination of both"),
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem SendACIErrorsToGroupAppliesAllCountries
		{
			get
			{
				return GetItem("SendACIErrorsToGroupAppliesAllCountries", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendACIErrorsToGroupAppliesAllCountries",
						Categories.Customs_Canada_Import_ACI,
						Business.ResString.GetMultilingualString("e10248cc-6e90-48bb-8355-610ab9a51471", "Send ACI Message Errors To Group"),
						Business.ResString.GetMultilingualString("919bf389-04da-4cd1-af30-adb1d1b0a6ff", "Send ACI message errors to selected group"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region CACustomsCategoryEManifestForwarder

		public BooleanRegistryItem SynchronizeBlindColoadMasterWithConsol
		{
			get
			{
				return GetItem("SynchronizeBlindColoadMasterWithConsol", delegate
				{
					return new BooleanRegistryItem(
						"SynchronizeBlindColoadMasterWithConsol",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("5A3765CD-BA88-4238-8758-D3235553612A", "Synchronize Blind Co-load Master with Consol"),
						Business.ResString.GetMultilingualString("BC699027-59D6-48D3-8087-FD8AE346FAE7", "Should synchronize Blind co-load masters with Consol?"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public CodePairRegistryItem SendeManifestForwarderMessageAcknowledgementsAppliesAllCountries
		{
			get
			{
				return GetItem("SendeManifestForwarderMessageAcknowledgementsAppliesAllCountries", delegate
				{
					return new CodePairRegistryItem(
						"SendeManifestForwarderMessageAcknowledgementsAppliesAllCountries",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("600AC303-6539-432B-A170-6C9E2FBF5F7D", "Send eManifest Forwarder Message Acknowledgements"),
						Business.ResString.GetMultilingualString("5666D4B7-3931-40CB-8F36-5389C3B8819E", "Send eManifest Forwarder message acknowledgements to staff member, nominated group or combination of both"),
						new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.EmailTo)),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem SendeManifestForwarderMessageAcknowledgementsToGroupAppliesAllCountries
		{
			get
			{
				return GetItem("SendeManifestForwarderMessageAcknowledgementsToGroupAppliesAllCountries", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendeManifestForwarderMessageAcknowledgementsToGroupAppliesAllCountries",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("F1F48024-B364-417D-A297-D95999087C19", "Send eManifest Forwarder Message Acknowledgements To Group"),
						Business.ResString.GetMultilingualString("F6AB754B-EEEC-4531-BFF7-7C932A0482D4", "Send eManifest Forwarder message acknowledgements to selected group"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem SendeManifestForwarderMessageErrorsAppliesAllCountries
		{
			get
			{
				return GetItem("SendeManifestForwarderMessageErrorsAppliesAllCountries", delegate
				{
					return new CodePairRegistryItem(
						"SendeManifestForwarderMessageErrorsAppliesAllCountries",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("47074E0A-A862-4D48-B2F1-EFE329245A51", "Send eManifest Forwarder Message Errors"),
						Business.ResString.GetMultilingualString("1878E55D-A6FF-49BB-AEEE-AC3899D21B70", "Send eManifest Forwarder message errors to staff member, nominated group or combination of both"),
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public BooleanRegistryItem SynchronizeAssemblyMasterwithLeadShipment
		{
			get
			{
				return GetItem("SynchronizeAssemblyMasterwithLeadShipment", delegate
				{
					return new BooleanRegistryItem(
						"SynchronizeAssemblyMasterwithLeadShipment",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("68C51147-D0B3-4104-BF2B-0412F493B7FE", "Synchronize Assembly Master with Lead Shipment"),
						Business.ResString.GetMultilingualString("3D2956E7-092B-4164-8B32-0B558A36FEDD", "If set to Yes, then when an Assembly Master Consolidation is synchronized with eManifest, only the lead shipment will be reported. If set to No, then the lead shipment will not be synchronized and instead, the sub-house bills will be synchronized for reporting the lowest level bills."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public GuidRegistryItem SendeManifestForwarderMessageErrorsToGroupAppliesAllCountries
		{
			get
			{
				return GetItem("SendeManifestForwarderMessageErrorsToGroupAppliesAllCountries", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendeManifestForwarderMessageErrorsToGroupAppliesAllCountries",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("FA085755-B1B7-4661-875E-A4AB0D1A293D", "Send eManifest Forwarder Message Errors To Group"),
						Business.ResString.GetMultilingualString("15CD5906-53D6-4E77-8C0B-8F6D48A81010", "Send eManifest Forwarder message errors to selected group"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem SendBrokerSNPMessageDetailsToGroupAppliesAllCountries
		{
			get
			{
				return GetItem("SendBrokerSNPMessageDetailsToGroupAppliesAllCountries", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendBrokerSNPMessageDetailsToGroupAppliesAllCountries",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("2A68BF14-0002-4E01-B116-15BCD5A382B6", "Send received Broker SNP information To Group"),
						Business.ResString.GetMultilingualString("39685B4D-A134-4034-A96A-4BF7FC4F1578", "Details contained in received Broker Secondary Notify Party messages will be emailed to this group of users."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem SendForwarderSNPMessageDetailsToGroupAppliesAllCountries
		{
			get
			{
				return GetItem("SendForwarderSNPMessageDetailsToGroupAppliesAllCountries", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendForwarderSNPMessageDetailsToGroupAppliesAllCountries",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("95C40492-90D2-435D-9F5B-949E0006B710", "Send received Forwarder SNP information To Group"),
						Business.ResString.GetMultilingualString("F18129CC-98C6-4E13-ADBD-31EB5949F974", "Details contained in received Forwarder Secondary Notify Party messages will be emailed to this group of users."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem SendCarrierSNPMessageDetailsToGroupAppliesAllCountries
		{
			get
			{
				return GetItem("SendCarrierSNPMessageDetailsToGroupAppliesAllCountries", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendCarrierSNPMessageDetailsToGroupAppliesAllCountries",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("47C5956E-D4E5-48E5-834C-A623074FB895", "Send received Carrier SNP information To Group"),
						Business.ResString.GetMultilingualString("133CB168-2E79-4E87-98BB-1478A2C2498B", "Details contained in received Carrier Secondary Notify Party messages will be emailed to this group of users."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries
		{
			get
			{
				return GetItem("SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("2CA8A5E2-F462-40ED-870A-02AE83D2E8DA", "Send received Warehouse SNP and RNS information to Group"),
						Business.ResString.GetMultilingualString("ACC965A6-D727-4D4D-8569-CD87C8F3A0CF", "Details contained in received Warehouse Secondary Notify Party and Warehouse RNS Status messages will be emailed to this group of users."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public BooleanRegistryItem IncludeAssociationAssignedCode
		{
			get
			{
				return GetItem("IncludeAssociationAssignedCode", delegate
				{
					return new BooleanRegistryItem(
						"IncludeAssociationAssignedCode",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("3baf1cac-324a-4448-abb6-5df9534826dd", "Include Association Assigned Code"),
						Business.ResString.GetMultilingualString("84a18394-8f90-4b05-b388-5b1e99acc49d", "If this item is set to true then the Associations Assigned Codes (ACIHG/ACIHCM) will be send in the UNH segment of eManifest House Bill and Close messages, otherwise by default the codes will not be sent."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem AutoSendCloseMessage
		{
			get
			{
				return GetItem("AutoSendCloseMessage", delegate
				{
					return new BooleanRegistryItem(
						"AutoSendCloseMessage",
						Categories.Customs_Canada_Import_eManifest,
						Business.ResString.GetMultilingualString("e21d20da-4386-4510-91fb-e9192fafc2da", "Automatically send eManifest Close Message"),
						Business.ResString.GetMultilingualString("97370785-d331-4a74-acef-859e7071b013", "If set to Yes, the system will automatically send the eManifest consol close message once all house bills have been accepted. Set the registry setting to No to disable this automation."),
						RegistryStorageFlags.Company,
						true)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		#endregion

		#region CACustomsCategoryImportDeclaration

		public BooleanRegistryItem EnableCreditCheckForCLVS
		{
			get
			{
				return GetItem("EnableCreditCheckForCLVS", delegate
				{
					return new BooleanRegistryItem(
						"EnableCreditCheckForCLVS",
						Categories.Customs_Canada_Import_Declaration,
						Business.ResString.GetMultilingualString("1D9A7F32-E200-42D1-8463-5DE19EAA6DA2", "Enable Credit Check For CLVS"),
						Business.ResString.GetMultilingualString("182642DD-FED6-488E-B534-3DC49E294F98", "If ticked, the credit check will run when the user ticks the \"Ready for Consolidation\" box on Courier LVS transactions."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public StringRegistryItem AccountSecurityNo
		{
			get
			{
				return GetItem("AccountSecurityNo", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"AccountSecurityNo",
						Categories.Customs_Canada_Import_Declaration,
						Business.ResString.GetMultilingualString("2cf458ef-b0b6-486a-82a2-d31f2e6c854f", "ASEC Number"),
						Business.ResString.GetMultilingualString("c9c9f25a-7988-47c3-aae9-2da56900d732", "CBSA assigned CADEX/CUSDEC Account Security Number."),
						new StringRegistryDataType(5, 5),
						RegistryStorageFlags.Company);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		IRegistryItem ICACustomsDataRegistry.AccountSecurityNo
		{
			get { return AccountSecurityNo; }
		}

		public StringRegistryItem AccountSecurityNoPassword
		{
			get
			{
				return GetItem("AccountSecurityNoPassword", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"AccountSecurityNoPassword",
						Categories.Customs_Canada_Import_Declaration,
						Business.ResString.GetMultilingualString("d4431bbe-03be-407b-8907-aa03ce42e024", "ASEC Number Password"),
						Business.ResString.GetMultilingualString("4d98645f-f776-4b16-8090-7198e8eeb75f", "CBSA assigned CADEX/CUSDEC Account Password."),
						RegistryStorageFlags.Company);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public DecimalRegistryItem MinimumVFDForDutyAndTax
		{
			get
			{
				return GetItem("MinimumVFDForDutyAndTax", delegate
				{
					return new DecimalRegistryItem(
						"MinimumVFDForDutyAndTax",
						Categories.Customs_Canada_Import_Declaration,
						Business.ResString.GetMultilingualString("7a6be033-a139-49b4-b9b7-e7ef9523131a", "Minimum VFD For Duty And Tax"),
						Business.ResString.GetMultilingualString("c584e46f-7c83-4ec2-b4ff-debdd6003618", "Minimum VFD for duty and tax."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						20.0m);
				});
			}
		}

		public CAPackageTypePairsRegistryItem CAPackageTypesMapping
		{
			get
			{
				return GetItem("CAPackageTypesMapping", delegate
				{
					var defaultValue = new CAPackageTypePairCollection();
					defaultValue.AddDefaultValues();
					return new CAPackageTypePairsRegistryItem(
						"CAPackageTypesMapping",
						Categories.Customs_Canada_Import_Declaration,
						Business.ResString.GetMultilingualString("1F539B95-16B8-4D5F-9D8E-204B18ACAD49", "Package Type Mappings"),
						Business.ResString.GetMultilingualString("015F3C82-1D33-4B2A-ADF4-E5C48EC66432", "The mappings from Freight package types to Customs package type. If a brokerage job is embedded in a shipment, this mapping is used to convert the package types Freight shipments use to those Customs jobs use."),
						RegistryStorageFlags.Company,
						defaultValue)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem SuspendAssignmentOfEntryNumberToDisbursementCharges
		{
			get
			{
				return GetItem("SuspendAssignmentOfEntryNumberToDisbursementCharges", delegate
				{
					return new BooleanRegistryItem(
						"SuspendAssignmentOfEntryNumberToDisbursementCharges",
						Categories.Customs_Canada_Import_Declaration,
						Business.ResString.GetMultilingualString("C327C2E2-978C-4653-A5E9-4CB8AB3D3413", "Suspend Assignment Of Entry Number To Disbursement Charges"),
						Business.ResString.GetMultilingualString("D843A8BF-58C6-4897-9235-0A8BAE5653B0", "If ticked, then entry number on import declaration will not be populated on disbursement charges."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		#region CustomsCanadaImportDeclarationDefaultLVSConsolStrategy

		public BooleanRegistryItem CreateIndividualLVSShipments
		{
			get
			{
				return GetItem("CreateIndividualLVSShipments", delegate
				{
					return new BooleanRegistryItem(
						"CreateIndividualLVSShipments",
						Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
						Business.ResString.GetMultilingualString("3e00a487-e9ba-490b-94b5-2a18d50da511", "Create Individual LVS Shipments"),
						Business.ResString.GetMultilingualString("d5800c9a-8a59-4c8b-a6c9-9d49c7bee702", "Create Individual LVS Shipments"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem ConsolidateByImporter
		{
			get
			{
				return GetItem("ConsolidateByImporter", delegate
				{
					return new BooleanRegistryItem(
						"ConsolidateByImporter",
						Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
						Business.ResString.GetMultilingualString("d6aa6589-19ca-4e53-832b-1479e5a270e8", "Consolidate by Importer"),
						Business.ResString.GetMultilingualString("25a87e29-3062-49df-a7d6-f87dedcd5409", "Consolidate by Importer"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem ConsolidateByBranch
		{
			get
			{
				return GetItem("ConsolidateByBranch", delegate
				{
					return new BooleanRegistryItem(
						"ConsolidateByBranch",
						Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
						Business.ResString.GetMultilingualString("2e4531ef-1184-4964-ae2b-8c7781bbffc7", "Consolidate by Branch"),
						Business.ResString.GetMultilingualString("0359d2e7-f2c0-462a-bd27-6a6cac2afd3d", "Consolidate by Branch"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem ConsolidateByProvinceOfClearance
		{
			get
			{
				return GetItem("ConsolidateByProvinceOfClearance", delegate
				{
					return new BooleanRegistryItem(
						"ConsolidateByProvinceOfClearance",
						Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
						Business.ResString.GetMultilingualString("5f3d7b5e-f65e-4d3a-95d3-b6eea33c5652", "Consolidate by Province of Clearance"),
						Business.ResString.GetMultilingualString("5ad3950f-6b1a-428b-b5d7-b7fa0191a5f5", "Consolidate by Province of Clearance"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem ConsolidateByBroker
		{
			get
			{
				return GetItem("ConsolidateByBroker", delegate
				{
					return new BooleanRegistryItem(
						"ConsolidateByBroker",
						Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
						Business.ResString.GetMultilingualString("820ace95-aa34-4b97-9e15-6731c44c421e", "Consolidate by Broker"),
						Business.ResString.GetMultilingualString("ec3a65b2-4cdb-4f4f-a1f0-de41fffb5535", "Consolidate by Broker"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem ConsolidateToOneFTypePerCLVSEntry
		{
			get
			{
				return GetItem("ConsolidateToOneFTypePerCLVSEntry", delegate
				{
					return new BooleanRegistryItem(
						"ConsolidateToOneFTypePerCLVSEntry",
						Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
						Business.ResString.GetMultilingualString("A2858E99-0FA9-48C6-9EAE-BC9B20B1079A", "Consolidate to one F type per CLVS entry"),
						Business.ResString.GetMultilingualString("43E2D5B0-00E4-434E-AD5C-0CD54CB6208B", "Consolidate by one F type per CLVS entry"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		#endregion

		#region CustomsCanandaImportDeclarationB3SendingOptions

		public IntRegistryItem EntryStatementDateThreshold
		{
			get
			{
				return GetItem("EntryStatementDateThreshold", delegate
				{
					IntRegistryItem result = new IntRegistryItem(
						"EntryStatementDateThreshold",
						Categories.Customs_Canada_Import_Declaration_EntrySendingOptions,
						Business.ResString.GetMultilingualString("46C87370-DC8A-49D0-A0EA-6580D09F1305", "Entry Accounting Date Warning Threshold"),
						Business.ResString.GetMultilingualString("4F699E09-045D-4987-B149-D15368A50DA1", "If this value is greater than zero then a warning will be shown when an attempt is made to send a Entry message where the release date plus this threshold is after the next ARL cut-off date. You will then have the option of scheduling a delayed sending of the message."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						4);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public DelayFactorRegistryItem EntryAutomaticSendingDelayThresholds
		{
			get
			{
				return GetItem("EntryAutomaticSendingDelayThresholds", delegate
				{
					var result = new DelayFactorRegistryItem(
						"EntryAutomaticSendingDelayThresholds",
						Categories.Customs_Canada_Import_Declaration_EntrySendingOptions,
						Business.ResString.GetMultilingualString("8B777DAB-524B-49B1-A7BD-B514450B33CD", "Entry Automatic Sending Delay Thresholds"),
						Business.ResString.GetMultilingualString("80A891FD-69D2-49D9-BEAE-53BA8C4738F2", "The thresholds to be used when determining when to automatically send Entry messages."),
						new DelayFactorRegistryBusinessObject(0, DelayIntervalTypeCodes.Codes.None, 0, DelayIntervalTypeCodes.Codes.None));
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public DelayFactorRegistryItem EntryLateSendingFailsafeWarningThresholds
		{
			get
			{
				return GetItem("EntryLateSendingFailsafeWarningThresholds", delegate
				{
					var result = new DelayFactorRegistryItem(
						"EntryLateSendingFailsafeWarningThresholds",
						Categories.Customs_Canada_Import_Declaration_EntrySendingOptions,
						Business.ResString.GetMultilingualString("CBBAB29E-8A62-4EEA-B33A-B74931CA32DF", "Entry Late Sending Fail-safe Warning Thresholds"),
						Business.ResString.GetMultilingualString("29AD58C4-1356-4566-BE46-985FAAE5AF64", "The thresholds to be used when determining when to send a warning that a declaration has not had a Entry successfully lodged."),
						new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.DAR, 12, DelayIntervalTypeCodes.Codes.DAY));
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public CodePairRegistryItem DefaultDeferredNormalEntrySendAction
		{
			get
			{
				return GetItem("DefaultDeferredNormalEntrySendAction", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"DefaultDeferredNormalEntrySendAction",
						Categories.Customs_Canada_Import_Declaration_EntrySendingOptions,
						Business.ResString.GetMultilingualString("617A3FFF-23E1-4191-A218-D67182F694C2", "Default Deferred Normal Entry Send Action"),
						Business.ResString.GetMultilingualString("20C0D7E1-0544-453C-A391-0AEAD7553741", "Default Deferred Normal Entry Send Action\r\n\r\nThe default Entry message sending action when the job is eligible to defer the Entry message sending."),
						new CodeDescriptionPairListProvider(() => DeferredB3SendAction),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						DeferredB3SendActionList.Codes.Defer);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public CodePairRegistryItem DefaultDeferredLowValueEntrySendAction
		{
			get
			{
				return GetItem("DefaultDeferredLowValueEntrySendAction", delegate
				{
					CodePairRegistryItem result = new CodePairRegistryItem(
						"DefaultDeferredLowValueEntrySendAction",
						Categories.Customs_Canada_Import_Declaration_EntrySendingOptions,
						Business.ResString.GetMultilingualString("D8F95A6A-DDFA-40FE-97AF-3BA3F667DD8E", "Default Deferred Low Value Entry Send Action"),
						Business.ResString.GetMultilingualString("8DA286CA-7FC9-4353-808F-2FE78785DBF0", "Default Deferred Low Value Entry Send Action\r\n\r\nThe default Entry message sending action when the Low Value job is eligible to defer the Entry message sending."),
						new CodeDescriptionPairListProvider(() => DeferredB3SendAction),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						DeferredB3SendActionList.Codes.Defer);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		internal CodeDescriptionPairList DeferredB3SendAction
		{
			get { return fDeferredB3SendAction ?? (fDeferredB3SendAction = new DeferredB3SendActionList()); }
		}
		CodeDescriptionPairList fDeferredB3SendAction;

		#endregion

		#region CustomsCanadaImportDeclarationCFIA

		public CodePairRegistryItem DefaultCFIAFeePaymentMethod
		{
			get
			{
				return GetItem("DefaultCFIAFeePaymentMethod", delegate
				{
					return new CodePairRegistryItem(
						"DefaultCFIAFeePaymentMethod",
						Categories.Customs_Canada_Import_Declaration_CFIA,
						Business.ResString.GetMultilingualString("8790e270-7f9b-4dd1-a896-97a016f4bc80", "Default CFIA Fee Payment Method"),
						Business.ResString.GetMultilingualString("7454c15e-94d0-4437-be33-60859d3786d2", "Set the default CFIA fee payment method."),
						new CodeDescriptionPairListProvider(() => CFIAFeePaymentMethods),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						CFIAPaymentMethods.Codes.Other)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		internal CFIAPaymentMethods CFIAFeePaymentMethods
		{
			get
			{
				if (fCFIAFeePaymentMethods == null)
				{
					fCFIAFeePaymentMethods = new CFIAPaymentMethods();
					fCFIAFeePaymentMethods.RemoveCode(CFIAPaymentMethods.Codes.RegistryDefault);
				}
				return fCFIAFeePaymentMethods;
			}
		}
		CFIAPaymentMethods fCFIAFeePaymentMethods;

		public StringRegistryItem AIRSValidationKey
		{
			get
			{
				return GetItem("AIRSValidationKey", delegate
				{
					return new StringRegistryItem(
						"AIRSValidationKey",
						Categories.Customs_Canada_Import_Declaration_CFIA,
						Business.ResString.GetMultilingualString("96e04e84-668f-4f8f-a7c9-d17ab397e8eb", "AIRS Validation Key"),
						Business.ResString.GetMultilingualString("4a07e509-2dee-4391-93e1-169235426ab2", "The broker/importer specific identifier assigned by CFIA for use with the AIRS Validation Service queries’ to this new group."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public IntRegistryItem AVSTimeoutInMinutes
		{
			get
			{
				return GetItem("AVSTimeoutInMinutes", delegate
				{
					return new IntRegistryItem(
						"AVSTimeoutInMinutes",
						Categories.Customs_Canada_Import_Declaration_CFIA,
						Business.ResString.GetMultilingualString("655c6347-3385-4ee9-bfe7-6eb305429c1d", "AVS Timeout in Minutes"),
						Business.ResString.GetMultilingualString("6f2502c9-7db2-40c1-a3fd-471e11d191b6", "AVS Timeout in Minutes."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						15)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public IntRegistryItem AVSRetryTimes
		{
			get
			{
				return GetItem("AVSRetryTimes", delegate
				{
					return new IntRegistryItem(
						"AVSRetryTimes",
						Categories.Customs_Canada_Import_Declaration_CFIA,
						Business.ResString.GetMultilingualString("801c3a71-7eff-436d-bd35-171448240f53", "AVS Retry Times"),
						Business.ResString.GetMultilingualString("6306d386-3811-41fd-b3f1-946302ba9a44", "AVS Retry Times."),
						RegistryStorageFlags.System,
						3);
				});
			}
		}

		public IntRegistryItem AVSRetryIntervalInMinutes
		{
			get
			{
				return GetItem("AVSRetryIntervalInMinutes", delegate
				{
					return new IntRegistryItem(
						"AVSRetryIntervalInMinutes",
						Categories.Customs_Canada_Import_Declaration_CFIA,
						Business.ResString.GetMultilingualString("92b6f27b-a3d4-43ae-926f-da85a4950e3d", "AVS Retry Interval In Minutes"),
						Business.ResString.GetMultilingualString("c77b4273-0482-41f5-8203-4cedffbd77e1", "AVS Retry Interval In Minutes."),
						RegistryStorageFlags.System,
						5);
				});
			}
		}

		public StringRegistryItem AIRSValidationRequestUserName
		{
			get
			{
				return GetItem("AIRSValidationRequestUserName", delegate
				{
					return new StringRegistryItem(
						"AIRSValidationRequestUserName",
						Categories.Customs_Canada_Import_Declaration_CFIA,
						Business.ResString.GetMultilingualString("61e07143-04fb-4e8f-ba82-9f428a0783c8", "AIRS Validation Request User Name"),
						Business.ResString.GetMultilingualString("38483689-d149-47bc-b5a0-820d0937ded7", "The user name that client uses to authenticate itself to AIRS Validation service."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public StringRegistryItem AIRSValidationRequestPassword
		{
			get
			{
				return GetItem("AIRSValidationRequestPassword", delegate
				{
					var result = new StringRegistryItem(
						"AIRSValidationRequestPassword",
						Categories.Customs_Canada_Import_Declaration_CFIA,
						Business.ResString.GetMultilingualString("9f3467b7-8870-4fbe-9620-e49b442025d2", "AIRS Validation Request Password"),
						Business.ResString.GetMultilingualString("3a6a44b6-3ada-4b0e-a143-7b395019fa74", "The password that client uses to authenticate itself to AIRS Validation service."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public StringRegistryItem AIRSValidationRequestURL
		{
			get
			{
				return GetItem("AIRSValidationRequestURL", () => new StringRegistryItem(
					"AIRSValidationRequestURL",
					Categories.Customs_Canada_Import_Declaration_CFIA,
					Business.ResString.GetMultilingualString("d2f202e8-91f7-40cb-9fb1-57214b73f012", "AIRS Validation Request URL"),
					Business.ResString.GetMultilingualString("36e54008-529d-410d-996f-7e69e295c08e", "AIRS Validation Request URL."),
					RegistryStorageFlags.System,
					"https://avs-svs.inspection.gc.ca/avs/bvs.svc/secure"));
			}
		}

		public StringRegistryItem WebProxyAddress
		{
			get
			{
				return GetItem("CAWebProxyAddress", () => new StringRegistryItem(
					"CAWebProxyAddress",
					Categories.Customs_Canada_Import_Declaration_CFIA,
					Business.ResString.GetMultilingualString("1b0f45a8-9663-4973-9c0a-9f1da44a0e81", "Web Proxy Address"),
					Business.ResString.GetMultilingualString("2b673ea7-9326-4542-ac97-f59a76d143c1", "Web Proxy Address."),
					RegistryStorageFlags.System));
			}
		}

		#endregion

		#region CustomsCanandaImportDeclarationCARM

		public StringRegistryItem CARMAPIKey
		{
			get
			{
				return GetItem("CARMAPIKey", delegate
				{
					return new StringRegistryItem(
						"CARMAPIKey",
						Categories.Customs_Canada_Import_Declaration_CARM,
						Business.ResString.GetMultilingualString("E2E9F13F-801F-4675-984B-4DF037AF4A6B", "CARM API Key"),
						Business.ResString.GetMultilingualString("55D99937-9F3E-41EF-99D4-CD39179755F5", "This will store the key issued to the Broker/Importer to submit a CAD query."),
						RegistryStorageFlags.Company);
				});
			}
		}

		public StringRegistryItem CARMEndPoint
		{
			get
			{
				return GetItem("CARMEndPoint", delegate
				{
					return new StringRegistryItem(
						"CARMEndPoint",
						Categories.Customs_Canada_Import_Declaration_CARM,
						Business.ResString.GetMultilingualString("737B68E0-3BF6-40AF-8BF8-9E79F8819967", "CARM End Point"),
						Business.ResString.GetMultilingualString("64B99790-C835-438A-84A6-AFA8A95736B0", "This will store the API end point to post the CARM query to."),
						RegistryStorageFlags.Company,
						CARMEndPointDefaultValue);
				});
			}
		}
		const string CARMEndPointDefaultValue = "https://ccapi-ipacc.cbsa-asfc.cloud-nuage.canada.ca/v1/declaration-srv-read/commercialAccountingDeclarations";

		#endregion

		#region CustomsCanandaImportDeclarationNotifications

		public GuidRegistryItem SendK84ReportNotificationsToGroup
		{
			get
			{
				return GetItem("SendK84ReportNotificationsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendK84ReportNotificationsToGroup",
						Categories.Customs_Canada_Import_Declaration_Notifications,
						Business.ResString.GetMultilingualString("85FAEC2C-06E3-4DB7-BE32-B9A67BE94775", "Send ARL Messages Notification To Group"),
						Business.ResString.GetMultilingualString("A232F74A-393D-4845-B5D1-C71D12267AD0", "The group of users who will receive ARL Message notification when ARL Messages are received. If not entered the Declaration Message Acknowledgements group will be used."),
						RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public GuidRegistryItem SendOverdueReportNotificationsToGroup
		{
			get
			{
				return GetItem("SendOverdueReportNotificationsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendOverdueReportNotificationsToGroup",
						Categories.Customs_Canada_Import_Declaration_Notifications,
						Business.ResString.GetMultilingualString("96D32D32-1015-4875-9E3B-30722DEBF567", "Send Overdue Report Notifications To Group"),
						Business.ResString.GetMultilingualString("1217FEBB-2BAF-47A0-B634-F70284FD96C1", "The group of users who will receive Overdue Report notification when Overdue Reports are received. If not entered the Declaration Message Acknowledgements group will be used."),
						RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public CodePairRegistryItem SendDeclarationMessageAcknowledgements
		{
			get
			{
				return GetItem("SendDeclarationMessageAcknowledgements", delegate
				{
					return new CodePairRegistryItem(
						"SendDeclarationMessageAcknowledgements",
						Categories.Customs_Canada_Import_Declaration_Notifications,
						Business.ResString.GetMultilingualString("1b1f24b3-a86f-4dfe-bd39-1fa7b29f9b63", "Send Declaration Message Acknowledgements"),
						Business.ResString.GetMultilingualString("ecd1b980-0207-43bd-bebc-7c61f9c1fc62", "Send declaration message acknowledgements to staff member, nominated group or combination of both"),
						new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.EmailTo)),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public GuidRegistryItem SendDeclarationMessageAcknowledgementsToGroup
		{
			get
			{
				return GetItem("SendDeclarationMessageAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendDeclarationMessageAcknowledgementsToGroup",
						Categories.Customs_Canada_Import_Declaration_Notifications,
						Business.ResString.GetMultilingualString("21c4ba9f-7d88-437d-b2b8-4459a7db8aac", "Send Declaration Message Acknowledgements To Group"),
						Business.ResString.GetMultilingualString("ee0ad243-06da-4501-8ca3-b80107919bd0", "Send declaration message acknowledgements to selected group"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public CodePairRegistryItem SendDeclarationMessageErrors
		{
			get
			{
				return GetItem("SendDeclarationMessageErrors", delegate
				{
					return new CodePairRegistryItem(
						"SendDeclarationMessageErrors",
						Categories.Customs_Canada_Import_Declaration_Notifications,
						Business.ResString.GetMultilingualString("74ab1ba2-e2f0-45c5-9a02-afd36702b35d", "Send Declaration Message Errors"),
						Business.ResString.GetMultilingualString("88231200-991a-4680-95e5-dd2f0a60f20b", "Send declaration message errors to staff member, nominated group or combination of both"),
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public GuidRegistryItem SendDeclarationMessageErrorsToGroup
		{
			get
			{
				return GetItem("SendDeclarationMessageErrorsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendDeclarationMessageErrorsToGroup",
						Categories.Customs_Canada_Import_Declaration_Notifications,
						Business.ResString.GetMultilingualString("0544aefe-3c13-4d8b-b46b-c7f49790a338", "Send Declaration Message Errors To Group"),
						Business.ResString.GetMultilingualString("25370ac7-9877-4b8c-bda8-dffa95a87f3f", "Send declaration message errors to selected group"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		#endregion

		#region CustomsCanandaImportDeclarationDefaults

		public BooleanRegistryItem AlwaysEnableACROSSMessageValidation
		{
			get
			{
				return GetItem("AlwaysEnableACROSSMessageValidation", delegate
				{
					return new BooleanRegistryItem(
						"AlwaysEnableACROSSMessageValidation",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("F90495FE-2EB1-46F8-8F7C-9A978080D1F2", "Always Enable Release Message Validation"),
						Business.ResString.GetMultilingualString("EC045F6E-3964-405A-9551-FCBC5A9D1A5B", @"If this registry item is overridden to YES then the Validate Release check box on declarations will always be checked and so validation related to the Release message will always be done regardless of the release status of the job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem AlwaysEnableB3MessageValidation
		{
			get
			{
				return GetItem("AlwaysEnableB3MessageValidation", delegate
				{
					return new BooleanRegistryItem(
						"AlwaysEnableB3MessageValidation",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("D852EC43-8437-4E74-96F6-AAD5155EABE6", "Always Enable Entry Message Validation"),
						Business.ResString.GetMultilingualString("2CB5DBE1-0FBB-4E08-BCC6-D3FC94898C92", @"If this registry item is overridden to YES then the Validate CADEX check box on declarations will always be checked and so validation related to the entry message will always be done regardless of the release status of the job."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem AlwaysSendDeliveryAddressOnReleaseMessages
		{
			get
			{
				return GetItem("AlwaysSendDeliveryAddressOnReleaseMessages", delegate
				{
					return new BooleanRegistryItem(
						"AlwaysSendDeliveryAddressOnReleaseMessages",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("3A349493-A85E-42AA-9E5E-D20F0802F056", "Always Send Delivery Address On Release Messages"),
						Business.ResString.GetMultilingualString("EE81E108-CED5-4294-B161-88499865C27C", @"If set then the address, phone and fax of the Delivery Address entered on the Delivery tab of a declaration will be sent in all ACROSS release messages.
If not set then the delivery address, phone and fax will only be sent for CFIA declarations."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem AlwaysUseImporterAccountSecurity
		{
			get
			{
				return GetItem("AlwaysUseImporterAccountSecurity", delegate
				{
					return new BooleanRegistryItem(
						"AlwaysUseImporterAccountSecurity",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("B7188BBD-786A-4F14-B95A-C5BD065897B2", "Always Use Importer Account Security"),
						Business.ResString.GetMultilingualString("653CC127-944D-4C3A-9C09-DC3701F1F2A5", @"If this item is overridden to ‘NO’ then when an importer, who has their own account security code set on the organization,
is selected on a new declaration then a dialogue will be displayed and the user asked if they wish to use the importers account security code or their own brokers account security code.
If this item is set to ‘YES’ then no dialogue will be displayed and the importers account security code will always be used."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public GuidRegistryItem DeclarantOnEntryDocsBrokerOnB3
		{
			get
			{
				return GetItem("DeclarantOnEntryDocsBrokerOnB3", delegate
				{
					return new GuidRegistryItem(
						"DeclarantOnEntryDocsBrokerOnB3",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("ED3AFB17-603A-469C-98C6-BDD30EAF3758", "Declarant On Entry?"),
						Business.ResString.GetMultilingualString("BBEF58F6-0344-453B-9514-1F55E9E15288", "The staff whose signature and name will be printed on the entry documents. If it is left blank, the signature and name of a declaration’s broker or login staff will be printed instead."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						Guid.Empty)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public GuidRegistryItem DeclarantOnEntryDocsBrokerOnB2
		{
			get
			{
				return GetItem("DeclarantOnEntryDocsBrokerOnB2", delegate
				{
					return new GuidRegistryItem(
						"DeclarantOnEntryDocsBrokerOnB2",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("3cd92ff1-2224-4175-84e7-5d478aaf1de1", "Declarant On B2?"),
						Business.ResString.GetMultilingualString("6f31ff2f-682b-4a7e-a280-994727d50915", "The staff whose signature and name will be printed on the B2 documents. If it is left blank, the signature and name of a declaration’s broker or login staff will be printed instead."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						Guid.Empty)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem DefaultExciseDutyQuantityToFirstCustomsQuantity
		{
			get
			{
				return GetItem("DefaultExciseDutyQuantityToFirstCustomsQuantity", delegate
				{
					return new BooleanRegistryItem(
						"DefaultExciseDutyQuantityToFirstCustomsQuantity",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("81bde59e-d222-48e3-82c9-159fc6be1b71", "Default Excise Duty Quantity to the First Customs Quantity"),
						Business.ResString.GetMultilingualString("88562ad9-f905-4f0f-a30f-8aee63bb7adf", "Default the specific Excise Duty quantity to the first Customs Quantity were possible."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		IRegistryItem ICACustomsDataRegistry.DefaultExciseTaxFromCustomsTariff
		{
			get { return DefaultExciseTaxFromCustomsTariff; }
		}

		public BooleanRegistryItem DefaultExciseTaxFromCustomsTariff
		{
			get
			{
				return GetItem("DefaultExciseTaxFromCustomsTariff", delegate
				{
					return new BooleanRegistryItem(
						"DefaultExciseTaxFromCustomsTariff",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("BA954CA1-715C-4303-AE0B-191D70DF94BD", "Default Excise Tax From Customs Tariff"),
						Business.ResString.GetMultilingualString("E33895B7-AEA6-48CE-8785-57D9D71779CA", @"If this registry item is set then any excise tax code associated with the entered HS code will be set in the Duty and Tax Grid.
If not set the excise tax line will be generated but there will be no rate code and so the user must decide if excise tax is applicable."),
						RegistryStorageFlags.Company,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public DecimalEffectiveDateRegistryItem DefaultGeneralRateOfDuty
		{
			get
			{
				return GetItem("DefaultGeneralRateOfDuty", delegate
				{
					return new DecimalEffectiveDateRegistryItem(
						"DefaultGeneralRateOfDuty",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("e810f7f1-edd3-4a10-8eed-cf3a06e7c66d", "Default General Rate Of Duty"),
						Business.ResString.GetMultilingualString("a0dd0cf1-0154-4e15-9d1c-774b2a6bb8ef", "Default general rate of duty."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						new DecimalEffectiveDate { PreviousValue = 35m, NewValue = 35m, EffectiveDate = new ZDateTime(2010, 1, 1) })
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public CodePairRegistryItem DefaultLVSInvoiceDetailsCode
		{
			get
			{
				return GetItem("DefaultLVSInvoiceDetailsCode", delegate
				{
					return new CodePairRegistryItem(
						"DefaultLVSInvoiceDetailsCode",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("C44BD6E8-93DB-405D-B46F-65F876EDE020", "Default LVS Invoice Detail Code"),
						Business.ResString.GetMultilingualString("6A2AEBE1-416C-4BB1-96D8-BF1CD301B78D", @"This is the default code to select how much detail is included on LVS Invoices.
'Summarize' will print totals by Importer, while 'Detail' will print individual shipments.
This code may be overridden on Importer Organizations."),
						new CodeDescriptionPairListProvider(() => LVSInvoiceDetails),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						LVSInvoiceDetailCodes.Codes.Summarize)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		internal LVSInvoiceDetailCodes LVSInvoiceDetails
		{
			get
			{
				if (lvsInvoiceDetails == null)
				{
					lvsInvoiceDetails = new LVSInvoiceDetailCodes();
					lvsInvoiceDetails.RemoveCode(LVSInvoiceDetailCodes.Codes.RegistryDefault);
				}
				return lvsInvoiceDetails;
			}
		}
		LVSInvoiceDetailCodes lvsInvoiceDetails;

		public BooleanRegistryItem ForceManualInputOfTransactionNumber
		{
			get
			{
				return GetItem("ForceManualTransactionNumberAllocation", delegate
				{
					return new BooleanRegistryItem(
						"ForceManualTransactionNumberAllocation",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("4D4417AF-2E17-4EBF-BD46-C09B30C3E97E", "Force Manual Input of Transaction Number"),
						Business.ResString.GetMultilingualString("92785907-6C03-45D8-A7CD-EB99F123451D", "If this registry is overridden to YES then user must enter in the transaction number."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem DisplaySequentialOfTransactionNumberSeparately
		{
			get
			{
				return GetItem("DisplaySequentialOfTransactionNumberSeparately", delegate
				{
					return new BooleanRegistryItem(
						"DisplaySequentialOfTransactionNumberSeparately",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("9A0D4E41-DC50-468E-BA22-64C1440A20BB", "Display Sequential Part of Transaction Number separately"),
						Business.ResString.GetMultilingualString("CDDE2F25-5783-47D4-8B48-883A641C16AE", "If this registry item is overridden to YES then system will display the transaction number in 3 separated fields."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem DisplayCargoControlNumberSeparately
		{
			get
			{
				return GetItem("DisplayCargoControlNumberSeparately", delegate
				{
					return new BooleanRegistryItem(
						"DisplayCargoControlNumberSeparately",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("D6EC3C22-34D7-4A81-950B-BA9672A08265", "Display Sequential Part of Cargo Control Number separately"),
						Business.ResString.GetMultilingualString("9F28701E-4974-4C12-9240-82D1363C3AC7", "If this registry item is overridden to YES then system will display the cargo control number in 2 separated fields."),
						RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public StringRegistryItem DefaultPortOfClearance
		{
			get
			{
				return GetItem("DefaultPortOfClearance", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"DefaultPortOfClearance",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("4D8267B7-CEFA-4307-A4F4-65D45D26F141", "Port Of Clearance"),
						Business.ResString.GetMultilingualString("F098BFE0-C9DD-4780-9F12-3A3023FB8F9B", "Enter the default Port Of Clearance"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					result.EditorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.Customs.Universal.ZZRefCusCodeList, GetPortOfClearanceCollection);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public BooleanRegistryItem ShouldPrintBrokerSignatureOnEntryDocs
		{
			get
			{
				return GetItem("ShouldPrintBrokerSignatureOnEntryDocs", delegate
				{
					return new BooleanRegistryItem(
						"ShouldPrintBrokerSignatureOnEntryDocs",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("313b5964-868d-46ec-9c2d-72c4fc1dbcc3", "Print Broker Signature On B2 and B3?"),
						Business.ResString.GetMultilingualString("36bc1857-eca6-4795-ac01-2c85bfd2b052", "Indicate whether the broker's electronic signature should be printed on the B2 and B3 Entry Documents. The signature will come from the brokers signature on their staff record."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public StringRegistryItem DefaultTariffTreatment
		{
			get
			{
				return GetItem("DefaultTariffTreatment", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"DefaultTariffTreatment",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("091c8acc-6000-48e9-9be6-0851fc76be7d", "Default Tariff Treatment"),
						Business.ResString.GetMultilingualString("d33ef7b3-3bd8-4663-bed4-516669838ee7", "This value will set as the default Tariff Treatment on new Invoice Headers."),
						new StringRegistryDataType(0, 2),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						TariffTreatmentCodes.Codes.MostFavouredNation)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
					return result;
				});
			}
		}

		public GuidRegistryItem DefaultBranchPgaContact
		{
			get
			{
				return GetItem<GuidRegistryItem>("DefaultBranchPGAContact", delegate
				{
					return new DefaultBranchPGAContactRegistryItem(
						"DefaultBranchPGAContact",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("406176DC-B749-43F9-A530-5E297D8538FA", "Default Branch PGA Contact"),
						Business.ResString.GetMultilingualString("2B62DE91-8321-43E0-BFD7-585EBFC3CDC0", "The contact name, email address, fax, mobile and phone number of the selected staff member will default for declaration when Partner Government Agency reporting is required. \r\n\r\nIf the staff member does not have a work phone number entered, the branch phone number of the home branch of that staff member will be used."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						Guid.Empty)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public DefaultFreightPercentagesRegistryItem DefaultFreightPercentages
		{
			get
			{
				return GetItem("DefaultFreightPercentages", delegate
				{
					return new DefaultFreightPercentagesRegistryItem(
						"DefaultFreightPercentages",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("59c11083-6d79-473f-af24-b2844d1dcde3", "Default Freight Percentages"),
						Business.ResString.GetMultilingualString("92002cd5-1588-4b25-bfb6-df456118f0b0", "Enter the percentages to be used, for each mode of transport, to calculate freight to appear in box 19 of the B3 when no freight charges are entered on the declaration invoices. These values may be overridden for individual Importers."),
						RegistryStorageFlags.System);
				});
			}
		}

		public BooleanRegistryItem DefaultOffsetNegativeGSTLinesForTotalsInB2Form
		{
			get
			{
				return GetItem("DefaultOffsetNegativeGSTLinesForTotalsInB2Form", delegate
				{
					return new BooleanRegistryItem(
						"DefaultOffsetNegativeGSTLinesForTotalsInB2Form",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("742A7CE7-6D78-4087-9E64-EF316E630BB9", "Offset negative GST lines for Totals in B2 form"),
						Business.ResString.GetMultilingualString("205CEE80-625B-44CC-A74C-262EEF175B48", "Default offset negative GST lines for Totals in B2 form."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public CodePairRegistryItem SeverityLevelForCertificateOfOriginValidations
		{
			get
			{
				return GetItem("SeverityLevelForCertificateOfOriginValidations", delegate
				{
					return new CodePairRegistryItem(
						"SeverityLevelForCertificateOfOriginValidations",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("1F3E9DAD-45EE-4C74-8386-AF379FCDCEBF", "Severity Level for Certificate of Origin Validations"),
						Business.ResString.GetMultilingualString("3A6299B8-665B-40A8-931F-83F09D79AEAE", "Set the severity level for validating related to the Certificate of Origin(COO) for the selected Treatment code(TT). If validation is activated, the system will look for a valid COO for TT. The COO may exist on the Product or on the Importer Organization."),
						new CodeDescriptionPairListProvider(() => new Enterprise.Registry.Business.Customs.ProductAuditActions()),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						ProductAuditActions.Codes.NoAction)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public CodePairRegistryItem DefaultToThisExciseTaxRateCodeWhenApplicable
		{
			get
			{
				return GetItem("DefaultToThisExciseTaxRateCodeWhenApplicable", delegate
				{
					return new CodePairRegistryItem(
						"DefaultToThisExciseTaxRateCodeWhenApplicable",
						Categories.Customs_Canada_Import_Declaration_Defaults,
						Business.ResString.GetMultilingualString("E04A5405-0426-40D2-910E-1DA8D063B155", "Default to this Excise Tax rate code when applicable"),
						Business.ResString.GetMultilingualString("DAED7E23-3E22-4DBC-A166-03AFB59AE4EB", "Set the default to this Excise Tax rate code when applicable."),
						new CodeDescriptionPairListProvider(() => RefCusRateCodePairList),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						ZString.Empty)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		internal CodeDescriptionPairList RefCusRateCodePairList
		{
			get
			{
				if (fRefCusRateCodePairList == null)
				{
					var factory = new BusinessObjectFactory();
					fRefCusRateCodePairList = UniversalReferenceHelper.GetRefCusRateCodePairList(factory, DutyAndTaxTypes.Codes.ExciseTax, ZDateTime.Today, isAddNO: false);
				}
				return fRefCusRateCodePairList;
			}
		}
		CodeDescriptionPairList fRefCusRateCodePairList;

		#endregion

		#region CustomsCanadaImportDeclarationQualityControl

		public IntRegistryItem TimeFrameForExceptionReporting
		{
			get
			{
				return GetItem("TimeFrameForExceptionReporting", delegate
				{
					return new IntRegistryItem(
						"TimeFrameForExceptionReporting",
						Categories.Customs_Canada_Import_Declaration_QualityControl,
						Business.ResString.GetMultilingualString("ff07e6ca-afc1-41f5-a8f4-415346bb5f73", "Time Frame for Exception Reporting"),
						Business.ResString.GetMultilingualString("2e4ca74b-0089-4e50-a032-c72063d1d290", "The number of days in the past that we will use to select possible jobs for exception reporting based on the first time that a message was sent to Customs. Increasing this value will decrease performance."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						60)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public IntRegistryItem B3AcceptedButNotReportedOnDN
		{
			get
			{
				return GetItem("B3AcceptedButNotReportedOnDN", () => new IntRegistryItem(
					"B3AcceptedButNotReportedOnDN",
					Categories.Customs_Canada_Import_Declaration_QualityControl,
					Business.ResString.GetMultilingualString("C18597BD-ACE8-4C80-84F5-32254824BB2A", "Entry accepted but not reported on DN"),
					Business.ResString.GetMultilingualString("7998E2EA-DAEA-421C-BB02-11C674DF4701",
						"Flag jobs that have had the entry accepted but have not been reported on a Daily Notice.(Days)"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company, 1)
				{ CountryFilterPKs = CountryFilterPKs.Canada });
			}
		}

		public IntRegistryItem B3NoResponseThreshold
		{
			get
			{
				return GetItem("B3NoResponseThreshold", () => new IntRegistryItem(
					"B3NoResponseThreshold",
					Categories.Customs_Canada_Import_Declaration_QualityControl,
					Business.ResString.GetMultilingualString("553E5FD9-6BEB-4411-8511-D41402722761", "Entry no response threshold"),
					Business.ResString.GetMultilingualString("A91E646C-FCBF-4833-BA27-08CA51696427",
						"Flag jobs that have had an Entry sent but there has not been a response.(Hours)"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company, 2)
				{ CountryFilterPKs = CountryFilterPKs.Canada });
			}
		}

		public IntRegistryItem PARSNotreleasedAirThreshold
		{
			get
			{
				return GetItem("PARSNotreleasedAirThreshold", () => new IntRegistryItem(
					"PARSNotreleasedAirThreshold",
					Categories.Customs_Canada_Import_Declaration_QualityControl,
					Business.ResString.GetMultilingualString("29285700-3D35-45F2-AF02-B5780A2F418E", "PARS not released, Air threshold"),
					Business.ResString.GetMultilingualString("AE89EA4D-E501-4960-9117-DD4ADBC3B628",
						"Flag PARS Air jobs that have not been released after ETA(Hours)"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company, 6)
				{ CountryFilterPKs = CountryFilterPKs.Canada });
			}
		}

		public IntRegistryItem PARSNotreleasedOceanThreshold
		{
			get
			{
				return GetItem("PARSNotreleasedOceanThreshold", () => new IntRegistryItem(
					"PARSNotreleasedOceanThreshold",
					Categories.Customs_Canada_Import_Declaration_QualityControl,
					Business.ResString.GetMultilingualString("F1096720-7724-4BB4-A303-1D84B3254654", "PARS not released Ocean threshold"),
					Business.ResString.GetMultilingualString("73E060B2-F97F-4A77-BEF6-D8504BC7C5B0",
						"Flag PARS Ocean jobs that have not been released after ETA.(Hours)"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company, 24)
				{ CountryFilterPKs = CountryFilterPKs.Canada });
			}
		}

		public IntRegistryItem PARSNotReleasedHighwayRailAndOtherThreshold
		{
			get
			{
				return GetItem("PARSNotReleasedHighwayRailAndOtherThreshold", () => new IntRegistryItem(
					"PARSNotReleasedHighwayRailAndOtherThreshold",
					Categories.Customs_Canada_Import_Declaration_QualityControl,
					Business.ResString.GetMultilingualString("86837B94-A1B6-4969-8955-7F929A4A6F0C", "PARS not released, Highway, Rail and Other threshold"),
					Business.ResString.GetMultilingualString("46C3CEAE-E61B-416E-B242-003A8890D548",
						"Flag PARS Highway, Rail and Other jobs that have not been released after ETA.(Hours)"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company, 12)
				{ CountryFilterPKs = CountryFilterPKs.Canada });
			}
		}

		public IntRegistryItem PostArrivalNotReleased
		{
			get
			{
				return GetItem("PostArrivalNotReleased", () => new IntRegistryItem(
					"PostArrivalNotReleased",
					Categories.Customs_Canada_Import_Declaration_QualityControl,
					Business.ResString.GetMultilingualString("C259C539-4577-41BD-BD35-3F090A122FBC", "Post Arrival not released"),
					Business.ResString.GetMultilingualString("B07377D4-F579-47A5-9F94-31AD1A3B271E",
						"Flag Post Arrival jobs that have not been released.(Days)"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company, 1)
				{ CountryFilterPKs = CountryFilterPKs.Canada });
			}
		}

		public IntRegistryItem ReleaseNoResponseThreshold
		{
			get
			{
				return GetItem("ReleaseNoResponseThreshold", delegate
				{
					return new IntRegistryItem(
						"ReleaseNoResponseThreshold",
						Categories.Customs_Canada_Import_Declaration_QualityControl,
						Business.ResString.GetMultilingualString("F39C8B4C-2402-4B78-AE48-DAE6B229D221", "Release No Response Threshold"),
						Business.ResString.GetMultilingualString("F9C4E487-93FC-43B3-BC7B-DE632D696911", "Flag jobs that have been released but there has not been a response.(Hours)."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						2)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public IntRegistryItem SendB3CADProgressFormThreshold
		{
			get
			{
				return GetItem("SendB3CADProgressFormThreshold", delegate
				{
					return new IntRegistryItem(
						"SendB3CADProgressFormThreshold",
						Categories.Customs_Canada_Import_Declaration_QualityControl,
						Business.ResString.GetMultilingualString("81d1a8d1-9831-4920-82c3-29100d502e56", "Send CAD Message Progress Form Threshold"),
						Business.ResString.GetMultilingualString("00ce12cc-c3f7-4b78-9d6c-3bca05bf41ec", "Defined the threshold to send CAD messages with progress bar dialog box when having CAD entry lines more than this value."),
						RegistryStorageFlags.Company, 100)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		#endregion

		#endregion

		#region CACustomsCategoryImportRNS

		public StringRegistryItem DefaultRNSOffice
		{
			get
			{
				return GetItem("DefaultRNSOffice", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"DefaultRNSOffice",
						Categories.Customs_Canada_Import_RNS,
						Business.ResString.GetMultilingualString("5D8267B7-CEFA-4307-A4F4-65D45D26F141",
						"Default CBSA Office"),
						Business.ResString.GetMultilingualString("G098BFE0-C9DD-4780-9F12-3A3023FB8F9B",
						"Enter the default CBSA office"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					result.EditorInfo = new CodeFindBoxRegistryEditorInfo(ModuleIDs.Customs.Universal.ZZRefCusCodeList, GetCBSAOfficeCollection);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		IRegistryItem ICACustomsDataRegistry.RNSActive
		{
			get { return RNSActive; }
		}

		public BooleanRegistryItem RNSActive
		{
			get
			{
				return GetItem("RNSActive", delegate
				{
					return new BooleanRegistryItem(
						"RNSActive",
						Categories.Customs_Canada_Import_RNS,
						Business.ResString.GetMultilingualString("8e5c3af6-4ea0-45c0-9219-7f2dd8e49a44",
						"Should RNS Functionality be activated?"),
						Business.ResString.GetMultilingualString("8e5c3af6-4ea0-45c0-9219-7f2dd8e49a44", "Should RNS Functionality be activated?"),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						true)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}
		#endregion

		#endregion

		#region CACustomsServiceTasks

		public GuidRegistryItem DefaultBranchForServiceTasks
		{
			get
			{
				return GetItem("DefaultBranchForServiceTasks", delegate
				{
					var result = new GuidRegistryItem(
						"DefaultBranchForServiceTasks",
						Categories.Customs_Canada_ServiceTasks,
						Business.ResString.GetMultilingualString("A97FC206-4F15-452A-828F-6C7EE2370089", "Default Service Task Branch"),
						Business.ResString.GetMultilingualString("0C0816E6-B5E0-4753-9666-04859E523E86", "Set the default branch which Canadian service tasks would run under."),
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbBranchNotCurrentCompanyRelated);
					return result;
				});
			}
		}

		public BooleanRegistryItem RunServiceProviderClientServiceTaskAppliesAllCountries
		{
			get
			{
				return GetItem("RunServiceProviderClientServiceTaskAppliesAllCountries", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"RunServiceProviderClientServiceTaskAppliesAllCountries",
						Categories.Customs_Canada_ServiceTasks,
						Business.ResString.GetMultilingualString("6d3ac504-1d41-446a-9f00-7a0685830354", "Run CA Messaging Service Tasks?"),
						Business.ResString.GetMultilingualString("39f0a474-7f28-424a-ae0d-c4cbd2ebe49a", "If you have a licensed Canadian company in your DB then the CA Messaging Service Tasks will automatically be active and you do not need to set this registry setting, but if you do not have a licensed CA company in your Data Base, but wish to use CA messaging like ACI and eManifest, then override this setting and set to ‘Yes’."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						false);
					return result;
				});
			}
		}

		#endregion

		#region CACustomsCategoryTestingDevelopment

		public BooleanRegistryItem EnableImportUniversalTransactionBatchXMLFiles
		{
			get
			{
				return GetItem("EnableImportUniversalTransactionBatchXMLFiles", delegate
				{
					return new BooleanRegistryItem(
						"EnableImportUniversalTransactionBatchXMLFiles",
						Categories.Customs_Canada_TestingDevelopment,
						Business.ResString.GetMultilingualString("6f99ac97-85a2-4fc3-abfe-4e3756568881", "Enable Import Universal Transaction Batch XML Files?"),
						Business.ResString.GetMultilingualString("e790c4b4-9be7-442a-b902-ecbef93cd273", @"Turning this on will cause the menu - ""Actions - Data Transfer - Import Universal Transaction Batch XML"" enable on the ""DN & SOA Statements"" module."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers
						, false);
				});
			}
		}

		public StringRegistryItem MessageOutputDirectory
		{
			get
			{
				return GetItem("MessageOutputDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"MessageOutputDirectory",
						Categories.Customs_Canada_TestingDevelopment,
						Business.ResString.GetMultilingualString("70c7522e-8d30-4d0d-91df-69bde49524b6", "Test Message Output Directory"),
						Business.ResString.GetMultilingualString("041a9e5a-4568-4a52-a6f4-24dbb36d2daa", "Enter the location where test messages will be placed."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForDevelopers);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public StringRegistryItem MessageInputDirectory
		{
			get
			{
				return GetItem("MessageInputDirectory", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"MessageInputDirectory",
						Categories.Customs_Canada_TestingDevelopment,
						Business.ResString.GetMultilingualString("601ad42a-b1ab-4aa9-93ea-d5a1d4310b89", "Test Message Input Directory"),
						Business.ResString.GetMultilingualString("ed6ad26f-96e3-4dbd-b092-ff5a71eaf609", "Enter the storage location where test messages will be read."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForDevelopers);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.DirectoryBrowser);
					result.CountryFilterPKs = CountryFilterPKs.Canada;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableDebugHooks
		{
			get
			{
				return GetItem("EnableDebugHooks", delegate
				{
					return new BooleanRegistryItem(
						"EnableDebugHooks",
						Categories.Customs_Canada_TestingDevelopment,
						Business.ResString.GetMultilingualString("387aff0e-2ba1-49c5-8a4d-4d43f0dbc413", "Enable Debug Hooks?"),
						Business.ResString.GetMultilingualString("510b6373-fc73-4c3b-b3f8-3002898f4838", "Turning this on will cause copies of data send and received from Customs will be stored in a DEBUG folder off the CIG folder. Other debugging features and logging will also be turned on."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem CSAFunctionActive
		{
			get
			{
				return GetItem("CSAFunctionActive", delegate
				{
					return new BooleanRegistryItem(
						"CSAFunctionActive",
						Categories.Customs_Canada_TestingDevelopment,
						Business.ResString.GetMultilingualString("132fe6f7-80c1-49d9-9942-f24c5244dafb", "Should CSA Functionality be activated?"),
						Business.ResString.GetMultilingualString("d7926dfa-f811-4872-9f9c-338ceb783f28", "Should CSA Functionality be activated?"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem UseCasualProvinceForTaxOverrides
		{
			get
			{
				return GetItem("UseCasualProvinceForTaxOverrides", delegate
				{
					return new BooleanRegistryItem(
						"UseCasualProvinceForTaxOverrides",
						Categories.Customs_Canada_TestingDevelopment,
						Business.ResString.GetMultilingualString("37D111E6-B1D9-4C1F-A2FB-0985F44A2BD1", "Use Casual Province For Tax Overrides"),
						Business.ResString.GetMultilingualString("A54C187B-A21C-469F-BC2D-AB3B6AEFB5C1", "Use Casual Province In Place Of Debtor Or Creditor Home Country/Region For Shipment and Declaration Jobs, for Tax Overrides (For Developers Only)."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false)
					{ CountryFilterPKs = CountryFilterPKs.Canada };
				});
			}
		}

		public BooleanRegistryItem ActivateAutoB3Sending
		{
			get
			{
				return GetItem("ActivateAutoB3Sending", delegate
				{
					return new BooleanRegistryItem(
						"ActivateAutoB3Sending",
						Categories.Customs_Canada_TestingDevelopment,
						Business.ResString.GetMultilingualString("39e4532a-2228-42dc-b614-b6548fe3b620", "Activate Auto Entry Sending"),
						Business.ResString.GetMultilingualString("712a37fc-673d-4824-a697-9f05d97dcca0", "Activate Auto Entry Sending (DO NOT ACTVATE)?\r\n\r\nDO NOT ACTIVATE THIS WITHOUT FIRST DISCUSSING WITH THE CUSTOMS TEAM, THIS FEATURE HAS BEEN REPLACED BY WORFLOW FUNCTION. The process controller needs to be restarted for the registry change to take effect."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem AllowPaymentPartyOverride
		{
			get
			{
				return GetItem("AllowPaymentPartyOverride", delegate
				{
					return new BooleanRegistryItem(
						"AllowPaymentPartyOverride",
						Categories.Customs_Canada_TestingDevelopment,
						Business.ResString.GetMultilingualString("6E39C3BB-7719-4A27-93EE-71B71F074403", "Allow Payment Party Override."),
						Business.ResString.GetMultilingualString("DCCE24F2-7193-418C-A0D9-BB22B68E6E8F", "Allow Payment Party Override."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem ShouldBatchNumberBeByInterchange
		{
			get
			{
				return GetItem("ShouldBatchNumberBeByInterchange", delegate
				{
					return new BooleanRegistryItem(
						"ShouldBatchNumberBeByInterchange",
						Categories.Customs_Canada_TestingDevelopment,
						Business.ResString.GetMultilingualString("9d436e8a-abf7-4b11-a7c3-5ddde7a7c240", "Should Batch Number be by Interchange"),
						Business.ResString.GetMultilingualString("0fda9807-40b1-4dd4-a0e2-2ae5a257bf5b", "Should Batch Number be by Interchange."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		#region CopyOGDToPGADataDateTime

		public DateTimeRegistryItem CopyOGDToPGADataDateTime
		{
			get
			{
				return GetItem("CopyOGDToPGADataDateTime", () => new DateTimeRegistryItem(
					"CopyOGDToPGADataDateTime",
					Categories.Customs_Canada_Import,
					(NoResString)"Processing copy OGD to PGA date",
					(NoResString)"It's time to processing copy OGD to PGA date for products.",
					RegistryStorageFlags.System,
					RegistryOptions.NotCached | RegistryOptions.IsOnlyForDevelopers));
			}
		}

		#endregion

		#region Implementation

		IBusinessObjectCollection GetPortOfClearanceCollection(BusinessObjectFactory factory)
		{
			return Universal.ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Constants.CountryCodes.Canada, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
		}

		IBusinessObjectCollection GetCBSAOfficeCollection(BusinessObjectFactory factory)
		{
			return Universal.ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Constants.CountryCodes.Canada, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
		}

		#endregion
	}
}
