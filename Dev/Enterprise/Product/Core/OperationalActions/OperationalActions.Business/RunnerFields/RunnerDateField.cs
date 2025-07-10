using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerDateField : RunnerField<OperationalActionDateFieldSupporter, ZDate>
	{
		public RunnerDateField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionDateFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter)
		{ }

		protected override void ValidatePropertyCore()
		{
			base.ValidatePropertyCore();
			TypeValidation.CheckValidZDateAndRange(PropertyInfo);
		}
	}
}
