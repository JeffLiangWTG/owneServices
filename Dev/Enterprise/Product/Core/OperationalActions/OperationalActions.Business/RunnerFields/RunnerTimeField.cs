using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerTimeField : RunnerField<OperationalActionTimeFieldSupporter, ZTime>
	{
		public RunnerTimeField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionTimeFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter) { }

		protected override void ValidatePropertyCore()
		{
			base.ValidatePropertyCore();
			TypeValidation.CheckValidZTime(PropertyInfo);
		}
	}
}
