using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using HtmlAgilityPack;
using static Enterprise.Customs.ES.Business.ESConstants;
using CertificateHelper = Enterprise.Customs.ES.Business.CertificateHelper;
using GlbExternalPassword = Enterprise.Customs.ES.Business.GlbExternalPassword;
using NctsMessageStatusList = Enterprise.Customs.EU.NCTS.Business.NctsMessageStatusList;
using NctsUnloadedStateList = Enterprise.Customs.EU.NCTS.Business.NctsUnloadedStateList;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsHeader : EU.NCTS.Business.NctsHeader, Integration.Customs.ES.ICusInBondHeader, IESMessageInfoProvider, IESResponseBOMessageStatus, INctsCusStorageDocPivotParent, IPollingTransactionParent
{
	public NctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.NCTS.Business.NctsHeader.Schema
	{
		public const string NationalSimplificatorInd = "NationalSimplificatorInd";
		public const string DeclarantOrgPK = "DeclarantOrgPK";
		public const string DeclarantAddressPK = "DeclarantAddressPK";
		public const string DeclEmailAddr = "DeclEmailAddr";
		public const string CombinedMessage = "CombinedMessage";
		public const string BrokerCode = "BrokerCode";
		public const string DocType = "DocType";
		public const string TADPrintProcedure = "TADPrintProcedure";
		public const string ClearanceCriteria = "ClearanceCriteria";
		public const string IsSecurity = "IsSecurity";
		public const string TrainingEntry = "TrainingEntry";
		public const string RequestDispatch = "RequestDispatch";
		public const string UpdatePreDeclaration = "UpdatePreDeclaration";
		public const string TNNDocumentType = "TNNDocumentType";

		public const int NationalSimplificatorIndMaxLength = 1;
		public const int TNNDocumentTypeMaxLength = 1;
		public const int BrokerCodeMaxLength = 3;
		public const int TADPrintProcedureMaxLength = 1;
		public const int ClearanceCriteriaMaxLength = 2;
	}

	#region GenAddOn
	public static class GenAddOnColumnConstants
	{
		public const string TrainingEntryColumnName = "ES_NCTS_TrainingEntry";
		public const string RequestDispatchColumnName = "ES_NCTS_RequestDispatch";
		public const string UpdatePreDeclarationColumnName = "ES_NCTS_UpdatePreDeclaration";
	}
	#endregion

	#region CombinedMessage

	public ZBool CombinedMessage
	{
		get => ESNctsHeader.CEN_CombinedMessage;
		set
		{
			ESNctsHeader.CEN_CombinedMessage = value;
		}
	}
	public ZWrappedPropertyInfo CombinedMessageInfo => GetWrappedZPropertyInfo(Schema.CombinedMessage, x => ESNctsHeader.CEN_CombinedMessageInfo);

	#endregion

	protected override ZString GetDefaultDepartureAndArrivalMessageStatus => EU.NCTS.Business.NctsMessageStatusList.Codes.ArrivalNotificationNotSent;

	public override bool UnloadingRemarksAllowedOverride => CombinedMessage;

	protected override bool IsDepartureAmendmentAllowedCore()
	{
		var departureStatus = MovementHeader.BM_CustomsStatus;
		return departureStatus == EU.NCTS.Business.NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid
		|| departureStatus == EU.NCTS.Business.NctsTransitStatusList.Codes.DeclarationCancelled;
	}

	protected override bool IsDepartureCancellationAllowedCore()
	{
		if (MovementHeader == null || !IsDepartureMovement)
		{
			return false;
		}
		var departureStatus = MovementHeader.BM_CustomsStatus;
		return departureStatus == EU.NCTS.Business.NctsTransitStatusList.Codes.DeclarationMrnAllocated
			|| departureStatus == EU.NCTS.Business.NctsTransitStatusList.Codes.GoodsUnderCustomsControl
			|| departureStatus == EU.NCTS.Business.NctsTransitStatusList.Codes.DeclarationGuaranteesNotValid;
	}

	protected override bool IsUnloadingAllowedOrCompleteCore => base.IsUnloadingAllowedOrCompleteCore || isLastIncomingMessageOBSAccepted;

	bool isLastIncomingMessageOBSAccepted => Messages.LastIncomingMessage != null && Messages.LastIncomingMessage.EM_MessageType == DeclarationMessageTypeList.Codes.NctsUnloadingRemarks &&
		Messages.LastIncomingMessage.EM_MessageSubType == DeclarationMessageSubTypeList.Codes.AcceptedResponse;

	protected override EU.NCTS.Business.NctsDepartureMovementHeader GetNewDepartureMovementHeader() =>
		EU.NCTS.Business.NctsCommonMovementHeader.LoadOrCreate<NctsDepartureMovementHeader>(this, NctsMoveHeaderType.Codes.Departure);

	#region NationalSimplificatorInd

	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.NatSimplificationIndicator))]
	[MaxLength(Schema.NationalSimplificatorIndMaxLength)]
	public ZString NationalSimplificatorInd
	{
		get => ESNctsHeader.CEN_NationalSimplificatorInd;
		set => ESNctsHeader.CEN_NationalSimplificatorInd = value;
	}

	public ZWrappedPropertyInfo NationalSimplificatorIndInfo => GetWrappedZPropertyInfo(Schema.NationalSimplificatorInd, x => ESNctsHeader.CEN_NationalSimplificatorIndInfo);

	#endregion

	#region Declarant

	#region DeclarantOrg

	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.Organisations))]
	public ZGuid DeclarantOrgPK
	{
		get { return DeclarantAddressPK_ZAddress.OrgPK; }
		set { DeclarantAddressPK_ZAddress.OrgPK = value; }
	}

	public ZPropertyInfo DeclarantOrgPKInfo
	{
		get { return GetWrappedZPropertyInfo(Schema.DeclarantOrgPK, x => DeclarantAddressPK_ZAddress.OrgPKInfo); }
	}

	#endregion

	#region DeclarantAddress

	[RelatedBusinessObject(nameof(DeclarantAddress))]
	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.OrgAddressesList))]
	public ZGuid DeclarantAddressPK
	{
		get => ESNctsHeader.CEN_OA_DeclarantAddress;
		set => ESNctsHeader.CEN_OA_DeclarantAddress = value;
	}
	public ZWrappedPropertyInfo DeclarantAddressPKInfo => GetWrappedZPropertyInfo(Schema.DeclarantAddressPK, x => ESNctsHeader.CEN_OA_DeclarantAddressInfo);

	public OrgAddress DeclarantAddress => Factory.Load<OrgAddress>(DeclarantAddressPK);

	public ZAddress DeclarantAddressPK_ZAddress
	{
		get
		{
			if (declarantAddressPK_ZAddress == null)
			{
				declarantAddressPK_ZAddress = new ZAddress(DeclarantAddressPKInfo);
				declarantAddressPK_ZAddress.DefaultAddressType = AddressType.OFC;
			}
			return declarantAddressPK_ZAddress;
		}
	}
	ZAddress declarantAddressPK_ZAddress;

	#endregion

	#endregion

	#region DeclEmailAddr

	public ZString DeclEmailAddr
	{
		get
		{
			if (!declEmailAddr.HasValue)
			{
				declEmailAddr = EmailHelper.GetDeclEmailAddrFromRegistry();
			}
			return declEmailAddr.Value;
		}
	}
	ZString? declEmailAddr;

	public ZPropertyInfo DeclEmailAddrInfo => GetZPropertyInfo(Schema.DeclEmailAddr);

	#endregion

	#region TADPrintProcedure

	[MaxLength(Schema.TADPrintProcedureMaxLength)]
	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.TADPrintProcedureList))]
	public ZString TADPrintProcedure { get => ESNctsHeader.CEN_TADPrintProcedure; set => ESNctsHeader.CEN_TADPrintProcedure = value; }
	public ZWrappedPropertyInfo TADPrintProcedureInfo => GetWrappedZPropertyInfo(Schema.TADPrintProcedure, x => ESNctsHeader.CEN_TADPrintProcedureInfo);

	public ZString TADPrintProcedureDescription => Lookups.TADPrintProcedureList.GetDescriptionFromCode(TADPrintProcedure);

	#endregion

	#region ClearanceCriteria

	[MaxLength(Schema.ClearanceCriteriaMaxLength)]
	public ZString ClearanceCriteria { get => ESNctsHeader.CEN_ClearanceCriteria; set => ESNctsHeader.CEN_ClearanceCriteria = value; }
	public ZWrappedPropertyInfo ClearanceCriteriaInfo => GetWrappedZPropertyInfo(Schema.ClearanceCriteria, x => ESNctsHeader.CEN_ClearanceCriteriaInfo);

	public ZString ClearanceCriteriaDescription => MovementHeader?.Lookups.NctsControlResultList.GetDescriptionFromCode(ClearanceCriteria) ?? ZString.Empty;

	#endregion

	#region BrokerCode

	[MaxLength(Schema.BrokerCodeMaxLength)]
	public ZString BrokerCode
	{
		get => ArrivalMovementHeader?.BM_GS_NKCusAgent ?? ZString.Empty;
		set
		{
			var nctsArrivalMovementHeader = ArrivalMovementHeader;
			if (nctsArrivalMovementHeader != null)
			{
				var oldValue = BrokerCode;
				nctsArrivalMovementHeader.BM_GS_NKCusAgent = value;
				BrokerCodeInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo BrokerCodeInfo => GetWrappedZPropertyInfo(Schema.BrokerCode, x => ArrivalMovementHeader?.BM_GS_NKCusAgentInfo);

	#endregion

	#region TNNDocumentType

	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.NCTS5ArrivalTNNTypeList))]
	[ResourceStringData("2420E1C1-C76F-4FC1-8D60-8D4CE2B010D9", Caption = "TNN Document", MediumCaption = "TNN Doc.", ShortCaption = "TNN D.", FullDescription = "TNN Document Type")]
	[MaxLength(Schema.TNNDocumentTypeMaxLength)]
	public ZString TNNDocumentType
	{
		get => ESNctsHeader.CEN_TNNDocumentType;
		set => ESNctsHeader.CEN_TNNDocumentType = value;
	}

	public ZWrappedPropertyInfo TNNDocumentTypeInfo => GetWrappedZPropertyInfo(Schema.TNNDocumentType, x => ESNctsHeader.CEN_TNNDocumentTypeInfo);

	#endregion

	#region BrokerName
	public ZString BrokerName
	{
		get
		{
			var staff = base.ArrivalMovementHeader?.CusAgent;
			return staff != null ? staff.GS_FullName.Left(35) : GlbStaff.CurrentUser.GS_FullName.Left(35);
		}
	}

	#endregion

	#region BH_CustomsProfile

	[List(nameof(Lookups) + "." + nameof(NctsHeaderLookups.CertificateNames))]
	[ResourceStringData("51399A6F-5513-4934-87D1-8789C5EDD50F", Caption = "Certificate", MediumCaption = "Certif.", ShortCaption = "Cert.", FullDescription = "The certificate selected will be used to sign and communicate with Customs to declare all entries in this Job")]
	public override ZString BH_CustomsProfile { get => base.BH_CustomsProfile; set => base.BH_CustomsProfile = value; }

	#endregion

	#region DestinationCustomsOffice
	protected override void CustomsOfficesForDeparture_ListChanged(object sender, ListChangedEventArgs e)
	{
		base.CustomsOfficesForDeparture_ListChanged(sender, e);
		var listChangedType = e.ListChangedType;
		if (listChangedType == ListChangedType.ItemChanged)
		{
			Bills?.ForEach(b => b.GoodsItems?.ForEach(item => item.SetDefaultTaxType()));
		}
	}
	#endregion

	public ZBool TrainingEntry
	{
		get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.TrainingEntryColumnName);
		set
		{
			var oldValue = TrainingEntry;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.TrainingEntryColumnName, value);
				TrainingEntryInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo TrainingEntryInfo => GetZPropertyInfo(Schema.TrainingEntry);

	void SetDefaultTrainingEntry()
	{
		TrainingEntry = true;
	}

	[MaxLength(1)]
	public ZString RequestDispatch
	{
		get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnConstants.RequestDispatchColumnName);
		set
		{
			var oldValue = RequestDispatch;
			if (oldValue != value)
			{
				CheckMaximumLength(RequestDispatchInfo, value);
				this.SetSystemDefinedValue(GenAddOnColumnConstants.RequestDispatchColumnName, value);
				RequestDispatchInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo RequestDispatchInfo => GetZPropertyInfo(Schema.RequestDispatch);

	public ZBool UpdatePreDeclaration
	{
		get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.UpdatePreDeclarationColumnName);
		set
		{
			var oldValue = UpdatePreDeclaration;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.UpdatePreDeclarationColumnName, value);
				UpdatePreDeclarationInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo UpdatePreDeclarationInfo => GetZPropertyInfo(Schema.UpdatePreDeclaration);

	[LightValidationTestExempt]
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
					MovementHeader?.MarkAsNeedingValidation();
					DepartureHeaderContainers.MarkAsNeedingValidation();
				}
			}
		}
	}

	public override ZString BH_JobReference
	{
		get => base.BH_JobReference;
		set
		{
			base.BH_JobReference = value;
			if (!IsMarkingAsNeedingValidationSuspended)
			{
				MovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	protected override ZBool IsBrokerNeededCore => !IsDepartureAndArrivalMovement;

	public override ZString BH_RL_NKImportLoadPort
	{
		get => base.BH_RL_NKImportLoadPort;
		set
		{
			if (BH_RL_NKImportLoadPort != value)
			{
				base.BH_RL_NKImportLoadPort = value;
				if (!IsMarkingAsNeedingValidationSuspended)
				{
					MovementHeader?.MarkAsNeedingValidation();
				}
			}
		}
	}

	public override ZBool BH_FTZMove
	{
		get => base.BH_FTZMove;
		set
		{
			var oldValue = BH_FTZMove;
			if (oldValue != value)
			{
				base.BH_FTZMove = value;
				if (!IsMarkingAsNeedingValidationSuspended)
				{
					Bills?.ForEach(b => b.GoodsItems.MarkAsNeedingValidation());
					MovementHeader?.GoodsItems?.MarkAsNeedingValidation();
					ArrivalMovementHeader?.GoodsItems?.MarkAsNeedingValidation();
				}
			}
		}
	}

	public ZString ClearanceReferenceNumber => ClearanceEntryNumber.CE_EntryNum;

	public bool IsSafetyAndSecurityUncheckedWithExistingSecurityData => !BH_FTZMove && ListOfFieldsThatWouldHaveToBeEmptyIfSafetyAndSecurityUnchecked.Any(x => !x.Value.IsEmpty);

	List<ZPropertyInfo> listOfSecurityRelatedFields;
	IEnumerable<ZPropertyInfo> ListOfFieldsThatWouldHaveToBeEmptyIfSafetyAndSecurityUnchecked
	{
		get
		{
			if (listOfSecurityRelatedFields == null)
			{
				listOfSecurityRelatedFields = new List<ZPropertyInfo>() { MovementHeader.BM_BTAIndicatorInfo, MovementHeader.BM_MethodOfPaymentInfo, MovementHeader.BM_AdditionalTextInfo, MovementHeader.BM_ConveyanceNumberInfo, PlaceOfUnloadingCodeInfo, BH_OH_CarrierInfo, BH_UniqueVoyageIdentifierInfo, SecurityConsignor.E2_OA_AddressInfo, SecurityConsignee.E2_OA_AddressInfo };

				foreach (EU.NCTS.Business.NonPersistentItineraryCountry iti in Itinerary)
				{
					listOfSecurityRelatedFields.Add(iti.CountryCodeInfo);
				}
			}
			return listOfSecurityRelatedFields;
		}
	}

	protected override ZBool IsConsigneeReadOnly => IsPhase5 && (MovementHeader?.IsPhaseStatusTNN ?? ZBool.False);

	protected override ZBool IsPrincipalReadOnly => MovementHeader?.CustomsStatusIsPRE ?? ZBool.False;

	protected override void OnChangedDestinationTraderDocumentaryAddress()
	{
		if (IsPhase5 && IsArrivalMovement && ESNctsHeader.CEN_TNNArrival && ArrivalMovementHeader.HeaderTNN != null)
		{
			ArrivalMovementHeader.HeaderTNN.Consignee.OrganisationPK = base.DestinationTrader.OrganisationPK;
		}
	}

	protected override ZBool ShouldWipeRelatedFieldsCore => ZBool.False;

	public CusEntryNumber ClearanceEntryNumber
	{
		get
		{
			if (clrEntryNumber == null || clrEntryNumber.IsDeleted)
			{
				clrEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Spain.ClearanceCSV, CountryCode);
				RegisterEditableChildObject(clrEntryNumber);
			}

			return clrEntryNumber;
		}
	}

	CusEntryNumber clrEntryNumber;

	public ZDateTime ClearanceDate => ClearanceEntryNumber.CE_IssueDate;

	public ZDateTime ArrivalLimit => ClearanceEntryNumber.CE_ExpiryDate;

	public ZDateTime AcceptanceDate => MovementReferenceEntryNumber.CE_IssueDate;

	public CusEntryNumber SummaryEntryNumber
	{
		get
		{
			if (summaryEntryNumber == null || summaryEntryNumber.IsDeleted)
			{
				summaryEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Spain.SummaryEntryNumber, CountryCode);
				RegisterEditableChildObject(summaryEntryNumber);
			}

			return summaryEntryNumber;
		}
	}

	CusEntryNumber summaryEntryNumber;

	[ResourceStringData("19D52DB7-CCE3-41CC-8147-3C7A6EDA97A4", Caption = "Release Date", MediumCaption = "Rel. Date", ShortCaption = "Rel. Date")]
	public ZDateTime ReleaseDate => SummaryEntryNumber.CE_IssueDate;

	[ResourceStringData("B0A7AE41-C975-4F77-9AF2-D5B94B62F748", Caption = "Arrival Summary Declaration", MediumCaption = "Arr. SD Number", ShortCaption = "Arr. SD", FullDescription = "Summary Declaration Number assigned to the Arrival by ES Customs")]
	public ZString ArrivalSummaryDeclaration => SummaryEntryNumber.CE_EntryNum;

	public ZPropertyInfo ArrivalSummaryDeclarationInfo => GetZPropertyInfo(nameof(ArrivalSummaryDeclaration));

	public ZString ArrivalSummaryDeclarationUrl
	{
		get
		{
			if (ArrivalSummaryDeclaration.Length < ArrivalSummaryDeclarationCanonicalLength)
			{
				return ZString.Empty;
			}

			var hasCanonicalLength = ArrivalSummaryDeclaration.Length == ArrivalSummaryDeclarationCanonicalLength;

			var recinto = hasCanonicalLength ? ArrivalSummaryDeclaration.Substring(0, 4) : ZString.Empty;
			var anio = hasCanonicalLength ? ArrivalSummaryDeclaration.Substring(4, 1) : ZString.Empty;
			var numero = hasCanonicalLength ? ArrivalSummaryDeclaration.Substring(5) : ZString.Empty;
			var mrn = hasCanonicalLength ? ZString.Empty : ArrivalSummaryDeclaration;

			var queryUrl = ESCustomsDataRegistry.Instance.SummaryDeclarationStatusQueryURL.Value.Replace(CustomsWebsiteUrlCodes.RecintoInRegistryUrl, recinto).Replace(CustomsWebsiteUrlCodes.AnioInRegistryUrl, anio).Replace(CustomsWebsiteUrlCodes.NumeroInRegistryUrl, numero).Replace(CustomsWebsiteUrlCodes.MRNinRegistryUrl, mrn);
			return queryUrl;
		}
	}

	const int ArrivalSummaryDeclarationCanonicalLength = 11;

	[ResourceStringData("64A318CF-3045-4115-9F1F-51A35E394207", Caption = "Circuit", MediumCaption = "Circuit", ShortCaption = "Circuit", FullDescription = "Circuit assigned by ES Customs")]
	public ZString Circuit
	{
		get
		{
			var code = MovementReferenceEntryNumber.CE_EntryStatus;
			var description = (ZString)CircuitCodeList.GetDescriptionFromCode(code);
			return !description.IsEmpty ? description : code;
		}
	}

	public ZPropertyInfo CircuitInfo => GetZPropertyInfo(nameof(Circuit));

	CodeDescriptionPairList CircuitCodeList => Factory.GetCachedValue<CircuitCodeList>();

	public CusESNctsHeader ESNctsHeader
	{
		get
		{
			if (esNctsHeader == null || esNctsHeader.IsDeleted)
			{
				esNctsHeader = Factory.Load<CusESNctsHeader>(new ZQuery(CusESNctsHeaderSchema.CEN_BH, SQLComparisonOperator.Equal, PK)).FirstOrDefault();
				if (esNctsHeader == null)
				{
					esNctsHeader = Factory.New<CusESNctsHeader>();
					esNctsHeader.CEN_BH = PK;
				}
				RegisterEditableChildObject(esNctsHeader);
				RegisterListChangedCalledRefreshBinding(esNctsHeader);
			}
			return esNctsHeader;
		}
	}
	CusESNctsHeader esNctsHeader;

	protected override EU.NCTS.Business.NctsGuaranteeRefresher GetNewGuaranteeRefresher() => new ESNctsGuaranteeRefresher(this);

	protected override EU.NCTS.Business.NctsHeaderDocumentSupporter GetNewDocumentSupporter() => new NctsHeaderDocumentSupporter(this);

	protected class ESNctsGuaranteeRefresher : EU.NCTS.Business.NctsGuaranteeRefresher
	{
		public ESNctsGuaranteeRefresher(EU.NCTS.Business.NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

			protected override Func<EU.Business.CusGuaranteeHeader, bool> GuaranteeFilter => (x) => x.CPH_Type == EUGuaranteeTypeList.Codes.TRA;
			protected override ZBool ManageMultipleGuarantees => ZBool.False;
		}

	public override void Delete()
	{
		EDocPivotCollection.RemoveAndDeleteAll();
		ESNctsHeader.Delete();
		base.Delete();
	}

	public new EU.NCTS.Business.INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader> DepartureHeaderContainers
		=> (EU.NCTS.Business.INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>)base.DepartureHeaderContainers;

	protected override EU.NCTS.Business.INctsDepartureHeaderContainerCollection<EU.NCTS.Business.NctsDepartureHeaderContainer, EU.NCTS.Business.NctsHeader> GetDepartureHeaderContainersCore()
		=> new EU.NCTS.Business.NctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader>(this);

	public new EU.NCTS.Business.INctsGuaranteeCollection<NctsGuarantee> Guarantees
		=> (EU.NCTS.Business.INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

	protected override EU.NCTS.Business.INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesForNonPhase5Departure()
		=> new EU.NCTS.Business.NctsGuaranteeCollection<NctsGuarantee>(this);

	public new NctsHeaderLookups Lookups => (NctsHeaderLookups)base.Lookups;

	protected override CusInBondHeaderLookups GetNewLookups() => new NctsHeaderLookups(this);

	public new EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActors => (EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>)base.CusSupplyChainActors;
	protected override EU.Business.Declaration.ICusSupplyChainActorReferenceCollection<EU.NCTS.Business.CusSupplyChainActorReference> GetCusSupplyChainActorsCore() => new EU.Business.Declaration.CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

	protected override CusInBondHeaderValidation GetNewPhase4Validation() => new NctsHeaderValidation(this);

	protected override CusInBondHeaderValidation GetNewPhase5Validation() => new NctsHeaderPhase5Validation(this);

	public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

	public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;

	public new NctsUnloadingMovementHeader UnloadingMovementHeader => (NctsUnloadingMovementHeader)base.UnloadingMovementHeader;

	public new EU.NCTS.Business.SealCollection<Seal> Seals => (EU.NCTS.Business.SealCollection<Seal>)base.Seals;

	public new EU.NCTS.Business.INctsBillCollection<NctsBill> Bills => (EU.NCTS.Business.INctsBillCollection<NctsBill>)base.Bills;

	protected override EU.NCTS.Business.INctsBillCollection<EU.NCTS.Business.NctsBill> GetNewBillCollection()
		=> new NctsBillCollection(this);

	protected override Type BillTypeCore => typeof(NctsBill);

	protected override EU.NCTS.Business.SealCollection GetNewSealCollection() => new EU.NCTS.Business.SealCollection<Seal>(this);

	public new NctsESOfficeCodeCollectionForDepartureGrid CustomsOfficesForDeparture => (NctsESOfficeCodeCollectionForDepartureGrid)base.CustomsOfficesForDeparture;

	protected override EU.NCTS.Business.NctsEuOfficeCodeCollectionForDepartureGrid GetNewCustomsOfficesForDeparture()
		=> new NctsESOfficeCodeCollectionForDepartureGrid(this);

	public new EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalDocuments
		=> (EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalDocuments;

	protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetAdditionalDocuments()
		=> new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

	public new EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument> PreviousDocuments
		=> (EU.NCTS.Business.ICommonPreviousDocumentCollection<CommonPreviousDocument>)base.PreviousDocuments;

	protected override EU.NCTS.Business.ICommonPreviousDocumentCollection<EU.NCTS.Business.CommonPreviousDocument> GetPreviousDocuments()
		=> new EU.NCTS.Business.CommonPreviousDocumentCollection<CommonPreviousDocument>(this);

	protected override Customs.Business.CommonGoodsItemsIntegration.ICommonGoodsItemsIntegrator CommonGoodsItemsIntegratorCore
			=> new NctsCommonGoodsItemsIntegrator(this);

	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
	protected override Type PreviousDocumentType => typeof(CommonPreviousDocument);

	[ChildEditable(false)]
	public new NctsESOfficeCodeCollection CustomsOffices => (NctsESOfficeCodeCollection)base.CustomsOffices;

	protected override EU.NCTS.Business.NctsEuOfficeCodeCollection GetNewCustomsOffices() => new NctsESOfficeCodeCollection(this);

	protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
	{
		var result = base.GetCusCodeDataTypesCore();
		result[EU.NCTS.Business.CusCodeDataTypeList.Codes.Seal] = typeof(Seal);
		result[EU.Business.CusCodeDataTypeList.Codes.OfficeCode] = typeof(NctsESOfficeCode);
		return result;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		if (Branch.OrgProxy?.MainAddress is OrgAddress orgProxyAddress)
		{
			DeclarantAddressPK = orgProxyAddress.PK;
		}
		SetDefaultTrainingEntry();
	}

	public override void OnSaving()
	{
		if (!IsInDatabase)
		{
			DefaultCusAgent();
		}
		base.OnSaving();
	}

	void DefaultCusAgent()
	{
		if (!GlbStaff.CurrentUser.GS_IsSystemAccount)
		{
			if (IsArrivalMovement && ArrivalMovementHeader.BM_GS_NKCusAgent.IsEmpty)
			{
				ArrivalMovementHeader.BM_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			}
			else if (!IsArrivalMovement && MovementHeader.BM_GS_NKCusAgent.IsEmpty)
			{
				MovementHeader.BM_GS_NKCusAgent = GlbStaff.CurrentUser.GS_Code;
			}
		}
	}

	protected override void OnChangedConsignorDocumentaryAddress()
	{
		ModifyReleaseStatusInPredeclaration();
	}

	protected override void OnChangedConsigneeDocumentaryAddress()
	{
		ModifyReleaseStatusInPredeclaration();
	}

	protected override void Guarantees_ListChanged(object sender, ListChangedEventArgs e)
	{
		var currentCount = Guarantees.Count;
		ModifyReleaseStatusInPredeclarationForCollections(e.ListChangedType, previousGuaranteesCount, currentCount);
		previousGuaranteesCount = currentCount;
	}
	int previousGuaranteesCount;

	protected override void AdditionalDocuments_ListChanged(object sender, ListChangedEventArgs e)
	{
		var currentCount = AdditionalDocuments.Count;
		ModifyReleaseStatusInPredeclarationForCollections(e.ListChangedType, previousAdditionalDocumentsCount, currentCount);
		previousAdditionalDocumentsCount = currentCount;
	}
	int previousAdditionalDocumentsCount;

	protected override void PreviousDocuments_ListChanged(object sender, ListChangedEventArgs e)
	{
		var currentCount = PreviousDocuments.Count;
		ModifyReleaseStatusInPredeclarationForCollections(e.ListChangedType, previousPreviousDocumentsCount, currentCount);
		previousPreviousDocumentsCount = currentCount;
	}
	int previousPreviousDocumentsCount;

	protected override void CountriesOfRouting_ListChanged(object sender, ListChangedEventArgs e)
	{
		var currentCount = CountriesOfRouting.Count;
		ModifyReleaseStatusInPredeclarationForCollections(e.ListChangedType, previousCountriesOfRoutingCount, currentCount);
		previousCountriesOfRoutingCount = currentCount;
	}
	int previousCountriesOfRoutingCount;

	public void ModifyReleaseStatusInPredeclarationForCollections(ListChangedType listChangedType, int previousCount, int currentCount)
	{
		bool isItemDeletedFromCollection(ListChangedType listChangedType, int previousCount, int currentCount)
			=> deletedChangedTypes.Contains(listChangedType) && previousCount > currentCount;

		if (listChangedType == ListChangedType.ItemAdded || isItemDeletedFromCollection(listChangedType, previousCount, currentCount))
		{
			ModifyReleaseStatusInPredeclaration();
		}
	}

	readonly ListChangedType[] deletedChangedTypes = new ListChangedType[] { ListChangedType.ItemDeleted, ListChangedType.Reset, ListChangedType.ItemChanged };

	public void ModifyReleaseStatusInPredeclaration()
	{
		if (IsDepartureMovement && MovementHeader.BM_CustomsStatus == ESNCTS5DepartureCustomsStatusList.Codes.PreLodged)
		{
			BH_ReleaseStatus = ChangesToPreDeclaration;
		}
	}

	const string ChangesToPreDeclaration = "1";

	public ZString CheckPreDeclarationChanges(NctsMessageSendingObject messageSendingObject)
	{
		var result = ZString.Empty;
		var lastMessage = NctsDeclarationComparatorHelper.GetLatestEffectiveMessage(this);

		if (lastMessage != null)
		{
			var comparableLastMessage = NctsDeclarationComparatorHelper.GetComparablePredeclaration(lastMessage);
			var comparableHeader = NctsDeclarationComparatorHelper.GetCurrentNctsHeaderComparable(this, messageSendingObject);

			result = NctsDeclarationComparatorHelper.Compare(comparableHeader, comparableLastMessage);
		}

		return result;
	}

	public override bool SupportsCombinedArrivalAndDepartureMessage => true;

	public ZBool IsACEAuthorizedHolder => DestinationTrader.IsAuthorizedHolder(Factory, CountryCode);

	ZString IESMessageBusinessObject.EntryReference => BH_JobReference;

	GlbStaff IESMessageInfoProvider.Broker => IsArrivalMovement ? ArrivalMovementHeader?.CusAgent : MovementHeader?.CusAgent;
	ZString IESMessageInfoProvider.MRN => MovementReferenceNumber;
	ZString IESMessageInfoProvider.DocumentJobReference => MovementReferenceNumber;

	ZGuid IESResponseBusinessObject.BranchPK => Branch.PK;

	EDIMessageCollection IESResponseBusinessObject.MessageCollection => Messages;

	ZString IESResponseBOMessageStatus.MessageStatus { set => EffectiveMessageStatus = value; }

	ZString IPollingTransactionParent.CertificateName => BH_CustomsProfile;

	ZBool IPollingTransactionParent.IsTest
	{
		get
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
						&& (!registration.IsWiseTechGlobalInternalSystem()
							|| (bool)TrainingEntry);
		}
	}

	GlbStaff IPollingTransactionParent.Broker => IsArrivalMovement ? ArrivalMovementHeader?.CusAgent : MovementHeader?.CusAgent;

	OrgHeader IPollingTransactionParent.Declarant => MovementHeader?.Representative?.Address?.Header ?? Principal?.Address?.Header;

	protected override bool LocalReferenceNumberReadOnlyCore
	{
		get
		{
			if ((MovementHeader?.BM_CustomsStatus ?? ZString.Empty) == NctsTransitStatusList.Codes.DeclarationRejected
				&& EffectiveMessageStatus != EU.NCTS.Business.NctsMessageStatusList.Codes.DepartureDeclarationSent)
			{
				return false;
			}
			else if ((ArrivalMovementHeader?.BM_CustomsStatus ?? ZString.Empty) == NctsTransitStatusList.Codes.DeclarationRejected
				&& EffectiveMessageStatus != EU.NCTS.Business.NctsMessageStatusList.Codes.ArrivalNotificationSent)
			{
				return false;
			}
			else
			{
				return base.LocalReferenceNumberReadOnlyCore;
			}
		}
	}

	public string GetUrlToLaunch() => !IsDeleted ? new UrlDecider(this).GetUrlNcts() : ZString.Empty;

	public ZBool CanLaunchNctsUrl() => !MovementReferenceNumber.IsEmpty;

	public void SetClearanceInfoClearanceNumber(ZString referenceNumberFromUser)
	{
		if (!referenceNumberFromUser.Equals(ClearanceReferenceNumber))
		{
			var oldValue = ClearanceReferenceNumber;
			ClearanceEntryNumber.CE_EntryNum = referenceNumberFromUser;
			MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture;

			var logReason = (NoResString)"Manually Modify Clearance Number to Entry " + BH_JobReference;
			Logs.AddNew(ZArchitecture.Business.Events.ChangeOfIdentifier, SetParametersInLog(oldValue, ClearanceEntryNumber.CE_EntryNumInfo.Value.ToString(), logReason));
		}
	}

	public void SetClearanceInfoClearanceDate(ZDateTime issueDateFromUser)
	{
		if (!issueDateFromUser.Equals(ClearanceDate))
		{
			var oldValue = ClearanceDate;
			ClearanceEntryNumber.CE_IssueDate = issueDateFromUser;

			var logReason = (NoResString)"Manually Modify Clearance Date to Entry " + BH_JobReference;
			Logs.AddNew(ZArchitecture.Business.Events.ChangeOfIdentifier, SetParametersInLog(oldValue.ToString(), ClearanceEntryNumber.CE_IssueDateInfo.Value.ToString(), logReason));
		}
	}

	public void SetClearanceInfoArrivalLimitDate(ZDateTime arrivalLimitDateFromUser)
	{
		if (!arrivalLimitDateFromUser.Equals(ArrivalLimit))
		{
			var oldValue = ArrivalLimit;
			ClearanceEntryNumber.CE_ExpiryDate = arrivalLimitDateFromUser;

			var logReason = (NoResString)"Manually Modify Arrival Limit Date to Entry " + BH_JobReference;
			Logs.AddNew(ZArchitecture.Business.Events.ChangeOfIdentifier, SetParametersInLog(oldValue.ToString(), ClearanceEntryNumber.CE_ExpiryDateInfo.Value.ToString(), logReason));
		}
	}

	KeyValuePair<string, string>[] SetParametersInLog(string oldValue, string newValue, string logReason)
	{
		var parameters = new KeyValuePair<string, string>[]
		{
			new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Old, oldValue),
			new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.New, newValue),
			new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, CusEntryNumberTypes.Spain.ClearanceCSV),
			new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, logReason)
		};
		return parameters;
	}

	protected override bool IsDepartureTabReadOnlyCore => IsPhase5
															? IsDepartureMovement
																	&& (DepartureStatusCodesForDepartureTabReadOnly.Contains(MovementHeader.BM_CustomsStatus)
																			|| IsSent)
															: base.IsDepartureTabReadOnlyCore;

	ImmutableList<ZString> DepartureStatusCodesForDepartureTabReadOnly => departureStatusCodesForDepartureTabReadOnly
																			?? (departureStatusCodesForDepartureTabReadOnly =
																					new List<ZString>()
																						{
																							ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl,
																							ESNCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid,
																							ESNCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit,
																							ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit,
																							ESNCTS5DepartureCustomsStatusList.Codes.Cancelled,
																							ESNCTS5DepartureCustomsStatusList.Codes.Invalidated,
																							ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance,
																							ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
																						}.ToImmutableList());
	ImmutableList<ZString> departureStatusCodesForDepartureTabReadOnly;

	protected override bool IsArrivalTabReadOnlyCore => IsArrivalMovement
															&& (MessageStatusCodesForArrivalTabReadOnly.Contains(EffectiveMessageStatus)
																	|| (IsPhase5
																			? ArrivalStatusCodesForArrivalTabReadOnlyPhase5.Contains(ArrivalMovementHeader.BM_CustomsStatus)
																			: ArrivalStatusCodesForArrivalTabReadOnlyPhase4.Contains(ArrivalMovementHeader.BM_CustomsStatus)));

	ImmutableList<ZString> ArrivalStatusCodesForArrivalTabReadOnlyPhase4 => arrivalStatusCodesForArrivalTabReadOnlyPhase4
																			?? (arrivalStatusCodesForArrivalTabReadOnlyPhase4 =
																					new List<ZString>()
																						{
																							NctsTransitStatusList.Codes.UnloadingPermissionGranted,
																							NctsTransitStatusList.Codes.GoodsWrittenOff,
																							NctsTransitStatusList.Codes.GoodsUnderCustomsControl,
																							NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival
																						}.ToImmutableList());
	ImmutableList<ZString> arrivalStatusCodesForArrivalTabReadOnlyPhase4;

	ImmutableList<ZString> ArrivalStatusCodesForArrivalTabReadOnlyPhase5 => arrivalStatusCodesForArrivalTabReadOnlyPhase5
																			?? (arrivalStatusCodesForArrivalTabReadOnlyPhase5 =
																					new List<ZString>()
																						{
																							ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted,
																							ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease,
																							ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl,
																							ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease
																						}.ToImmutableList());
	ImmutableList<ZString> arrivalStatusCodesForArrivalTabReadOnlyPhase5;

	ImmutableList<ZString> MessageStatusCodesForArrivalTabReadOnly => messageStatusCodesForArrivalTabReadOnly
																			?? (messageStatusCodesForArrivalTabReadOnly =
																					new List<ZString>()
																					{
																					NctsMessageStatusList.Codes.ArrivalNotificationSent,
																					NctsMessageStatusList.Codes.UnloadingRemarksSent,
																					NctsMessageStatusList.Codes.UnloadingRemarksRejected
																					}.ToImmutableList());
	ImmutableList<ZString> messageStatusCodesForArrivalTabReadOnly;

	protected override bool ArrivalMrnFromUserReadOnlyCore
	{
		get
		{
			return base.ArrivalMrnFromUserReadOnlyCore || (ArrivalMovementHeader.HeaderTNN != null && !ArrivalMovementHeader.HeaderTNN.MovementHeader.BM_CustomsStatus.IsEmpty && ESNctsHeader.CEN_TNNArrival);
		}
	}

	protected override bool HasArrivalGoodItemsForMRNReadOnly => false;

	protected override bool IsArrivalDetailsReadOnlyCore => IsArrivalMovement && IsPhase5 && (IsSent || IsArrivalTabReadOnlyCore);

	protected override bool IsUnloadingRemarksTabReadOnlyCore
	{
		get
		{
			var customsStatus = ArrivalMovementHeader.BM_CustomsStatus;
			if (IsPhase5Arrival
				&& (customsStatus == NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease
					|| customsStatus == NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease))
			{
				return true;
			}

			return base.IsUnloadingRemarksTabReadOnlyCore;
		}
	}

	public WriteOffResult GetTrasitStatusFromCustomsAndAddTransactionIfNeeded()
	{
		string excludedArrivalText = Res.GetString("E1BB3694-91BF-4CD0-A5FE-7246105071AD", "Excluded (not a departure)");
		string excludedNotRegisteredText = Res.GetString("55110A81-B8AA-4FA8-9DD1-A0E08D5A95FA", "Excluded (not registered)");
		string excludedBrokerText = Res.GetString("C0153FE8-15B1-4B30-9102-698B4F1CA7DC", "Excluded (review broker)");
		const string EmptyStatus = "-";

		var jobNumber = JobNumber;
		var mrn = MovementReferenceNumber;
		var guaranteeStatus = IsDepartureMovement ? excludedNotRegisteredText : excludedArrivalText;
		var nctsStatus = EmptyStatus;

		if (IsDepartureMovement && !mrn.IsEmpty)
		{
			var broker = IsDepartureAndArrivalMovement ? ArrivalMovementHeader?.CusAgent : MovementHeader?.CusAgent;
			var certificateName = BH_CustomsProfile;
			guaranteeStatus = excludedBrokerText;
			if (broker != null && !certificateName.IsEmpty)
			{
				(nctsStatus, guaranteeStatus) = TryGetResponseAndCreatetransaction(broker, certificateName, mrn, nctsStatus, guaranteeStatus);
			}
		}

		return new WriteOffResult(jobNumber, mrn, guaranteeStatus, nctsStatus);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
	(string nctsStatus, string guaranteeStatus) TryGetResponseAndCreatetransaction(GlbStaff broker, ZString certificateName, ZString mrn, string previousNctsStatus, string previousGuaranteeStatus)
	{
		string excludedExceptionText = Res.GetString("85607A5F-0F06-4313-9CF5-7961FBA288A1", "Excluded (can't access Spanish Customs)");
		string notWrittenOffText = Res.GetString("33D6B162-D0E2-4CAB-9B8F-848086C3A884", "Not Written Off");

		const string StatusTextInHTML = "Estado:";
		const string DateTextInHTML = "Fecha de ultim";
		const string WrittenOffStatusFromHTML = "Ultimado";

		var guaranteeStatus = previousGuaranteeStatus;
		var nctsStatus = previousNctsStatus;

		try
		{
			var certificate = CertificateHelper.GetCertificate(broker, certificateName);
			if (certificate != null)
			{
				guaranteeStatus = notWrittenOffText;

				var htmlResponseText = CreateRequestAndGetResponse(mrn, certificate);
				var statusText = string.Empty;
				var dateText = string.Empty;

				var document = new HtmlDocument();
				document.LoadHtml(htmlResponseText);

				var documentNode = document.DocumentNode;
				var liNodes = documentNode.SelectNodes("//li");
				statusText = liNodes?.FirstOrDefault(x => x.InnerText.Contains(StatusTextInHTML, StringComparison.CurrentCultureIgnoreCase))?.InnerText.Split(':')[1].Trim();
				dateText = liNodes?.FirstOrDefault(x => x.InnerText.Contains(DateTextInHTML, StringComparison.CurrentCultureIgnoreCase))?.InnerText.Split(':')[1].Trim();

				if (!string.IsNullOrEmpty(statusText))
				{
					nctsStatus = statusText;

					if (nctsStatus.Equals(WrittenOffStatusFromHTML, StringComparison.CurrentCultureIgnoreCase))
					{
						ZDateTime.TryParseExact(dateText, out var dateForTransaction, CustomsDateTimeExtension.DateFormatSpainWithDash);

						var writeOffTransactionCreator = new WriteOffTransactionCreator();
						guaranteeStatus = writeOffTransactionCreator.AddGuaranteesWriteOffTransactionsNcts(this, dateForTransaction);
					}
				}
			}
		}
		catch (Exception ex) when (ex is IOException || !ex.IsCriticalException())
		{
			guaranteeStatus = excludedExceptionText;
			ErrorReporter.ReportOnce("NctsHeader.GetTrasitStatusFromCustomsAndAddTransactionIfNeeded", "Exception thrown when trying to get a response from Customs Url", ex);
		}
		return (nctsStatus, guaranteeStatus);
	}

	public virtual ZString CreateRequestAndGetResponse(ZString mrn, GlbExternalPassword certificate)
	{
		var urlFromRegistry = ESCustomsDataRegistry.Instance.NctsTransitStatusQueryUrl.Value;
		var url = urlFromRegistry.Replace(CustomsWebsiteUrlCodes.MRNinRegistryUrl, mrn);
		return new UrlRequest(url).GetResponseFromUrl(certificate);
	}

	public class TQUMessageInfo
	{
		public ZBool MessageSentCorrectly;
		public ZBool MessageCanBeSent;
		public ZString DeclarationWithBrokerError;
	}

	ZBool CanSendTQUForGuaranteeWriteOff() => IsDepartureMovement
											&& !MovementReferenceNumber.IsEmpty
											&& EntryHasGuaranteeAssociated();

	ZBool EntryHasGuaranteeAssociated()
	{
		var result = false;

		foreach (NctsGuarantee guarantee in GetEffectiveGuarantees())
		{
			if (CusGuaranteeHeaderHelper.LoadCusGuaranteeHeadersFromReferenceWithOBLTransaction(Factory, guarantee.PW_BondNumber, CountryCode, EUGuaranteeTypeList.Codes.TRA).Length > 0)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public TQUMessageInfo SendTQUForGuaranteeWriteOffIfPossible()
	{
		var result = new TQUMessageInfo()
		{
			MessageCanBeSent = false
		};

		if (CanSendTQUForGuaranteeWriteOff())
		{
			result.MessageCanBeSent = true;
			result.DeclarationWithBrokerError = BH_JobReference;
			var broker = IsDepartureAndArrivalMovement ? ArrivalMovementHeader?.CusAgent : MovementHeader?.CusAgent;
			if (broker != null && !CertificateHasMessageErrors())
			{
				var certificate = CertificateHelper.GetCertificate(broker, BH_CustomsProfile);
				if (certificate != null)
				{
					result.DeclarationWithBrokerError = ZString.Empty;
					TrySendTQUMessageForGuaranteeWriteOff(broker, certificate, result);
				}
			}
		}
		return result;

		ZBool CertificateHasMessageErrors()
		{
			Validation.ValidateBH_CustomsProfile();
			return BH_CustomsProfileInfo.HasMessageErrors();
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:ErrorReporterKey", Justification = "Baseline")]
	void TrySendTQUMessageForGuaranteeWriteOff(GlbStaff broker, GlbExternalPassword certificate, TQUMessageInfo messageInfo)
	{
		var messages = new List<ESEDIMessage>();
		try
		{
			var certificateObject = new CertificateObject(broker, certificate.GP_Name, certificate.GP_UserID);

			var builderManager = new ESNctsMessageBuilderManager(this, certificateObject, DeclarationMessageTypeList.Codes.TransitNcts5Query);
			var messageBuilders = ESNctsMessageSender.GetQueryMessageBuilders(this, builderManager);

			if (!messageBuilders.IsNullOrEmpty())
			{
				ESNctsMessageSender.Send(messageBuilders, messages);
			}

			if (messages.Count > 0)
			{
				messageInfo.MessageSentCorrectly = true;
			}
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ErrorReporter.ReportOnce("NctsHeader.TrySendTQUMessageForGuaranteeWriteOff", Res.GetString("BC3D2E71-BA6B-4FFA-B0B5-7D41F6A4CE26", "Error when creating the Transit NCTS Query message in declaration") + " " + BH_JobReference);
		}
	}

	public bool CanCreateExsDeclaration()
	{
		if (IsPhase4)
		{
			var canCreate = false;
			UnloadingMovementHeader.GoodsItems.ForEach(x =>
			{
				var currentItem = x;
				if (!currentItem.IsMissing)
				{
					canCreate = true;
				}
			});
			return canCreate;
		}

		return Bills.Any(bill => bill.UnloadedStatus != NctsUnloadedStateList.Codes.MIS && bill.ArrivalGoodsItems.Any(goodsItem => goodsItem.UnloadedStatus != NctsUnloadedStateList.Codes.MIS));
	}

	readonly List<Tuple<string, short>> containersList = new List<Tuple<string, short>>();

	public ZGuid CreateEXSFromArrival()
	{
		return IsPhase4 ? CreateEXSFromArrival_Phase4() : CreateEXSFromArrival_Phase5();
	}

	ZGuid CreateEXSFromArrival_Phase4()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = Customs.EU.Business.MessageTypeList.Codes.Export;
		declaration.JE_EntryStyle = Customs.EU.Business.EntryStyleListExport.Codes.ExportNormal;
		declaration.JE_CustomsOffice = DestinationCustomsOfficeCode;
		declaration.JE_LocationOfGoods = ArrivalMovementHeader.BM_LocationOfGoodsCode;
		declaration.JE_OH_Supplier = DestinationTrader.OrganisationPK;

		var entryInstructions = declaration.CustomsEntryInstructions.AddNew();
		entryInstructions.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;

		var secuentialNumberForNew = 99000;
		var secuentialNumberForOthers = 0;
		containersList.Clear();
		foreach (NctsArrivalAndUnloadingCargoDesc goodsItem in ArrivalMovementHeader.GoodsItems)
		{
			var goodsItemUnloading = UnloadingMovementHeader.GoodsItems.Cast<NctsArrivalAndUnloadingCargoDesc>().FirstOrDefault(x => (x.BY_LineNo == goodsItem.BY_LineNo));
			if (goodsItemUnloading.IsMissing == false)
			{
				if ((goodsItemUnloading.IsNew == false) && (goodsItemUnloading.HasDifferences == false))
				{
					secuentialNumberForOthers++;
					CreateDeclarationBody_Phase4(declaration, goodsItem, goodsItemUnloading.IsNew, secuentialNumberForNew, secuentialNumberForOthers);
				}
				else if (goodsItemUnloading.HasDifferences == true)
				{
					secuentialNumberForOthers++;
					CreateDeclarationBody_Phase4(declaration, goodsItemUnloading, goodsItemUnloading.IsNew, secuentialNumberForNew, secuentialNumberForOthers);
				}
				else if (goodsItemUnloading.IsNew == true)
				{
					secuentialNumberForNew++;
					CreateDeclarationBody_Phase4(declaration, goodsItemUnloading, goodsItemUnloading.IsNew, secuentialNumberForNew, secuentialNumberForOthers);
				}
			}
		}

		foreach (NctsArrivalAndUnloadingCargoDesc goodsItem in UnloadingMovementHeader.GoodsItems)
		{
			var arrivalGoodsItems = goodsItem.Header.ArrivalMovementHeader.GoodsItems;
			if ((goodsItem.IsNew == true) && (!arrivalGoodsItems.Any(line => line.BY_LineNo == goodsItem.BY_LineNo)))
			{
				secuentialNumberForNew++;
				CreateDeclarationBody_Phase4(declaration, goodsItem, goodsItem.IsNew, secuentialNumberForNew, secuentialNumberForOthers);
			}
		}

		LinkPackagesWithUniqueContainer(declaration);
		if (declaration.CusContainers.Count > 0)
		{
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
		}
		var invoiceNumber = LocalReferenceNumber;
		if (!invoiceNumber.IsEmpty)
		{
			declaration.JE_HouseBill = invoiceNumber;
		}
		return declaration.PK;
	}

	void CreateDeclarationBody_Phase4(JobDeclaration declaration, NctsArrivalAndUnloadingCargoDesc goodsItem, ZBool isNew, int secuentialNumberForNew, int secuentialNumberForOthers)
	{
		CreateInvoiceHeader(declaration);

		var declarationInvoiceLine = CreateInvoiceLine_Phase4(declaration, goodsItem);

		CreateDeclarationPackages(declaration, goodsItem.Packages, declarationInvoiceLine);
		CreateDeclarationContainers_Phase4(declaration, goodsItem, declarationInvoiceLine.JI_LineNo);
		CreateDeclarationSupportingDocuments(goodsItem.SupportingDocuments, declarationInvoiceLine);
		CreatePreviousDocuments(declarationInvoiceLine, isNew, secuentialNumberForNew, secuentialNumberForOthers);
	}

	JobComInvoiceLine CreateInvoiceLine_Phase4(JobDeclaration declaration, NctsArrivalAndUnloadingCargoDesc goodsItem)
	{
		var declarationInvoiceLine = declaration.InvoiceLines.AddNew();

		declarationInvoiceLine.JI_Weight = goodsItem.BY_GrossWeight;
		declarationInvoiceLine.JI_NetWeight = goodsItem.BY_NetWeight;
		declarationInvoiceLine.JI_Tariff = goodsItem.BY_HarmonisedTariff;
		declarationInvoiceLine.JI_Description = goodsItem.BY_Description;

		return declarationInvoiceLine;
	}

	void CreateInvoiceHeader(JobDeclaration declaration)
	{
		var arrivalGoodsItems = Bills
									.Where(bill => bill.UnloadedStatus != NctsUnloadedStateList.Codes.MIS)
									.SelectMany(bill => bill.ArrivalGoodsItems.Where(g => g.UnloadedStatus != NctsUnloadedStateList.Codes.MIS));
		bool IsN380Doc(NctsSupportingDocument doc) => doc.CSI_Code == SupportingDocumentHelper.SupportingDocumentCodeN380 && !doc.CSI_ReferenceNumber.IsEmpty;
		var supportingDocument = ArrivalMovementHeader.SupportingDocuments.FirstOrDefault(IsN380Doc) ?? Bills.SelectMany(bill => bill.SupportingDocuments).FirstOrDefault(IsN380Doc) ?? arrivalGoodsItems.SelectMany(items => items.SupportingDocuments).FirstOrDefault(IsN380Doc);

		var invoiceNumber = IsPhase4 ? LocalReferenceNumber : supportingDocument?.CSI_ReferenceNumber ?? LocalReferenceNumber;
		if (!invoiceNumber.IsEmpty && declaration.Invoices.Count == 0)
		{
			var declarationInvoice = declaration.Invoices.AddNew();
			declarationInvoice.JZ_InvoiceNumber = invoiceNumber;
			declarationInvoice.JZ_InvoiceDate = ZDateTime.Today;
		}
	}

	void CreateDeclarationPackages(JobDeclaration declaration, IEnumerable<EU.NCTS.Business.NctsPackage> packages, JobComInvoiceLine declarationInvoiceLine)
	{
		var invoiceNumber = LocalReferenceNumber;
		var packageList = new List<BasePackage>();
		foreach (var package in packages)
		{
			if (!invoiceNumber.IsEmpty)
			{
				BasePackage cw;
				if (!declaration.Bills.Cast<Bill>().Any(x => x.CU_BillNum.Contains(invoiceNumber)))
				{
					cw = declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
					cw.Bill.CU_BillNum = invoiceNumber;
					cw.Bill.CU_BillType = BillTypeList.Codes.HouseBill;
				}
				else
				{
					cw = declaration.Packages.AddNew();
				}
				CopyValuesToPackage(cw, package);
				packageList.Add(cw);
				if (IsPhase5)
				{
					CreateDeclarationContainers_Phase5(declaration, package, declarationInvoiceLine.JI_LineNo);
				}
			}
		}
		LinkInvoiceLineWithPackage(declarationInvoiceLine, packageList);
	}

	void CopyValuesToPackage(BasePackage cw, EU.NCTS.Business.NctsPackage packages)
	{
		cw.CW_PackType = packages.B5_UnitType;
		cw.CW_PackQty = packages.B5_UnitCount.ToZInt();
		cw.CW_MarksAndNos = packages.B5_MarksAndNumbers;
		cw.CW_HouseBill = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill + ":" + LocalReferenceNumber;
	}

	void LinkInvoiceLineWithPackage(JobComInvoiceLine declarationInvoiceLine, List<BasePackage> packages)
	{
		foreach (BaseCusLinkPackage linkPackage in declarationInvoiceLine.PackagesForInvoiceLinesForBindingOnly)
		{
			if (packages.Contains(linkPackage.Package))
			{
				linkPackage.IsLinked = true;
			}
			else
			{
				linkPackage.IsLinked = false;
			}
		}
	}

	void LinkPackagesWithUniqueContainer(JobDeclaration declaration)
	{
		foreach (JobComInvoiceLine invoicesLine in declaration.InvoiceLines)
		{
			foreach (NonPersistentCusContainer containers in invoicesLine.ContainersForInvoiceLinesForBindingOnly)
			{
				if (containersList.Where(cont => cont.Item1 == containers.ContainerNumber).Count() == 1 &&
					containersList.Where(cont => cont.Item1 == containers.ContainerNumber && cont.Item2 == invoicesLine.JI_LineNo).Count() == 1)
				{
					containers.IsForInvoiceLine = true;
				}
			}
		}
	}

	void CreateDeclarationContainers_Phase4(JobDeclaration declaration, NctsArrivalAndUnloadingCargoDesc goodsItem, ZShort lineInvoice)
	{
		foreach (EU.NCTS.Business.NctsContainer containers in goodsItem.Containers)
		{
			if ((!containers.ContainerNumber.IsEmpty) && (!declaration.CusContainers.Any(y => y.CO_ContainerNumber.Contains(containers.ContainerNumber))))
			{
				var declarationContainer = declaration.CusContainers.AddNew();
				declarationContainer.CO_ContainerNumber = containers.ContainerNumber;
			}
			if (!containers.ContainerNumber.IsEmpty)
			{
				containersList.Add(new Tuple<string, short>(containers.ContainerNumber.ToUpper(), lineInvoice));
			}
		}
	}

	void CreateDeclarationSupportingDocuments(IEnumerable<EU.NCTS.Business.NctsSupportingDocument> supportingDocuments, JobComInvoiceLine declarationInvoiceLine)
	{
		foreach (var supportingDocument in supportingDocuments)
		{
			var declarationInvoiceLinesSupportingDocuments = declarationInvoiceLine.SupportingDocuments.AddNew();
			declarationInvoiceLinesSupportingDocuments.CSI_Code = supportingDocument.CSI_Code;
			declarationInvoiceLinesSupportingDocuments.CSI_ReferenceNumber = supportingDocument.CSI_ReferenceNumber;
		}
	}

	void CreatePreviousDocuments(JobComInvoiceLine declarationInvoiceLine, ZBool isNew, int secuentialNumberForNew, int declarationGoodsItemNumber)
	{
		var declarationInvoiceLinesPreviousDocuments = declarationInvoiceLine.PreviousDocuments.AddNew();
		declarationInvoiceLinesPreviousDocuments.CSI_Code = NctsPreviousDocumentTypeCodeList.Codes.SumDocument;
		declarationInvoiceLinesPreviousDocuments.CSI_SubType = EU.Business.PreviousDocumentClassList.Codes.SummaryDeclaration;
		declarationInvoiceLinesPreviousDocuments.CSI_LineNo = isNew ? secuentialNumberForNew : declarationGoodsItemNumber;

		var previousSumaryDeclaration = CusEntryNumber.Load(this, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
		declarationInvoiceLinesPreviousDocuments.CSI_ReferenceNumber = previousSumaryDeclaration?.CE_EntryNum ?? ZString.Empty;
	}

	ZGuid CreateEXSFromArrival_Phase5()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = Customs.EU.Business.MessageTypeList.Codes.Export;
		declaration.JE_EntryStyle = Customs.EU.Business.EntryStyleListExport.Codes.ExportNormal;
		declaration.JE_CustomsOffice = ArrivalMovementHeader.DestinationCustomsOfficeCode;
		declaration.JE_LocationOfGoods = ArrivalMovementHeader.GoodsLocation.AdditionalIdentifier;
		declaration.JE_OH_Supplier = DestinationTrader.OrganisationPK;

		var entryInstructions = declaration.CustomsEntryInstructions.AddNew();
		entryInstructions.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;

		containersList.Clear();

		var sequentialNumberForNew = 99000;
		var arrivalGoodsItems = Bills
									.Where(bill => bill.UnloadedStatus != NctsUnloadedStateList.Codes.MIS)
									.SelectMany(bill => bill.ArrivalGoodsItems.Where(g => g.UnloadedStatus != NctsUnloadedStateList.Codes.MIS));
		foreach (var goodsItem in arrivalGoodsItems)
		{
			if (goodsItem.UnloadedStatus == NctsUnloadedStateList.Codes.NEW)
			{
				sequentialNumberForNew++;
			}
			CreateDeclarationBody_Phase5(declaration, goodsItem, sequentialNumberForNew);
		}

		LinkPackagesWithUniqueContainer(declaration);

		if (declaration.CusContainers.Count > 0)
		{
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
		}

		var invoiceNumber = LocalReferenceNumber;
		if (!invoiceNumber.IsEmpty)
		{
			declaration.JE_HouseBill = invoiceNumber;
		}

		return declaration.PK;
	}

	void CreateDeclarationBody_Phase5(JobDeclaration declaration, NctsArrivalCargoDesc goodsItem, int sequentialNumberForNew)
	{
		CreateInvoiceHeader(declaration);

		var declarationInvoiceLine = CreateInvoiceLine_Phase5(declaration, goodsItem);

		CreateDeclarationPackages(declaration, goodsItem.Packages.Where(packages => packages.UnloadedStatus != NctsUnloadedStateList.Codes.MIS), declarationInvoiceLine);
		CreatePreviousDocuments(declarationInvoiceLine, goodsItem.UnloadedStatus == NctsUnloadedStateList.Codes.NEW, sequentialNumberForNew, goodsItem.BY_DeclarationGoodsItemNumber);
	}

	JobComInvoiceLine CreateInvoiceLine_Phase5(JobDeclaration declaration, NctsArrivalCargoDesc goodsItem)
	{
		var declarationInvoiceLine = declaration.InvoiceLines.AddNew();
		EU.NCTS.Business.NctsCommonCargoDesc unloadedGoodsItem = goodsItem.UnloadedStatus == NctsUnloadedStateList.Codes.DIF ? goodsItem.UnloadedGoodsItem : goodsItem;
		declarationInvoiceLine.JI_Weight = unloadedGoodsItem.BY_GrossWeight;
		declarationInvoiceLine.JI_NetWeight = unloadedGoodsItem.BY_NetWeight;
		declarationInvoiceLine.JI_Tariff = unloadedGoodsItem.BY_HarmonisedTariff;
		declarationInvoiceLine.JI_Description = unloadedGoodsItem.BY_Description;

		return declarationInvoiceLine;
	}

	void CreateDeclarationContainers_Phase5(JobDeclaration declaration, EU.NCTS.Business.NctsPackage packages, ZShort lineInvoice)
	{
		var packageContainers = packages.ContainersPivotsForBindingOnly
									.Select(x => x.Container)
									.Where(x => !x.BC_ContainerNum.IsEmpty && x.BC_UnloadedState != NctsUnloadedStateList.Codes.MIS && x.BC_Mode == Core.Constants.ContainerModes.Containerised);
		foreach (var containers in packageContainers)
		{
			if (!declaration.CusContainers.Any(x => x.CO_ContainerNumber.Contains(containers.BC_ContainerNum)))
			{
				var declarationContainer = declaration.CusContainers.AddNew();
				declarationContainer.CO_ContainerNumber = containers.BC_ContainerNum;
			}

			containersList.Add(new (containers.BC_ContainerNum.ToUpper(), lineInvoice));
		}
	}

	#region EDocPivotCollection

	Type ICusStorageDocPivotTypeSupporter.CusStorageDocPivotType => typeof(NctsCusStorageDocPivot);

	void ICusStorageDocPivotTypeSupporter.ReloadCollection()
	{
		if (IsEDocPivotCollectionLoaded)
		{
			EDocPivotCollection.Reload(true);
		}
	}

	[ChildEditable(true)]
	public NctsCusStorageDocPivotCollection EDocPivotCollection
	{
		get
		{
			if (eDocPivotCollection == null)
			{
				eDocPivotCollection = new NctsCusStorageDocPivotCollection(this);
				eDocPivotCollection.Load();
				RegisterEditableChildObject(eDocPivotCollection);
			}

			return eDocPivotCollection;
		}
	}
	NctsCusStorageDocPivotCollection eDocPivotCollection;

	bool IsEDocPivotCollectionLoaded => eDocPivotCollection != null && eDocPivotCollection.IsLoaded;

	NctsCusStorageDocPivotCollection INctsCusStorageDocPivotParent.EDocPivotCollection => EDocPivotCollection;

	IEnumerable<IStorageDocsBaseCollection> ICusStorageDocPivotTypeSupporter.EDocCollections
	{
		get
		{
			var arrivalHeaderForTNN = MovementHeader?.ArrivalHeaderForTNN;
			var eDocCollectionList = arrivalHeaderForTNN != null ? EDocsHelper.GetEDocCollections(arrivalHeaderForTNN, arrivalHeaderForTNN.Shipment) : EDocsHelper.GetEDocCollections(this, Shipment);
			foreach (var eDocCollection in eDocCollectionList)
			{
				yield return eDocCollection;
			}
		}
	}

	public List<NctsCusStorageDocPivot> GetAllSendableEDocPivots()
	{
		return EDocPivotCollection.Where(p =>
									p.MessageStatus == ZString.Empty
									|| p.MessageStatus == EDIMessageStatusList.Codes.Failed
									|| p.MessageStatus == EDIMessageStatusList.Codes.Error
									|| p.MessageStatus == EDIMessageStatusList.Codes.Rejected
								).ToList();
	}

	ZBool HasAnnexes => EDocPivotCollection != null && EDocPivotCollection.Count > 0;

	public ZBool HasAnnexesSentWithoutResponse() => HasAnnexes && EDocPivotCollection.Cast<CusStorageDocPivot>().Any(x => x.IsSentWithoutResponse);

	public ZBool RequiresAnnexes() => IsDepartureMovement
									&& MovementHeader.IsPhaseStatusTNN
									|| (!MovementReferenceNumber.IsEmpty
										&& (ClearanceReferenceNumber.IsEmpty || HasAnnexesAccepted() || HasAnnexesSentWithoutResponse()));

	ZBool HasAnnexesAccepted() => HasAnnexes && EDocPivotCollection.Cast<CusStorageDocPivot>().Any(x => x.IsAccepted);

	#endregion

	protected override Type DepartureContainerTypeCore => typeof(NctsDepartureHeaderContainer);

	protected override ShortSequenceNumberGenerator HeaderContainersLineNumberGeneratorCore => new NctsHeaderContainerSequenceNumberGenerator(() => DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>());

	protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
	{
		var nctsHeader = (NctsHeader)base.CloneInternal(args);
		if (IsPhase5Arrival)
		{
			CloneNCTS5ArrivalMovement(nctsHeader);
		}
		return nctsHeader;
	}

	void CloneNCTS5ArrivalMovement(NctsHeader newDeclaration)
	{
		var esHeader = ESNctsHeader;
		var newESHeader = newDeclaration.ESNctsHeader;
		newESHeader.CEN_AutomaticCompletion = esHeader.CEN_AutomaticCompletion;
		newESHeader.CEN_AutomaticTranshipment = esHeader.CEN_AutomaticTranshipment;
		newESHeader.CEN_TIRArrival = esHeader.CEN_TIRArrival;
		newESHeader.CEN_TIRPartialUnloading = esHeader.CEN_TIRPartialUnloading;
		newESHeader.CEN_SummaryType = esHeader.CEN_SummaryType;

		newDeclaration.ArrivalMovementHeader.Representative.E2_OA_Address = ArrivalMovementHeader.Representative.E2_OA_Address;
	}

	public ZBool IsPhaseStatusTNN => (MovementHeader?.IsPhaseStatusTNN ?? false)
									|| (ArrivalMovementHeader?.BM_Phase ?? ZString.Empty) == ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;

	public ZBool IsSent => EffectiveMessageStatus == LogicalStatusList.Codes.Sent;

	protected override void ValidateConsignee(JobDocAddressValidation validation)
	{
		base.ValidateConsignee(validation);

		if (IsPhase5 && IsPhaseStatusTNN && Consignee.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Consignee.OrganisationPKInfo, Res.GetString("D6DE35AC-9878-4786-AF9A-263E9511E66C", "value in Arrival Notification/Arrival Details/Destination Trader"));
		}
	}

	public bool IsMovementReferenceNumberES => MovementReferenceNumber.SubstringSafe(2, 2) == Enterprise.Core.Constants.CountryCodes.Spain;
}
