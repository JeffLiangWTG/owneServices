using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerGeographyField : RunnerField<OperationalActionGeographyFieldSupporter, ZGeography>
	{
		public RunnerGeographyField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionGeographyFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter)
		{ }

		protected override void ValidatePropertyCore()
		{
			base.ValidatePropertyCore();
			TypeValidation.CheckValidGeography(PropertyInfo);
		}
	}
}
