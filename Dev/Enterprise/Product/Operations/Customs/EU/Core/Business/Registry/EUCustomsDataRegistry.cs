using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Customs.EU.Business.ResString;

namespace Enterprise.Customs.EU.Registry
{
	public sealed class EUCustomsDataRegistry : RegistryItemSet, Integration.Customs.EU.IEUCustomsRegistry
	{
		public EUCustomsDataRegistry()
		{
		}

		public override bool IsForProductivityWise => false;

		public static EUCustomsDataRegistry Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new EUCustomsDataRegistry();
				}
				return instance;
			}
		}
		[ThreadStatic]
		static EUCustomsDataRegistry instance;

		public BooleanRegistryItem DefaultDV1
		{
			get
			{
				return GetItem("DefaultDV1Value", delegate
				{
					return new BooleanRegistryItem(
						"DefaultDV1Value",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_Import,
						Enterprise.Customs.EU.Business.ResString.GetMultilingualString("6BF90374-7873-419F-8AEF-B1F1939E683B", "Enable Default DV.1"),
						ResString.GetMultilingualString("9043E212-F898-43A7-AF81-48B2EBAA94F9", "When set to 'Yes', D.V.1 in import declarations will be ticked by default."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem ExportOldSuportingInfoSchemaInUniversalXML
		{
			get
			{
				return GetItem("ExportOldSuportingInfoSchemaInUniversalXML", delegate
				{
					return new BooleanRegistryItem(
						"ExportOldSuportingInfoSchemaInUniversalXML",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("FF28EEF0-D723-4C9F-B52D-D4004C2D4D96", "Show Old Supporting Info Schema."),
						ResString.GetMultilingualString("1F33D328-2D7B-4A77-8AAD-75A5BD03F51B", "When set to 'Yes' the old Supporting Info Schema will be included in Declaration Universal XML export."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem CheckChangeMessageTypeFromSupplierOrImporter
		{
			get
			{
				return GetItem("CheckChangeMessageTypeFromSupplierOrImporter", delegate
				{
					BooleanRegistryItem result = new BooleanRegistryItem(
						"CheckChangeMessageTypeFromSupplierOrImporter",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("D327E3E1-8E31-445A-8C18-C224056AAFD1", "Warn Change in Flux After Changing Supplier/Importer in Declaration."),
						ResString.GetMultilingualString("681E9444-7D24-4510-ABAD-20A57011030A", "When set to 'Yes', warn user a change in flux (entry type - import/export/etc.) will happen after changing suppler/importer."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						RegistryOptions.Default,
						false);
					result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction;
					return result;
				});
			}
		}

		public BooleanRegistryItem SADGenerationOnClearanceEnabled
		{
			get
			{
				return GetItem("SADGenerationOnClearanceEnabled", delegate
				{
					var result = new BooleanRegistryItem(
						"SADGenerationOnClearanceEnabled",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("8F0F1F8F-AAD4-4C54-B0EC-1564E408F5DE", "SAD Generation on Clearance Enabled."),
						ResString.GetMultilingualString("BFD1CE87-0288-441E-AB99-7B5217AE2A9B", "Set to YES to enable SAD Generation on Clearance."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false
					);
					return result;
				});
			}
		}

		public BooleanRegistryItem AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee
		{
			get
			{
				return GetItem("AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee", delegate
				{
					return new BooleanRegistryItem(
						"AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee",
						RawDataRegistry.Categories.Customs,
						ResString.GetMultilingualString("6DDB09AC-15E8-45C1-80FC-0FE61E56921A", "Allow {0} data import and export via Universal XML", "CusAddInfo"),
						ResString.GetMultilingualString("06CE49E9-28B2-4460-B97B-7C288F5716F6", "Set to YES to allow old-style {0} key-value-pair data to be imported and exported via Universal XML. Set to NO to require that this data is only loaded from Universal XML via the new structure, {1}. Support only.", "CusAddInfo", "TaxOrFees"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public ImageRegistryItem SADLogo
		{
			get
			{
				return GetItem("SADLogo", delegate
				{
					return new ImageRegistryItem(
						"SADLogo",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("B1524B07-C717-4D74-AE07-0C6D0D860105", "SAD Logo"),
						ResString.GetMultilingualString("05C33AC4-2DFA-414B-951E-601A2BF5CA95", "Image that appears as a logo in SAD documents."),
						RegistryStorageFlags.Company);
				});
			}
		}

		public StringRegistryItem QuotaBalanceURL
		{
			get
			{
				return GetItem("QuotaBalanceURL", delegate
				{
					var item = new StringRegistryItem(
						"QuotaBalanceURL",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("CA94D502-5FB6-458D-89F3-1EE8BFA2D141", "Quota Balance URL"),
						ResString.GetMultilingualString("3C444262-20AD-4143-9646-618DFA749BCE", "URL for check of Quota Balance."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"https://ec.europa.eu/taxation_customs/dds2/taric/quota_consultation.jsp");
					var uriDataType = new UriRegistryDataType(Uri.UriSchemeHttps);
					uriDataType.AllowQueryString = false;
					item.DataType = uriDataType;

					return item;
				});
			}
		}
		public BillCustomisationRegistryItem LimitedFiscalRepresentationNumberCustomisation
		{
			get
			{
				return GetItem("LimitedFiscalRepresentationNumberCustomisation", delegate
				{
					return new BillCustomisationRegistryItem(
						"LimitedFiscalRepresentationNumberCustomisation",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_FiscalRepresentation,
						ResString.GetMultilingualString("4E0D0401-6627-4387-850A-AC179BFCA3A6", "LFR Number Customization"),
						ResString.GetMultilingualString("1D9E0F6E-2A8F-466F-990A-973885C892C3", "Override this value to customize how limited fiscal representation numbers are formatted"),
						RegistryStorageFlags.Company,
						new LimitedFiscalRepresentationNumberCustomisationRegistryDataType());
				});
			}
		}

		public CodePairRegistryItem LimitedFiscalRepresentationDefaultReportingPeriodType
		{
			get
			{
				return GetItem("LimitedFiscalRepresentationDefaultReportingPeriodType", delegate
				{
					return new CodePairRegistryItem(
						"LimitedFiscalRepresentationDefaultReportingPeriodType",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_FiscalRepresentation,
						ResString.GetMultilingualString("6C80377F-2B84-4CD0-955C-DB4DC1C8413C", "LFR Default Reporting Period Type"),
						ResString.GetMultilingualString("55356C6E-E363-4B45-AA36-C95AF6BDEFBF", "Override this value to customize the limited fiscal representation reporting period type"),
						new CodeDescriptionPairListProvider(() => new LFRReportingPeriodType()),
						RegistryStorageFlags.Company,
						LFRReportingPeriodType.Codes.Monthly);
				});
			}
		}

		public BooleanRegistryItem ExportAuthorizedLocationCheck
		{
			get
			{
				return GetItem("ExportAuthorizedLocationCheck", delegate
				{
					return new BooleanRegistryItem(
						"ExportAuthorizedLocationCheck",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_Export,
						ResString.GetMultilingualString("3F5B0382-2AD3-427A-A92F-FBA8936BE7CD", "Authorized Location Check"),
						ResString.GetMultilingualString("E66B4E25-2639-4357-A09B-7E571555F32E", "When a goods location in an Export declaration isn't set to 'Yes' as Authorized location submitting will be blocked with an error"),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem NctsAuthorizedLocationCheck
		{
			get
			{
				return GetItem("NctsAuthorizedLocationCheck", delegate
				{
					return new BooleanRegistryItem(
						"NctsAuthorizedLocationCheck",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
						ResString.GetMultilingualString("C4E04705-2591-4D2E-9620-421B672F6C98", "Authorized Location Check"),
						ResString.GetMultilingualString("88EACE81-B4CA-49EC-ABE4-B491246D6A68", "When a goods location in an NCTS declaration isn't set to 'Yes' as Authorized location submitting will be blocked with an error"),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem NctsIsManualDepartureCustomerReferenceEnabled
		{
			get
			{
				return GetItem("NctsIsManualCustomerReferenceEnabled", delegate
				{
					return new BooleanRegistryItem(
						"NctsIsManualCustomerReferenceEnabled",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
						ResString.GetMultilingualString("AFDABA81-1EDC-470B-A28A-0D16D530734F", "Enable Manual Departure Customer Reference"),
						ResString.GetMultilingualString("EA740213-C80E-49F6-BED1-AD1CAE9A47BA", "Set this to enable manual Departure Customer Reference Number (LRN) entry instead of auto-generating it. When set to 'Yes', it is necessary to supply a reference number in imported Universal Shipment XML files; when set to 'No', the customer reference is automatically generated even for jobs created from Universal Shipment XML."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem NctsIsManualArrivalCustomerReferenceEnabled
		{
			get
			{
				return GetItem("NctsIsManualArrivalCustomerReferenceEnabled", delegate
				{
					return new BooleanRegistryItem(
						new BooleanCountryDependingRegistryItem(
							"NctsIsManualArrivalCustomerReferenceEnabled",
							RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
							ResString.GetMultilingualString("C9F454EF-4805-4267-AB6D-D505E16CFFC1", "Enable Manual Arrival Customer Reference"),
							ResString.GetMultilingualString("C130F459-CA80-43F4-B5B9-00281FECE9A1", "Set this to enable manual Arrival Customer Reference Number entry instead of auto-generating it."),
							RegistryStorageFlags.Company,
							true,
							new string[] { Core.Constants.CountryCodes.Switzerland }));
				});
			}
		}

		public NctsDefaultPrincipalRegistryItem NCTSDefaultPrincipal
		{
			get
			{
				return GetItem("NCTSDefaultPrincipal", delegate
				{
					return new NctsDefaultPrincipalRegistryItem(
						"NCTSDefaultPrincipal",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
						ResString.GetMultilingualString("B5FBB6BC-A113-4866-AFBF-D185CFA9CB4B", "Default Principal"),
						ResString.GetMultilingualString("5105785C-5A1A-4380-9710-B26B99ED0140", "Tick 'Leave Blank' if you do not want a Principal to be suggested else please select an organization to be suggested as Principal"),
						RegistryStorageFlags.Company,
						new NctsDefaultPrincipal());
				});
			}
		}

		public NctsDefaultTraderAtDestinationRegistryItem NCTSDefaultTraderAtDestination
		{
			get
			{
				return GetItem("NCTSDefaultTraderAtDestination", delegate
				{
					return new NctsDefaultTraderAtDestinationRegistryItem(
						"NCTSDefaultTraderAtDestination",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
						ResString.GetMultilingualString("F25DD5F3-955A-4EC8-A2C5-BD5F0FA5D6E8", "Default Trader at Destination"),
						ResString.GetMultilingualString("6CB6BFD6-4CD3-4D1B-A400-0FD590E0FCE6", "Tick 'Leave Blank' if you do not want a Trader at Destination to be suggested else please select an organization to be suggested as Trader at Destination for an Arrival Notification."),
						RegistryStorageFlags.Company,
						new NctsDefaultTraderAtDestination());
				});
			}
		}

		public NctsDefaultConsignorConsigneeRegistryItem NctsDefaultConsignorConsignee
		{
			get
			{
				return GetItem("NctsDefaultConsignorConsignee", delegate
				{
					return new NctsDefaultConsignorConsigneeRegistryItem(
						"NctsDefaultConsignorConsignee",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
						ResString.GetMultilingualString("B051793F-4AF1-42DC-89F5-87AF8AB42E48", "Default Consignor and Consignee"),
						ResString.GetMultilingualString("434A86D6-6FAC-4B23-901E-17C7AC8EFE8C", "Do not override if you want Consignee AND Consignor to be copied from shipment or consol. Tick 'Leave Blank' if you do not want Consignor and Consignee to be copied from consol or shipment or tick 'take values from...' if you want to choose which one to copy."),
						RegistryStorageFlags.Company,
						new NctsDefaultConsignorConsignee());
				});
			}
		}

		public BooleanRegistryItem SyncTransportDetailsFromForwardingToNCTS
		{
			get
			{
				return GetItem("SyncTransportDetailsFromForwardingToNCTS", delegate
				{
					return new BooleanRegistryItem(
						"SyncTransportDetailsFromForwardingToNCTS",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
						ResString.GetMultilingualString("CA7FC4F4-7F99-457C-ACCB-296886A5F264", "Synchronize transport details from forwarding to NCTS for all modes"),
						ResString.GetMultilingualString("3AD9C4C6-7679-46F6-8FE6-12B403D57364", "If set to false, transport details are only copied from the Forwarding record for mode ROA(d). If set to true, transport details are copied for all modes"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public GroupNotificationRegistryItem<NctsGroupNotification> SendNctsErrors =>
			GetItem("SendNctsErrorsTo", () =>
			{
				var result = new GroupNotificationRegistryItem<NctsGroupNotification>(
					"SendNctsErrorsTo",
					RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
					ResString.GetMultilingualString("004fa68f-fba4-4c6c-bec5-f66367c18a9f", "Send NCTS Errors To"),
					ResString.GetMultilingualString("da31383f-fffe-4080-b3c2-87c1b5bb2b3d", "Send NCTS errors to staff member, nominated group or both"),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					new NctsGroupNotification(Constants.EmailTo.StaffMember, ZGuid.Empty));
				result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey;
				return result;
			});

		public GroupNotificationRegistryItem<NctsGroupNotification> SendNctsAcknowledgements =>
			GetItem("SendNctsAcknowledgementsTo", () =>
			{
				var result = new GroupNotificationRegistryItem<NctsGroupNotification>(
					"SendNctsAcknowledgementsTo",
					RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
					ResString.GetMultilingualString("6a1ae7d19-43e0-4d32-9c80-0ccb45a2aedb", "Send NCTS Acknowledgements To"),
					ResString.GetMultilingualString("b8f137e9-d300-424e-80aa-dcdedd6d710c", "Send NCTS acknowledgements to staff member, nominated group or both"),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					new NctsGroupNotification(Constants.EmailTo.StaffMember, ZGuid.Empty));
				result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey;
				return result;
			});

		public BooleanRegistryItem EnableAutoTariffDescriptionPopulation
		{
			get
			{
				return GetItem("NCTSEnableAutoTariffDescriptionPopulation", delegate
				{
					return new BooleanRegistryItem(
						"NCTSEnableAutoTariffDescriptionPopulation",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
						ResString.GetMultilingualString("E2A3EAFB-85A9-4CB0-91AD-B2C9E9BE02D2", "Enable Auto Population of Goods Description in NCTS"),
						ResString.GetMultilingualString("E2A3EAFB-85A9-4CB0-91AD-B2C9E9BE02D2", "Enable Auto Population of Goods Description in NCTS"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.CannotCallParameterlessValueGetter,
						true);
				});
			}
		}

		public GroupNotificationRegistryItem<NctsGroupNotification> SendGuaranteeNotifications =>
			GetItem("SendGuaranteeNotificationsTo", () =>
			{
				var result = new GroupNotificationRegistryItem<NctsGroupNotification>(
					"SendGuaranteeNotificationsTo",
					RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
					ResString.GetMultilingualString("7E112E14-AC52-4429-8F1B-78BEB6B76DA2", "Send Guarantee Notifications To"),
					ResString.GetMultilingualString("5A5A933A-1363-4E35-AFD5-DD299933228A", "Send Guarantee Notifications to staff member, nominated group or both"),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					new NctsGroupNotification(Constants.EmailTo.StaffMember, ZGuid.Empty));
				result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey;
				return result;
			});

		IRegistryItem Integration.Customs.EU.IEUCustomsRegistry.AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee => AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee;

		public BooleanRegistryItem PNTSEnabledDeveloperOnly
		{
			get
			{
				return GetItem("TemporaryStorageEnabled", delegate
				{
					return new BooleanRegistryItem(
						"TemporaryStorageEnabled",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("A4F70286-1425-4A1B-9EB7-A51A7BF1E438", "PNTS Enabled (Developer Only)"),
						ResString.GetMultilingualString("D5906EDC-9254-448B-895A-5B16B429AFC7", "Set this to true to enable Temporary Storage UCC6 module."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem PNTSEnabled
		{
			get
			{
				return GetItem("PNTSEnabled", delegate
				{
					var result = new BooleanRegistryItem(
						"PNTSEnabled",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("9FCDCFEF-B733-450D-A4F8-3E4C07E16CBF", "PNTS Enabled"),
						ResString.GetMultilingualString("6C80229F-908A-44CB-B92C-985C829D1016", "Set this to true to enable Temporary Storage UCC6 module."),
						RegistryStorageFlags.Company,
						false);

					result.CountryFilterPKs = CountryFilterPKs.FranceAndOverseasDepartments.Union(CountryFilterPKs.Spain);
					return result;
				});
			}
		}

		public BooleanRegistryItem RegisterEnabledDeveloperOnly
		{
			get
			{
				return GetItem("RegisterEnabledDeveloperOnly", delegate
				{
					return new BooleanRegistryItem(
						"RegisterEnabledDeveloperOnly",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("AADF38E4-C5F9-4E76-BDA2-0568C158FAAA", "Register Enabled (Developer Only)"),
						ResString.GetMultilingualString("CAF700F8-7823-49F7-93F4-4A6BFDAB10AB", "Set this to true to enable Temporary Storage Register module."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem RegisterEnabled
		{
			get
			{
				return GetItem("RegisterEnabled", delegate
				{
					var result = new BooleanRegistryItem(
						"RegisterEnabled",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("F06FA49E-C643-45E9-B2A1-74C19B24F831", "Register Enabled"),
						ResString.GetMultilingualString("451B955F-9B3B-48E4-9138-F4CA9B285ED8", "Set this to true to enable Temporary Storage Register module."),
						RegistryStorageFlags.Company,
						false);

					result.CountryFilterPKs = CountryFilterPKs.Spain;
					return result;
				});
			}
		}

		public GroupNotificationRegistryItem<TemporaryStorageGroupNotification> SendTemporaryStorageErrors =>
			GetItem("SendTemporaryStorageErrorsRegistry", () =>
			{
				var result = new GroupNotificationRegistryItem<TemporaryStorageGroupNotification>(
					"SendTemporaryStorageErrorsRegistry",
					RawDataRegistry.Categories.Customs_EuropeanUnionCommon_TemporaryStorage,
					ResString.GetMultilingualString("F710C99F-A0F7-4440-B5F0-F10D833522BC", "Send Errors To"),
					ResString.GetMultilingualString("C9FD5930-A109-4124-9EA1-BB5372077EF9", "Send Temporary Storage errors to staff member, nominated group or both"),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					new TemporaryStorageGroupNotification(Constants.EmailTo.StaffMember, ZGuid.Empty));
				result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction;
				return result;
			});

		public GroupNotificationRegistryItem<TemporaryStorageGroupNotification> SendTemporaryStorageAcknowledgements =>
			GetItem("SendTemporaryStorageAcknowledgementsRegistry", () =>
			{
				var result = new GroupNotificationRegistryItem<TemporaryStorageGroupNotification>(
					"SendTemporaryStorageAcknowledgementsRegistry",
					RawDataRegistry.Categories.Customs_EuropeanUnionCommon_TemporaryStorage,
					ResString.GetMultilingualString("69C7811F-2C34-4906-AEF2-F4D7CE2E46EE", "Send Acknowledgements To"),
					ResString.GetMultilingualString("F9546C07-5E55-4141-A1C7-BB7E47CC27FB", "Send Temporary Storage acknowledgements to staff member, nominated group or both"),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					new TemporaryStorageGroupNotification(Constants.EmailTo.StaffMember, ZGuid.Empty));
				result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction;
				return result;
			});

		public GroupNotificationRegistryItem<TemporaryStorageUnsolicitedGroupNotification> SendTemporaryStorageUnsolicited =>
			GetItem("SendTemporaryStorageUnsolicitedRegistry", () =>
			{
				var result = new GroupNotificationRegistryItem<TemporaryStorageUnsolicitedGroupNotification>(
					"SendTemporaryStorageUnsolicitedRegistry",
					RawDataRegistry.Categories.Customs_EuropeanUnionCommon_TemporaryStorage,
					ResString.GetMultilingualString("96F1D25F-7C06-4C3C-AA70-17D821940236", "Send Unsolicited To"),
					ResString.GetMultilingualString("C7660110-18EA-4969-80FF-BB962BE863E3", "Send Temporary Storage unsolicited to nominated group"),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					new TemporaryStorageUnsolicitedGroupNotification(Constants.EmailTo.NoEmails, ZGuid.Empty));
				result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction;
				return result;
			});

		IRegistryItem Integration.Customs.EU.IEUCustomsRegistry.PNTSEnabledDeveloperOnly => PNTSEnabledDeveloperOnly;

		IRegistryItem Integration.Customs.EU.IEUCustomsRegistry.PNTSEnabled => PNTSEnabled;

		IRegistryItem Integration.Customs.EU.IEUCustomsRegistry.RegisterEnabledDeveloperOnly => RegisterEnabledDeveloperOnly;

		IRegistryItem Integration.Customs.EU.IEUCustomsRegistry.RegisterEnabled => RegisterEnabled;

		public GroupNotificationRegistryItem<ExportGroupNotification> SendExportMessageErrors
		{
			get
			{
				return GetItem("SendExportMessageErrorsRegistry", () =>
				{
					var result = new GroupNotificationRegistryItem<ExportGroupNotification>("SendExportMessageErrorsRegistry",
						CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_Export,
						ResString.GetMultilingualString("E5A4DA8B-B426-4DC5-861E-345066F3091D", "Send Export Errors To"),
						ResString.GetMultilingualString("CCBB3D7C-7E3D-4578-8EF3-60F965349FBE", "Send Export message errors to staff member, nominated group or both"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						new ExportGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));
					result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction;
					return result;
				});
			}
		}

		public GroupNotificationRegistryItem<ExportGroupNotification> SendExportAcknowledgements
		{
			get
			{
				return GetItem("SendExportAcknowledgementsRegistry", () =>
				{
					var result = new GroupNotificationRegistryItem<ExportGroupNotification>("SendExportAcknowledgementsRegistry",
						CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_Export,
						ResString.GetMultilingualString("D6A7A86F-E0B3-4AC6-962D-B535F728CEDE", "Send Export Acknowledgements To"),
						ResString.GetMultilingualString("5EE21656-77D8-4840-AE8E-503A82BD378B", "Send Export acknowledgements to staff member, nominated group or both"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						new ExportGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty));
					result.CountryFilterPKs = Core.CountryGuids.CountriesUnderEUCustomsJurisdiction;
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableCentralizedClearanceForImport
		{
			get
			{
				return GetItem("EnableCentralizedClearanceForImport", delegate
				{
					return new BooleanRegistryItem(
						"EnableCentralizedClearanceForImport",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon_Import,
						ResString.GetMultilingualString("81C39AE1-CFB9-4FA2-9827-0329271050DE", "Enable Centralized Clearance for Import (CCI)"),
						ResString.GetMultilingualString("81B460EE-9417-4A0A-9D4D-DA60DF012FBD", "When set to 'Yes', Centralized Clearance for Import (CCI) related functionalities will be enabled."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem ForbidMergingInvoiceLinesWithRatioBasedTariff
		{
			get
			{
				return GetItem("ForbidMergingInvoiceLinesWithRatioBasedTariff", delegate
				{
					return new BooleanRegistryItem(
						"ForbidMergingInvoiceLinesWithRatioBasedTariff",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						ResString.GetMultilingualString("3E0785F2-D26D-4F11-B5E3-16C86D182631", "Forbid merging of invoice lines with ratio-based tariff ‘CLASS’ conditions"),
						ResString.GetMultilingualString("12060C4A-1347-4B96-9097-534F0DD85DB2", "When set to 'Yes', merging of invoice lines with ratio-based tariff ‘CLASS’ conditions is not allowed."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}
	}
}
