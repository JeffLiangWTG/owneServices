using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.OceanCarrier;

sealed class RenameCPO_RV_NKVesselToCPO_VesselName : RenameColumnTransformation
{
	public override IEnumerable<IRenameColumnTransformationInfo> RenameColumnInfoList =>
		[new RenameColumnTransformationInfo(CarrierVoyagePortCallSchema.Constants.TableName, "CPO_RV_NKVessel", CarrierVoyagePortCallSchema.Constants.CPO_VesselName),];
}
