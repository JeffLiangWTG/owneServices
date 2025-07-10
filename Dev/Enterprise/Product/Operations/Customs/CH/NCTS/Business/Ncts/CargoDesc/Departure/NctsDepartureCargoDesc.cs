using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using EUNcts = Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsDepartureCargoDesc : EUNcts.NctsDepartureCargoDesc
	, Integration.Customs.CH.IDepartureCargoDesc
	, INctsAdditionalInfoParent
	, IRestrictionParent
{
	public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new NctsDepartureMovementHeader MoveHeader => (NctsDepartureMovementHeader)base.MoveHeader;

	public new NctsDepartureCargoDescValidation Validation => (NctsDepartureCargoDescValidation)base.Validation;

	protected override NctsDepartureCargoDescPhase5Validation GetNewPhase5Validation() => new NctsDepartureCargoDescValidation(this);

	public new NctsDepartureCargoDescLookups Lookups => (NctsDepartureCargoDescLookups)base.Lookups;

	protected override NctsCommonCargoDescLookups GetNewPhase4Lookups() => new NctsDepartureCargoDescLookups(this);

	protected override NctsCommonCargoDescLookups GetNewPhase5Lookups() => new NctsDepartureCargoDescLookups(this);

	public new INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (NctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;
	protected override INctsSupportingDocumentCollection<EUNcts.NctsSupportingDocument> GetNewNctsSupportingDocumentCollection() => new NctsSupportingDocumentCollection<NctsSupportingDocument>(this);
	protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var cusSupportingInfoTypes = base.GetCusSupportingInfoTypes();
		cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.Restriction] = typeof(Restriction);
		return cusSupportingInfoTypes;
	}

	public new ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences => (ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>)base.CusSupplyChainActorReferences;
	protected override ICusSupplyChainActorReferenceCollection<EUNcts.CusSupplyChainActorReference> GetCusSupplyChainActorReferences() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
	protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

	public new NctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (NctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;
	protected override INctsAdditionalInfoCollection<EUNcts.NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);
	protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);

	public new NctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments => (NctsPreviousDocumentCollection<NctsPreviousDocument>)base.PreviousDocuments;
	protected override INctsPreviousDocumentCollection<EUNcts.NctsPreviousDocument> GetPreviousDocuments() => new NctsPreviousDocumentCollection<NctsPreviousDocument>(this);
	protected override Type PreviousDocumentType => typeof(NctsPreviousDocument);

	protected override ZString TariffTypeCore => Enterprise.Customs.Universal.Constants.TariffTypes.Export;

	protected override TariffFormatter GetNewTariffFormatter() => new TariffFormatterCH();

	protected override void ValidateConsignee(JobDocAddressValidation validation)
	{
	}

	protected override string CustomsSecondUnitQtySpecificUOM => Constants.UnitOfMeasureTypes.CustomsUOM3Type;

	public ZString EffectiveCountryCodeOfDestination
	{
		get
		{
			var result = BY_RN_NKCountryOfDestination;
			if (result.IsEmpty)
			{
				result = Bill.B0_RN_NKCountryOfDestination;
				if (result.IsEmpty)
				{
					result = MoveHeader.BM_RL_NKDestinationPort;
				}
			}
			return result;
		}
	}

	public ZBool IsNationalTransitSwitzerland => MoveHeader?.IsNationalTransitSwitzerland ?? false;

	[ReadOnlyMember(nameof(IsNationalTransitSwitzerland))]
	public override ZString BY_RN_NKCountryOfDispatch
	{
		get => base.BY_RN_NKCountryOfDispatch;
		set => base.BY_RN_NKCountryOfDispatch = value;
	}

	public ZString EffectiveCountryCodeOfDispatch
	{
		get
		{
			var result = BY_RN_NKCountryOfDispatch;
			if (result.IsEmpty)
			{
				result = Bill.B0_RN_NKCountryOfExport;
				if (result.IsEmpty)
				{
					result = MoveHeader.BM_RN_NKCountryOfDispatch;
				}
			}
			return result;
		}
	}

	[ReadOnlyMember(nameof(IsNationalTransitSwitzerland))]
	public override ZString BY_RN_NKCountryOfDestination
	{
		get => base.BY_RN_NKCountryOfDestination;
		set => base.BY_RN_NKCountryOfDestination = value;
	}

	[ReadOnlyMember(nameof(IsNationalTransitSwitzerland))]
	public override ZString BY_CusC4Number
	{
		get => base.BY_CusC4Number;
		set => base.BY_CusC4Number = value;
	}

	[ChildEditable(true)]
	public RestrictionCollection Restrictions
	{
		get
		{
			if (restrictions == null)
			{
				restrictions = new RestrictionCollection(this);
				restrictions.Load();
				RegisterEditableChildObject(restrictions);
			}
			return restrictions;
		}
	}
	RestrictionCollection restrictions;

	HugeSequenceNumberGenerator IRestrictionParent.RestrictionsLineNumberGenerator => restrictionsLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => new TypedEnumerable<IHugeSequenceNumberLine>(Restrictions));
	HugeSequenceNumberGenerator restrictionsLineNumberGenerator;

	ZDateTime IRestrictionParent.EffectiveAssessmentDate => ValuationDate;

	protected override INctsPackageCollection<EUNcts.NctsPackage, NctsCommonCargoDesc> GetNctsPackageCollection() => new NctsPackageCollection(this);

	protected override void OnFactorySaving()
	{
		if (!MoveHeader?.IsNationalTransitSwitzerland ?? false)
		{
			Restrictions.RemoveAndDeleteAll();
		}
		base.OnFactorySaving();
	}

	public override void Delete()
	{
		using (((IRestrictionParent)this).RestrictionsLineNumberGenerator.GetLineNumberSuspender())
		{
			base.Delete();
		}
	}
}
