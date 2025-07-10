using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Wizards.CFSP;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Universal.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Registry.Business.CustomsReferenceNumberType;
using GBCodeList = Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.Declaration
{
	[SystemDefinedValues]
	public partial class JobDeclaration : AutoGBJobDeclaration
		, Integration.Customs.GB.IJobDeclaration
		, ICusAddInfoTypeSupporter
		, ICusEntryNumberValidationDeciderOfType
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override IReadOnlyList<string> MultipleKeysToUseCore
		{
			get
			{
				if (JE_ApplicationCode == Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
				{
					if (IsExport)
					{
						return new[] { MultipleKeyCdsExport };
					}
					else if (IsImport)
					{
						return new[] { MultipleKeyCdsImport };
					}
					else
					{
						return new[] { MultipleKeyCdsMisc };
					}
				}
				return new[] { MultipleKeyChief };
			}
		}

		public const string MultipleKeyCdsImport = "597D187B-6807-47AA-AA96-3AAF7733BDBA";
		public const string MultipleKeyCdsExport = "81CE1996-F292-474B-8E98-0310C7AC3FF1";
		public const string MultipleKeyCdsMisc = "3ABCCCB8-C5DC-4CFB-A535-4A674CCF74BE";
		public const string MultipleKeyChief = "6101A4FE-9EC0-41C1-B06E-3721D5FB5A39";

		protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
		{
			return new JobDeclarationDeepCloneStrategy(this, cloneType, alternateFactory);
		}

		protected override ZString GetEntrySubStyleForCommonTransit(RefCountry country)
		{
			var entryStyle = ZString.Empty;

			if (country.IsEFTA())
			{
				entryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
			}
			else if (IsImport)
			{
				entryStyle = EntryStyleListImport.Codes.ImportNormal;
			}
			else if (IsExport)
			{
				entryStyle = EntryStyleListExport.Codes.ExportNormal;
			}

			return entryStyle;
		}

		protected override ZBool SupportValidateCustomsMessagingCore => true;

		public override ZBool AreMultipleEntryInstructionsAllowed => true;

		public override ZBool ZG_GatewayVisible => true;

		public override ZDateTime DateOfValuation => !EarliestCustomsEntryIssueDate.IsEmpty ? EarliestCustomsEntryIssueDate :
			(JE_EntryAuthorisationDate.IsEmpty ? CachedTodaysDate : JE_EntryAuthorisationDate);

		internal bool ShouldMarkAsNeedingValidationOnInvoiceLinesWhenChangingValueBuildUp => ShouldMarkAsNeedingValidationOnInvoiceLinesWhenChangingValueBuildUpCore;
		protected virtual bool ShouldMarkAsNeedingValidationOnInvoiceLinesWhenChangingValueBuildUpCore => true;

		protected override bool IsIntegrationWithAccountingSupported => true;

		[ResourceStringData("Enterprise.Customs.GB.Business.Declaration.JobDeclaration|JE_CustomsProfile", Caption = "Badge", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("4EB98FF7-FAA0-4413-B5A5-BC7993FF45DE", Caption = "Profile", FullDescription = "Select a profile to determine the appropriate service or badge for this declaration.")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ProfileList))]
		[MaxLength(Schema.JE_CustomsProfileMaxLength)]
		public override ZString JE_CustomsProfile
		{
			get => base.JE_CustomsProfile;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(JobDeclaration.Schema.JE_CustomsProfile))
				{
					var oldValue = JE_CustomsProfile;
					base.JE_CustomsProfile = value;
					if (oldValue != value)
					{
						var declaration = this;
						var badgeCodeSetting = GBCustomsDataRegistry.Instance.BadgeCodes.Value.FindByBadgeCode(declaration.JE_CustomsProfile, declaration.JE_MessageType);
						declaration.ZG_Gateway = badgeCodeSetting?.CSPCode ?? ZString.Empty;
						CalculateMasterUCR();

						foreach (JobComInvoiceLine line in declaration.InvoiceLines)
						{
							line.MarkAsNeedingValidation();
						}

						SetApplicationCodeFromBadge();
					}
				}
			}
		}

		[ResourceStringData("52043029-F9E6-4B51-81C5-7847F35DF88D", Caption = "Office of Presentation", FullDescription = "Customs Office", MultipleKey = MultipleKeyChief, IsApplicableMember = nameof(IsImport))]
		[ResourceStringData("E65FB264-067C-4524-ACF9-702BEA41CEF6", Caption = "[29] Office of Exit", FullDescription = "Customs Office", MultipleKey = MultipleKeyChief, IsApplicableMember = nameof(IsExport))]
		[ResourceStringData("1BC08FDE-61D6-4DE8-814E-5D54609F8A02", Caption = "[UCC 5/12] Customs Office of Exit", ShortCaption = "[UCC 5/12] Office of Exit", FullDescription = "This is the Customs Office of Exit responsible for dealing with this customs declaration.", MultipleKey = MultipleKeyCdsExport)]
		[ResourceStringData("18C726C2-7EA3-4CFD-939D-AEEE356621D8", Caption = "[UCC 5/26] Customs Office of Presentation", ShortCaption = "[UCC 5/26] Office of Pres.", FullDescription = "This is the Customs Office of Presentation responsible for dealing with this customs declaration.", MultipleKey = MultipleKeyCdsImport)]
		[ResourceStringData("07680CFC-4C95-4BE2-BCD3-24DA52B2484F", Caption = "Customs Office", FullDescription = "This is the Customs Office responsible for dealing with this customs declaration.", MultipleKey = MultipleKeyCdsMisc)]
		[MaxLength(Schema.JE_CustomsOfficeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOffices))]
		public override ZString JE_CustomsOffice { get => base.JE_CustomsOffice; set => base.JE_CustomsOffice = value; }

		[MaxLength(Schema.JE_PaymentMethodMaxLength)]
		public override ZString JE_PaymentMethod
		{
			get { return base.JE_PaymentMethod; }
			set
			{
				var hasChanged = base.JE_PaymentMethod != value;
				base.JE_PaymentMethod = value;
				if (hasChanged)
				{
					UCCHelper?.DoIfPaymentMethodAndDefermentAccountNumberBothAreSet(this, null);
				}
			}
		}

		public override ZDateTime JE_EntryAuthorisationDate
		{
			get { return base.JE_EntryAuthorisationDate; }
			set
			{
				var hasChanged = base.JE_EntryAuthorisationDate != value;
				if (hasChanged)
				{
					base.JE_EntryAuthorisationDate = value;
					Invoices.MarkAsNeedingValidation();
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		protected override bool UCRReadOnly
		{
			get { return !IsInDatabase || !DeclarationNumber.IsEmpty; }
		}

		public override ZString JE_DefermentAccountNumber
		{
			get => base.JE_DefermentAccountNumber;
			set
			{
				var oldValue = JE_DefermentAccountNumber;
				base.JE_DefermentAccountNumber = value;
				if (oldValue != JE_DefermentAccountNumber)
				{
					UCCHelper?.DoIfPaymentMethodAndDefermentAccountNumberBothAreSet(this, null);
				}
			}
		}

		[ResourceStringData("F3DA76B7-09DC-4C9D-86CF-A144E4C86A45", Caption = "[14] Rep. Type", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("9dcb28eb-8bd5-41c3-8de3-b00af8ab3134", Caption = "[UCC 3/21] Rep. Type")]
		public override ZString JE_DeclarantType
		{
			get => base.JE_DeclarantType;
			set
			{
				var oldValue = JE_DeclarantType;
				base.JE_DeclarantType = value;
				if (oldValue != JE_DeclarantType)
				{
					UCCHelper?.DoIfRepresentationTypeIsSelf(this);
				}
			}
		}

		public bool IsGoodsArrivedSubStyle => IsSubStyleGoodsArrived(JE_EntrySubStyle);

		public static bool IsSubStyleGoodsArrived(ZString subStyle)
		{
			return GoodsArrivedSubStyles.Contains(subStyle.ToUpper());
		}

		public static HashSet<string> GoodsArrivedSubStyles
		{
			get
			{
				return new HashSet<string>
				{
					GBCodeList.EntrySubStyleListExport.Codes.C21_GoodsArrived_IECR ,
					GBCodeList.EntrySubStyleListExport.Codes.FullDeclarationGoodsArrived_IEFD,
					GBCodeList.EntrySubStyleListExport.Codes.SDP_PSA_GoodsArrived_IESP,
					GBCodeList.EntrySubStyleListImport.Codes.TransitSfdGoodsArrived,
					EntrySubStyleCodeList.Codes.B
				};
			}
		}

		public bool IsGoodsNotArrivedSubStyle => CusEntryInstruction.IsGoodsNotArrivedSubStyle;

		protected override NotificationTypes ErrorTypeForCWPackQtyExceededErrorCore => NotificationTypes.Warning;

		public ZBool IsFSD
		{
			get { return IsSDI && InvoiceLines.Count == 1 && InvoiceLines[0].JI_Procedure == JobComInvoiceLine.CfspFsdCPCCode; }
		}

		public bool IsSDI
		{
			get { return IsISD && JE_EntrySubStyle == GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration; }
		}

		public bool IsICR { get { return IsImport && JE_DeclarationType == ImportSADDeclarationTypeList.Codes.ImportClearanceRequest; } }
		public bool IsIFD { get { return IsImport && JE_DeclarationType == ImportSADDeclarationTypeList.Codes.ImportFullDeclaration; } }
		public bool IsIFW { get { return IsImport && JE_DeclarationType == ImportSADDeclarationTypeList.Codes.ImportFullWarehouse; } }
		public bool IsISD { get { return IsImport && JE_DeclarationType == ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration; } }
		public bool IsISW { get { return IsImport && JE_DeclarationType == ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse; } }
		public bool IsESD { get { return IsExport && JE_DeclarationType == ExportSADDeclarationTypeList.Codes.ExportSupplementaryDeclaration; } }
		public bool IsELP { get { return IsExport && JE_DeclarationType == ExportSADDeclarationTypeList.Codes.ExportLCPPreShipment; } }
		public bool IsEFD { get { return IsExport && JE_DeclarationType == ExportSADDeclarationTypeList.Codes.ExportFullDeclaration; } }
		public bool IsECR { get { return IsExport && JE_DeclarationType == ExportSADDeclarationTypeList.Codes.ExportClearanceRequest; } }
		public bool IsESP { get { return IsExport && JE_DeclarationType == ExportSADDeclarationTypeList.Codes.ExportSDPPreShipment; } }
		public bool IsEXS { get { return IsExport && JE_DeclarationType == ExportSADDeclarationTypeList.Codes.ExitSummaryDeclaration; } }

		public bool IsInventoryControlledAirImport { get { return JE_TransportMode == TransportTypeList.Codes.Air && IsInventoryControlledImport; } }

		public bool IsInventoryControlledImport
		{
			get
			{
				return IsImport
						&& JE_DeclarationType != ImportSADDeclarationTypeList.Codes.ImportFullWarehouse
						&& JE_DeclarationType != ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration
						&& JE_DeclarationType != ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse;
			}
		}

		[MaxLength(Schema.SubLocationaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ShedsList))]
		public override ZString SubLocation
		{
			get { return base.JE_SubLocationOfGoods.Right(3); }
			set
			{
				ZString result = ZString.Empty;
				if (!value.IsEmpty)
				{
					CheckMaximumLength(base.SubLocationInfo, value);
					result = JE_LocationOfGoods + value.PadRight(3).Left(3);
					SubLocationInfo.RefreshBinding();
				}
				base.JE_SubLocationOfGoods = result;
			}
		}

		protected override string SubLocationFriendlyName => "Shed";

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
		{
			return new EntryInstructionProvider(this);
		}

		public new partial class Schema : AutoGBJobDeclaration.Schema
		{
			public const string JE_GBRouteOfEntry = "JE_GBRouteOfEntry";
			public const string JE_GBIrcInventoryReturnCode = "JE_GBIrcInventoryReturnCode";
			public const string JE_Nch1RequestType = "JE_Nch1RequestType";
			public const string JE_Nch1Priority = "JE_Nch1Priority";
			public const string JE_ACAReference = "JE_ACAReference";
			public const string JE_Calc_LocationOtherInformationCountry = "JE_Calc_LocationOtherInformationCountry";
			public const string JE_Calc_LocationOtherInformationType = "JE_Calc_LocationOtherInformationType";
			public const string JE_GoodsLocation = "JE_GoodsLocation";
			public const int JE_GoodsLocationMaxLength = 15;
			public const string JE_CHIEF_GoodsLocation = "JE_CHIEF_GoodsLocation";
			public const int JE_CHIEF_GoodsLocationMaxLength = 3;
			public const string JE_EntrySubStyle = "JE_EntrySubStyle";
			public const string JE_DeclarationType = "JE_DeclarationType";
			public const string ZG_ImportClearanceStatusICSDescription = "ZG_ImportClearanceStatusICSDescription";
			public const string JE_GBRouteOfEntryDescription = "JE_GBRouteOfEntryDescription";
			public const string ZG_StyleOfEntrySOEDescription = "ZG_StyleOfEntrySOEDescription";
			public const string CourierConsignmentType = "CourierConsignmentType";
			public const string JE_MasterUCR = "JE_MasterUCR";

			public const int JE_DeclarationTypeMaxLength = 3;
			public const int JE_EntrySubStyleMaxLength = 1;
			public const int SubLocationaxLength = 3;
			public new const int JE_CustomsOfficeMaxLength = 8;
			public new const int JE_CustomsProfileMaxLength = 3;
			public new const int JE_PaymentMethodMaxLength = 1;
			public const int JE_ACAReferenceMaxLength = 9;
			public new const int JE_LocationQualifierMaxLength = 2;
			public const int JE_Calc_LocationOtherInformationCountryMaxLength = 2;
			public const int JE_Calc_LocationOtherInformationTypeMaxLength = 2;
			public const int ZG_VATDeferNumberMaxLength = 7;
			public const int CourierConsignmentTypeMaxLength = 3;
		}

		public new JobComInvoiceGroupHeader TopGroupInvoice => (JobComInvoiceGroupHeader)base.TopGroupInvoice;

		[ChildEditable(true)]
		public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

		protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

		public bool JE_EntrySubStyle_ReadOnly => CustomsEntryInstructions.Count > 1;

		#region EntrySubStyle
		[MaxLength(Schema.JE_EntrySubStyleMaxLength)]
		[List(nameof(CusEntryInstruction) + "." + nameof(JobDeclaration.CusEntryInstruction.Lookups) + "." + nameof(CusEntryInstructionLookups.EntrySubStyleList))]
		[ReadOnlyMember(nameof(JE_EntrySubStyle_ReadOnly))]
		public ZString JE_EntrySubStyle
		{
			get
			{
				return CusEntryInstruction.CEI_SubStyle.Left(1);
			}
			set
			{
				CheckMaximumLength(JE_EntrySubStyleInfo, value);
				CusEntryInstruction.CEI_SubStyle = value.Left(1);

				if (!IsValidationSuspended)
				{
					Validation.ValidateJE_EntrySubStyle();
				}
				JE_EntrySubStyleInfo.RefreshBinding();
				MarkInvoiceLinesAsNeedingValidation();
			}
		}

		public ZPropertyInfo JE_EntrySubStyleInfo
		{
			get { return GetZPropertyInfo(Schema.JE_EntrySubStyle); }
		}
		#endregion

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
		public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

		protected override void SetDefaultValues()
		{
			using (SuspendSettingHasChanges())
			{
				base.SetDefaultValues();
				ZG_ShipmentType = ShipmentTypeList.Codes.BasicDirect;
				ApplicationExtender.DefaultLocationCountry(this);
			}
		}

		protected override ZString DefaultDataGroupingCore => (ApplicationExtender?.GetDefaultDataGroupingCode(this) ?? CountryCode);
		protected override ZString DefaultDataGroupingForTariffsCore => (ApplicationExtender?.GetDefaultDataGroupingCodeForTariffs(this) ?? CountryCode);
		protected override ZString DefaultDataGroupingForDutyRateCodesCore => (ApplicationExtender?.GetDefaultDataGroupingCodeForDutyRateCodes(this) ?? CountryCode);

		protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
		{
			return new JobDeclarationSynchroniser(this);
		}

		public bool IsSendForeignEoriToCds => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.SendForeignEoriToCds, CountryCodes.UnitedKingdom, ZDateTime.Now);

		protected override ZString GetTransportModeGeneric()
		{
			switch (JE_TransportMode)
			{
				case TransportTypeList.Codes.Air:
				case TransportTypeList.Codes.Sea:
				case TransportTypeList.Codes.Rail:
				case TransportTypeList.Codes.Road:
				case TransportTypeList.Codes.FixedTransportInstallations:
				case TransportTypeList.Codes.OwnPropulsion:
				case TransportTypeList.Codes.InlandWaterwayTransport:
				case TransportTypeList.Codes.Mail:
					return base.GetTransportModeGeneric();
				case GBTransportTypeList.Codes.ROR:
					return TransportTypeGenericList.Codes.Other;
				default:
					return ZString.Empty;
			}
		}

		public bool IsRoRoLocation
		{
			get
			{
				var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, JE_LocationOfGoods, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today, null, new ZString[] { RefCusCodeListAttributeTypes.Codes.RoRoLocation });
				return (cusCode != null && (cusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.RoRoLocation, "") || cusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.RoRoLocation, "MIXED")));
			}
		}

		public bool IsRoRoOnlyLocation
		{
			get
			{
				var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, JE_LocationOfGoods, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today, null, new ZString[] { RefCusCodeListAttributeTypes.Codes.RoRoLocation });
				return (cusCode != null && (cusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.RoRoLocation, "")));
			}
		}

		public bool IsRoRoMixedLocation
		{
			get
			{
				var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, JE_LocationOfGoods, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today, null, new ZString[] { RefCusCodeListAttributeTypes.Codes.RoRoLocation });
				return (cusCode != null && cusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.RoRoLocation, "MIXED"));
			}
		}

		protected bool IsLocationCodeNorthernIreland(string code)
		{
			return new RefUNLOCO.Loader(Factory).Load(code)?.IsInNorthernIreland ?? false;
		}
		public bool IsOriginNorthernIreland => IsLocationCodeNorthernIreland(JE_RL_NKOrigin);
		public bool IsDestinationNorthernIreland => IsLocationCodeNorthernIreland(JE_RL_NKFinalDestination);

		protected override string SubmissionTypeBuiltinCode => Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

		internal bool WasSavedAsCHIEF => IsInDatabase && Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF.Equals(JE_ApplicationCodeInfo.OriginalValue.ToString(), StringComparison.InvariantCultureIgnoreCase);
		protected override bool SupportMultipleBuiltInTypes => WasSavedAsCHIEF;

		public override void DefaultValueForFakeDeclaration()
		{
			JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
		}

		protected override string DefaultTransportMode
		{
			get
			{
				var result = base.DefaultTransportMode;
				if (IsRoRoOnlyLocation)
				{
					JE_TransportMode = GBTransportTypeList.Codes.ROR;
				}

				return result;
			}
		}

		void SetApplicationCodeFromBadge()
		{
			if (JE_ApplicationCode != Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced)
			{
				BadgeCodeSettingCollection coll = GB.Registry.GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(this.CompanyPK.ToGuid(), this.RegistryBranchPK, Guid.Empty);
				BadgeCodeSetting badge = coll.FindByBadgeCode(JE_CustomsProfile, JE_MessageType);
				if (badge != null)
				{
					if (badge.ApplicationCode.IsEmpty)
					{
						JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
					}
					else
					{
						JE_ApplicationCode = badge.ApplicationCode;
					}
				}
			}
		}

		public override ZGuid JE_JS
		{
			get { return base.JE_JS; }
			set
			{
				base.JE_JS = value;
				CusHAWB linkedHawb = null;
				DetermineHawbForSynching(Shipment, out linkedHawb);
				if (linkedHawb != null)
				{
					linkedHawb.CS_JE_CustomsFormalEntry = PK;
				}
			}
		}

		[ResourceStringData("8FD15E7A-05DB-4AF0-B4C6-EE0DA8654A5B", Caption = "[25] Transport", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("677F0A6F-9307-4D5E-8E04-7BDEBA497587", Caption = "[UCC 7/4] Transport", MediumCaption = "[UCC 7/4] Transp.", ShortCaption = "[UCC 7/4] Transp.", FullDescription = "The Mode of Transport at the Border describes how the goods will cross the border.")]
		public override ZString JE_TransportMode
		{
			get { return base.JE_TransportMode; }
			set
			{
				bool hasChanged = JE_TransportMode != value;
				if (!IsCopying && !fIsImportingData && !IsSettingDefaultValues && hasChanged)
				{
					if (value == TransportTypeList.Codes.Sea || value == TransportTypeList.Codes.Air)
					{
						ZG_Box18TransportID = ZString.Empty;
						ZG_Box18TransportNationality = ZString.Empty;
					}
				}

				base.JE_TransportMode = value;
			}
		}

		public ResourceStringData CaptionResourceStringForProperty(string propertyName) => DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), propertyName, MultipleKeysToUse);
		public string CaptionForProperty(string propertyName) => CaptionResourceStringForProperty(propertyName)?.Caption ?? string.Empty;

		[ResourceStringData("E75EFA97-24E1-41A8-9DBC-31A0FACED64F", Caption = "Flight", MultipleKey = MultipleKeyChief, IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("C87C9068-5C7E-4185-9B7C-D909506A1C86", Caption = "[UCC 7/9] Identity of the means of transport at departure - Flight Number", ShortCaption = "[UCC 7/9] Flight No", FullDescription = "Enter the Flight Number for when the goods are presented and the customs formalities for their release are to be completed.", MultipleKey = MultipleKeyCdsImport, IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("DA5ADAE3-BEE2-466F-B6C6-BD88F0785AE7", ShortCaption = "[UCC 7/7] Flight No", Caption = "[UCC 7/7] Identity of the means of transport at departure - Flight Number", FullDescription = "Enter the Flight Number for which the goods are directly loaded at the time of export.", MultipleKey = MultipleKeyCdsExport, IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("ED849947-FC56-43BC-8824-80260B8C0C64", Caption = "[UCC 7/9] Flight No", MultipleKey = MultipleKeyCdsMisc, IsApplicableMember = nameof(IsAir))]
		public override ZString JE_VoyageFlightNo { get => base.JE_VoyageFlightNo; set => base.JE_VoyageFlightNo = value; }

		public void DetermineHawbForSynching(Freight.Forwarding.Business.ForwardingShipment shipment, out CusHAWB hawbFoundResult)
		{
			hawbFoundResult = null;

			if (!IsInDatabase && shipment != null)
			{
				JE_MessageType = shipment.IsImport() ? MessageTypeList.Codes.Import : shipment.IsExport() ? MessageTypeList.Codes.Export : (string)JE_MessageType;
			}

			if (IsImport && (JE_TransportMode.IsEmpty || JE_TransportMode == TransportTypeList.Codes.Air))  // Mode will be empty if the declaration is brand-new (first click brokerage tab)
			{
				var consol = RelevantConsol; // pulled from Shipment but only works after JE_MessageType is correctly set, pffff.
				if (consol != null && consol.IsDirect)
				{
					// Two loads because an inner join (i.e. ZDbOnlySubQuery) requires IsInDatabase, which cannot be guaranteed
					var basicQuery = new ZQuery(CusMAWBSchema.CM_JK, consol.PK);
					basicQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
					var basics = Factory.Load<CusMAWB>(basicQuery);
					if (basics.Length == 1)
					{
						var houseHelperQuery = new ZQuery(CusHAWBSchema.CS_CM, basics[0].PK);
						houseHelperQuery.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, true);
						hawbFoundResult = Factory.LoadTop1<CusHAWB>(houseHelperQuery);
					}
					else if (basics.Length > 1)
					{
						// multiple inventory records, e.g. at different sheds/airports. Make an educated guess by selecting the most recent basic without an open customs action code.  If more than one candidate exists, do not synch.
						var hawbHelpersWithNoCustomsActionCodeQuery = new ZQuery(CusHAWBSchema.CS_CM, (from CusMAWB b in basics where b.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk select b.PK)); //basicPks);
						hawbHelpersWithNoCustomsActionCodeQuery.AddToFilter(CusHAWBSchema.CS_IsMasterHouse, true);
						hawbHelpersWithNoCustomsActionCodeQuery.AddToFilter(CusHAWBSchema.CS_CustomsStatus, new ZString[] { "", "CX" });  // what about CA?
						var workerHousesForBasicsWithoutCustomsActionCode = Factory.Load<CusHAWB>(hawbHelpersWithNoCustomsActionCodeQuery);
						if (workerHousesForBasicsWithoutCustomsActionCode.Length == 1)  // if we find more than one, do not synch
						{
							hawbFoundResult = workerHousesForBasicsWithoutCustomsActionCode[0];
						}
					}
				}
				else
				{
					if (shipment != null)
					{
						var houses = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JS, shipment.PK));
						var openBritishHouses = (from CusHAWB h in houses where new ZString[] { "", "CX" }.Contains(h.CS_CustomsStatus) && h.MAWB.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk select h);
						var houseHits = openBritishHouses.Count();
						if (houseHits == 1)
						{
							hawbFoundResult = openBritishHouses.First();
						}
					}
				}
			}
		}

		protected override void CleanUpNewDeclarationAfterCloneCore(BaseJobDeclaration result, CloneType cloneType)
		{
			base.CleanUpNewDeclarationAfterCloneCore(result, cloneType);

			var gbDeclaration = ((JobDeclaration)result);
			if (cloneType == CloneType.DeepTemplateCopy && IsExport && IsAir)
			{
				gbDeclaration.JE_MasterUCR = JE_MasterUCR;
			}

			gbDeclaration.ZG_ImportClearanceStatusICS = ZString.Empty;
			gbDeclaration.JE_GBRouteOfEntry = ZString.Empty;
			gbDeclaration.JE_ACAReference = ZString.Empty;
			gbDeclaration.ClearFecChallenges();
			foreach (EU.Business.Declaration.JobComInvoiceLine line in gbDeclaration.InvoiceLines)
			{
				line.ClearFecDSTChallengeAndTickbox();
			}
		}

		void ClearFecChallenges()
		{
			foreach (CusEntryHeader header in CustomsEntryHeaders)
			{
				header.FECChallenges.RemoveAndDeleteAll();
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ImportClearanceStatusICSList))]
		public override ZString JE_ImportClearanceStatusICS
		{
			get => base.JE_ImportClearanceStatusICS;
			set => base.JE_ImportClearanceStatusICS = value;
		}

		[ResourceStringData("495C5A91-3665-467A-8BD0-DF12FD5C1067", Caption = "ICS", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("015CE55B-9067-478C-A197-E436EF86FAA0", Caption = "Customs Position Reason", FullDescription = "The Customs Position Reason gives the current status code for the declaration or the reason for rejection.  It will only be set when a query message is sent to CDS and the response processed.")]
		[ReadOnlyMember(nameof(ZG_ImportClearanceStatusICS_ReadOnly))]
		public override ZString ZG_ImportClearanceStatusICS
		{
			get
			{
				return GetEntryDataIfAllSameOrElseReturnEmpty(x => x.CH_ImportClearanceStatusICS);
			}
			set
			{
				base.ZG_ImportClearanceStatusICS = value;
				CustomsEntryHeaders.ForEach(x => x.CH_ImportClearanceStatusICS = value);
			}
		}
		protected virtual bool ZG_ImportClearanceStatusICS_ReadOnly => true;

		protected override ZInt MaximumNumberOfPacksForEntryLineCore => 99999;

		public bool ICSMeansClear(ZString icsCode)
		{
			// TODO - these are defined in Chief, use their code values
			var codesThatMeanClear = new List<ZString>() { "01", "04", "15", "13", "23", "33", "43", "03", "21" };
			return codesThatMeanClear.Contains(icsCode);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			// To help with JE_GBRouteOfEntry via (GB)CusAddInfo
			return new JobDeclarationFetchStrategy(this);
		}

		bool Nch1ReadOnly
		{
			get
			{
				return !(JE_GBRouteOfEntry.Contains("1", StringComparison.OrdinalIgnoreCase)
						  || JE_GBRouteOfEntry.Contains("2", StringComparison.OrdinalIgnoreCase)
						  || JE_GBRouteOfEntry.Contains("3", StringComparison.OrdinalIgnoreCase));
			}
		}

		[MaxLength(GBCusAddInfo.Schema.G9_Nch1RequestTypeMaxLength)]
		[ResourceStringData("JobDeclaration.JE_Nch1RequestType", ShortCaption = "Type", MediumCaption = "Request Type", Caption = "NCH1 Request Type/Reason", FullDescription = "Form NCH1, question 5.")]
		[List(nameof(GbNch1RequestTypeList))]
		[ReadOnlyMember(nameof(Nch1ReadOnly))]
		public ZString JE_Nch1RequestType
		{
			get { return GbCusAddInfo.G9_Nch1RequestType; }
			set
			{
				GbCusAddInfo.G9_Nch1RequestType = value;
				JE_Nch1RequestTypeInfo.RefreshBinding();
			}
		}

		public ZString JE_Nch1RequestTypeDescription
		{
			get { return GbNch1RequestTypeList.GetDescriptionFromCode(JE_Nch1RequestType); }
		}

		public ZPropertyInfo JE_Nch1RequestTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_Nch1RequestType, x => GbCusAddInfo.G9_Nch1RequestTypeInfo); }
		}

		public CodeDescriptionPairList GbNch1RequestTypeList
		{
			get { return Factory.GetCachedValue<Nch1RequestTypes>(); }
		}

		#region Value Build up

		protected override bool ShowApportionmentMenuItemCore => UseStandardValuation;

		public bool UseStandardValuation => !ZG_ManualCalc;

		public override ZBool ZG_ApportionByWeight
		{
			get => base.ZG_ApportionByWeight;
			set
			{
				base.ZG_ApportionByWeight = value;
				if (UseStandardValuation)
				{
					TopGroupInvoice.Charges.Cast<GroupInvoiceCharge>().ForEach(i => i.UpdateDistributeBy());
				}
				RefreshBinding();
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZDecimal ZG_OSAirTransportAmount
		{
			get => UseStandardValuation ? this.OSAirTransportAmount().Amount : base.ZG_OSAirTransportAmount;
			set
			{
				var oldValue = ZG_OSAirTransportAmount;
				base.ZG_OSAirTransportAmount = value;
				if (oldValue != ZG_OSAirTransportAmount)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZDecimal ZG_FrtChgAmt
		{
			get => UseStandardValuation ? this.FrtChgAmt().Amount : base.ZG_FrtChgAmt;
			set
			{
				var oldValue = ZG_FrtChgAmt;
				base.ZG_FrtChgAmt = value;
				if (oldValue != ZG_FrtChgAmt)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZString ZG_RX_NKFrtChg
		{
			get => UseStandardValuation ? (ZString)(this.FrtChgAmt().Currency?.Code ?? string.Empty) : base.ZG_RX_NKFrtChg;
			set => base.ZG_RX_NKFrtChg = value;
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZDecimal ZG_DiscAmt
		{
			get => UseStandardValuation ? this.DiscAmt().Amount : base.ZG_DiscAmt;
			set
			{
				var oldValue = ZG_DiscAmt;
				base.ZG_DiscAmt = value;
				if (oldValue != ZG_DiscAmt)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZString ZG_RX_NKDisc
		{
			get => UseStandardValuation ? (ZString)(this.DiscAmt().Currency?.Code ?? string.Empty) : base.ZG_RX_NKDisc;
			set
			{
				base.ZG_RX_NKDisc = value;
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZDecimal ZG_DiscPerc
		{
			get => UseStandardValuation ? this.DiscPercentage() : base.ZG_DiscPerc;
			set
			{
				var oldValue = ZG_DiscPerc;
				base.ZG_DiscPerc = value;
				if (oldValue != ZG_DiscPerc)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZDecimal ZG_InsAmt
		{
			get => UseStandardValuation ? this.InsAmt().Amount : base.ZG_InsAmt;
			set
			{
				var oldValue = ZG_InsAmt;
				base.ZG_InsAmt = value;
				if (oldValue != ZG_InsAmt)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZString ZG_RX_NKIns
		{
			get => UseStandardValuation ? (ZString)(this.InsAmt().Currency?.Code ?? string.Empty) : base.ZG_RX_NKIns;
			set
			{
				base.ZG_RX_NKIns = value;
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZDecimal ZG_OthChgAmt
		{
			get => UseStandardValuation ? this.OthChgAmt().Amount : base.ZG_OthChgAmt;
			set
			{
				var oldValue = ZG_OthChgAmt;
				base.ZG_OthChgAmt = value;
				if (oldValue != ZG_OthChgAmt)
				{
					MarkInvoiceLinesAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZString ZG_RX_NKOthChg
		{
			get => UseStandardValuation ? (ZString)(this.OthChgAmt().Currency?.Code ?? string.Empty) : base.ZG_RX_NKOthChg;
			set
			{
				base.ZG_RX_NKOthChg = value;
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZDecimal ZG_VATAdjAmt
		{
			get => UseStandardValuation ? this.VATAdjAmt().Amount : base.ZG_VATAdjAmt;
			set
			{
				base.ZG_VATAdjAmt = value;
				if (!value.IsEmpty && ZG_RX_NKVATAdj.IsEmpty)
				{
					ZG_RX_NKVATAdj = LocalCurrencyCode;
				}
			}
		}

		[ReadOnlyMember(nameof(UseStandardValuation))]
		public override ZString ZG_RX_NKVATAdj
		{
			get => UseStandardValuation ? (ZString)(this.VATAdjAmt().Currency?.Code ?? string.Empty) : base.ZG_RX_NKVATAdj;
			set
			{
				base.ZG_RX_NKVATAdj = value;
			}
		}
		#endregion

		[MaxLength(GBCusAddInfo.Schema.G9_Nch1RequestTypeMaxLength)]
		[ResourceStringData("JobDeclaration.JE_Nch1Priority", ShortCaption = "Priority", MediumCaption = "Priority", Caption = "Request Priority", FullDescription = "Form NCH1, question 10.")]
		[List(nameof(GbNch1PriorityList))]
		[ReadOnlyMember(nameof(Nch1ReadOnly))]
		public ZString JE_Nch1Priority
		{
			get { return GbCusAddInfo.G9_Nch1Priority; }
			set
			{
				GbCusAddInfo.G9_Nch1Priority = value;
				JE_Nch1PriorityInfo.RefreshBinding();
			}
		}

		public ZString JE_Nch1PriorityDescription
		{
			get { return GbNch1PriorityList.GetDescriptionFromCode(JE_Nch1Priority); }
		}

		public ZPropertyInfo JE_Nch1PriorityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JE_Nch1Priority, x => GbCusAddInfo.G9_Nch1PriorityInfo); }
		}

		public CodeDescriptionPairList GbNch1PriorityList
		{
			get { return Factory.GetCachedValue<Nch1PriorityTypes>(); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(GBCusAddInfo.Schema.G9_IrcInventoryReturnCodeMaxLength)]
		[ResourceStringData("JobDeclaration.JE_GBIrcInventoryReturnCode", ShortCaption = "IRC", MediumCaption = "Inventory Match", Caption = "Inventory Return Code", FullDescription = "Inventory Return Code (IRC) for last E0 report processed.")]
		[List(nameof(GbInventoryReturnCodeIRCList))]
		[ReadOnly(true)]
		public ZString JE_GBIrcInventoryReturnCode
		{
			get { return SingleEntry?.CH_IrcInventoryReturnCode ?? string.Empty; }
			set
			{
				if (SingleEntry != null)
				{
					SingleEntry.CH_IrcInventoryReturnCode = value;
				}
				GbCusAddInfo.G9_IrcInventoryReturnCode = value;
				JE_GBIrcInventoryReturnCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_GBIrcInventoryReturnCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JE_GBIrcInventoryReturnCode); }
		}

		public CodeDescriptionPairList GbInventoryReturnCodeIRCList
		{
			get
			{
				return Factory.GetCachedValue("GB.Declaration.GbInventoryResurnCodeIRCList." + ZG_Gateway, () =>
				  {
					  CodeDescriptionPairList result;
					  if (IsCCSUK)
					  {
						  result = new InventoryReturnCodesCCS();
					  }
					  else if (IsCNS)
					  {
						  result = new InventoryReturnCodesCNS();
					  }
					  else if (IsPentant)
					  {
						  result = new InventoryReturnCodesPentant();
					  }
					  else if (ZG_Gateway == GatewayList.Codes.MCP_CUSDECOnly)
					  {
						  result = new InventoryReturnCodesMCP();
					  }
					  else
					  {
						  result = new CodeDescriptionPairList();
					  }
					  return result;
				  });
			}
		}

		public bool ForceNewAcaReferenceOnSaving { get; set; }

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (ForceNewAcaReferenceOnSaving)
			{
				AcaHelper.ForceAllocateNewAca(this);
			}
			else
			{
				AcaHelper.AllocateNewAcaIfNeededOnFirstSaving(this);
			}
			ForceNewAcaReferenceOnSaving = false;
		}

		public bool IsCCSUK => ZG_Gateway == GatewayList.Codes.CCSUKviaNTMsgGW;
		public bool IsPentant => ZG_Gateway == GatewayList.Codes.Pentant;
		public bool IsMCP => ZG_Gateway == GatewayList.Codes.MCP_CUSDECOnly;
		public bool IsCNS => ZG_Gateway == GatewayList.Codes.CNS_CUSDECOnly;
		public bool IsCNSAirImport => IsCNS && IsAir && IsImport;

		[MaxLength(Schema.JE_ACAReferenceMaxLength)]
		public ZString JE_ACAReference
		{
			get
			{
				return CusEntryNumber.Load(this, "ACA", Core.Constants.CountryCodes.UnitedKingdom)?.CE_EntryNum ?? ZString.Empty;
			}
			set
			{
				if (!value.IsEmpty)
				{
					var newCen = CusEntryNumber.LoadOrCreate(this, "ACA", Core.Constants.CountryCodes.UnitedKingdom);
					newCen.CE_EntryNum = value;
					newCen.CE_Category = "OTH";
					AdditionalReferenceNumbers.Load();
					AdditionalReferenceNumbers.RefreshBindingIncludingChildren();
				}
				else
				{
					var cen = CusEntryNumber.Load(this, "ACA", Core.Constants.CountryCodes.UnitedKingdom);
					if (cen != null)
					{
						cen.Delete();
					}
				}
			}
		}

		[ResourceStringData("C7EEE14C-81F1-41A7-99D4-A44BA5196E50", Caption = "SOE", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("900ACACB-C7B1-4715-BD0C-8B1C8B73C17B", Caption = "Topic" , MultipleKey = MultipleKeyCdsImport)]
		[ResourceStringData("4EEBC1F6-8A58-4020-BDF5-B4D6B4C2B102", ShortCaption = "Top.", Caption = "Topic", FullDescription = "The Topic gives the Status of Entry (SOE) code for the export declaration.")]
		public override ZString ZG_StyleOfEntrySOE
		{
			get => GetEntryDataIfAllSameOrElseReturnEmpty(x => x.CH_StyleOfEntrySOE);
			set
			{
				CustomsEntryHeaders.ForEach(x => x.CH_StyleOfEntrySOE = value);
				base.ZG_StyleOfEntrySOE = value;
			}
		}

		[BusinessObjectTestExclude]
		[MaxLength(GBCusAddInfo.Schema.G9_RouteOfEntryMaxLength)]
		[ResourceStringData("FC9F75EC-370B-4B26-8217-582458B60DF8", Caption = "Route", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("D0A2E4F1-3C5B-4F7A-8E6C-9D1B0F5A2D3E", Caption = "Route of Examination", FullDescription = "The Route of Entry code determines how the goods will be cleared through customs and what checks will need to be performed. It will only be set when a query message is sent to CDS and the response processed.")]
		public ZString JE_GBRouteOfEntry
		{
			get => GetEntryDataIfAllSameOrElseReturnEmpty(x => x.CH_RouteOfEntry);
			set
			{
				CustomsEntryHeaders.ForEach(x => x.CH_RouteOfEntry = value);
				GbCusAddInfo.G9_RouteOfEntry = value;
				JE_GBRouteOfEntryInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JE_GBRouteOfEntryInfo
		{
			get { return GetZPropertyInfo(Schema.JE_GBRouteOfEntry, "Route"); }
		}

		[ResourceStringData("Enterprise.Customs.GB.Business.Declaration.JobDeclaration|ZG_UsePostponedVatAccounting", Caption = "Use postponed VAT accounting?")]
		public override ZBool ZG_UsePostponedVatAccounting
		{
			get => base.ZG_UsePostponedVatAccounting;
			set => base.ZG_UsePostponedVatAccounting = value;
		}

		public ZString JE_GBRouteOfEntryDescription => Lookups.EntryStatusList.GetDescriptionFromCode(GetStatusChecker().GetStatusCodeFromRouteOfEntry(JE_GBRouteOfEntry));

		public ZString ZG_StyleOfEntrySOEDescription => AddInfoLookups.StyleOfEntrySOEList.GetDescriptionFromCode(ZG_StyleOfEntrySOE);

		public ZString ZG_ImportClearanceStatusICSDescription => Lookups.ImportClearanceStatusICSList.GetDescriptionFromCode(ZG_ImportClearanceStatusICS);

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.GbNIMode))]
		public override ZString JE_NorthernIrelandMode
		{
			get => base.JE_NorthernIrelandMode;
			set => base.JE_NorthernIrelandMode = value;
		}

		[ReadOnly(true)]
		public override ZBool JE_IsGvmsPort
		{
			get => base.JE_IsGvmsPort;
			set => base.JE_IsGvmsPort = value;
		}

		public override ZBool JE_ClaimEuSubsidy
		{
			get => base.JE_ClaimEuSubsidy;
			set => base.JE_ClaimEuSubsidy = value;
		}

		public override ZBool JE_NiGoodsAtRiskOfMovingToROI
		{
			get => base.JE_NiGoodsAtRiskOfMovingToROI;
			set => base.JE_NiGoodsAtRiskOfMovingToROI = value;
		}

		[ChildEditable(true)]
		CusAddInfoCollection<GBCusAddInfo> GbCusAddInfosCollectionOfOneItem
		{
			get
			{
				if (gbCusAddInfosCollectionOfOneItem == null)
				{
					gbCusAddInfosCollectionOfOneItem = new CusAddInfoCollection<GBCusAddInfo>(this);
					gbCusAddInfosCollectionOfOneItem.Load();
					RegisterEditableChildObject(gbCusAddInfosCollectionOfOneItem);
				}
				return gbCusAddInfosCollectionOfOneItem;
			}
		}
		CusAddInfoCollection<GBCusAddInfo> gbCusAddInfosCollectionOfOneItem;
		GBCusAddInfo GbCusAddInfo
		{
			get
			{
				if (GbCusAddInfosCollectionOfOneItem.Count == 0)
				{
					var addInfo = GbCusAddInfosCollectionOfOneItem.AddNew();
					addInfo.HasChanges = false;
				}
				var v = GbCusAddInfosCollectionOfOneItem[0].Data;
				v.Declaration = this;
				return v;
			}
		}

		public bool IsSFD
		{
			get { return IsIFD && SimplifiedFrontierHelper.IsSimplifiedFrontierSubstyle(JE_EntrySubStyle); }
		}

		protected override ZBool IsReciprocalRatesCore
		{
			get { return IsReciprocalRatesConstant; }
		}

		internal static bool IsReciprocalRatesConstant
		{
			get { return false; }
		}

		protected override ZString LocalCurrencyCodeCore
		{
			get { return LocalCurrencyConstantCode; }
		}

		internal static ZString LocalCurrencyConstantCode
		{
			get { return Enterprise.Core.Constants.CurrencyCodes.UnitedKingdom; }
		}

		public CusEntryInstruction CusEntryInstruction => GetCusEntryInstruction();

		protected CusEntryInstruction GetCusEntryInstruction()
		{
			if (!IsDeleted && (cusEntryInstruction == null || cusEntryInstruction.IsDeleted))
			{
				cusEntryInstruction = CustomsEntryInstructionProvider.CustomsEntryInstructions.OfType<CusEntryInstruction>().OrderBy(x => x.PK).FirstOrDefault();
				if (cusEntryInstruction == null)
				{
					cusEntryInstruction = (CusEntryInstruction)CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				}
				if (cusEntryInstruction != null)
				{
					RegisterEditableChildObject(cusEntryInstruction);
					if (!IsPersistent)
					{
						cusEntryInstruction.MakeNonPersistent(); // Otherwise, the stupid declaration-faker thing for standalone invoices would leave a CEI without a JE, which cannot be saved
					}
				}
			}
			return cusEntryInstruction;
		}
		CusEntryInstruction cusEntryInstruction;

		protected override void WarehouseDocAddress_ValueChanged(object sender, EventArgs e)
		{
			JobDocAddress warehouse = sender as JobDocAddress;
			var isCusEntryInstructionNotNull = CusEntryInstruction != null;
			if (warehouse != null)
			{
				var oldValue = warehouse.E2_OA_Address;
				base.WarehouseDocAddress_ValueChanged(sender, e);
				GetValueSetStrategy().ValueSet(warehouse.E2_OA_AddressInfo, oldValue);
				if (IsWarehouseNeeded && isCusEntryInstructionNotNull)
				{
					CusEntryInstruction.CEI_OA_Warehouse = WarehouseDocAddress.E2_OA_Address;
				}
			}

			if (warehouse.E2_OA_Address.IsEmpty && isCusEntryInstructionNotNull)
			{
				CusEntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			}
		}

		public CodeDescriptionPairList DeclarationTypeList => CusEntryInstruction.Lookups.DeclarationTypeList;

		[List(nameof(DeclarationTypeList))]
		[MaxLength(Schema.JE_DeclarationTypeMaxLength)]
		public ZString JE_DeclarationType
		{
			get
			{
				var entryInstuction = GetCusEntryInstruction();
				return entryInstuction?.CEI_Style ?? ZString.Empty;
			}
			set
			{
				var oldValue = JE_DeclarationType;
				CheckMaximumLength(JE_DeclarationTypeInfo, value);
				CusEntryInstruction.CEI_Style = value;
				if (!IsCopying && oldValue != JE_DeclarationType)
				{
					DeclarationTypeChanged();
					ApplicationExtender.GetValueStrategy(this)?.DefaultEntrySubStyle((CusEntryInstruction));
					if (!IsValidationSuspended)
					{
						Validation.ValidateJE_DeclarationType();
					}
				}
				JE_DeclarationTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo JE_DeclarationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JE_DeclarationType); }
		}

		void DeclarationTypeChanged()
		{
			CalculateMasterUCR();
			if (!IsInventoryControlledAirImport)
			{
				WipeSplitHouseReferenceSinceNotInventoryControlledAirImport();
			}
			ApplicationExtender.GetValueStrategy(this)?.ValueSet(JE_IsGvmsPortInfo, null);
		}

		public void CalculateMasterUCR()
		{
			ZString masterUCR = new MasterUCRCalculator().Calculate(this);
			if (!string.IsNullOrEmpty(masterUCR))
			{
				JE_MasterUCR = masterUCR.Left(JE_MasterUCRInfo.MaxLength);
			}
		}

		[ReadOnlyMember(nameof(IsZG_CTStatusIDReadOnly))]
		public override ZString ZG_CTStatusID
		{
			get => base.ZG_CTStatusID;
			set => base.ZG_CTStatusID = value;
		}

		[ResourceStringData("A4C05652-1D4A-49B2-86DE-6E8154D2C021", Caption = "[17] Destination", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("D4544CAD-C284-4758-8874-DA23A783EF4A", Caption = "[UCC 5/8] Destination Port", FullDescription = "Please enter the UNLOCO for the final Destination port where the goods will arrive.")]
		public override ZString JE_RL_NKFinalDestination
		{
			get { return base.JE_RL_NKFinalDestination; }
			set { base.JE_RL_NKFinalDestination = value; }
		}

		[ResourceStringData("67785277-7631-4373-AE44-734979503812", Caption = "[15] Origin", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("51A6A414-FDF4-4A52-BE70-77D553394E41", Caption = "[UCC 5/14] Origin Port", FullDescription = "Please enter the UNLOCO for the port of the goods are dispatched from.")]
		public override ZString JE_RL_NKOrigin
		{
			get { return base.JE_RL_NKOrigin; }
			set { base.JE_RL_NKOrigin = value; }
		}

		[ResourceStringData("C8C55C58-C84E-4BDE-B12D-4D32B1606B94", Caption = "[27] Load Port", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("969F0B0A-8549-407E-B2E8-BC096C0BFB34", Caption = "Load Port", FullDescription = "Please enter the UNLOCO for the port of the goods are dispatched from.")]
		public override ZString JE_RL_NKPortOfLoading
		{
			get { return base.JE_RL_NKPortOfLoading; }
			set { base.JE_RL_NKPortOfLoading = value; }
		}

		[ResourceStringData("F3CEFB36-F7DF-4C63-B8F7-4F840ECC42CD", Caption = "[30] Goods Location", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("79099EF7-F575-47CD-9E00-1FE23E6302E3", Caption = "[UCC 5/23] Location of Goods")]
		[MaxLength(Schema.JE_CHIEF_GoodsLocationMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.Locations))]
		public ZString JE_CHIEF_GoodsLocation
		{
			get { return JE_LocationOfGoods; }
			set
			{
				CheckMaximumLength(JE_CHIEF_GoodsLocationInfo, value);
				JE_LocationOfGoods = value.PadRight(3).Left(3);
				JE_CHIEF_GoodsLocationInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateJE_CHIEF_GoodsLocation();
				}
				if (IsPortGvmsArrivedAttribute(value, Core.Constants.CountryCodes.UnitedKingdom) && !IsAir && IsExport)
				{
					ChangeSubStyleFromNotArrivedToArrived();
				}
			}
		}

		public ZPropertyInfo JE_CHIEF_GoodsLocationInfo
		{
			get { return GetZPropertyInfo(Schema.JE_CHIEF_GoodsLocation); }
		}

		[ResourceStringData("A7E31699-E7EE-4B53-8C2F-3D4CA2C624DB", Caption = "Location", ShortCaption = "Loc.", FullDescription = "Goods Location for [UCC 5/23] Location of Goods.\nThis will allow customs to determine where a control will take place if required.")]
		[MaxLength(Schema.JE_GoodsLocationMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationOfGoods))]
		public ZString JE_GoodsLocation // FOR CDS ONLY - the HESLHRELX etc part
		{
			get { return JE_LocationOtherInformation.PadRight(7).SubstringSafe(6).Trim(); }
			set
			{
				CheckMaximumLength(JE_GoodsLocationInfo, value);
				JE_LocationOtherInformation = JE_Calc_LocationOtherInformationCountry.PadRight(2) + JE_Calc_LocationOtherInformationType.PadRight(2) + JE_LocationQualifier.PadRight(2) + value;
				JE_GoodsLocationInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateJE_GoodsLocation();
				}
				if (IsPortGvmsArrivedAttribute(value, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService) && !IsAir && IsExport)
				{
					ChangeSubStyleFromNotArrivedToArrived();
				}
			}
		}

		public ZPropertyInfo JE_GoodsLocationInfo
		{
			get { return GetZPropertyInfo(Schema.JE_GoodsLocation); }
		}

		[ResourceStringData("F442FB33-1726-48B7-A18A-18A6A76728EE", Caption = "Qualifier", ShortCaption = "Qua.", MediumCaption = "Loc. Qualifier", FullDescription = "Qualifier for [UCC 5/23] Location of Goods.\nThis will allow customs to determine where a control will take place if required.")]
		[MaxLength(Schema.JE_LocationQualifierMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationQualifiers))]
		public override ZString JE_LocationQualifier  // Blank or FZ or OP etc
		{
			get { return base.JE_LocationQualifier; }
			set
			{
				base.JE_LocationQualifier = value;
				JE_LocationOtherInformation = JE_Calc_LocationOtherInformationCountry.PadRight(2) + JE_Calc_LocationOtherInformationType.PadRight(2) + value.PadRight(2) + JE_GoodsLocation;
			}
		}

		[ResourceStringData("CE3824AC-5FE2-4F1B-ADF0-9CAB334AB7D8", Caption = "Country/Region", ShortCaption = "C/R", MediumCaption = "Ctry/Rgn.", FullDescription = "Country/Region of goods location for [UCC 5/23] Location of Goods.\nThis will allow customs to determine where a control will take place if required.")]
		[MaxLength(Schema.JE_Calc_LocationOtherInformationCountryMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationOfGoodsCountries))]
		public ZString JE_Calc_LocationOtherInformationCountry // GB obviously
		{
			get
			{
				return JE_LocationOtherInformation.PadRight(4).Left(2).Trim();
			}
			set
			{
				CheckMaximumLength(JE_Calc_LocationOtherInformationCountryInfo, value);
				JE_LocationOtherInformation = value.PadRight(2) + JE_Calc_LocationOtherInformationType.PadRight(2) + JE_LocationQualifier.PadRight(2) + JE_GoodsLocation;
				JE_Calc_LocationOtherInformationCountryInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateJE_Calc_LocationOtherInformationCountry();
				}
			}
		}

		public ZPropertyInfo JE_Calc_LocationOtherInformationCountryInfo
		{
			get { return GetZPropertyInfo(Schema.JE_Calc_LocationOtherInformationCountry); }
		}

		[ResourceStringData("E274BD6B-38D1-4BE6-A571-14C3FA9CA540", Caption = "Location Type", ShortCaption = "Type", FullDescription = "Location Type Code for [UCC 5/23] Location of Goods.\nThis will allow customs to determine where a control will take place if required.")]
		[MaxLength(Schema.JE_Calc_LocationOtherInformationTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationOfGoodsTypes))]
		public ZString JE_Calc_LocationOtherInformationType  // AU, etc
		{
			get
			{
				return JE_LocationOtherInformation.PadRight(5).SubstringSafe(2, 2).Trim();
			}
			set
			{
				CheckMaximumLength(JE_Calc_LocationOtherInformationTypeInfo, value);
				JE_LocationOtherInformation = JE_Calc_LocationOtherInformationCountry.PadRight(2) + value.PadRight(2) + JE_LocationQualifier.PadRight(2) + JE_GoodsLocation;
				JE_Calc_LocationOtherInformationTypeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateJE_Calc_LocationOtherInformationType();
				}
			}
		}

		public ZPropertyInfo JE_Calc_LocationOtherInformationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JE_Calc_LocationOtherInformationType); }
		}

		public ZString FullLocationOfGoods => JE_Calc_LocationOtherInformationCountry + JE_Calc_LocationOtherInformationType + JE_LocationQualifier + JE_GoodsLocation;

		protected override bool DoesCustomsEntryStatusAllowCancellation
		{
			get { return CanDeactivateCore; }
		}

		public override bool CanDelete
		{
			get { return true; }  // This member is touched when trying to deactivate or re-activate (!) a job.  Always give true and then let CanCancel (which is not touched during reactivation) to the work. Pfff.
		}

		bool CanDeactivateCore
		{
			get
			{
				reasonForNotAbleToDelete = (NoResString)string.Empty;
				var isEntryOnHold = IsEntryOnHold;
				var isCancelledOnChief = JE_EntryStatus == EntryStatusList.Codes.Cancelled;
				if (isEntryOnHold)
				{
					reasonForNotAbleToDelete = ResString.GetMultilingualString("B77D6537-5BAE-4766-86C6-A30A7D40F49B", "The entry is on hold (route 1 or 2, or status XA)");
				}
				return isCancelledOnChief || !isEntryOnHold;
			}
		}

		MultilingualString reasonForNotAbleToDelete;
		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return reasonForNotAbleToDelete; }
		}

		bool IsZG_CTStatusIDReadOnly
		{
			get { return IsExport && (IsPrelodgedAndNotCancelled || IsEntryOnHold); }
		}

		public bool IsPrelodgedAndNotCancelled
		{
			get
			{
				var routeInEnterpriseParlance = GetStatusChecker().GetStatusCodeFromRouteOfEntry(JE_GBRouteOfEntry);
				return routeInEnterpriseParlance == EntryStatusList.Codes.RouteH && JE_EntryStatus != EntryStatusList.Codes.Cancelled;
			}
		}

		public bool IsEntryOnHold
		{
			get
			{
				var routeInEnterpriseParlance = GetStatusChecker().GetStatusCodeFromRouteOfEntry(JE_GBRouteOfEntry);
				return new List<ZString>() { EntryStatusList.Codes.Route1, EntryStatusList.Codes.Route2 }.Contains(routeInEnterpriseParlance) || JE_EntryStatus == EntryStatusList.Codes.XA;
			}
		}

		[ResourceStringData("2BCCFF4B-5020-4799-9AD2-9851E1A6CB41", Caption = "Status", FullDescription = "The Status field shows the most recent status message from customs as seen on the Entries tab.", MultipleKey = MultipleKeyCdsExport)]
		[ResourceStringData("585D6DBF-B68F-47BB-AB92-9D2A9CEAB2F3", Caption = "Status", FullDescription = "The Status field shows the most recent status message from customs as seen on the Entries tab.", MultipleKey = MultipleKeyCdsImport)]
		[ResourceStringData("A4C05652-1D4A-49B2-86DE-6E8154D2C021", Caption = "Status", FullDescription = "The Status field shows the most recent status message from customs as seen on the Entries tab.", MultipleKey = MultipleKeyCdsMisc)]
		public override ZString JE_EntryStatusDescription
		{
			get
			{
				ZString description = base.JE_EntryStatusDescription;
				if (description == "Unknown" && this.JE_EntryStatus != ZString.Empty)
				{
					if (JE_EntryStatus == Customs.Common.EU.MessageStatusList.Codes.FailedFromTransmission)
					{
						description = Customs.Common.EU.MessageStatusList.Descriptions.FailedFromTransmission;
					}
					else if (!JE_GBRouteOfEntry.IsEmpty)
					{
						description = "Route " + this.JE_GBRouteOfEntry;
					}
					else
					{
						description = this.ApplicationExtender.GetEntryHeaderStatusDescription(this.JE_EntryStatus, this.JE_EntryStatus);
					}
				}
				return description;
			}
		}

		#region ICusAddInfoTypeSupporter Members

		protected override IDictionary<ZString, Type> GetCusAddInfoTypesCore()
		{
			var result = base.GetCusAddInfoTypesCore();
			result.Add(CusAddInfoTypeAttribute.Codes.GBAllSimpleProperties, typeof(CusAddInfo<GBCusAddInfo>));
			return result;
		}

		#endregion

		protected override IEnumerable<ZString> JobDeclarationMessageCollectionApplicationCodeListCore
		{
			get
			{
				return new ZString[]
				{
					ApplicationCodeList.Codes.GbCcsuk,
					ApplicationCodeList.Codes.GbCnsEdifactOutboundOnly,
					ApplicationCodeList.Codes.GbCnsCompass,
					ApplicationCodeList.Codes.GbEdifactShared,
					ApplicationCodeList.Codes.GbMcpEdifactOutboundOnly,
					ApplicationCodeList.Codes.GbMcpRra01AndRra11,
					ApplicationCodeList.Codes.GbMcpRra12,
					ApplicationCodeList.Codes.GbNesAllMessageTypes,
					ApplicationCodeList.Codes.GbMcpClaimUcn
				};
			}
		}

		[ReadOnlyMember(nameof(ZG_IsTrainingDeclaration_Readonly))]
		public override ZBool ZG_IsTrainingDeclaration
		{
			get { return base.ZG_IsTrainingDeclaration; }
			set { base.ZG_IsTrainingDeclaration = value; }
		}

		bool ZG_IsTrainingDeclaration_Readonly
		{
			get { return !DeclarationNumber.IsEmpty; }
		}

		protected override ZString TradersOwnReferenceFullForBox7Core
		{
			get
			{
				return GetBox7ReferenceFullOrTruncated(base.TradersOwnReferenceFullForBox7Core)
						  .KeepChars(ZString.AlphanumericCharacters + " ")
						  .ToUpper();
			}
		}

		protected override ZString OtherReferenceNumber
		{
			get
			{
				return JE_MasterUCR;
			}
		}

		protected override ZString OtherReferenceNumberCaption
		{
			get
			{
				return "UCR";
			}
		}

		ZString GetBox7ReferenceFullOrTruncated(ZString defaultValue)
		{
			// Options for what to do when the box 7 refer is too long and it's for CCSUK

			var result = defaultValue;
			if (IsInventoryControlledAirImport && IsCCSUK && result.Length > 8)
			{
				var registryOption = GBCustomsDataRegistry.Instance.CcsukTruncateChiefBox7ToRight8Characters.Value;
				if (registryOption == CcsukChiefBox7BehaviourList.Codes.TruncateWithIntelliganceIfSpaceAllowsSendRightmost8CharactersAndThenFullReferenceOtherwiseSameAsDef)
				{
					result = (result.Length <= 12)   // 21 for chief less 8 for ccsuk less 1 for a space:  "SLHR12345678" becomes "12345678 SLHR12345678", but "SLHR00000012345678" is sent unchanged as "SLHR00000012345678"
								 ? string.Format("{0} {1}", result.Right(8), result)
								 : result.ToString();
				}
				else if (registryOption == CcsukChiefBox7BehaviourList.Codes.TruncateWithCompromiseSendRightmost8CharactersAndThenLeftmost12Characters)
				{
					result = string.Format("{0} {1}", result.Right(8), result.Left(12));  // SLHR00000012345678 becomes "12345678 SLHR00000012"
				}
				else if (registryOption == CcsukChiefBox7BehaviourList.Codes.TruncateSimplySendRightmost8CharactersOnly)
				{
					result = result.Right(8);
				}
			}
			return result;
		}

		#region Supplementary declaration statistics

		public ZString NumberOfLinkedSupplementaryDeclarationsString
		{
			get { return IsDeclarationTypeImpliesRequiresSupplementaryDeclaration ? NumberOfLinkedSupplementaryDeclarations.ToString() : ""; }
		}
		public ZInt NumberOfLinkedSupplementaryDeclarations
		{
			get { return GetRelatedSupplementaryDeclarationChildren().Length; }
		}

		BaseJobDeclaration[] GetRelatedSupplementaryDeclarationChildren()
		{
			var result = Array.Empty<BaseJobDeclaration>();
			var pivotQuery = new ZQuery(GenPivotSchema.XX_Relation1ID, PK);
			pivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, "SUP");
			var relatedPks = new List<ZGuid>();
			foreach (var pivot in Factory.Load<GenPivot>(pivotQuery))
			{
				relatedPks.Add(pivot.XX_Relation2ID);
			}
			if (relatedPks.Count > 0)
			{
				var decsQuery = new ZQuery(JobDeclarationSchema.PK, relatedPks);
				result = Factory.Load<BaseJobDeclaration>(decsQuery);
			}
			return result;
		}

		public ZString NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarationsString
		{
			get { return IsDeclarationTypeImpliesRequiresSupplementaryDeclaration ? NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarations.ToString() : ""; }
		}
		public ZInt NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarations
		{
			get { return GetRelatedSupplementaryDeclarationChildren().Sum(d => (int)d.JE_TotalNoOfPacks); }
		}

		public ZString NumberOfPackagesRemainingOnChildSupplementaryDeclarationsString
		{
			get { return IsDeclarationTypeImpliesRequiresSupplementaryDeclaration ? NumberOfPackagesRemainingOnChildSupplementaryDeclarations.ToString() : ""; }
		}
		public ZInt NumberOfPackagesRemainingOnChildSupplementaryDeclarations
		{
			get { return JE_TotalNoOfPacks - NumberOfPackagesDeclaredOnLinkedSupplementaryDeclarations; }
		}

		public ZBool IsDeclarationTypeImpliesRequiresSupplementaryDeclaration
		{
			get { return IsSFD || IsESP || IsELP; }
		}

		public ZBool IsSupplementaryDeclarationType
		{
			get { return IsISD || IsISW || IsESD; }
		}

		protected override string[] RelatedDeclarationTypes => new[] { string.Empty, SuppDecWizardManager.SupplementaryDeclarationRelationshipType };

		#endregion

		public ZString ReasonWhyCannotPrintNch1Document
		{
			get
			{
				return (JE_Nch1Priority.IsEmpty || JE_Nch1RequestType.IsEmpty)
					? "NCH1 form cannot be generated until the Type and Priority fields are completed.\r\nThese can only be set for Route 1 and 2 entries.\r\nSee the Misc tab."
					: "";
			}
		}
		public ZBool JE_FecDSP
		{
			get
			{
				ZBool result = false;
				foreach (CusEntryHeader header in CustomsEntryHeaders)
				{
					if (header.FECChallenges.Find(x => x.CY_Code == FECChallengeFields.Codes.JE_DSP && x.CY_ParentID == header.PK && x.CY_IsOverridden).Any())
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public ZBool JE_FecDST
		{
			get
			{
				ZBool result = false;
				foreach (CusEntryHeader header in CustomsEntryHeaders)
				{
					if (header.FECChallenges.Find(x => x.CY_Code == FECChallengeFields.Codes.JE_DST && x.CY_ParentID == header.PK && x.CY_IsOverridden).Any())
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public ZBool JE_FecFLG
		{
			get
			{
				ZBool result = false;
				foreach (CusEntryHeader header in CustomsEntryHeaders)
				{
					if (header.FECChallenges.Find(x => x.CY_Code == FECChallengeFields.Codes.JE_FLG && x.CY_ParentID == header.PK && x.CY_IsOverridden).Any())
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public CusEntryHeader SingleEntry => CustomsEntryHeaders.Count == 1 ? CustomsEntryHeaders[0] : null;

		public override SupportingDocSendingObject GetSupportingDocSendingObject() => new DocumentSending.SupportingDocSendingObject(this);

		public new GBGuaranteeCollection Guarantees => (GBGuaranteeCollection)base.Guarantees;
		protected override EU.Business.Declaration.GuaranteeForDeclarationCollection GetGuaranteesCore() => new GBGuaranteeCollection(this);

		protected override ZString CustomsVATTypeCaptionCore => DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_ZZF_NKTaxType), MultipleKeysToUse)?.Caption;

		[ResourceStringData("71CC66B4-E25C-4750-ABFB-C746B8E8E7C3", Caption = "[6] No. Pkgs.", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("11B78986-B532-455F-B362-C4E576D2220D", Caption = "Total number of Packages and Unit", ShortCaption = "No. Pkgs.", FullDescription = "Enter the number of packages and [UCC 6/9] Type of Packages making up the consignment covered by the declaration, based on the smallest external packing unit.")]
		public override ZInt JE_TotalNoOfPacks
		{
			get => base.JE_TotalNoOfPacks;
			set
			{
				bool hasChanged = JE_TotalNoOfPacks != value;

				if (!IsCopying && hasChanged)
				{
					if (CustomsEntryInstructions.Count == 1)
					{
						CusEntryInstruction.CEI_PackageCount = value;
					}
				}

				base.JE_TotalNoOfPacks = value;
			}
		}

		bool IsPortGvmsArrivedAttribute(string port, string dataGrouping)
		{
			return GetPortCodeListCombinedForPort(port, dataGrouping)?.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Any(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(GBCommonConstants.RefCusCodeListAttributeCodes.GvmsArrived)) ?? ZBool.False;
		}

		ZZRefCusCodeListCombined GetPortCodeListCombinedForPort(string port, string dataGrouping)
		{
			return Factory.GetCachedValue("GetPortCodeListCombined_" + port, delegate
			{
				var query = new ZDBOnlyQuery(typeof(ZZRefCusCodeListCombined));
				query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, dataGrouping);
				query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
				query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, port);
				return Factory.Load<ZZRefCusCodeListCombined>(query)?.FirstOrDefault();
			});
		}

		public void ChangeSubStyleFromNotArrivedToArrived()
		{
			CustomsEntryInstructions.ForEach(cei =>
			{
				if (cei.IsGoodsNotArrivedSubStyle)
				{
					cei.ChangeSubStyleFromNotArrivedToArrived();
				}
			});
		}

		protected override string GetDeclarantTypeForMatchingEORICodes()
		{
			if (IsUCCCompliant)
			{
				return "";    //for CDS.
			}
			else
			{
				return RepresentationTypeList.Codes._1Self;    //for CHIEF.
			}
		}

		protected override bool IsSupplementaryMenuVisibleCore => true;

		public ZString LocationOfGoodsForDocumentsAndMessaging
		{
			get
			{
				var location = LocationOfGoodsWithoutGbPrefix;
				return location.Length == 3 ? new ZString(Core.Constants.CountryCodes.UnitedKingdom + location) : location;
			}
		}

		public ZString LocationOfGoodsWithoutGbPrefix
		{
			get
			{
				var location = JE_LocationOfGoods;
				var facility = Shed.LoadByCode(Factory, CountryCode, JE_SubLocationOfGoods);
				if (facility != null && !facility.ChiefPort.IsEmpty)
				{
					location = facility.ChiefPort;
				}
				return location;
			}
		}

		public ZString ShedPhysicalCodeFromDatabaseOrHeathrowERT
		{
			get
			{
				Shed shed = null;
				ZString shedCode = JE_LocationOfGoods.Right(3) + SubLocation;
				if (!shedCode.IsEmpty)
				{
					shed = Shed.LoadByCode(Factory, CountryCode, shedCode);
				}
				return shed != null && (shed.IsEtsfAtLondonHeathrow || !shed.ChiefShed.IsEmpty)
						? shed.ChiefShed
						: SubLocation;
			}
		}

		protected override ZString Box30LocationOfGoodsForDocumentsAndMessagingCore
		{
			get
			{
				return LocationOfGoodsForDocumentsAndMessaging + ShedPhysicalCodeFromDatabaseOrHeathrowERT;
			}
		}

		protected override ZBool ShouldMrnBeHiddenFromShipmentEntryNumbersListViaGetValidCusEntryNumFilterCore => false;

		public bool ShouldSynchroniseGuaranteeWithCusPermitHeader => false;

		protected override EU.Business.JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelper(this);

		public int MaximumEntryLineCount
		{
			get
			{
				var result = 99;
				var maxCount = ZZCustomsFunctionalityEffectiveDate.GetEffectiveCusCodeAttribute(Constants.FunctionalityTypes.MaxEntryLines, GetDefaultDataGroupingCode(), ZDateTime.Today, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.MaxEntryLines);
				if (int.TryParse(maxCount, out var newMaxCount) && newMaxCount > 0)
				{
					result = newMaxCount;
				}

				return result;
			}
		}

		[MaxLength(Schema.CourierConsignmentTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CourierConsignmentTypes))]
		public ZString CourierConsignmentType
		{
			get => this.GetSystemDefinedValue<ZString>(Customs.Business.GenAddOnHelper.CourierConsignmentType);
			set
			{
				var oldValue = CourierConsignmentType;
				CheckMaximumLength(CourierConsignmentTypeInfo, value);
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.CourierConsignmentType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCourierConsignmentType();
				}
				CourierConsignmentTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CourierConsignmentTypeInfo => GetZPropertyInfo(Schema.CourierConsignmentType);

		public ZString CourierConsignmentReference => GetReferenceNumber(CustomsAdditionalReferenceNumbersCodes.CourierConsignmentReference); // COU

		public ZString CourierBagReference => GetReferenceNumber(CustomsAdditionalReferenceNumbersCodes.BagReference); //BAG
		public ZString CourierSiteId => Shed.LoadByCode(Factory, Core.Constants.CountryCodes.UnitedKingdom, LocationOfGoods + SubLocation)?.SiteCode ?? ZString.Empty;

		ZString GetReferenceNumber(string typeCode)
		{
			var numbers = Shipment?.Numbers ?? this.AdditionalReferenceNumbers;
			return numbers.OfType<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == typeCode)?.CE_EntryNum ?? ZString.Empty;
		}

		public ZString CourierCarrierCode => ShippingLine?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedKingdom) ?? ZString.Empty; // CarrierCode = CCC

		protected override string GetReasonForNotAbleToUpdateCore()
		{
			var result = new ZStringBuilder();
			var updatableEntryStatuses = new ZString[] { EntryStatusList.Codes.NotSent, EDIMessageStatusList.Codes.Rejected, EntryStatusList.Codes.Cancelled };
			var nonUpdatableEntries = CustomsEntryHeaders.Where(eh => !updatableEntryStatuses.Contains(eh.CH_EntryStatus)).ToArray();
			if (nonUpdatableEntries.Length > 0)
			{
				var failedMessage = Res.GetString("c54dea5d-fca5-46e1-a27e-bea870cb4176", "Entry {0} is in status {1} and the import of USXML is disallowed");
				nonUpdatableEntries.ForEach(eh => result.AppendFormat(failedMessage, eh.CH_BGMReference, eh.CH_EntryStatus));
			}
			if (CustomsEntryHeaders.Any(eh => eh.CH_EntryStatus == EntryStatusList.Codes.AwaitingResponse))
			{
				result.Append(Res.GetString("cfd00944-de31-4421-9894-2717cfc9342a", "Declaration has at least one entry which is awaiting a response from customs"));
			}
			return result.IsEmpty ? string.Empty : $"{result.ToStringWithNewLineBetweenAppends()} Job Number: {JobNumber}";
		}

		#region Override IJobDeclarationMessageSupporter Members
		protected override ZBool SupportEntryDeclarationMessageCore => true;

		protected override IProcessor GetEntryDeclarationMessageProcessorCore()
		{
			return ApplicationExtender.GetEntryDeclarationMessageProcessor(this);
		}

		#endregion

		protected override bool SupportMultipleWarehouseEntryCore => SupportsBondedWarehousingCore;

		public ZString GetCountryCodeForSupplementaryCodeProvider() => CountryCode + JE_ApplicationCode;

		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.EidrTypes))]
		[ResourceStringData("JobDeclaration.JE_EidrType", ShortCaption = "EIDR", Caption = "EIDR Type", FullDescription = "Type of Entry In Declarant's Records")]
		public override ZString JE_EidrType
		{
			get => base.JE_EidrType;
			set => base.JE_EidrType = value;
		}

		[ResourceStringData("JobDeclaration.JE_SuppDecDueDate", ShortCaption = "Supp Dec Due", Caption = "Supp Dec Due Date", FullDescription = "Supplementary Declaration Due Date")]
		public override ZDateTime JE_SuppDecDueDate
		{
			get { return base.JE_SuppDecDueDate; }
			set { base.JE_SuppDecDueDate = value; }
		}

		public bool IsNorthernIrelandDomestic => ZG_NorthernIrelandMode == NIModeList.Codes.MovementFromGreatBritainToNi;

		public bool IsNorthernIrelandImportFromRow => ZG_NorthernIrelandMode == NIModeList.Codes.ImportIntoNiFromRestOfWorld;

		public bool IsEuTariffToBeUsedForNorthernIreland => IsNorthernIrelandDomestic || (IsNorthernIrelandImportFromRow && ZG_NiGoodsAtRiskOfMovingToROI);

		[ResourceStringData("74E19C6F-F189-4452-B34F-AB17C18B0A60", Caption = "Master UCR", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("214AFCE9-EC86-4024-916D-61E66642A655", Caption = "Master UCR", FullDescription = "A MUCR (Master Unique Consignment Reference) is normally used to associate or link several Declaration UCRs for export or to provide an inventory consignment reference (ICR) for imports.")]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[BusinessObjectTestExclude]  // tested below
		public ZString JE_MasterUCR
		{
			get { return MasterUCR != null ? MasterUCR.CE_EntryNum : ZString.Empty; }
			set
			{
				if (MasterUCR == null)
				{
					masterUCR = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.EU.MasterUCR, CountryCode);
					RegisterEditableChildObject(masterUCR);
				}
				var oldValue = MasterUCR.CE_EntryNum;
				MasterUCR.CE_EntryNum = value;
				GetValueSetStrategy().ValueSet(JE_MasterUCRInfo, oldValue);
				Validation.ValidateJE_MasterUCR();
				JE_MasterUCRInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo JE_MasterUCRInfo
			=> MasterUCR != null && !MasterUCR.CE_EntryNum.IsEmpty ? GetWrappedZPropertyInfo(Schema.JE_MasterUCR, x => MasterUCR.CE_EntryNumInfo) : GetZPropertyInfo(Schema.JE_MasterUCR);

		public CusEntryNumber MasterUCR
		{
			get
			{
				if (masterUCR == null || masterUCR.IsDeleted)
				{
					masterUCR = CusEntryNumber.Load(this, CusEntryNumberTypes.EU.MasterUCR, CountryCode);

					if (masterUCR != null)
					{
						RegisterEditableChildObject(masterUCR);
					}
				}
				return masterUCR;
			}
		}
		CusEntryNumber masterUCR;

		void EnsureChangedMasterUcrSaved()
		{
			if (IsAir && IsExport && MasterUCR != null && MasterUCR.IsInDatabase && MasterUCR.HasChanges)
			{
				var oldMasterUcrFromDatabase = (ZString)MasterUCR.CE_EntryNumInfo.OriginalValue;
				if (!oldMasterUcrFromDatabase.IsEmpty && oldMasterUcrFromDatabase != JE_MasterUCR)
				{
					RecordOldMasterUcrAsPreviousDocument(oldMasterUcrFromDatabase);
				}
			}
		}

		const string MasterAirWayBillCode = "741";

		void RecordOldMasterUcrAsPreviousDocument(ZString oldMucr)
		{
			var zClass = PreviousDocumentClassList.Codes.PreviousDocument;
			var prevDoc741 = (from PreviousDocument caiPd in PreviousDocuments where caiPd.CSI_ReferenceNumber == oldMucr && caiPd.CSI_Code == MasterAirWayBillCode && caiPd.CSI_SubType == zClass select caiPd).FirstOrDefault() ?? PreviousDocuments.AddNew();
			prevDoc741.CSI_SubType = zClass;
			prevDoc741.CSI_Code = MasterAirWayBillCode;
			prevDoc741.CSI_ReferenceNumber = oldMucr;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			EnsureChangedMasterUcrSaved();
		}

		public void WipeSplitHouseReferenceSinceNotInventoryControlledAirImport()
		{
			ZG_HouseSplitReference = string.Empty;
		}

		Type ICusEntryNumberValidationDeciderOfType.GetCusEntryNumberValidationType() => CusEntryNumValidationForMUCRCore;

		protected Type CusEntryNumValidationForMUCRCore
		{
			get
			{
				return (IsCCSUK && IsInventoryControlledAirImport)
					?
					ObjectFactory.GetType<Integration.Customs.GB.CCSUK.IGbCcsukMUCREntryNumValidation>()
					:
					typeof(GbMUCREntryNumValidation);
			}
		}

		public ZString DunsForBranchOrgProxy => Branch?.OrgProxy?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Core.Constants.CountryCodes.UnitedKingdom) ?? ZString.Empty;

		protected override EU.Business.TransportModeTranslator GetTransportModeTranslator() => new TransportModeTranslator();

		protected override IApportionStrategy GetNewApportionStrategy() => ApplicationExtender.GetApportionStrategy();

		protected override bool IsInventorySelectionEnabledCore => SupportsBondedWarehousingCore && CustomsEntryInstructions.Any(x => x.WarehouseIsInventoryManagementOn);

		internal ShortSequenceNumberGenerator CusEntryInstructionSequenceNumberGenerator => fCusEntryInstructionSequenceNumberGenerator ?? (fCusEntryInstructionSequenceNumberGenerator = new ShortSequenceNumberGenerator(() => CustomsEntryInstructions));
		ShortSequenceNumberGenerator fCusEntryInstructionSequenceNumberGenerator;

		ZString GetEntryDataIfAllSameOrElseReturnEmpty(Func<CusEntryHeader, ZString> getData)
		{
			var statuses = CustomsEntryHeaders.Select(getData).Distinct().Take(2).ToArray();
			return statuses.Length == 1 ? statuses[0] : ZString.Empty;
		}

		#region Universal Copy

		protected override void AfterUniversalCopy()
		{
			base.AfterUniversalCopy();

			ApplicationExtender.AfterUniversalCopy(this);
		}

		#endregion

		public bool HasPortInventoryAttribute
		{
			get
			{
				var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, JE_CHIEF_GoodsLocation, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today, null, new ZString[] { GBCommonConstants.RefCusCodeListAttributeCodes.Inventory });
				return (cusCode != null && cusCode.HasAttribute(GBCommonConstants.RefCusCodeListAttributeCodes.Inventory));
			}
		}

		protected override bool DeclarationMessagesHaveBeenSentCore(bool reloadMessages)
		{
			if (reloadMessages)
			{
				Messages.Reload(true);
			}

			if (Messages.HasNonDiscardedMessage())
			{
				return true;
			}
			foreach (CusEntryHeader entryHeader in CustomsEntryHeaders.Cast<CusEntryHeader>())
			{
				if (reloadMessages)
				{
					entryHeader.Messages.Reload(true);
				}

				if (entryHeader.Messages.Cast<EDIMessage>().Any(x => x.EM_Status != EDIMessageStatusList.Codes.Discarded))
				{
					return true;
				}
			}
			return false;
		}

		public override bool IsCreditCheckEnabledForValidateCustomsMessaging => base.IsCreditCheckEnabledForValidateCustomsMessaging
					&& ObjectFactory.Get<Integration.Customs.GB.GBChief.IGbMessageManagerCreditCheckWithSecurityHelper>("GBChief.IGbMessageManagerCreditCheckWithSecurityHelper", this, null).ShouldCheckCreditForThisDeclarationAndMessage();

		[ResourceStringData("A7DC8FBD-84DB-4142-9619-ACE2F10595BB", Caption = "[1a] Entry Style", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("2BAE9642-3950-4136-BEEA-BCFE219CDA09", Caption = "[UCC 1/1] Dec. Type", FullDescription = "The Declaration Type determines if goods are for Rest of World or for/from a special territory of the UK.")]
		public override ZString JE_EntryStyle
		{
			get => base.JE_EntryStyle;
			set => base.JE_EntryStyle = value;
		}

		[ResourceStringData("FD1131B6-F69B-4A46-B4A3-183BC326F89F", Caption = "Entry Number", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("37B096DB-3AB8-44B0-97E4-170663F6F4B0", Caption = "MRN", FullDescription = "The MRN (Movement Reference Number) is a unique reference assigned by customs to this declaration.")]
		public override ZString DeclarationNumber
		{
			get => base.DeclarationNumber;
			set => base.DeclarationNumber = value;
		}

		[ResourceStringData("BC98911C-17EF-4A2B-92A7-77BE14097E28", Caption = "[21] Nationality", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("66C44B65-B261-4417-873C-B9F5141EE90B", Caption = "[UCC 7/15] Nationality", FullDescription = "[UCC 7/15] Nationality of active means of transport crossing the border")]
		public override ZString JE_RN_NKTransportNationality
		{
			get => base.JE_RN_NKTransportNationality;
			set => base.JE_RN_NKTransportNationality = value;
		}

		[ResourceStringData("51CECDFE-28E4-48C7-BD29-17B274D57F34", Caption = "[UCC 5/21] IATA", MediumCaption = "[UCC 5/21]", ShortCaption = "5/21", FullDescription = "[UCC 5/21] IATA Loading Location", MultipleKey = MultipleKeyCdsImport)]
		[ResourceStringData("29E69F32-4BDB-412B-8156-D7A597CBA3C8", Caption = "IATA", MultipleKey = MultipleKeyChief)]
		public override ZString JE_IATALoadPort
		{
			get => base.JE_IATALoadPort;
			set => base.JE_IATALoadPort = value;
		}

		[ResourceStringData("AFBE01C5-90E6-4B81-847B-C5F9DEE6DE06", Caption = "[21] Vessel", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("7BD46860-830B-475F-9339-2505B3EE95B7", Caption = "[UCC 7/9] Identity of Means of Transport on Arrival", ShortCaption = "[UCC 7/9] Vessel", FullDescription = "Enter the identity of the vessel at the point when the goods are presented and the customs formalities for their release are to be completed.", MultipleKey = MultipleKeyCdsImport)]
		[ResourceStringData("C4930C9E-5BBA-4A87-B99E-32AA916EEAA4", Caption = "[UCC 7/7] Identity of the means of transport at departure - Vessel", ShortCaption = "[UCC 7/7] Vessel", FullDescription = "Enter the identity of the vessel which the goods are directly loaded at the time of export.", MultipleKey = MultipleKeyCdsExport)]
		[ResourceStringData("2196DA51-F753-4EDD-98AA-782B2B266279", Caption = "[UCC 7/9] Vessel", MultipleKey = MultipleKeyCdsMisc)]
		public override ZString JE_VesselName
		{
			get => base.JE_VesselName;
			set => base.JE_VesselName = value;
		}

		[ResourceStringData("10A26EC6-7EA1-493A-BE72-64846B20A958", Caption = "[21] Trans")]
		public string DepartureTransportIDCaption => CaptionForProperty(nameof(DepartureTransportIDCaption));

		[ResourceStringData("5B08D3D1-2EF6-4F5D-92B8-C9E9C6B083EA", Caption = "[18] Transport ID (inland)", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("1747A962-24D9-4036-90D9-4E07FB2421B7", Caption = "[UCC 7/9] Identity of Means of Transport on Arrival", ShortCaption = "[UCC 7/9] Transport ID", FullDescription = "Enter the identity of the means of transport at the point when the goods are presented and the customs formalities for their release are to be completed.", MultipleKey = MultipleKeyCdsImport)]
		[ResourceStringData("8A512BDE-CF69-4FC3-8E99-6DCF2161D672", Caption = "[UCC 7/7] Identity of the means of transport at departure", ShortCaption = "[UCC 7/7] Transport ID", FullDescription = "Enter the identity of the means of transport on which the goods are directly loaded at the time of export.", MultipleKey = MultipleKeyCdsExport)]
		[ResourceStringData("163D1694-A6E5-4E88-B790-C979376E7F8C", Caption = "[UCC 7/9] Transport ID", ShortCaption = "[UCC 7/9] ID", MultipleKey = MultipleKeyCdsMisc)]
		public override ZString ZG_Box18TransportID
		{
			get => base.ZG_Box18TransportID;
			set => base.ZG_Box18TransportID = value;
		}

		[ResourceStringData("3A551871-0447-4F81-BF50-B231F2F66393", Caption = "[26] Inland M.O.T", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("CF6F7434-39F5-4B87-B981-9B26FE03D48D", Caption = "[UCC 7/5] Inland Mode of Transport", ShortCaption = "[UCC 7/5] Inland M.O.T")]
		public override ZString JE_TransportModeInland
		{
			get => base.JE_TransportModeInland;
			set => base.JE_TransportModeInland = value;
		}

		[ResourceStringData("F3CEFB36-F7DF-4C63-B8F7-4F840ECC42CD", Caption = "[30] Goods Location", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("79099EF7-F575-47CD-9E00-1FE23E6302E3", Caption = "[UCC 5/23] Location of Goods")]
		public override ZString JE_LocationOfGoods
		{
			get => base.JE_LocationOfGoods;
			set => base.JE_LocationOfGoods = value;
		}

		[ResourceStringData("AD005B37-2489-42CB-8F8D-29481D8C967A", Caption = "[20.1] Incoterm", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("A7561BF8-FAC3-4905-8743-483EF582E1BA", Caption = "[UCC 4/1] Delivery Terms - Incoterm", ShortCaption = "[UCC 4/1] Incoterm", FullDescription = "Incoterms are agreed between the supplier and buyer of the goods being moved. They set out which party is responsible for each part of the shipment.")]
		public override ZString JE_ShipmentIncoTerm
		{
			get => base.JE_ShipmentIncoTerm;
			set => base.JE_ShipmentIncoTerm = value;
		}

		[ResourceStringData("42A27ACC-3F6F-4846-A08E-FD0D61BBAF2A", Caption = "[20.2] Place", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("6BB47F1E-DFE2-4488-95DB-814EC108EA73", Caption = "[UCC 4/1] Delivery Terms - Place", ShortCaption = "[UCC 4/1] Place", FullDescription = "The Place up to which the Incoterms apply forms part of the Delivery Term. Please enter a UNLOCO or Country Code & Location Name")]
		public override ZString JE_ShipmentIncoTermPlace
		{
			get => base.JE_ShipmentIncoTermPlace;
			set => base.JE_ShipmentIncoTermPlace = value;
		}

		[ResourceStringData("108F96A1-9BB2-4FE7-995A-38D5FFC6FFAE", Caption = "[7] Declarant\'s Ref", ShortCaption = "[7] Dec. Ref", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("E808A0A3-63C1-427F-A3E0-8714C14388B4", Caption = "Declarant\'s Reference", ShortCaption = "Dec. Ref", FullDescription = "Please enter the Declarant\'s own reference for this consignment.")]
		public override ZString JE_OwnerRef
		{
			get => base.JE_OwnerRef;
			set => base.JE_OwnerRef = value;
		}

		[ResourceStringData("42120DA4-080B-4AB7-9344-6C34364C409E", Caption = "[8] Importer", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("A8022B55-F639-4BF6-B90A-0246E704BF83", Caption = "[UCC 3/9] Consignee", FullDescription = "The Consignee is the party to whom the goods are consigned / shipped in the third country.",  MultipleKey = MultipleKeyCdsExport)]
		[ResourceStringData("EF6C0B37-0DC7-47B8-8950-A54DD080857A", Caption = "[UCC 3/15] Importer", FullDescription = "Enter details of the Importer. Typically, this is the first buyer of the goods in the union, but please refer to HMRC guidance when declaring the Importer.", MultipleKey = MultipleKeyCdsImport)]
		[ResourceStringData("6BBFEE3C-835B-430C-A232-793DC5D8836D", Caption = "Importer / Consignee", FullDescription = "This is the party to whom the goods are consigned / shipped to.", MultipleKey = MultipleKeyCdsMisc)]
		public override JobDocAddress ImporterDocumentaryAddress
		{
			get => base.ImporterDocumentaryAddress;
		}

		[ResourceStringData("D65DCF70-750C-4918-8EEE-AD5C34A76B52", Caption = "[2] Supplier", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("2D775BAB-7163-4772-86E8-74C4F9FEBACA", Caption = "[UCC 3/1] Exporter", FullDescription = "Enter details of the Exporter. Typically, this is the entity who has the power to determine and has determined that the goods are to be taken out of that customs territory.", MultipleKey = MultipleKeyCdsExport)]
		[ResourceStringData("853C1C43-33C4-4491-BF43-C64E97C78DDB", Caption = "[UCC 3/1] Exporter", FullDescription = "Enter details of the Exporter. Typically, this is the entity that is the last seller of the goods prior to crossing the border.", MultipleKey = MultipleKeyCdsImport)]
		[ResourceStringData("24DF822E-3A4D-4D9F-A5D8-CAB60B5C753A", Caption = "[UCC 3/1] Exporter", FullDescription = "Enter details of the Exporter. Typically, this is the entity that is the last seller of the goods prior to crossing the border.", MultipleKey = MultipleKeyCdsMisc)]
		public override JobDocAddress SupplierDocumentaryAddress
		{
			get => base.SupplierDocumentaryAddress;
		}

		[ResourceStringData("52043029-F9E6-4B51-81C5-7847F35DF88D", Caption = "Office of Presentation", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("BC7156C6-A052-4B83-A015-4A17075EF8BB", Caption = "[UCC 5/26] Customs Office of Presentation")]
		public string JE_CustomsOfficeImportCaption => CaptionForProperty(nameof(JE_CustomsOfficeImportCaption));

		[ResourceStringData("E65FB264-067C-4524-ACF9-702BEA41CEF6", Caption = "[29] Office of Exit", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("6EC0FDC4-8901-4FE8-8EBE-F80AF75B8F32", Caption = "[UCC 5/12] Customs Office of Exit")]
		public string JE_CustomsOfficeExportCaption => CaptionForProperty(nameof(JE_CustomsOfficeExportCaption));

		[ResourceStringData("1E01868A-0476-483E-A5A9-A23098F75006", Caption = "[49] Customs Warehouse", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("95DA7447-6EE1-4313-BD32-52FBA186A93F", Caption = "[UCC 2/7] Customs Warehouse")]
		public override JobDocAddress WarehouseDocAddress
		{
			get => base.WarehouseDocAddress;
		}

		[ResourceStringData("39CC8D47-1DB4-487A-9576-EF35984FD7D4", Caption = "[44] Supervising Office", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("2CC14200-F97C-4CB6-89BB-2AD52607F9CA", Caption = "[UCC 5/27] Supervising Office")]
		public override JobDocAddress SupervisingOfficeDocAddress
		{
			get => base.SupervisingOfficeDocAddress;
		}

		[ResourceStringData("3842A93B-B809-4366-BB41-00A7FB5822A1", Caption = "Representative", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("9DA28F8D-C44C-4A48-93FD-6AB0002479CF", Caption = "[UCC 3/20] Representative")]
		public override ZGuid JE_OA_Representative
		{
			get => base.JE_OA_Representative;
			set => base.JE_OA_Representative = value;
		}

		[ResourceStringData("2199DA3D-D70C-40B1-AAB6-1ECE334B1462", Caption = "Seller", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("EC045EBA-2531-40A0-9BD6-B703486BA616", Caption = "[UCC 3/24] Seller")]
		public override ZGuid JE_OA_SellerAddress
		{
			get => base.JE_OA_SellerAddress;
			set => base.JE_OA_SellerAddress = value;
		}

		[ResourceStringData("DCA1B416-C422-4F22-B092-0A64636029A9", Caption = "Manufacturer", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("6B36C71E-42C2-4109-96C8-5C2287F60C15", Caption = "[UCC 3/37] Manufacturer")]
		public override ZGuid JE_OA_ManufacturerAddress
		{
			get => base.JE_OA_ManufacturerAddress;
			set => base.JE_OA_ManufacturerAddress = value;
		}

		[ResourceStringData("8AF70BFD-13C8-4C7F-A475-DE3F360922C8", Caption = "[14] Declarant", ShortCaption = "[14] Declarant", MediumCaption = "[14] Declarant", FullDescription = "[14] Declarant. Name of the declarant controlling this declaration.", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("3145800b-15b6-4c34-9bcc-46946e3d55e0", Caption = "[UCC 3/18] Declarant", ShortCaption = "[UCC 3/18] Declarant", MediumCaption = "[UCC 3/18] Declarant", FullDescription = "[UCC 3/18] Declarant. Name of the declarant controlling this declaration.")]
		public override ZGuid JE_OA_DeclarantAddress
		{
			get => base.JE_OA_DeclarantAddress;
			set => base.JE_OA_DeclarantAddress = value;
		}

		[ResourceStringData("B1F6F9EB-D90D-415F-8111-A8092BC300D3", Caption = "[UCC 3/7] Consignor")]
		public override ZGuid JE_OA_ShipperAddress
		{
			get => base.JE_OA_ShipperAddress;
			set => base.JE_OA_ShipperAddress = value;
		}

		[ResourceStringData("B6608847-4EBB-445B-B3CD-AA6AF97A7637", Caption = "Entry Type", FullDescription = "The Entry Type describes the flux/direction of the goods concerned.", MultipleKey = MultipleKeyCdsExport)]
		[ResourceStringData("A4F1E5D2-0C3B-4F7C-8A6D-9E5B0A1F2E8C", Caption = "Entry Type", FullDescription = "The Entry Type describes the flux/direction of the goods concerned.", MultipleKey = MultipleKeyCdsImport)]
		[ResourceStringData("F0FAFC70-8BEA-4A14-9F4A-A2B76EC40B31", Caption = "Entry Type", FullDescription = "The Entry Type describes the flux/direction of the goods concerned.", MultipleKey = MultipleKeyCdsMisc)]
		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set => base.JE_MessageType = value;
		}

		[ResourceStringData("BD368421-C973-4CE9-B49C-1157734669D3", Caption = "Service", FullDescription = "Select the Service Level for this Declaration.", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("5E6EAC0D-2BFD-4432-A701-44745859E7A8", Caption = "Service Level", FullDescription = "The Service Level for this declaration describes the quality/speed of service agreed to with the customer.")]
		public override ZString JE_RS_NKServiceLevel
		{
			get => base.JE_RS_NKServiceLevel;
			set => base.JE_RS_NKServiceLevel = value;
		}

		[ResourceStringData("BF9BEED7-963C-49C3-84F6-1EC52CD1BF8E", Caption = "[UCC 1/7] Circumstance", FullDescription = "[UCC 1/7] Specific Circumstance Indicator allows you to indicate if this is an Express Consignment.", MultipleKey = MultipleKeyCdsExport)]
		public override ZString ZG_SpecificCircumstanceIndicator
		{
			get => base.ZG_SpecificCircumstanceIndicator;
			set => base.ZG_SpecificCircumstanceIndicator = value;
		}

		[ResourceStringData("529C63A8-633A-4C37-A815-D2C2C1770321", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("47C95080-5BF3-4B9A-B4A2-7C3B5D08B73F", Caption = "Earliest Customs Entry Issue Date", FullDescription = "This box shows the issue date for this declaration. i.e. when the MRN was assigned by customs.")]
		public override ZDateTime EarliestCustomsEntryIssueDate
		{
			get => base.EarliestCustomsEntryIssueDate;
		}

		[ResourceStringData("77B2DEA5-CE6F-43E2-A07C-8B9726F25FF1", Caption = "Master Bill", FullDescription = "Master Bill of the consignment.")]
		[ResourceStringData("E6C29C2C-9F2A-44CC-91D4-09E023EE0D42", ShortCaption = "Master Bill", Caption = "Master Bill of Lading", FullDescription = "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.", MultipleKey = MultipleKeyCdsExport , IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("C9F9F9FC-8551-4C61-B344-E7591C12C41E", ShortCaption = "Master Bill", Caption = "Master Bill of Lading", FullDescription = "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.", MultipleKey = MultipleKeyCdsImport , IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("FF114104-4E40-4D3B-AE2A-518AD4D2C2DF", ShortCaption = "Master Bill", Caption = "Master Bill of Lading", FullDescription = "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.", MultipleKey = MultipleKeyCdsMisc , IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("8B12E484-AEAA-49C1-86FC-D9B94171A489", ShortCaption = "Ocean Bill", Caption = "Master Bill of Lading", FullDescription = "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.", MultipleKey = MultipleKeyCdsExport , IsApplicableMember = nameof(IsSea))]
		[ResourceStringData("3AD4B362-FFFE-40C1-A142-FC63C9A722E5", ShortCaption = "Ocean Bill", Caption = "Master Bill of Lading", FullDescription = "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.", MultipleKey = MultipleKeyCdsImport , IsApplicableMember = nameof(IsSea))]
		[ResourceStringData("CB96FA2B-F5ED-4488-91E6-0D03759B2FD4", ShortCaption = "Ocean Bill", Caption = "Master Bill of Lading", FullDescription = "The Master Bill of Lading is usually issued by the carrier. It serves as a contract of carriage between shipper and carrier.", MultipleKey = MultipleKeyCdsMisc , IsApplicableMember = nameof(IsSea))]
		public override ZString JE_MasterBill
		{
			get => base.JE_MasterBill;
			set => base.JE_MasterBill = value;
		}

		[ResourceStringData("610B687E-3EE1-4D43-B571-8333D8FEDD0C", Caption = "Discharge Port", ShortCaption = "Discharge", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("06862E56-77A8-4C87-A5B9-99D089A5708D", Caption = "Discharge Port", ShortCaption = "Discharge", FullDescription = "Please enter the UNLOCO for the Destination port where the goods will arrive into.")]
		public override ZString JE_RL_NKPortOfArrival
		{
			get { return base.JE_RL_NKPortOfArrival; }
			set { base.JE_RL_NKPortOfArrival = value; }
		}

		[ResourceStringData("F3CEFB36-F7DF-4C63-B8F7-4F840ECC42CD", Caption = "[15] Country/Region of Dispatch",ShortCaption = "[15] Dispatch",  FullDescription = "[15] Country/Region of Dispatch of the goods", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("C6B5DCD8-B680-43D2-81EA-7887881B444E", Caption = "[UCC 5/14] Country of Dispatch/Export Code", FullDescription = "This field shows the country code for where the goods are shipping from.")]
		public override ZString JE_GoodsOrigin
		{
			get { return base.JE_GoodsOrigin; }
			set { base.JE_GoodsOrigin = value; }
		}

		[ResourceStringData("F3CEFB36-F7DF-4C63-B8F7-4F840ECC42CD", Caption = "Estimated Departure Date At Origin Port", ShortCaption = "ETD", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("F3CEFB36-F7DF-4C63-B8F7-4F840ECC42CD", Caption = "Estimated Departure Date from Origin Port", FullDescription = "Estimated Departure Date from Origin Port")]
		public override ZDateTime JE_DateAtOrigin
		{
			get { return base.JE_DateAtOrigin; }
			set { base.JE_DateAtOrigin = value; }
		}

		[ResourceStringData("ADF9D985-D3EC-40F3-9FD3-48823A09EDDB", Caption = "Dep.", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("DA0C40A2-F12A-43B8-B210-D244A4555403", Caption = "Departure Date", ShortCaption = "Dep.", FullDescription = "The Departure Date is the date the goods are due to leave the Load Port.")]
		public override ZDateTime JE_ExportDate
		{
			get => base.JE_ExportDate;
			set => base.JE_ExportDate = value;
		}

		[ResourceStringData("241D8792-066C-4AC7-973A-F0BCCA89C56A", Caption = "Date of Arrival", ShortCaption = "Arr.", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("5F555A16-4B9B-42CB-BB6D-B2BDF0FBE8E5", Caption = "Arrival Date", ShortCaption = "Arr.", FullDescription = "The Arrival Date is the date the goods are due to arrive at the Discharge Port.")]
		public override ZDateTime JE_DateOfArrival
		{
			get => base.JE_DateOfArrival;
			set => base.JE_DateOfArrival = value;
		}

		[ResourceStringData("CA8CC595-C4BA-4798-9BA2-ADB4B9880193", Caption = "House Bill", ShortCaption = "House", FullDescription = "House Bill of shipment", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("921EFEF4-AA72-4FF2-B9B9-5B576F972BC8", Caption = "House Bill of Lading", ShortCaption = "House Bill", FullDescription = "The House Bill of Lading is usually issued by the Freight Forwarder. It serves as a contract of carriage between them and the shipper or consignee.")]
		public override ZString JE_HouseBill
		{
			get => base.JE_HouseBill;
			set => base.JE_HouseBill = value;
		}

		[ResourceStringData("337F588B-3647-4E75-A77D-B74E95B756AC", Caption = "Goods Destination", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("CE40D1A1-30D0-44F8-86E7-AAA32095F5AD", Caption = "[UCC 5/8] Country of Destination Code", FullDescription = "This field shows the final destination country code where the goods will arrive.")]
		public override ZString JE_GoodsDestination
		{
			get => base.JE_GoodsDestination;
			set => base.JE_GoodsDestination = value;
		}

		[ResourceStringData("ED108E27-E533-4AFC-80E3-F3AA2E2D5EA7", Caption = "Estimated Time / Date of Arrival Date at the Final Destination Port", ShortCaption = "ETA", FullDescription = "Estimated Time / Date of Arrival at the Final Destination Port.", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("06E8F53B-38A8-4E03-A2BB-73BE5626FA7F", Caption = "Estimated Date of Arrival at the Final Destination Port")]
		public override ZDateTime JE_DateAtFinalDestination
		{
			get => base.JE_DateAtFinalDestination;
			set => base.JE_DateAtFinalDestination = value;
		}

		[ResourceStringData("AAE9FE27-905C-473A-8485-4E300F447D8B", Caption = "Sub-Location for [UCC 5/23] Location of Goods.", ShortCaption = "Sub.", FullDescription = "Sub-Location for [UCC 5/23] Location of Goods. For CCS-UK jobs, this will usually be the airport and shed code.\nThis will allow customs to determine where a control will take place if required.")]
		public override ZString JE_SubLocationOfGoods
		{
			get => base.JE_SubLocationOfGoods;
			set => base.JE_SubLocationOfGoods = value;
		}

		[ResourceStringData("B6E9B0B8-6643-4156-BA98-3F0EFEAF8082", Caption = "Total Weight", ShortCaption = "Weight", FullDescription = "Enter the total weight of this shipment", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("F188BE47-3710-466D-A9A8-07BCD3166041", Caption = "Total Weight and Unit", ShortCaption = "Total Weight", FullDescription = "Please enter the total weight and unit of measurement.")]
		public override ZDecimal JE_TotalWeight
		{
			get => base.JE_TotalWeight;
			set => base.JE_TotalWeight = value;
		}

		[ResourceStringData("919820F1-9CBF-4088-B996-798D98C68505", Caption = "Total Volume", ShortCaption = "Vol", MediumCaption = "Volume", FullDescription = "Enter the total volume of this shipment", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("B1F3C242-5F88-4681-AB05-E35D63F3AA0D", Caption = "Total Volume and Unit", ShortCaption = "Total Volume", FullDescription = "Please enter the total volume and unit of measurement.")]
		public override ZDecimal JE_TotalVolume
		{
			get => base.JE_TotalVolume;
			set => base.JE_TotalVolume = value;
		}

		[ResourceStringData("D9777516-31E1-48E9-A88D-192B1DC9860D", Caption = "Incoterm Place Code", MediumCaption = "Inco. Place Code", ShortCaption = "Inco. Place Code", FullDescription = "Incoterm Place Code: insert a Country (2 chars) or an UNLOCO (5 chars)", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("819237FB-9C38-468D-89F8-7CFEDC6554CC", Caption = "Agreed Place Code", FullDescription = "This field confirms the status of the goods when the responsibility is transferred from the buyer to the seller according to the Incoterm. i.e. whether the goods will remain in the country of departure, reach another union country or leave the union.")]
		public override ZString ZG_AgreedPlaceCode
		{
			get => base.ZG_AgreedPlaceCode;
			set => base.ZG_AgreedPlaceCode = value;
		}

		[ResourceStringData("249AF279-A052-4817-9C5F-6E249BE73802", Caption = "Agents Reference", ShortCaption = "Agents Ref.", MultipleKey = MultipleKeyChief)]
		[ResourceStringData(".123652F4-E11D-4619-94A1-9EBE7C6A86A5", Caption = "Agent's Reference", ShortCaption = "Agent's Ref.", FullDescription = "Please enter the Agent's own reference for this consignment.")]
		public override ZString JE_AgentsReference
		{
			get => base.JE_AgentsReference;
			set => base.JE_AgentsReference = value;
		}

		[ResourceStringData("A88D2792-C8B3-4450-8D1B-8799AC2BBF64", Caption = "DUCR", FullDescription = "Declaration UCR", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("265C47DF-085D-4A9E-ACDA-51241D721F25", Caption = "[UCC 2/4] DUCR (Declaration Unique Consignment Reference)", ShortCaption = "[UCC 2/4] DUCR", FullDescription = "The DUCR (Declaration Unique Consignment Reference) is a unique identifier that customs use to track a consignment at different stages of a shipment.")]
		public override ZString JE_UCR
		{
			get => base.JE_UCR;
			set => base.JE_UCR = value;
		}

		[ResourceStringData("BB82263A-398A-4349-9D24-A506014F4EBD", Caption = "Screening Status", ShortCaption = "Scrn.", MediumCaption = "Screening", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("DF63D7C4-F273-4FE9-AAD7-76F82C46D75E", Caption = "Screening Status", ShortCaption = "Scrn.", MediumCaption = "Screening", FullDescription = "CargoWise DPS (Denied Party Screening) Service can be used to prevent the submission of Standalone Customs declarations if any organizations used on the job have an Unknown or Matched status.")]
		public override ZString JE_ScreeningStatus
		{
			get => base.JE_ScreeningStatus;
			set => base.JE_ScreeningStatus = value;
		}

		[ResourceStringData("1EE136AD-CD57-4FBB-99D1-404510244B49", Caption = "[44] Supporting Documents", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("D7FF7490-38C1-4499-858A-C80CB2F16914", Caption = "[UCC 2/3 && 8/7] Supporting Documents")]
		public string SupportingDocumentsCaption => CaptionForProperty(nameof(SupportingDocumentsCaption));

		[ResourceStringData("DDE4776F-533C-42B5-A8BE-EBDEBCF494E0", Caption = "[40] Previous Documents", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("2A5F78DF-64C2-4A80-92A4-3755D9B1838E", Caption = "[UCC 2/1] Previous Documents")]
		public string PreviousDocumentsCaption => CaptionForProperty(nameof(PreviousDocumentsCaption));

		[ResourceStringData("0B38F9F8-A63A-48B8-BB3D-901161E2D7F3", Caption = "[44] Additional Info", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("47B8137F-3FB2-4A38-B109-17E0F9AC1776", Caption = "[UCC 2/2] Additional Info")]
		public string AdditionalInfoCaption => CaptionForProperty(nameof(AdditionalInfoCaption));

		[ResourceStringData("84DE3212-2020-4627-984E-D872471C99D0", Caption = "[52] Guarantees", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("B6694A2D-A173-4AE8-B2EC-6D6DEE570763", Caption = "[UCC 8/3] Guarantees")]
		public string GuaranteesCaption => CaptionForProperty(nameof(GuaranteesCaption));

		[ResourceStringData("174210EE-3C37-4A8A-A095-2395131E5942", Caption = "[47] Taxes", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("62BB8627-A3FB-4088-95A8-C372F093E76C", Caption = "Taxes")]
		public string TaxGroupCaption => CaptionForProperty(nameof(TaxGroupCaption));

		[ResourceStringData("9522874B-E6A6-4528-B6C9-F7300FABF73C", Caption = "Value Indicators", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("F84D766C-1DCD-4AEF-8106-F1A062E63041", Caption = "[UCC 4/13] Value Indicators")]
		public string ValueIndicatorsCaption => CaptionForProperty(nameof(ValueIndicatorsCaption));

		[ResourceStringData("31B0986E-DED1-4F77-B223-EEFB4DD4A014", Caption = "[31] Packages", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("9006DEC3-471B-4AFB-9B80-2A629D1636C9", Caption = "[UCC 6/10] Packages")]
		public string InvoiceLinePackagesPivotTabCaption => CaptionForProperty(nameof(InvoiceLinePackagesPivotTabCaption));

		[ResourceStringData("ED09A5AD-3CEF-4EBC-810B-3E2247D48B0F", Caption = "[47] Tax", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("C5CB2806-4780-4861-B314-325F09CFF403", Caption = "Tax")]
		public string InvoiceLineTaxTabCaption => CaptionForProperty(nameof(InvoiceLineTaxTabCaption));

		[ResourceStringData("4B6BB764-CACF-4680-BE8F-32C5AE416AC1", Caption = "/", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("1D93E74D-5A87-4C46-8D89-F80A2D3AAB4D", Caption = "[UCC 6/16 && 6/17] Additional Codes")]
		public string InvoiceLineAdditionalCodesCaption => CaptionForProperty(nameof(InvoiceLineAdditionalCodesCaption));

		[ResourceStringData("B9C5DAED-CE3A-4D4F-AAF5-539076F74DA9", Caption = "/", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("2AAB5244-2671-41E0-8583-27E072E7F43C", Caption = " ")]
		public string InvoiceLineBlankCaption => CaptionForProperty(nameof(InvoiceLineBlankCaption));

		public static ZBool IsWritingOffQuantityForbiddenForH4 => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.H4ForbidWritingOff, CountryCodes.UnitedKingdom, ZDateTime.Now);

		public string GetDDPCalculationResults()
		{
			var dDPCalculationResults = string.Empty;
			_ = new DutyCalculatorStrategy(this);
			var dDPResult = DdpCalculator.Calculate(this);
			if (dDPResult != null)
			{
				if (dDPResult.LinesMissingCommodityPreferenceOriginOrPrice.Count == 0 && dDPResult.LinesNotAttractingSimpleAdValoremDuty.Count == 0 && dDPResult.LinesAlreadyHave9WKSWorksheet.Count == 0)
				{
					dDPCalculationResults = $"The calculator successfully processed {InvoiceLines.Count} invoice lines.";
				}
				else
				{
					var sb = new StringBuilder();
					_ = sb.Append("The calculator skipped one or more invoice lines due to the following incompatible data or state. Please adjust those manually.").Append("\n");
					AddDDPCalculationResultLines(sb, dDPResult.LinesMissingCommodityPreferenceOriginOrPrice, "Lines lacking commodity, preference, origin or price:");
					AddDDPCalculationResultLines(sb, dDPResult.LinesNotAttractingSimpleAdValoremDuty, "Lines not attracting simple ad valorem duty:");
					AddDDPCalculationResultLines(sb, dDPResult.LinesAlreadyHave9WKSWorksheet, "Lines which already have a 9WKS worksheet:");
					dDPCalculationResults = sb.ToString();
				}
			}
			else
			{
				dDPCalculationResults = "This calculator only works for invoices of INCO term DDP and having an invoice currency, of which there are none.";
			}
			return dDPCalculationResults;
		}

		void AddDDPCalculationResultLines(StringBuilder sb, IReadOnlyList<JobComInvoiceLine> lines, ZString description)
		{
			if (lines.Count > 0)
			{
				var results = lines.GroupBy(
					p => p.InvoiceHeader.JZ_InvoiceNumber,
					p => p.JI_LineNo,
					(key, g) => new { Invoice = key, Lines = string.Join(", ", g.ToList()) });

				_ = sb.Append(description).Append("\n");

				foreach (var invoice in results)
				{
					_ = sb.Append("\t").Append($"Invoice {invoice.Invoice}, line(s) {invoice.Lines}").Append("\n");
				}
			}
		}

		[ResourceStringData("BDC0C87F-F891-45C6-961E-518E4C34C3F2", Caption = "Gateway", MultipleKey = MultipleKeyChief)]
		[ResourceStringData("04A99362-A423-4A0D-AE32-0006BA801028", Caption = "Gateway", FullDescription = "This field shows which Gateway or CSP (Community System Provider) will receive this declaration.")]
		public override ZString ZG_Gateway
		{
			get => base.ZG_Gateway;
			set => base.ZG_Gateway = value;
		}
	}
}
