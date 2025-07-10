using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class RenameAdvancePaymentRelatedTransactionCategoryCodes : DataTransformation
	{
		public override string UserDescription => "Rename transaction category codes that are related to Advance Payment (Obsolete)";
	}
}
