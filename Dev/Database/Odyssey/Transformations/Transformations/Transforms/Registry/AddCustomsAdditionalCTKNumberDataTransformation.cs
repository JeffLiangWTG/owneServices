namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class AddCustomsAdditionalCTKNumberDataTransformation : AddCustomsAdditionalNumberDataTransformation
	{
		public override string UserDescription => "Add CustomsAdditionalNumber CTK if user has overridden it.";

		protected override string Code => "CTK";

		protected override string Description => "Cargo Tracking Note";

		protected override bool IsUnique => true;

		protected override bool IsAutomation => false;
	}
}
