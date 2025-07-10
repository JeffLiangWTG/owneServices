using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUCustomsDataRegistry))]
	sealed class AUCustomsDataRegistryTest : RegistryItemSetTestCaseWithFactory<AUCustomsDataRegistry>
	{
		public void TestLocalContactPhoneNumber() => CombineAssertions(() =>
		{
			TestRegistryItem(ItemSet.LocalContactPhoneNumber,
				"LocalContactPhoneNumber",
				RawDataRegistry.Categories.Customs_Australia,
				"Local Contact Phone Number",
				"When submitting a Self-Assessed Clearance via the Customs Declaration Module (Short SAC) and no Local Customs Branch Identifier has been entered, this phone number will be used instead.",
				RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				TextEditorType.TextBox,
				string.Empty,
				"0293110000");
			var dataType = (NumericOnlyStringRegistryDataType)ItemSet.LocalContactPhoneNumber.DataType;
			AssertEquals("MinLength", 0, dataType.MinLength);
			AssertEquals("MaxLength", 25, dataType.MaxLength);
		});

		public void TestUseCustomsReferenceData()
		{
			TestRegistryItem(ItemSet.UseCustomsReferenceData,
				"UseCustomsReferenceData",
				RawDataRegistry.Categories.Customs_Australia,
				"Use Customs Reference Data",
				"Setting this to Yes will use the Nomenclature reference data captured by Customs and not BorderWise.  Please do not change this without contacting CW1 support first.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestEnableCWRefForAHECC()
		{
			TestGenericRegistryItem(ItemSet.EnableCWRefForAHECC,
				"EnableCWRefForAHECC",
				RawDataRegistry.Categories.Customs_Australia_Testing,
				"Enable CW-Ref for AHECC",
				"Setting this to True will force CW1 to use the AHECC tariff data from CW-Ref instead of TRF-AU.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestICSReleaseEffectiveDate()
		{
			TestGenericRegistryItem(ItemSet.ICSReleaseEffectiveDate,
				"ICSReleaseEffectiveDate",
				RawDataRegistry.Categories.Customs_Australia,
				"ICS Release 17.4.02 effective date",
				"ICS Release 17.4.02 effective date.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForController,
				new DateTime(2017, 6, 14));
		}

		public void TestEnableAUServiceTaskCheckForSendingMessage()
		{
			TestGenericRegistryItem(ItemSet.EnableAUServiceTaskCheckForSendingMessage,
				"EnableAUServiceTaskCheckForSendingMessage",
				RawDataRegistry.Categories.Customs_Australia,
				"Enable AU Service Task Check For Sending Message",
				"The system will check whether the required service task is running before sending any message",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				true);
		}

		public void TestBackgroundCargoReportMaxMsg()
		{
			TestRegistryItem(ItemSet.BackgroundCargoReportMaxMsg,
				"AUBackgroundCargoReportMaxMsg",
				RawDataRegistry.Categories.Customs_Australia,
				"Background Cargo Report maximum messages",
				"Specify the maximum number of EDI messages to be sent to Customs over the defined interval for background Cargo Reporting process.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				300,
				0,
				15000);
			var editorInfo = (NumericRegistryEditorInfo)ItemSet.BackgroundCargoReportMaxMsg.EditorInfo;
			AssertEquals(0, editorInfo.DecimalPlaces);
		}

		public void TestBackgroundCargoReportSubThrottleWindow()
		{
			TestRegistryItem(ItemSet.BackgroundCargoReportSubThrottleWindow,
				"AUBackgroundCargoReportSubmissionThrottleWindow",
				RawDataRegistry.Categories.Customs_Australia,
				"Background Cargo Report submission throttle window",
				"Specify the throttle window in minutes for the restriction of EDI Cargo Report messages to Customs.",
				RegistryStorageFlags.Company,
				RegistryOptions.Default,
				1,
				0,
				60);
			var editorInfo = (NumericRegistryEditorInfo)ItemSet.BackgroundCargoReportSubThrottleWindow.EditorInfo;
			AssertEquals(0, editorInfo.DecimalPlaces);
		}

		public void TestSecurityRequiredTreatmentCodes()
		{
			TestRegistryItem(ItemSet.SecurityRequiredTreatmentCodes,
				"SecurityRequiredTreatmentCodes",
				AUCustomsDataRegistry.ImportDeclarationCategory,
				"Security-Required Treatment Codes",
				"Entry lines with these Treatment Codes require security (either collected or un-collected). Security related validation will be run and Duty and GST will be calculated as zero.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				"351,352,354");
		}

		public void TestNoProcessingChargeTreatmentCodes()
		{
			TestRegistryItem(ItemSet.NoProcessingChargeTreatmentCodes,
				"NoProcessingChargeTreatmentCodes",
				AUCustomsDataRegistry.ImportDeclarationCategory,
				"No Processing Charge Treatment Codes",
				"If all entry lines of a job have these treatment codes then the Customs Entry Processing Charge will be zero.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				"354");
		}

		public void TestUnsolicitedICSReportsRegistryItems()
		{
			TestRegistryItem(ItemSet.SendUnmatchedICSReports, "SendUnmatchedICSReports", AUCustomsDataRegistry.ImportDeclarationCategory, "Send Unsolicited / Unmatched ICS Reports", "Indicate the action to perform when an unsolicited or unmatched ICS document is received.", RegistryStorageFlags.Company, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Core.Constants.EmailTo.NoEmails);
			TestRegistryItem(ItemSet.SendUnmatchedICSReportsToGroup, "SendUnmatchedICSReportsToGroup", AUCustomsDataRegistry.ImportDeclarationCategory, "Group To Send Unmatched ICS Reports To", "Indicate the email address/Group to deliver the unsolicited or unmatched ICS document to.", RegistryStorageFlags.Company, RegistryFindBoxCollection.GlbGroup, Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestThirdPartyRefundRegistryItems()
		{
			TestRegistryItem(ItemSet.ThirdPartyRefundRejectionSendAcknowledgements, "ThirdPartyRefundRejectionSendAcknowledgements", AUCustomsDataRegistry.ImportDeclarationCategory, "Send Third Party Refund Rejections", "Indicate the action to perform when an unmatched Refund Reject message is detected.", RegistryStorageFlags.Company, new CodeDescriptionPairList(OLookUpEditType.EmailTo), Core.Constants.EmailTo.NoEmails);
			TestRegistryItem(ItemSet.ThirdPartyRefundRejectionSendAcknowledgementsToGroup, "ThirdPartyRefundRejectionSendAcknowledgementsToGroup", AUCustomsDataRegistry.ImportDeclarationCategory, "Group To Send Third Party Refund Rejections To", "Indicate the email address/Group to deliver the Refund Reject message to.", RegistryStorageFlags.Company, RegistryFindBoxCollection.GlbGroup, Core.Constants.Groups.PostMastersGroupPK);
		}

		public void TestTotalUnknownFreightandInsurancePercentage()
		{
			TestRegistryItem(ItemSet.TotalUnknownFreightandInsurancePercentage,
				"TotalUnknownFreightandInsurancePercentage",
				AUCustomsDataRegistry.ImportDeclarationCategory,
				"Total Unknown Freight and Insurance Percentage",
				"This is the total percentage allowable for OFT and ONS when nominating the unknown Freight and Insurance percentages.  Enter 0% to disable the Unknown percentage function for OFT.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				10, 0, 100);
		}

		public void TestActivateAlternatePartShipmentModel()
		{
			TestRegistryItem(ItemSet.ActivateAlternatePartShipmentModel,
				"ActivateAlternatePartShipmentModel",
				AUCustomsDataRegistry.AirCargoCategory,
				"Activate Alternate Part Shipment Model",
				"The Alternate Part-shipment Model involves sending a ‘Consignment Reference’ number with Air Cargo Reports. This number is used to match AIRCRs with FIDs and SACs. Users must be authorised by Australian Customs to use this feature, and is primarily intended for use by the major Air Couriers. If this feature is activated, but authorisation has not been obtained from Australian Customs, then all Air Cargo reports utilising this feature will be rejected by Customs.",
				RegistryStorageFlags.Company,
				false);
		}

		public void TestAttachOrphanedCARSTsWhenCusHAWBCreated()
		{
			TestRegistryItem(ItemSet.AttachOrphanedCARSTsWhenCusHAWBCreated,
				"AttachOrphanedCARSTsWhenCusHAWBCreated",
				AUCustomsDataRegistry.AirCargoCategory,
				"Attach Orphaned CARSTs When HAWB Created",
				"When a new HAWB is added to a non-HVLV Air Cargo job, any unattached CARST messages (related to the house bill) will be attached to the new HAWB job.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				true);
		}

		public void TestAttachOrphanedCARSTsWhenCusSCAHouseCreated()
		{
			TestRegistryItem(ItemSet.AttachOrphanedCARSTsWhenCusSCAPivotCreated,
				"AttachOrphanedCARSTsWhenCusSCAHouseCreated",
				AUCustomsDataRegistry.SeaCargoCategory,
				"Attach Orphaned CARSTs When Package Created",
				"When a new Package is added to an Sea Cargo job, any unattached CARST messages (related to the package) will be attached to the new Package.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				true);
		}

		public void TestUseSenderReferenceToFilterSeaCargoReport()
		{
			TestRegistryItem(ItemSet.UseSenderReferenceToFilterSeaCargoReport,
				"UseSenderReferenceToFilterSeaCargoReport",
				AUCustomsDataRegistry.SeaCargoCategory,
				"Use Sender Reference To Filter Sea Cargo Report",
				"When import a CMR message, use the sender reference of message to find an expected Sea Cargo Report data.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				true);
		}

		public void TestSendAirCargoMessagesInBackGroundDefault()
		{
			TestRegistryItem(ItemSet.SendAirCargoMessagesInBackGroundDefault,
				"SendAirCargoMessagesInBackGroundDefault",
				AUCustomsDataRegistry.AirCargoCategory,
				"Send Air Cargo messages in background default",
				"If set to YES then an option will be available to generate and delay sending of Air Cargo messages, in the background, and until out-of-hours.",
				RegistryStorageFlags.Company,
				false);
		}

		public void TestManifestSACOverride()
		{
			TestRegistryItem(ItemSet.ManifestSACOverride,
				"ManifestSACOverride",
				AUCustomsDataRegistry.AirCargoCategory,
				"Ignore Thesaurus description when creating HVLV cargo reports",
				"If set to YES and the manifest value is under the threshold, the Thesaurus word matching will be ignored and the SAC indicator will be set.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController,
				false);
		}

		public void TestDefaultAirConsolResponsibleParty()
		{
			TestRegistryItem(ItemSet.DefaultAirConsolResponsibleParty,
				"DefaultAirConsolResponsibleParty",
				AUCustomsDataRegistry.AirCargoCategory,
				"Default Air Consol Responsible Party?",
				"The Responsible Party on the Consol Air Cargo tab normally defaults to the organisation set as the Proxy Organisation on the current company. If you override this setting and set it to NO then the Responsible Party on the Consol Air Cargo tab will not be defaulted and must set it manually before sending Air Cargo messages.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTmpOrTempPath", Justification = "I need file name only not file itself")]
		public void TestLoadFromOutturnScanFilePath()
		{
			AssertEquals("Default Value", "", ItemSet.LoadFromOutturnScanFilePath.DefaultValue);
			AssertEquals("Registry Options", RegistryOptions.IsOnlyForSupport, ItemSet.LoadFromOutturnScanFilePath.Options);
			ItemSet.LoadFromOutturnScanFilePath.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, @"c:\temp\scan\ScannerOutput.csv"); // I need file name only not file itself
			AssertEquals("Assigned Value", @"c:\temp\scan\ScannerOutput.csv", ItemSet.LoadFromOutturnScanFilePath.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); // I need file name only not file itself
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1097:DoNotHardcodeTmpOrTempPath", Justification = "I need file name only not file itself")]
		public void TestSaveToOutturnScanFileFolder()
		{
			AssertEquals("Default Value", "", ItemSet.SaveToOutturnScanFileFolder.DefaultValue);
			AssertEquals("Registry Options", RegistryOptions.IsOnlyForSupport, ItemSet.SaveToOutturnScanFileFolder.Options);
			ItemSet.SaveToOutturnScanFileFolder.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, @"c:\temp\scan\"); // I need file name only not file itself

			AssertEquals("Assigned Value", @"c:\temp\scan\", ItemSet.SaveToOutturnScanFileFolder.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); // I need file name only not file itself
		}

		public void TestAirCargoOutturnScanningCONDCLEARReleaseStatus()
		{
			AssertEquals("Default Value", "HELD", ItemSet.AirCargoOutturnScanningCONDCLEARReleaseStatus.DefaultValue);

			ItemSet.AirCargoOutturnScanningCONDCLEARReleaseStatus.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLEAR");
			AssertEquals("Assigned Value", "CLEAR", ItemSet.AirCargoOutturnScanningCONDCLEARReleaseStatus.Value);
		}

		public void TestCreatedTimeOFLastHeldAirCargoMessage()
		{
			TestGenericRegistryItem(ItemSet.CreatedTimeOFLastHeldAirCargoMessage,
				"AUCreatedTimeOFLastHeldAirCargoMessage",
				AUCustomsDataRegistry.AirCargoCategory,
				"Created Time Of Last Held AirCargo Message",
				"Created Time Of Last Held AirCargo Message.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				DateTime.MinValue);
		}

		public void TestDefaultAirCargoConsignee()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("CON", "Consignee");
			expectedList.AddPair("DEL", "Deliver To");
			expectedList.AddPair("NON", "None");

			TestRegistryItem(ItemSet.DefaultAirCargoConsignee,
				"DefaultAirCargoConsignee",
				AUCustomsDataRegistry.AirCargoCategory,
				"Default Air Consol Consignee",
				"This determines what party will be used to populate the Consignee Organisation in an Air Cargo Report that is linked to a Consolidation.",
				RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				expectedList,
				Core.Constants.AUCustoms.DefaultConsigneeOption.DeliverTo);
		}

		public void TestSeaCargoOutturnScanningCONDCLEARReleaseStatus()
		{
			AssertEquals("Default Value", "CLEAR", ItemSet.SeaCargoOutturnScanningCONDCLEARReleaseStatus.DefaultValue);

			ItemSet.SeaCargoOutturnScanningCONDCLEARReleaseStatus.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "HELD");
			AssertEquals("Assigned Value", "HELD", ItemSet.SeaCargoOutturnScanningCONDCLEARReleaseStatus.Value);
		}

		public void TestDefaultSeaCargoConsignee()
		{
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("CON", "Consignee");
			expectedList.AddPair("DEL", "Deliver To");
			expectedList.AddPair("NON", "None");

			TestRegistryItem(ItemSet.DefaultSeaCargoConsignee,
				"DefaultSeaCargoConsignee",
				AUCustomsDataRegistry.SeaCargoCategory,
				"Default Sea Consol Consignee",
				"This determines what party will be used to populate the Consignee Organisation in a Sea Cargo Report that is linked to a Consolidation.",
				RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				expectedList,
				Core.Constants.AUCustoms.DefaultConsigneeOption.DeliverTo);
		}

		public void TestCreatedTimeOFLastHeldSeaCargoMessage()
		{
			TestGenericRegistryItem(ItemSet.CreatedTimeOFLastHeldSeaCargoMessage,
				"AUCreatedTimeOFLastHeldSeaCargoMessage",
				AUCustomsDataRegistry.SeaCargoCategory,
				"Created Time Of Last Held SeaCargo Message",
				"Created Time Of Last Held SeaCargo Message.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				DateTime.MinValue);
		}

		public void TestDefaultBranchForTransferIn()
		{
			TestRegistryItem(ItemSet.DefaultBranchForTransferIn,
				"DefaultBranchForTransferIn",
				AUCustomsDataRegistry.AQISDeclarationSubCategory,
				"Default Branch For Transfer In",
				"The branch under which a Quarantine job will be created if the RFP or Client Reference number of an RFP Transfer In cannot be found on any existing Quarantine job.",
				RegistryStorageFlags.System,
				RegistryOptions.IsValueMandatory,
				RegistryFindBoxCollection.GlbBranch,
				Guid.Empty);
		}

		public void TestEXDOCTestEmailAddress()
		{
			TestGenericRegistryItem(ItemSet.EXDOCTestEmailAddress,
				"EXDOCTestEmailAddress_1511",
				AUCustomsDataRegistry.AQISDeclarationSubCategory,
				"Email address for test EXDOC messages",
				"The Email address to which EXDOC TEST messages will be sent.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
			AssertEquals("Previous value", "edi.test@daff.gov.au", AUCustomsDataRegistry.Instance.EXDOCTestEmailAddress.Value.PreviousValue);
			AssertEquals("New value", "edi.test@agriculture.gov.au", AUCustomsDataRegistry.Instance.EXDOCTestEmailAddress.Value.NewValue);
			AssertEquals("Effective Date", new ZDateTime(2015, 11, 17), AUCustomsDataRegistry.Instance.EXDOCTestEmailAddress.Value.EffectiveDate);
		}

		public void TestEXDOCProdEmailAddress()
		{
			TestGenericRegistryItem(ItemSet.EXDOCProdEmailAddress,
				"EXDOCProdEmailAddress_1512",
				AUCustomsDataRegistry.AQISDeclarationSubCategory,
				"Email address for live production EXDOC messages",
				"The Email address to which EXDOC live production messages will be sent.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
			AssertEquals("Previous value", "edi.prod@agriculture.gov.au", AUCustomsDataRegistry.Instance.EXDOCProdEmailAddress.Value.PreviousValue);
			AssertEquals("New value", "EXDOC@mail.p3.awe.gov.au", AUCustomsDataRegistry.Instance.EXDOCProdEmailAddress.Value.NewValue);
			AssertEquals("Effective Date", new ZDateTime(2022, 07, 10), AUCustomsDataRegistry.Instance.EXDOCProdEmailAddress.Value.EffectiveDate);
		}

		public void TestErrata44EffectiveDate()
		{
			TestGenericRegistryItem(ItemSet.Errata44EffectiveDate,
				"Errata44EffectiveDate_1703",
				AUCustomsDataRegistry.AQISDeclarationSubCategory,
				AUCustomsDataRegistry.Errata44EffectiveDateCaption,
				AUCustomsDataRegistry.Errata44EffectiveDateHint,
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				new DateTime(2017, 03, 31));
		}

		public void TestMaximumInterchangeSize()
		{
			TestRegistryItem(ItemSet.MaximumInterchangeSize, "MaximumInterchangeSize", AUCustomsDataRegistry.CMRCategory, "Maximum Interchange Size (in bytes)", "The maximum size (in bytes) that can be transmitted as a single interchange. If this size is exceeded, the unprocessed messages will be processed in the next batch.", RegistryStorageFlags.Company, 10000000);
		}

		public void TestIgnoreUnknownResponses()
		{
			TestRegistryItem(ItemSet.IgnoreUnknownResponses,
				"IgnoreUnknownResponses",
				AUCustomsDataRegistry.CMRCategory,
				"Ignore Unknown/Unassociated inbound CMR messages?",
				"If a messages is received from CMR that cannot be linked with any existing job then this is normally notified via email to the notify errors group for that type of message. If this registry item is changed from the default NO to YES then these messages will be ignored and no notification sent.",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		}

		public void TestAUReferenceFileAlternateDownloadURL()
		{
			TestGenericRegistryItem(ItemSet.AUReferenceFileAlternateDownloadURL,
				"AUReferenceFileAlternateDownloadURL",
				RawDataRegistry.Categories.Customs_Australia_ReferenceFiles,
				"Alternate Download URL",
				"URL to the WiseTech Global CMR Reference File mirror for Australian Customs.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				"https://myaccount-portal.cargowise.com/my-account/public/customs/");
		}

		public void TestUseRefDatabaseData()
		{
			TestRegistryItem(
				ItemSet.UseRefDatabaseData,
				"AUUseRefDatabaseData",
				RawDataRegistry.Categories.Customs_Australia,
				"Use RefDatabase Data",
				"Setting this to True will force CW1 to use the reference data in CW-RefDatabase instead of the various tables in RefDb_CMR_AU. The list of what has and hasn't been added can be found in WI00569357.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestMaximumSeaOutturnLinesPerMessage()
		{
			TestRegistryItem(ItemSet.MaximumSeaOutturnLinesPerMessage,
				"MaximumSeaOutturnLinesPerMessage",
				AUCustomsDataRegistry.SeaCargoCategory,
				"Maximum Number of Sea Outturn Lines per message",
				"The maximum number of sea outturn lines per message, accepted by Customs.",
				RegistryStorageFlags.System,
				999);
			var editorInfo = (NumericRegistryEditorInfo)ItemSet.MaximumSeaOutturnLinesPerMessage.EditorInfo;
			AssertEquals(0, editorInfo.DecimalPlaces);
		}

		public void TestEnableDebugHooksForAU()
		{
			TestRegistryItem(ItemSet.EnableDebugHooksForAU,
				"EnableDebugHooksForAU",
				AUCustomsDataRegistry.DebugCategory,
				"Enable Debug Hooks?",
				"Debugging features and logging will be turned on.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestNEXDOCSGroupToken()
		{
			TestGenericRegistryItem(ItemSet.NEXDOCSGroupToken,
				"NEXDOCSGroupToken",
				AUCustomsDataRegistry.NEXDOCSCategory,
				"NEXDOCS Group Token",
				"This password will be used as a company credential for NEXDOCS.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForController);
		}

		public void TestNEXDOCSSendCredentialMessages()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "ABC";

			Factory.Save();

			var registry = AUCustomsDataRegistry.Instance;

			registry.OnUpdateAction(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NGT { Password = "pwd" });
			registry.NEXDOCSGroupToken.OnAllValuesSaved();

			var zQuery = new ZQuery(EDIInterchangeSchema.EI_To, "eHub");
			zQuery.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + " DESC";
			var xml = Factory.LoadTop1<EDIInterchange>(zQuery);

#if NETFRAMEWORK
			var expectedPrefix = @"<Configuration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Name=""NEXDOCSSystemLevel"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">";
#else
			var expectedPrefix = @"<Configuration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Name=""NEXDOCSSystemLevel"" Version=""1.0"" xmlns=""http://www.wisetechglobal.com/Schemas/Configuration"">";
#endif

			var expectedXml = $@"{expectedPrefix}
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""NGT"" Status=""VAL"">
        <Credential Name=""Current"">
          <Password>Password</Password>
        </Credential>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertNotNull("One message should be generated", xml);
			AssertEquals("The XML should be properly generated with the credential", expectedXml, Regex.Replace(xml.EI_BodyText, "<Password>.*</Password>", "<Password>Password</Password>"));

			registry.OnUpdateAction(company.PK.ToGuid(), Guid.Empty, Guid.Empty, new NGT { Password = "" });

			registry.NEXDOCSGroupToken.OnAllValuesSaved();

			xml = Factory.LoadTop1<EDIInterchange>(zQuery);
			var expectedEmptyXml = $@"{expectedPrefix}
  <Group Type=""System"" Reference=""EDIDAT"">
    <Group Type=""Company"" Reference=""ABC"">
      <Group Type=""NGT"" Status=""VAL"">
        <Credential Name=""Current"">
          <Password />
        </Credential>
      </Group>
    </Group>
  </Group>
</Configuration>";

			AssertEquals("The empty XML should be properly generated", expectedEmptyXml, xml.EI_BodyText);
		}

		public void TestDisableDSAMessages()
		{
			TestRegistryItem(ItemSet.DisableDSAMessages,
				"DisableDSAMessages",
				RawDataRegistry.Categories.Customs_Australia_Testing,
				"Disable DSA messages",
				"All EDIMessages where the EM_ApplicationCode = CMR, EM_MessageType = DSA and EM_ReceiveTransmit = RCV will have their EM_Status set to DCD instead of QUE",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestIsTestSystem()
		{
			TestRegistryItem(ItemSet.NEXDOCSTestingSystem,
				"IsNEXDOCSTesting",
				AUCustomsDataRegistry.NEXDOCSCategory,
				"Is NEXDOCS Testing System?",
				"NEXDOCS messages be sent to the test rather than production system?",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableNewPackTypeConversions()
		{
			TestRegistryItem(ItemSet.EnableNewPackTypeConversions,
				"EnableNewPackTypeConversions",
				RawDataRegistry.Categories.Customs_Australia_Testing,
				"Enable New Pack Type Conversions",
				"This will enable the functionality that adds new pack type conversions being created in WI00672052.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestUseCMRTariffTestData()
		{
			TestRegistryItem(ItemSet.UseCMRTariffTestData,
				"UseCMRTariffTestData",
				RawDataRegistry.Categories.Customs_Australia_Testing,
				"Use CMR Tariff Test Data",
				"Enabling this option will force the system to use the CMR Industry_Test data files for tariffs instead of production.",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestAlwaysUseIndustryTestCMRFiles()
		{
			TestRegistryItem(ItemSet.AlwaysUseIndustryTestCMRFiles,
				"AlwaysUseIndustryTestCMRFiles",
				RawDataRegistry.Categories.Customs_Australia_Testing,
				"Always Use Industry Test CMR Files",
				"This will force the system to ignore the SharedRefDb restrictions and always use the Industry_Test.Q1-MAIN.tar.gz CMR file.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			AssertEquals("Is production System", true, EnvProxy.Instance.IsProductionSystem);

			var useIndustryTestCMRFilesItem = AUCustomsDataRegistry.Instance.FindByName("AlwaysUseIndustryTestCMRFiles") as BooleanRegistryItem;
			AssertNoExceptionThrown(() => useIndustryTestCMRFilesItem.DataType.ValidateBeforeRegistryFormSave(useIndustryTestCMRFilesItem, false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

			var exception = AssertExceptionThrown<RegistryValidationException>(() => useIndustryTestCMRFilesItem.DataType.ValidateBeforeRegistryFormSave(useIndustryTestCMRFilesItem, true, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertEquals("Industry Test CMR Files cannot be used in a production system.", exception.Message);

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);
			AssertNoExceptionThrown(() => useIndustryTestCMRFilesItem.DataType.ValidateBeforeRegistryFormSave(useIndustryTestCMRFilesItem, false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
			AssertNoExceptionThrown(() => useIndustryTestCMRFilesItem.DataType.ValidateBeforeRegistryFormSave(useIndustryTestCMRFilesItem, true, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
		}

		public void TestCMRContingencyDataEmailAddresses()
		{
			CodeDescriptionPairList defaultValue = ItemSet.CMRContingencyDataEmailAddressesRaw.DefaultValue.GetCodeDescriptionPairList();

			AssertEquals("DefaultValue.Count", 1, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"Primary BCP Mail Address\")", "ICSBCP@customs.gov.au", defaultValue.GetDescriptionFromCode("Primary BCP Mail Address"));

			ContingencyDataEmailAddressCollection collection = new ContingencyDataEmailAddressCollection();
			ContingencyDataEmailAddress address = collection.AddNew();
			address.Code = "J";
			address.Description = (NoResString)"J@G.COM";

			ItemSet.CMRContingencyDataEmailAddressesRaw.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			CodeDescriptionPairList cMRContingencyDataEmailAddresses = ItemSet.CMRContingencyDataEmailAddresses;
			AssertEquals("CMRContingencyDataEmailAddresses.Count", 1, cMRContingencyDataEmailAddresses.Count);
			AssertEquals("CMRContingencyDataEmailAddresses.GetDescriptionFromCode(\"J\")", "J@G.COM", cMRContingencyDataEmailAddresses.GetDescriptionFromCode("J"));

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("O", "O@P.COM");
			list.AddPair("B", "B@C.COM");

			ItemSet.CMRContingencyDataEmailAddresses = list;
			cMRContingencyDataEmailAddresses = ItemSet.CMRContingencyDataEmailAddresses;
			AssertEquals("CMRContingencyDataEmailAddresses.Count", 2, cMRContingencyDataEmailAddresses.Count);
			AssertEquals("CMRContingencyDataEmailAddresses.GetDescriptionFromCode(\"O\")", "O@P.COM", cMRContingencyDataEmailAddresses.GetDescriptionFromCode("O"));
			AssertEquals("CMRContingencyDataEmailAddresses.GetDescriptionFromCode(\"B\")", "B@C.COM", cMRContingencyDataEmailAddresses.GetDescriptionFromCode("B"));
		}

		public void TestAllRegistryItemsHaveAustraliaCountryFilter()
		{
			foreach (IRegistryItem registryItem in AllItems)
			{
				Assert(registryItem.Name + ".CountryFilterPK", registryItem.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Australia));
			}
		}

		public void TestCMRIsBureau()
		{
			AssertEquals("Default Value", false, ItemSet.CMRIsBureau.DefaultValue);
			ItemSet.CMRIsBureau.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Default Value", true, ItemSet.CMRIsBureau.Value);
			ItemSet.CMRIsBureau.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Default Value", false, ItemSet.CMRIsBureau.Value);
		}

		public void TestIncludeAQISServicePaymentsInTotalPayableOnEntryPrint()
		{
			AssertEquals("Default Value", true, ItemSet.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint.DefaultValue);
			ItemSet.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Default Value", false, ItemSet.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint.Value);
			ItemSet.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Default Value", true, ItemSet.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint.Value);
		}

		public void TestSoleTrader()
		{
			TestRegistryItem(ItemSet.SoleTrader, "SoleTrader", "Customs/Country or Region Specific/Australia", "Sole Trader", "Company listed is nominated as a sole trader for customs purposes.", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, false);
		}

		public void TestMaximumNumberOfEntryLines()
		{
			TestRegistryItem(ItemSet.MaxNumberOfEntryLinesAcceptedAtCustoms, "MaxNumberOfEntryLinesAcceptedAtCustoms", "Customs/Country or Region Specific/Australia", "Maximum Number of Entry Lines Accepted at Customs", "The maximum number of entry lines accepted at Customs", RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, 1000);
		}

		public void TestCMRReferenceFilesUpdateNotificationGroup()
		{
			AssertEquals("Default Value", Guid.Empty, ItemSet.CMRReferenceFilesUpdateNotificationGroup.DefaultValue);
			Assert("CountryFilterPK", ItemSet.CMRReferenceFilesUpdateNotificationGroup.CountryFilterPKs.Contains(Core.Constants.CountryGuids.Australia));
			Guid testValue = Guid.NewGuid();
			ItemSet.CMRReferenceFilesUpdateNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testValue);
			AssertEquals("Assigned Value", testValue, ItemSet.CMRReferenceFilesUpdateNotificationGroup.Value);
		}

		public void TestDefaultPremiseIDs()
		{
			DefaultPremiseIDCollection dischargeCollection = new DefaultPremiseIDCollection();
			DefaultPremiseID dischargeItem = dischargeCollection.AddNew();
			dischargeItem.AirlineCode = "QF";
			dischargeItem.PortOfDischarge = "AUSYD";
			dischargeItem.PremiseID = "9914N";
			ItemSet.DefaultDischargePremiseIDs = dischargeCollection;

			DefaultPremiseIDCollection dischargeRegistryCollection = ItemSet.DefaultDischargePremiseIDs;
			AssertEquals("Count", 1, dischargeRegistryCollection.Count);
			DefaultPremiseID dischargeRegistryItem = dischargeCollection[0];
			AssertEquals("Airline Code", "QF", dischargeRegistryItem.AirlineCode);
			AssertEquals("Port Of Discharge", "AUSYD", dischargeRegistryItem.PortOfDischarge);
			AssertEquals("Premise ID", "9914N", dischargeRegistryItem.PremiseID);

			var destinationCollection = new DefaultDestinationPremiseIDCollection();
			var destinationItem = destinationCollection.AddNew();
			destinationItem.AirlineCode = "QF";
			destinationItem.PortOfDischarge = "AUSYD";
			destinationItem.PremiseID = "1234X";
			destinationItem.UseDischargePort = true;
			ItemSet.DefaultDestinationPremiseIDs = destinationCollection;

			var destinationRegistryCollection = ItemSet.DefaultDestinationPremiseIDs;
			AssertEquals("Count", 1, destinationRegistryCollection.Count);
			var destinationRegistryItem = destinationCollection[0];
			AssertEquals("Airline Code", "QF", destinationRegistryItem.AirlineCode);
			AssertEquals("Port Of Discharge", "AUSYD", destinationRegistryItem.PortOfDischarge);
			AssertEquals("Premise ID", "1234X", destinationRegistryItem.PremiseID);
			Assert(destinationRegistryItem.UseDischargePort);
		}

		public void TestBranchForAutomaticUnderbonds()
		{
			AssertEquals("Default Value", "", ItemSet.BranchForAutomaticUnderbonds.DefaultValue);
			ItemSet.BranchForAutomaticUnderbonds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "bne");
			AssertEquals("Assigned Value", "bne", ItemSet.BranchForAutomaticUnderbonds.Value);
		}

		public void TestSendAQISAcknowledgements()
		{
			TestRegistryItem(ItemSet.SendAQISAcknowledgements,
				"SendAQISAcknowledgements",
				AUCustomsDataRegistry.Categories.Customs_Australia + "/Quarantine Declaration",
				"Send Quarantine Declaration Acknowledgements",
				"Send message acknowledgements to user",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
		}

		public void TestSendAQISAcknowledgementsToGroup()
		{
			TestRegistryItem(ItemSet.SendAQISAcknowledgementsToGroup,
				"SendAQISAcknowledgementsToGroup",
				AUCustomsDataRegistry.Categories.Customs_Australia + "/Quarantine Declaration",
				"Group To Send Quarantine Declaration Acknowledgements To",
				"Send message acknowledgements to group",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue,
				RegistryFindBoxCollection.GlbGroup,
				Guid.Empty);
		}

		public void TestSendAQISErrors()
		{
			TestRegistryItem(ItemSet.SendAQISErrors,
				"SendAQISErrors",
				AUCustomsDataRegistry.Categories.Customs_Australia + "/Quarantine Declaration",
				"Send Quarantine Declaration Errors",
				"Send message errors to user",
				RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
		}

		public void TestSendAQISErrorsToGroup()
		{
			TestRegistryItem(ItemSet.SendAQISErrorsToGroup,
				"SendAQISErrorsToGroup",
				AUCustomsDataRegistry.Categories.Customs_Australia + "/Quarantine Declaration",
				"Group To Send Quarantine Declaration Errors To",
				"Send message errors to group",
				RegistryStorageFlags.Company,
				RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue,
				RegistryFindBoxCollection.GlbGroup,
				Guid.Empty);
		}

		public void TestSendAQISImpediments()
		{
			TestRegistryItem(ItemSet.SendAQISImpediments,
				"SendAQISImpediments",
				AUCustomsDataRegistry.Categories.Customs_Australia + "/Quarantine Declaration",
				"Send Quarantine Declaration Impediments",
				"Send message impediments to user",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				new CodeDescriptionPairList(OLookUpEditType.EmailTo),
				Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
		}

		public void TestSendAQISImpedimentsToGroup()
		{
			TestRegistryItem(ItemSet.SendAQISImpedimentsToGroup,
				"SendAQISImpedimentsToGroup",
				AUCustomsDataRegistry.Categories.Customs_Australia + "/Quarantine Declaration",
				"Group To Send Quarantine Declaration Impediments To",
				"Send message impediments to group",
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.IsValueMandatory | RegistryOptions.PreserveTestValue,
				RegistryFindBoxCollection.GlbGroup,
				Guid.Empty);
		}

		public void TestEnableAQISDeclarationMessaging()
		{
			TestRegistryItem(ItemSet.EnableAQISDeclarationMessaging,
				"EnableAQISDeclarationMessaging",
				AUCustomsDataRegistry.Categories.Customs_Australia + "/Quarantine Declaration",
				"Enable Quarantine Declaration Messaging",
				"WARNING - Activating this will result in a charge per transaction, please contact your WiseTech Global Sales Representative for details.",
				RegistryStorageFlags.Company,
				RegistryOptions.PreserveTestValue,
				false);
		}

		public void TestAllowOverrideOfCustomsUnitsOnDeclarations()
		{
			TestRegistryItem(
				ItemSet.AllowOverrideOfCustomsUnitsOnDeclarations,
				"AllowOverrideOfCustomsUnitsOnDeclarations",
				AUCustomsDataRegistry.Categories.Customs_Australia,
				"Allow Override of Customs Units on Declarations",
				"Allow Override of Customs Units on Declarations if true.",
				RegistryStorageFlags.Company,
				false);
		}

		public void TestSeaMandatoryLatestCargoReportingTimeframe()
		{
			TestRegistryItem(
				ItemSet.SeaMandatoryLatestCargoReportingTimeframe,
				"SeaMandatoryLatestCargoReportingTimeframe",
				AUCustomsDataRegistry.Categories.Customs_Australia + "/Sea Cargo",
				"Sea Mandatory Latest Cargo Reporting Timeframe (hours)",
				"The Sea Mandatory Latest Cargo Reporting Timeframe is the timeframe, specified in hours by Australian Customs, which defines the latest time within which a Cargo Report must be lodged for Sea Freight shipments.",
				RegistryStorageFlags.Company,
				48);
		}

		public void TestAirMandatoryLatestCargoReportingTimeframe()
		{
			TestRegistryItem(
				ItemSet.AirMandatoryLatestCargoReportingTimeframe,
				"AirMandatoryLatestCargoReportingTimeframe",
				AUCustomsDataRegistry.Categories.Customs_Australia + "/Air Cargo",
				"Air Mandatory Latest Cargo Reporting Timeframe (hours)",
				"The Air Mandatory Latest Cargo Reporting Timeframe is the timeframe, specified in hours by Australian Customs, which defines the latest time within which a Cargo Report must be lodged for Air Freight shipments.",
				RegistryStorageFlags.Company,
				4);
		}

		public void TestAdditionalEDIMessageSender()
		{
			TestRegistryItem(
				ItemSet.AdditionalEDIMessageSender,
				"AdditionalEDIMessageSender",
				AUCustomsDataRegistry.Categories.Customs_Australia,
				"Additional EDI Message Sender",
				"Any inbound messages with a from address containing this field will also be selected for message processing",
				RegistryStorageFlags.Company,
				RegistryOptions.IsOnlyForDevelopers,
				TextEditorType.TextBox, "");
		}

		public void TestSendAutoSEQRequests()
		{
			TestRegistryItem(
				ItemSet.SendAutoSEQRequests,
				"SendAutoSEQRequests",
				AUCustomsDataRegistry.Categories.Customs_Australia_CFS,
				"Automatically send SEQ requests",
				"If this registry setting is enabled, then an SEQ (Sea Cargo Establishment Information Request) message will be automatically sent whenever an Expected Cargo Arrival Advice, or a Container Level CARST, is received.",
				RegistryStorageFlags.Company,
				true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestCreateCFSShipmentWhenCustomsStatusReceivedForUnknownShipment()
		{
			TestRegistryItem(
				ItemSet.CreateCFSShipmentWhenCustomsStatusReceivedForUnknownShipment,
				"CreateCFSShipmentWhenCustomsStatusReceivedForUnknownShipment",
				AUCustomsDataRegistry.Categories.Customs_Australia_CFS,
				"Create CFS Shipment When Customs Status Received For Unknown Shipment",
				"If this item is set then CargoWise One will automatically create a CFS Shipment on the appropriate Load List when Customs Status is received for an unknown shipment.",
				RegistryStorageFlags.Company,
				true);
		}

		public void TestEnableValidationTool()
		{
			TestRegistryItem(ItemSet.EnableValidationTool,
				"EnableValidationTool",
				RawDataRegistry.Categories.Customs_Australia_Testing,
				"Enable Validation Tool",
				"This will enable the macro-configurable validation functionality being created in WI00713510.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		public void TestPrintEntryWhenPrintingInvoice()
		{
			TestRegistryItem(
				ItemSet.PrintEntryWhenPrintingInvoice,
				"PrintEntryWhenPrintingInvoice",
				AUCustomsDataRegistry.Categories.Customs_Australia,
				Res.GetString("c9b83d8e-0de1-4bdd-a1aa-4c3bcabc3e08", "Print Entry when Printing Invoice"),
				Res.GetString("ee63de1c-9b97-4db9-9558-5d9b3d057ee2", "Print Entry Print when printing Invoices with Disbursement Charges."),
				RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.PreserveTestValue,
				true);
		}

		public void TestEnableNEXDOCForInedibleMeat()
		{
			TestRegistryItem(ItemSet.EnableNEXDOCForInedibleMeat,
				"EnableNEXDOCForInedibleMeat",
				RawDataRegistry.Categories.Customs_Australia_Testing,
				"Enable NEXDOC for Inedible Meat",
				"This enables NEXDOC functionality for the Inedible Meat produce type while it is under development. Refer to WI00884518 - [Lead Job] NEXDOC Inedible Meat for more info.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForDevelopers,
				false);
		}

		protected override bool IsCountrySpecificRegistrySet => true;
	}
}
