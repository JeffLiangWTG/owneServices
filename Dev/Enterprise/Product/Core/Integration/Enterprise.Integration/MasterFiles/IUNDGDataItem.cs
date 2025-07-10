using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IUNDGDataItem
	{
		ZGuid DI_DG { get; }
		ZDecimal DI_DGFlashPoint { get; }
		ZDecimal DI_DGVolume { get; }
		ZDecimal DI_DGWeight { get; }
		ZString DI_DG_NKSubs { get; }
		ZString DI_F3_NKPackType { get; }
		ZBool DI_HasOverpack { get; }
		ZString DI_IMOClass { get; }
		ZBool DI_IsLimitedQuantity { get; }
		ZString DI_MPMarinePollutant { get; }
		ZGuid DI_OC_DGContact { get; }
		ZString DI_OverpackID { get; }
		ZInt DI_PackageCount { get; }
		ZGuid DI_ParentID { get; }
		ZString DI_ParentTableCode { get; }
		ZString DI_TechnicalName { get; }
		ZString DI_UnitOfVolume { get; }
		ZString DI_UnitOfWeight { get; }
		ZString DI_RadioactiveLabelCategory { get; }
		ZString DI_RadionuclideElement { get; }
		ZString DI_RadionuclideElementSuffix { get; }
		ZDecimal DI_RadioactiveMaximumActivity { get; }
		ZString DI_RadioactiveMaximumActivityUnit { get; }
		ZDecimal DI_RadioactiveTransportIndex { get; }
		ZBool DI_IsExclusiveUse { get; }
		ZBool DI_IsFissileExcepted { get; }
		ZBool DI_IsHighwayRouteControlledQuantity { get; }
		ZString DI_MaterialFormDescription { get; }
		ZString DI_PackingInstructionSection { get; }
	}
}
