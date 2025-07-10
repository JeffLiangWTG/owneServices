using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed partial class OperationalActionDateFieldSupporter : OperationalActionFieldSupporter
	{
		public OperationalActionDateFieldSupporter(string field, bool readOnly)
			: base(field, readOnly)
		{
		}

		protected override void PopulateDefaultingStrategies(IList<IFieldDefaultingStrategy> strategies)
		{
			strategies.Add(new FixedDateOnlyFieldDefaultingStrategy(this));
			strategies.Add(new RelativeDateOnlyFieldDefaultingStrategy(this));
		}

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerDateField(factory, descriptor, this);
		}

		protected override string AsFilterStringCore(IZType value, BusinessObjectFactory factory)
		{
			return ((ZDate)value).ToShortDateString();
		}
	}
}
