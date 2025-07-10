using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CH.NCTS.Business.UniversalReferenceConstants;
using EUNcts = Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeader : EUNcts.NctsHeader, ICusEntryNumberParent, INctsAdditionalInfoParent
{
	public NctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ResourceStringData("CH.NctsHeader.BH_CommunicationLanguage", Caption = "Language", FullDescription = "Language used to communicate with customs")]
	public override ZString BH_CommunicationLanguage { get => base.BH_CommunicationLanguage; set => base.BH_CommunicationLanguage = value; }

	public override ZString DefaultDataGroupingCode => Core.Constants.CountryCodes.Switzerland;

	protected override ZString DefaultApplicationCode => CusInBondApplicationCodeList.Codes.NCTS5;

	public ZString ArrivalReferenceNumber => GetArrivalReferenceEntryNumber(false)?.CE_EntryNum ?? ZString.Empty;

	public CusEntryNumber ArrivalReferenceEntryNumber => GetArrivalReferenceEntryNumber(true);

	CusEntryNumber GetArrivalReferenceEntryNumber(bool createIfNotExists)
	{
		if (arnEntryNumber == null || arnEntryNumber.IsDeleted)
		{
			arnEntryNumber = (createIfNotExists ? CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.EU.ArrivalReferenceNumber, CountryCode) : CusEntryNumber.Load(this, CusEntryNumberTypes.EU.ArrivalReferenceNumber, CountryCode));
			if (arnEntryNumber != null)
			{
				RegisterEditableChildObject(arnEntryNumber);
			}
		}

		return arnEntryNumber;
	}
	CusEntryNumber arnEntryNumber;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		SetDefaultCommunicationLanguage();
	}

	void SetDefaultCommunicationLanguage()
	{
		var language = TranslationHelper.GetCurrentLanguageCode();
		if (!Lookups.CommunicationLanguageList.ContainsCode(language))
		{
			language = SwissCustomsLanguageList.Codes.German;
		}
		BH_CommunicationLanguage = language;
	}

	public new NctsBillCollection Bills => (NctsBillCollection)base.Bills;
	protected override EUNcts.INctsBillCollection<EUNcts.NctsBill> GetNewBillCollection() => new NctsBillCollection(this);
	protected override Type BillTypeCore => typeof(NctsBill);

	protected override EUNcts.NctsGuaranteeRefresher GetNewGuaranteeRefresher()
	{
		return new NctsGuaranteeRefresher(this);
	}

	class NctsGuaranteeRefresher : EUNcts.NctsGuaranteeRefresher
	{
		readonly NctsHeader nctsHeader;

		public NctsGuaranteeRefresher(NctsHeader nctsHeader) : base(nctsHeader)
		{
			this.nctsHeader = nctsHeader;
		}

		protected override void CreateNctsGuaranteeFromPrincipalGuarantee(CusGuaranteeHeader guarantee)
		{
			if (nctsHeader.IsDepartureMovement)
			{
				var guarantees = nctsHeader.MovementHeader.Guarantees;
				if (guarantees.Count == 0)
				{
					var nctsGuarantee = guarantees.AddNew();
					nctsGuarantee.PW_BondType = guarantee.CPH_SubType;
					nctsGuarantee.PW_BondNumber = guarantee.CPH_Number;
					nctsGuarantee.PW_Password = guarantee.MainAccessCode.Left(CusBondDetail.Schema.PW_PasswordMaxLength);
				}
				else if (guarantees.Count(x => nctsHeader.Principal.OrganisationPK == guarantee.CPH_OH_PermitHolder && x.PW_BondType == guarantee.CPH_SubType) == 1)
				{
					var nctsGuarantee = guarantees.Cast<NctsGuarantee>().FirstOrDefault(x => nctsHeader.Principal.OrganisationPK == guarantee.CPH_OH_PermitHolder && x.PW_BondType == guarantee.CPH_SubType);
					nctsGuarantee.PW_BondNumber = guarantee.CPH_Number;
					nctsGuarantee.PW_Password = guarantee.MainAccessCode.Left(CusBondDetail.Schema.PW_PasswordMaxLength);
				}
			}
		}

		protected override ZBool ManageMultipleGuarantees => false;
	}

	public new ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActors => (ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>)base.CusSupplyChainActors;
	protected override ICusSupplyChainActorReferenceCollection<EUNcts.CusSupplyChainActorReference> GetCusSupplyChainActorsCore() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

	public new NctsHeaderLookups Lookups => (NctsHeaderLookups)base.Lookups;

	public new NctsHeaderPhase5Validation Validation => (NctsHeaderPhase5Validation)base.Validation;

	public new NctsDepartureMovementHeader MovementHeader => (NctsDepartureMovementHeader)base.MovementHeader;

	public new NctsArrivalMovementHeader ArrivalMovementHeader => (NctsArrivalMovementHeader)base.ArrivalMovementHeader;

		protected override EUNcts.NctsDepartureMovementHeader GetNewDepartureMovementHeader() => EUNcts.NctsCommonMovementHeader.LoadOrCreate<NctsDepartureMovementHeader>(this, Common.EU.NctsMoveHeaderType.Codes.Departure);

	protected override CusInBondHeaderLookups GetNewLookups() => new NctsHeaderLookups(this);

	protected override CusInBondHeaderValidation GetNewPhase5Validation() => new NctsHeaderPhase5Validation(this);

	public new NctsEuOfficeCodeCollectionForDepartureGrid CustomsOfficesForDeparture => (NctsEuOfficeCodeCollectionForDepartureGrid)base.CustomsOfficesForDeparture;

	public new NctsArrivalHeaderContainerCollection ArrivalHeaderContainers => (NctsArrivalHeaderContainerCollection)base.ArrivalHeaderContainers;

	protected override EUNcts.NctsArrivalHeaderContainerCollection GetArrivalHeaderContainersCore() => new NctsArrivalHeaderContainerCollection(this);

	protected override Type ArrivalContainerTypeCore => typeof(NctsArrivalHeaderContainer);

	protected override EUNcts.NctsEuOfficeCodeCollectionForDepartureGrid GetNewCustomsOfficesForDeparture() => new NctsEuOfficeCodeCollectionForDepartureGrid(this);

	protected override EUNcts.INctsDefaultTraderAtDestinationManager GetNewDefaultTraderAtDestinationManager() => new NctsDefaultTraderAtDestinationManager(this);

	protected override EUNcts.ICommonPreviousDocumentCollection<EUNcts.CommonPreviousDocument> GetPreviousDocuments() => new EUNcts.CommonPreviousDocumentCollection<CommonPreviousDocument>(this);

	public void MovementReferenceNumberSetter(ZString mrn, ZDateTime? issueDate = null, ZDateTime? expiryDate = null)
	{
		var cusEntryNumber = CusEntryNumber.LoadOrCreate(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, CountryCode);
		cusEntryNumber.CE_EntryIsSystemGenerated = true;
		cusEntryNumber.CE_EntryNum = mrn;
		if (issueDate.HasValue)
		{
			cusEntryNumber.CE_IssueDate = issueDate.Value;
		}

		if (expiryDate.HasValue)
		{
			cusEntryNumber.CE_ExpiryDate = expiryDate.Value;
		}
	}

	#region ICusEntryNumberParent

	bool ICusEntryNumberParent.CanBeChangedOrDeleted(CusEntryNumber entryNumber, out string errMsg)
	{
		errMsg = string.Empty;
		return string.IsNullOrEmpty(errMsg);
	}

	void ICusEntryNumberParent.EntryNumberChanged(ZString oldValue, ZString newValue)
	{
		MovementHeader?.MarkAsNeedingValidation();
	}

	string ICusEntryNumberParent.EntryNumberChangedCallStack => ZString.Empty;

	#endregion

	protected override bool DestinationCustomsOfficeCodeForArrivalReadOnlyCore => true;

	protected override bool BH_ExportFlagReadOnlyCore => true;

	protected override bool IsUnloadingAllowedOrCompleteCore => base.IsUnloadingAllowedOrCompleteCore && (!ArrivalMovementHeader?.MultipleMRNIndicator ?? true);

	protected override bool DestinationTraderReadOnlyCore => IsArrivalMovement;

	public ZBool IsNationalTransit => MovementReferenceNumber.SubstringSafe(2, 2) == Core.Constants.CountryCodes.Switzerland && MovementReferenceNumber.SubstringSafe(16, 1) == "N";

	[LightValidationTestExempt]
	public override ZString BH_HeaderType { get => base.BH_HeaderType; set => base.BH_HeaderType = value; }

	public bool IsLinkedExport => Factory.GetCached(ref isLinkedExport, () => PreviousDocuments.Cast<EUNcts.CommonPreviousDocument>().Any(x => x.CSI_Code == PreviousDocumentCodes.Export));
	CachedProperty<bool> isLinkedExport;

	public bool IsLinkedOrRelatedExport => IsLinkedExport || MovementHeader?.RelatedExportEntryHeaders.Count > 0;

	public bool IsMessageStatusSent => EffectiveMessageStatus.ToString() is CHLogicalStatusList.Codes.Sent or CHLogicalStatusList.Codes.Acknowledged;

	public int PreviousDocumentsEXPOCount => Factory.GetCached(ref previousDocumentsEXPOCount, () => PreviousDocuments.Count(y => y.CSI_Code == PreviousDocumentCodes.Export));
	CachedProperty<int> previousDocumentsEXPOCount;

	public int GoodsItemsCount => Factory.GetCached(ref goodsItemsCount, () => Bills.Sum(y => y.GoodsItems.Count));
	CachedProperty<int> goodsItemsCount;

	protected override void ValidatePrincipalTrader(JobDocAddressValidation validation)
	{
		base.ValidatePrincipalTrader(validation);
		if (IsDepartureMovement)
		{
			PassarValidation.CheckNS30128(Principal.OrganisationPKInfo, Principal);
		}
	}

	public new EUNcts.INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalDocuments => (EUNcts.INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalDocuments;
	protected override EUNcts.INctsAdditionalInfoCollection<EUNcts.NctsAdditionalInfo> GetAdditionalDocuments() => new EUNcts.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);

	protected override bool ExistNonDECEntryCore
	{
		get
		{
			var result = base.ExistNonDECEntryCore;
			if (!result)
			{
				result = ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(x => x.BC_UnloadedState != EUNcts.NctsUnloadedStateList.Codes.DEC || x.Seals.Cast<CusSeal>().Any(x => x.BK_UnloadingState != EUNcts.NctsUnloadedStateList.Codes.DEC && x.BK_UnloadingState != EUNcts.NctsUnloadedStateList.Codes.DAM));
			}
			return result;
		}
	}

	public ZBool IsNationalTransitSwitzerland => MovementHeader?.IsNationalTransitSwitzerland ?? false;

	protected override void SetAllUnloadedStateToDECCore()
	{
		base.SetAllUnloadedStateToDECCore();

		var objectsToDelete = new List<BusinessObject>();
		ProcessAllContainers();
		objectsToDelete.ForEach(x => x.Delete());

		void ProcessAllContainers()
		{
			foreach (var container in ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>())
			{
				switch (container.BC_UnloadedState)
				{
					case EUNcts.NctsUnloadedStateList.Codes.NEW:
						objectsToDelete.Add(container);
						break;
					case EUNcts.NctsUnloadedStateList.Codes.DIF:
						container.BC_UnloadedState = EUNcts.NctsUnloadedStateList.Codes.DEC;
						RestoreOriginalValue(container.BC_ModeInfo);
						RestoreOriginalValue(container.BC_ContainerNumInfo);
						ProcessAllSeals(container);
						break;
					case EUNcts.NctsUnloadedStateList.Codes.MIS:
						container.BC_UnloadedState = EUNcts.NctsUnloadedStateList.Codes.DEC;
						ProcessAllSeals(container);
						break;
					default:
						ProcessAllSeals(container);
						break;
				}
			}
		}

		void ProcessAllSeals(NctsArrivalHeaderContainer container)
		{
			foreach (var seal in container.Seals.Cast<CusSeal>())
			{
				switch (seal.BK_UnloadingState)
				{
					case EUNcts.NctsUnloadedStateList.Codes.NEW:
						objectsToDelete.Add(seal);
						break;
					case EUNcts.NctsUnloadedStateList.Codes.DIF:
						RestoreOriginalValue(seal.BK_SealNumberInfo);
						seal.BK_UnloadingState = EUNcts.NctsUnloadedStateList.Codes.DEC;
						break;
					case EUNcts.NctsUnloadedStateList.Codes.MIS:
						seal.BK_UnloadingState = EUNcts.NctsUnloadedStateList.Codes.DEC;
						break;
				}
			}
		}

		void RestoreOriginalValue(ZPropertyInfo property)
		{
			property.Value = property.OriginalValue;
		}
	}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypes = base.GetCusSupportingInfoTypes();
			cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(NctsAdditionalInfo);
			cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(CommonPreviousDocument);
			return cusSupportingInfoTypes;
		}

	public static NctsHeader GetLinkedNctsHeader(BusinessObject linkedObject)
	{
		return linkedObject switch
		{
			NctsHeader nctsHeader => nctsHeader,
			NctsDepartureMovementHeader nctsMovementHeader => nctsMovementHeader.Header,
			_ => null
		};
	}

	protected override ZDateTime MovementReferenceIssueDateCore => GetArrivalReferenceEntryNumber(false)?.CE_ExpiryDate ?? base.MovementReferenceIssueDateCore;

	public new class Loader : BusinessObject.Loader
	{
		public Loader(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override Type GetTypeOfBusinessObjectToLoad() => typeof(NctsHeader);

		public NctsHeader FindByMovementReferenceNumber(string movementType, string mrn, string mrnVersion)
		{
			return FindByMovementReferenceNumber(movementType, mrn.AppendEntryNumVersion(mrnVersion));
		}

		public NctsHeader FindByMovementReferenceNumber(string movementType, ZString movementReferenceNumber)
		{
			var headerQuery = GetFindByEntryNumberQuery(movementType, CusEntryNumberTypes.Standard.MovementReferenceNumber, movementReferenceNumber);
			return Factory.LoadTop1<NctsHeader>(headerQuery);
		}

		public NctsHeader[] FindByMovementReferenceNumbers(string movementType, ZString[] movementReferenceNumbers)
		{
			var headerQuery = GetFindByEntryNumberQuery(movementType, CusEntryNumberTypes.Standard.MovementReferenceNumber, movementReferenceNumbers);
			return Factory.Load<NctsHeader>(headerQuery);
		}

		public NctsHeader FindByMovementReferenceNumberAnyVersion(string movementType, ZString mrn)
		{
			if (mrn.IsEmpty)
			{
				return null;
			}
			var entryNumberQuery = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrn).AddToFilter(JoinCondition.Or, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, mrn + ".");
			var headerQuery = GetFindByEntryNumberQuery(movementType, CusEntryNumberTypes.Standard.MovementReferenceNumber, entryNumberQuery);
			return Factory.LoadTop1<NctsHeader>(headerQuery);
		}

		public NctsHeader FindByArrivalReferenceNumber(ZString arrivalReferenceNumber)
		{
			if (arrivalReferenceNumber.IsEmpty)
			{
				return null;
			}
			var headerQuery = GetFindByEntryNumberQuery(EUNcts.NctsMovementType.Codes.Arrival, CusEntryNumberTypes.EU.ArrivalReferenceNumber, arrivalReferenceNumber);
			return Factory.LoadTop1<NctsHeader>(headerQuery);
		}

		static ZQuery GetFindByEntryNumberQuery(ZString movementType, ZString entryType, params ZString[] entryNumbers)
		{
			var validEntryNumbers = entryNumbers.Where(x => !x.IsEmpty);
			return GetFindByEntryNumberQuery(movementType, entryType, !validEntryNumbers.Any() ? ZQuery.NoResultQuery : new ZQuery(CusEntryNumSchema.CE_EntryNum, validEntryNumbers));
		}

		static ZQuery GetFindByEntryNumberQuery(ZString movementType, ZString entryType, ZQuery entryNumberQuery)
		{
			if (movementType.IsEmpty || entryType.IsEmpty || entryNumberQuery.IsNoResultQuery)
			{
				return ZQuery.NoResultQuery;
			}

			var headerQuery = GetNctsHeaderQuery(movementType);

			var cusEntryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusInBondHeader.Schema.TableName);
			cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Code);
			cusEntryNumSubQuery.AddToFilter(entryNumberQuery);
			headerQuery.AddSubQuery(cusEntryNumSubQuery, JoinCondition.And);

			return headerQuery;
		}

		public static ZDBOnlyQuery GetNctsHeaderQuery(string movementType)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.NCTS5);
			headerQuery.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, movementType);

			var branchesQuery = new ZDBOnlySubQuery(typeof(GlbBranch), CusInBondHeaderSchema.BH_GB);
			branchesQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			headerQuery.AddSubQuery(branchesQuery, JoinCondition.And);

			return headerQuery;
		}
	}
}
