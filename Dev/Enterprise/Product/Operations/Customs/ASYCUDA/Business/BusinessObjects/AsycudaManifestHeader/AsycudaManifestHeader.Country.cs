using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using MessageSubTypeCodes = Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists.MessageSubTypeCodes;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public abstract partial class AsycudaManifestHeader : IInterchangeSenderIdProvider
		, IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider
		, IStatusSupporter
		, IMessageParent
	{
		public new partial class Schema
		{
			public const string MessageStatus = "MessageStatus";

			public const string RegistrationDate = "RegistrationDate";
			public const string RegistrationNumber = "RegistrationNumber";
			public const string RegistrationStatus = "RegistrationStatus";

			public const string SpecificCircumstanceIndicator = "SpecificCircumstanceIndicator";
			public const string MethodOfPayment = "MethodOfPayment";
			public const string SpecialMentions = "SpecialMentions";
			public const string ETAatFirstCustomsOffice = "ETAatFirstCustomsOffice";
			public const string ATAatFirstCustomsOffice = "ATAatFirstCustomsOffice";
		}

		public const string StatusCodeMultiple = "MULTIPLE";

		public virtual bool HasContainers => true;

		public virtual bool HasBillsAndPacks => true;

		ZString IInterchangeSenderIdProvider.SenderID
		{
			get
			{
				return ApplicationBusinessProvider.MessagingProvider.GetInterchangeSenderID(Factory);
			}
		}

		internal string MessageSenderCode => ((IInterchangeSenderIdProvider)this).SenderID;

		public string SetNewCountryMessagingStatus(ZString newStatus)
		{
			var oldStatusForRollback = "";
			oldStatusForRollback = AMA_MessageStatus;
			AMA_MessageStatus = newStatus;

			return oldStatusForRollback;
		}

		public string SetNewCountryCustomsStatus(ZString newStatus)
		{
			var oldStatusForRollback = "";
			oldStatusForRollback = RegistrationStatus;
			RegistrationStatus = newStatus;
			return oldStatusForRollback;
		}

		public string GetXmlFileName()
		{
			const string extension = ".xml";
			return "AsycudaManifest_" + AMA_RN_NKCountry + "_" + AMA_JobReference + "_" + ZDateTime.Now.ToString("yyyy-MM-dd_HHmmss", CultureInfo.CurrentCulture) + extension;
		}

		internal void DefaultManifestTypeOnceAttachToManifestAndCountryIsSet()
		{
			if (!AMA_RN_NKCountry.IsEmpty)
			{
				var manifestTypes = Lookups.ManifestTypes;
				if (manifestTypes.Count > 0 && !manifestTypes.ContainsCode(AMA_ManifestType))
				{
					AMA_ManifestType = new ZString(manifestTypes[0].Code).Left(Schema.AMA_ManifestTypeMaxLength);
				}
			}
		}

		public ZString CountryName => NameFromCode(AMA_RN_NKCountry);

		protected virtual ZString NameFromCode(ZString countryCode)
		{
			RefCountry c = null;
			if (!countryCode.IsEmpty)
			{
				c = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
			}
			return c?.RN_DescMultilingual ?? ZString.Empty;
		}

		public ZString CalculateCustomsOfficeCode(ZString localPortCode, ZString transportMode)
		{
			// There will be a single attribute with mode=PORT for one office regardless of specific mode, the specific transport mode will be saved in RefCusCodeOrAttributeTransportMode.
			var result = ZString.Empty;
			var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, AMA_RN_NKCountry);
			var codes = Factory.Load<ZZRefCusCodeListCombined>(query);
			var codesForThisMode = codes.Where(c => c.TransportModes.IsNullOrEmpty() || c.TransportModes.Any(b => b == transportMode)).ToArray();  // ZZE_ZXE_NKName cannot be blank
			if (codesForThisMode.Length == 1)
			{
				// e.g. Sri Lanka, offices divided only by mode
				var zzd = codesForThisMode.First();
				if (!localPortCode.IsEmpty)
				{
					var zze = zzd.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().FirstOrDefault(a => a.ZZE_Value.EqualsIgnoringCase(localPortCode) || a.ZZE_Value.IsEmpty);
					if (zze != null)
					{
						result = zzd.ZZD_Code;
					}
				}
				else
				{
					result = zzd.ZZD_Code;
				}
			}
			else if (codesForThisMode.Length > 1)
			{
				// e.g. FJ and SB, offices are divided by various combiations of port and mode
				var codesForThisModeAndPort = codesForThisMode.Where(c => c.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Any(a => a.ZZE_Value.EqualsIgnoringCase(localPortCode))).ToArray();
				if (codesForThisModeAndPort.Length == 1)
				{
					result = codesForThisModeAndPort.First().ZZD_Code;
				}
			}
			else
			{
				// e.g. PNG, offices are based (we assume) only by port
				var codesForThisPortRegardlessOfMode = codes.Where(c => c.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Any(a => a.ZZE_Value.EqualsIgnoringCase(localPortCode))).ToArray();
				if (codesForThisPortRegardlessOfMode.Any())
				{
					// Now check that the mode is not a mismatch - just because we have an exact match based on port, make sure that the mode does not preclude it
					var zzd = codesForThisPortRegardlessOfMode.FirstOrDefault(c => c.TransportModes.IsNullOrEmpty() || c.TransportModes.Any(b => b == transportMode));
					if (zzd != null)
					{
						result = zzd.ZZD_Code;
					}
				}
			}
			return result.Left(AsycudaManifestHeader.Schema.AMA_CustomsOfficeMaxLength);
		}

		public override bool CanDelete => base.CanDelete && !HasManifestBeenSubmittedToCustoms;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get => HasManifestBeenSubmittedToCustoms
					? ResString.GetMultilingualString("0ADDC024-3CE4-4290-9287-173BB1FCD64D", "Cannot delete the record once a message has been sent.")
					: base.ReasonForNotAbleToDelete;
		}

		[BusinessObjectTestExclude]
		[LightValidationTestExempt]
		public override ZString AMA_RN_NKCountry
		{
			get => base.AMA_RN_NKCountry;
			set
			{
				var oldValue = AMA_RN_NKCountry;
				base.AMA_RN_NKCountry = value;
				if (oldValue != value)
				{
					validationHeaderHelper = null;
					messageSendingNotificationHelper = null;
					OnNatureOrCountryChanged?.Invoke(this, null);
					manifestType = null;
					var masterBill = MasterBill; // Ensure that a master bill is created
					DefaultManifestTypeOnceAttachToManifestAndCountryIsSet();

					if (!IsCopying)
					{
						Containers.MarkAsNeedingValidation();
						masterBill?.MarkAsNeedingValidation();
						MasterBOLInfo.RefreshBinding();
					}

					foreach (var bill in Bills)
					{
						if (!IsCopying)
						{
							bill.MarkAsNeedingValidation();
							bill.CustomsEntryNumbers.MarkAsNeedingValidation();
						}

						if (!bill.IsChildMasterBill)
						{
							bill.DefaultOnCountryChanged();
						}
					}

					CheckBusinessObjectType(oldValue, AMA_ManifestType, value, AMA_ManifestType);
				}
			}
		}

		public DisposableAction GetCheckBusinessObjectTypeSuspender() => new DisposableAction(SuspendCheckBusinessObjectType, () => shouldCheckBusinessObjectType = true);

		bool shouldCheckBusinessObjectType = true;

		public void SuspendCheckBusinessObjectType()
		{
			shouldCheckBusinessObjectType = false;
		}

		internal void CheckBusinessObjectType(ZString oldCountry, ZString oldManifestType, ZString newCountry, ZString newManifestType)
		{
			if (shouldCheckBusinessObjectType)
			{
				var newHeaderType = AsycudaManifestHeader.TypeDecider.GetGlobalManifestType(Factory, AMA_RN_NKCountry, AMA_ManifestType, AMA_ApplicationCode);
				var currentHeaderType = GetType();
				if (newHeaderType != currentHeaderType)
				{
					ReportChangingBusinessObjectType(oldCountry, oldManifestType, newCountry, newManifestType, newHeaderType, currentHeaderType);
				}
			}
		}

		void ReportChangingBusinessObjectType(ZString currentCountry, ZString currentManifestType, ZString newCountry, ZString newManifestType, Type newType, Type curentType)
		{
			ErrorReporter.ReportOnce(
				string.Format(CultureInfo.InvariantCulture, "Changing GlobalManifestApplicationKey from '{0}{1}' to '{2}{3}' results in a different type '{4}' for business object '{5}'.",
					currentCountry,
					currentManifestType,
					newCountry,
					newManifestType,
					newType,
					curentType));
		}

		public virtual ZBool SupportAssociatedPacks => ZBool.False;

		public virtual ZBool SupportMultipleCustomsNumbers => ZBool.False;

		public virtual ZBool SupportUNDGsOnPackedItemLevel => ZBool.False;

		public IManifestType ManifestType => manifestType ?? (manifestType = GetManifestType());
		IManifestType manifestType;

		internal void ResetManifestType()
		{
			manifestType = null;
		}

		IManifestType GetManifestType() => ApplicationBusinessProvider?.ManifestTypes.FirstOrDefault(x => x.Code == AMA_ManifestType);

		#region Registration date/number/status
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		[ReadOnlyMember(nameof(RegistrationDetails_ReadOnly))]
		[ResourceStringData("AsycudaManifestHeader.RegistrationNumber", Caption = "Registration Number", ShortCaption = "Reg. Number")]
		public virtual ZString RegistrationNumber
		{
			get => RegistrationEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var cusEntryNumber = RegistrationEntryNumber;
				var oldValue = cusEntryNumber?.CE_EntryNum ?? ZString.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						// use cached variable for performance
						registrationEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry);
						if (!registrationEntryNumber.IsInDatabase)
						{
							registrationEntryNumber.CE_EntryStatus = AsycudaRegistrationStatuses.Codes.Registered;
							RegistrationStatusInfo.RefreshBinding(ZString.Empty);
						}
						RegisterEditableChildObject(registrationEntryNumber);
					}
					registrationEntryNumber.CE_EntryNum = value;
					if (registrationEntryNumber.CE_IssueDate.IsEmpty)
					{
						registrationEntryNumber.CE_IssueDate = ZDateTime.Now;
						RegistrationDateInfo.RefreshBinding(ZDateTime.Empty);
					}
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_EntryNum = value;
					}
				}
				RegistrationNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo RegistrationNumberInfo => GetZPropertyInfo(Schema.RegistrationNumber);

		protected virtual bool RegistrationDetails_ReadOnly => true;

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.RegistrationStatusList))]
		[MaxLength(3)]
		[ReadOnlyMember(nameof(RegistrationDetails_ReadOnly))]
		[ResourceStringData("AsycudaManifestHeader.RegistrationStatus", Caption = "Customs Status", ShortCaption = "Cus. Status")]
		public virtual ZString RegistrationStatus
		{
			get
			{
				var headerEntryStatus = RegistrationEntryNumber?.CE_EntryStatus ?? ZString.Empty;
				return (headerEntryStatus.IsEmpty && IsBillLevelManifestType) ? CombineBillStatuses(country => country.ABL_BillStatus, StatusCodeMultiple) : headerEntryStatus;
			}

			set
			{
				var cusEntryNumber = RegistrationEntryNumber;
				var oldValue = cusEntryNumber?.CE_EntryStatus ?? ZString.Empty;
				if (oldValue != value)
				{
					if (!value.IsEmpty)
					{
						if (cusEntryNumber == null)
						{
							// use cached variable for performance
							registrationEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry);
							RegisterEditableChildObject(registrationEntryNumber);
						}
						registrationEntryNumber.CE_EntryStatus = value;
					}
					else
					{
						if (cusEntryNumber != null)
						{
							cusEntryNumber.CE_EntryStatus = value;
						}
					}

					((IStatusSupporter)this).LogEventsOnParent(Events.CustomsManifestStatus, value);
					RegistrationStatusInfo.RefreshBinding(oldValue);
					if (!IsValidationSuspended)
					{
						Validation.ValidateRegistrationStatus();
					}
				}
			}
		}

		public ZPropertyInfo RegistrationStatusInfo => GetZPropertyInfo(Schema.RegistrationStatus);

		[ReadOnlyMember(nameof(RegistrationDetails_ReadOnly))]
		[ResourceStringData("AsycudaManifestHeader.RegistrationDate", Caption = "Registration Date", ShortCaption = "Date")]
		public virtual ZDateTime RegistrationDate
		{
			get => RegistrationEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				var cusEntryNumber = RegistrationEntryNumber;
				var oldValue = cusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				if (!value.IsEmpty)
				{
					if (cusEntryNumber == null)
					{
						// use cached variable for performance
						registrationEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry);
						if (!registrationEntryNumber.IsInDatabase)
						{
							registrationEntryNumber.CE_EntryStatus = AsycudaRegistrationStatuses.Codes.Registered;
						}
						RegisterEditableChildObject(registrationEntryNumber);
					}

					registrationEntryNumber.CE_IssueDate = value;
				}
				else
				{
					if (cusEntryNumber != null)
					{
						cusEntryNumber.CE_IssueDate = value;
					}
				}

				RegistrationDateInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRegistrationDate();
				}
			}
		}
		public ZPropertyInfo RegistrationDateInfo => GetZPropertyInfo(Schema.RegistrationDate);

		[ReadOnlyMember(nameof(RegistrationDetails_ReadOnly))]
		[ResourceStringData("AsycudaManifestHeader.RegistrationYear", Caption = "Registration Year", ShortCaption = "Year")]
		[BusinessObjectTestExclude]
		public virtual ZInt RegistrationYear
		{
			get => RegistrationDate.IsEmpty ? ZInt.Zero : RegistrationDate.Year;
			set
			{
				if (value == RegistrationYear)
				{
					return;
				}

				var year = value;
				year = year < ZDateTime.MinSmallDateTimeValue.Year ? ZDateTime.MinSmallDateTimeValue.Year : year;
				year = year > ZDateTime.MaxSmallDateTimeValue.Year ? ZDateTime.MaxSmallDateTimeValue.Year : year;
				RegistrationDate = new ZDateTime(year, 1, 1);
			}
		}
		public ZPropertyInfo RegistrationYearInfo => GetZPropertyInfo(nameof(RegistrationYear));

		[ReadOnly(true)]
		public CusEntryNumber RegistrationEntryNumber
		{
			get
			{
				if (registrationEntryNumber == null || registrationEntryNumber.IsDeleted || registrationEntryNumber.CE_EntryType != CusEntryNumberTypes.ASYCUDA.AsycudaRegistration)
				{
					registrationEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, AMA_RN_NKCountry);
					if (registrationEntryNumber != null)
					{
						RegisterEditableChildObject(registrationEntryNumber);
					}
				}
				return registrationEntryNumber;
			}
		}
		public CusEntryNumber registrationEntryNumber;

		#endregion

		void DefaultOfficeCodeIfNeed()
		{
			if (!AMA_RN_NKCountry.IsEmpty && AMA_CustomsOffice.IsEmpty)
			{
				var port = ZString.Empty;

				if (AMA_RL_NKPortOfLoading.StartsWith(AMA_RN_NKCountry, StringComparison.OrdinalIgnoreCase))
				{
					port = AMA_RL_NKPortOfLoading;
				}
				else if (AMA_RL_NKPortOfDischarge.StartsWith(AMA_RN_NKCountry, StringComparison.OrdinalIgnoreCase))
				{
					port = AMA_RL_NKPortOfDischarge;
				}

				var newOfficeCode = CalculateCustomsOfficeCode(port, AMA_TransportMode);
				AMA_CustomsOffice = newOfficeCode.Left(AsycudaManifestHeader.Schema.AMA_CustomsOfficeMaxLength);
			}
		}

		void DefaultNature()
		{
			if ((FeatureProvider?.AllowDefaultingOfNature ?? true))
			{
				DefaultNatureFromManifestType();

				if (AMA_Nature.IsEmpty)
				{
					if (IsStandAlone)
					{
						DefaultNatureFromPorts();
					}
					else
					{
						DefaultNatureFromConsol();
					}
				}
			}
		}

		[ResourceStringData("AsycudaManifestHeader.LloydsNumber", Caption = "Vessel IMO Number")]
		public override ZString AMA_LloydsNumber { get => base.AMA_LloydsNumber; set => base.AMA_LloydsNumber = value; }

		#region SpecificCircumstanceIndicator

		[MaxLength(1)]
		[ResourceStringData("AsycudaManifestHeader.SpecificCircumstanceIndicator", Caption = "Specific Circumstance Indicator")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.SpecificCircumstanceList))]
		[ReadOnlyMember(nameof(SpecificCircumstanceIndicator_ReadOnly))]
		public virtual ZString SpecificCircumstanceIndicator
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.SpecificCircumstanceIndicator);
			set
			{
				var oldValue = SpecificCircumstanceIndicator;
				CheckMaximumLength(SpecificCircumstanceIndicatorInfo, value);
				this.SetSystemDefinedValue(Schema.SpecificCircumstanceIndicator, value);
				OnSpecificCircumstanceIndicatorChanged?.Invoke(this, null);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSpecificCircumstanceIndicator();
				}
				SpecificCircumstanceIndicatorInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SpecificCircumstanceIndicatorInfo => GetZPropertyInfo(Schema.SpecificCircumstanceIndicator);

		protected virtual bool SpecificCircumstanceIndicator_ReadOnly => !IsPartOfEuropeanUnion;

		public ZBool SpecificCircumstanceIndicatorVisible => IsPartOfEuropeanUnion;

		#endregion

		#region MethodOfPayment

		[MaxLength(1)]
		[ResourceStringData("AsycudaManifestHeader.MethodOfPayment", Caption = "Method of Payment", FullDescription = "Transport Charges - Method of Payment")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MethodOfPaymentList))]
		[ReadOnlyMember(nameof(MethodOfPayment_ReadOnly))]
		public ZString MethodOfPayment
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.MethodOfPayment);
			set
			{
				var oldValue = MethodOfPayment;
				CheckMaximumLength(MethodOfPaymentInfo, value);
				this.SetSystemDefinedValue(Schema.MethodOfPayment, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMethodOfPayment();
				}
				MethodOfPaymentInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo MethodOfPaymentInfo => GetZPropertyInfo(Schema.MethodOfPayment);

		protected virtual bool MethodOfPayment_ReadOnly => !IsPartOfEuropeanUnion;

		public ZBool MethodOfPaymentVisible => IsPartOfEuropeanUnion;

		#endregion

		#region SpecialMentions

		[MaxLength(5)]
		[ResourceStringData("AsycudaManifestHeader.SpecialMentions", Caption = "Special Mentions")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.SpecialMentionsList))]
		[ReadOnlyMember(nameof(SpecialMentions_ReadOnly))]
		public ZString SpecialMentions
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.SpecialMentions);
			set
			{
				var oldValue = SpecialMentions;
				CheckMaximumLength(SpecialMentionsInfo, value);
				this.SetSystemDefinedValue(Schema.SpecialMentions, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSpecialMentions();
				}
				SpecialMentionsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SpecialMentionsInfo
		{
			get { return GetZPropertyInfo(nameof(SpecialMentions)); }
		}

		protected virtual bool SpecialMentions_ReadOnly => !IsPartOfEuropeanUnion;

		public ZBool SpecialMentionsVisible => IsPartOfEuropeanUnion;

		#endregion

		#region ETAatFirstCustomsOffice

		[ResourceStringData("AsycudaManifestHeader.ETAatFirstCustomsOffice", Caption = "ETA at First Customs Office")]
		[ReadOnlyMember(nameof(ETAatFirstCustomsOffice_ReadOnly))]
		public ZDateTime ETAatFirstCustomsOffice
		{
			get
			{
				return this.GetSystemDefinedValue<ZDateTime>(Schema.ETAatFirstCustomsOffice);
			}
			set
			{
				var oldValue = ETAatFirstCustomsOffice;
				this.SetSystemDefinedValue(Schema.ETAatFirstCustomsOffice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateETAatFirstCustomsOffice();
				}
				ETAatFirstCustomsOfficeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ETAatFirstCustomsOfficeInfo => GetZPropertyInfo(nameof(ETAatFirstCustomsOffice));

		protected virtual bool ETAatFirstCustomsOffice_ReadOnly => !IsPartOfEuropeanUnion;

		public ZBool ETAatFirstCustomsOfficeVisible => IsPartOfEuropeanUnion;

		#endregion

		#region ATAatFirstCustomsOffice

		[ResourceStringData("AsycudaManifestHeader.ATAatFirstCustomsOffice", Caption = "ATA at First Customs Office")]
		[ReadOnlyMember(nameof(ATAatFirstCustomsOffice_ReadOnly))]
		public ZDateTime ATAatFirstCustomsOffice
		{
			get
			{
				return this.GetSystemDefinedValue<ZDateTime>(Schema.ATAatFirstCustomsOffice);
			}
			set
			{
				var oldValue = ATAatFirstCustomsOffice;
				this.SetSystemDefinedValue(Schema.ATAatFirstCustomsOffice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateATAatFirstCustomsOffice();
				}
				ATAatFirstCustomsOfficeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ATAatFirstCustomsOfficeInfo
		{
			get { return GetZPropertyInfo(nameof(ATAatFirstCustomsOffice)); }
		}

		protected virtual bool ATAatFirstCustomsOffice_ReadOnly => !IsPartOfEuropeanUnion;

		public ZBool ATAatFirstCustomsOfficeVisible => IsPartOfEuropeanUnion;

		#endregion

		[ReadOnly(true)]
		public ZDateTime DateAtCustomsOffice => AMA_DateAtCustomsOffice;

		public ZBool IsPartOfEuropeanUnion => ParentDataGrouping != null && ParentDataGrouping.ZZZ_DataGrouping == Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

		public RefDataGrouping ParentDataGrouping => RefDataGrouping.GetParentDataGrouping(Factory, AMA_RN_NKCountry);

		public virtual bool LockedBills => false;

		public virtual bool IsDeclarationCreationEnabled => false;
		public virtual bool IsRoutingEnabled => true;

		ZString IMessageParent.ManifestType => AMA_ManifestType;

		ISelectionItem IMessageParent.SelectionItem => null;

		ZString IMessageParent.MessageStatus => AMA_MessageStatus;

		ZString IMessageParent.CustomsStatus => RegistrationStatus;

		void IMessageParent.ResetMessageStatus()
		{
			AMA_MessageStatus = ZString.Empty;
		}
		bool IMessageParent.HasCustomsNumbers => HasCustomsNumbers;
		protected virtual bool HasCustomsNumbers => !RegistrationNumber.IsEmpty;

		public virtual ZString CustomsSystem => ZString.Empty;

		public string MessageFunctionSubTypeForSend => MessageSubTypeCodes.Codes.Original;

		public string MessageFunctionSubTypeForCancel => MessageSubTypeCodes.Codes.Cancellation;

		public string MessageFunctionSubTypeForAmend => MessageSubTypeCodes.Codes.Change;

		[ChildEditable(true)]
		EDIMessageCollection MessagesHeaderOnly
		{
			get
			{
				if (ediMessagesHeaderOnly == null)
				{
					ediMessagesHeaderOnly = new EDIMessageCollection(this, Factory);
					ediMessagesHeaderOnly.Load();
					ediMessagesHeaderOnly.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(ediMessagesHeaderOnly);
				}
				return ediMessagesHeaderOnly;
			}
		}
		EDIMessageCollection ediMessagesHeaderOnly;

		public virtual bool ShowPackedItems => IsManyPackedItemRelationship || IsOnePackedItemRelationship;

		public bool IsBillLevelManifestType => ManifestType?.IsBillMessageLevel() ?? false;

		public bool IsPackedItemLevelManifestType => ManifestType?.IsPackMessageLevel() ?? false;

		public ZString DataGrouping => GetDataGroupingCore();

		protected virtual ZString GetDataGroupingCore() => AMA_RN_NKCountry;

		public bool IsCustomsCleared => AsycudaUniversalReference.CustomsStatusAttributeHelper.IsCustomsCleared(Factory, DataGrouping, RegistrationStatus);

		public bool HasManifestBeenSubmittedToCustoms => MessageStatusProvider?.HasManifestBeenSubmittedToCustoms(this) ?? false;

		public ApplicationBusinessProvider ApplicationBusinessProvider => GetApplicationBusinessProvider();

		protected virtual ApplicationBusinessProvider GetApplicationBusinessProvider() => ApplicationBusinessProvider.GetApplicationBusinessProvider(this);

		public MessagingProvider MessagingProvider => ApplicationBusinessProvider?.MessagingProvider;

		public MessageStatusProvider MessageStatusProvider => MessagingProvider?.MessageStatusProvider;

		[ResourceStringData("AsycudaManifestHeader.RegistrationStatusCodeAndDescription", Caption = "Customs Status", ShortCaption = "Cus. Status")]
		public ZString RegistrationStatusCodeAndDescription
		{
			get
			{
				var code = RegistrationStatus;

				if (code.IsEmpty)
				{
					return ZString.Empty;
				}

				var description = RegistrationStatusDescription;
				return code != "MULTIPLE" ? $"{code} - {description}" : description;
			}
		}

		[ResourceStringData("AsycudaManifestHeader.RegistrationStatusDescription", Caption = "Customs Status Description", ShortCaption = "Cus. Status Desc.")]
		public ZString RegistrationStatusDescription
		{
			get
			{
				var headerEntryStatus = RegistrationEntryNumber?.CE_EntryStatus ?? ZString.Empty;
				return (headerEntryStatus.IsEmpty && IsBillLevelManifestType) ? CombineBillStatuses(country => country.ABL_BillStatusDescription, "MULTIPLE Registration Statuses") : (ZString)Lookups.RegistrationStatusList.GetDescriptionFromCode(RegistrationStatus);
			}
		}

		public ZString CombineBillStatuses(Func<AsycudaBill, ZString> getter, ZString multipleStatusesText)
		{
			if (getter == null)
			{
				return ZString.Empty;
			}
			var statuses = Bills.OfType<AsycudaBill>().Select(getter).Distinct().ToArray();
			switch (statuses.Length)
			{
				case 0:
					return ZString.Empty;
				case 1:
					return statuses[0];
				default:
					if (statuses.Any(x => x.IsEmpty))
					{
						return ZString.Empty;
					}
					else
					{
						return multipleStatusesText;
					}
			}
		}

		#region IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider

		EDIMessageCollection IEDIMessageCollectionProvider.Messages => MessagesHeaderOnly;

		EDIFACTMessageStatusCalculator IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider.GetCalculator(string country)
		{
			return ApplicationBusinessProvider.MessagingProvider.GetEDIFACTStatusCalculator();
		}

		void IEDIFACTMessageAttachee.AddMessage(EDIMessage message)
		{
			MessagesHeaderOnly.Add(message);
			Messages.Add(message);
		}

		[ResourceStringData("AsycudaManifestHeaderCountry.MessageStatus", Caption = "Message Status")]
		public virtual ZString MessageStatus => AMA_MessageStatus;

		public ZPropertyInfo MessageStatusInfo => GetZPropertyInfo(Schema.MessageStatus);

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get => MessageStatus;
			set => AMA_MessageStatus = value;
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get => RegistrationStatus;
			set => RegistrationStatus = value;
		}

		ZString IEDIFACTMessageAttachee.JobIdentification => ((IJobNumber)this).JobNumber;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => Consol ?? (BusinessObject)this;

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		#endregion

		public (ZString CountryOrGrouping, ZString ManfestTypeCode, ZString ApplicationCode) GetApplicationProviderKey() => (AMA_RN_NKCountry, AMA_ManifestType, AMA_ApplicationCode);

		#region IStatusSupporter

		ZString IStatusSupporter.ManifestPermitNumber
		{
			set => RegistrationNumber = value;
		}
		ZString IStatusSupporter.CustomsStatus
		{
			get => RegistrationStatus;
			set => RegistrationStatus = value;
		}
		ZString IStatusSupporter.MessageStatus
		{
			set => AMA_MessageStatus = value;
		}

		ZBool IStatusSupporter.SupportsPackLevelMessages => IsPackedItemLevelManifestType;

		ZString IStatusSupporter.CustomsEntryNumber
		{
			set => ErrorReporter.ReportOnce(ZString.Format("AsycudaManifestHeader do not have CustomsEntryNumbers collection, so value '{0}' won't be stored in header level."), value);
		}

		void IStatusSupporter.LogEventsOnParent(Event eventType, ZString reference)
		{
			Logs.AddNew(eventType, reference, ZDateTimeOffset.Now);
		}

		#endregion

		public BaseMessageSendingNotificationHelper MessageSendingNotificationHelper => messageSendingNotificationHelper ?? (messageSendingNotificationHelper = GetMessageSendingNotificationHelper());
		BaseMessageSendingNotificationHelper messageSendingNotificationHelper;

		public virtual BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			if (IsTrueAsycudaCountry)
			{
				return new AsycudaMessageSendingNotificationHelper(this);
			}
			else
			{
				return new MessageSendingNotificationHelper(this);
			}
		}
	}
}
