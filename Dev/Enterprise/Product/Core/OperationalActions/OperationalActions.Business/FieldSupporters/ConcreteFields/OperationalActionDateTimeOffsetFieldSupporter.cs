using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed partial class OperationalActionDateTimeOffsetFieldSupporter : OperationalActionFieldSupporter
	{
		public OperationalActionDateTimeOffsetFieldSupporter(string field, bool readOnly)
			: base(field, readOnly)
		{
		}

		protected override void PopulateDefaultingStrategies(IList<IFieldDefaultingStrategy> strategies)
		{
			strategies.Add(new FixedDateTimeOffsetFieldDefaultingStrategy(this));
			strategies.Add(new RelativeDateTimeOffsetFieldDefaultingStrategy(this));
		}

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerDateTimeOffsetField(factory, descriptor, this);
		}

		protected override string AsFilterStringCore(IZType value, BusinessObjectFactory factory)
		{
			if (value is ZDateTime)
			{
				ZDateTime dateTime = (ZDateTime)value;

				return dateTime.ToLongTimeString();
			}
			else
			{
				return ((ZDateTimeOffset)value).ToZDateTime().ToLongTimeString();
			}
		}
	}
}
