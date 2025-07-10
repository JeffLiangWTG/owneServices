using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(RenameAdvancePaymentRelatedTransactionCategoryCodes))]
	internal class RenameAdvancePaymentRelatedTransactionCategoryCodesTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameAdvancePaymentRelatedTransactionCategoryCodes();
		}
	}
}
