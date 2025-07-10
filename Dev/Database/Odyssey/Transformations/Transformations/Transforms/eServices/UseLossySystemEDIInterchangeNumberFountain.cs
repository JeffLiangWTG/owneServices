using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.eServices;

class UseLossySystemEDIInterchangeNumberFountain : LossyNumberFountainTransform
{
	public override string Table => "EDIInterchange";
	public override string Column => "EI_InterchangeNum";
	public override string FountainName => "SystemEDIInterchangeNumber";
}
