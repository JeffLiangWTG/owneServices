using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerDateTimeOffsetField : RunnerField<OperationalActionDateTimeOffsetFieldSupporter, ZDateTimeOffset>
	{
		public RunnerDateTimeOffsetField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionDateTimeOffsetFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter)
		{ }

		protected override void ValidatePropertyCore()
		{
			base.ValidatePropertyCore();
			TypeValidation.CheckValidZDateTimeOffsetAndRange(PropertyInfo);
		}
	}
}
