using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.ServiceTask;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.FR.Business.NCTS
{
	[VisualizableDocumentsSupportable("FR.NctsHeaderVisualizableDocumentSupporter")]
	public class NctsHeader : EU.NCTS.Business.NctsHeader, Integration.Customs.FR.ICusInBondHeader, IFRMessagesOwner, ICorrelationIDProvider
	{
		public NctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.NCTS.Business.NctsHeader.Schema
		{
			public const string DetailedDepartureStatusCode = "DetailedDepartureStatusCode";
			public const string DetailedArrivalStatusCode = "DetailedArrivalStatusCode";
			public const int DetailedDepartureStatusCodeMaxLength = 3;
			public const string IsPrelodgedMovement = "IsPrelodgedMovement";
			public const string IsQueried = "IsQueried";
			public const string IsQueryAvailableOnPaper = "IsQueryAvailableOnPaper";
			public const string TC11Date = nameof(NctsHeader.TC11Date);
			public const string IsTC11DeliveredByCustoms = "IsTC11DeliveredByCustoms";
			public const string QueryInformation = nameof(NctsHeader.QueryInformation);
			public const int QueryInformationMaxLength = 140;
			public const string DepartureCustomsOfficeDate = "DepartureCustomsOfficeDate";
			public const string DeltaT = "DeltaT";
			public const string CorrelationID = "CorrelationID";
			public const int CorrelationMaxLength = 10;
		}

		#region Synchronisers

		protected override BusinessObjectSynchroniser GetNewNctsPhase4ShipmentSynchroniser(ICusInBondParent parent) => new NctsShipmentSynchroniser(this, parent);

		protected override BusinessObjectSynchroniser GetNewNctsPhase4ConsolSynchroniser(ICusInBondParent parent) => new NctsConsolSynchroniser(this, parent);

		public override bool ShouldTransportDetailsSyncDependsOnTransportMode => false;

		#endregion

		#region DetailedDepartureStatusCode

		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.DetailedStatusCodeList))]
		[MaxLength(Schema.DetailedDepartureStatusCodeMaxLength)]
		[ReadOnly(true)]
		public ZString DetailedDepartureStatusCode
		{
			get => FRNctsHeader.CFN_DetailedDepartureStatusCode;
			set => FRNctsHeader.CFN_DetailedDepartureStatusCode = value;
		}
		public ZWrappedPropertyInfo DetailedDepartureStatusCodeInfo => GetWrappedZPropertyInfo(Schema.DetailedDepartureStatusCode, x => FRNctsHeader.CFN_DetailedDepartureStatusCodeInfo);

		[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.DetailedStatusCodeList))]
		[MaxLength(Schema.DetailedDepartureStatusCodeMaxLength)]
		[ReadOnly(true)]
		public ZString DetailedArrivalStatusCode
		{
			get => FRNctsHeader.CFN_DetailedArrivalStatusCode;
			set => FRNctsHeader.CFN_DetailedArrivalStatusCode = value;
		}
		public ZWrappedPropertyInfo DetailedArrivalStatusCodeInfo => GetWrappedZPropertyInfo(Schema.DetailedArrivalStatusCode, x => FRNctsHeader.CFN_DetailedArrivalStatusCodeInfo);
		#endregion

		#region IsPrelodgedMovement

		public ZBool IsPrelodgedMovement
		{
			get => FRNctsHeader.CFN_IsPrelodgedMovement;
			set
			{
				FRNctsHeader.CFN_IsPrelodgedMovement = value;
			}
		}
		public ZWrappedPropertyInfo IsPrelodgedMovementInfo => GetWrappedZPropertyInfo(Schema.IsPrelodgedMovement, x => FRNctsHeader.CFN_IsPrelodgedMovementInfo);

		#endregion

		#region IsQueried
		[ReadOnlyMember(nameof(IsQueriedReadOnly))]
		public ZBool IsQueried
		{
			get => FRNctsHeader.CFN_IsQueried;
			set => FRNctsHeader.CFN_IsQueried = value;
		}
		public ZWrappedPropertyInfo IsQueriedInfo => GetWrappedZPropertyInfo(Schema.IsQueried, x => FRNctsHeader.CFN_IsQueriedInfo);
		protected bool IsQueriedReadOnly => !GlbStaff.CurrentUser.IsSupportUser;

		#endregion

		#region IsQueryAvailableOnPaper

		public ZBool IsQueryAvailableOnPaper
		{
			get => FRNctsHeader.CFN_IsQueryAvailableOnPaper;
			set => FRNctsHeader.CFN_IsQueryAvailableOnPaper = value;
		}
		public ZWrappedPropertyInfo IsQueryAvailableOnPaperInfo => GetWrappedZPropertyInfo(Schema.IsQueryAvailableOnPaper, x => FRNctsHeader.CFN_IsQueryAvailableOnPaperInfo);

		#endregion

		#region IsTC11DeliveredByCustoms

		public ZBool IsTC11DeliveredByCustoms
		{
			get => FRNctsHeader.CFN_IsTC11DeliveredByCustoms;
			set => FRNctsHeader.CFN_IsTC11DeliveredByCustoms = value;
		}
		public ZWrappedPropertyInfo IsTC11DeliveredByCustomsInfo => GetWrappedZPropertyInfo(Schema.IsTC11DeliveredByCustoms, x => FRNctsHeader.CFN_IsTC11DeliveredByCustomsInfo);

		#endregion

		#region QueryInformation

		[MaxLength(Schema.QueryInformationMaxLength)]
		public ZString QueryInformation
		{
			get => FRNctsHeader.CFN_QueryInformation;
			set => FRNctsHeader.CFN_QueryInformation = value;
		}

		public ZPropertyInfo QueryInformationInfo => GetWrappedZPropertyInfo(Schema.QueryInformation, x => FRNctsHeader.CFN_QueryInformationInfo);

		#endregion

		#region TC11Date

		public ZDateTime TC11Date
		{
			get => FRNctsHeader.CFN_TC11Date;
			set => FRNctsHeader.CFN_TC11Date = value;
		}

		public ZPropertyInfo TC11DateInfo => GetWrappedZPropertyInfo(Schema.TC11Date, x => FRNctsHeader.CFN_TC11DateInfo);

		#endregion

		#region LeafBizObject implementation

		public override void Delete()
		{
			FRNctsHeader.Delete();
			base.Delete();
		}
		public CusFRNctsHeader FRNctsHeader
		{
			get
			{
				if (frNctsHeader == null || frNctsHeader.IsDeleted)
				{
					frNctsHeader = Factory.LoadTop1<CusFRNctsHeader>(new ZQuery(CusFRNctsHeaderSchema.CFN_BH, SQLComparisonOperator.Equal, PK));
					if (frNctsHeader == null)
					{
						frNctsHeader = Factory.New<CusFRNctsHeader>();
						frNctsHeader.CFN_BH = PK;
					}
					RegisterEditableChildObject(frNctsHeader);
					RegisterListChangedCalledRefreshBinding(frNctsHeader);
				}
				return frNctsHeader;
			}
		}

		CusFRNctsHeader frNctsHeader;

		#endregion

		#region Declarant

		public override JobDocAddress GetDeclarantCore()
		{
			if (declarantJobDocAddress == null || declarantJobDocAddress.IsDeleted)
			{
				if (declarantJobDocAddress != null)
				{
					declarantJobDocAddress.DocAddressChanged -= DeclarantJobDocAddressChanged;
				}

				declarantJobDocAddress = DocAddresses.FindOrCreateWithRequirement(DeclarantJobDocAddressRequirement);
				declarantJobDocAddress.DocAddressChanged += DeclarantJobDocAddressChanged;
			}

			return declarantJobDocAddress;
		}

		JobDocAddress declarantJobDocAddress;

		public JobDocAddressRequirement DeclarantJobDocAddressRequirement
		{
			get
			{
				if (declarantJobDocAddressRequirement == null)
				{
					declarantJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Declarant, ContactType.NotifyParty);
					declarantJobDocAddressRequirement.ValidateOrganisationPK = ValidateDeclarant;
					declarantJobDocAddressRequirement.CanOverride = true;
					JobDocAddressManager.AddRequirement(declarantJobDocAddressRequirement);
				}
				return declarantJobDocAddressRequirement;
			}
		}

		JobDocAddressRequirement declarantJobDocAddressRequirement;

		protected void ValidateDeclarant(JobDocAddressValidation validation)
		{
			this.CheckDeclarantIsValid(Declarant.OrganisationPKInfo);
			this.CheckNoOrMultipleGuarantees(Declarant.OrganisationPKInfo);
		}

		protected override JobDocAddressRequirement GetAdditionalDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.Declarant:
					return DeclarantJobDocAddressRequirement;
				default:
					return base.GetAdditionalDocAddressRequirement(addressType);
			}
		}

		void DeclarantJobDocAddressChanged(object sender, EventArgs e)
		{
			ChangeValueInBMLocationOfGoodsCode();
			DefaultDepartureCustomsOfficeIfApplicable();
			UpdateDestinationOffice(sender, e);
			GuaranteeRefresher.PopulateGuaranteeWithFallbacks();
		}

		void ChangeValueInBMLocationOfGoodsCode()
		{
			MovementHeader?.ChangeValueInBMLocationOfGoodsCode();
		}

		#endregion

		#region CorrelationID

		[ReadOnly(true)]
		[MaxLength(Schema.CorrelationMaxLength)]
		public ZString CorrelationID
		{
			get { return CorrelationIDEntryNumber.CE_EntryNum; }
			set { CorrelationIDEntryNumber.CE_EntryNum = value; }
		}

		public ZPropertyInfo CorrelationIDInfo { get { return GetWrappedZPropertyInfo(Schema.CorrelationID, x => CorrelationIDEntryNumber.CE_EntryNumInfo); } }

		public CusEntryNumber CorrelationIDEntryNumber
		{
			get
			{
				if (correlationIDEntryNumber == null)
				{
					correlationIDEntryNumber = new CachedProperty<CusEntryNumber>(Factory, delegate
					{
						var correlationEntryNumberInternal = CusEntryNumber.Load(this, CusEntryNumberTypes.EU.CorrelationIdentifier, CountryCode);
						if (correlationEntryNumberInternal == null)
						{
							correlationEntryNumberInternal = CusEntryNumber.New(this, CusEntryNumberTypes.EU.CorrelationIdentifier, CountryCode);
							correlationEntryNumberInternal.CE_EntryIsSystemGenerated = true;
							correlationEntryNumberInternal.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
						}
						return correlationEntryNumberInternal;
					});
				}
				return correlationIDEntryNumber.Value;
			}
		}
		CachedProperty<CusEntryNumber> correlationIDEntryNumber;

		public CorrelationIDGenerator CorrelationIdGenerator => correlationIdGenerator ?? (correlationIdGenerator = new CorrelationIDGenerator(this, this));
		CorrelationIDGenerator correlationIdGenerator;

		bool IsUniqueCorrelationID(ZString correlationID)
		{
			return NumberGeneratorHelper.IsUniqueEntryNumber(Factory, TableName, correlationID, CusEntryNumberTypes.EU.CorrelationIdentifier, CountryCode);
		}

		public ZString CorrelationIDPrefix => ZString.Empty;

		#endregion

		#region Bills

		public new INctsBillCollection<NctsBill> Bills => (INctsBillCollection<NctsBill>)base.Bills;

		protected override INctsBillCollection<EU.NCTS.Business.NctsBill> GetNewBillCollection()
			=> new NctsBillCollection<NctsBill>(this);

		protected override Type BillTypeCore => typeof(NctsBill);

		#endregion

		public new CusGoodsLocation CusGoodsLocation => (CusGoodsLocation)base.CusGoodsLocation;

		public override void OnSaved(bool saveSucceeded)
		{
			CorrelationIdGenerator.ClearCorrelationIDOnSaved(saveSucceeded);
			base.OnSaved(saveSucceeded);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			CorrelationIdGenerator.InitCorrelationID(IsUniqueCorrelationID);
		}

		protected override void OnChangedConsignorDocumentaryAddress()
		{
			base.OnChangedConsignorDocumentaryAddress();
			ChangeValueInBMLocationOfGoodsCode();
		}

		public override ZString BH_HeaderType
		{
			get => base.BH_HeaderType;
			set
			{
				if (BH_HeaderType != value)
				{
					base.BH_HeaderType = value;
					if (!IsMarkingAsNeedingValidationSuspended)
					{
						FRNctsHeader.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZBool BH_FTZMove
		{
			get => base.BH_FTZMove;
			set
			{
				base.BH_FTZMove = value;
				if (!IsMarkingAsNeedingValidationSuspended)
				{
					FRNctsHeader.MarkAsNeedingValidation();
				}
			}
		}

		protected override bool LocalReferenceNumberReadOnlyCore => base.LocalReferenceNumberReadOnlyCore && (MovementHeader?.BM_CustomsStatus ?? ZString.Empty) != NctsTransitStatusList.Codes.DeclarationRejected;

		public EDIMessage GetOutgoingMessage(EDIMessage inboundMessage)
		{
			var messages = IsPhase4 ? Messages : MovementHeader.Messages;
			var matchingOutgoingMessage = messages.Count > 0 ? messages.Cast<EDIMessage>().FirstOrDefault(m => m.EM_InterchangeNumber == inboundMessage.EM_InterchangeNumber.Split('.').First() && m.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit) : null;
			return matchingOutgoingMessage ?? messages.LastOutgoingMessage;
		}

		protected override NctsGuaranteeRefresher GetNewGuaranteeRefresher() => new FRNctsGuaranteeRefresher(this);

		public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

		public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;

		public new NctsHeaderLookups Lookups => (NctsHeaderLookups)base.Lookups;

		protected override CusInBondHeaderLookups GetNewLookups() => new NctsHeaderLookups(this);

		protected override CusInBondHeaderValidation GetNewPhase4Validation() => new NctsHeaderValidation(this);

		protected override CustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new NctsHeaderCustomsOfficeRequirementHelper(this);

		protected override EU.NCTS.Business.NctsHeaderDocumentSupporter GetNewDocumentSupporter() => new NctsHeaderDocumentSupporter(this);

		public new ICusAuthorizationUsageCollection<NctsCusAuthorizationUsage, NctsHeader> CusAuthorizationUsages => (EU.NCTS.Business.CusAuthorizationUsageCollection<NctsCusAuthorizationUsage, NctsHeader>)base.CusAuthorizationUsages;

		protected override ICusAuthorizationUsageCollection<EU.NCTS.Business.CusAuthorizationUsage, EU.NCTS.Business.NctsHeader> GetCusAuthorizationUsages()
			=> new EU.NCTS.Business.CusAuthorizationUsageCollection<NctsCusAuthorizationUsage, NctsHeader>(this);

		protected override ZInt MaximumGuaranteeCountCore => 1;

		public new INctsDepartureHeaderContainerCollection<FRNctsDepartureHeaderContainer, NctsHeader> DepartureHeaderContainers
			=> (INctsDepartureHeaderContainerCollection<FRNctsDepartureHeaderContainer, NctsHeader>)base.DepartureHeaderContainers;

		protected override INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, EU.NCTS.Business.NctsHeader> GetDepartureHeaderContainersCore()
			=> new NctsDepartureHeaderContainerCollection<FRNctsDepartureHeaderContainer, NctsHeader>(this);

		[ChildEditable(false)]
		public new NctsFrOfficeCodeCollection CustomsOffices => (NctsFrOfficeCodeCollection)base.CustomsOffices;

		protected override NctsEuOfficeCodeCollection GetNewCustomsOffices() => new NctsFrOfficeCodeCollection(this);

		protected override void UpdateCustomsStatusForAmendment()
		{
			MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.ReadyForAmendment;
		}

		protected override void UpdateLocalReferenceNumberForAmendment()
		{
		}

		public ZDateTime DepartureCustomsOfficeDate
		{
			get
			{
				var departureOffice = DepartureCustomsOffice;
				return departureOffice == null ? ZDateTime.Empty : departureOffice.ArrivalTime;
			}
		}

		public ZPropertyInfo DepartureCustomsOfficeDateInfo
		{
			get
			{
				if (DepartureCustomsOffice is NctsEuOfficeCode office)
				{
					return GetWrappedZPropertyInfo(Schema.DepartureCustomsOfficeDate, x => office.CY_DateInfo);
				}
				return GetZPropertyInfo(Schema.DepartureCustomsOfficeDate);
			}
		}

		protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			var result = base.GetCusCodeDataTypesCore();
			result[EU.Business.CusCodeDataTypeList.Codes.OfficeCode] = typeof(NctsFrOfficeCode);
			return result;
		}

		protected override EU.NCTS.Business.ModeOfRepresentationCalculator GetModeOfRepresentationCalculator() => new ModeOfRepresentationCalculator(this);

		public bool HasDepartureReachedGoodsReleasedForTransitStatus
		{
			get
			{
				return Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code).AddToFilter(StmALogSchema.SL_Reference, NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture)).Any();
			}
		}

		protected override bool IsDepartureTabEditableCountrySpecificRules(ZString messageStatus, ZString departureStatus) => IsQueried || DetailedDepartureStatusCode == NctsDetailedStatusList.Codes.ResearchProcedureNotification || messageStatus == EDIMessageStatusList.Codes.Rejected;

		public override bool StopFromSendingMessageWithMandatoryError => false;

		public ZBool DeltaTFallbackAnnounced => IsRefCusCodeListCombinedExistInDataBase(Schema.DeltaT, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, Core.Constants.CountryCodes.France, ZDateTime.Today, ZDateTime.Today);

		ZBool IsRefCusCodeListCombinedExistInDataBase(ZString deltaCode, ZString code, ZString countryCode, ZDateTime startDate, ZDateTime endDate)
		{
			var query = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_CountryOrGrouping, countryCode);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_CodeType, code);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_Code, deltaCode);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, startDate);
			query.AddToFilter(ZZRefCusCodeListCombinedSchema.ZZD_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, endDate);
			return Factory.Exists(typeof(Universal.ZZRefCusCodeListCombined), query);
		}

		public ZBool DeltaTFallbackAnnouncedButNotActive => DeltaTFallbackAnnounced && !FallBackIsActive;

		public override ZBool FallBackIsActive => Registry.FRCustomsDataRegistry.DeltaTFallbackIsActive;

		protected override bool AdditionalClausesForWhenDepartureAmendmentIsAllowed
		{
			get
			{
				return (MovementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.DeclarationAccepted && DetailedDepartureStatusCode == NctsDetailedStatusList.Codes.Anticipated)
					|| DetailedDepartureStatusCode == NctsDetailedStatusList.Codes.AmendmentRefused
					|| DetailedDepartureStatusCode == NctsDetailedStatusList.Codes.AmendmentAccepted
					|| MovementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.DeclarationMrnAllocated;
			}
		}

		protected override ZString GetFallbackInformationCore()
		{
			var result = ZString.Empty;
			if (Registry.FRCustomsDataRegistry.DeltaTFallbackIsActive && MovementHeader != null)
			{
				result = $@"PLAN DE CONTINUITÉ DES OPÉRATIONS
TRANSIT DE L’UNION/TRANSIT COMMUN
AUCUNE DONNÉE DISPONIBLE DANS LE 
SYSTÈME
ENGAGÉE LE {ZDateTime.Now.ToISO8601ShortDateString()}/{ZDateTime.Now.ToShortTimeString()}";
			}
			return result;
		}

		public new INctsGuaranteeCollection<FRNctsGuarantee> Guarantees => (INctsGuaranteeCollection<FRNctsGuarantee>)base.Guarantees;

		protected override INctsGuaranteeCollection<NctsGuarantee> GetGuaranteesForNonPhase5Departure() => new NctsGuaranteeCollection<FRNctsGuarantee>(this);

		protected override NctsIE29CusdecParser GetNctsIE29CusdecParser()
		{
			return new ServiceTasks.NctsIE29CusdecParser();
		}

		[ChildEditable(true)]
		FREDIMessageCollection IFRMessagesOwner.Messages => (FREDIMessageCollection)base.Messages;

		protected override EDIMessageCollection GetNewMessageCollection()
		{
			return new FREDIMessageCollection(this);
		}

		protected override IProcessor CreateNCTSMessageProcessorCore()
		{
			if (IsPhase5)
			{
				return new AutoSendNCTSP5MessageProcessor(this);
			}
			else
			{
				return new FRSendNCTSMessageProcessor(this);
			}
		}

		protected override IProcessor CreateNCTSArrivalNotificationMessageProcessorCore()
		{
			if (IsPhase5)
			{
				return new AutoSendNCTSP5MessageProcessor(this);
			}
			else
			{
				return new FRSendNCTSArrivalNotificationMessageProcessor(this);
			}
		}

		protected override bool IsDepartureTabReadOnlyCountrySpecificRules(ZString messageStatus, ZString departureStatus) => departureStatus == NctsTransitStatusList.Codes.GoodsWrittenOff;

		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ?? (valueSetStrategy = IsPhase5 ? base.GetValueSetStrategy() : new NctsHeaderPhase4ValueSetStrategy(this));
		IValueSetStrategy valueSetStrategy;

		protected override void ValidateConsignor(JobDocAddressValidation validation)
		{
			base.ValidateConsignor(validation);

			if (IsDepartureMovement)
			{
				var isConsignorFilled = !Consignor.OrganisationPK.IsEmpty;
				var isAnyGoodsItemConsignorFilled = MovementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().Any(item => !item.Consignor.OrganisationPK.IsEmpty);

				if (isConsignorFilled && isAnyGoodsItemConsignorFilled)
				{
					Consignor.OrganisationPKInfo.AddWarning(Res.GetString("a487d9a1-a1d0-46e4-bedb-f45a2782b7e2", "You can't fill in the Consignor in both Departure Declaration and Goods Items tab. Please note that only the Consignor in Departure Declaration tab will be sent to customs."));
				}
				else if (!isConsignorFilled && !isAnyGoodsItemConsignorFilled)
				{
					Consignor.OrganisationPKInfo.AddWarning(Res.GetString("6961b660-71f5-4bab-af28-4c9e688a01ac", "Please specify the Consignor, either in Departure Declaration or Goods Items tab."));
				}
			}
		}

		protected override void ValidateConsignee(JobDocAddressValidation validation)
		{
			base.ValidateConsignee(validation);

			if (IsDepartureMovement)
			{
				var isConsigneeFilled = !Consignee.OrganisationPK.IsEmpty;
				var isAnyGoodsItemConsigneeFilled = MovementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().Any(item => !item.Consignee.OrganisationPK.IsEmpty);

				if (isConsigneeFilled && isAnyGoodsItemConsigneeFilled)
				{
					Consignee.OrganisationPKInfo.AddWarning(Res.GetString("a998f0bc-3836-4093-8d1e-e19dbad5ccf9", "You can't fill in the Consignee in both Departure Declaration and Goods Items tab. Please note that only the Consignee in Departure Declaration tab will be sent to customs."));
				}
				else if (!isConsigneeFilled && !isAnyGoodsItemConsigneeFilled)
				{
					Consignee.OrganisationPKInfo.AddWarning(Res.GetString("67a4456b-bc81-4e78-a8d0-1c7080c6b4f1", "Please specify the Consignee, either in Departure Declaration or Goods Items tab."));
				}
			}
		}

		protected override Type DepartureContainerTypeCore => typeof(FRNctsDepartureHeaderContainer);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Filter strings")]
		public ZString GetLastFRMEventErrorDescription() => Factory.GetCached(ref lastFRMEventErrorDescription, () =>
		{
			var result = ZString.Empty;

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.FrenchCustomsMessageStatus.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, new ZString[]
			{
				CustomsMessageStatusList.Codes.MessageNotAcknowledged,
				CustomsMessageStatusList.Codes.MessageNotDeliveredToCustoms,
				CustomsMessageStatusList.Codes.MessageRejected
			});

			var lastFRMEventWithErrorDescriptionLog = Logs.Find(query).OrderByDescending(x => x.SL_EventTime).FirstOrDefault();
			if (lastFRMEventWithErrorDescriptionLog != null)
			{
				result = lastFRMEventWithErrorDescriptionLog.SourceInfoItems.FirstOrDefault(x => x.Key == "Error")?.Data ?? ZString.Empty;
			}

			return result;
		});

		CachedProperty<ZString> lastFRMEventErrorDescription;

		protected override bool IsDepartureAmendmentAllowedCore() => MovementHeader.BM_CustomsStatus != NctsTransitStatusList.Codes.DeclarationCancelled && base.IsDepartureAmendmentAllowedCore();

		protected override bool IsDepartureCancellationAllowedCore()
		{
			return MovementHeader != null && MovementHeader.IsDepartureCancellationAllowed && !FRCustomsDataRegistry.DeltaTFallbackIsActive;
		}

		protected override bool IsArrivalTabReadOnlyCore => base.IsArrivalTabReadOnlyCore || DetailedArrivalStatusCode == NctsDetailedStatusList.Codes.PreArrivalNotificationRequest;

		protected override void DefaultDepartureCustomsOfficeIfApplicable()
		{
			if (IsPhase5 || !(MovementHeader?.IsSimplifiedNctsProcedure ?? true))
			{
				base.DefaultDepartureCustomsOfficeIfApplicable();
			}
			else
			{
				var consignorAcrAuthorisationOfcRuleValue = ConsignorACRAuthorisationOFCRuleValue;
				if (!consignorAcrAuthorisationOfcRuleValue.IsEmpty)
				{
					SetOfficeData(DepartureCustomsOffice ?? AddNewCustomsOfficeCodeForDeparture(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture), consignorAcrAuthorisationOfcRuleValue);
				}
				if (DepartureCustomsOfficeCode.IsEmpty)
				{
					var consignorCTR = Consignor?.Organisation?.CustomsCodes?.GetCustomsRegNo(CodeTypes.CustomsOfficeForTransit, GlbCompany.CurrentCompany.Country.Code, Consignor.E2_OA_Address) ?? ZString.Empty;
					if (!consignorCTR.IsEmpty)
					{
						SetOfficeData(DepartureCustomsOffice ?? AddNewCustomsOfficeCodeForDeparture(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture), consignorCTR);
					}
				}
			}
		}

		public ZString ConsignorACRAuthorisationOFCRuleValue
		{
			get
			{
				var consignorAddress = Consignor.E2_OA_Address;
				var declarantOrganisationPK = Declarant.OrganisationPK;
				return Factory.GetCachedValue($"{Declarant.OrganisationPK}{Consignor.E2_OA_Address}", () =>
				{
					var result = ZString.Empty;

					if (!declarantOrganisationPK.IsEmpty)
					{
						var authorisations = CusAuthorisationHeader.Loader.GetAuthorisationsForAddressesAndPermitHolder(Factory, CountryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit }, ZDate.Today, new[] { declarantOrganisationPK }, new[] { consignorAddress });
						if (authorisations != null && authorisations.Any())
						{
							result = authorisations.FirstOrDefault(x => x.CusAuthorisationRules.Any(rule => rule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.OFC))?.CusAuthorisationRules.FirstOrDefault(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.OFC)?.CPR_ValueFrom ?? ZString.Empty;
						}
					}

					return result;
				});
			}
		}

		protected override void UpdateDestinationOffice(object sender, EventArgs e)
		{
			if (IsPhase5 || !(ArrivalMovementHeader?.IsSimplifiedNctsProcedure ?? true))
			{
				base.UpdateDestinationOffice(sender, e);
			}
			else if (IsArrivalMovement)
			{
				var ruleValue = DestinationACEAuthorisationOFCRuleValue;
				if (!ruleValue.IsEmpty)
				{
					DestinationCustomsOfficeCodeForArrival = ruleValue;
				}
			}
		}

		public ZString DestinationACEAuthorisationOFCRuleValue
		{
			get
			{
				var destinationAddress = DestinationTrader.E2_OA_Address;
				var declarantOrganisationPK = Declarant.OrganisationPK;
				return Factory.GetCachedValue($"DestinationACEAuthorisationOFCRuleValue_{declarantOrganisationPK}_{destinationAddress}", () =>
				{
					var result = ZString.Empty;

					if (!declarantOrganisationPK.IsEmpty)
					{
						var authorisations = CusAuthorisationHeader.Loader.GetAuthorisationsForAddressesAndPermitHolder(Factory, CountryCode, new ZString[] { CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit }, ZDate.Today, new[] { declarantOrganisationPK }, new[] { destinationAddress });
						result = authorisations?.FirstOrDefault(x => x.CusAuthorisationRules.Any(rule => rule.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.OFC))?.CusAuthorisationRules.FirstOrDefault(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.OFC)?.CPR_ValueFrom ?? ZString.Empty;
					}
					return result;
				});
			}
		}
	}
}
