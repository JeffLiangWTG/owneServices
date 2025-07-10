using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;

public class RenameCommercialAndOperationalMeasurementsToChargeableAndActual : RenameColumnTransformation
{
	public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList
	{
		get
		{
			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialGrossWeight",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableGrossWeight);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialGrossWeightUnit",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableGrossWeightUnit);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialLength",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableLength);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialWidth",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableWidth);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialHeight",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableHeight);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialArea",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableArea);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialVolume",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableVolume);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialUnitOfDimension",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableUnitOfDimension);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialUnitOfArea",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableUnitOfArea);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialUnitOfVolume",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableUnitOfVolume);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_CommercialRevenueTons",
				CarrierShipmentCargoSchema.Constants.CSC_ChargeableRevenueTons);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OperationalGrossWeight",
				CarrierShipmentCargoSchema.Constants.CSC_ActualGrossWeight);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OperationalGrossWeightUnit",
				CarrierShipmentCargoSchema.Constants.CSC_ActualGrossWeightUnit);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OperationalLength",
				CarrierShipmentCargoSchema.Constants.CSC_ActualLength);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OperationalWidth",
				CarrierShipmentCargoSchema.Constants.CSC_ActualWidth);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OperationalHeight",
				CarrierShipmentCargoSchema.Constants.CSC_ActualHeight);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OperationalArea",
				CarrierShipmentCargoSchema.Constants.CSC_ActualArea);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OperationalVolume",
				CarrierShipmentCargoSchema.Constants.CSC_ActualVolume);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OperationalUnitOfDimension",
				CarrierShipmentCargoSchema.Constants.CSC_ActualUnitOfDimension);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OperationalUnitOfArea",
				CarrierShipmentCargoSchema.Constants.CSC_ActualUnitOfArea);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OperationalUnitOfVolume",
				CarrierShipmentCargoSchema.Constants.CSC_ActualUnitOfVolume);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_RC_EquipmentType",
				CarrierShipmentCargoSchema.Constants.CSC_RC_ChargeableEquipmentType);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_RC_OperationalSizeType",
				CarrierShipmentCargoSchema.Constants.CSC_RC_ActualEquipmentType);

			yield return new RenameColumnTransformationInfo(
				CarrierShipmentCargoSchema.Constants.TableName,
				"CSC_OOGLostSlotsCommercial",
				CarrierShipmentCargoSchema.Constants.CSC_OOGLostSlotsChargeable);
		}
	}
}
