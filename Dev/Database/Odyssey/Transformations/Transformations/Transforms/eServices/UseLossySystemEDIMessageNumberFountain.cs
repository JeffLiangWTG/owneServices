using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.eServices;

class UseLossySystemEDIMessageNumberFountain : LossyNumberFountainTransform
{
	public override string Table => "EDIMessage";
	public override string Column => "EM_MessageNum";
	public override string FountainName => "SystemEDIMessageNumber";
}
