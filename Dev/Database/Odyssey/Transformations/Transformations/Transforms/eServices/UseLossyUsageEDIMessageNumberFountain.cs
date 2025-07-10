using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.eServices
{
	class UseLossyUsageEDIMessageNumberFountain : LossyNumberFountainTransform
	{
		public override string Table => "EDIMessage";
		public override string Column => "EM_MessageNum";
		public override string FountainName => "UsageEDIMessageNumber";
	}
}

