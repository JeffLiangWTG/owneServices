using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Environment.RegistryItemSet;

namespace Enterprise.Customs.EU.Registry.Testing
{
	[TestedType(typeof(EUCustomsDataRegistry))]
	sealed class EUCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<EUCustomsDataRegistry>
	{
		public void TestSendNctsErrors() => CombineAssertions(() =>
		{
			var registryItem = EUCustomsDataRegistry.Instance.SendNctsErrors;
			AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<NctsGroupNotification>), registryItem.GetType());
			TestGenericRegistryItem(registryItem,
				"SendNctsErrorsTo",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Send NCTS Errors To",
				"Send NCTS errors to staff member, nominated group or both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
			AssertSequencesEqual("CountryFilterPKs", Core.CountryGuids.CountriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey, registryItem.CountryFilterPKs);
		});

		public void TestSendNctsAcknowledgements() => CombineAssertions(() =>
		{
			var registryItem = EUCustomsDataRegistry.Instance.SendNctsAcknowledgements;
			AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<NctsGroupNotification>), registryItem.GetType());
			TestGenericRegistryItem(registryItem,
				"SendNctsAcknowledgementsTo",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Send NCTS Acknowledgements To",
				"Send NCTS acknowledgements to staff member, nominated group or both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
			AssertSequencesEqual("CountryFilterPKs", Core.CountryGuids.CountriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey, registryItem.CountryFilterPKs);
		});

		public void TestSendGuaranteeNotifications() => CombineAssertions(() =>
		{
			var registryItem = EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
			AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<NctsGroupNotification>), registryItem.GetType());
			TestGenericRegistryItem(registryItem,
				"SendGuaranteeNotificationsTo",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Send Guarantee Notifications To",
				"Send Guarantee Notifications to staff member, nominated group or both",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
			AssertSequencesEqual("CountryFilterPKs", Core.CountryGuids.CountriesUnderEUCustomsJurisdictionPlusSwissNorwayTurkey, registryItem.CountryFilterPKs);
		});

		public void TestPNTSEnabledDeveloperOnly()
		{
			TestRegistryItem(ItemSet.PNTSEnabledDeveloperOnly,
				"TemporaryStorageEnabled",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
				"PNTS Enabled (Developer Only)",
				"Set this to true to enable Temporary Storage UCC6 module.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestPNTSEnabled()
		{
			TestRegistryItem(ItemSet.PNTSEnabled,
				"PNTSEnabled",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
				"PNTS Enabled",
				"Set this to true to enable Temporary Storage UCC6 module.",
				RegistryStorageFlags.Company,
				false);
			var registryItem = EUCustomsDataRegistry.Instance.PNTSEnabled;
			AssertSequencesEqual("CountryFilterPKs", CountryFilterPKs.FranceAndOverseasDepartments.Union(CountryFilterPKs.Spain), registryItem.CountryFilterPKs);
		}

		public void TestRegisterEnabledDeveloperOnly()
		{
			TestRegistryItem(ItemSet.RegisterEnabledDeveloperOnly,
				"RegisterEnabledDeveloperOnly",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
				"Register Enabled (Developer Only)",
				"Set this to true to enable Temporary Storage Register module.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestRegisterEnabled()
		{
			TestRegistryItem(ItemSet.RegisterEnabled,
				"RegisterEnabled",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
				"Register Enabled",
				"Set this to true to enable Temporary Storage Register module.",
				RegistryStorageFlags.Company,
				false);
			var registryItem = EUCustomsDataRegistry.Instance.RegisterEnabled;
			AssertSequencesEqual("CountryFilterPKs", CountryFilterPKs.Spain, registryItem.CountryFilterPKs);
		}

		public void TestSendTemporaryStorageErrors()
		{
			var registryItem = EUCustomsDataRegistry.Instance.SendTemporaryStorageErrors;
			CombineAssertions(() =>
			{
				AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<TemporaryStorageGroupNotification>), registryItem.GetType());
				TestGenericRegistryItem(registryItem,
					"SendTemporaryStorageErrorsRegistry",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_TemporaryStorage,
					"Send Errors To",
					"Send Temporary Storage errors to staff member, nominated group or both",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
				AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
				AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
				AssertSequencesEqual("CountryFilterPKs", Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, registryItem.CountryFilterPKs);
			});
		}

		public void TestSendTemporaryStorageAcknowledgements()
		{
			var registryItem = EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements;
			CombineAssertions(() =>
			{
				AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<TemporaryStorageGroupNotification>), registryItem.GetType());
				TestGenericRegistryItem(registryItem,
					"SendTemporaryStorageAcknowledgementsRegistry",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_TemporaryStorage,
					"Send Acknowledgements To",
					"Send Temporary Storage acknowledgements to staff member, nominated group or both",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
				AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, registryItem.DefaultValue.SendMode);
				AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
				AssertSequencesEqual("CountryFilterPKs", Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, registryItem.CountryFilterPKs);
			});
		}

		public void TestSendTemporaryStorageUnsolicited()
		{
			var registryItem = EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited;
			CombineAssertions(() =>
			{
				AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<TemporaryStorageUnsolicitedGroupNotification>), registryItem.GetType());
				TestGenericRegistryItem(registryItem,
					"SendTemporaryStorageUnsolicitedRegistry",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_TemporaryStorage,
					"Send Unsolicited To",
					"Send Temporary Storage unsolicited to nominated group",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
				AssertEquals("Default Send To", Core.Constants.EmailTo.NoEmails, registryItem.DefaultValue.SendMode);
				AssertEquals("Default Group", ZGuid.Empty, registryItem.DefaultValue.SendGroupPK);
				AssertSequencesEqual("CountryFilterPKs", Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, registryItem.CountryFilterPKs);
			});
		}

		public void TestSyncFromForwardingToNCTS()
		{
			TestRegistryItem(ItemSet.SyncTransportDetailsFromForwardingToNCTS,
				"SyncTransportDetailsFromForwardingToNCTS",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Synchronize transport details from forwarding to NCTS for all modes",
				"If set to false, transport details are only copied from the Forwarding record for mode ROA(d). If set to true, transport details are copied for all modes",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestDefaultDV1()
		{
			TestRegistryItem(ItemSet.DefaultDV1,
						"DefaultDV1Value",
						CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_Import,
						"Enable Default DV.1",
						"When set to 'Yes', D.V.1 in import declarations will be ticked by default.",
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
		}

		public void TestExportOldSuportingInfoSchemaInUniversalXML()
		{
			TestRegistryItem(ItemSet.ExportOldSuportingInfoSchemaInUniversalXML,
						"ExportOldSuportingInfoSchemaInUniversalXML",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						"Show Old Supporting Info Schema.",
						"When set to 'Yes' the old Supporting Info Schema will be included in Declaration Universal XML export.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
		}

		public void TestCheckChangeMessageTypeFromSupplierOrImporter()
		{
			TestRegistryItem(ItemSet.CheckChangeMessageTypeFromSupplierOrImporter,
				"CheckChangeMessageTypeFromSupplierOrImporter",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
				"Warn Change in Flux After Changing Supplier/Importer in Declaration.",
				"When set to 'Yes', warn user a change in flux (entry type - import/export/etc.) will happen after changing suppler/importer.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
				RegistryOptions.Default,
				false);

			AssertContainsExactElementsInAnyOrder(Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, ItemSet.CheckChangeMessageTypeFromSupplierOrImporter.CountryFilterPKs);
		}

		public void TestSADGenerationOnClearanceEnabled()
		{
			TestGenericRegistryItem(ItemSet.SADGenerationOnClearanceEnabled,
				"SADGenerationOnClearanceEnabled",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
				"SAD Generation on Clearance Enabled.",
				"Set to YES to enable SAD Generation on Clearance.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestAllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee()
		{
			TestRegistryItem(ItemSet.AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee,
				"AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee",
				RawDataRegistry.Categories.Customs,
				"Allow CusAddInfo data import and export via Universal XML",
				"Set to YES to allow old-style CusAddInfo key-value-pair data to be imported and exported via Universal XML. Set to NO to require that this data is only loaded from Universal XML via the new structure, TaxOrFees. Support only.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestSADLogo()
		{
			TestRegistryItem(ItemSet.SADLogo,
						"SADLogo",
						RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
						"SAD Logo",
						"Image that appears as a logo in SAD documents.",
						RegistryStorageFlags.Company,
						RegistryOptions.Default);
		}

		public void TestCheckQuotaUrl()
		{
			TestRegistryItem(ItemSet.QuotaBalanceURL,
							 "QuotaBalanceURL",
							 RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
							 "Quota Balance URL",
							 "URL for check of Quota Balance.",
							 RegistryStorageFlags.System,
							 RegistryOptions.Default,
							 TextEditorType.TextBox,
							 "https://ec.europa.eu/taxation_customs/dds2/taric/quota_consultation.jsp",
							 "https://ec.europa.eu/taxation_customs/dds2/taric/quota_consultation.jsp");
		}

		public void TestLimitedFiscalRepresentationNumberCustomisation()
		{
			TestGenericRegistryItem(ItemSet.LimitedFiscalRepresentationNumberCustomisation, "LimitedFiscalRepresentationNumberCustomisation", CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_FiscalRepresentation, "LFR Number Customization", "Override this value to customize how limited fiscal representation numbers are formatted", RegistryStorageFlags.Company);
			var dataType = (BillCustomisationRegistryDataType)ItemSet.LimitedFiscalRepresentationNumberCustomisation.DataType;
			AssertEquals("GeneratedNumberName", "LFR Number", dataType.GeneratedNumberName);
			AssertEquals("MaxLength", 35, dataType.MaxLength);
			AssertEquals("PrefixLength", 3, dataType.PrefixLength);
		}

		public void TestLimitedFiscalRepresentationDefaultReportingPeriodType()
		{
			TestRegistryItem(ItemSet.LimitedFiscalRepresentationDefaultReportingPeriodType,
				"LimitedFiscalRepresentationDefaultReportingPeriodType",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_FiscalRepresentation,
				"LFR Default Reporting Period Type",
				"Override this value to customize the limited fiscal representation reporting period type",
				RegistryStorageFlags.Company,
				new LFRReportingPeriodType(),
				LFRReportingPeriodType.Codes.Monthly);
		}

		public void TestExportAuthorizedLocationCheck()
		{
			TestRegistryItem(ItemSet.ExportAuthorizedLocationCheck,
				"ExportAuthorizedLocationCheck",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_Export,
				"Authorized Location Check",
				"When a goods location in an Export declaration isn't set to 'Yes' as Authorized location submitting will be blocked with an error",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestNctsAuthorizedLocationCheck()
		{
			TestRegistryItem(ItemSet.NctsAuthorizedLocationCheck,
				"NctsAuthorizedLocationCheck",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Authorized Location Check",
				"When a goods location in an NCTS declaration isn't set to 'Yes' as Authorized location submitting will be blocked with an error",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				false);
		}

		public void TestNctsIsManualDepartureCustomerReferenceEnabled()
		{
			TestRegistryItem(ItemSet.NctsIsManualDepartureCustomerReferenceEnabled,
				"NctsIsManualCustomerReferenceEnabled",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Enable Manual Departure Customer Reference",
				"Set this to enable manual Departure Customer Reference Number (LRN) entry instead of auto-generating it. When set to 'Yes', it is necessary to supply a reference number in imported Universal Shipment XML files; when set to 'No', the customer reference is automatically generated even for jobs created from Universal Shipment XML.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				expectedDefaultValue: false);
		}

		public void TestNctsIsManualArrivalCustomerReferenceEnabled()
		{
			TestRegistryItem(ItemSet.NctsIsManualArrivalCustomerReferenceEnabled,
				"NctsIsManualArrivalCustomerReferenceEnabled",
				RawDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Enable Manual Arrival Customer Reference",
				"Set this to enable manual Arrival Customer Reference Number entry instead of auto-generating it.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				true);
		}

		public void TestNCTSDefaultPrincipal()
		{
			TestGenericRegistryItem(ItemSet.NCTSDefaultPrincipal,
				"NCTSDefaultPrincipal",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Default Principal",
				"Tick 'Leave Blank' if you do not want a Principal to be suggested else please select an organization to be suggested as Principal",
				RegistryStorageFlags.Company);
		}

		public void TestNCTSDefaultTraderAtDestination()
		{
			TestGenericRegistryItem(ItemSet.NCTSDefaultTraderAtDestination,
				"NCTSDefaultTraderAtDestination",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Default Trader at Destination",
				"Tick 'Leave Blank' if you do not want a Trader at Destination to be suggested else please select an organization to be suggested as Trader at Destination for an Arrival Notification.",
				RegistryStorageFlags.Company);
		}

		public void TestNctsDefaultConsignorAndConsignee()
		{
			TestGenericRegistryItem(ItemSet.NctsDefaultConsignorConsignee,
				"NctsDefaultConsignorConsignee",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Default Consignor and Consignee",
				"Do not override if you want Consignee AND Consignor to be copied from shipment or consol. Tick 'Leave Blank' if you do not want Consignor and Consignee to be copied from consol or shipment or tick 'take values from...'" +
				" if you want to choose which one to copy.",
				RegistryStorageFlags.Company);
		}

		public void TestEnableAutoTariffDescriptionPopulation()
		{
			TestGenericRegistryItem(ItemSet.EnableAutoTariffDescriptionPopulation,
				"NCTSEnableAutoTariffDescriptionPopulation",
				CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_NCTS,
				"Enable Auto Population of Goods Description in NCTS",
				"Enable Auto Population of Goods Description in NCTS",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.CannotCallParameterlessValueGetter,
				true);
		}

		public void TestExportSendExportAcknowledgementsTo()
		{
			var registryItem = EUCustomsDataRegistry.Instance.SendExportAcknowledgements;
			CombineAssertions(() =>
			{
				AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<ExportGroupNotification>), registryItem.GetType());
				TestGenericRegistryItem(registryItem,
					"SendExportAcknowledgementsRegistry",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_Export,
					"Send Export Acknowledgements To",
					"Send Export acknowledgements to staff member, nominated group or both",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
				AssertDefaultValues(registryItem.DefaultValue);
				AssertSequencesEqual("CountryFilterPKs", Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, registryItem.CountryFilterPKs);
			});
		}

		public void TestSendExportErrorsTo()
		{
			var registryItem = EUCustomsDataRegistry.Instance.SendExportMessageErrors;
			CombineAssertions(() =>
			{
				AssertEquals("Registry Type", typeof(GroupNotificationRegistryItem<ExportGroupNotification>), registryItem.GetType());
				TestGenericRegistryItem(registryItem,
					"SendExportMessageErrorsRegistry",
					CustomsDataRegistry.Categories.Customs_EuropeanUnionCommon_Export,
					"Send Export Errors To",
					"Send Export message errors to staff member, nominated group or both",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default);
				AssertDefaultValues(registryItem.DefaultValue);
				AssertSequencesEqual("CountryFilterPKs", Core.CountryGuids.CountriesUnderEUCustomsJurisdiction, registryItem.CountryFilterPKs);
			});
		}

		public void TestEnableCentralizedClearanceForImport()
		{
			TestGenericRegistryItem(
				item: ItemSet.EnableCentralizedClearanceForImport,
				expectedName: "EnableCentralizedClearanceForImport",
				expectedCategory: RawDataRegistry.Categories.Customs_EuropeanUnionCommon_Import,
				expectedCaption: "Enable Centralized Clearance for Import (CCI)",
				expectedHint: "When set to 'Yes', Centralized Clearance for Import (CCI) related functionalities will be enabled.",
				expectedStorage: RegistryStorageFlags.Company,
				expectedOptions: RegistryOptions.IsOnlyForDevelopers,
				expectedDefaultValue: false);
		}

		public void TestForbidMergingInvoiceLinesWithRatioBasedTariff()
		{
			TestGenericRegistryItem(
				item: ItemSet.ForbidMergingInvoiceLinesWithRatioBasedTariff,
				expectedName: "ForbidMergingInvoiceLinesWithRatioBasedTariff",
				expectedCategory: RawDataRegistry.Categories.Customs_EuropeanUnionCommon,
				expectedCaption: "Forbid merging of invoice lines with ratio-based tariff ‘CLASS’ conditions",
				expectedHint: "When set to 'Yes', merging of invoice lines with ratio-based tariff ‘CLASS’ conditions is not allowed.",
				expectedStorage: RegistryStorageFlags.System,
				expectedDefaultValue: true);
		}

		void AssertDefaultValues(ExportGroupNotification defaultValue)
		{
			AssertEquals("Default Send To", Core.Constants.EmailTo.StaffMember, defaultValue.SendMode);
			AssertEquals("Default Group", ZGuid.Empty, defaultValue.SendGroupPK);
		}
	}
}
