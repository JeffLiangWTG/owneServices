using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.BR;
using ResString = Enterprise.Customs.BR.Business.ResString;

namespace Enterprise.Customs.BR.Registry
{
	public sealed class BRCustomsDataRegistry : RegistryItemSet, IBRCustomsDataRegistry
	{
		#region Construction

		public static BRCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new BRCustomsDataRegistry()); }
		}

		[ThreadStatic]
		static BRCustomsDataRegistry instance;

		public BRCustomsDataRegistry()
		{
		}

		#endregion

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Brazil { get { return CombineCategories(Customs_CountryOrRegion, ResString.GetMultilingualString("cd488781-6613-4ee3-aed9-5130e5426025", "Brazil")); } }
			public static MultilingualString Customs_Brazil_Export { get { return CombineCategories(Customs_Brazil, ResString.GetMultilingualString("c8cf7185-a9ab-4de8-bf90-71f7260343e8", "Export")); } }
			public static MultilingualString Customs_Brazil_ImportLicense { get { return CombineCategories(Customs_Brazil, ResString.GetMultilingualString("2FD69943-B279-47B6-987E-91E0684B8B24", "Import License")); } }
			public static MultilingualString Customs_Brazil_ImportSiscomex { get { return CombineCategories(Customs_Brazil, ResString.GetMultilingualString("79ACB70E-D110-4A9C-94E9-7A1EBBC12414", "Import (SISCOMEX Web)")); } }
			public static MultilingualString Customs_Brazil_ProductCatalog { get { return CombineCategories(Customs_Brazil, ResString.GetMultilingualString("1E58451A-5ED1-47C7-AD8C-F52D29801905", "Product Catalog")); } }
			public static MultilingualString Customs_Brazil_ForeignOperator { get { return CombineCategories(Customs_Brazil, ResString.GetMultilingualString("03C4CB5E-E0A8-4195-B4DC-3155B727585A", "Foreign Operator")); } }
			public static MultilingualString Customs_Brazil_LPCO { get { return CombineCategories(Customs_Brazil, ResString.GetMultilingualString("afbe1d93-a327-4980-97d1-d2512b3aacaa", "LPCO")); } }
			public static MultilingualString Customs_Brazil_Import { get { return CombineCategories(Customs_Brazil, ResString.GetMultilingualString("556ce107-df50-4dd8-9f07-e3751f42e9cf", "Import")); } }
			public static MultilingualString Customs_Brazil_Push { get { return CombineCategories(Customs_Brazil, ResString.GetMultilingualString("AC378404-8DDD-406E-BD4B-15272B55B635", "Push")); } }
		}

		#endregion

		public override bool IsForProductivityWise => false;

		public BooleanRegistryItem ShouldLoadTariffAttributesOfTestEnvironment
		{
			get
			{
				return GetItem("BRShouldLoadTariffAttributesOfTestEnvironment", delegate
				{
					var result = new BooleanRegistryItem(
						"BRShouldLoadTariffAttributesOfTestEnvironment",
						Categories.Customs_Brazil,
						ResString.GetMultilingualString("895F1342-D068-41BB-BBA9-0F5A4E9E5BFD", "Tariff Attributes Mode"),
						ResString.GetMultilingualString("CE2D477C-6760-4EE8-A33C-85D5E149D4AA", @"This registry is used for selecting to which reference file (usually attributes) will be loaded in customs declarations and other modules.

By default, this registry value is set to the database instance of the license type. BE AWARE that if you change it to a value that does not match with ""Connection To XT Server (Production/Test/Local)"" your declaration might be created using the reference file selected, but MESSAGES MIGHT BE REJECTED BECAUSE THE REFERENCE FILE CODES DOES NOT MATCH TO THE ENVIRONMENT THAT WILL RECEIVE THE MESSAGE.

Should CargoWise load Test reference file rather than Production reference file?"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						IsTestLicence);
					result.CountryFilterPKs = CountryFilterPKs.Brazil;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableUCRManifest
		{
			get
			{
				return GetItem("IsBREnableUCRManifest", delegate
				{
					var result = new BooleanRegistryItem(
						"IsBREnableUCRManifest",
						Categories.Customs_Brazil,
						ResString.GetMultilingualString("A703C797-90C3-4199-9D42-CE33312523E5", "Enable UCR Manifest"),
						ResString.GetMultilingualString("E095A9BE-6D72-4B65-BEE2-B745D9138D0D", "Should enable Brazil UCR Manifest creation?"),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
					result.CountryFilterPKs = CountryFilterPKs.Brazil;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableLPCO
		{
			get
			{
				return GetItem("EnableBRLPCO", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableBRLPCO",
						Categories.Customs_Brazil_LPCO,
						ResString.GetMultilingualString("8C6159F1-195E-4F10-A4DD-9BA6EAF9E5CB", "Enable LPCO"),
						ResString.GetMultilingualString("490C1630-E78B-4CFC-83AF-16BAB8AAA836", "Should enable Brazil LPCO creation?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		bool IBRCustomsDataRegistry.EnableLPCO => EnableLPCO.Value;

		public BooleanRegistryItem EnableForeignOperator
		{
			get
			{
				return GetItem("EnableBRForeignOperator", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableBRForeignOperator",
						Categories.Customs_Brazil_ForeignOperator,
						ResString.GetMultilingualString("273015FC-56B4-4F20-88BC-A503A8D31DB2", "Enable Register or Amend Foreign Operator"),
						ResString.GetMultilingualString("62DF2A1E-271A-47F4-950A-F1F12D69E9CC", "Should enable Register or Amend a Foreign Operator?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		bool IBRCustomsDataRegistry.EnableForeignOperator => EnableForeignOperator.Value;

		public BooleanRegistryItem EnableImportLicense
		{
			get
			{
				return GetItem("EnableBRImportLicense", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableBRImportLicense",
						Categories.Customs_Brazil_ImportLicense,
						ResString.GetMultilingualString("581816D7-A4FC-43C8-A0A5-66397E2FA699", "Enable Import License"),
						ResString.GetMultilingualString("BA399F2E-F7F7-4D7C-94D0-915672E5C652", "Enable Import License"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		bool IBRCustomsDataRegistry.EnableImportLicense => EnableImportLicense.Value;

		public bool IsTestLicence => (RegKey?.DatabaseType ?? string.Empty) != DatabaseTypes.Codes.Production;

		IProductRegistrationKey RegKey => ObjectFactory.Get<IProductRegistration>()?.Key;

		public GuidRegistryItem TaxFeeCustomsPaymentBankAccount
		{
			get
			{
				return GetItem("BRTaxFeeCustomsPaymentBankAccount", delegate
				{
					var result = new GuidRegistryItem(
					"BRTaxFeeCustomsPaymentBankAccount",
					Categories.Customs_Brazil_ImportSiscomex,
					ResString.GetMultilingualString("ddc73b4a-edde-4704-b07b-ddec9e7ed2b8", "Tax and Fee Payment Bank Account"),
					ResString.GetMultilingualString("210ba55a-16a2-4507-b6e8-118eff0c5c1c", "This Bank Account is used for payment taxes and fees related  to message sent to customs."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccBankAccount);
					result.CountryFilterPKs = CountryFilterPKs.Brazil;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableUpdateEntryStatusViaUniversalEventXML_ISW
		{
			get
			{
				return GetItem("BREnableUpdateEntryStatusViaUniversalEventXML_ISW", delegate
				{
					var result = new BooleanRegistryItem(
					"BREnableUpdateEntryStatusViaUniversalEventXML_ISW",
					Categories.Customs_Brazil_ImportSiscomex,
					ResString.GetMultilingualString("2D190BC0-6564-40CB-A76D-69DC3270D6E3", "Update Entry Status via Universal Event XML (BLT Submit Type)"),
					ResString.GetMultilingualString("B317ABF6-9EA3-426B-95D9-034BA966400A", "Update Entry Status via Universal Event XML (BLT Submit Type)"),
					RegistryStorageFlags.System,
					false);
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableUpdateEntryStatusViaUniversalEventXML_LIC
		{
			get
			{
				return GetItem("BREnableUpdateEntryStatusViaUniversalEventXML_LIC", delegate
				{
					var result = new BooleanRegistryItem(
					"BREnableUpdateEntryStatusViaUniversalEventXML_LIC",
					Categories.Customs_Brazil_ImportLicense,
					ResString.GetMultilingualString("F561D193-C883-4C1C-813C-99D349F61606", "Update Entry Status via Universal Event XML (BLT Submit Type)"),
					ResString.GetMultilingualString("1439E1CD-E8F9-47EC-AD54-3474837C359A", "Update Entry Status via Universal Event XML (BLT Submit Type)"),
					RegistryStorageFlags.System,
					false);
					return result;
				});
			}
		}

		public GuidRegistryItem SendExportDeclarationErrorsToGroup
		{
			get
			{
				return GetItem("BRSendExportDeclarationErrorsToGroup", delegate
				{
					var result = new GuidRegistryItem(
						"BRSendExportDeclarationErrorsToGroup",
						Categories.Customs_Brazil_Export,
						ResString.GetMultilingualString("B1B53179-52B4-49BD-B15C-470EDAD8521F", "Group To Send Export Declaration Errors To"),
						ResString.GetMultilingualString("A8338DAB-6A82-44D4-9503-952B54E150A1", "Indicate the email address/Group to deliver the error details in Export Declaration."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil;
					return result;
				});
			}
		}

		public CodePairRegistryItem SendExportDeclarationErrorsTo
		{
			get
			{
				return GetItem("BRSendExportDeclarationErrorsTo", delegate
				{
					return new CodePairRegistryItem(
							"BRSendExportDeclarationErrorsTo",
							Categories.Customs_Brazil_Export,
							ResString.GetMultilingualString("6DCB038D-66D8-4C4C-8D4C-CD9FD72908B0", "Send Export Declaration Errors"),
							sendErrorMessageHint,
							OLookUpEditType.EmailTo,
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Core.Constants.EmailTo.StaffMember)
					{ CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil };
				});
			}
		}

		public BooleanRegistryItem EnableImportSiscomex
		{
			get
			{
				return GetItem("EnableBRImportSiscomex", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableBRImportSiscomex",
						Categories.Customs_Brazil_ImportSiscomex,
						ResString.GetMultilingualString("48F082D8-B6B4-4F86-AB47-E33CB51CD970", "Enable Import (SISCOMEX Web)"),
						ResString.GetMultilingualString("48F082D8-B6B4-4F86-AB47-E33CB51CD970", "Enable Import (SISCOMEX Web)"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
					return result;
				});
			}
		}

		bool IBRCustomsDataRegistry.EnableImportSiscomex => EnableImportSiscomex.Value;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's the name of the system")]
		public BooleanRegistryItem EnableMercanteSystem
		{
			get
			{
				var mercanteString = "Mercante";

				return GetItem("BREnableMercanteSystem", delegate
				{
					return new BooleanRegistryItem(
						"BREnableMercanteSystem",
						Categories.Customs_Brazil,
						ResString.GetMultilingualString("7046B4FE-2D25-4E90-B5AD-26743315C3FD", "Enable {0} System", mercanteString),
						ResString.GetMultilingualString("92D2318F-25CB-4AD5-A942-08C38592EDAD", "Enable {0} System?", mercanteString),
						RegistryStorageFlags.Company,
						false)
					{ CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil };
				});
			}
		}

		public ZBool IsMercanteSystemEnabled => EnableMercanteSystem.Value;

		public StringRegistryItem XTEndPointAddress
		{
			get
			{
				return GetItem("BRXTEndPointAddress", delegate
				{
					var result = new StringRegistryItem(
						"BRXTEndPointAddress",
						Categories.Customs_Brazil,
						ResString.GetMultilingualString("b5a28908-eeaa-4245-8b9b-a3b534ed0d5a", "xT Endpoint Address"),
						ResString.GetMultilingualString("ed9fc339-a139-434d-9ff8-7f9de6aa7089", "The default value uses the xT test endpoint address."),
						RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
						IsTestLicence ? "https://xttest-customs-brpushnotification.wisegrid.net/BRC" : "https://xt-customs-brpushnotification.wisegrid.net/BRC");
					return result;
				});
			}
		}

		public GuidRegistryItem SendLPCOErrorsToGroup
		{
			get
			{
				return GetItem("BRSendLPCOErrorsToGroup", delegate
				{
					var result = new GuidRegistryItem(
						"BRSendLPCOErrorsToGroup",
						Categories.Customs_Brazil_LPCO,
						ResString.GetMultilingualString("13f9b950-a7b4-4cce-a8f6-20a9cf8084a2", "Group To Send LPCO Errors To"),
						ResString.GetMultilingualString("9d2c0c3a-76bf-4dec-b17c-d40e597b21ba", "Indicate the email address/Group to deliver the error details in LPCO."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil;
					return result;
				});
			}
		}

		public CodePairRegistryItem SendLPCOErrorsTo
		{
			get
			{
				return GetItem("BRSendLPCOErrorsTo", delegate
				{
					return new CodePairRegistryItem(
							"BRSendLPCOErrorsTo",
							Categories.Customs_Brazil_LPCO,
							ResString.GetMultilingualString("a560998b-0f8e-4f30-b4d7-d82948e0b30b", "Send LPCO Errors"),
							sendErrorMessageHint,
							OLookUpEditType.EmailTo,
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Core.Constants.EmailTo.StaffMember)
					{ CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil };
				});
			}
		}

		public GuidRegistryItem SendImportDeclarationErrorsToGroup
		{
			get
			{
				return GetItem("BRSendImportDeclarationErrorsToGroup", delegate
				{
					var result = new GuidRegistryItem(
						"BRSendImportDeclarationErrorsToGroup",
						Categories.Customs_Brazil_Import,
						ResString.GetMultilingualString("e8f6c899-a5cc-4b91-b1df-1addb0072ea4", "Group To Send Import Declaration Errors To"),
						ResString.GetMultilingualString("0df8faf3-8ace-4d36-857c-baf5e132bc2b", "Indicate the email address/Group to deliver the error details in Import Declaration."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil;
					return result;
				});
			}
		}

		public CodePairRegistryItem SendImportDeclarationErrorsTo
		{
			get
			{
				return GetItem("BRSendImportDeclarationErrorsTo", delegate
				{
					return new CodePairRegistryItem(
							"BRSendImportDeclarationErrorsTo",
							Categories.Customs_Brazil_Import,
							ResString.GetMultilingualString("75857f51-0c8b-4317-9edf-3da3fa6db27b", "Send Import Declaration Errors"),
							sendErrorMessageHint,
							OLookUpEditType.EmailTo,
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Core.Constants.EmailTo.StaffMember)
					{ CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil };
				});
			}
		}

		public GuidRegistryItem SendProductCatalogErrorsToGroup
		{
			get
			{
				return GetItem("BRSendProductCatalogErrorsToGroup", delegate
				{
					var result = new GuidRegistryItem(
						"BRSendProductCatalogErrorsToGroup",
						Categories.Customs_Brazil_ProductCatalog,
						ResString.GetMultilingualString("021638f0-18ad-4c76-8724-71b5ea3a4a31", "Group To Send Product Catalog Errors To"),
						ResString.GetMultilingualString("ed2a1217-73aa-4b8a-82e2-5d4fc92b39da", "Indicate the email address/Group to deliver the error details in Product Catalog."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil;
					return result;
				});
			}
		}

		public CodePairRegistryItem SendProductCatalogErrorsTo
		{
			get
			{
				return GetItem("BRSendProductCatalogErrorsTo", delegate
				{
					return new CodePairRegistryItem(
							"BRSendProductCatalogErrorsTo",
							Categories.Customs_Brazil_ProductCatalog,
							ResString.GetMultilingualString("43dd5728-89d4-4337-9a2a-eff0a9604855", "Send Product Catalog Errors"),
							sendErrorMessageHint,
							OLookUpEditType.EmailTo,
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Core.Constants.EmailTo.StaffMember)
					{ CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil };
				});
			}
		}

		public CodePairRegistryItem SendSubscriptionErrorsTo
		{
			get
			{
				return GetItem("BRSendSubscriptionErrorsTo", delegate
				{
					return new CodePairRegistryItem(
							"BRSendSubscriptionErrorsTo",
							Categories.Customs_Brazil_Push,
							ResString.GetMultilingualString("88306729-2CF6-4559-87F7-4E386FA37844", "Send Subscription Errors"),
							sendErrorMessageHint,
							OLookUpEditType.EmailTo,
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							Core.Constants.EmailTo.StaffMember)
					{ CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil };
				});
			}
		}

		public GuidRegistryItem SendSubscriptionErrorsToGroup
		{
			get
			{
				return GetItem("BRSendSubscriptionErrorsToGroup", delegate
				{
					var result = new GuidRegistryItem(
						"BRSendSubscriptionErrorsToGroup",
						Categories.Customs_Brazil_Push,
						ResString.GetMultilingualString("B7AC151A-3F23-45D1-8B5B-5AAB58FA44C6", "Group To Send Subscription Errors To"),
						ResString.GetMultilingualString("30081AF6-578A-46B2-B45C-037AC282E4BC", "Indicate the email address/Group to deliver the error details in Subscription."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Brazil;
					return result;
				});
			}
		}

		readonly MultilingualString sendErrorMessageHint = ResString.GetMultilingualString("5274DE68-1112-44AB-A12A-897035CD0D5E", "Indicate the action to perform when failed message is detected.");

		public BooleanRegistryItem EnableCatalogModule
		{
			get
			{
				return GetItem("BREnableCatalogModule", delegate
				{
					return new BooleanRegistryItem(
						"BREnableCatalogModule",
						Categories.Customs_Brazil_ProductCatalog,
						ResString.GetMultilingualString("F3889D88-AF63-4D5A-BB7B-8C738738B867", "Enable Catalog"),
						ResString.GetMultilingualString("E44A6297-C56A-418C-93C7-F74663F87B10", "Enable Catalog Module?"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		bool IBRCustomsDataRegistry.EnableCatalogModule => EnableCatalogModule.Value;

		public IntRegistryItem MaxNumberOfRowsInForeignOperatorMessage
		{
			get
			{
				return GetItem("BRMaxNumberOfRowsInForeignOperatorMessage", delegate
				{
					var result = new IntRegistryItem(
						"BRMaxNumberOfRowsInForeignOperatorMessage",
						Categories.Customs_Brazil_ForeignOperator,
						MaximumNumberofMessagesCaption,
						OverrideTheDefaultValueHint,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						100,
						1,
						999);
					return result;
				});
			}
		}

		public IntRegistryItem MaxNumberOfEntryLineInDuimpMessage
		{
			get
			{
				return GetItem("BRMaxNumberOfEntryLineInDuimpMessage", delegate
				{
					var result = new IntRegistryItem(
						"BRMaxNumberOfEntryLineInDuimpMessage",
						Categories.Customs_Brazil_Import,
						ResString.GetMultilingualString("30184a41-2141-4a1c-8942-8ca7bbe9c26b", "Maximum Number of Entry Lines in the message"),
						OverrideTheDefaultValueHint,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						100,
						1,
						999);
					return result;
				});
			}
		}

		public IntRegistryItem MaxNumberOfRowsInProductCatalogMessage
		{
			get
			{
				return GetItem("BRMaxNumberOfRowsInProductCatalogMessage", delegate
				{
					var result = new IntRegistryItem(
						"BRMaxNumberOfRowsInProductCatalogMessage",
						Categories.Customs_Brazil_ProductCatalog,
						MaximumNumberofMessagesCaption,
						OverrideTheDefaultValueHint,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						100,
						1,
						999);
					return result;
				});
			}
		}

		public BillCustomisationRegistryItem CatalogCodeCustomization
		{
			get
			{
				return GetItem("BRCatalogCodeCustomization", delegate
				{
					var dataType = new UniqueNumberCustomisationRegistryDataType();
					dataType.GeneratedNumberName = ResString.GetMultilingualString("8E2E1C1A-AD96-4B90-8D74-E1890B9E0D7D", "Catalog Code");
					dataType.MaxLength = CusGoodsCatalog.Schema.CGC_CatalogCodeMaxLength;
					return new BillCustomisationRegistryItem(
						"BRCatalogCodeCustomization",
						Categories.Customs_Brazil_ProductCatalog,
						ResString.GetMultilingualString("12C4E739-6632-43DB-8BD5-249869E41B97", "Catalog Code Customization"),
						ResString.GetMultilingualString("39F9D430-F3A5-4EC1-8B89-E6A84F43B1E6", "Override this Registry Item to customize how the Catalog Codes are formatted."),
						RegistryStorageFlags.All,
						dataType);
				});
			}
		}

		public BillCustomisationRegistryItem LPCOJobNumberCustomization
		{
			get
			{
				return GetItem("BRLPCOJobNumberCustomization", delegate
				{
					var dataType = new UniqueNumberCustomisationRegistryDataType(false);
					dataType.GeneratedNumberName = ResString.GetMultilingualString("D85C7CD3-14EF-4C58-8982-F44333CCDC82", "LPCO Job Number");
					dataType.MaxLength = AutoCusPermitHeader.Schema.CPH_JobNumberMaxLength;
					return new BillCustomisationRegistryItem(
						"BRLPCOJobNumberCustomization",
						Categories.Customs_Brazil_LPCO,
						ResString.GetMultilingualString("B8AD7D12-CE7D-4C0C-967B-DEEBDDA14B10", "LPCO Job Number Customization"),
						ResString.GetMultilingualString("D59522D5-3A42-4F4F-B314-B0F152AA94FA", "Override this Registry Item to customize how the LPCO Job Numbers are formatted."),
						RegistryStorageFlags.All,
						dataType);
				});
			}
		}

		MultilingualString MaximumNumberofMessagesCaption => ResString.GetMultilingualString("C41092F7-70E9-40EC-A016-2BB16B13DFEA", "Maximum Number of Messages per Interchange");
		MultilingualString OverrideTheDefaultValueHint => ResString.GetMultilingualString("C0734B05-81C5-4E8F-A8DA-74085F351CA5", "Override the Default Value between 1 and 999");
	}
}
