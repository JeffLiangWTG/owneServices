using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using UniversalReferenceConstants = Enterprise.Customs.CH.Business.UniversalReferenceConstants;

[assembly: UsesConstants(typeof(UniversalReferenceConstants))]

namespace Enterprise.Customs.CH.NCTS.Module;

public class NctsMovementFilterStripBusinessObject : EU.NCTS.Module.NctsMovementFilterStripBusinessObject
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class CHFilterConstants
	{
		public const string ActivationDeadline = "Activation Deadline";
		public const string ArrivalMRNATOReference = "Arrival MRN/Additional Transit Ref.#";
	}

	protected override CodeDescriptionPairList DeclarationTypeListCore => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.NCTSDeclarationType, ZDateTime.Today);

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();
		AddActivationDeadlineDateFilter(filters);
		AddArrivalMRNATOFilter(filters);
		return filters;
	}

	void AddActivationDeadlineDateFilter(ModuleFilterCollection filters)
	{
		var issueFilter = filters.AddDateFilter(CHFilterConstants.ActivationDeadline, CusEntryNumSchema.CE_ExpiryDate);
		issueFilter.Category = FilterCategories.Dates;
		issueFilter.MultilingualDescription = ResString.GetMultilingualString("B8805630-3AAF-420B-B356-F1C797170C35", CHFilterConstants.ActivationDeadline);
		issueFilter.SubGroup = MRNSubGroup;
	}

	public void AddArrivalMRNATOFilter(ModuleFilterCollection filters)
	{
		var arrivalFilter = filters.AddTextFilter(CHFilterConstants.ArrivalMRNATOReference, GetArrivalMRNATOQuery);
		arrivalFilter.Category = FilterCategories.NumbersAndReferences;
		arrivalFilter.MultilingualDescription = ResString.GetMultilingualString("D3E3CEC1-0E0C-4646-A369-155E58B7537D", CHFilterConstants.ArrivalMRNATOReference);
	}

	ZQuery GetArrivalMRNATOQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var supportingInfoQuery = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID, CusInBondMoveHeaderSchema.PK);
		supportingInfoQuery.AddToFilter(CusSupportingInfoSchema.CSI_Type, new[] { CusSupportingInfoTypeList.Codes.MovementReferenceNumber, CusSupportingInfoTypeList.Codes.AdditionalTransitOperation });
		supportingInfoQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, comparisonOperator, value);

		var movementQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
		movementQuery.AddSubQuery(supportingInfoQuery, JoinCondition.And);

		var query = new ZDBOnlyQuery(typeof(CusInBondHeader));
		query.AddSubQuery(movementQuery, JoinCondition.And);

		return query;
	}

	protected override CodeDescriptionPairList NctsDepartureStatusListPhase5Core => Factory.GetCachedValue<NCTS5DepartureCustomsStatusList>();

	protected override CodeDescriptionPairList NctsArrivalStatusListPhase5Core => Factory.GetCachedValue<NCTS5ArrivalCustomsStatusList>();

	protected override CodeDescriptionPairList NctsDeparturePhaseStatusListPhase5Core => new DeparturePhaseList();
}
