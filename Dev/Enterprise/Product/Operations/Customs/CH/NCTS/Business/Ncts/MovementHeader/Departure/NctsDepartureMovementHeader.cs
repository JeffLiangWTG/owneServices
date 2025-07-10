using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsDepartureMovementHeader : EU.NCTS.Business.NctsDepartureMovementHeader, Integration.Customs.CH.IDepartureMovementHeader, ICusGoodsLocationProvider, IAdditionalBusinessObjectFetchStrategyProvider
{
	public NctsDepartureMovementHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override void OnSaving()
	{
		base.OnSaving();
		if (IsNationalTransitSwitzerland)
		{
			Header?.Bills.ForEach(b =>
			{
				b.GoodsItems.ForEach(g => g.CusSupplyChainActorReferences.RemoveAndDeleteAll());
				b.CusSupplyChainActorReferences.RemoveAndDeleteAll();
			});
			CusSupplyChainActors.RemoveAndDeleteAll();
			Header?.CountriesOfRouting.RemoveAndDeleteAll();
		}
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		SetDefaultRepresentative();
	}

	protected override EDIMessageCollection GetNewMessageCollection() => new EDIMessageCollection(this, Factory);

	void SetDefaultRepresentative()
	{
		var orgHeader = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;
		if (orgHeader != null)
		{
			using (Representative.GetValidationSuspender())
			{
				Representative.OrganisationPK = orgHeader.PK;
			}
		}
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	public new CusGoodsLocation GoodsLocation => (CusGoodsLocation)base.GoodsLocation;

	protected override IDictionary<ZString, Type> GetCusCodeDataTypesCore()
	{
		var result = base.GetCusCodeDataTypesCore();
		result[EU.Business.CusCodeDataTypeList.Codes.OfficeCode] = typeof(NctsEuOfficeCode);
		return result;
	}

	protected override NctsDepartureMovementHeaderPhase5Validation GetNewPhase5Validation() => new NctsDepartureMovementHeaderValidation(this);

	protected override CusInBondMoveHeaderLookups GetNewPhase5Lookups() => new NctsDepartureMovementHeaderLookups(this);

	protected override CusInBondMoveHeaderLookups GetNewPhase4Lookups() => new NctsDepartureMovementHeaderLookups(this);

	public new NctsDepartureMovementHeaderLookups Lookups => (NctsDepartureMovementHeaderLookups)base.Lookups;

	public new NctsEuOfficeCodeCollectionForDepartureGrid CustomsOfficesForDeparture => (NctsEuOfficeCodeCollectionForDepartureGrid)base.CustomsOfficesForDeparture;

	protected override EU.NCTS.Business.NctsEuOfficeCodeCollectionForDepartureGrid GetNewCustomsOfficesForDeparture() => new NctsEuOfficeCodeCollectionForDepartureGrid(this);

	protected override CustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new NctsMovementHeaderCustomsOfficeRequirementHelper(this);

	public new INctsGuaranteeCollection<NctsGuarantee> Guarantees => (INctsGuaranteeCollection<NctsGuarantee>)base.Guarantees;

	protected override INctsGuaranteeCollection<EU.NCTS.Business.NctsGuarantee> GetGuaranteesCore() => new NctsGuaranteeCollection<NctsGuarantee>(this);

		public new INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;

		protected override INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetSupportingDocuments() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypes = base.GetCusSupportingInfoTypes();
			cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(NctsSupportingDocument);
			return cusSupportingInfoTypes;
		}

		protected override ZString GenerateLocalReferenceNumberCore() => CHLRNGeneratorHelper.GenerateLocalReferenceNumber(Factory, LrnNumberFountain);

	protected override INumberFountainProxy LrnNumberFountain => Env.NumberFountains.CHLocalReferenceNumber(GlbCompany.CurrentCompany.PK.ToGuid());

	public RefUNLOCO PlaceOfUnLoading => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, BM_PlaceOfUnloading);

	public ZDateTime ValuationDate => BM_ValuationDate.IsValid ? BM_ValuationDate : ZDateTime.Today;

	public bool IsNationalTransitSwitzerland => BM_InBondEntryType == NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;

	ZString ICusGoodsLocationProvider.ProviderKey => DefaultDataGroupingCode + Customs.Business.GoodsLocationProviderApplications.Codes.NCTSMovement;

	protected override bool IsRepresentativeReadOnly => true;

	[MaxLength(3)]
	public override ZString BM_SpecificCircumstance
	{
		get => base.BM_SpecificCircumstance;
		set => base.BM_SpecificCircumstance = value;
	}

	[MaxLength(2)]
	public override ZShort BM_ExportTimeLimit
	{
		get => base.BM_ExportTimeLimit;
		set => base.BM_ExportTimeLimit = value;
	}

	public override ZDateTime BM_ValuationDate
	{
		get => base.BM_ValuationDate;
		set
		{
			var oldValue = BM_ValuationDate;
			base.BM_ValuationDate = value;
			if (BM_ValuationDate != oldValue)
			{
				Header?.MarkAsNeedingValidation();
				CustomsOffices.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString BM_MethodOfPayment
	{
		get => base.BM_MethodOfPayment;
		set
		{
			var oldValue = BM_MethodOfPayment;
			base.BM_MethodOfPayment = value;
			if (BM_MethodOfPayment != oldValue)
			{
				Header.Bills.ForEach(x => x.B0_TransportPaymentMethodInfo.RefreshBinding());
			}
		}
	}

	public override ZString BM_InBondEntryType
	{
		get => base.BM_InBondEntryType;
		set
		{
			var oldValue = BM_InBondEntryType;
			base.BM_InBondEntryType = value;
			if (!IsCopying && oldValue != BM_InBondEntryType)
			{
				SetDefaultBM_TypeOfSecurity();
				Header?.Bills.ForEach(b => b.GoodsItems.ForEach(g => BM_InBondEntryTypeChanged(g)));
			}

			void BM_InBondEntryTypeChanged(NctsDepartureCargoDesc goodsItem)
			{
				if (IsNationalTransitSwitzerland)
				{
					goodsItem.BY_RN_NKCountryOfDispatch = ZString.Empty;
					goodsItem.BY_RN_NKCountryOfDestination = ZString.Empty;
					goodsItem.BY_CusC4Number = ZString.Empty;
				}
				else
				{
					goodsItem.BY_RN_NKCountryOfDispatchInfo.RefreshBinding();
					goodsItem.BY_RN_NKCountryOfDestinationInfo.RefreshBinding();
					goodsItem.BY_CusC4NumberInfo.RefreshBinding();
				}
			}
		}
	}

	[ReadOnlyMember(nameof(IsNationalTransitSwitzerland))]
	public override ZString BM_TypeOfSecurity
	{
		get => base.BM_TypeOfSecurity;
		set
		{
			var oldValue = BM_TypeOfSecurity;
			base.BM_TypeOfSecurity = value;
			if (BM_TypeOfSecurity != oldValue)
			{
				Header?.CustomsOfficesForDeparture.MarkAsNeedingValidation();
				CustomsOfficesForDeparture.MarkAsNeedingValidation();
			}
		}
	}

	void SetDefaultBM_TypeOfSecurity()
	{
		if (IsNationalTransitSwitzerland)
		{
			BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		}
	}

	[ChildEditable(true)]
	public RelatedExportEntryHeaderGenPivotCollection RelatedExportEntryHeaders
	{
		get
		{
			if (relatedExportEntryHeaders == null)
			{
				relatedExportEntryHeaders = new RelatedExportEntryHeaderGenPivotCollection(this);
				relatedExportEntryHeaders.Load();
				RegisterEditableChildObject(relatedExportEntryHeaders);
			}

			return relatedExportEntryHeaders;
		}
	}
	RelatedExportEntryHeaderGenPivotCollection relatedExportEntryHeaders;

	protected override ICusSupplyChainActorReferenceCollection<EU.NCTS.Business.CusSupplyChainActorReference> GetCusSupplyChainActorsCore() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);

	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

	internal HugeSequenceNumberGenerator RelatedExportEntryHeadersLineNumberGenerator => relatedExportEntryHeadersLineNumberGenerator ?? (relatedExportEntryHeadersLineNumberGenerator = new HugeSequenceNumberGenerator(() => new TypedEnumerable<IHugeSequenceNumberLine>(RelatedExportEntryHeaders)));
	HugeSequenceNumberGenerator relatedExportEntryHeadersLineNumberGenerator;

	public bool IsUniformCountryOfDestination => Factory.GetCached(ref isUniformCountryOfDestination,
		() => Header.DepartureGoodsItems.Cast<NctsDepartureCargoDesc>().Select(x => x.EffectiveCountryCodeOfDestination).AllSame());
	CachedProperty<bool> isUniformCountryOfDestination;

	protected override ICusInBondMoveDetailCollection CreateMovementDetails() => new CusInBondMoveDetailCollection(this);

		public int RelatedExportMergedLinesCount => Factory.GetCached(ref relatedExportMergedLinesCount, () => RelatedExportEntryHeaders.Select(y => y.EntryHeader?.MergedLines.Count ?? 0).Sum());
		CachedProperty<int> relatedExportMergedLinesCount;

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}
	}
