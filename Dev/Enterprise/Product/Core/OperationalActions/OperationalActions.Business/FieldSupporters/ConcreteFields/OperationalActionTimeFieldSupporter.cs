using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed partial class OperationalActionTimeFieldSupporter : OperationalActionFieldSupporter
	{
		public OperationalActionTimeFieldSupporter(string field, bool readOnly)
			: base(field, readOnly)
		{
		}

		protected override void PopulateDefaultingStrategies(IList<IFieldDefaultingStrategy> strategies)
		{
			strategies.Add(new FixedTimeFieldDefaultingStrategy(this));
		}

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerTimeField(factory, descriptor, this);
		}

		protected override string AsFilterStringCore(IZType value, BusinessObjectFactory factory)
		{
			ZTime time = (ZTime)value;
			return time.ToString();
		}
	}
}
