using System;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(CACustomsDataRegistry))]
	sealed class CACustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<CACustomsDataRegistry>
	{
		#region DataLoadingModuleCategory

		public void TestDataLoadingModuleOutputDirectory()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				StringRegistryItem directory = ItemSet.DataLoadingModuleOutputDirectory;
				directory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				AssertEquals(tempDir.DirectoryName, directory.Value);
				TestRegistryItem(ItemSet.DataLoadingModuleOutputDirectory, "DataLoadingModuleOutputDirectory", CACustomsDataRegistry.Categories.Customs_Canada_Export_DataLoadingModule, "Output Directory", "Enter the storage location for export messages to be sent to Customs", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, TextEditorType.DirectoryBrowser, "");
				Assert(ItemSet.DataLoadingModuleOutputDirectory.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			}
		}

		public void TestInBondTermsAndConditions()
		{
			TestGenericRegistryItem(ItemSet.InBondTermsAndConditions, "CAInBondDSVTermsAndConditions", CACustomsDataRegistry.Categories.Customs_Canada, "In Bond Terms & Conditions", "Enter the Terms & Conditions that will appear on the In Bond Document", RegistryStorageFlags.Company, CACustomsDataRegistry.InBondDefaultTermsAndConditions);
			TextRegistryEditorInfo editorInfo = ItemSet.InBondTermsAndConditions.EditorInfo as TextRegistryEditorInfo;
			AssertNotNull("EditorInfo should be of Type TextRegistryEditorInfo", editorInfo);
			AssertEquals("editorInfo.EditorType", TextEditorType.Memo, editorInfo.EditorType);
			Assert(ItemSet.InBondTermsAndConditions.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		#endregion

		#region CACustomsCategory

		public void TestWTGBusinessNumber()
		{
			TestGenericRegistryItem(ItemSet.WTGBusinessNumber,
									"WTGBusinessNumber",
									CACustomsDataRegistry.Categories.Customs_Canada,
									"WTG Business Number",
									"WTG Business Number",
									RegistryStorageFlags.System | RegistryStorageFlags.Company,
									RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
									"822066668RM0002");
			AssertEquals(((StringRegistryDataType)CACustomsDataRegistry.Instance.WTGBusinessNumber.DataType).CharacterCase, CharacterCase.Upper);
		}

		public void TestClientNetworkIDAppliesAllCountries()
		{
			TestGenericRegistryItem(ItemSet.MailBoxIDAppliesAllCountries,
									"MailBoxIDAppliesAllCountries",
									CACustomsDataRegistry.Categories.Customs_Canada,
									"Network / Mailbox ID",
									"Enter the Client Network ID allocated to you by CargoWise for production messages.",
									RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
									RegistryOptions.PreserveTestValue);
			AssertEquals(((StringRegistryDataType)CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.DataType).CharacterCase, CharacterCase.Upper);
			Assert(!ItemSet.MailBoxIDAppliesAllCountries.CountryFilterPKs.Any());
		}

		public void TestCBSATestNetworkIDAppliesAllCountries()
		{
			TestGenericRegistryItem(ItemSet.CBSATestNetworkIDAppliesAllCountries, "CBSATestNetworkIDAppliesAllCountries", CACustomsDataRegistry.Categories.Customs_Canada, "CBSA Test Network ID", "Enter the CBSA's Network ID for test system access", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, "RCCECECPW");
			Assert(!ItemSet.CBSATestNetworkIDAppliesAllCountries.CountryFilterPKs.Any());
		}

		public void TestCBSAProdNetworkIDAppliesAllCountries()
		{
			TestGenericRegistryItem(ItemSet.CBSAProdNetworkIDAppliesAllCountries, "CBSAProdNetworkIDAppliesAllCountries", CACustomsDataRegistry.Categories.Customs_Canada, "CBSA Prod Network ID", "Enter the CBSA's Network ID for production system access", RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, "RCCECECPW");
			Assert(!ItemSet.CBSAProdNetworkIDAppliesAllCountries.CountryFilterPKs.Any());
		}

		public void TestControlOfficeAppliesAllCountries()
		{
			TestGenericRegistryItem(ItemSet.ControlOfficeAppliesAllCountries, "ControlOfficeAppliesAllCountries", CACustomsDataRegistry.Categories.Customs_Canada, "Control Office", "Enter the Control Office", RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
			Assert(!ItemSet.ControlOfficeAppliesAllCountries.CountryFilterPKs.Any());
		}

		public void TestTransmissionSite()
		{
			TestGenericRegistryItem(ItemSet.TransmissionSite,
				"TransmissionSite",
				CACustomsDataRegistry.Categories.Customs_Canada,
				"Transmission Site", "Enter the Transmission Site",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				"U10207V2");
		}

		public void TestFrenchLanguageIndicator()
		{
			TestRegistryItem(ItemSet.FrenchLanguageIndicator,
							 "FrenchLanguageIndicator",
							 CACustomsDataRegistry.Categories.Customs_Canada,
							 "Is French Language Preferred?",
							 "Override if French is to be specified as the preferred language of communication with PGAs.",
							 RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 false);
			Assert(ItemSet.FrenchLanguageIndicator.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestMakeSomeFieldsJobDocAddress()
		{
			TestRegistryItem(ItemSet.MakeSomeFieldsJobDocAddress,
							 "MakeSomeFieldsJobDocAddress",
							 CACustomsDataRegistry.Categories.Customs_Canada,
							 "Make Some Fields Job Doc Address",
							 "Setting this value to 'Yes' will make Consignee, Delivery Party, Manufacturer Job Doc Address.",
							 RegistryStorageFlags.System,
							 RegistryOptions.IsOnlyForSupport,
							 false);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.MakeSomeFieldsJobDocAddress.CountryFilterPKs);
		}

		#endregion

		#region CACustomsCategoryAuditActions

		public void TestReleaseHighValueProductAudit()
		{
			TestRegistryItem(ItemSet.ReleaseHighValueProductAudit,
				"ReleaseHighValueProductAudit",
				CACustomsDataRegistry.Categories.Customs_Canada_AuditActions,
				"Release High Value Product Audit",
				"Set the Product Audit Action for Release High Value.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new Enterprise.Registry.Business.Customs.ProductAuditActions(),
				ProductAuditActions.Codes.NoAction);
			Assert(ItemSet.ReleaseHighValueProductAudit.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestReleaseLowValueProductAudit()
		{
			TestRegistryItem(ItemSet.ReleaseLowValueProductAudit,
				"ReleaseLowValueProductAudit",
				CACustomsDataRegistry.Categories.Customs_Canada_AuditActions,
				"Release Low Value Product Audit",
				"Set the Product Audit Action for Release Low Value.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new Enterprise.Registry.Business.Customs.ProductAuditActions(),
				ProductAuditActions.Codes.NoAction);
			Assert(ItemSet.ReleaseLowValueProductAudit.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestEntryHighValueProductAudit()
		{
			TestRegistryItem(ItemSet.EntryHighValueProductAudit,
				"EntryHighValueProductAudit",
				CACustomsDataRegistry.Categories.Customs_Canada_AuditActions,
				"Entry High Value Product Audit",
				"Set the Product Audit Action for Entry High Value.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new Enterprise.Registry.Business.Customs.ProductAuditActions(),
				ProductAuditActions.Codes.NoAction);
			Assert(ItemSet.EntryHighValueProductAudit.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestEntryLowValueProductAudit()
		{
			TestRegistryItem(ItemSet.EntryLowValueProductAudit,
				"EntryLowValueProductAudit",
				CACustomsDataRegistry.Categories.Customs_Canada_AuditActions,
				"Entry Low Value (including Consolidated LVS) Product Audit",
				"Set the Product Audit Action for Entry Low Value (including Consolidated LVS).",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new Enterprise.Registry.Business.Customs.ProductAuditActions(),
				ProductAuditActions.Codes.NoAction);
			Assert(ItemSet.EntryLowValueProductAudit.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		#endregion

		#region CACustomsCategoryExport

		public void TestDefaultServiceProviderOrganization()
		{
			TestRegistryItem(
				ItemSet.DefaultServiceProviderOrganization,
				"DefaultServiceProviderOrganization",
				CACustomsDataRegistry.Categories.Customs_Canada_Export,
				"Default Service Provider Organization",
				"This organization will be used to default the Service Provider on G7 export jobs.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryFindBoxCollection.OrgHeader,
				Guid.Empty);
			Assert(ItemSet.DefaultServiceProviderOrganization.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDataLoadingModuleDefaultPlaceOfReport()
		{
			TestGenericRegistryItem(ItemSet.DefaultPlaceOfReport,
									"DataLoadingModuleDefaultPlaceOfReport",
									CACustomsDataRegistry.Categories.Customs_Canada_Export,
									"Place Of Report",
									"Enter the default Place Of Report",
									RegistryStorageFlags.Branch,
									RegistryOptions.Default);
			Assert(ItemSet.DefaultPlaceOfReport.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			var editorInfo = ItemSet.DefaultPlaceOfReport.EditorInfo;
			AssertType<CodeFindBoxRegistryEditorInfo>("EditorInfo type", editorInfo);
			AssertEquals("EditorInfo ModuleID", ModuleIDs.Customs.Universal.ZZRefCusCodeList, ((CodeFindBoxRegistryEditorInfo)editorInfo).ModuleID);
		}

		public void TestExportDeclarationActive()
		{
			TestRegistryItem(ItemSet.ExportDeclarationActive,
							 "ExportDeclarationActive",
							 CACustomsDataRegistry.Categories.Customs_Canada_Export,
							 "Should Export Declaration Functionality be activated?",
							 "Should Export Declaration Functionality be activated?",
							 RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.PreserveTestValue,
							 true);
			Assert(ItemSet.ExportDeclarationActive.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendG7ExportMessages()
		{
			TestRegistryItem(ItemSet.SendG7ExportMessages,
							 "SendG7ExportMessages",
							 CACustomsDataRegistry.Categories.Customs_Canada_Export,
							 "Send G7 Export Message",
							 "Should G7 Export Message be sent (if not then the CAED export message is sent)?",
							 RegistryStorageFlags.System,
							 RegistryOptions.IsOnlyForSupport,
							 true);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendG7ExportMessages.CountryFilterPKs);
		}

		public void TestSendEXPAcknowledgements()
		{
			TestRegistryItem(ItemSet.SendEXPAcknowledgements,
							 "SendEXPAcknowledgements",
							 CACustomsDataRegistry.Categories.Customs_Canada_Export,
							 "Send G7 Export Message Acknowledgements",
							 "Send G7 Export message acknowledgements to staff member, nominated group or combination of both",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 new CodeDescriptionPairList(OLookUpEditType.EmailTo),
							 Constants.EmailTo.StaffMemberAndNominatedGroup);
			Assert(ItemSet.SendEXPAcknowledgements.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendEXPAcknowledgementsToGroup()
		{
			TestRegistryItem(ItemSet.SendEXPAcknowledgementsToGroup,
							 "SendEXPAcknowledgementsToGroup",
							 CACustomsDataRegistry.Categories.Customs_Canada_Export,
							 "Send G7 Export message Acknowledgements To Group",
							 "Send G7 Export message acknowledgements to selected group",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Core.Constants.Groups.PostMastersGroupPK);
			Assert(ItemSet.SendEXPAcknowledgementsToGroup.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendEXPErrors()
		{
			TestRegistryItem(ItemSet.SendEXPErrors,
							 "SendEXPErrors",
							 CACustomsDataRegistry.Categories.Customs_Canada_Export,
							 "Send G7 Export Message Errors",
							 "Send G7 Export message errors to staff member, nominated group or combination of both",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 new CodeDescriptionPairList(OLookUpEditType.EmailTo),
							 Constants.EmailTo.StaffMemberAndNominatedGroup);
			Assert(ItemSet.SendEXPErrors.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendEXPErrorsToGroup()
		{
			TestRegistryItem(ItemSet.SendEXPErrorsToGroup,
							 "SendEXPErrorsToGroup",
							 CACustomsDataRegistry.Categories.Customs_Canada_Export,
							 "Send G7 Export Message Errors To Group",
							 "Send G7 Export message errors to selected group",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryFindBoxCollection.GlbGroup,
							 Core.Constants.Groups.PostMastersGroupPK);
			Assert(ItemSet.SendEXPErrorsToGroup.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		#endregion

		#region CACustomsCategoryImport

		public void TestShouldDefaultCustomsCodes()
		{
			TestRegistryItem(ItemSet.ShouldDefaultCustomsCodes,
							 "ShouldDefaultCustomsCodes",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import,
							 "Default Customs Codes",
							 "Should Sub-Location and Customs Port of Clearance be defaulted from the Port of Discharge UNLOCO?",
							 RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 true);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.ShouldDefaultCustomsCodes.CountryFilterPKs);
		}

		public void TestConsolidateSOA()
		{
			TestRegistryItem(ItemSet.ConsolidateSOA,
							 "ConsolidateSOA",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import,
							 "Consolidate SOA",
							 "Setting this value to 'Yes' will consolidate multi-part SOA message sent by CBSA into one ARL statement of Account in CW1.",
							 RegistryStorageFlags.System,
							 false);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.ConsolidateSOA.CountryFilterPKs);
		}

		#region CASuppressShipmentRelatedFields

		public void TestCASuppressShipmentRelatedFields()
		{
			TestRegistryItem(ItemSet.SuppressShipmentRelatedFields,
				"SuppressShipmentRelatedFields",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
				"Suppress Shipment Related Fields for non synchronized declarations?",
				"If this item is set to YES, then forwarding / shipment related fields used for data synchronization will be suppressed and not visible.\r\n\r\nIf set to NO, then these fields will be visible by default in each new declaration.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
			Assert(ItemSet.SuppressShipmentRelatedFields.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		#endregion

		#region CACustomsCategoryImportACI

		public void TestSupplementaryNumberSuffixAppliesAllCountries()
		{
			TestGenericRegistryItem(ItemSet.SupplementaryNumberSuffixAppliesAllCountries, "SupplementaryNumberSuffixAppliesAllCountries", CACustomsDataRegistry.Categories.Customs_Canada_Import_ACI, "Supplementary Reference Number Suffix", "This value will be added to the end of the generated ACI Supplementary Reference Number", RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SupplementaryNumberSuffixAppliesAllCountries.CountryFilterPKs);
		}

		public void TestSendACIAcknowledgementsAppliesAllCountries()
		{
			TestRegistryItem(ItemSet.SendACIAcknowledgementsAppliesAllCountries,
							 "SendACIAcknowledgementsAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_ACI,
							 "Send ACI Message Acknowledgements",
							 "Send ACI message acknowledgements to staff member, nominated group or combination of both",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 new CodeDescriptionPairList(OLookUpEditType.EmailTo),
							 Constants.EmailTo.StaffMemberAndNominatedGroup);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendACIAcknowledgementsAppliesAllCountries.CountryFilterPKs);
		}

		public void TestSendACIAcknowledgementsToGroupAppliesAllCountries()
		{
			TestRegistryItem(ItemSet.SendACIAcknowledgementsToGroupAppliesAllCountries,
							 "SendACIAcknowledgementsToGroupAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_ACI,
							 "Send ACI Message Acknowledgements To Group",
							 "Send ACI message acknowledgements to selected group",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Core.Constants.Groups.PostMastersGroupPK);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendACIAcknowledgementsToGroupAppliesAllCountries.CountryFilterPKs);
		}

		public void TestSendACIErrorsAppliesAllCountries()
		{
			TestRegistryItem(ItemSet.SendACIErrorsAppliesAllCountries,
							 "SendACIErrorsAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_ACI,
							 "Send ACI Message Errors",
							 "Send ACI message errors to staff member, nominated group or combination of both",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 new CodeDescriptionPairList(OLookUpEditType.EmailTo),
							 Constants.EmailTo.StaffMemberAndNominatedGroup);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendACIErrorsAppliesAllCountries.CountryFilterPKs);
		}

		public void TestSendACIErrorsToGroupAppliesAllCountries()
		{
			TestRegistryItem(ItemSet.SendACIErrorsToGroupAppliesAllCountries,
							 "SendACIErrorsToGroupAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_ACI,
							 "Send ACI Message Errors To Group",
							 "Send ACI message errors to selected group",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Core.Constants.Groups.PostMastersGroupPK);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendACIErrorsToGroupAppliesAllCountries.CountryFilterPKs);
		}

		#endregion

		#region CACustomsCategoryImportACI

		public void TestSynchronizeAssemblyMasterwithLeadShipment()
		{
			TestRegistryItem(ItemSet.SynchronizeAssemblyMasterwithLeadShipment,
							 "SynchronizeAssemblyMasterwithLeadShipment",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Synchronize Assembly Master with Lead Shipment",
							 "If set to Yes, then when an Assembly Master Consolidation is synchronized with eManifest, only the lead shipment will be reported. If set to No, then the lead shipment will not be synchronized and instead, the sub-house bills will be synchronized for reporting the lowest level bills.",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 true);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SynchronizeAssemblyMasterwithLeadShipment.CountryFilterPKs);
		}

		public void TestSynchronizeBlindColoadMasterWithConsol()
		{
			TestRegistryItem(ItemSet.SynchronizeBlindColoadMasterWithConsol,
							 "SynchronizeBlindColoadMasterWithConsol",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Synchronize Blind Co-load Master with Consol",
							 "Should synchronize Blind co-load masters with Consol?",
							 RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 false);
			Assert(ItemSet.SynchronizeBlindColoadMasterWithConsol.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendeManifestForwarderMessageAcknowledgementsAppliesAllCountries()
		{
			TestRegistryItem(ItemSet.SendeManifestForwarderMessageAcknowledgementsAppliesAllCountries,
							 "SendeManifestForwarderMessageAcknowledgementsAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Send eManifest Forwarder Message Acknowledgements",
							 "Send eManifest Forwarder message acknowledgements to staff member, nominated group or combination of both",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 new CodeDescriptionPairList(OLookUpEditType.EmailTo),
							 Constants.EmailTo.StaffMemberAndNominatedGroup);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendeManifestForwarderMessageAcknowledgementsAppliesAllCountries.CountryFilterPKs);
		}

		public void TestSendeManifestForwarderMessageAcknowledgementsToGroup()
		{
			TestRegistryItem(ItemSet.SendeManifestForwarderMessageAcknowledgementsToGroupAppliesAllCountries,
							 "SendeManifestForwarderMessageAcknowledgementsToGroupAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Send eManifest Forwarder Message Acknowledgements To Group",
							 "Send eManifest Forwarder message acknowledgements to selected group",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Constants.Groups.PostMastersGroupPK);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendeManifestForwarderMessageAcknowledgementsToGroupAppliesAllCountries.CountryFilterPKs);
		}

		public void TestSendeManifestForwarderMessageErrors()
		{
			TestRegistryItem(ItemSet.SendeManifestForwarderMessageErrorsAppliesAllCountries,
							 "SendeManifestForwarderMessageErrorsAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Send eManifest Forwarder Message Errors",
							 "Send eManifest Forwarder message errors to staff member, nominated group or combination of both",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 new CodeDescriptionPairList(OLookUpEditType.EmailTo),
							 Constants.EmailTo.StaffMemberAndNominatedGroup);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendeManifestForwarderMessageErrorsAppliesAllCountries.CountryFilterPKs);
		}

		public void TestSendeManifestForwarderMessageErrorsToGroup()
		{
			TestRegistryItem(ItemSet.SendeManifestForwarderMessageErrorsToGroupAppliesAllCountries,
							 "SendeManifestForwarderMessageErrorsToGroupAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Send eManifest Forwarder Message Errors To Group",
							 "Send eManifest Forwarder message errors to selected group",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Constants.Groups.PostMastersGroupPK);
			Assert(!ItemSet.SendeManifestForwarderMessageErrorsToGroupAppliesAllCountries.CountryFilterPKs.Any());
		}

		public void TestSendBrokerSNPMessageDetailsToGroupAppliesAllCountries()
		{
			TestRegistryItem(ItemSet.SendBrokerSNPMessageDetailsToGroupAppliesAllCountries,
							 "SendBrokerSNPMessageDetailsToGroupAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Send received Broker SNP information To Group",
							 "Details contained in received Broker Secondary Notify Party messages will be emailed to this group of users.",
							 RegistryStorageFlags.System | RegistryStorageFlags.Company,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Constants.Groups.PostMastersGroupPK);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendBrokerSNPMessageDetailsToGroupAppliesAllCountries.CountryFilterPKs);
		}

		public void TestSendForwarderSNPMessageDetailsToGroupAppliesAllCountries()
		{
			TestRegistryItem(ItemSet.SendForwarderSNPMessageDetailsToGroupAppliesAllCountries,
							 "SendForwarderSNPMessageDetailsToGroupAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Send received Forwarder SNP information To Group",
							 "Details contained in received Forwarder Secondary Notify Party messages will be emailed to this group of users.",
							 RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Constants.Groups.PostMastersGroupPK);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendForwarderSNPMessageDetailsToGroupAppliesAllCountries.CountryFilterPKs);
		}

		public void TestSendCarrierSNPMessageDetailsToGroupAppliesAllCountries()
		{
			TestRegistryItem(ItemSet.SendCarrierSNPMessageDetailsToGroupAppliesAllCountries,
							 "SendCarrierSNPMessageDetailsToGroupAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Send received Carrier SNP information To Group",
							 "Details contained in received Carrier Secondary Notify Party messages will be emailed to this group of users.",
							 RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Constants.Groups.PostMastersGroupPK);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendCarrierSNPMessageDetailsToGroupAppliesAllCountries.CountryFilterPKs);
		}

		public void TestSendWarehouseSNPMessageDetailsToGroupAppliesAllCountries()
		{
			TestRegistryItem(ItemSet.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries,
							 "SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Send received Warehouse SNP and RNS information to Group",
							 "Details contained in received Warehouse Secondary Notify Party and Warehouse RNS Status messages will be emailed to this group of users.",
							 RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Constants.Groups.PostMastersGroupPK);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.SendWarehouseSNPMessageDetailsToGroupAppliesAllCountries.CountryFilterPKs);
		}

		public void TestIncludeAssociationAssignedCode()
		{
			TestRegistryItem(ItemSet.IncludeAssociationAssignedCode,
							 "IncludeAssociationAssignedCode",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
							 "Include Association Assigned Code",
							 "If this item is set to true then the Associations Assigned Codes (ACIHG/ACIHCM) will be send in the UNH segment of eManifest House Bill and Close messages, otherwise by default the codes will not be sent.",
							 RegistryStorageFlags.System | RegistryStorageFlags.Company,
							 false);
			Assert(ItemSet.IncludeAssociationAssignedCode.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAutoSendCloseMessage()
		{
			TestRegistryItem(ItemSet.AutoSendCloseMessage,
				 "AutoSendCloseMessage",
				 CACustomsDataRegistry.Categories.Customs_Canada_Import_eManifest,
				 "Automatically send eManifest Close Message",
				 "If set to Yes, the system will automatically send the eManifest consol close message once all house bills have been accepted. Set the registry setting to No to disable this automation.",
				 RegistryStorageFlags.Company,
				 true);
			Assert(ItemSet.AutoSendCloseMessage.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		#endregion

		#region CACustomsCategoryImportDeclaration

		public void TestEnableCreditCheckForCLVS()
		{
			TestRegistryItem(ItemSet.EnableCreditCheckForCLVS,
							 "EnableCreditCheckForCLVS",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration,
							 "Enable Credit Check For CLVS",
							 "If ticked, the credit check will run when the user ticks the \"Ready for Consolidation\" box on Courier LVS transactions.",
							 RegistryStorageFlags.System | RegistryStorageFlags.Company,
							 true);
			Assert(ItemSet.EnableCreditCheckForCLVS.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAccountSecurityNo()
		{
			TestGenericRegistryItem(ItemSet.AccountSecurityNo,
									"AccountSecurityNo",
									CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration,
									"ASEC Number", "CBSA assigned CADEX/CUSDEC Account Security Number.",
									RegistryStorageFlags.Company);

			StringRegistryDataType dataType = (StringRegistryDataType)ItemSet.AccountSecurityNo.DataType;
			AssertEquals(5, dataType.MinLength);
			Assert(ItemSet.AccountSecurityNo.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAccountSecurityNoPassword()
		{
			TestGenericRegistryItem(ItemSet.AccountSecurityNoPassword,
									"AccountSecurityNoPassword",
									CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration,
									"ASEC Number Password",
									"CBSA assigned CADEX/CUSDEC Account Password.",
									RegistryStorageFlags.Company);
			Assert(ItemSet.AccountSecurityNoPassword.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestMinimumVFDForDutyAndTax()
		{
			TestGenericRegistryItem(ItemSet.MinimumVFDForDutyAndTax,
									"MinimumVFDForDutyAndTax",
									CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration,
									"Minimum VFD For Duty And Tax",
									"Minimum VFD for duty and tax.",
									RegistryStorageFlags.System,
									RegistryOptions.Default);

			AssertEquals("Default value", 20.0m, ItemSet.MinimumVFDForDutyAndTax.Value);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.MinimumVFDForDutyAndTax.CountryFilterPKs);
		}

		public void TestCAPackageTypesMapping()
		{
			TestGenericRegistryItem(ItemSet.CAPackageTypesMapping,
									"CAPackageTypesMapping",
									CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration,
									"Package Type Mappings",
									"The mappings from Freight package types to Customs package type. If a brokerage job is embedded in a shipment, this mapping is used to convert the package types Freight shipments use to those Customs jobs use.",
									RegistryStorageFlags.Company,
									RegistryOptions.Default);
			Assert(ItemSet.CAPackageTypesMapping.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSuspendAssignmentOfEntryNumberToDisbursementCharges()
		{
			TestGenericRegistryItem(ItemSet.SuspendAssignmentOfEntryNumberToDisbursementCharges,
									"SuspendAssignmentOfEntryNumberToDisbursementCharges",
									CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration,
									"Suspend Assignment Of Entry Number To Disbursement Charges",
									"If ticked, then entry number on import declaration will not be populated on disbursement charges.",
									RegistryStorageFlags.System | RegistryStorageFlags.Company,
									RegistryOptions.IsOnlyForSupport,
									false);
			Assert(ItemSet.CAPackageTypesMapping.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		#region CustomsCanadaImportDeclarationDefaultLVSConsolStrategy

		public void TestCreateIndividualLVSShipments()
		{
			TestGenericRegistryItem(ItemSet.CreateIndividualLVSShipments,
							"CreateIndividualLVSShipments",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
							"Create Individual LVS Shipments",
							"Create Individual LVS Shipments",
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							false);
			Assert(ItemSet.CreateIndividualLVSShipments.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestConsolidateByImporter()
		{
			TestGenericRegistryItem(ItemSet.ConsolidateByImporter,
							 "ConsolidateByImporter",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
							 "Consolidate by Importer",
							 "Consolidate by Importer",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 false);
			Assert(ItemSet.ConsolidateByImporter.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestConsolidateByBranch()
		{
			TestGenericRegistryItem(ItemSet.ConsolidateByBranch,
							 "ConsolidateByBranch",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
							 "Consolidate by Branch",
							 "Consolidate by Branch",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 false);
			Assert(ItemSet.ConsolidateByBranch.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestConsolidateByProvinceOfClearance()
		{
			TestGenericRegistryItem(ItemSet.ConsolidateByProvinceOfClearance,
							 "ConsolidateByProvinceOfClearance",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
							 "Consolidate by Province of Clearance",
							 "Consolidate by Province of Clearance",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 false);
			Assert(ItemSet.ConsolidateByProvinceOfClearance.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestConsolidateByBroker()
		{
			TestGenericRegistryItem(ItemSet.ConsolidateByBroker,
							 "ConsolidateByBroker",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
							 "Consolidate by Broker",
							 "Consolidate by Broker",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 false);
			Assert(ItemSet.ConsolidateByBroker.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void ConsolidateToOneFTypePerCLVSEntry()
		{
			TestGenericRegistryItem(ItemSet.ConsolidateToOneFTypePerCLVSEntry,
							 "ConsolidateToOneFTypePerCLVSEntry",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_DefaultLVSConsolStrategy,
							 "Consolidate to one F type per CLVS entry",
							 "Consolidate to one F type per CLVS entry",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 false);
			Assert(ItemSet.ConsolidateToOneFTypePerCLVSEntry.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		#endregion

		#region CustomsCanadaImportDeclarationB3SendingOptions

		public void TestEntryStatementDateThreshold()
		{
			TestRegistryItem(ItemSet.EntryStatementDateThreshold,
				"EntryStatementDateThreshold",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_EntrySendingOptions,
				"Entry Accounting Date Warning Threshold",
				"If this value is greater than zero then a warning will be shown when an attempt is made to send a Entry message where the release date plus this threshold is after the next ARL cut-off date. You will then have the option of scheduling a delayed sending of the message.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch, 4);
			Assert(ItemSet.EntryStatementDateThreshold.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestCCNReuseTimeSetting()
		{
			TestRegistryItem(ItemSet.CCNReuseTimeSetting,
				"CCNReuseTimeSetting",
				CACustomsDataRegistry.Categories.Customs_Canada_Import,
				"CCN Reuse Time Frame (years)",
				"CCN Reuse Time Frame (years)",
				RegistryStorageFlags.System, 3);
		}

		public void TestEntryAutomaticSendingDelayThresholds()
		{
			TestGenericRegistryItem(ItemSet.EntryAutomaticSendingDelayThresholds,
				"EntryAutomaticSendingDelayThresholds",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_EntrySendingOptions,
				"Entry Automatic Sending Delay Thresholds",
				"The thresholds to be used when determining when to automatically send Entry messages.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				new DelayFactorRegistryBusinessObject(0, DelayIntervalTypeCodes.Codes.None, 0, DelayIntervalTypeCodes.Codes.None));
			Assert(ItemSet.EntryAutomaticSendingDelayThresholds.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestEntryLateSendingFailsafeWarningThresholds()
		{
			TestGenericRegistryItem(ItemSet.EntryLateSendingFailsafeWarningThresholds,
				"EntryLateSendingFailsafeWarningThresholds",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_EntrySendingOptions,
				"Entry Late Sending Fail-safe Warning Thresholds",
				"The thresholds to be used when determining when to send a warning that a declaration has not had a Entry successfully lodged.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				new DelayFactorRegistryBusinessObject(5, DelayIntervalTypeCodes.Codes.DAR, 12, DelayIntervalTypeCodes.Codes.DAY));
			Assert(ItemSet.EntryLateSendingFailsafeWarningThresholds.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDefaultDeferredNormalEntrySendAction()
		{
			TestGenericRegistryItem(ItemSet.DefaultDeferredNormalEntrySendAction,
				"DefaultDeferredNormalEntrySendAction",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_EntrySendingOptions,
				"Default Deferred Normal Entry Send Action",
				"Default Deferred Normal Entry Send Action\r\n\r\nThe default Entry message sending action when the job is eligible to defer the Entry message sending.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
			Assert(ItemSet.DefaultDeferredNormalEntrySendAction.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			AssertEquals("Default value", DeferredB3SendActionList.Codes.Defer, ItemSet.DefaultDeferredNormalEntrySendAction.Value);
		}

		public void TestDefaultDeferredLowValueEntrySendAction()
		{
			TestGenericRegistryItem(ItemSet.DefaultDeferredLowValueEntrySendAction,
				"DefaultDeferredLowValueEntrySendAction",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_EntrySendingOptions,
				"Default Deferred Low Value Entry Send Action",
				"Default Deferred Low Value Entry Send Action\r\n\r\nThe default Entry message sending action when the Low Value job is eligible to defer the Entry message sending.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
			Assert(ItemSet.DefaultDeferredLowValueEntrySendAction.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			AssertEquals("Default value", DeferredB3SendActionList.Codes.Defer, ItemSet.DefaultDeferredLowValueEntrySendAction.Value);
		}

		#endregion

		#region CustomsCanandaImportDeclarationCARM

		public void TestCARMAPIKey()
		{
			TestGenericRegistryItem(ItemSet.CARMAPIKey,
				"CARMAPIKey",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CARM,
				"CARM API Key",
				"This will store the key issued to the Broker/Importer to submit a CAD query.",
				RegistryStorageFlags.Company
				);
		}

		public void TestCARMEndPoint()
		{
			TestGenericRegistryItem(ItemSet.CARMEndPoint,
				"CARMEndPoint",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CARM,
				"CARM End Point",
				"This will store the API end point to post the CARM query to.",
				RegistryStorageFlags.Company,
				"https://ccapi-ipacc.cbsa-asfc.cloud-nuage.canada.ca/v1/declaration-srv-read/commercialAccountingDeclarations"
				);
		}

		#endregion

		#region CustomsCanandaImportDeclarationNotifications

		public void TestSendDeclarationMessageAcknowledgements()
		{
			TestRegistryItem(ItemSet.SendDeclarationMessageAcknowledgements,
							 "SendDeclarationMessageAcknowledgements",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Notifications,
							 "Send Declaration Message Acknowledgements",
							 "Send declaration message acknowledgements to staff member, nominated group or combination of both",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 new CodeDescriptionPairList(OLookUpEditType.EmailTo),
							 Constants.EmailTo.StaffMemberAndNominatedGroup);
			Assert(ItemSet.SendDeclarationMessageAcknowledgements.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendDeclarationMessageAcknowledgementsToGroup()
		{
			TestRegistryItem(ItemSet.SendDeclarationMessageAcknowledgementsToGroup,
							 "SendDeclarationMessageAcknowledgementsToGroup",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Notifications,
							 "Send Declaration Message Acknowledgements To Group",
							 "Send declaration message acknowledgements to selected group",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Constants.Groups.PostMastersGroupPK);
			Assert(ItemSet.SendDeclarationMessageAcknowledgementsToGroup.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendK84ReportNotificationsToGroup()
		{
			TestRegistryItem(ItemSet.SendK84ReportNotificationsToGroup,
							 "SendK84ReportNotificationsToGroup",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Notifications,
							 "Send ARL Messages Notification To Group",
							 "The group of users who will receive ARL Message notification when ARL Messages are received. If not entered the Declaration Message Acknowledgements group will be used.",
							 RegistryStorageFlags.Company,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup, Guid.Empty);
			Assert(ItemSet.SendK84ReportNotificationsToGroup.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendOverdueReportNotificationsToGroup()
		{
			TestRegistryItem(ItemSet.SendOverdueReportNotificationsToGroup,
							 "SendOverdueReportNotificationsToGroup",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Notifications,
							 "Send Overdue Report Notifications To Group",
							 "The group of users who will receive Overdue Report notification when Overdue Reports are received. If not entered the Declaration Message Acknowledgements group will be used.",
							 RegistryStorageFlags.Company,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup, Guid.Empty);
			Assert(ItemSet.SendOverdueReportNotificationsToGroup.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendDeclarationMessageErrors()
		{
			TestRegistryItem(ItemSet.SendDeclarationMessageErrors,
							 "SendDeclarationMessageErrors",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Notifications,
							 "Send Declaration Message Errors",
							 "Send declaration message errors to staff member, nominated group or combination of both",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 new CodeDescriptionPairList(OLookUpEditType.EmailTo),
							 Constants.EmailTo.StaffMemberAndNominatedGroup);
			Assert(ItemSet.SendDeclarationMessageErrors.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendDeclarationMessageErrorsToGroup()
		{
			TestRegistryItem(ItemSet.SendDeclarationMessageErrorsToGroup,
							 "SendDeclarationMessageErrorsToGroup",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Notifications,
							 "Send Declaration Message Errors To Group",
							 "Send declaration message errors to selected group",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbGroup,
							 Constants.Groups.PostMastersGroupPK);
			Assert(ItemSet.SendDeclarationMessageErrorsToGroup.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		#endregion

		#region CustomsCanandaImportDeclarationDefaults

		public void TestSeverityLevelForCertificateOfOriginValidations()
		{
			TestRegistryItem(ItemSet.SeverityLevelForCertificateOfOriginValidations,
				"SeverityLevelForCertificateOfOriginValidations",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
				"Severity Level for Certificate of Origin Validations",
				"Set the severity level for validating related to the Certificate of Origin(COO) for the selected Treatment code(TT). If validation is activated, the system will look for a valid COO for TT. The COO may exist on the Product or on the Importer Organization.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				new Enterprise.Registry.Business.Customs.ProductAuditActions(),
				ProductAuditActions.Codes.NoAction);
			Assert(ItemSet.SeverityLevelForCertificateOfOriginValidations.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAlwaysEnableReleaseMessageValidation()
		{
			TestRegistryItem(ItemSet.AlwaysEnableACROSSMessageValidation,
							"AlwaysEnableACROSSMessageValidation",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							"Always Enable Release Message Validation",
							@"If this registry item is overridden to YES then the Validate Release check box on declarations will always be checked and so validation related to the Release message will always be done regardless of the release status of the job.",
							RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							false);
			Assert(ItemSet.AlwaysEnableACROSSMessageValidation.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAlwaysEnableEntryMessageValidation()
		{
			TestRegistryItem(ItemSet.AlwaysEnableB3MessageValidation,
							"AlwaysEnableB3MessageValidation",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							"Always Enable Entry Message Validation",
							@"If this registry item is overridden to YES then the Validate CADEX check box on declarations will always be checked and so validation related to the entry message will always be done regardless of the release status of the job.",
							RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							false);
			Assert(ItemSet.AlwaysEnableB3MessageValidation.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAlwaysSendDeliveryAddressOnReleaseMessages()
		{
			TestRegistryItem(ItemSet.AlwaysSendDeliveryAddressOnReleaseMessages,
							"AlwaysSendDeliveryAddressOnReleaseMessages",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							"Always Send Delivery Address On Release Messages",
							@"If set then the address, phone and fax of the Delivery Address entered on the Delivery tab of a declaration will be sent in all ACROSS release messages.
If not set then the delivery address, phone and fax will only be sent for CFIA declarations.",
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							true);
			Assert(ItemSet.AlwaysSendDeliveryAddressOnReleaseMessages.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAlwaysUseImporterAccountSecurity()
		{
			TestRegistryItem(ItemSet.AlwaysUseImporterAccountSecurity,
							"AlwaysUseImporterAccountSecurity",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							"Always Use Importer Account Security",
							@"If this item is overridden to ‘NO’ then when an importer, who has their own account security code set on the organization,
is selected on a new declaration then a dialogue will be displayed and the user asked if they wish to use the importers account security code or their own brokers account security code.
If this item is set to ‘YES’ then no dialogue will be displayed and the importers account security code will always be used.",
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							true);
			Assert(ItemSet.AlwaysUseImporterAccountSecurity.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDeclarantOnEntryDocsBrokerOnEntry()
		{
			TestRegistryItem(ItemSet.DeclarantOnEntryDocsBrokerOnB3,
							 "DeclarantOnEntryDocsBrokerOnB3",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							 "Declarant On Entry?",
							 "The staff whose signature and name will be printed on the entry documents. If it is left blank, the signature and name of a declaration’s broker or login staff will be printed instead.",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbStaff,
							 Guid.Empty);
			Assert(ItemSet.DeclarantOnEntryDocsBrokerOnB3.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDeclarantOnEntryDocsBrokerOnB2()
		{
			TestRegistryItem(ItemSet.DeclarantOnEntryDocsBrokerOnB2,
							 "DeclarantOnEntryDocsBrokerOnB2",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							 "Declarant On B2?",
							 "The staff whose signature and name will be printed on the B2 documents. If it is left blank, the signature and name of a declaration’s broker or login staff will be printed instead.",
							 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							 RegistryOptions.IsValueMandatory,
							 RegistryFindBoxCollection.GlbStaff,
							 Guid.Empty);
			Assert(ItemSet.DeclarantOnEntryDocsBrokerOnB2.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDefaultExciseDutyQuantityToFirstCustomsQuantity()
		{
			TestGenericRegistryItem(ItemSet.DefaultExciseDutyQuantityToFirstCustomsQuantity,
									"DefaultExciseDutyQuantityToFirstCustomsQuantity",
									CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
									"Default Excise Duty Quantity to the First Customs Quantity",
									"Default the specific Excise Duty quantity to the first Customs Quantity were possible.",
									RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
									false);
			Assert(ItemSet.DefaultExciseDutyQuantityToFirstCustomsQuantity.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDefaultExciseTaxFromCustomsTariff()
		{
			TestRegistryItem(ItemSet.DefaultExciseTaxFromCustomsTariff,
							"DefaultExciseTaxFromCustomsTariff",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							"Default Excise Tax From Customs Tariff",
							@"If this registry item is set then any excise tax code associated with the entered HS code will be set in the Duty and Tax Grid.
If not set the excise tax line will be generated but there will be no rate code and so the user must decide if excise tax is applicable.",
							RegistryStorageFlags.Company,
							false);
			Assert(ItemSet.DefaultExciseTaxFromCustomsTariff.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDefaultGeneralRateOfDuty()
		{
			TestGenericRegistryItem(ItemSet.DefaultGeneralRateOfDuty,
									"DefaultGeneralRateOfDuty",
									CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
									"Default General Rate Of Duty",
									"Default general rate of duty.",
									RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
									RegistryOptions.Default);
			Assert(ItemSet.DefaultGeneralRateOfDuty.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			AssertEquals("Previous value", 35m, ItemSet.DefaultGeneralRateOfDuty.Value.PreviousValue);
			AssertEquals("New value", 35m, ItemSet.DefaultGeneralRateOfDuty.Value.NewValue);
			AssertEquals("Effective Date", new ZDateTime(2010, 1, 1), ItemSet.DefaultGeneralRateOfDuty.Value.EffectiveDate);
		}

		public void TestDefaultLVSInvoiceDetailsCode()
		{
			TestRegistryItem(ItemSet.DefaultLVSInvoiceDetailsCode,
							 "DefaultLVSInvoiceDetailsCode",
							 CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							 "Default LVS Invoice Detail Code",
							 @"This is the default code to select how much detail is included on LVS Invoices.
'Summarize' will print totals by Importer, while 'Detail' will print individual shipments.
This code may be overridden on Importer Organizations.",
							RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							ItemSet.LVSInvoiceDetails,
							LVSInvoiceDetailCodes.Codes.Summarize);
			Assert(ItemSet.DefaultLVSInvoiceDetailsCode.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestForceManualTransactionNumberAllocation()
		{
			TestRegistryItem(ItemSet.ForceManualInputOfTransactionNumber,
							"ForceManualTransactionNumberAllocation",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							"Force Manual Input of Transaction Number",
							"If this registry is overridden to YES then user must enter in the transaction number.",
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							false);
			Assert(ItemSet.ForceManualInputOfTransactionNumber.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDisplaySequentialOfTransactionNumberSeparately()
		{
			TestRegistryItem(ItemSet.DisplaySequentialOfTransactionNumberSeparately,
							"DisplaySequentialOfTransactionNumberSeparately",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							"Display Sequential Part of Transaction Number separately",
							"If this registry item is overridden to YES then system will display the transaction number in 3 separated fields.",
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							false);
			Assert(ItemSet.DisplaySequentialOfTransactionNumberSeparately.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDisplayCargoControlNumberSeparately()
		{
			TestRegistryItem(ItemSet.DisplayCargoControlNumberSeparately,
							"DisplayCargoControlNumberSeparately",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							"Display Sequential Part of Cargo Control Number separately",
							"If this registry item is overridden to YES then system will display the cargo control number in 2 separated fields.",
							RegistryStorageFlags.Branch,
							false);
			Assert(ItemSet.DisplayCargoControlNumberSeparately.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDefaultPortOfClearance()
		{
			TestGenericRegistryItem(ItemSet.DefaultPortOfClearance,
									"DefaultPortOfClearance",
									CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
									"Port Of Clearance",
									"Enter the default Port Of Clearance",
									RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
									RegistryOptions.Default);
			Assert(ItemSet.DefaultPortOfClearance.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			IRegistryEditorInfo editorInfo = ItemSet.DefaultPortOfClearance.EditorInfo;
			AssertEquals("EditorInfo's type", typeof(CodeFindBoxRegistryEditorInfo), editorInfo.GetType());
			AssertEquals("EditorInfo's ModuleID", ModuleIDs.Customs.Universal.ZZRefCusCodeList, ((CodeFindBoxRegistryEditorInfo)editorInfo).ModuleID);
		}

		public void TestShouldPrintBrokerSignatureOnEntryDocs()
		{
			TestGenericRegistryItem(ItemSet.ShouldPrintBrokerSignatureOnEntryDocs,
									"ShouldPrintBrokerSignatureOnEntryDocs",
									CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
									"Print Broker Signature On B2 and B3?",
									"Indicate whether the broker's electronic signature should be printed on the B2 and B3 Entry Documents. The signature will come from the brokers signature on their staff record.",
									RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
									false);
			Assert(ItemSet.ShouldPrintBrokerSignatureOnEntryDocs.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDefaultTariffTreatment()
		{
			TestGenericRegistryItem(ItemSet.DefaultTariffTreatment,
									"DefaultTariffTreatment",
									CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
									"Default Tariff Treatment",
									"This value will set as the default Tariff Treatment on new Invoice Headers.",
									RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
									"02");
			Assert(ItemSet.DefaultTariffTreatment.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			StringRegistryDataType dataType = (StringRegistryDataType)ItemSet.DefaultTariffTreatment.DataType;
			AssertEquals(0, dataType.MinLength);
			AssertEquals(2, dataType.MaxLength);
		}

		public void TestDefaultBranchPgaContact()
		{
			TestRegistryItem(ItemSet.DefaultBranchPgaContact,
								"DefaultBranchPGAContact",
								CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
								"Default Branch PGA Contact",
								"The contact name, email address, fax, mobile and phone number of the selected staff member will default for declaration when Partner Government Agency reporting is required. \r\n\r\nIf the staff member does not have a work phone number entered, the branch phone number of the home branch of that staff member will be used.",
								RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
								RegistryOptions.Default,
								RegistryFindBoxCollection.GlbStaff,
								Guid.Empty);
			Assert(ItemSet.DefaultBranchPgaContact.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDefaultFreightPercentages()
		{
			var collection = new DefaultFreightPercentageCollection();
			var item = collection.AddNew();
			item.ModeofTransport = "ROA";
			item.FreightPercentage = 0.12m;
			ItemSet.DefaultFreightPercentages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			collection = ItemSet.DefaultFreightPercentages.Value;
			AssertEquals("Count", 1, collection.Count);
			item = collection[0];
			AssertEquals("ModeofTransport", "ROA", item.ModeofTransport);
			AssertEquals("FreightPercentage", 0.12m, item.FreightPercentage);
		}

		public void TestDefaultOffsetNegativeGSTLinesForTotalsInB2Form()
		{
			TestRegistryItem(ItemSet.DefaultOffsetNegativeGSTLinesForTotalsInB2Form,
							"DefaultOffsetNegativeGSTLinesForTotalsInB2Form",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
							"Offset negative GST lines for Totals in B2 form",
							"Default offset negative GST lines for Totals in B2 form.",
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							true);
			Assert(ItemSet.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestDefaultToThisExciseTaxRateCodeWhenApplicable()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.ExciseTax);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "E90", rateType.PK);
			var rateCode2 = universalHelper.LoadOrCreateNewCusRateCode(Factory, "E91", rateType.PK);
			Factory.Save();

			TestRegistryItem(ItemSet.DefaultToThisExciseTaxRateCodeWhenApplicable,
				"DefaultToThisExciseTaxRateCodeWhenApplicable",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_Defaults,
				"Default to this Excise Tax rate code when applicable",
				"Set the default to this Excise Tax rate code when applicable.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				ItemSet.RefCusRateCodePairList,
				ZString.Empty);
			Assert(ItemSet.DefaultToThisExciseTaxRateCodeWhenApplicable.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		#endregion

		#region CACustomsCategoryImportDeclaration

		public void TestDefaultCFIAFeePaymentMethod()
		{
			TestRegistryItem(ItemSet.DefaultCFIAFeePaymentMethod,
							"DefaultCFIAFeePaymentMethod",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CFIA,
							"Default CFIA Fee Payment Method",
							"Set the default CFIA fee payment method.",
							RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
							ItemSet.CFIAFeePaymentMethods,
							CFIAPaymentMethods.Codes.Other);
			Assert(ItemSet.DefaultCFIAFeePaymentMethod.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAIRSValidationKey()
		{
			TestGenericRegistryItem(ItemSet.AIRSValidationKey,
							"AIRSValidationKey",
							CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CFIA,
							"AIRS Validation Key",
							"The broker/importer specific identifier assigned by CFIA for use with the AIRS Validation Service queries’ to this new group.",
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							RegistryOptions.Default,
							string.Empty);
			Assert(ItemSet.AIRSValidationKey.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAVSTimeoutInMinutes()
		{
			TestGenericRegistryItem(ItemSet.AVSTimeoutInMinutes,
						"AVSTimeoutInMinutes",
						CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CFIA,
						"AVS Timeout in Minutes",
						"AVS Timeout in Minutes.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						15);
			Assert(ItemSet.AVSTimeoutInMinutes.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAVSRetryTimes()
		{
			TestGenericRegistryItem(ItemSet.AVSRetryTimes,
						"AVSRetryTimes",
						CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CFIA,
						"AVS Retry Times",
						"AVS Retry Times.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						3);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.AVSRetryTimes.CountryFilterPKs);
		}

		public void TestAVSRetryIntervalInMinutes()
		{
			TestGenericRegistryItem(ItemSet.AVSRetryIntervalInMinutes,
						"AVSRetryIntervalInMinutes",
						CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CFIA,
						"AVS Retry Interval In Minutes",
						"AVS Retry Interval In Minutes.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						5);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.AVSRetryIntervalInMinutes.CountryFilterPKs);
		}

		public void TestAIRSValidationRequestUserName()
		{
			TestGenericRegistryItem(ItemSet.AIRSValidationRequestUserName,
						"AIRSValidationRequestUserName",
						CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CFIA,
						"AIRS Validation Request User Name",
						"The user name that client uses to authenticate itself to AIRS Validation service.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"");
			Assert(ItemSet.AIRSValidationRequestUserName.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAIRSValidationRequestPassword()
		{
			TestGenericRegistryItem(ItemSet.AIRSValidationRequestPassword,
						"AIRSValidationRequestPassword",
						CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CFIA,
						"AIRS Validation Request Password",
						"The password that client uses to authenticate itself to AIRS Validation service.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"");
			Assert(ItemSet.AIRSValidationRequestPassword.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestAIRSValidationRequestURL()
		{
			TestGenericRegistryItem(ItemSet.AIRSValidationRequestURL,
						"AIRSValidationRequestURL",
						CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CFIA,
						"AIRS Validation Request URL",
						"AIRS Validation Request URL.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"https://avs-svs.inspection.gc.ca/avs/bvs.svc/secure");
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.AIRSValidationRequestURL.CountryFilterPKs);
		}

		public void TestWebProxyAddress()
		{
			TestGenericRegistryItem(ItemSet.WebProxyAddress,
						"CAWebProxyAddress",
						CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_CFIA,
						"Web Proxy Address",
						"Web Proxy Address.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						"");
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.WebProxyAddress.CountryFilterPKs);
		}

		#endregion

		#region CustomsCanadaImportDeclarationQualityControl

		public void TestTimeFrameForExceptionReporting()
		{
			TestGenericRegistryItem(ItemSet.TimeFrameForExceptionReporting,
						"TimeFrameForExceptionReporting",
						CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_QualityControl,
						"Time Frame for Exception Reporting",
						"The number of days in the past that we will use to select possible jobs for exception reporting based on the first time that a message was sent to Customs. Increasing this value will decrease performance.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						60);
			Assert(ItemSet.TimeFrameForExceptionReporting.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestEntryAcceptedButNotReportedOnDN()
		{
			TestRegistryItem(ItemSet.B3AcceptedButNotReportedOnDN,
				"B3AcceptedButNotReportedOnDN",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_QualityControl,
				"Entry accepted but not reported on DN",
				"Flag jobs that have had the entry accepted but have not been reported on a Daily Notice.(Days)",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, 1);
			Assert(ItemSet.B3AcceptedButNotReportedOnDN.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestB3NoResponseThreshold()
		{
			TestRegistryItem(ItemSet.B3NoResponseThreshold,
				"B3NoResponseThreshold",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_QualityControl,
				"Entry no response threshold",
				"Flag jobs that have had an Entry sent but there has not been a response.(Hours)",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, 2);
			Assert(ItemSet.B3NoResponseThreshold.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestPARSNotreleasedAirThreshold()
		{
			TestRegistryItem(ItemSet.PARSNotreleasedAirThreshold,
				"PARSNotreleasedAirThreshold",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_QualityControl,
				"PARS not released, Air threshold",
				"Flag PARS Air jobs that have not been released after ETA(Hours)",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, 6);
			Assert(ItemSet.PARSNotreleasedAirThreshold.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestPARSNotreleasedOceanThreshold()
		{
			TestRegistryItem(ItemSet.PARSNotreleasedOceanThreshold,
				"PARSNotreleasedOceanThreshold",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_QualityControl,
				"PARS not released Ocean threshold",
				"Flag PARS Ocean jobs that have not been released after ETA.(Hours)",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, 24);
			Assert(ItemSet.PARSNotreleasedOceanThreshold.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestPARSNotReleasedHighwayRailAndOtherThreshold()
		{
			TestRegistryItem(ItemSet.PARSNotReleasedHighwayRailAndOtherThreshold,
				"PARSNotReleasedHighwayRailAndOtherThreshold",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_QualityControl,
				"PARS not released, Highway, Rail and Other threshold",
				"Flag PARS Highway, Rail and Other jobs that have not been released after ETA.(Hours)",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, 12);
			Assert(ItemSet.PARSNotReleasedHighwayRailAndOtherThreshold.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestPostArrivalNotReleased()
		{
			TestRegistryItem(ItemSet.PostArrivalNotReleased,
				"PostArrivalNotReleased",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_QualityControl,
				"Post Arrival not released",
				"Flag Post Arrival jobs that have not been released.(Days)",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, 1);
			Assert(ItemSet.PostArrivalNotReleased.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestReleaseNoResponseThreshold()
		{
			TestRegistryItem(ItemSet.ReleaseNoResponseThreshold,
				"ReleaseNoResponseThreshold",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_QualityControl,
				"Release No Response Threshold",
				"Flag jobs that have been released but there has not been a response.(Hours).",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, 2);
			Assert(ItemSet.ReleaseNoResponseThreshold.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestSendB3CADProgressFormThreshold()
		{
			TestRegistryItem(ItemSet.SendB3CADProgressFormThreshold,
				"SendB3CADProgressFormThreshold",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_Declaration_QualityControl,
				"Send CAD Message Progress Form Threshold",
				"Defined the threshold to send CAD messages with progress bar dialog box when having CAD entry lines more than this value.",
				RegistryStorageFlags.Company, 100);
			Assert(ItemSet.ReleaseNoResponseThreshold.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		#endregion

		#endregion

		#region CACustomsCategoryImportRNS
		public void TestDefaultRNSOffice()
		{
			TestGenericRegistryItem(ItemSet.DefaultRNSOffice,
				"DefaultRNSOffice",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_RNS,
				"Default CBSA Office",
				"Enter the default CBSA office",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default);
			Assert(ItemSet.DefaultRNSOffice.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			IRegistryEditorInfo editorInfo = ItemSet.DefaultRNSOffice.EditorInfo;
			AssertEquals("EditorInfo's type", typeof(CodeFindBoxRegistryEditorInfo), editorInfo.GetType());
			AssertEquals("EditorInfo's ModuleID", ModuleIDs.Customs.Universal.ZZRefCusCodeList, ((CodeFindBoxRegistryEditorInfo)editorInfo).ModuleID);
		}

		public void TestRNSActive()
		{
			TestRegistryItem(ItemSet.RNSActive,
				"RNSActive",
				CACustomsDataRegistry.Categories.Customs_Canada_Import_RNS,
				"Should RNS Functionality be activated?",
				"Should RNS Functionality be activated?",
				RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				true);
			Assert(ItemSet.RNSActive.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}
		#endregion

		#endregion

		#region CACustomsSreviceTasks

		public void TestDefaultBranchForServiceTasks()
		{
			TestRegistryItem(ItemSet.DefaultBranchForServiceTasks,
				"DefaultBranchForServiceTasks",
				CACustomsDataRegistry.Categories.Customs_Canada_ServiceTasks,
				"Default Service Task Branch",
				"Set the default branch which Canadian service tasks would run under.",
				RegistryStorageFlags.System,
				RegistryFindBoxCollection.GlbBranchNotCurrentCompanyRelated,
				Guid.Empty);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.DefaultBranchForServiceTasks.CountryFilterPKs);
		}

		public void TestRunServiceProviderClientServiceTaskAppliesAllCountries()
		{
			TestRegistryItem(ItemSet.RunServiceProviderClientServiceTaskAppliesAllCountries,
							 "RunServiceProviderClientServiceTaskAppliesAllCountries",
							 CACustomsDataRegistry.Categories.Customs_Canada_ServiceTasks,
							 "Run CA Messaging Service Tasks?",
							 "If you have a licensed Canadian company in your DB then the CA Messaging Service Tasks will automatically be active and you do not need to set this registry setting, but if you do not have a licensed CA company in your Data Base, but wish to use CA messaging like ACI and eManifest, then override this setting and set to ‘Yes’.",
							 RegistryStorageFlags.System,
							 RegistryOptions.PreserveTestValue,
							 false);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.RunServiceProviderClientServiceTaskAppliesAllCountries.CountryFilterPKs);
		}

		#endregion

		#region CACustomsCategoryTestingDevelopment

		public void TestEnableImportUniversalTransactionBatchXMLFiles()
		{
			TestRegistryItem(ItemSet.EnableImportUniversalTransactionBatchXMLFiles,
				"EnableImportUniversalTransactionBatchXMLFiles",
				CACustomsDataRegistry.Categories.Customs_Canada_TestingDevelopment,
				"Enable Import Universal Transaction Batch XML Files?",
				@"Turning this on will cause the menu - ""Actions - Data Transfer - Import Universal Transaction Batch XML"" enable on the ""DN & SOA Statements"" module.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.EnableImportUniversalTransactionBatchXMLFiles.CountryFilterPKs);
		}

		public void TestMessageOutputDirectory()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				StringRegistryItem directory = ItemSet.MessageOutputDirectory;
				directory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				AssertEquals(tempDir.DirectoryName, directory.Value);
				TestRegistryItem(ItemSet.MessageOutputDirectory, "MessageOutputDirectory", CACustomsDataRegistry.Categories.Customs_Canada_TestingDevelopment, "Test Message Output Directory", "Enter the location where test messages will be placed.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.IsOnlyForDevelopers, TextEditorType.DirectoryBrowser, "");
				Assert(ItemSet.MessageOutputDirectory.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			}
		}

		public void TestMessageInputDirectory()
		{
			using (TempDirectory tempDir = new TempDirectory())
			{
				StringRegistryItem directory = ItemSet.MessageInputDirectory;
				directory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				AssertEquals(tempDir.DirectoryName, directory.Value);
				TestRegistryItem(ItemSet.MessageInputDirectory, "MessageInputDirectory", CACustomsDataRegistry.Categories.Customs_Canada_TestingDevelopment, "Test Message Input Directory", "Enter the storage location where test messages will be read.", RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.IsOnlyForDevelopers, TextEditorType.DirectoryBrowser, "");
				Assert(ItemSet.MessageInputDirectory.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			}
		}

		public void TestEnableDebugHooks()
		{
			TestRegistryItem(ItemSet.EnableDebugHooks,
							 "EnableDebugHooks",
							 CACustomsDataRegistry.Categories.Customs_Canada_TestingDevelopment,
							 "Enable Debug Hooks?",
							 "Turning this on will cause copies of data send and received from Customs will be stored in a DEBUG folder off the CIG folder. Other debugging features and logging will also be turned on.",
							 RegistryStorageFlags.Company,
							 RegistryOptions.IsOnlyForDevelopers,
							 false);
			Assert(ItemSet.EnableDebugHooks.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
		}

		public void TestCSAFunctionActive()
		{
			TestGenericRegistryItem(ItemSet.CSAFunctionActive,
									"CSAFunctionActive",
									CACustomsDataRegistry.Categories.Customs_Canada_TestingDevelopment,
									"Should CSA Functionality be activated?",
									"Should CSA Functionality be activated?",
									RegistryStorageFlags.Company,
									RegistryOptions.IsOnlyForDevelopers);
			Assert(ItemSet.CSAFunctionActive.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			AssertEquals("Default value", false, ItemSet.CSAFunctionActive.Value);
		}

		public void TestUseCasualProvinceForTaxOverrides()
		{
			TestGenericRegistryItem(ItemSet.UseCasualProvinceForTaxOverrides,
							 "UseCasualProvinceForTaxOverrides",
							 CACustomsDataRegistry.Categories.Customs_Canada_TestingDevelopment,
							 "Use Casual Province For Tax Overrides",
							 "Use Casual Province In Place Of Debtor Or Creditor Home Country/Region For Shipment and Declaration Jobs, for Tax Overrides (For Developers Only).",
								RegistryStorageFlags.Company,
								RegistryOptions.IsOnlyForDevelopers);
			Assert(ItemSet.UseCasualProvinceForTaxOverrides.CountryFilterPKs.Contains(Constants.CountryGuids.Canada));
			AssertEquals("Default value", false, ItemSet.UseCasualProvinceForTaxOverrides.Value);
		}

		public void TestActivateAutoEntrySending()
		{
			TestGenericRegistryItem(ItemSet.ActivateAutoB3Sending,
							 "ActivateAutoB3Sending",
							 CACustomsDataRegistry.Categories.Customs_Canada_TestingDevelopment,
							 "Activate Auto Entry Sending",
							 "Activate Auto Entry Sending (DO NOT ACTVATE)?\r\n\r\nDO NOT ACTIVATE THIS WITHOUT FIRST DISCUSSING WITH THE CUSTOMS TEAM, THIS FEATURE HAS BEEN REPLACED BY WORFLOW FUNCTION. The process controller needs to be restarted for the registry change to take effect.",
								RegistryStorageFlags.System,
								RegistryOptions.IsOnlyForDevelopers);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.ActivateAutoB3Sending.CountryFilterPKs);
			AssertEquals("Default value", false, ItemSet.ActivateAutoB3Sending.Value);
		}

		public void TestAllowPaymentPartyOverride()
		{
			TestGenericRegistryItem(ItemSet.AllowPaymentPartyOverride,
					"AllowPaymentPartyOverride",
					CACustomsDataRegistry.Categories.Customs_Canada_TestingDevelopment,
					"Allow Payment Party Override.",
					"Allow Payment Party Override.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.AllowPaymentPartyOverride.CountryFilterPKs);
			AssertEquals("Default value", false, ItemSet.AllowPaymentPartyOverride.Value);
		}

		public void TestShouldBatchNumberBeByInterchange()
		{
			TestGenericRegistryItem(ItemSet.ShouldBatchNumberBeByInterchange,
					"ShouldBatchNumberBeByInterchange",
					CACustomsDataRegistry.Categories.Customs_Canada_TestingDevelopment,
					"Should Batch Number be by Interchange",
					"Should Batch Number be by Interchange.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForDevelopers);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.ShouldBatchNumberBeByInterchange.CountryFilterPKs);
			AssertEquals("Default value", false, ItemSet.ShouldBatchNumberBeByInterchange.Value);
		}

		#endregion

		#region CopyOGDToPGADataDateTime

		public void TestCopyOGDToPGADataDateTime()
		{
			TestGenericRegistryItem(ItemSet.CopyOGDToPGADataDateTime,
				"CopyOGDToPGADataDateTime",
				CACustomsDataRegistry.Categories.Customs_Canada_Import,
				"Processing copy OGD to PGA date",
				"It's time to processing copy OGD to PGA date for products.",
				RegistryStorageFlags.System,
				RegistryOptions.NotCached | RegistryOptions.IsOnlyForDevelopers
			);
			AssertEquals(Enumerable.Empty<Guid>(), ItemSet.CopyOGDToPGADataDateTime.CountryFilterPKs);
		}

		#endregion

		public override void TestAccessingValuesOnlyDoesNotDemandResourceStrings()
		{
			// The resource strings are from ZZ Ref DB
			Assert(true);
		}
	}
}
