using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Internal.AutoTransforms;
using Enterprise.DbUpgrader.Transformations.Transforms.eServices;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.eServices;

[TestedType(typeof(UseLossySystemEDIInterchangeNumberFountain))]
public class UseLossySystemEDIInterchangeNumberFountainTest : LossyNumberFountainTransformTest
{
	protected override LossyNumberFountainTransform GetLossyTransformationInstance()
	{
		return new UseLossySystemEDIInterchangeNumberFountain();
	}
}
