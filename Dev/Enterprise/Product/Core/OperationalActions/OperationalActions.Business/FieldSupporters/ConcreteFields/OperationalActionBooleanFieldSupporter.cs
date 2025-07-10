using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionBooleanFieldSupporter : OperationalActionFieldSupporter
	{
		public OperationalActionBooleanFieldSupporter(string field, bool readOnly)
			: base(field, readOnly) { }

		protected override void PopulateDefaultingStrategies(IList<IFieldDefaultingStrategy> strategies)
		{
			base.PopulateDefaultingStrategies(strategies);
			strategies.Add(new FixedBooleanFieldDefaultingStrategy(this));
		}

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerBooleanField(factory, descriptor, this);
		}
	}
}
