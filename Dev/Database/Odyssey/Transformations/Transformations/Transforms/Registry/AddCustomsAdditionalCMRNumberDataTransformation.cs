namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class AddCustomsAdditionalCMRNumberDataTransformation : AddCustomsAdditionalNumberDataTransformation
	{
		public override string UserDescription => "Add CustomsAdditionalNumber CMR if user has overridden it.";

		protected override string Code => "CMR";

		protected override string Description => "Carrier Message Reference";

		protected override bool IsUnique => true;

		protected override bool IsAutomation => false;
	}
}
