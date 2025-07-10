using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class AUCustomsDataRegistry : RegistryItemSet, IAUCustomsRegistry
	{
		public static AUCustomsDataRegistry Instance
		{
			get { return instance ?? (instance = new AUCustomsDataRegistry()); }
		}

		[ThreadStatic]
		static AUCustomsDataRegistry instance;

		AUCustomsDataRegistry()
		{
		}

		public override bool IsForProductivityWise => false;

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_Australia_AirCargo_UnderbondMovementRequest { get { return CombineCategories(Customs_Australia_AirCargo, ResString.GetMultilingualString("Underbond Movement Request", "Underbond Movement Request")); } }
			public static MultilingualString Customs_Australia_AQISDeclaration { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("Quarantine Declaration", "Quarantine Declaration")); } }
			public static MultilingualString Customs_Australia_CFS { get { return CombineCategories(Customs_Australia, ResString.GetMultilingualString("CFS", "CFS")); } }
		}

		public IntRegistryItem BackgroundCargoReportMaxMsg
		{
			get
			{
				return GetItem("AUBackgroundCargoReportMaxMsg", delegate
				{
					return new IntRegistryItem(
						"AUBackgroundCargoReportMaxMsg",
						RawDataRegistry.Categories.Customs_Australia,
						ResString.GetMultilingualString("577BEF70-25A7-42B5-A896-30F823300A5C", "Background Cargo Report maximum messages"),
						ResString.GetMultilingualString("56C20144-7913-448D-A200-35820D18F933", "Specify the maximum number of EDI messages to be sent to Customs over the defined interval for background Cargo Reporting process."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						300,
						0,
						15000);
				});
			}
		}

		public IntRegistryItem BackgroundCargoReportSubThrottleWindow
		{
			get
			{
				return GetItem("AUBackgroundCargoReportSubmissionThrottleWindow", delegate
				{
					return new IntRegistryItem(
						"AUBackgroundCargoReportSubmissionThrottleWindow",
						RawDataRegistry.Categories.Customs_Australia,
						ResString.GetMultilingualString("849E6477-964E-4DB1-ACFD-EEE68113A88C", "Background Cargo Report submission throttle window"),
						ResString.GetMultilingualString("C13674A9-F591-48AA-B447-8D891D286579", "Specify the throttle window in minutes for the restriction of EDI Cargo Report messages to Customs."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						1,
						0,
						60);
				});
			}
		}

		public BooleanRegistryItem EnableAUServiceTaskCheckForSendingMessage
		{
			get
			{
				return GetItem("EnableAUServiceTaskCheckForSendingMessage", () => new BooleanRegistryItem(
						"EnableAUServiceTaskCheckForSendingMessage",
						RawDataRegistry.Categories.Customs_Australia,
						(NoResString)"Enable AU Service Task Check For Sending Message",
						(NoResString)"The system will check whether the required service task is running before sending any message",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true));
			}
		}

		public DateTimeRegistryItem ICSReleaseEffectiveDate
		{
			get
			{
				return GetItem("ICSReleaseEffectiveDate", () => new DateTimeRegistryItem(
					"ICSReleaseEffectiveDate",
					RawDataRegistry.Categories.Customs_Australia,
					(NoResString)"ICS Release 17.4.02 effective date",
					(NoResString)"ICS Release 17.4.02 effective date.",
					new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController,
					new DateTime(2017, 6, 14),
					false));
			}
		}

		public BooleanRegistryItem UseCustomsReferenceData
		{
			get
			{
				return GetItem("UseCustomsReferenceData", delegate
				{
					return new BooleanRegistryItem(
						"UseCustomsReferenceData",
						RawDataRegistry.Categories.Customs_Australia,
						ResString.GetMultilingualString("1785E52D-4991-4CFD-ABDD-CB70D53D819B", "Use Customs Reference Data"),
						ResString.GetMultilingualString("2970D3FE-B8DF-47F6-8868-183A8DD9D0F2", "Setting this to Yes will use the Nomenclature reference data captured by Customs and not BorderWise.  Please do not change this without contacting CW1 support first."),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		IRegistryItem IAUCustomsRegistry.UseCustomsReferenceData => UseCustomsReferenceData;

		public BooleanRegistryItem EnableCWRefForAHECC
		{
			get
			{
				return GetItem("EnableCWRefForAHECC", delegate
				{
					return new BooleanRegistryItem(
						"EnableCWRefForAHECC",
						RawDataRegistry.Categories.Customs_Australia_Testing,
						ResString.GetMultilingualString("265D7497-42B7-4FAB-AA48-8FDF2B7BBD92", "Enable CW-Ref for AHECC"),
						ResString.GetMultilingualString("CF0B79D4-0088-49EA-A354-DD8D841F1BEA", "Setting this to True will force CW1 to use the AHECC tariff data from CW-Ref instead of TRF-AU."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		IRegistryItem IAUCustomsRegistry.EnableCWRefForAHECC => EnableCWRefForAHECC;

		public StringRegistryItem LocalContactPhoneNumber
		{
			get
			{
				return GetItem("LocalContactPhoneNumber", delegate
				{
					var dataType = new NumericOnlyStringRegistryDataType()
					{
						MinLength = 0,
						MaxLength = 25
					};
					return new StringRegistryItem(
						"LocalContactPhoneNumber",
						RawDataRegistry.Categories.Customs_Australia,
						(NoResString)"Local Contact Phone Number",
						(NoResString)"When submitting a Self-Assessed Clearance via the Customs Declaration Module (Short SAC) and no Local Customs Branch Identifier has been entered, this phone number will be used instead.",
						dataType,
						RegistryStorageFlags.Branch,
						string.Empty);
				});
			}
		}

		public BooleanRegistryItem SendAutoSEQRequests
		{
			get
			{
				return GetItem("SendAutoSEQRequests", delegate
				{
					return new BooleanRegistryItem(
						"SendAutoSEQRequests",
						Categories.Customs_Australia_CFS,
						(NoResString)"Automatically send SEQ requests",
						(NoResString)"If this registry setting is enabled, then an SEQ (Sea Cargo Establishment Information Request) message will be automatically sent whenever an Expected Cargo Arrival Advice, or a Container Level CARST, is received.",
						RegistryStorageFlags.Company, true);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public BooleanRegistryItem CreateCFSShipmentWhenCustomsStatusReceivedForUnknownShipment
		{
			get
			{
				return GetItem("CreateCFSShipmentWhenCustomsStatusReceivedForUnknownShipment", delegate
				{
					return new BooleanRegistryItem(
						"CreateCFSShipmentWhenCustomsStatusReceivedForUnknownShipment",
						Categories.Customs_Australia_CFS,
						(NoResString)"Create CFS Shipment When Customs Status Received For Unknown Shipment",
						(NoResString)"If this item is set then CargoWise One will automatically create a CFS Shipment on the appropriate Load List when Customs Status is received for an unknown shipment.",
						RegistryStorageFlags.Company, true);
				});
			}
		}

		public BooleanRegistryItem EnableValidationTool
		{
			get
			{
				return GetItem("EnableValidationTool",
					() => new BooleanRegistryItem(
						"EnableValidationTool",
						RawDataRegistry.Categories.Customs_Australia_Testing,
						(NoResString)"Enable Validation Tool",
						(NoResString)"This will enable the macro-configurable validation functionality being created in WI00713510.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false));
			}
		}

		IRegistryItem IAUCustomsRegistry.EnableValidationTool => EnableValidationTool;

		#region Import Declaration
		internal static MultilingualString ImportDeclarationCategory { get { return RawDataRegistry.Categories.Customs_Australia_ImportDeclaration; } }

		#region Import Declaration

		internal CodePairRegistryItem SendUnmatchedICSReports
		{
			get
			{
				return GetItem("SendUnmatchedICSReports", delegate
				{
					return new CodePairRegistryItem(
						"SendUnmatchedICSReports",
						ImportDeclarationCategory,
						ResString.GetMultilingualString("50F7F101-DF2B-46EC-98EE-B63437230305", "Send Unsolicited / Unmatched ICS Reports"),
						ResString.GetMultilingualString("AA8CC42F-CE79-45B2-A20D-0D3FBB183D7E", "Indicate the action to perform when an unsolicited or unmatched ICS document is received."),
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, Core.Constants.EmailTo.NoEmails);
				});
			}
		}

		public GuidRegistryItem SendUnmatchedICSReportsToGroup
		{
			get
			{
				return GetItem("SendUnmatchedICSReportsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SendUnmatchedICSReportsToGroup",
						ImportDeclarationCategory,
						ResString.GetMultilingualString("38E57DBA-952E-462B-9A53-A084BED74B82", "Group To Send Unmatched ICS Reports To"),
						ResString.GetMultilingualString("8055C5FC-66B6-4BEB-8CA9-89937A4B898A", "Indicate the email address/Group to deliver the unsolicited or unmatched ICS document to."),
						RegistryStorageFlags.Company,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		internal CodePairRegistryItem ThirdPartyRefundRejectionSendAcknowledgements
		{
			get
			{
				return GetItem("ThirdPartyRefundRejectionSendAcknowledgements", delegate
				{
					return new CodePairRegistryItem(
						"ThirdPartyRefundRejectionSendAcknowledgements",
						ImportDeclarationCategory,
						ResString.GetMultilingualString("3E0FA2DB-3C29-4993-B2FA-A6DDFB2E653A", "Send Third Party Refund Rejections"),
						ResString.GetMultilingualString("54F55862-F428-4020-80B0-5B8B4458792E", "Indicate the action to perform when an unmatched Refund Reject message is detected."),
						OLookUpEditType.EmailTo, RegistryStorageFlags.Company, Core.Constants.EmailTo.NoEmails);
				});
			}
		}

		public GuidRegistryItem ThirdPartyRefundRejectionSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("ThirdPartyRefundRejectionSendAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ThirdPartyRefundRejectionSendAcknowledgementsToGroup",
						ImportDeclarationCategory,
						ResString.GetMultilingualString("B68CFE56-A303-4116-91DA-2B0C8A9B7251", "Group To Send Third Party Refund Rejections To"),
						ResString.GetMultilingualString("53475D4E-344F-4695-B251-73BCDFD2CB13", "Indicate the email address/Group to deliver the Refund Reject message to."),
						RegistryStorageFlags.Company,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion
		public DateTimeRegistryItem UPEImplementationDate
		{
			get
			{
				return GetItem("UPEImplementationDate", delegate
				{
					return new DateTimeRegistryItem(
						"UPEImplementationDate",
						ImportDeclarationCategory,
						(NoResString)"Unaccompanied Personal Effects Implementation Date",
						(NoResString)"The date from which Unaccompanied Personal Effects may be filed along with a formal import declaration.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new DateTime(2014, 9, 10),
						false);
				});
			}
		}

		public DateTimeRegistryItem LowValueSecurityImplementationDate
		{
			get
			{
				return GetItem("LowValueSecurityImplementationDate", delegate
				{
					return new DateTimeRegistryItem(
						"LowValueSecurityImplementationDate",
						ImportDeclarationCategory,
						(NoResString)"Low Value Security Implementation Date",
						(NoResString)"The date when new Australian Customs regulations come into effect where a Security ID is not required under certain low value circumstances.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new DateTime(2012, 2, 27),
						false);
				});
			}
		}

		public IntRegistryItem LowValueSecurityDeclarationQuestion
		{
			get
			{
				return GetItem("LowValueSecurityDeclarationQuestion", delegate
				{
					return new IntRegistryItem(
							"LowValueSecurityDeclarationQuestion",
							ImportDeclarationCategory,
							(NoResString)"Low Value Security Declaration Question",
							(NoResString)"The general declaration question that must be answered when no security ID is provided for shipments that qualify for the low value security arrangements.",
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							442);
				});
			}
		}

		public DecimalRegistryItem TotalUnknownFreightandInsurancePercentage
		{
			get
			{
				return GetItem("TotalUnknownFreightandInsurancePercentage", delegate
				{
					return new DecimalRegistryItem(
							"TotalUnknownFreightandInsurancePercentage",
							ImportDeclarationCategory,
							(NoResString)"Total Unknown Freight and Insurance Percentage",
							(NoResString)"This is the total percentage allowable for OFT and ONS when nominating the unknown Freight and Insurance percentages.  Enter 0% to disable the Unknown percentage function for OFT.",
							new NumericRegistryEditorInfo(2),
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							10, 0, 100);
				});
			}
		}

		public StringRegistryItem SecurityRequiredTreatmentCodes
		{
			get
			{
				return GetItem("SecurityRequiredTreatmentCodes", delegate
				{
					return new StringRegistryItem(
						"SecurityRequiredTreatmentCodes",
						ImportDeclarationCategory,
						(NoResString)"Security-Required Treatment Codes",
						(NoResString)"Entry lines with these Treatment Codes require security (either collected or un-collected). Security related validation will be run and Duty and GST will be calculated as zero.",
						RegistryStorageFlags.System,
						"351,352,354");
				});
			}
		}

		public StringRegistryItem NoProcessingChargeTreatmentCodes
		{
			get
			{
				return GetItem("NoProcessingChargeTreatmentCodes", delegate
				{
					return new StringRegistryItem(
						"NoProcessingChargeTreatmentCodes",
						ImportDeclarationCategory,
						(NoResString)"No Processing Charge Treatment Codes",
						(NoResString)"If all entry lines of a job have these treatment codes then the Customs Entry Processing Charge will be zero.",
						RegistryStorageFlags.System,
						"354");
				});
			}
		}

		#endregion

		#region Export Declaration

		internal static MultilingualString ExportDeclarationCategory { get { return RawDataRegistry.Categories.Customs_Australia_ExportDeclaration; } }

		#endregion

		#region Air Cargo
		internal static MultilingualString AirCargoCategory { get { return RawDataRegistry.Categories.Customs_Australia_AirCargo; } }

		public DateTimeRegistryItem CreatedTimeOFLastHeldAirCargoMessage
		{
			get
			{
				return GetItem("AUCreatedTimeOFLastHeldAirCargoMessage", delegate
				{
					return new DateTimeRegistryItem(
						"AUCreatedTimeOFLastHeldAirCargoMessage",
						AirCargoCategory,
						(NoResString)"Created Time Of Last Held AirCargo Message",
						(NoResString)"Created Time Of Last Held AirCargo Message.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						DateTime.MinValue,
						false);
				});
			}
		}

		public DateTimeRegistryItem CreatedTimeOFLastHeldSeaCargoMessage
		{
			get
			{
				return GetItem("AUCreatedTimeOFLastHeldSeaCargoMessage", delegate
				{
					return new DateTimeRegistryItem(
						"AUCreatedTimeOFLastHeldSeaCargoMessage",
						SeaCargoCategory,
						(NoResString)"Created Time Of Last Held SeaCargo Message",
						(NoResString)"Created Time Of Last Held SeaCargo Message.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						DateTime.MinValue,
						false);
				});
			}
		}

		public BooleanRegistryItem ActivateAlternatePartShipmentModel
		{
			get
			{
				return GetItem("ActivateAlternatePartShipmentModel", delegate
				{
					return new BooleanRegistryItem(
						"ActivateAlternatePartShipmentModel",
						AirCargoCategory,
						(NoResString)"Activate Alternate Part Shipment Model",
						(NoResString)"The Alternate Part-shipment Model involves sending a ‘Consignment Reference’ number with Air Cargo Reports. This number is used to match AIRCRs with FIDs and SACs. Users must be authorised by Australian Customs to use this feature, and is primarily intended for use by the major Air Couriers. If this feature is activated, but authorisation has not been obtained from Australian Customs, then all Air Cargo reports utilising this feature will be rejected by Customs.",
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem AttachOrphanedCARSTsWhenCusHAWBCreated
		{
			get
			{
				return GetItem("AttachOrphanedCARSTsWhenCusHAWBCreated", delegate
				{
					return new BooleanRegistryItem(
						"AttachOrphanedCARSTsWhenCusHAWBCreated",
						AirCargoCategory,
						(NoResString)"Attach Orphaned CARSTs When HAWB Created",
						(NoResString)"When a new HAWB is added to a non-HVLV Air Cargo job, any unattached CARST messages (related to the house bill) will be attached to the new HAWB job.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		public BooleanRegistryItem SendAirCargoMessagesInBackGroundDefault
		{
			get
			{
				return GetItem("SendAirCargoMessagesInBackGroundDefault", delegate
				{
					return new BooleanRegistryItem(
						"SendAirCargoMessagesInBackGroundDefault",
						AirCargoCategory,
						(NoResString)"Send Air Cargo messages in background default",
						(NoResString)"If set to YES then an option will be available to generate and delay sending of Air Cargo messages, in the background, and until out-of-hours.",
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem DefaultAirConsolResponsibleParty
		{
			get
			{
				return GetItem("DefaultAirConsolResponsibleParty", delegate
				{
					return new BooleanRegistryItem(
						"DefaultAirConsolResponsibleParty",
						AirCargoCategory,
						(NoResString)"Default Air Consol Responsible Party?",
						(NoResString)"The Responsible Party on the Consol Air Cargo tab normally defaults to the organisation set as the Proxy Organisation on the current company. If you override this setting and set it to NO then the Responsible Party on the Consol Air Cargo tab will not be defaulted and must set it manually before sending Air Cargo messages.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public CodePairRegistryItem DefaultAirCargoConsignee
		{
			get
			{
				return GetItem("DefaultAirCargoConsignee", delegate
				{
					return new CodePairRegistryItem(
						"DefaultAirCargoConsignee",
						AirCargoCategory,
						(NoResString)"Default Air Consol Consignee",
						(NoResString)"This determines what party will be used to populate the Consignee Organisation in an Air Cargo Report that is linked to a Consolidation.",
						OLookUpEditType.DefaultCargoReportConsigneeOption,
						RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						Core.Constants.AUCustoms.DefaultConsigneeOption.DeliverTo
						);
				});
			}
		}

		public BooleanRegistryItem ManifestSACOverride
		{
			get
			{
				return GetItem("ManifestSACOverride", delegate
				{
					return new BooleanRegistryItem(
						"ManifestSACOverride",
						AirCargoCategory,
						(NoResString)"Ignore Thesaurus description when creating HVLV cargo reports",
						(NoResString)"If set to YES and the manifest value is under the threshold, the Thesaurus word matching will be ignored and the SAC indicator will be set.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForController,
						false);
				});
			}
		}

		public IntRegistryItem MaximumAirOutturnLinesPerMessage
		{
			get
			{
				return GetItem("MaximumAirOutturnLinesPerMessage", delegate
				{
					return new IntRegistryItem(
						"MaximumAirOutturnLinesPerMessage",
						AirCargoCategory,
						(NoResString)"Maximum Number of Air Outturn Lines per message",
						(NoResString)"The maximum number of air outturn lines per message, accepted by Customs.",
						RegistryStorageFlags.System,
						999);
				});
			}
		}

		public CodePairRegistryItem AirCargoOutturnScanningCONDCLEARReleaseStatus
		{
			get
			{
				return GetItem("AirCargoOutturnScanningCONDCLEARReleaseStatus", delegate
				{
					return new CodePairRegistryItem(
						"AirCargoOutturnScanningCONDCLEARReleaseStatus",
						AirCargoCategory,
						ResString.GetMultilingualString("0246A347-A414-46B4-8132-885E15183423", "Outturn Scanning CONDCLEAR Release Status"),
						ResString.GetMultilingualString("CEB9A335-3A7D-4D6F-8EA8-6EF4CDA2D1E8", "The effective release status of Outturn scanning."),
						OLookUpEditType.CondClearReleaseStatus,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						Core.Constants.AUCustoms.CondClearReleaseStatus.Held
						);
				});
			}
		}

		public IntRegistryItem MaximumSeaOutturnLinesPerMessage
		{
			get
			{
				return GetItem("MaximumSeaOutturnLinesPerMessage", delegate
				{
					return new IntRegistryItem(
						"MaximumSeaOutturnLinesPerMessage",
						SeaCargoCategory,
						(NoResString)"Maximum Number of Sea Outturn Lines per message",
						(NoResString)"The maximum number of sea outturn lines per message, accepted by Customs.",
						RegistryStorageFlags.System,
						999);
				});
			}
		}

		#region Outturn Scanning

		public StringRegistryItem SaveToOutturnScanFileFolder
		{
			get
			{
				return GetItem("SaveToOutturnScanFileFolder", delegate
				{
					return new StringRegistryItem("SaveToOutturnScanFileFolder", AirCargoCategory, (NoResString)"Save To Outturn Scan File Folder", null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, string.Empty);
				});
			}
		}

		public StringRegistryItem LoadFromOutturnScanFilePath
		{
			get
			{
				return GetItem("LoadFromOutturnScanFilePath", delegate
				{
					return new StringRegistryItem("LoadFromOutturnScanFilePath", AirCargoCategory, (NoResString)"Load From Outturn Scan File Path", null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, string.Empty);
				});
			}
		}

		#endregion

		#endregion

		#region Sea Cargo

		internal static MultilingualString SeaCargoCategory { get { return RawDataRegistry.Categories.Customs_Australia_SeaCargo; } }

		public BooleanRegistryItem AttachOrphanedCARSTsWhenCusSCAPivotCreated
		{
			get
			{
				return GetItem("AttachOrphanedCARSTsWhenCusSCAHouseCreated", delegate
				{
					return new BooleanRegistryItem(
						"AttachOrphanedCARSTsWhenCusSCAHouseCreated",
						SeaCargoCategory,
						(NoResString)"Attach Orphaned CARSTs When Package Created",
						(NoResString)"When a new Package is added to an Sea Cargo job, any unattached CARST messages (related to the package) will be attached to the new Package.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		public CodePairRegistryItem DefaultSeaCargoConsignee
		{
			get
			{
				return GetItem("DefaultSeaCargoConsignee", delegate
				{
					return new CodePairRegistryItem(
						"DefaultSeaCargoConsignee",
						SeaCargoCategory,
						(NoResString)"Default Sea Consol Consignee",
						(NoResString)"This determines what party will be used to populate the Consignee Organisation in a Sea Cargo Report that is linked to a Consolidation.",
						OLookUpEditType.DefaultCargoReportConsigneeOption,
						RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						Core.Constants.AUCustoms.DefaultConsigneeOption.DeliverTo
						);
				});
			}
		}

		public CodePairRegistryItem SeaCargoOutturnScanningCONDCLEARReleaseStatus
		{
			get
			{
				return GetItem("SeaCargoOutturnScanningCONDCLEARReleaseStatus", delegate
				{
					return new CodePairRegistryItem(
						"SeaCargoOutturnScanningCONDCLEARReleaseStatus",
						SeaCargoCategory,
						ResString.GetMultilingualString("633FF586-AE36-49ED-981D-2C2A4853963B", "Outturn Scanning CONDCLEAR Release Status"),
						ResString.GetMultilingualString("6E5426DF-837C-4386-BD40-9FF55C6E1CEF", "The effective release status of Outturn scanning."),
						OLookUpEditType.CondClearReleaseStatus,
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						Core.Constants.AUCustoms.CondClearReleaseStatus.Clear
						);
				});
			}
		}

		public BooleanRegistryItem UseSenderReferenceToFilterSeaCargoReport
		{
			get
			{
				return GetItem("UseSenderReferenceToFilterSeaCargoReport", delegate
				{
					return new BooleanRegistryItem(
						"UseSenderReferenceToFilterSeaCargoReport",
						SeaCargoCategory,
						(NoResString)"Use Sender Reference To Filter Sea Cargo Report",
						(NoResString)"When import a CMR message, use the sender reference of message to find an expected Sea Cargo Report data.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		#endregion

		#region AQIS Declaration

		internal static MultilingualString AQISDeclarationSubCategory { get { return RegistryConstants.GetCategory(RawDataRegistry.Categories.Customs_Australia, (NoResString)"Quarantine Declaration"); } }

		public GuidRegistryItem DefaultBranchForTransferIn
		{
			get
			{
				return GetItem("DefaultBranchForTransferIn", delegate
				{
					return new GuidRegistryItem(
					"DefaultBranchForTransferIn",
					AQISDeclarationSubCategory,
					(NoResString)"Default Branch For Transfer In",
					(NoResString)"The branch under which a Quarantine job will be created if the RFP or Client Reference number of an RFP Transfer In cannot be found on any existing Quarantine job.",
					new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbBranch),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					Guid.Empty);
				});
			}
		}

		public StringEffectiveDateRegistryItem EXDOCTestEmailAddress
		{
			get
			{
				return GetItem("EXDOCTestEmailAddress_1511", delegate
				{
					StringEffectiveDate stringEffectiveDateDefault = new StringEffectiveDate();
					stringEffectiveDateDefault.PreviousValue = "edi.test@daff.gov.au";
					stringEffectiveDateDefault.NewValue = "edi.test@agriculture.gov.au";
					stringEffectiveDateDefault.EffectiveDate = new ZDateTime(2015, 11, 17);
					return new StringEffectiveDateRegistryItem(
						"EXDOCTestEmailAddress_1511",
						AQISDeclarationSubCategory,
						(NoResString)"Email address for test EXDOC messages",
						(NoResString)"The Email address to which EXDOC TEST messages will be sent.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						stringEffectiveDateDefault);
				});
			}
		}

		internal static ZString GetEXDOCTestEmailAddress()
		{
			return AUCustomsDataRegistry.Instance.EXDOCTestEmailAddress.Value.EffectiveValueForToday;
		}

		public StringEffectiveDateRegistryItem EXDOCProdEmailAddress
		{
			get
			{
				return GetItem("EXDOCProdEmailAddress_1512", delegate
				{
					StringEffectiveDate stringEffectiveDateDefault = new StringEffectiveDate();
					stringEffectiveDateDefault.PreviousValue = "edi.prod@agriculture.gov.au";
					stringEffectiveDateDefault.NewValue = "EXDOC@mail.p3.awe.gov.au";
					stringEffectiveDateDefault.EffectiveDate = new ZDateTime(2022, 07, 10);
					return new StringEffectiveDateRegistryItem(
						"EXDOCProdEmailAddress_1512",
						AQISDeclarationSubCategory,
						(NoResString)"Email address for live production EXDOC messages",
						(NoResString)"The Email address to which EXDOC live production messages will be sent.",
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						stringEffectiveDateDefault);
				});
			}
		}

		internal static ZString GetEXDOCProdEmailAddress()
		{
			return AUCustomsDataRegistry.Instance.EXDOCProdEmailAddress.Value.EffectiveValueForToday;
		}

		public DateTimeRegistryItem Errata44EffectiveDate
		{
			get
			{
				return GetItem("Errata44EffectiveDate_1703", delegate
				{
					return new DateTimeRegistryItem(
						"Errata44EffectiveDate_1703",
						AQISDeclarationSubCategory,
						(NoResString)Errata44EffectiveDateCaption,
						(NoResString)Errata44EffectiveDateHint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						new DateTime(2017, 03, 31));
				});
			}
		}
		internal const string Errata44EffectiveDateCaption = "ERRATA 44 Effective Date";
		internal const string Errata44EffectiveDateHint = "The date when EXDOCS ERRATA 44 is effective.";

		internal ZBool IsErrata44Effective
		{
			get { return ZDateTime.Today >= AUCustomsDataRegistry.Instance.Errata44EffectiveDate.Value; }
		}

		internal static MultilingualString COLSSubCategory { get { return RawDataRegistry.Categories.Customs_Australia_COLS; } }

		#endregion

		#region CMR

		internal static MultilingualString CMRCategory { get { return RawDataRegistry.Categories.Customs_Australia_CMR; } }

		public IntRegistryItem MaximumInterchangeSize
		{
			get
			{
				return GetItem("MaximumInterchangeSize", delegate
				{
					return new IntRegistryItem(
						"MaximumInterchangeSize",
						CMRCategory,
						ResString.GetMultilingualString("89CDD938-2B56-4F91-B6F0-7D31B26CB830", "Maximum Interchange Size (in bytes)"),
						ResString.GetMultilingualString("AF9E6644-3704-46C8-AF6B-DE76DB6F0333", "The maximum size (in bytes) that can be transmitted as a single interchange. If this size is exceeded, the unprocessed messages will be processed in the next batch."),
						RegistryStorageFlags.Company,
						10000000);
				});
			}
		}

		public BooleanRegistryItem IgnoreUnknownResponses
		{
			get
			{
				return GetItem("IgnoreUnknownResponses", delegate
				{
					return new BooleanRegistryItem(
						"IgnoreUnknownResponses",
						CMRCategory,
						(NoResString)"Ignore Unknown/Unassociated inbound CMR messages?",
						(NoResString)"If a messages is received from CMR that cannot be linked with any existing job then this is normally notified via email to the notify errors group for that type of message. If this registry item is changed from the default NO to YES then these messages will be ignored and no notification sent.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public StringRegistryItem AUReferenceFileAlternateDownloadURL
		{
			get
			{
				return GetItem("AUReferenceFileAlternateDownloadURL", delegate
				{
					var result = new StringRegistryItem("AUReferenceFileAlternateDownloadURL",
						RawDataRegistry.Categories.Customs_Australia_ReferenceFiles,
						ResString.GetMultilingualString("3C110A34-C50E-43F9-8124-F686504CCEC4", "Alternate Download URL"),
						ResString.GetMultilingualString("914C19D2-3971-46FE-9CF1-F9F9BC3AC8E0",
							"URL to the WiseTech Global CMR Reference File mirror for Australian Customs."),
						RegistryStorageFlags.System,
						"https://myaccount-portal.cargowise.com/my-account/public/customs/");
					return result;
				});
			}
		}

		public BooleanRegistryItem UseRefDatabaseData
		{
			get
			{
				return GetItem("AUUseRefDatabaseData", delegate
				{
					return new BooleanRegistryItem(
						"AUUseRefDatabaseData",
						RawDataRegistry.Categories.Customs_Australia,
						(NoResString)"Use RefDatabase Data",
						(NoResString)"Setting this to True will force CW1 to use the reference data in CW-RefDatabase instead of the various tables in RefDb_CMR_AU. The list of what has and hasn't been added can be found in WI00569357.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		#region Debug
		internal static MultilingualString DebugCategory { get { return RegistryConstants.GetCategory(RawDataRegistry.Categories.Customs_Australia, (NoResString)"Debug"); } }
		public BooleanRegistryItem EnableDebugHooksForAU
		{
			get
			{
				return GetItem("EnableDebugHooksForAU", delegate
				{
					return new BooleanRegistryItem(
						"EnableDebugHooksForAU",
						DebugCategory,
						(NoResString)"Enable Debug Hooks?",
						(NoResString)"Debugging features and logging will be turned on.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}
		#endregion

		#region NEXDOCS

		GlbCompanyCollection Companies => companies ?? (companies = new GlbCompanyCollection(Factory));
		GlbCompanyCollection companies;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory { NameForDebugging = "AUCustomsRegistry" });
		BusinessObjectFactory factory;

		const string ConfigurationName = "NEXDOCSSystemLevel";
		const string CurrentPwd = "Current";
		const string CompanyType = "Company";
		const string NGTType = "NGT";

		internal static MultilingualString NEXDOCSCategory { get { return RawDataRegistry.Categories.Customs_Australia_NEXDOCS; } }

		public NGTRegistryItem NEXDOCSGroupToken
		{
			get
			{
				return GetItem("NEXDOCSGroupToken", delegate
				{
					return new NGTRegistryItem(
						"NEXDOCSGroupToken",
						NEXDOCSCategory,
						ResString.GetMultilingualString("21E192F1-3B05-4DF2-B5DE-209583F9B80A", "NEXDOCS Group Token"),
						ResString.GetMultilingualString("90EB2086-B2C3-44D1-9435-2F61F2E64D12", "This password will be used as a company credential for NEXDOCS.")
					)
					{
						OnUpdateAction = OnUpdateAction,
						OnAllValuesSavedAction = OnAllValuesSavedAction
					};
				});
			}
		}

		public void OnUpdateAction(Guid companyPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			var pwd = (NGT)newValue;
			var company = (GlbCompany)Companies.FindByPK(companyPK);
			var credentialSender = new CredentialSender(ConfigurationName);

			var credential = CredentialSender.CreateCredential(CurrentPwd, ZString.Empty, pwd.Password);
			var groupCompany = CredentialSender.CreateGroup(CompanyType, company.GC_Code, ZString.Empty);
			var groupNGT = CredentialSender.CreateGroup(NGTType, ZString.Empty, Enterprise.MasterFiles.Business.Customs.XmlCredential.Constants.CredentialStatusList.Valid);
			groupNGT.Items = new object[] { credential };
			groupCompany.Items = new object[] { groupNGT };
			credentialSender.AddItems(groupCompany);

			credentialSender.SendCredential(Factory);
		}

		public void OnAllValuesSavedAction()
		{
			Factory.Save();
			Factory.CleanUp();
			factory = null;
		}

		public BooleanRegistryItem NEXDOCSTestingSystem
		{
			get
			{
				return GetItem("IsNEXDOCSTesting", delegate
				{
					return new BooleanRegistryItem(
						"IsNEXDOCSTesting",
						NEXDOCSCategory,
						(NoResString)"Is NEXDOCS Testing System?",
						(NoResString)"NEXDOCS messages be sent to the test rather than production system?",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		internal ZBool IsNEXDOCSTestingSystem
		{
			get { return AUCustomsDataRegistry.Instance.NEXDOCSTestingSystem.Value; }
		}

		#endregion

		#region Testing

		public BooleanRegistryItem DisableDSAMessages
		{
			get
			{
				return GetItem("DisableDSAMessages",
					() => new BooleanRegistryItem(
						"DisableDSAMessages",
						RawDataRegistry.Categories.Customs_Australia_Testing,
						(NoResString)"Disable DSA messages",
						(NoResString)"All EDIMessages where the EM_ApplicationCode = CMR, EM_MessageType = DSA and EM_ReceiveTransmit = RCV will have their EM_Status set to DCD instead of QUE",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false));
			}
		}

		public BooleanRegistryItem EnableNewPackTypeConversions
		{
			get
			{
				return GetItem("EnableNewPackTypeConversions",
					() => new BooleanRegistryItem(
						"EnableNewPackTypeConversions",
						RawDataRegistry.Categories.Customs_Australia_Testing,
						(NoResString)"Enable New Pack Type Conversions",
						(NoResString)"This will enable the functionality that adds new pack type conversions being created in WI00672052.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false));
			}
		}

		public BooleanRegistryItem UseCMRTariffTestData
		{
			get
			{
				return GetItem("UseCMRTariffTestData",
					() => new BooleanRegistryItem(
						"UseCMRTariffTestData",
						RawDataRegistry.Categories.Customs_Australia_Testing,
						(NoResString)"Use CMR Tariff Test Data",
						(NoResString)"Enabling this option will force the system to use the CMR Industry_Test data files for tariffs instead of production.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers,
						false));
			}
		}

		public BooleanRegistryItem AlwaysUseIndustryTestCMRFiles
		{
			get
			{
				return GetItem("AlwaysUseIndustryTestCMRFiles", delegate
				{
					var result = new BooleanRegistryItem(
						"AlwaysUseIndustryTestCMRFiles",
						RawDataRegistry.Categories.Customs_Australia_Testing,
						(NoResString)"Always Use Industry Test CMR Files",
						(NoResString)"This will force the system to ignore the SharedRefDb restrictions and always use the Industry_Test.Q1-MAIN.tar.gz CMR file.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);

					(result.DataType as BooleanRegistryDataType).Validating += OnAlwaysUseIndustryTestCMRFilesValidating;

					return result;
				});
			}
		}

		void OnAlwaysUseIndustryTestCMRFilesValidating(object sender, RegistryDataTypeValidatingEventArgs<bool> e)
		{
			if (e.ProposedValue && EnvProxy.Instance.IsProductionSystem)
			{
				throw new RegistryValidationException(Res.GetString("4CEE7EB2-3546-4913-BE07-B7039E3396F3", "Industry Test CMR Files cannot be used in a production system."));
			}
		}

		public BooleanRegistryItem EnableNEXDOCForInedibleMeat
		{
			get
			{
				return GetItem("EnableNEXDOCForInedibleMeat",
					() => new BooleanRegistryItem(
						"EnableNEXDOCForInedibleMeat",
						RawDataRegistry.Categories.Customs_Australia_Testing,
						(NoResString)"Enable NEXDOC for Inedible Meat",
						(NoResString)"This enables NEXDOC functionality for the Inedible Meat produce type while it is under development. Refer to WI00884518 - [Lead Job] NEXDOC Inedible Meat for more info.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false));
			}
		}

		#endregion

		#region Contingency Data Email Addresses

		public CodeDescriptionPairList CMRContingencyDataEmailAddresses
		{
			get
			{
				ContingencyDataEmailAddressCollection collection = CMRContingencyDataEmailAddressesRaw.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				return collection.GetCodeDescriptionPairList();
			}
			set
			{
				ContingencyDataEmailAddressCollection collection = new ContingencyDataEmailAddressCollection(value);
				CMRContingencyDataEmailAddressesRaw.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
			}
		}

		public BooleanRegistryItem CMRIsBureau
		{
			get
			{
				return GetItem("CMRIsBureau", delegate
				{
					return new BooleanRegistryItem(
						"CMRIsBureau",
						Categories.Customs_Australia_CMR,
						(NoResString)"Company is providing only Bureau Messaging Services",
						(NoResString)"Company is providing only Bureau messaging services if true.",
						RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, false);
				});
			}
		}

		internal ContingencyDataEmailAddressesRegistryItem CMRContingencyDataEmailAddressesRaw
		{
			get
			{
				return GetItem("CMRContingencyDataEmailAddresses", delegate
				{
					CodeDescriptionPairList defaultValue = new CodeDescriptionPairList();

					defaultValue.AddPair("Primary BCP Mail Address", "ICSBCP@customs.gov.au");

					return new ContingencyDataEmailAddressesRegistryItem(
						"CMRContingencyDataEmailAddresses",
						Categories.Customs_Australia_CMR,
						(NoResString)"Contingency Data Email Addresses",
						(NoResString)"List of customs email addresses for CMR Contingency data.",
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						new ContingencyDataEmailAddressCollection(defaultValue));
				});
			}
		}

		#endregion

		#region CMR Reference Files Update Notification Group

		public GuidRegistryItem CMRReferenceFilesUpdateNotificationGroup
		{
			get
			{
				return GetItem("CMRReferenceFilesUpdateNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CMRReferenceFilesUpdateNotificationGroup",
						Categories.Customs_Australia,
						(NoResString)"CMR Reference Files Update Notification Group",
						(NoResString)"The staff group that will be receiving notification about CMR Reference Files updates.",
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						Guid.Empty);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Australia;
					return result;
				});
			}
		}

		#endregion

		#region Sole Trader

		public BooleanRegistryItem SoleTrader
		{
			get
			{
				return GetItem("SoleTrader", delegate
				{
					return new BooleanRegistryItem(
						"SoleTrader",
						Categories.Customs_Australia,
						(NoResString)"Sole Trader",
						(NoResString)"Company listed is nominated as a sole trader for customs purposes.",
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		#endregion

		#region Max Number of Entry Lines

		public IntRegistryItem MaxNumberOfEntryLinesAcceptedAtCustoms
		{
			get
			{
				return GetItem("MaxNumberOfEntryLinesAcceptedAtCustoms", delegate
				{
					return new IntRegistryItem(
						"MaxNumberOfEntryLinesAcceptedAtCustoms",
						Categories.Customs_Australia,
						(NoResString)"Maximum Number of Entry Lines Accepted at Customs",
						(NoResString)"The maximum number of entry lines accepted at Customs",
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						1000);
				});
			}
		}

		#endregion

		#region Default Destination/Discharge Premise IDs

		public DefaultPremiseIDCollection DefaultDischargePremiseIDs
		{
			get
			{
				return DefaultDischargePremiseIDsRaw.Value;
			}
#if DEBUG
			set
			{
				DefaultDischargePremiseIDsRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
#endif
		}

		internal DefaultPremiseIDsRegistryItem DefaultDischargePremiseIDsRaw
		{
			get
			{
				return GetItem("DefaultDischargePremiseIDs", delegate
				{
					return new DefaultPremiseIDsRegistryItem(
						"DefaultDischargePremiseIDs",
						Categories.Customs_Australia_AirCargo_UnderbondMovementRequest,
						(NoResString)"Default Discharge Premise IDs",
						(NoResString)"List of customs default discharge premise IDs.",
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public DefaultDestinationPremiseIDCollection DefaultDestinationPremiseIDs
		{
			get
			{
				return DefaultDestinationPremiseIDsRaw.Value;
			}
#if DEBUG
			set
			{
				DefaultDestinationPremiseIDsRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
#endif
		}

		internal DefaultDestinationPremiseIDRegistryItem DefaultDestinationPremiseIDsRaw
		{
			get
			{
				return GetItem("DefaultDestinationPremiseIDs", delegate
				{
					return new DefaultDestinationPremiseIDRegistryItem(
						"DefaultDestinationPremiseIDs",
						Categories.Customs_Australia_AirCargo_UnderbondMovementRequest,
						(NoResString)"Default Destination Premise IDs",
						(NoResString)"List of customs default destination premise IDs.",
						RegistryStorageFlags.System);
				});
			}
		}

		public BooleanRegistryItem AllowMultipleDCLUnderbondRequest
		{
			get
			{
				return GetItem("AllowMultipleDCLUnderbondRequest", delegate
				{
					return new BooleanRegistryItem(
						"AllowMultipleDCLUnderbondRequest",
						Categories.Customs_Australia_AirCargo_UnderbondMovementRequest,
						(NoResString)"Allow multiple DCL Underbond requests.",
						(NoResString)"Enable/Disable multiple DCL Underbond requests.",
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public StringRegistryItem BranchForAutomaticUnderbonds
		{
			get
			{
				return GetItem("BranchForAutomaticUnderbonds", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"BranchForAutomaticUnderbonds",
						Categories.Customs_Australia_AirCargo_UnderbondMovementRequest,
						(NoResString)"Branch For Automatic Underbonds",
						(NoResString)"Enter the 3 character branch of the Australian Branch that should be used to create and send all automatically generated Underbonds.",
						RegistryStorageFlags.System
						);
					return result;
				});
			}
		}

		#endregion

		#region Include Quarantine Service Payments In Total Payable On Entry Print

		public BooleanRegistryItem IncludeAQISServicePaymentsInTotalPayableOnEntryPrint
		{
			get
			{
				return GetItem("IncludeAQISServicePaymentsInTotalPayableOnEntryPrint", delegate
				{
					return new BooleanRegistryItem(
						"IncludeAQISServicePaymentsInTotalPayableOnEntryPrint",
						Categories.Customs_Australia,
						(NoResString)"Include Quarantine Service Payments In Total Payable On Entry Print",
						(NoResString)"Includes any Quarantine Service Payments in Total Payable on the Entry Print, otherwise list them separately.",
						RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, true);
				});
			}
		}

		#endregion

		#region Quarantine Declaration

		public CodePairRegistryItem SendAQISAcknowledgements
		{
			get
			{
				return GetItem("SendAQISAcknowledgements", delegate
				{
					return new CodePairRegistryItem(
					"SendAQISAcknowledgements",
					Categories.Customs_Australia_AQISDeclaration,
					(NoResString)"Send Quarantine Declaration Acknowledgements",
					(NoResString)"Send message acknowledgements to user",
					OLookUpEditType.EmailTo,
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.PreserveTestValue,
					Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem SendAQISAcknowledgementsToGroup
		{
			get
			{
				return GetItem("SendAQISAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
					"SendAQISAcknowledgementsToGroup",
					Categories.Customs_Australia_AQISDeclaration,
					(NoResString)"Group To Send Quarantine Declaration Acknowledgements To",
					(NoResString)"Send message acknowledgements to group",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem SendAQISErrors
		{
			get
			{
				return GetItem("SendAQISErrors", delegate
				{
					return new CodePairRegistryItem(
						"SendAQISErrors",
						Categories.Customs_Australia_AQISDeclaration,
						(NoResString)"Send Quarantine Declaration Errors",
						(NoResString)"Send message errors to user",
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem SendAQISErrorsToGroup
		{
			get
			{
				return GetItem("SendAQISErrorsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
					"SendAQISErrorsToGroup",
					Categories.Customs_Australia_AQISDeclaration,
					(NoResString)"Group To Send Quarantine Declaration Errors To",
					(NoResString)"Send message errors to group",
					RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem SendAQISImpediments
		{
			get
			{
				return GetItem("SendAQISImpediments", delegate
				{
					return new CodePairRegistryItem(
					"SendAQISImpediments",
					Categories.Customs_Australia_AQISDeclaration,
					(NoResString)"Send Quarantine Declaration Impediments",
					(NoResString)"Send message impediments to user",
					OLookUpEditType.EmailTo,
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.PreserveTestValue,
					Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem SendAQISImpedimentsToGroup
		{
			get
			{
				return GetItem("SendAQISImpedimentsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
					"SendAQISImpedimentsToGroup",
					Categories.Customs_Australia_AQISDeclaration,
					(NoResString)"Group To Send Quarantine Declaration Impediments To",
					(NoResString)"Send message impediments to group",
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableAQISDeclarationMessaging
		{
			get
			{
				return GetItem("EnableAQISDeclarationMessaging", delegate
				{
					return new BooleanRegistryItem(
					"EnableAQISDeclarationMessaging",
					Categories.Customs_Australia_AQISDeclaration,
					(NoResString)"Enable Quarantine Declaration Messaging",
					(NoResString)"WARNING - Activating this will result in a charge per transaction, please contact your WiseTech Global Sales Representative for details.",
					RegistryStorageFlags.Company,
					RegistryOptions.PreserveTestValue,
					false);
				});
			}
		}

		#endregion

		#region AllowOverrideOfCustomsUnitsOnDeclarations

		public BooleanRegistryItem AllowOverrideOfCustomsUnitsOnDeclarations
		{
			get
			{
				return GetItem("AllowOverrideOfCustomsUnitsOnDeclarations", delegate
				{
					return new BooleanRegistryItem(
						"AllowOverrideOfCustomsUnitsOnDeclarations",
						Categories.Customs_Australia,
						(NoResString)"Allow Override of Customs Units on Declarations",
						(NoResString)"Allow Override of Customs Units on Declarations if true.",
						RegistryStorageFlags.Company, false);
				});
			}
		}

		#endregion

		#region Sea Mandatory Latest Cargo Reporting Timeframe
		public IntRegistryItem SeaMandatoryLatestCargoReportingTimeframe
		{
			get
			{
				return GetItem("SeaMandatoryLatestCargoReportingTimeframe", delegate
				{
					return new IntRegistryItem(
						"SeaMandatoryLatestCargoReportingTimeframe",
						Categories.Customs_Australia_SeaCargo,
						(NoResString)"Sea Mandatory Latest Cargo Reporting Timeframe (hours)",
						(NoResString)"The Sea Mandatory Latest Cargo Reporting Timeframe is the timeframe, specified in hours by Australian Customs, which defines the latest time within which a Cargo Report must be lodged for Sea Freight shipments.",
						RegistryStorageFlags.Company,
						48);
				});
			}
		}
		#endregion

		#region Air Mandatory Latest Cargo Reporting Timeframe
		public IntRegistryItem AirMandatoryLatestCargoReportingTimeframe
		{
			get
			{
				return GetItem("AirMandatoryLatestCargoReportingTimeframe", delegate
				{
					return new IntRegistryItem(
						"AirMandatoryLatestCargoReportingTimeframe",
						Categories.Customs_Australia_AirCargo,
						(NoResString)"Air Mandatory Latest Cargo Reporting Timeframe (hours)",
						(NoResString)"The Air Mandatory Latest Cargo Reporting Timeframe is the timeframe, specified in hours by Australian Customs, which defines the latest time within which a Cargo Report must be lodged for Air Freight shipments.",
						RegistryStorageFlags.Company,
						4);
				});
			}
		}
		#endregion

		#region Additional EDI Message Sender
		public StringRegistryItem AdditionalEDIMessageSender
		{
			get
			{
				return GetItem("AdditionalEDIMessageSender", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"AdditionalEDIMessageSender",
						Categories.Customs_Australia,
						(NoResString)"Additional EDI Message Sender",
						(NoResString)"Any inbound messages with a from address containing this field will also be selected for message processing",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers);
					return result;
				});
			}
		}
		#endregion

		#region Print Entry when printing Invoice

		public BooleanRegistryItem PrintEntryWhenPrintingInvoice
		{
			get
			{
				return GetItem("PrintEntryWhenPrintingInvoice", delegate
				{
					return new BooleanRegistryItem(
						"PrintEntryWhenPrintingInvoice",
						Categories.Customs_Australia,
						ResString.GetMultilingualString("c9b83d8e-0de1-4bdd-a1aa-4c3bcabc3e08", "Print Entry when Printing Invoice"),
						ResString.GetMultilingualString("ee63de1c-9b97-4db9-9558-5d9b3d057ee2", "Print Entry Print when printing Invoices with Disbursement Charges."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		IRegistryItem IAUCustomsRegistry.PrintEntryWhenPrintingInvoice => PrintEntryWhenPrintingInvoice;

		#endregion

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.Australia;
		}
	}
}
