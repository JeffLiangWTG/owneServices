using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Packing;
using static Enterprise.Integration.Customs;

namespace Enterprise.Integration.TransitWarehouse
{
	public interface ITransitPackage
	{
		ZInt PackageQty { get; }
		ZString PackType { get; }
		ZString PackageID { get; }
		ZDecimal Length { get; }
		ZDecimal Width { get; }
		ZDecimal Height { get; }
		ZDecimal Weight { get; }
		ZDecimal Volume { get; }
		ZString DimensionUQ { get; }
		ZString WeightUQ { get; }
		ZString VolumeUQ { get; }
		ZString MarksAndNumbers { get; }
		ZString GoodsDescription { get; }
		ZBool RequiresTemperatureControl { get; }
		ZDecimal RequiredTemperatureMinimum { get; }
		ZDecimal RequiredTemperatureMaximum { get; }
		ZString RequiredTemperatureUnit { get; }
		ZString CommodityCode { get; }
		ZString HSCode { get; }
		ZString RCN { get; }
		ZDateTime UnloadTime { get; }
		ZBool IsDamaged { get; }
		ZString DamagedReason { get; }
		IEnumerable<IUNDGDataItem> UNDGDataItems { get; }
		IEnumerable<ICusEntryNumber> Numbers { get; }
		IPkgPackage TransitPackage { get; }
		ZString UnitType { get; }
		ZBool IsHighRisk { get; }
		ZString ExternalReference { get; }
		ZString OverriddenAviationSecurityInspectionType { get; }
	}
}
