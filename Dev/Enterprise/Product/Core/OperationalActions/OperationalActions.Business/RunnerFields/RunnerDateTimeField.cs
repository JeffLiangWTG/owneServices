using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerDateTimeField : RunnerField<OperationalActionDateTimeFieldSupporter, ZDateTime>
	{
		public RunnerDateTimeField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionDateTimeFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter) { }

		protected override void ValidatePropertyCore()
		{
			base.ValidatePropertyCore();
			TypeValidation.CheckValidZDateTimeAndRange(PropertyInfo);
		}
	}
}
